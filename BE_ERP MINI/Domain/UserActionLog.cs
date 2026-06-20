namespace BE_ERP_MINI.Domain;

public sealed class UserActionLog
{
    public long Id { get; set; }                        // long cho bảng log (tăng trưởng nhanh)
    public int? UserId { get; set; }                    // FK về Users.Id
    public string? UserName { get; set; }
    public Role UserRole { get; set; }
    public string Action { get; set; } = "";
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";          // giữ string vì EntityId có thể là bất kỳ loại nào
    public string? EntityLabel { get; set; }
    public string? Summary { get; set; }
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string? ChangedFieldsJson { get; set; }
    public string? Reason { get; set; }
    public string Source { get; set; } = "USER";
    public string? SessionId { get; set; }
    public long? ParentAuditId { get; set; }            // Phục vụ truy vết action cascade
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
