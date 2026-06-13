using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ── SYSTEM ──────────────────────────────────────────
    public DbSet<AppUser>         Users          => Set<AppUser>();
    public DbSet<SystemConfig>    SystemConfigs  => Set<SystemConfig>();

    // ── AUDIT ───────────────────────────────────────────
    public DbSet<UserActionLog>   UserActionLogs => Set<UserActionLog>();

    // ── HR ──────────────────────────────────────────────
    public DbSet<Employee>           Employees         => Set<Employee>();
    public DbSet<Contract>           Contracts         => Set<Contract>();
    public DbSet<SalaryHistory>      SalaryHistories   => Set<SalaryHistory>();
    public DbSet<LeaveRequest>       LeaveRequests     => Set<LeaveRequest>();
    public DbSet<AttendanceRecord>   AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PayrollRecord>      PayrollRecords    => Set<PayrollRecord>();
    public DbSet<PayrollAdjustment>  PayrollAdjustments=> Set<PayrollAdjustment>();

    // ── INVENTORY ────────────────────────────────────────
    public DbSet<Product>              Products           => Set<Product>();
    public DbSet<Lot>                  Lots               => Set<Lot>();
    public DbSet<WarehouseReceipt>     WarehouseReceipts  => Set<WarehouseReceipt>();
    public DbSet<WarehouseReceiptLine> WarehouseReceiptLines => Set<WarehouseReceiptLine>();
    public DbSet<WarehouseIssue>       WarehouseIssues    => Set<WarehouseIssue>();
    public DbSet<ShrinkageRecord>      ShrinkageRecords   => Set<ShrinkageRecord>();
    public DbSet<StocktakeSession>     StocktakeSessions  => Set<StocktakeSession>();
    public DbSet<PurchaseRequest>      PurchaseRequests   => Set<PurchaseRequest>();
    public DbSet<PurchaseOrder>        PurchaseOrders     => Set<PurchaseOrder>();

    // ── POS ──────────────────────────────────────────────
    public DbSet<PosSession>        PosSessions         => Set<PosSession>();
    public DbSet<PosTransaction>    PosTransactions     => Set<PosTransaction>();
    public DbSet<PosTransactionLine>PosTransactionLines => Set<PosTransactionLine>();
    public DbSet<PosRefund>         PosRefunds          => Set<PosRefund>();
    public DbSet<Promotion>         Promotions          => Set<Promotion>();

    // ── ACCOUNTING ───────────────────────────────────────
    public DbSet<JournalEntry>    JournalEntries    => Set<JournalEntry>();
    public DbSet<JournalLine>     JournalLines      => Set<JournalLine>();
    public DbSet<AccountingPeriod>AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<APInvoice>       APInvoices        => Set<APInvoice>();
    public DbSet<APPayment>       APPayments        => Set<APPayment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── AppUser ──────────────────────────────────────
        mb.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();   // IDENTITY(1,1)
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.IsDeleted);
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(30);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(50);
        });

        // ── SystemConfig ─────────────────────────────────
        mb.Entity<SystemConfig>(e =>
        {
            e.ToTable("SystemConfigs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.Key).IsUnique();
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.Property(x => x.Value).HasMaxLength(1000);
            e.Property(x => x.Description).HasMaxLength(500);
        });

        // ── UserActionLog ─────────────────────────────────
        mb.Entity<UserActionLog>(e =>
        {
            e.ToTable("UserActionLogs");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();   // IDENTITY(1,1) — long
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.EntityType, x.EntityId });
            e.HasIndex(x => x.CreatedAt);
            e.Property(x => x.UserName).HasMaxLength(100);
            e.Property(x => x.UserRole).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Action).HasMaxLength(30).IsRequired();
            e.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
            e.Property(x => x.EntityLabel).HasMaxLength(255);
            e.Property(x => x.Summary).HasMaxLength(500);
            e.Property(x => x.Source).HasMaxLength(50);
            e.Property(x => x.SessionId).HasMaxLength(100);
            e.Property(x => x.DeviceInfo).HasMaxLength(255);
            e.Property(x => x.IpAddress).HasMaxLength(80);
            e.Property(x => x.OldValueJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.NewValueJson).HasColumnType("nvarchar(max)");
            e.Property(x => x.ChangedFieldsJson).HasColumnType("nvarchar(max)");
        });

        // ── Employee ─────────────────────────────────────
        mb.Entity<Employee>(e =>
        {
            e.ToTable("Employees");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasIndex(x => x.IsDeleted);
            e.HasIndex(x => x.Status);
            e.Property(x => x.EmployeeCode).HasMaxLength(20).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.NationalId).HasMaxLength(20);
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.Position).HasMaxLength(100);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.MealAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.AttendanceAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.AnnualLeaveBalance).HasColumnType("decimal(5,1)");
            e.Property(x => x.BankAccountNumber).HasMaxLength(30);
            e.Property(x => x.BankName).HasMaxLength(100);
            e.HasMany(x => x.Contracts).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
            e.HasMany(x => x.SalaryHistories).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
            e.HasMany(x => x.LeaveRequests).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
            e.HasMany(x => x.AttendanceRecords).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
            e.HasMany(x => x.PayrollRecords).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
        });

        // ── Contract ─────────────────────────────────────
        mb.Entity<Contract>(e =>
        {
            e.ToTable("Contracts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.EmployeeId);
            e.Property(x => x.ContractType).HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.Notes).HasMaxLength(500);
        });

        // ── SalaryHistory ─────────────────────────────────
        mb.Entity<SalaryHistory>(e =>
        {
            e.ToTable("SalaryHistories");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.EmployeeId);
            e.Property(x => x.OldBaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.NewBaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── LeaveRequest ──────────────────────────────────
        mb.Entity<LeaveRequest>(e =>
        {
            e.ToTable("LeaveRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.EmployeeId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.LeaveType).HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Days).HasColumnType("decimal(5,1)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── AttendanceRecord ──────────────────────────────
        mb.Entity<AttendanceRecord>(e =>
        {
            e.ToTable("AttendanceRecords");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => new { x.EmployeeId, x.WorkDate }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.WorkedHours).HasColumnType("decimal(5,2)");
            e.Property(x => x.OtHours).HasColumnType("decimal(5,2)");
        });

        // ── PayrollRecord ─────────────────────────────────
        mb.Entity<PayrollRecord>(e =>
        {
            e.ToTable("PayrollRecords");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => new { x.EmployeeId, x.Year, x.Month }).IsUnique();
            e.HasIndex(x => x.Status);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.BaseSalary).HasColumnType("decimal(18,2)");
            e.Property(x => x.MealAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.AttendanceAllowance).HasColumnType("decimal(18,2)");
            e.Property(x => x.OtPay).HasColumnType("decimal(18,2)");
            e.Property(x => x.UnpaidLeaveDeduction).HasColumnType("decimal(18,2)");
            e.Property(x => x.LateDeduction).HasColumnType("decimal(18,2)");
            e.Property(x => x.Gross).HasColumnType("decimal(18,2)");
            e.Property(x => x.Insurance).HasColumnType("decimal(18,2)");
            e.Property(x => x.PersonalIncomeTax).HasColumnType("decimal(18,2)");
            e.Property(x => x.Net).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Adjustments).WithOne(x => x.Payroll).HasForeignKey(x => x.PayrollId);
        });

        // ── PayrollAdjustment ─────────────────────────────
        mb.Entity<PayrollAdjustment>(e =>
        {
            e.ToTable("PayrollAdjustments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.PayrollId);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
        });

        // ── Product ───────────────────────────────────────
        mb.Entity<Product>(e =>
        {
            e.ToTable("Products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.Sku).IsUnique();
            e.HasIndex(x => x.Barcode);
            e.HasIndex(x => x.IsDeleted);
            e.HasIndex(x => x.CategoryCode);
            e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CategoryCode).HasMaxLength(30).IsRequired();
            e.Property(x => x.Unit).HasMaxLength(20).IsRequired();
            e.Property(x => x.Barcode).HasMaxLength(50);
            e.Property(x => x.MinStockLevel).HasColumnType("decimal(10,3)");
            e.Property(x => x.AverageCost).HasColumnType("decimal(18,4)");
            e.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Lots).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
            e.HasMany(x => x.Promotions).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);
        });

        // ── Lot ───────────────────────────────────────────
        mb.Entity<Lot>(e =>
        {
            e.ToTable("Lots");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.ExpiryDate);
            e.HasIndex(x => x.ReceiptId);
            e.Property(x => x.LotNumber).HasMaxLength(50);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,4)");
            e.HasOne(x => x.Receipt).WithMany().HasForeignKey(x => x.ReceiptId).IsRequired(false);
        });

        // ── WarehouseReceipt ──────────────────────────────
        mb.Entity<WarehouseReceipt>(e =>
        {
            e.ToTable("WarehouseReceipts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.Status);
            e.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
            e.Property(x => x.PoReference).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasMany(x => x.Lines).WithOne(x => x.Receipt).HasForeignKey(x => x.ReceiptId);
        });

        // ── WarehouseReceiptLine ──────────────────────────
        mb.Entity<WarehouseReceiptLine>(e =>
        {
            e.ToTable("WarehouseReceiptLines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.ReceiptId);
            e.HasIndex(x => x.ProductId);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.UnitCost).HasColumnType("decimal(18,4)");
        });

        // ── WarehouseIssue ────────────────────────────────
        mb.Entity<WarehouseIssue>(e =>
        {
            e.ToTable("WarehouseIssues");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.ProductId);
            e.HasIndex(x => x.CreatedAt);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── ShrinkageRecord ───────────────────────────────
        mb.Entity<ShrinkageRecord>(e =>
        {
            e.ToTable("ShrinkageRecords");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.ProductId);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.Cost).HasColumnType("decimal(18,4)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── StocktakeSession ──────────────────────────────
        mb.Entity<StocktakeSession>(e =>
        {
            e.ToTable("StocktakeSessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        // ── PurchaseRequest ───────────────────────────────
        mb.Entity<PurchaseRequest>(e =>
        {
            e.ToTable("PurchaseRequests");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.ProductId);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── PurchaseOrder ─────────────────────────────────
        mb.Entity<PurchaseOrder>(e =>
        {
            e.ToTable("PurchaseOrders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.Status);
            e.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.OverrideReason).HasMaxLength(500);
        });

        // ── PosSession ────────────────────────────────────
        mb.Entity<PosSession>(e =>
        {
            e.ToTable("PosSessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.CashierId);
            e.HasIndex(x => x.Status);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.OpeningFloat).HasColumnType("decimal(18,2)");
            e.Property(x => x.ClosingCash).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Transactions).WithOne(x => x.Session).HasForeignKey(x => x.SessionId);
        });

        // ── PosTransaction ────────────────────────────────
        mb.Entity<PosTransaction>(e =>
        {
            e.ToTable("PosTransactions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.SessionId);
            e.HasIndex(x => x.CashierId);
            e.HasIndex(x => x.CreatedAt);
            e.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.VatAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CostAmount).HasColumnType("decimal(18,4)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Lines).WithOne(x => x.Transaction).HasForeignKey(x => x.TransactionId);
            e.HasMany(x => x.Refunds).WithOne(x => x.Transaction).HasForeignKey(x => x.TransactionId);
        });

        // ── PosTransactionLine ────────────────────────────
        mb.Entity<PosTransactionLine>(e =>
        {
            e.ToTable("PosTransactionLines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.TransactionId);
            e.Ignore(x => x.LineTotal);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        });

        // ── PosRefund ─────────────────────────────────────
        mb.Entity<PosRefund>(e =>
        {
            e.ToTable("PosRefunds");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.TransactionId);
            e.Property(x => x.Quantity).HasColumnType("decimal(10,3)");
            e.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // ── Promotion ─────────────────────────────────────
        mb.Entity<Promotion>(e =>
        {
            e.ToTable("Promotions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => new { x.FromDate, x.ToDate });
            e.HasIndex(x => x.IsActive);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.CategoryCode).HasMaxLength(30);
            e.Property(x => x.DiscountType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.DiscountValue).HasColumnType("decimal(10,4)");
        });

        // ── JournalEntry ──────────────────────────────────
        mb.Entity<JournalEntry>(e =>
        {
            e.ToTable("JournalEntries");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.CreatedAt);
            e.HasIndex(x => x.Source);
            e.Ignore(x => x.IsSystemGenerated);
            e.Property(x => x.Source).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasMany(x => x.Lines).WithOne(x => x.JournalEntry).HasForeignKey(x => x.JournalEntryId);
        });

        // ── JournalLine ───────────────────────────────────
        mb.Entity<JournalLine>(e =>
        {
            e.ToTable("JournalLines");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.JournalEntryId);
            e.HasIndex(x => x.AccountCode);
            e.Property(x => x.AccountCode).HasMaxLength(10).IsRequired();
            e.Property(x => x.AccountName).HasMaxLength(100);
            e.Property(x => x.Debit).HasColumnType("decimal(18,2)");
            e.Property(x => x.Credit).HasColumnType("decimal(18,2)");
        });

        // ── AccountingPeriod ──────────────────────────────
        mb.Entity<AccountingPeriod>(e =>
        {
            e.ToTable("AccountingPeriods");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => new { x.Year, x.Month }).IsUnique();
        });

        // ── APInvoice ─────────────────────────────────────
        mb.Entity<APInvoice>(e =>
        {
            e.ToTable("APInvoices");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.SupplierName);
            e.HasIndex(x => x.DueDate);
            e.HasIndex(x => x.IsPaid);
            e.Property(x => x.SupplierName).HasMaxLength(200).IsRequired();
            e.Property(x => x.InvoiceNumber).HasMaxLength(50);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.HasMany(x => x.Payments).WithOne(x => x.APInvoice).HasForeignKey(x => x.APInvoiceId);
        });

        // ── APPayment ─────────────────────────────────────
        mb.Entity<APPayment>(e =>
        {
            e.ToTable("APPayments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedOnAdd();
            e.HasIndex(x => x.APInvoiceId);
            e.Property(x => x.Method).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Reference).HasMaxLength(100);
        });
    }
}
