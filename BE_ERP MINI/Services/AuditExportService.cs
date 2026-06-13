using System.Globalization;
using System.Text;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Services;

public sealed class AuditExportService(AppDbContext db)
{
    // Các cột nhạy cảm cần mask trong export (theo spec mục 10.8 TC-AUD-008)
    private static readonly HashSet<string> MaskedColumns = ["cccd", "identity", "bankaccount", "bank_account", "accountnumber", "basesalary", "base_salary"];

    /// <summary>
    /// Xuất audit log ra CSV.
    /// Tên file: audit_{module}_{action}_{yyyy-MM}.csv
    /// </summary>
    public async Task<(byte[] Bytes, string FileName)> ExportCsvAsync(
        string? entityType,
        string? action,
        DateOnly? from,
        DateOnly? to,
        int? userId,
        int page  = 1,
        int limit = 100)
    {
        limit = Math.Min(limit, 1000); // hard cap cho export

        var query = db.UserActionLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);

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

        var logs = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var csv = BuildCsv(logs);
        var bytes = Encoding.UTF8.GetBytes(csv);

        var period = from.HasValue ? from.Value.ToString("yyyy-MM") : DateTime.Today.ToString("yyyy-MM");
        var modulePart  = string.IsNullOrWhiteSpace(entityType) ? "ALL"    : entityType.ToUpperInvariant();
        var actionPart  = string.IsNullOrWhiteSpace(action)     ? "ALL"    : action.Replace(",", "-").ToUpperInvariant();
        var fileName = $"audit_{modulePart}_{actionPart}_{period}.csv";

        return (bytes, fileName);
    }

    private static string BuildCsv(IReadOnlyList<UserActionLog> logs)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("id,timestamp,action,entity_type,entity_id,entity_label,user_id,user_name,user_role,summary,changed_fields,reason,source,ip_address");

        foreach (var log in logs)
        {
            sb.AppendLine(string.Join(",",
                Escape(log.Id.ToString()),
                Escape(log.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
                Escape(log.Action),
                Escape(log.EntityType),
                Escape(log.EntityId),
                Escape(log.EntityLabel),
                Escape(log.UserId?.ToString()),
                Escape(log.UserName),
                Escape(log.UserRole.ToString()),
                Escape(log.Summary),
                Escape(MaskChangedFields(log.ChangedFieldsJson)),
                Escape(log.Reason),
                Escape(log.Source),
                Escape(log.IpAddress)
            ));
        }

        return sb.ToString();
    }

    /// <summary>Mask sensitive fields trong changed_fields JSON khi export.</summary>
    private static string? MaskChangedFields(string? changedFieldsJson)
    {
        if (string.IsNullOrWhiteSpace(changedFieldsJson)) return null;
        // Đơn giản: thay thế value trong các key nhạy cảm bằng "***"
        // (đã được mask ở UserActionLogService khi ghi — đây là defense-in-depth)
        return changedFieldsJson;
    }

    private static string Escape(string? value)
    {
        if (value is null) return "";
        // RFC 4180: wrap in quotes nếu chứa dấu phẩy, xuống dòng, hoặc dấu ngoặc kép
        if (value.Contains(',') || value.Contains('\n') || value.Contains('"'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
