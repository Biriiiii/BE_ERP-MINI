using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController(
    AppDbContext db,
    ErpContext context,
    InventoryService inventory,
    UserActionLogService actionLog) : ControllerBase
{
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] string? keyword, [FromQuery] string? category)
    {
        var query = db.Products.AsNoTracking().Where(x => !x.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.Name.Contains(keyword) || x.Sku.Contains(keyword) || (x.Barcode != null && x.Barcode.Contains(keyword)));
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(x => x.CategoryCode == category.ToUpperInvariant());
        return Ok(await query.OrderBy(x => x.CategoryCode).ThenBy(x => x.Sku).ToListAsync());
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        var cat    = request.CategoryCode.ToUpperInvariant();
        var skuPfx = $"{cat}-";
        // Đếm theo SKU prefix — tránh trùng với seed data
        var seq = await db.Products.CountAsync(x => x.Sku.StartsWith(skuPfx)) + 1;
        var product = new Product
        {
            Sku           = $"{cat}-{seq:0000}",
            Name          = request.Name,
            CategoryCode  = cat,
            Unit          = request.Unit,
            Barcode       = request.Barcode,
            MinStockLevel = request.MinStockLevel,
            AverageCost   = request.AverageCost,
            SalePrice     = request.SalePrice,
            CreatedBy     = context.Current(Request).UserId ?? 0
        };
        await using var tx = await db.Database.BeginTransactionAsync();
        db.Products.Add(product);
        await db.SaveChangesAsync();
        await actionLog.AddLogAsync(Request, "CREATE", "Product", product.Id.ToString(),
            $"Tao san pham {product.Sku}", null, product, product.Sku);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(product);
    }

    [HttpPatch("products/{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product is null) return NotFound("Khong tim thay san pham.");
        if (request.Version.HasValue && request.Version.Value != product.Version)
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac.");

        var oldValue = new { product.Name, product.CategoryCode, product.Unit, product.Barcode, product.MinStockLevel, product.AverageCost, product.SalePrice };
        product.Name          = request.Name          ?? product.Name;
        product.CategoryCode  = request.CategoryCode?.ToUpperInvariant() ?? product.CategoryCode;
        product.Unit          = request.Unit          ?? product.Unit;
        product.Barcode       = request.Barcode       ?? product.Barcode;
        product.MinStockLevel = request.MinStockLevel ?? product.MinStockLevel;
        product.AverageCost   = request.AverageCost   ?? product.AverageCost;
        product.SalePrice     = request.SalePrice     ?? product.SalePrice;
        product.UpdatedAt     = DateTime.UtcNow;
        product.UpdatedBy     = context.Current(Request).UserId;
        product.Version++;

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "UPDATE", "Product", product.Id.ToString(),
            $"Sua san pham {product.Sku}", oldValue, product, product.Sku, request.Reason);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(product);
    }

    [HttpDelete("products/{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id, [FromQuery] string reason, [FromQuery] int? version)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        if (string.IsNullOrWhiteSpace(reason)) return BadRequest("Bat buoc nhap ly do.");
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product is null) return NotFound("Khong tim thay san pham.");
        if (version.HasValue && version.Value != product.Version)
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac.");
        var stock = await db.Lots.Where(x => x.ProductId == id).SumAsync(x => x.Quantity);
        if (stock > 0) return BadRequest("San pham con ton kho, khong the xoa.");

        var oldValue = new { product.Sku, product.Name };
        product.IsDeleted = true; product.DeletedAt = DateTime.UtcNow;
        product.DeletedBy = context.Current(Request).UserId; product.Version++;

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "SOFT_DELETE", "Product", id.ToString(),
            $"Xoa mem san pham {product.Sku}", oldValue, null, product.Sku, reason);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    [HttpGet("stock-balance")]
    public async Task<IActionResult> StockBalance([FromQuery(Name = "product_id")] int? productId)
    {
        var query = db.Lots.AsNoTracking().Include(x => x.Product).Where(x => !x.Product.IsDeleted).AsQueryable();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var data = await query
            .GroupBy(x => new { x.ProductId, x.Product.Sku, x.Product.Name, x.Product.MinStockLevel, x.Product.AverageCost, x.Product.Unit })
            .Select(g => new { g.Key.ProductId, g.Key.Sku, g.Key.Name, g.Key.Unit, Quantity = g.Sum(l => l.Quantity), g.Key.MinStockLevel, g.Key.AverageCost, TotalValue = g.Sum(l => l.Quantity * l.UnitCost), LotCount = g.Count() })
            .ToListAsync();
        return Ok(data);
    }

    [HttpGet("expiry-alerts")]
    public async Task<IActionResult> ExpiryAlerts([FromQuery] int days = 2)
    {
        var threshold = DateOnly.FromDateTime(DateTime.Today.AddDays(days));
        var data = await db.Lots.AsNoTracking().Include(x => x.Product)
            .Where(x => x.Quantity > 0 && x.ExpiryDate.HasValue && x.ExpiryDate.Value <= threshold && !x.Product.IsDeleted)
            .OrderBy(x => x.ExpiryDate)
            .Select(x => new { x.Id, x.Product.Sku, x.Product.Name, x.Quantity, x.ExpiryDate, IsExpired = x.ExpiryDate!.Value <= DateOnly.FromDateTime(DateTime.Today) })
            .ToListAsync();
        return Ok(data);
    }

    [HttpPost("receipts")]
    public async Task<IActionResult> CreateReceipt(CreateReceiptRequest request)
    {
        context.Require(Request, Role.OWNER, Role.WAREHOUSE_STAFF);
        var receipt = new WarehouseReceipt
        {
            SupplierName = request.SupplierName,
            CreatedBy    = request.CreatedBy,
            Lines        = request.Lines.Select(x => new WarehouseReceiptLine { ProductId = x.ProductId, Quantity = x.Quantity, UnitCost = x.UnitCost, ExpiryDate = x.ExpiryDate }).ToList()
        };
        db.WarehouseReceipts.Add(receipt);
        await db.SaveChangesAsync();
        return Ok(receipt);
    }

    [HttpPatch("receipts/{id:int}/approve")]
    public async Task<IActionResult> ApproveReceipt(int id, ApproveRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        try { return Ok(await inventory.ApproveReceiptAsync(id, request.ApproverId, context.Current(Request).Role)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("issues")]
    public async Task<IActionResult> Issue(IssueStockRequest request)
    {
        try
        {
            var (cost, lots) = await inventory.IssueFEFOAsync(request.ProductId, request.Quantity, request.Sale);
            var issue = new WarehouseIssue
            {
                ProductId = request.ProductId, Quantity  = request.Quantity,
                Type      = request.Sale ? InventoryIssueType.SALE : InventoryIssueType.INTERNAL_USE,
                Cost      = cost, CreatedBy = request.CreatedBy ?? 0, Reason = request.Reason ?? ""
            };
            db.WarehouseIssues.Add(issue);
            await db.SaveChangesAsync();
            return Ok(new { issue, lots });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("shrinkage")]
    public async Task<IActionResult> Shrinkage(CreateShrinkageRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        try
        {
            var (cost, lots) = await inventory.IssueFEFOAsync(request.ProductId, request.Quantity, false);
            var shrinkage = new ShrinkageRecord { ProductId = request.ProductId, Quantity = request.Quantity, Cost = cost, Reason = request.Reason, CreatedBy = request.CreatedBy, Status = ApprovalStatus.PENDING };
            db.ShrinkageRecords.Add(shrinkage);
            db.JournalEntries.Add(new JournalEntry { Source = JournalSource.SHRINKAGE, Description = $"Hao hut: {request.Reason}", Lines = [new JournalLine { AccountCode = "632", AccountName = "Gia von/hao hut", Debit = cost }, new JournalLine { AccountCode = "156", AccountName = "Hang hoa", Credit = cost }] });
            await db.SaveChangesAsync();
            return Ok(new { shrinkage, lots });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPost("stocktake")]
    public async Task<IActionResult> Stocktake(CreateStocktakeRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var session = new StocktakeSession { Name = request.Name, CreatedBy = request.CreatedBy };
        db.StocktakeSessions.Add(session);
        await db.SaveChangesAsync();
        var theoretical = await db.Lots.AsNoTracking().Include(x => x.Product).Where(x => !x.Product.IsDeleted)
            .GroupBy(x => new { x.ProductId, x.Product.Sku, x.Product.Name })
            .Select(g => new { g.Key.ProductId, g.Key.Sku, g.Key.Name, Quantity = g.Sum(l => l.Quantity) })
            .ToListAsync();
        return Ok(new { Session = session, TheoreticalStock = theoretical });
    }
}
