namespace BE_ERP_MINI.Domain;

/// <summary>
/// Lưu trữ các giao dịch nhận được từ SePay Webhook.
/// Dùng SepayTransactionId (id từ SePay) làm UNIQUE để chống trùng lặp (idempotent).
/// </summary>
public sealed class SepayTransaction
{
    public int Id { get; set; }
    
    /// <summary>ID giao dịch do SePay trả về (dùng để chống duplicate)</summary>
    public long SepayTransactionId { get; set; }
    
    /// <summary>Ngân hàng: Vietcombank, BIDV, TPBank...</summary>
    public string Gateway { get; set; } = "";
    
    /// <summary>Thời gian giao dịch từ SePay</summary>
    public string TransactionDate { get; set; } = "";
    
    /// <summary>Số tài khoản nhận tiền</summary>
    public string AccountNumber { get; set; } = "";
    
    /// <summary>Mã thanh toán SePay tự tách (VD: DH12345)</summary>
    public string? Code { get; set; }
    
    /// <summary>Nội dung chuyển khoản gốc</summary>
    public string Content { get; set; } = "";
    
    /// <summary>"in" = tiền vào, "out" = tiền ra</summary>
    public string TransferType { get; set; } = "in";
    
    /// <summary>Mô tả từ ngân hàng</summary>
    public string? Description { get; set; }
    
    /// <summary>Số tiền chuyển khoản</summary>
    public decimal TransferAmount { get; set; }
    
    /// <summary>Số dư tích lũy</summary>
    public decimal Accumulated { get; set; }
    
    /// <summary>Mã tham chiếu ngân hàng</summary>
    public string? ReferenceCode { get; set; }
    
    /// <summary>ID hóa đơn bán hàng đã khớp (null nếu chưa khớp)</summary>
    public int? MatchedInvoiceId { get; set; }
    
    /// <summary>Đã xử lý business logic chưa</summary>
    public bool IsProcessed { get; set; }
    
    /// <summary>Thời điểm hệ thống nhận webhook</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
