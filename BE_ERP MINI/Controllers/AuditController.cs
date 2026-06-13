using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

/// <summary>
/// Legacy controller — giữ backward-compat với route /api/v1/audit-logs.
/// Các endpoint đầy đủ đã chuyển sang /api/v1/audit/* (UserActionLogsController).
/// </summary>
[ApiController]
[Route("api/audit-logs")]
public sealed class AuditController(AppDbContext db, ErpContext context) : ControllerBase
{
    /// <summary>
    /// GET /api/v1/audit-logs — Xem toàn bộ audit log (DB-backed, không phải in-memory).
    /// Quyền: OWNER, ACCOUNTANT
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logs(
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page  = 1,
        [FromQuery] int limit = 50)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        limit = Math.Min(limit, 100);

        var query = db.UserActionLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);
        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));

        var total = await query.CountAsync();
        var data  = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }
}
