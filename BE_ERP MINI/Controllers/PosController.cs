using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/pos")]
public sealed class PosController(
    AppDbContext db,
    ErpContext context,
    InventoryService inventory,
    AccountingService accounting,
    UserActionLogService actionLog) : ControllerBase
{
    [HttpGet("products/lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string barcode)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Barcode == barcode && !x.IsDeleted);
        if (product is null) return NotFound("Khong tim thay san pham theo barcode.");
        var today = DateOnly.FromDateTime(DateTime.Today);
        var lots  = await db.Lots.AsNoTracking().Where(x => x.ProductId == product.Id && x.Quantity > 0).OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue).ToListAsync();
        var nearestExpiry = lots.FirstOrDefault()?.ExpiryDate;
        return Ok(new { Product = product, Quantity = lots.Sum(x => x.Quantity), NearestExpiry = nearestExpiry, IsExpired = nearestExpiry.HasValue && nearestExpiry.Value <= today, NearExpiry = nearestExpiry.HasValue && nearestExpiry.Value <= today.AddDays(2) });
    }

    [HttpGet("promotions/active")]
    public async Task<IActionResult> ActivePromotions()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var promos = await db.Promotions.AsNoTracking().Include(x => x.Product)
            .Where(x => x.IsActive && x.FromDate <= today && x.ToDate >= today).ToListAsync();
        return Ok(promos.Select(x => new { x.Id, x.Name, x.ProductId, ProductName = x.Product?.Name, x.DiscountType, x.DiscountValue, x.FromDate, x.ToDate }));
    }

    [HttpPost("session/open")]
    public async Task<IActionResult> OpenSession(OpenPosSessionRequest request)
    {
        context.Require(Request, Role.OWNER, Role.CASHIER);
        var cashier = await db.Employees.SingleOrDefaultAsync(x => x.Id == request.CashierId && !x.IsDeleted);
        if (cashier is null) return NotFound("Khong tim thay thu ngan.");
        if (await db.PosSessions.AnyAsync(x => x.CashierId == request.CashierId && x.ClosedAt == null))
            return BadRequest("Thu ngan dang co ca chua dong.");
        var session = new PosSession { CashierId = request.CashierId, OpeningFloat = request.OpeningFloat };
        db.PosSessions.Add(session);
        await db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPost("session/close")]
    public async Task<IActionResult> CloseSession(ClosePosSessionRequest request)
    {
        context.Require(Request, Role.OWNER, Role.CASHIER);
        var session = await db.PosSessions.SingleOrDefaultAsync(x => x.Id == request.SessionId && x.CashierId == request.CashierId);
        if (session is null) return NotFound("Khong tim thay ca thu ngan.");
        if (session.ClosedAt is not null) return BadRequest("Ca da dong.");
        var sales = await db.PosTransactions.Where(x => x.SessionId == session.Id && x.PaymentMethod == PaymentMethod.CASH).SumAsync(x => x.TotalAmount);
        session.ClosingCash = request.ClosingCash;
        session.ClosedAt    = DateTime.UtcNow;
        session.Status      = ApprovalStatus.APPROVED;
        await db.SaveChangesAsync();
        return Ok(new { Session = session, ExpectedCash = session.OpeningFloat + sales, Difference = request.ClosingCash - (session.OpeningFloat + sales) });
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(CreatePosTransactionRequest request)
    {
        context.Require(Request, Role.OWNER, Role.CASHIER);
        var session = await db.PosSessions.SingleOrDefaultAsync(x => x.Id == request.SessionId && x.CashierId == request.CashierId && x.ClosedAt == null);
        if (session is null) return BadRequest("Ca thu ngan khong hop le hoac da dong.");
        if (request.Lines.Count == 0) return BadRequest("Gio hang trong.");

        var transaction = new PosTransaction { SessionId = request.SessionId, CashierId = request.CashierId, PaymentMethod = request.PaymentMethod };
        try
        {
            foreach (var line in request.Lines)
            {
                var product = await db.Products.SingleOrDefaultAsync(x => x.Id == line.ProductId && !x.IsDeleted);
                if (product is null) return NotFound($"Khong tim thay san pham {line.ProductId}.");
                var (cost, _) = await inventory.IssueFEFOAsync(line.ProductId, line.Quantity, true);
                transaction.Lines.Add(new PosTransactionLine { ProductId = line.ProductId, Quantity = line.Quantity, UnitPrice = product.SalePrice });
                transaction.CostAmount += cost;
            }
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        transaction.TotalAmount = transaction.Lines.Sum(x => x.LineTotal);
        transaction.VatAmount   = decimal.Round(transaction.TotalAmount / 11m, 0);
        db.PosTransactions.Add(transaction);

        var revenue            = transaction.TotalAmount - transaction.VatAmount;
        var (accCode, accName) = request.PaymentMethod == PaymentMethod.CASH ? ("111", "Tien mat") : ("1121", "Tien gui ngan hang");
        accounting.Create(new JournalEntry { Source = JournalSource.POS_SALE, Description = $"POS sale", CreatedBy = request.CashierId, Lines = [new JournalLine { AccountCode = accCode, AccountName = accName, Debit = transaction.TotalAmount }, new JournalLine { AccountCode = "511", AccountName = "Doanh thu ban hang", Credit = revenue }, new JournalLine { AccountCode = "3331", AccountName = "Thue GTGT", Credit = transaction.VatAmount }] });
        accounting.Create(new JournalEntry { Source = JournalSource.POS_COGS, Description = $"POS COGS", CreatedBy = request.CashierId, Lines = [new JournalLine { AccountCode = "632", AccountName = "Gia von", Debit = transaction.CostAmount }, new JournalLine { AccountCode = "156", AccountName = "Hang hoa", Credit = transaction.CostAmount }] });

        await actionLog.AddLogAsync(Request, "CREATE", "PosTransaction", "0",
            "Tao hoa don POS", null, transaction, source: "POS_SYSTEM");
        await db.SaveChangesAsync();

        // Cập nhật EntityId sau khi có Id thực
        var lastLog = db.UserActionLogs.Local.LastOrDefault(x => x.EntityType == "PosTransaction");
        if (lastLog is not null) lastLog.EntityId = transaction.Id.ToString();
        await db.SaveChangesAsync();

        return Ok(transaction);
    }

    [HttpPost("refunds")]
    public async Task<IActionResult> CreateRefund(CreatePosRefundRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var transaction = await db.PosTransactions.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == request.TransactionId);
        if (transaction is null) return NotFound("Khong tim thay giao dich POS.");
        
        var line = transaction.Lines.SingleOrDefault(x => x.ProductId == request.ProductId);
        if (line is null) return BadRequest("San pham khong co trong giao dich nay.");
        
        if (request.Quantity > line.Quantity) return BadRequest("So luong hoan tra khong duoc lon hon so luong da mua.");

        // Calculate refund amount
        var unitPrice = line.UnitPrice;
        var refundAmount = request.Quantity * unitPrice;

        var refund = new PosRefund
        {
            TransactionId = request.TransactionId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            RefundAmount = refundAmount,
            Reason = request.Reason,
            ProcessedBy = context.Current(Request).UserId ?? 0
        };

        db.PosRefunds.Add(refund);

        var vatAmount = decimal.Round(refundAmount / 11m, 0);
        var revenue = refundAmount - vatAmount;
        var (accCode, accName) = transaction.PaymentMethod == PaymentMethod.CASH ? ("111", "Tien mat") : ("1121", "Tien gui ngan hang");
        
        accounting.Create(new JournalEntry 
        { 
            Source = JournalSource.REVERSAL, 
            Description = $"Hoan tra POS {transaction.Id}: {request.Reason}", 
            CreatedBy = context.Current(Request).UserId, 
            Lines = [
                new JournalLine { AccountCode = "511", AccountName = "Doanh thu ban hang", Debit = revenue }, 
                new JournalLine { AccountCode = "3331", AccountName = "Thue GTGT", Debit = vatAmount }, 
                new JournalLine { AccountCode = accCode, AccountName = accName, Credit = refundAmount }
            ] 
        });

        await actionLog.AddLogAsync(Request, "CREATE", "PosRefund", "0",
            $"Hoan tra {request.Quantity} SP {request.ProductId} cho POS {transaction.Id}", null, refund);
        
        await db.SaveChangesAsync();

        var lastLog = db.UserActionLogs.Local.LastOrDefault(x => x.EntityType == "PosRefund");
        if (lastLog is not null) lastLog.EntityId = refund.Id.ToString();
        await db.SaveChangesAsync();

        return Ok(refund);
    }
}
