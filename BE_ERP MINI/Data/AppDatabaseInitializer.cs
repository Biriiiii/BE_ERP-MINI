using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Data;

public static class AppDatabaseInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Chỉ seed khi DB còn trống
        if (await db.Employees.AnyAsync()) return;

        // ── SYSTEM CONFIG ────────────────────────────────────────────────────────
        db.SystemConfigs.AddRange(
            new SystemConfig { Key = "store_name",           Value = "Sieu Thi Minh Phat",  UpdatedBy = 1 },
            new SystemConfig { Key = "vat_rate",             Value = "0.10",                  UpdatedBy = 1 },
            new SystemConfig { Key = "po_approval_threshold",Value = "10000000",              UpdatedBy = 1 },
            new SystemConfig { Key = "annual_leave_days",    Value = "12",                    UpdatedBy = 1 }
        );

        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");

        // ── USERS ────────────────────────────────────────────────────────────────
        // ID sẽ được SQL Server tự cấp: User 1, User 2, ...
        db.Users.AddRange(
            new AppUser { Username = "owner",     FullName = "Chu Sieu Thi",      Email = "owner@erpmini.vn",     PasswordHash = defaultPasswordHash, Role = Role.OWNER,          IsActive = true, CreatedBy = 1 },
            new AppUser { Username = "manager",   FullName = "Quan Ly Cua Hang",  Email = "manager@erpmini.vn",   PasswordHash = defaultPasswordHash, Role = Role.STORE_MANAGER,  IsActive = true, CreatedBy = 1 },
            new AppUser { Username = "warehouse", FullName = "Thu Kho Hung",      Email = "warehouse@erpmini.vn", PasswordHash = defaultPasswordHash, Role = Role.WAREHOUSE_STAFF,IsActive = true, CreatedBy = 1 },
            new AppUser { Username = "cashier",   FullName = "Thu Ngan Lan",      Email = "cashier@erpmini.vn",   PasswordHash = defaultPasswordHash, Role = Role.CASHIER,        IsActive = true, CreatedBy = 1 },
            new AppUser { Username = "accountant",FullName = "Ke Toan Hoa",       Email = "accountant@erpmini.vn",PasswordHash = defaultPasswordHash, Role = Role.ACCOUNTANT,     IsActive = true, CreatedBy = 1 }
        );
        await db.SaveChangesAsync();

        // Lấy ID thực sau khi SQL Server cấp (User 1, 2, 3, 4, 5...)
        var ownerUserId     = db.Users.Local.First(x => x.Username == "owner").Id;
        var managerUserId   = db.Users.Local.First(x => x.Username == "manager").Id;
        var warehouseUserId = db.Users.Local.First(x => x.Username == "warehouse").Id;
        var cashierUserId   = db.Users.Local.First(x => x.Username == "cashier").Id;
        var accountantUserId= db.Users.Local.First(x => x.Username == "accountant").Id;

        // ── EMPLOYEES ────────────────────────────────────────────────────────────
        // Employee ID tự cấp: Employee 1, 2, 3, ...
        db.Employees.AddRange(
            new Employee { EmployeeCode = "NV-2026-001", FullName = "Chu Sieu Thi",     Department = "Management", Position = "Owner",           Role = Role.OWNER,          BaseSalary = 20_000_000, HireDate = new DateOnly(2020, 1, 1),  CreatedBy = ownerUserId },
            new Employee { EmployeeCode = "NV-2026-002", FullName = "Quan Ly Cua Hang", Department = "Store",      Position = "Store Manager",   Role = Role.STORE_MANAGER,  BaseSalary = 12_000_000, HireDate = new DateOnly(2021, 3, 1),  CreatedBy = ownerUserId },
            new Employee { EmployeeCode = "NV-2026-003", FullName = "Thu Kho Hung",     Department = "Warehouse",  Position = "Warehouse Staff", Role = Role.WAREHOUSE_STAFF,BaseSalary = 8_000_000,  HireDate = new DateOnly(2022, 6, 1),  CreatedBy = ownerUserId },
            new Employee { EmployeeCode = "NV-2026-004", FullName = "Thu Ngan Lan",     Department = "POS",        Position = "Cashier",         Role = Role.CASHIER,        BaseSalary = 7_000_000,  MealAllowance = 500_000, AttendanceAllowance = 300_000, HireDate = new DateOnly(2023, 1, 15), CreatedBy = ownerUserId },
            new Employee { EmployeeCode = "NV-2026-005", FullName = "Ke Toan Hoa",      Department = "Accounting", Position = "Accountant",      Role = Role.ACCOUNTANT,     BaseSalary = 10_000_000, HireDate = new DateOnly(2021, 8, 1),  CreatedBy = ownerUserId }
        );

        // ── PRODUCTS ─────────────────────────────────────────────────────────────
        // Product ID tự cấp: Product 1, 2, 3, ...
        db.Products.AddRange(
            new Product { Sku = "THIT-0001", Name = "Thit Heo Thai Lat",  CategoryCode = "THIT", Unit = "kg",   Barcode = "8930000000011", MinStockLevel = 10, AverageCost = 130_000, SalePrice = 200_000, IsFresh = true, CreatedBy = ownerUserId },
            new Product { Sku = "FMCG-0001", Name = "Gao ST25",           CategoryCode = "FMCG", Unit = "kg",   Barcode = "8930000000028", MinStockLevel = 50, AverageCost = 45_000,  SalePrice = 62_000,  CreatedBy = ownerUserId },
            new Product { Sku = "FMCG-0002", Name = "Dau An Neptune 1L",  CategoryCode = "FMCG", Unit = "chai", Barcode = "8930000000035", MinStockLevel = 20, AverageCost = 45_000,  SalePrice = 62_000,  CreatedBy = ownerUserId }
        );
        await db.SaveChangesAsync();

        // Lấy Product ID thực (Product 1, 2, 3...)
        var pork = db.Products.Local.First(x => x.Sku == "THIT-0001");
        var rice = db.Products.Local.First(x => x.Sku == "FMCG-0001");
        var oil  = db.Products.Local.First(x => x.Sku == "FMCG-0002");

        // ── LOTS ────────────────────────────────────────────────────────────────
        db.Lots.AddRange(
            new Lot { ProductId = pork.Id, Quantity = 15, UnitCost = 130_000, ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)) },
            new Lot { ProductId = pork.Id, Quantity = 20, UnitCost = 132_000, ExpiryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)) },
            new Lot { ProductId = rice.Id, Quantity = 45, UnitCost = 45_000 },
            new Lot { ProductId = oil.Id,  Quantity = 30, UnitCost = 45_000 }
        );

        // ── AP INVOICES ───────────────────────────────────────────────────────────
        db.APInvoices.Add(new APInvoice
        {
            SupplierName = "NCC Anh Tuan",
            Amount       = 5_000_000,
            InvoiceDate  = DateOnly.FromDateTime(DateTime.Today.AddDays(-30)),
            DueDate      = DateOnly.FromDateTime(DateTime.Today.AddDays(-20)),
            IsPaid       = false
        });

        // ── ACCOUNTING PERIODS ────────────────────────────────────────────────────
        for (int i = 5; i >= 0; i--)
        {
            var d = DateTime.Today.AddMonths(-i);
            db.AccountingPeriods.Add(new AccountingPeriod { Year = d.Year, Month = d.Month, IsClosed = i > 0 });
        }

        await db.SaveChangesAsync();
    }
}
