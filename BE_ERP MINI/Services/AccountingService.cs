using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Services;

public sealed class AccountingService(AppDbContext db)
{
    /// <summary>
    /// Tạo bút toán kế toán — kiểm tra cân bằng Nợ = Có.
    /// TC-ACC-002: Block bút toán không cân bằng.
    /// </summary>
    public JournalEntry Create(JournalEntry entry)
    {
        var debit  = entry.Lines.Sum(x => x.Debit);
        var credit = entry.Lines.Sum(x => x.Credit);
        if (debit != credit)
            throw new InvalidOperationException($"But toan chua can bang (lech {Math.Abs(debit - credit):N0}d).");

        db.JournalEntries.Add(entry);
        return entry;
    }

    /// <summary>
    /// Kiểm tra NCC có bị block vì công nợ quá hạn > 14 ngày không.
    /// </summary>
    public async Task<bool> SupplierBlockedAsync(string supplierName)
    {
        var today = DateOnly.FromDateTime(DateTime.Today.AddDays(-14));
        return await db.APInvoices.AnyAsync(x =>
            !x.IsPaid &&
            x.SupplierName.Equals(supplierName, StringComparison.OrdinalIgnoreCase) &&
            x.DueDate < today);
    }
}
