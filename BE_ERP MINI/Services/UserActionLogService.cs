using System.Text.Json;
using System.Text.Json.Nodes;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Services;

public sealed class UserActionLogService(AppDbContext db, ErpContext context)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };
    private static readonly string[] ReasonRequiredActions = ["SOFT_DELETE", "REJECT", "UNLOCK", "REVERSAL"];
    private static readonly string[] IgnoredDiffFields = ["updatedAt", "updatedBy", "version"];
    private static readonly string[] SensitiveKeys = ["password", "cccd", "identity", "nationalid", "bankAccount", "bank_account", "accountNumber", "baseSalary", "base_salary"];

    // ─── Ghi log thông thường — tự SaveChanges ────────────────────────────────
    public async Task<UserActionLog> WriteAsync(
        HttpRequest request,
        string action,
        string entityType,
        string entityId,
        string? summary,
        object? oldValue = null,
        object? newValue = null,
        string? entityLabel = null,
        string? reason = null,
        string source = "USER",
        int? systemUserId = null)
    {
        var log = await BuildLogAsync(request, action, entityType, entityId, summary, oldValue, newValue, entityLabel, reason, source, systemUserId);
        db.UserActionLogs.Add(log);
        await db.SaveChangesAsync();
        return log;
    }

    // ─── Ghi log trong cùng transaction (caller tự SaveChanges) ───────────────
    public async Task<UserActionLog> AddLogAsync(
        HttpRequest request,
        string action,
        string entityType,
        string entityId,
        string? summary,
        object? oldValue = null,
        object? newValue = null,
        string? entityLabel = null,
        string? reason = null,
        string source = "USER")
    {
        var log = await BuildLogAsync(request, action, entityType, entityId, summary, oldValue, newValue, entityLabel, reason, source, null);
        db.UserActionLogs.Add(log);
        return log;
    }

    // ─── Ghi log SYSTEM (job tự động) ────────────────────────────────────────
    public async Task<UserActionLog> WriteSystemAsync(
        HttpRequest request,
        string action,
        string entityType,
        string entityId,
        string? summary,
        object? oldValue = null,
        object? newValue = null,
        string? entityLabel = null,
        string source = "SYSTEM")
    {
        var log = await BuildLogAsync(request, action, entityType, entityId, summary, oldValue, newValue, entityLabel, null, source, null);
        log.UserId   = null;
        log.UserName = "SYSTEM";
        db.UserActionLogs.Add(log);
        await db.SaveChangesAsync();
        return log;
    }

    // ─── Ghi PERMISSION_DENIED ───────────────────────────────────────────────
    public async Task WritePermissionDeniedAsync(HttpRequest request, string path)
    {
        var current = context.Current(request);
        db.UserActionLogs.Add(new UserActionLog
        {
            UserId    = current.UserId,
            UserRole  = current.Role,
            Action    = "PERMISSION_DENIED",
            EntityType= "API",
            EntityId  = path,
            Summary   = $"Truy cap bi tu choi: {path}",
            Source    = "USER",
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            SessionId = request.Headers["X-Session-Id"].FirstOrDefault(),
            DeviceInfo= request.Headers.UserAgent.FirstOrDefault(),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    // ─── Internal builder ────────────────────────────────────────────────────
    private async Task<UserActionLog> BuildLogAsync(
        HttpRequest request,
        string action,
        string entityType,
        string entityId,
        string? summary,
        object? oldValue,
        object? newValue,
        string? entityLabel,
        string? reason,
        string source,
        int? systemUserId)
    {
        if (ReasonRequiredActions.Contains(action) && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException($"Action {action} bat buoc co ly do.");

        var current = context.Current(request);
        AppUser? user = null;
        var actorId = systemUserId ?? current.UserId;
        if (actorId.HasValue)
            user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == actorId.Value);

        var maskedOld = MaskValue(oldValue);
        var maskedNew = MaskValue(newValue);

        return new UserActionLog
        {
            UserId           = user?.Id ?? actorId,
            UserName         = user?.Username ?? (source == "SYSTEM" ? "SYSTEM" : null),
            UserRole         = user?.Role ?? current.Role,
            Action           = action,
            EntityType       = entityType,
            EntityId         = entityId,
            EntityLabel      = entityLabel,
            Summary          = summary,
            OldValueJson     = maskedOld is null ? null : maskedOld.ToJsonString(JsonOptions),
            NewValueJson     = maskedNew is null ? null : maskedNew.ToJsonString(JsonOptions),
            ChangedFieldsJson= ComputeDiff(maskedOld, maskedNew),
            Reason           = reason,
            Source           = source,
            SessionId        = request.Headers["X-Session-Id"].FirstOrDefault(),
            DeviceInfo       = request.Headers["X-Device-Info"].FirstOrDefault() ?? request.Headers.UserAgent.FirstOrDefault(),
            IpAddress        = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedAt        = DateTime.UtcNow
        };
    }

    // ─── Mask sensitive data ──────────────────────────────────────────────────
    private static JsonObject? MaskValue(object? value)
    {
        if (value is null) return null;
        var node = JsonSerializer.SerializeToNode(value, JsonOptions) as JsonObject;
        if (node is null) return null;
        MaskObject(node);
        return node;
    }

    private static void MaskObject(JsonObject obj)
    {
        foreach (var property in obj.ToList())
        {
            if (property.Value is JsonObject child) { MaskObject(child); continue; }
            if (property.Value is JsonArray array)  { foreach (var item in array.OfType<JsonObject>()) MaskObject(item); continue; }
            if (SensitiveKeys.Any(x => property.Key.Contains(x, StringComparison.OrdinalIgnoreCase)))
                obj[property.Key] = MaskScalar(property.Value?.ToString());
        }
    }

    private static string MaskScalar(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "***";
        if (value.Length <= 4) return "****";
        return new string('x', Math.Max(0, value.Length - 4)) + value[^4..];
    }

    // ─── Diff computation ─────────────────────────────────────────────────────
    private static string? ComputeDiff(JsonObject? oldValue, JsonObject? newValue)
    {
        if (oldValue is null || newValue is null) return null;
        var diff = new JsonObject();
        var keys = oldValue.Select(x => x.Key).Union(newValue.Select(x => x.Key), StringComparer.OrdinalIgnoreCase);
        foreach (var key in keys)
        {
            if (IgnoredDiffFields.Contains(key, StringComparer.OrdinalIgnoreCase)) continue;
            oldValue.TryGetPropertyValue(key, out var oldNode);
            newValue.TryGetPropertyValue(key, out var newNode);
            if (oldNode?.ToJsonString(JsonOptions) == newNode?.ToJsonString(JsonOptions)) continue;
            diff[key] = new JsonObject { ["from"] = oldNode?.DeepClone(), ["to"] = newNode?.DeepClone() };
        }
        return diff.Count == 0 ? null : diff.ToJsonString(JsonOptions);
    }
}
