using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/accounting")]
public sealed class AccountingController(
    AppDbContext db,
    ErpContext context,
    AccountingService accounting) : ControllerBase
{
    [HttpGet("journal")]
    public async Task<IActionResult> Journal([FromQuery] string? source, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        limit = Math.Min(limit, 100);
        var query = db.JournalEntries.AsNoTracking().Include(x => x.Lines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(source) && Enum.TryParse<JournalSource>(source, true, out var s)) query = query.Where(x => x.Source == s);
        if (from.HasValue) query = query.Where(x => x.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue)   query = query.Where(x => x.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue));
        var total = await query.CountAsync();
        var data  = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * limit).Take(limit).ToListAsync();
        return Ok(new { data, total, page, limit });
    }

    [HttpPost("journal")]
    public async Task<IActionResult> CreateJournal(CreateJournalRequest request)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        try
        {
            var entry = new JournalEntry
            {
                Description = request.Description,
                CreatedBy   = context.Current(Request).UserId,
                Lines       = request.Lines.Select(x => new JournalLine { AccountCode = x.AccountCode, AccountName = x.AccountName, Debit = x.Debit, Credit = x.Credit }).ToList()
            };
            accounting.Create(entry);
            await db.SaveChangesAsync();
            return Ok(entry);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("journal/{id:int}/reverse")]
    public async Task<IActionResult> Reverse(int id, [FromQuery] string reason)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        if (string.IsNullOrWhiteSpace(reason)) return BadRequest("Dao nguoc but toan bat buoc nhap ly do.");
        var original = await db.JournalEntries.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id);
        if (original is null) return NotFound("Khong tim thay but toan.");
        if (original.Source == JournalSource.REVERSAL) return BadRequest("Khong the dao nguoc but toan da dao nguoc.");
        try
        {
            var reversal = new JournalEntry { Source = JournalSource.REVERSAL, Description = $"Dao nguoc: {original.Description} — {reason}", ReversalOfId = original.Id, CreatedBy = context.Current(Request).UserId, Lines = original.Lines.Select(x => new JournalLine { AccountCode = x.AccountCode, AccountName = x.AccountName, Debit = x.Credit, Credit = x.Debit }).ToList() };
            accounting.Create(reversal);
            await db.SaveChangesAsync();
            return Ok(reversal);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpGet("ap-invoices")]
    public async Task<IActionResult> APInvoices([FromQuery] bool? unpaidOnly, [FromQuery] string? supplier)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        var query = db.APInvoices.AsNoTracking().Include(x => x.Payments).AsQueryable();
        if (unpaidOnly == true) query = query.Where(x => !x.IsPaid);
        if (!string.IsNullOrWhiteSpace(supplier)) query = query.Where(x => x.SupplierName.Contains(supplier));
        return Ok(await query.OrderBy(x => x.DueDate).ToListAsync());
    }

    [HttpPost("ap-invoices/{id:int}/pay")]
    public async Task<IActionResult> PayInvoice(int id, APPaymentRequest request)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        var invoice = await db.APInvoices.Include(x => x.Payments).SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null) return NotFound("Khong tim thay hoa don.");
        if (invoice.IsPaid) return BadRequest("Hoa don da duoc thanh toan.");
        var payment = new APPayment { APInvoiceId = id, Amount = request.Amount, Method = request.Method, Reference = request.Reference, PaidBy = context.Current(Request).UserId ?? 0 };
        db.APPayments.Add(payment);
        if (invoice.Payments.Sum(x => x.Amount) + request.Amount >= invoice.Amount) invoice.IsPaid = true;
        accounting.Create(new JournalEntry { Source = JournalSource.AP_PAYMENT, Description = $"Thanh toan cong no {invoice.SupplierName}", CreatedBy = context.Current(Request).UserId, Lines = [new JournalLine { AccountCode = "331", AccountName = "Phai tra nha cung cap", Debit = request.Amount }, new JournalLine { AccountCode = "1121", AccountName = "Tien gui ngan hang", Credit = request.Amount }] });
        await db.SaveChangesAsync();
        return Ok(new { invoice, payment });
    }

    [HttpGet("periods")]
    public async Task<IActionResult> Periods()
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        return Ok(await db.AccountingPeriods.AsNoTracking().OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync());
    }

    [HttpPost("periods/{id:int}/close")]
    public async Task<IActionResult> ClosePeriod(int id)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        var period = await db.AccountingPeriods.SingleOrDefaultAsync(x => x.Id == id);
        if (period is null) return NotFound("Khong tim thay ky ke toan.");
        if (period.IsClosed) return BadRequest("Ky ke toan da dong.");
        period.IsClosed = true; period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = context.Current(Request).UserId;
        await db.SaveChangesAsync();
        return Ok(period);
    }
}
