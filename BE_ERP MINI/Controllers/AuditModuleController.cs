using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

/// <summary>
/// Audit Trail phân theo module nghiệp vụ — giúp Swagger hiển thị rõ từng nhóm.
/// </summary>
[ApiController]
[Route("api/audit")]
public sealed class AuditModuleController(AppDbContext db, ErpContext context) : ControllerBase
{
    // ══════════════════════════════════════════════════════════════
    // NHÂN SỰ
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử thay đổi nhân viên (Employee)</summary>
    [HttpGet("employees")]
    [Tags("Audit - Nhân sự")]
    public Task<IActionResult> EmployeeAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["Employee"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);

    /// <summary>Lịch sử chấm công (AttendanceRecord)</summary>
    [HttpGet("attendance")]
    [Tags("Audit - Nhân sự")]
    public Task<IActionResult> AttendanceAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["AttendanceRecord"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER);

    /// <summary>Lịch sử nghỉ phép (LeaveRequest)</summary>
    [HttpGet("leaves")]
    [Tags("Audit - Nhân sự")]
    public Task<IActionResult> LeaveAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["LeaveRequest"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER);

    /// <summary>Lịch sử bảng lương (PayrollRecord)</summary>
    [HttpGet("payroll")]
    [Tags("Audit - Nhân sự")]
    public Task<IActionResult> PayrollAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["PayrollRecord"], action, from, to, page, limit, Role.OWNER, Role.ACCOUNTANT);

    // ══════════════════════════════════════════════════════════════
    // SẢN PHẨM & KHO HÀNG
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử sản phẩm (Product) — tạo/sửa/xoá</summary>
    [HttpGet("products")]
    [Tags("Audit - Sản phẩm & Kho")]
    public Task<IActionResult> ProductAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["Product"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);

    /// <summary>Lịch sử nhập kho (WarehouseReceipt)</summary>
    [HttpGet("warehouse-receipts")]
    [Tags("Audit - Sản phẩm & Kho")]
    public Task<IActionResult> ReceiptAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["WarehouseReceipt"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);

    /// <summary>Lịch sử xuất kho & hao hụt (WarehouseIssue, ShrinkageRecord)</summary>
    [HttpGet("warehouse-issues")]
    [Tags("Audit - Sản phẩm & Kho")]
    public Task<IActionResult> IssueAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["WarehouseIssue", "ShrinkageRecord"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);

    // ══════════════════════════════════════════════════════════════
    // ĐƠN HÀNG & BÁN HÀNG (POS)
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử giao dịch bán hàng (PosTransaction)</summary>
    [HttpGet("transactions")]
    [Tags("Audit - Đơn hàng & POS")]
    public Task<IActionResult> TransactionAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["PosTransaction"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.CASHIER);

    /// <summary>Lịch sử ca thu ngân (PosSession)</summary>
    [HttpGet("pos-sessions")]
    [Tags("Audit - Đơn hàng & POS")]
    public Task<IActionResult> SessionAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["PosSession"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.CASHIER);

    /// <summary>Lịch sử hoàn trả hàng (PosRefund)</summary>
    [HttpGet("refunds")]
    [Tags("Audit - Đơn hàng & POS")]
    public Task<IActionResult> RefundAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["PosRefund"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER);

    // ══════════════════════════════════════════════════════════════
    // MUA HÀNG
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử đơn mua hàng (PurchaseOrder, PurchaseRequest)</summary>
    [HttpGet("purchasing")]
    [Tags("Audit - Mua hàng")]
    public Task<IActionResult> PurchaseAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["PurchaseOrder", "PurchaseRequest"], action, from, to, page, limit, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);

    // ══════════════════════════════════════════════════════════════
    // KẾ TOÁN
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử bút toán kế toán (JournalEntry)</summary>
    [HttpGet("journal")]
    [Tags("Audit - Kế toán")]
    public Task<IActionResult> JournalAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["JournalEntry"], action, from, to, page, limit, Role.OWNER, Role.ACCOUNTANT);

    /// <summary>Lịch sử công nợ & thanh toán (APInvoice, APPayment)</summary>
    [HttpGet("ap")]
    [Tags("Audit - Kế toán")]
    public Task<IActionResult> ApAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["APInvoice", "APPayment"], action, from, to, page, limit, Role.OWNER, Role.ACCOUNTANT);

    // ══════════════════════════════════════════════════════════════
    // TÀI KHOẢN HỆ THỐNG
    // ══════════════════════════════════════════════════════════════

    /// <summary>Lịch sử quản lý tài khoản (User)</summary>
    [HttpGet("users")]
    [Tags("Audit - Tài khoản")]
    public Task<IActionResult> UserAudit(
        [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
        => QueryByTypes(["User"], action, from, to, page, limit, Role.OWNER);

    /// <summary>Lịch sử toàn hệ thống</summary>
    [HttpGet("all")]
    [Tags("Audit - Toàn hệ thống")]
    public async Task<IActionResult> AllAudit(
        [FromQuery] string? entityTypes, [FromQuery] string? action, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var types = string.IsNullOrWhiteSpace(entityTypes) 
            ? Array.Empty<string>() 
            : entityTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
        // Require OWNER for full audit access
        context.Require(Request, Role.OWNER);
        limit = Math.Min(limit, 100);

        var query = db.UserActionLogs.AsNoTracking();
        
        if (types.Length > 0)
            query = query.Where(x => types.Contains(x.EntityType));

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actions = action.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(x => actions.Contains(x.Action));
        }
        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.EntityLabel,
                x.Summary,
                x.UserName,
                x.UserId,
                x.CreatedAt,
                x.Reason,
                x.IpAddress
            })
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }

    // ══════════════════════════════════════════════════════════════
    // HELPER CHUNG
    // ══════════════════════════════════════════════════════════════

    private async Task<IActionResult> QueryByTypes(
        string[] entityTypes,
        string? action,
        DateOnly? from,
        DateOnly? to,
        int page,
        int limit,
        params Role[] allowedRoles)
    {
        context.Require(Request, allowedRoles);
        limit = Math.Min(limit, 100);

        var query = db.UserActionLogs.AsNoTracking()
            .Where(x => entityTypes.Contains(x.EntityType));

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actions = action.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(x => actions.Contains(x.Action));
        }
        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var total = await query.CountAsync();
        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.EntityLabel,
                x.Summary,
                x.UserName,
                x.UserId,
                x.CreatedAt,
                x.Reason,
                x.IpAddress
            })
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }
}
