using System.Text.Json;
using System.Text.Json.Nodes;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

/// <summary>
/// Audit Trail API — Mục 10.6 (BRD/SRS ERP_Sieu_Thi_v2_AuditTrail.docx)
/// 
/// Route:   GET /api/audit/entity/{type}/{id}           — Lịch sử 1 bản ghi
///          GET /api/audit/user/{userId}                 — Thao tác của 1 nhân viên
///          GET /api/audit/feed                          — Feed real-time các thay đổi
///          GET /api/audit/entity/{type}/{id}/diff/{v1}/{v2} — So sánh 2 version
///          GET /api/audit/export                        — Export CSV
///          GET /api/audit/user-actions                  — Backward-compat (legacy)
/// </summary>
[ApiController]
[Route("api/audit")]
public sealed class UserActionLogsController(
    AppDbContext db,
    ErpContext context,
    AuditExportService exportService,
    UserActionLogService actionLog) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // ─── GET /audit/entity/{type}/{id} ─────────────────────────────────────────
    // Xem toàn bộ lịch sử của 1 bản ghi cụ thể
    // Quyền: STORE_MGR (kho/HR), ACCOUNTANT, OWNER
    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        string entityId,
        [FromQuery] string? action,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery(Name = "user_id")] int? userId,
        [FromQuery] int page  = 1,
        [FromQuery] int limit = 50)
    {
        var current = context.Current(Request);
        if (!CanReadAudit(current.Role, entityType))
        {
            await actionLog.WritePermissionDeniedAsync(Request, Request.Path);
            return StatusCode(403, new { error = "Ban khong co quyen xem audit log nay." });
        }

        limit = Math.Min(limit, 100);
        var query = db.UserActionLogs.AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId);

        query = ApplyCommonFilters(query, action, from, to, userId);

        var total = await query.CountAsync();
        var data  = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }

    // ─── GET /audit/user/{userId} ───────────────────────────────────────────────
    // Xem mọi thao tác của 1 nhân viên
    // Quyền: OWNER, ACCOUNTANT
    [HttpGet("user/{targetUserId:int}")]
    public async Task<IActionResult> GetUserHistory(
        int targetUserId,
        [FromQuery] string? action,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page  = 1,
        [FromQuery] int limit = 50)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        limit = Math.Min(limit, 100);

        var query = db.UserActionLogs.AsNoTracking()
            .Where(x => x.UserId == targetUserId);

        query = ApplyCommonFilters(query, action, from, to, null);

        var total = await query.CountAsync();
        var data  = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }

    // ─── GET /audit/feed ────────────────────────────────────────────────────────
    // Feed tất cả thay đổi (filter theo module = entityType)
    // Quyền: OWNER, ACCOUNTANT
    [HttpGet("feed")]
    public async Task<IActionResult> GetFeed(
        [FromQuery] string? module,       // filter entityType (HR/Inventory/Accounting)
        [FromQuery] string? action,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery(Name = "user_id")] int? userId,
        [FromQuery] int page  = 1,
        [FromQuery] int limit = 50)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        limit = Math.Min(limit, 100);

        var query = db.UserActionLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(module))
            query = query.Where(x => x.EntityType.StartsWith(module));

        query = ApplyCommonFilters(query, action, from, to, userId);

        var total = await query.CountAsync();
        var data  = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new { data, total, page, limit });
    }

    // ─── GET /audit/entity/{type}/{id}/diff/{v1}/{v2} ──────────────────────────
    // So sánh 2 phiên bản cụ thể (version v1 và v2)
    // Quyền: OWNER, ACCOUNTANT
    [HttpGet("entity/{entityType}/{entityId}/diff/{v1:int}/{v2:int}")]
    public async Task<IActionResult> GetDiff(
        string entityType,
        string entityId,
        int v1,
        int v2)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);

        // Lấy tất cả logs của entity theo thứ tự thời gian (version tăng dần)
        var logs = await db.UserActionLogs.AsNoTracking()
            .Where(x => x.EntityType == entityType && x.EntityId == entityId && x.NewValueJson != null)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        if (logs.Count < 2)
            return NotFound(new { error = "Khong du lich su de so sanh." });

        // Map version index (1-based)
        var maxV = logs.Count;
        if (v1 < 1 || v2 < 1 || v1 > maxV || v2 > maxV || v1 >= v2)
            return BadRequest(new { error = $"Version phai trong khoang 1-{maxV} va v1 < v2." });

        var logV1 = logs[v1 - 1];
        var logV2 = logs[v2 - 1];

        JsonObject? snap1 = TryParseJson(logV1.NewValueJson);
        JsonObject? snap2 = TryParseJson(logV2.NewValueJson);

        var diff = ComputeVisualDiff(snap1, snap2);

        return Ok(new
        {
            entityType,
            entityId,
            v1 = new { version = v1, timestamp = logV1.CreatedAt, user = logV1.UserName, action = logV1.Action },
            v2 = new { version = v2, timestamp = logV2.CreatedAt, user = logV2.UserName, action = logV2.Action },
            diff
        });
    }

    // ─── GET /audit/export ──────────────────────────────────────────────────────
    // Export audit log ra CSV (TC-AUD-008)
    // Quyền: OWNER, ACCOUNTANT
    [HttpGet("export")]
    public async Task<IActionResult> Export(
        [FromQuery(Name = "entity_type")] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery(Name = "user_id")] int? userId,
        [FromQuery] int page  = 1,
        [FromQuery] int limit = 1000)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);

        var (bytes, fileName) = await exportService.ExportCsvAsync(entityType, action, from, to, userId, page, limit);
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    // ─── Backward-compat (legacy endpoint) ─────────────────────────────────────
    [HttpGet("user-actions")]
    public async Task<IActionResult> GetActions(
        [FromQuery] int? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? entityId,
        [FromQuery] string? action)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT, Role.STORE_MANAGER);

        var query = db.UserActionLogs.AsNoTracking().AsQueryable();
        if (userId.HasValue)                           query = query.Where(x => x.UserId == userId.Value);
        if (!string.IsNullOrWhiteSpace(entityType))    query = query.Where(x => x.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(entityId))      query = query.Where(x => x.EntityId == entityId);
        if (!string.IsNullOrWhiteSpace(action))        query = query.Where(x => x.Action == action);

        return Ok(await query.OrderByDescending(x => x.CreatedAt).Take(200).ToListAsync());
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// RBAC cho audit: STORE_MANAGER chỉ xem HR + Kho, không xem tài chính (spec mục 2.2).
    /// </summary>
    private static bool CanReadAudit(Role role, string entityType)
    {
        if (role is Role.OWNER or Role.ACCOUNTANT) return true;
        if (role is Role.STORE_MANAGER)
        {
            // STORE_MGR chỉ xem HR và Kho
            var hrEntities  = new[] { "Employee", "LeaveRequest", "AttendanceRecord", "PayrollRecord" };
            var invEntities = new[] { "Product", "StockLot", "InventoryReceipt", "InventoryIssue", "StocktakeSession" };
            return hrEntities.Contains(entityType, StringComparer.OrdinalIgnoreCase)
                || invEntities.Contains(entityType, StringComparer.OrdinalIgnoreCase);
        }
        return false; // CASHIER, WAREHOUSE_STAFF, EMPLOYEE, SALES_STAFF không có quyền
    }

    private static IQueryable<UserActionLog> ApplyCommonFilters(
        IQueryable<UserActionLog> query,
        string? action,
        DateOnly? from,
        DateOnly? to,
        int? userId)
    {
        if (!string.IsNullOrWhiteSpace(action))
        {
            var actions = action.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            query = query.Where(x => actions.Contains(x.Action));
        }
        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));
        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);
        return query;
    }

    private static JsonObject? TryParseJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<JsonObject>(json, JsonOpts); }
        catch { return null; }
    }

    /// <summary>
    /// Tạo visual diff: { field: { from: oldVal, to: newVal } }
    /// </summary>
    private static Dictionary<string, object> ComputeVisualDiff(JsonObject? snap1, JsonObject? snap2)
    {
        var result = new Dictionary<string, object>();
        if (snap1 is null && snap2 is null) return result;

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (snap1 is not null) foreach (var k in snap1) keys.Add(k.Key);
        if (snap2 is not null) foreach (var k in snap2) keys.Add(k.Key);

        foreach (var key in keys)
        {
            JsonNode? v1Node = null;
            JsonNode? v2Node = null;
            snap1?.TryGetPropertyValue(key, out v1Node);
            snap2?.TryGetPropertyValue(key, out v2Node);
            var t1 = v1Node?.ToJsonString(JsonOpts);
            var t2 = v2Node?.ToJsonString(JsonOpts);
            if (t1 != t2)
                result[key] = new { from = v1Node, to = v2Node };
        }
        return result;
    }
}
