using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/inventory")]
[Authorize]
public sealed class InventoryController(
    AppDbContext db,
    ErpContext context,
    InventoryService inventory,
    UserActionLogService actionLog,
    IImageService imageService) : ControllerBase
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
            Sku           = string.IsNullOrWhiteSpace(request.Sku) ? $"{cat}-{seq:0000}" : request.Sku,
            Name          = request.Name,
            CategoryCode  = cat,
            Unit          = request.Unit,
            Barcode       = request.Barcode,
            MinStockLevel = request.MinStockLevel,
            AverageCost   = request.AverageCost,
            SalePrice     = request.SalePrice,
            ImageUrl      = request.ImageUrl,
            Brand         = request.Brand,
            Supplier      = request.Supplier,
            IsFresh       = request.IsFresh,
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
        product.Sku           = request.Sku           ?? product.Sku;
        product.IsFresh       = request.IsFresh       ?? product.IsFresh;
        if (request.ImageUrl is not null) product.ImageUrl = request.ImageUrl;
        if (request.Brand is not null) product.Brand = request.Brand;
        if (request.Supplier is not null) product.Supplier = request.Supplier;
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

    [HttpPost("products/{id:int}/image")]
    public async Task<IActionResult> UploadProductImage(int id, [FromForm] IFormFile file)
    {
        try {
            context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
            var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (product is null) return NotFound("Khong tim thay san pham.");

            if (file == null || file.Length == 0) return BadRequest(new { error = "File không hợp lệ hoặc rỗng." });

            var uploadResult = await imageService.AddImageAsync(file);
            if (uploadResult.Error != null) return BadRequest(new { error = uploadResult.Error.Message });

            // Delete old image if exists
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                try {
                    // Try to extract publicId from URL
                    var uri = new Uri(product.ImageUrl);
                    var segments = uri.Segments;
                    var lastSegment = segments.Last();
                    var publicId = lastSegment.Split('.')[0];
                    var folderSegment = segments.Length > 2 ? segments[segments.Length - 2] : "";
                    var fullPublicId = folderSegment + publicId;
                    if (!string.IsNullOrEmpty(fullPublicId)) {
                        await imageService.DeleteImageAsync(fullPublicId);
                    }
                } catch { /* ignore */ }
            }

            if (uploadResult.SecureUrl == null) {
                return StatusCode(500, new { error = "Tải ảnh lên thành công nhưng không lấy được link ảnh (SecureUrl is null)." });
            }

            product.ImageUrl = uploadResult.SecureUrl.ToString();
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = context.Current(Request).UserId;
            product.Version++;
            
            await db.SaveChangesAsync();

            return Ok(new { imageUrl = product.ImageUrl, version = product.Version });
        } catch (Exception ex) {
            return StatusCode(500, new { error = "Lỗi Server (500): " + ex.Message + "\n" + ex.StackTrace });
        }
    }

    [HttpDelete("products/{id:int}/image")]
    public async Task<IActionResult> DeleteProductImage(int id)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (product is null) return NotFound("Khong tim thay san pham.");

        if (string.IsNullOrEmpty(product.ImageUrl)) return NoContent();

        try {
            var uri = new Uri(product.ImageUrl);
            var segments = uri.Segments;
            var lastSegment = segments.Last();
            var publicId = lastSegment.Split('.')[0];
            var folderSegment = segments.Length > 2 ? segments[segments.Length - 2] : "";
            var fullPublicId = folderSegment + publicId;
            if (!string.IsNullOrEmpty(fullPublicId)) {
                await imageService.DeleteImageAsync(fullPublicId);
            }
        } catch { /* ignore */ }

        product.ImageUrl = null;
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = context.Current(Request).UserId;
        product.Version++;
        
        await db.SaveChangesAsync();

        return Ok(new { version = product.Version });
    }

    [HttpGet("stock-balance")]
    public async Task<IActionResult> StockBalance([FromQuery(Name = "product_id")] int? productId)
    {
        var query = db.Lots.AsNoTracking().Include(x => x.Product).Where(x => !x.Product.IsDeleted).AsQueryable();
        if (productId.HasValue) query = query.Where(x => x.ProductId == productId.Value);
        var data = await query
            .GroupBy(x => new { x.ProductId, x.Product.Sku, x.Product.Name, x.Product.MinStockLevel, x.Product.AverageCost, x.Product.Unit })
            .Select(g => new { 
                g.Key.ProductId, 
                g.Key.Sku, 
                g.Key.Name, 
                g.Key.Unit, 
                Quantity = g.Sum(l => l.Quantity), 
                g.Key.MinStockLevel, 
                g.Key.AverageCost, 
                TotalValue = g.Sum(l => l.Quantity * l.UnitCost), 
                LotCount = g.Count(),
                MinExpiryDate = g.Min(l => l.ExpiryDate),
                MaxMfgDate = g.Max(l => l.ManufacturingDate)
            })
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
            SupplierName  = request.SupplierName,
            CreatedBy     = request.CreatedBy,
            Notes         = request.Notes,
            PaymentStatus = (ReceiptPaymentStatus)request.PaymentStatus,
            Lines         = request.Lines.Select(x => new WarehouseReceiptLine { 
                ProductId = x.ProductId, 
                Quantity = x.Quantity, 
                UnitCost = x.UnitCost, 
                ManufacturingDate = x.ManufacturingDate, 
                ExpiryDate = x.ExpiryDate,
                Notes = x.Notes
            }).ToList()
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

    [HttpGet("stocktake")]
    public async Task<IActionResult> GetStocktakeSessions()
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        var sessions = await db.StocktakeSessions.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(sessions);
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

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF, Role.CASHIER, Role.ACCOUNTANT);
        
        var users = await db.Users.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.FullName);
        string GetStaffName(int id) => users.TryGetValue(id, out var name) ? name : "Nhan vien";

        var receipts = await db.WarehouseReceipts.AsNoTracking()
            .Include(x => x.Lines).ThenInclude(x => x.Product)
            .ToListAsync();
            
        var issues = await db.WarehouseIssues.AsNoTracking()
            .Include(x => x.Product)
            .ToListAsync();
            
        var shrinkages = await db.ShrinkageRecords.AsNoTracking()
            .Include(x => x.Product)
            .ToListAsync();
            
        var list = new List<object>();
        foreach (var r in receipts)
        {
            foreach (var l in r.Lines)
            {
                list.Add(new {
                    Id = $"PNK{r.Id:000}",
                    Type = "in",
                    Date = r.CreatedAt.ToString("dd/MM/yyyy"),
                    Time = r.CreatedAt.ToString("HH:mm"),
                    Product = l.Product.Name,
                    Qty = l.Quantity,
                    Unit = l.Product.Unit,
                    Price = l.UnitCost,
                    Total = l.Quantity * l.UnitCost,
                    Supplier = r.SupplierName,
                    Staff = GetStaffName(r.CreatedBy),
                    CreatedAt = r.CreatedAt
                });
            }
        }
        
        foreach (var i in issues)
        {
            list.Add(new {
                Id = $"PXK{i.Id:000}",
                Type = "out",
                Date = i.CreatedAt.ToString("dd/MM/yyyy"),
                Time = i.CreatedAt.ToString("HH:mm"),
                Product = i.Product.Name,
                Qty = i.Quantity,
                Unit = i.Product.Unit,
                Price = i.Product.SalePrice,
                Total = i.Quantity * i.Product.SalePrice,
                Supplier = i.Type == InventoryIssueType.SALE ? "Khach le" : "Hao hut/Khac",
                Staff = GetStaffName(i.CreatedBy),
                CreatedAt = i.CreatedAt
            });
        }
        
        foreach (var s in shrinkages)
        {
            list.Add(new {
                Id = $"SHR{s.Id:000}",
                Type = "out",
                Date = s.CreatedAt.ToString("dd/MM/yyyy"),
                Time = s.CreatedAt.ToString("HH:mm"),
                Product = s.Product.Name,
                Qty = s.Quantity,
                Unit = s.Product.Unit,
                Price = s.Cost,
                Total = s.Quantity * s.Cost,
                Supplier = "Dieu chinh/Hao hut",
                Staff = GetStaffName(s.CreatedBy),
                CreatedAt = s.CreatedAt
            });
        }
        
        var orderedList = list.OrderByDescending(x => (DateTime)x.GetType().GetProperty("CreatedAt").GetValue(x)).ToList();
        return Ok(orderedList);
    }

    // ─── Phân nhóm hàng hóa (Categories) ───────────────────────────────────────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        return Ok(await db.ProductCategories.AsNoTracking().OrderBy(x => x.Code).ToListAsync());
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
    {
        context.Require(Request, Role.OWNER);
        
        var code = request.Code.ToUpperInvariant().Trim();
        if (await db.ProductCategories.AnyAsync(x => x.Code == code))
            return BadRequest("Ma nhom hang da ton tai.");

        var category = new ProductCategory
        {
            Code = code,
            Name = request.Name.Trim()
        };

        db.ProductCategories.Add(category);
        await db.SaveChangesAsync();

        await actionLog.AddLogAsync(Request, "CREATE", "ProductCategory", category.Id.ToString(),
            $"Tao nhom hang {category.Code} - {category.Name}", null, category, category.Code);

        return Ok(category);
    }

    [HttpPatch("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest request)
    {
        context.Require(Request, Role.OWNER);
        
        var category = await db.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound("Khong tim thay nhom hang.");

        var oldCode = category.Code;
        var old = new { category.Code, category.Name };

        if (request.Code is { } c)
        {
            var newCode = c.ToUpperInvariant().Trim();
            if (newCode != oldCode && await db.ProductCategories.AnyAsync(x => x.Code == newCode))
                return BadRequest("Ma nhom hang moi da ton tai.");
            category.Code = newCode;
        }

        category.Name = request.Name?.Trim() ?? category.Name;

        await using var tx = await db.Database.BeginTransactionAsync();
        
        if (oldCode != category.Code)
        {
            var products = await db.Products.Where(x => x.CategoryCode == oldCode).ToListAsync();
            foreach (var p in products)
            {
                p.CategoryCode = category.Code;
            }
        }

        await db.SaveChangesAsync();
        await actionLog.AddLogAsync(Request, "UPDATE", "ProductCategory", category.Id.ToString(),
            $"Cap nhat nhom hang {category.Code}", old, category, category.Code);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(category);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        context.Require(Request, Role.OWNER);
        
        var category = await db.ProductCategories.SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound("Khong tim thay nhom hang.");

        if (await db.Products.AnyAsync(x => x.CategoryCode == category.Code && !x.IsDeleted))
            return BadRequest("Nhom hang dang co san pham, khong the xoa.");

        db.ProductCategories.Remove(category);
        await db.SaveChangesAsync();

        await actionLog.AddLogAsync(Request, "DELETE", "ProductCategory", category.Id.ToString(),
            $"Xoa nhom hang {category.Code}", null, null, category.Code);

        return NoContent();
    }
    // ─── Thương hiệu (Brands) ──────────────────────────────────────────────────

    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands()
    {
        return Ok(await db.ProductBrands.AsNoTracking().OrderBy(x => x.Code).ToListAsync());
    }

    [HttpPost("brands")]
    public async Task<IActionResult> CreateBrand(CreateBrandRequest request)
    {
        context.Require(Request, Role.OWNER);
        
        var code = request.Code.ToUpperInvariant().Trim();
        if (await db.ProductBrands.AnyAsync(x => x.Code == code))
            return BadRequest("Ma thuong hieu da ton tai.");

        var brand = new ProductBrand
        {
            Code = code,
            Name = request.Name.Trim()
        };

        db.ProductBrands.Add(brand);
        await db.SaveChangesAsync();

        await actionLog.AddLogAsync(Request, "CREATE", "ProductBrand", brand.Id.ToString(),
            $"Tao thuong hieu {brand.Code} - {brand.Name}", null, brand, brand.Code);

        return Ok(brand);
    }

    [HttpPatch("brands/{id:int}")]
    public async Task<IActionResult> UpdateBrand(int id, UpdateBrandRequest request)
    {
        context.Require(Request, Role.OWNER);
        
        var brand = await db.ProductBrands.SingleOrDefaultAsync(x => x.Id == id);
        if (brand is null) return NotFound("Khong tim thay thuong hieu.");

        var oldCode = brand.Code;
        var old = new { brand.Code, brand.Name };

        if (request.Code is { } c)
        {
            var newCode = c.ToUpperInvariant().Trim();
            if (newCode != oldCode && await db.ProductBrands.AnyAsync(x => x.Code == newCode))
                return BadRequest("Ma thuong hieu moi da ton tai.");
            brand.Code = newCode;
        }

        brand.Name = request.Name?.Trim() ?? brand.Name;

        await using var tx = await db.Database.BeginTransactionAsync();
        
        if (oldCode != brand.Code)
        {
            var products = await db.Products.Where(x => x.Brand == oldCode).ToListAsync();
            foreach (var p in products)
            {
                p.Brand = brand.Code;
            }
        }

        await db.SaveChangesAsync();
        await actionLog.AddLogAsync(Request, "UPDATE", "ProductBrand", brand.Id.ToString(),
            $"Cap nhat thuong hieu {brand.Code}", old, brand, brand.Code);
        await db.SaveChangesAsync();
        await tx.CommitAsync();

        return Ok(brand);
    }

    [HttpDelete("brands/{id:int}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        context.Require(Request, Role.OWNER);
        
        var brand = await db.ProductBrands.SingleOrDefaultAsync(x => x.Id == id);
        if (brand is null) return NotFound("Khong tim thay thuong hieu.");

        if (await db.Products.AnyAsync(x => x.Brand == brand.Code && !x.IsDeleted))
            return BadRequest("Thuong hieu dang co san pham, khong the xoa.");

        db.ProductBrands.Remove(brand);
        await db.SaveChangesAsync();

        await actionLog.AddLogAsync(Request, "DELETE", "ProductBrand", brand.Id.ToString(),
            $"Xoa thuong hieu {brand.Code}", null, null, brand.Code);

        return NoContent();
    }

    // ─── END INVENTORY ───────────────────────────────────
}

public sealed record CreateBrandRequest(string Code, string Name);
public sealed record UpdateBrandRequest(string? Code, string? Name);
