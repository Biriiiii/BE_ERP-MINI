using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

/// <summary>
/// Controller xử lý tích hợp SePay:
/// - Webhook endpoint nhận thông báo giao dịch từ SePay
/// - Tạo mã QR VietQR cho thanh toán chuyển khoản
/// - Kiểm tra trạng thái thanh toán theo hóa đơn
/// </summary>
[ApiController]
[Route("api/sepay")]
public sealed class SepayController(
    AppDbContext db,
    ErpContext context,
    IConfiguration config) : ControllerBase
{
    // ── Webhook: SePay gọi khi có giao dịch ─────────────────────────────────
    /// <summary>
    /// Endpoint nhận webhook từ SePay.
    /// SePay POST payload JSON mỗi khi có giao dịch chuyển khoản vào tài khoản.
    /// Phải trả về {"success": true} trong vòng 30 giây.
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveWebhook([FromBody] SepayWebhookPayload payload)
    {
        // 1. Chống trùng lặp: Kiểm tra xem giao dịch này đã được xử lý chưa
        var exists = await db.SepayTransactions
            .AnyAsync(x => x.SepayTransactionId == payload.Id);

        if (exists)
        {
            // Đã xử lý trước đó, trả OK để SePay không retry
            return Ok(new { success = true });
        }

        // 2. Lưu giao dịch vào DB
        var transaction = new SepayTransaction
        {
            SepayTransactionId = payload.Id,
            Gateway = payload.Gateway ?? "",
            TransactionDate = payload.TransactionDate ?? "",
            AccountNumber = payload.AccountNumber ?? "",
            Code = payload.Code,
            Content = payload.Content ?? "",
            TransferType = payload.TransferType ?? "in",
            Description = payload.Description,
            TransferAmount = payload.TransferAmount,
            Accumulated = payload.Accumulated,
            ReferenceCode = payload.ReferenceCode,
            ReceivedAt = DateTime.UtcNow
        };

        db.SepayTransactions.Add(transaction);

        // 3. Tự động khớp với hóa đơn nếu có mã thanh toán (code hoặc tìm trong content)
        var textToMatch = !string.IsNullOrWhiteSpace(payload.Code) ? payload.Code : payload.Content;
        if (!string.IsNullOrWhiteSpace(textToMatch) && payload.TransferType?.ToLower() == "in")
        {
            await MatchInvoiceAsync(transaction, textToMatch, payload.TransferAmount);
        }

        await db.SaveChangesAsync();

        // 4. Trả về đúng format SePay yêu cầu
        return Ok(new { success = true });
    }

    // ── Tạo thông tin QR thanh toán cho hóa đơn ─────────────────────────────
    /// <summary>
    /// Tạo link ảnh QR VietQR cho một hóa đơn cụ thể.
    /// Thu ngân gọi API này khi khách chọn phương thức "Chuyển khoản".
    /// </summary>
    [HttpGet("qr/{invoiceId:int}")]
    public async Task<IActionResult> GetPaymentQR(int invoiceId)
    {
        var invoice = await db.SalesInvoices
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == invoiceId);

        if (invoice is null)
            return NotFound("Khong tim thay hoa don.");

        var totalAmount = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount);

        // Đọc cấu hình SePay từ appsettings.json
        var sepayConfig = config.GetSection("SePay");
        var bankName = sepayConfig["BankName"] ?? "MBBank";
        var accountNumber = sepayConfig["AccountNumber"] ?? "";
        var accountHolder = sepayConfig["AccountHolder"] ?? "";
        var paymentPrefix = sepayConfig["PaymentPrefix"] ?? "DH";

        // Tạo mã thanh toán duy nhất cho hóa đơn
        var paymentCode = $"{paymentPrefix}{invoice.Id}";

        // Tạo URL ảnh QR VietQR (dùng dịch vụ miễn phí của SePay/VietQR)
        var qrUrl = $"https://qr.sepay.vn/img?acc={accountNumber}&bank={bankName}"
                  + $"&amount={totalAmount:0}"
                  + $"&des={Uri.EscapeDataString(paymentCode)}"
                  + $"&template=compact";

        return Ok(new
        {
            InvoiceId = invoice.Id,
            PaymentCode = paymentCode,
            Amount = totalAmount,
            BankName = bankName,
            AccountNumber = accountNumber,
            AccountHolder = accountHolder,
            QrImageUrl = qrUrl,
            Message = $"Chuyen khoan {totalAmount:N0}d voi noi dung: {paymentCode}"
        });
    }

    // ── Kiểm tra trạng thái thanh toán ───────────────────────────────────────
    /// <summary>
    /// Frontend gọi polling API này mỗi 3-5 giây để kiểm tra khách đã chuyển khoản chưa.
    /// Khi webhook nhận được tiền và khớp mã → trả về isPaid = true.
    /// </summary>
    [HttpGet("check/{invoiceId:int}")]
    public async Task<IActionResult> CheckPaymentStatus(int invoiceId)
    {
        var invoice = await db.SalesInvoices
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId);

        if (invoice is null)
            return NotFound("Khong tim thay hoa don.");

        var sepayConfig = config.GetSection("SePay");
        var paymentPrefix = sepayConfig["PaymentPrefix"] ?? "DH";
        var paymentCode = $"{paymentPrefix}{invoiceId}";

        // Tìm giao dịch SePay đã khớp với hóa đơn này
        var matchedTransaction = await db.SepayTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MatchedInvoiceId == invoiceId && x.IsProcessed);

        return Ok(new
        {
            InvoiceId = invoiceId,
            PaymentCode = paymentCode,
            IsPaid = invoice.PaymentStatus == ReceiptPaymentStatus.PAID,
            MatchedTransaction = matchedTransaction != null ? new
            {
                matchedTransaction.SepayTransactionId,
                matchedTransaction.TransferAmount,
                matchedTransaction.Gateway,
                matchedTransaction.TransactionDate,
                matchedTransaction.Content
            } : null
        });
    }

    // ── Xem lịch sử giao dịch SePay ─────────────────────────────────────────
    /// <summary>
    /// API cho quản lý xem toàn bộ lịch sử giao dịch SePay đã nhận.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] bool? matched)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);

        var query = db.SepayTransactions.AsNoTracking().AsQueryable();

        if (DateOnly.TryParse(from, out var dFrom))
            query = query.Where(x => DateOnly.FromDateTime(x.ReceivedAt) >= dFrom);
        if (DateOnly.TryParse(to, out var dTo))
            query = query.Where(x => DateOnly.FromDateTime(x.ReceivedAt) <= dTo);
        if (matched.HasValue)
            query = query.Where(x => x.IsProcessed == matched.Value);

        var transactions = await query
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync();

        return Ok(transactions);
    }

    // ── Private: Khớp giao dịch với hóa đơn ─────────────────────────────────
    private async Task MatchInvoiceAsync(SepayTransaction transaction, string content, decimal amount)
    {
        var sepayConfig = config.GetSection("SePay");
        var paymentPrefix = sepayConfig["PaymentPrefix"] ?? "DH";

        // Tìm paymentPrefix (VD: "DH") trong chuỗi content (VD: "Nguyen Van A CK DH123")
        int prefixIndex = content.IndexOf(paymentPrefix, StringComparison.OrdinalIgnoreCase);
        if (prefixIndex >= 0)
        {
            // Lấy phần phía sau prefix
            string afterPrefix = content.Substring(prefixIndex + paymentPrefix.Length);
            
            // Trích xuất tất cả các chữ số liền kề ngay sau chữ DH
            string invoiceIdStr = new string(afterPrefix.TakeWhile(char.IsDigit).ToArray());

            if (!string.IsNullOrEmpty(invoiceIdStr) && int.TryParse(invoiceIdStr, out var invoiceId))
            {
                var invoice = await db.SalesInvoices
                    .Include(x => x.Lines)
                    .SingleOrDefaultAsync(x => x.Id == invoiceId);

            if (invoice is not null)
            {
                var totalAmount = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount);

                // Kiểm tra số tiền khớp (cho phép chênh lệch ±1000đ do phí ngân hàng)
                if (amount >= totalAmount - 1000)
                {
                    invoice.PaymentStatus = ReceiptPaymentStatus.PAID;
                    invoice.PaymentMethod = PaymentMethod.BANK;
                    transaction.MatchedInvoiceId = invoice.Id;
                    transaction.IsProcessed = true;
                }
            }
        }
        }
    }
}

// ── DTO nhận payload từ SePay Webhook ────────────────────────────────────────
public sealed class SepayWebhookPayload
{
    public long Id { get; set; }
    public string? Gateway { get; set; }
    public string? TransactionDate { get; set; }
    public string? AccountNumber { get; set; }
    public string? SubAccount { get; set; }
    public string? Code { get; set; }
    public string? Content { get; set; }
    public string? TransferType { get; set; }
    public string? Description { get; set; }
    public decimal TransferAmount { get; set; }
    public decimal Accumulated { get; set; }
    public string? ReferenceCode { get; set; }
}
