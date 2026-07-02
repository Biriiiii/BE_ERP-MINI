using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/sales")]
public sealed class SalesController(
    AppDbContext db,
    ErpContext context,
    InventoryService inventory,
    AccountingService accounting,
    UserActionLogService actionLog) : ControllerBase
{
    // ── Invoices ─────────────────────────────────────────────────────────────
    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] string? search,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] int? paymentStatus,
        [FromQuery] int? paymentMethod)
    {
        var query = db.SalesInvoices.AsNoTracking()
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                x.CustomerName.ToLower().Contains(s) ||
                (x.CustomerPhone != null && x.CustomerPhone.Contains(s)) ||
                x.Id.ToString() == s);
        }
        if (DateOnly.TryParse(from, out var dFrom))
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) >= dFrom);
        if (DateOnly.TryParse(to, out var dTo))
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) <= dTo);
        if (paymentStatus.HasValue)
            query = query.Where(x => (int)x.PaymentStatus == paymentStatus);
        if (paymentMethod.HasValue)
            query = query.Where(x => (int)x.PaymentMethod == paymentMethod);

        var invoices = await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CustomerName,
                x.CustomerPhone,
                x.CustomerId,
                x.CreatedBy,
                x.Status,
                PaymentStatus = (int)x.PaymentStatus,
                PaymentMethod = (int)x.PaymentMethod,
                x.Notes,
                x.CreatedAt,
                TotalAmount = x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount),
                Lines = x.Lines.Select(l => new {
                    l.Id, l.ProductId, ProductName = l.Product.Name, l.Quantity, l.UnitPrice, l.DiscountAmount, l.Notes
                })
            })
            .ToListAsync();
        return Ok(invoices);
    }

    [HttpGet("invoices/summary")]
    public async Task<IActionResult> GetSummary([FromQuery] string? from, [FromQuery] string? to)
    {
        var query = db.SalesInvoices.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.Status == ApprovalStatus.APPROVED);

        if (DateOnly.TryParse(from, out var dFrom))
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) >= dFrom);
        if (DateOnly.TryParse(to, out var dTo))
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) <= dTo);

        var invoices = await query.ToListAsync();
        var totalRevenue = invoices.Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));
        var totalCash = invoices.Where(x => x.PaymentMethod == PaymentMethod.CASH).Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));
        var totalBank = invoices.Where(x => x.PaymentMethod == PaymentMethod.BANK).Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));
        var totalCard = invoices.Where(x => x.PaymentMethod == PaymentMethod.CARD).Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));
        var totalEwallet = invoices.Where(x => x.PaymentMethod == PaymentMethod.EWALLET).Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));
        var totalUnpaid = invoices.Where(x => x.PaymentStatus == ReceiptPaymentStatus.UNPAID).Sum(x => x.Lines.Sum(l => l.Quantity * l.UnitPrice - l.DiscountAmount));

        return Ok(new
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = totalRevenue,
            Cash = totalCash,
            Bank = totalBank,
            Card = totalCard,
            Ewallet = totalEwallet,
            Unpaid = totalUnpaid
        });
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice(CreateSalesInvoiceRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.CASHIER, Role.WAREHOUSE_STAFF, Role.SALES_STAFF);

        int? customerId = request.CustomerId;

        // Auto-create or find customer if phone or name is provided
        if (!customerId.HasValue && (!string.IsNullOrWhiteSpace(request.CustomerPhone) || !string.IsNullOrWhiteSpace(request.CustomerName)))
        {
            var phone = request.CustomerPhone?.Trim() ?? "";
            var name = request.CustomerName?.Trim() ?? "";

            if (name.ToLower() != "khách lẻ" || !string.IsNullOrEmpty(phone))
            {
                Customer? existingCust = null;

                if (!string.IsNullOrEmpty(phone))
                {
                    existingCust = await db.Customers.FirstOrDefaultAsync(c => c.Phone == phone && !c.IsDeleted);
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    existingCust = await db.Customers.FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
                }

                if (existingCust != null)
                {
                    customerId = existingCust.Id;
                    
                    // Cập nhật lại Tên nếu khách hàng này bị lưu trống tên hoặc nhập tên mới
                    if (!string.IsNullOrWhiteSpace(name) && name.ToLower() != "khách lẻ" && name.ToLower() != "khach le" && existingCust.Name != name)
                    {
                        existingCust.Name = name;
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    var newCust = new Customer
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? "Khách hàng" : name,
                        Phone = phone,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Customers.Add(newCust);
                    await db.SaveChangesAsync(); // Save to get the ID
                    customerId = newCust.Id;
                }
            }
        }

        var invoice = new SalesInvoice
        {
            CustomerName  = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerId    = customerId,
            CreatedBy     = request.CreatedBy,
            Notes         = request.Notes,
            PaymentStatus = (ReceiptPaymentStatus)request.PaymentStatus,
            PaymentMethod = (PaymentMethod)request.PaymentMethod,
            Lines         = request.Lines.Select(x => new SalesInvoiceLine {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                DiscountAmount = x.DiscountAmount,
                Notes = x.Notes
            }).ToList()
        };
        db.SalesInvoices.Add(invoice);
        await db.SaveChangesAsync();
        return Ok(invoice);
    }

    [HttpPatch("invoices/{id:int}/approve")]
    public async Task<IActionResult> ApproveInvoice(int id, ApproveRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.SALES_STAFF, Role.CASHIER);

        var invoice = await db.SalesInvoices.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null) return NotFound("Khong tim thay hoa don.");
        if (invoice.Status == ApprovalStatus.APPROVED) return BadRequest("Hoa don da duoc duyet.");

        try
        {
            decimal totalCost = 0;
            decimal totalRevenue = 0;

            foreach (var line in invoice.Lines)
            {
                var (cost, _) = await inventory.IssueFEFOAsync(line.ProductId, line.Quantity, true);
                totalCost += cost;
                totalRevenue += line.Quantity * line.UnitPrice - line.DiscountAmount;
            }

            invoice.Status = ApprovalStatus.APPROVED;
            invoice.ApprovedBy = request.ApproverId;
            invoice.ApprovedAt = DateTime.UtcNow;

            var vatAmount = decimal.Round(totalRevenue / 11m, 0);
            var netRevenue = totalRevenue - vatAmount;

            var (accCode, accName) = invoice.PaymentMethod switch
            {
                PaymentMethod.CASH => ("111", "Tien mat"),
                PaymentMethod.BANK => ("1121", "Tien gui ngan hang"),
                PaymentMethod.CARD => ("1121", "Tien gui ngan hang"),
                PaymentMethod.EWALLET => ("1121", "Tien gui ngan hang"),
                _ => invoice.PaymentStatus == ReceiptPaymentStatus.PAID ? ("111", "Tien mat") : ("131", "Phai thu khach hang")
            };

            accounting.Create(new JournalEntry
            {
                Source = JournalSource.POS_SALE,
                Description = $"Hoa don ban hang {invoice.Id}",
                CreatedBy = request.ApproverId > 0 ? request.ApproverId : invoice.CreatedBy,
                Lines = [
                    new JournalLine { AccountCode = accCode, AccountName = accName, Debit = totalRevenue },
                    new JournalLine { AccountCode = "511", AccountName = "Doanh thu ban hang", Credit = netRevenue },
                    new JournalLine { AccountCode = "3331", AccountName = "Thue GTGT", Credit = vatAmount }
                ]
            });

            accounting.Create(new JournalEntry
            {
                Source = JournalSource.POS_COGS,
                Description = $"Gia von hoa don {invoice.Id}",
                CreatedBy = request.ApproverId > 0 ? request.ApproverId : invoice.CreatedBy,
                Lines = [
                    new JournalLine { AccountCode = "632", AccountName = "Gia von", Debit = totalCost },
                    new JournalLine { AccountCode = "156", AccountName = "Hang hoa", Credit = totalCost }
                ]
            });

            // Update customer stats if linked
            if (invoice.CustomerId.HasValue)
            {
                var cust = await db.Customers.FindAsync(invoice.CustomerId.Value);
                if (cust is not null)
                {
                    cust.TotalSpent += totalRevenue;
                    cust.TotalOrders += 1;
                }
            }

            await actionLog.AddLogAsync(Request, "CREATE", "SalesInvoice", invoice.Id.ToString(),
                $"Ban hang: {invoice.CustomerName} - {totalRevenue:N0}d", null, invoice);

            await db.SaveChangesAsync();
            return Ok(invoice);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Chuyển đổi phương thức thanh toán (VD: Chuyển khoản → Tiền mặt) ─────
    [HttpPatch("invoices/{id:int}/switch-payment")]
    public async Task<IActionResult> SwitchPaymentMethod(int id, [FromBody] SwitchPaymentRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.CASHIER, Role.SALES_STAFF);

        var invoice = await db.SalesInvoices.SingleOrDefaultAsync(x => x.Id == id);
        if (invoice is null) return NotFound("Khong tim thay hoa don.");

        invoice.PaymentMethod = (PaymentMethod)request.PaymentMethod;
        invoice.PaymentStatus = (ReceiptPaymentStatus)request.PaymentStatus;
        await db.SaveChangesAsync();

        return Ok(invoice);
    }

    // ── Customers ────────────────────────────────────────────────────────────
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search)
    {
        var query = db.Customers.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(s) ||
                (x.Phone != null && x.Phone.Contains(s)) ||
                (x.Email != null && x.Email.ToLower().Contains(s)));
        }
        return Ok(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.SALES_STAFF, Role.CASHIER);
        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Notes = request.Notes
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpPut("customers/{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerRequest request)
    {
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        if (request.Name is not null) customer.Name = request.Name;
        if (request.Phone is not null) customer.Phone = request.Phone;
        if (request.Email is not null) customer.Email = request.Email;
        if (request.Address is not null) customer.Address = request.Address;
        if (request.Notes is not null) customer.Notes = request.Notes;
        await db.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpDelete("customers/{id:int}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var customer = await db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        customer.IsDeleted = true;
        await db.SaveChangesAsync();
        return Ok();
    }
}
