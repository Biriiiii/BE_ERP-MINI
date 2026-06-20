using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Services;

public sealed class InventoryService(AppDbContext db)
{
    /// <summary>
    /// Duyệt phiếu nhập kho: cập nhật giá vốn bình quân và tạo Lot mới.
    /// </summary>
    public async Task<WarehouseReceipt> ApproveReceiptAsync(int receiptId, int approverId, Role approverRole = Role.STORE_MANAGER)
    {
        var receipt = await db.WarehouseReceipts
            .Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == receiptId)
            ?? throw new InvalidOperationException("Khong tim thay phieu nhap.");

        if (receipt.CreatedBy == approverId && approverRole != Role.OWNER)
            throw new InvalidOperationException("Ban khong the duyet phieu do chinh minh tao.");
        if (receipt.Status == ApprovalStatus.APPROVED)
            return receipt;

        foreach (var line in receipt.Lines)
        {
            var product = await db.Products.SingleAsync(x => x.Id == line.ProductId);
            var oldQty   = await db.Lots.Where(x => x.ProductId == line.ProductId).SumAsync(x => x.Quantity);
            var oldValue = oldQty * product.AverageCost;
            var newValue = line.Quantity * line.UnitCost;
            // TC-INV-003: Giá vốn bình quân gia quyền
            product.AverageCost = (oldQty + line.Quantity) == 0
                ? line.UnitCost
                : decimal.Round((oldValue + newValue) / (oldQty + line.Quantity), 4);

            db.Lots.Add(new Lot
            {
                ProductId  = line.ProductId,
                ReceiptId  = receipt.Id,
                Quantity   = line.Quantity,
                UnitCost   = line.UnitCost,
                ExpiryDate = line.ExpiryDate
            });
        }

        var totalAmount = receipt.Lines.Sum(x => x.Quantity * x.UnitCost);

        // Auto create APInvoice
        db.APInvoices.Add(new APInvoice
        {
            SupplierName = receipt.SupplierName,
            InvoiceNumber = $"INV-{receipt.Id}",
            Amount = totalAmount,
            InvoiceDate = DateOnly.FromDateTime(DateTime.Today),
            DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)), // default 30 days
            IsPaid = false,
            PurchaseOrderId = null // could extract if needed
        });

        // Auto journal
        db.JournalEntries.Add(new JournalEntry
        {
            Source = JournalSource.PO_RECEIPT,
            Description = $"Nhap kho {receipt.SupplierName} (Receipt {receipt.Id})",
            CreatedBy = approverId,
            Lines = [
                new JournalLine { AccountCode = "156", AccountName = "Hang hoa", Debit = totalAmount },
                new JournalLine { AccountCode = "331", AccountName = "Phai tra NCC", Credit = totalAmount }
            ]
        });

        receipt.ApprovedBy = approverId;
        receipt.ApprovedAt = DateTime.UtcNow;
        receipt.Status     = ApprovalStatus.APPROVED;
        await db.SaveChangesAsync();
        return receipt;
    }

    /// <summary>
    /// Xuất kho theo nguyên tắc FEFO (First Expired, First Out).
    /// TC-INV-002: Lô có HSD gần nhất xuất trước.
    /// TC-INV-001: Block bán hàng hết HSD.
    /// </summary>
    public async Task<(decimal Cost, List<object> Lots)> IssueFEFOAsync(int productId, decimal quantity, bool isSale)
    {
        var product = await db.Products.SingleOrDefaultAsync(x => x.Id == productId && !x.IsDeleted)
            ?? throw new InvalidOperationException("Khong tim thay san pham.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var lots  = await db.Lots
            .Where(x => x.ProductId == productId && x.Quantity > 0)
            .OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue)
            .ToListAsync();

        // TC-INV-001: Block bán hàng hết HSD
        if (isSale && lots.Any(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value <= today))
            throw new InvalidOperationException("SAN PHAM DA HET HAN SU DUNG — Khong the ban.");

        var totalStock = lots.Sum(x => x.Quantity);
        if (totalStock < quantity)
            throw new InvalidOperationException($"Ton kho khong du (con {totalStock:N3}, yeu cau {quantity:N3}).");

        var remaining = quantity;
        decimal cost  = 0;
        var issued    = new List<object>();
        foreach (var lot in lots)
        {
            if (remaining <= 0) break;
            var take    = Math.Min(lot.Quantity, remaining);
            lot.Quantity -= take;
            remaining    -= take;
            cost         += take * lot.UnitCost;
            issued.Add(new { lot.Id, Quantity = take, lot.UnitCost, lot.ExpiryDate });
        }

        // Alert khi tồn kho xuống dưới ngưỡng tối thiểu
        var balance = lots.Sum(x => x.Quantity);
        if (balance <= product.MinStockLevel)
        {
            db.UserActionLogs.Add(new UserActionLog
            {
                Action     = "ALERT",
                EntityType = "ReplenishmentAlert",
                EntityId   = product.Id.ToString(),
                EntityLabel= product.Name,
                Summary    = $"{product.Name}: ton kho ({balance:N0}) xuong duoi nguong toi thieu ({product.MinStockLevel:N0}).",
                Source     = "SYSTEM",
                CreatedAt  = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        return (cost, issued);
    }
}
