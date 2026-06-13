using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/purchasing")]
public sealed class PurchaseController(
    AppDbContext db,
    ErpContext context,
    AccountingService accounting,
    UserActionLogService actionLog) : ControllerBase
{
    private const decimal PoApprovalThreshold = 10_000_000m;

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests()
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        return Ok(await db.PurchaseRequests.AsNoTracking().Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost("requests")]
    public async Task<IActionResult> CreateRequest(CreatePurchaseRequestRequest request)
    {
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == request.ProductId && !x.IsDeleted);
        if (product is null) return NotFound("Khong tim thay san pham.");
        var pr = new PurchaseRequest { ProductId = request.ProductId, Quantity = request.Quantity, CreatedBy = request.CreatedBy, Reason = request.Reason };
        db.PurchaseRequests.Add(pr);
        await db.SaveChangesAsync();
        return Ok(pr);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.ACCOUNTANT);
        return Ok(await db.PurchaseOrders.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(CreatePurchaseOrderRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var current = context.Current(Request);
        if (await accounting.SupplierBlockedAsync(request.SupplierName))
            return BadRequest($"NCC [{request.SupplierName}] dang co cong no qua han. Khong the tao PO.");
        var needsOwnerApproval = request.Amount > PoApprovalThreshold;
        if (needsOwnerApproval && current.Role != Role.OWNER && !request.OwnerOverride)
            return BadRequest($"PO vuot nguong {PoApprovalThreshold:N0}d — Can duoc duyet boi Owner.");
        var order = new PurchaseOrder { SupplierName = request.SupplierName, Amount = request.Amount, CreatedBy = request.CreatedBy ?? current.UserId ?? 0, Status = ApprovalStatus.PENDING, OverrideReason = request.OverrideReason };
        if (current.Role == Role.OWNER || request.OwnerOverride) { order.ApprovedBy = current.UserId; order.ApprovedAt = DateTime.UtcNow; order.Status = ApprovalStatus.APPROVED; }
        db.PurchaseOrders.Add(order);
        await actionLog.AddLogAsync(Request, "CREATE", "PurchaseOrder", "0", $"Tao PO {request.SupplierName} - {request.Amount:N0}d", null, order);
        await db.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPatch("orders/{id:int}/approve")]
    public async Task<IActionResult> ApproveOrder(int id, ApproveRequest request)
    {
        context.Require(Request, Role.OWNER);
        var order = await db.PurchaseOrders.SingleOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound("Khong tim thay PO.");
        if (order.Status == ApprovalStatus.APPROVED) return BadRequest("PO da duoc duyet.");
        var old = order.Status;
        order.Status = ApprovalStatus.APPROVED; order.ApprovedBy = request.ApproverId; order.ApprovedAt = DateTime.UtcNow;
        await actionLog.AddLogAsync(Request, "APPROVE", "PurchaseOrder", order.Id.ToString(), $"Duyet PO {order.SupplierName}", new { Status = old.ToString() }, new { Status = order.Status.ToString() });
        await db.SaveChangesAsync();
        return Ok(order);
    }
}
