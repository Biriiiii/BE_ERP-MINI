namespace BE_ERP_MINI.Domain;

// ═══════════════════════════════════════════════════════
//  HR — NHÂN SỰ
// ═══════════════════════════════════════════════════════

public sealed class Employee
{
    public int Id { get; set; }                          // auto-increment: 1, 2, 3...
    public string EmployeeCode { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? NationalId { get; set; }
    public string Department { get; set; } = "";
    public string Position { get; set; } = "";
    public Role Role { get; set; } = Role.EMPLOYEE;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.ACTIVE;
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? TerminationDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal AttendanceAllowance { get; set; }
    public decimal AnnualLeaveBalance { get; set; } = 12;
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    // Audit columns
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public int Version { get; set; } = 1;
    // Navigation
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<SalaryHistory> SalaryHistories { get; set; } = [];
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = [];
}

public sealed class Contract
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContractType { get; set; } = "FULL_TIME";
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string? Notes { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.ACTIVE;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    // Navigation
    public Employee Employee { get; set; } = null!;
}

public sealed class SalaryHistory
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public decimal OldBaseSalary { get; set; }
    public decimal NewBaseSalary { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public string Reason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    // Navigation
    public Employee Employee { get; set; } = null!;
}

public sealed class LeaveRequest
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string LeaveType { get; set; } = "ANNUAL";
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public decimal Days { get; set; }
    public string Reason { get; set; } = "";
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Employee Employee { get; set; } = null!;
}

public sealed class AttendanceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateTime? CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public decimal WorkedHours { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public bool HasApprovedOt { get; set; }
    public decimal OtHours { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.CHECKED_IN;
    // Navigation
    public Employee Employee { get; set; } = null!;
}

public sealed class PayrollRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal AttendanceAllowance { get; set; }
    public decimal OtPay { get; set; }
    public decimal UnpaidLeaveDeduction { get; set; }
    public decimal LateDeduction { get; set; }
    public decimal Gross { get; set; }
    public decimal Insurance { get; set; }
    public decimal PersonalIncomeTax { get; set; }
    public decimal Net { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.DRAFT;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    // Navigation
    public Employee Employee { get; set; } = null!;
    public ICollection<PayrollAdjustment> Adjustments { get; set; } = [];
}

public sealed class PayrollAdjustment
{
    public int Id { get; set; }
    public int PayrollId { get; set; }
    public string Type { get; set; } = "BONUS";
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    // Navigation
    public PayrollRecord Payroll { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════
//  INVENTORY — KHO HÀNG
// ═══════════════════════════════════════════════════════

public sealed class ProductCategory
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class ProductBrand
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string CategoryCode { get; set; } = "";
    public string Unit { get; set; } = "";
    public string? Barcode { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal AverageCost { get; set; }
    public decimal SalePrice { get; set; }
    public string? ImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? Supplier { get; set; }
    public bool IsFresh { get; set; }
    // Audit columns
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public int Version { get; set; } = 1;
    // Navigation
    public ICollection<Lot> Lots { get; set; } = [];
    public ICollection<Promotion> Promotions { get; set; } = [];
}

public sealed class Lot
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? ReceiptId { get; set; }
    public string? LotNumber { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Product Product { get; set; } = null!;
    public WarehouseReceipt? Receipt { get; set; }
}

public sealed class WarehouseReceipt
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = "";
    public string? PoReference { get; set; }
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public ReceiptPaymentStatus PaymentStatus { get; set; } = ReceiptPaymentStatus.UNPAID;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public ICollection<WarehouseReceiptLine> Lines { get; set; } = [];
}

public sealed class WarehouseReceiptLine
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateOnly? ManufacturingDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Notes { get; set; }
    // Navigation
    public WarehouseReceipt Receipt { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class WarehouseIssue
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public InventoryIssueType Type { get; set; } = InventoryIssueType.SALE;
    public decimal Cost { get; set; }
    public string Reason { get; set; } = "";
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.APPROVED;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Product Product { get; set; } = null!;
}

public sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}

public sealed class SalesInvoice
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public int? CustomerId { get; set; }
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public ReceiptPaymentStatus PaymentStatus { get; set; } = ReceiptPaymentStatus.UNPAID;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Customer? Customer { get; set; }
    public ICollection<SalesInvoiceLine> Lines { get; set; } = [];
}

public sealed class SalesInvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? Notes { get; set; }
    // Navigation
    public SalesInvoice Invoice { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class ShrinkageRecord
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Cost { get; set; }
    public string Reason { get; set; } = "";
    public ShrinkageType Type { get; set; } = ShrinkageType.EXPIRED;
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Product Product { get; set; } = null!;
}

public sealed class StocktakeSession
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
}

// ═══════════════════════════════════════════════════════
//  POS — BÁN HÀNG
// ═══════════════════════════════════════════════════════

public sealed class PosSession
{
    public int Id { get; set; }
    public int CashierId { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingCash { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    // Navigation
    public ICollection<PosTransaction> Transactions { get; set; } = [];
}

public sealed class PosTransaction
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public int CashierId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CASH;
    public decimal TotalAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal CostAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public PosSession Session { get; set; } = null!;
    public ICollection<PosTransactionLine> Lines { get; set; } = [];
    public ICollection<PosRefund> Refunds { get; set; } = [];
}

public sealed class PosTransactionLine
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal => Quantity * UnitPrice - DiscountAmount;
    // Navigation
    public PosTransaction Transaction { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public sealed class PosRefund
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = "";
    public int ProcessedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public PosTransaction Transaction { get; set; } = null!;
}

public sealed class Promotion
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int? ProductId { get; set; }
    public string? CategoryCode { get; set; }
    public PromotionType DiscountType { get; set; } = PromotionType.PERCENT;
    public decimal DiscountValue { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    // Navigation
    public Product? Product { get; set; }
}

// ═══════════════════════════════════════════════════════
//  PURCHASING — MUA HÀNG
// ═══════════════════════════════════════════════════════

public sealed class PurchaseRequest
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public int CreatedBy { get; set; }
    public string Reason { get; set; } = "";
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public Product Product { get; set; } = null!;
}

public sealed class PurchaseOrder
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = "";
    public decimal Amount { get; set; }
    public int CreatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.PENDING;
    public string? OverrideReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ═══════════════════════════════════════════════════════
//  ACCOUNTING — KẾ TOÁN
// ═══════════════════════════════════════════════════════

public sealed class JournalEntry
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public JournalSource Source { get; set; } = JournalSource.MANUAL;
    public string Description { get; set; } = "";
    public int? CreatedBy { get; set; }
    public bool IsSystemGenerated => Source != JournalSource.MANUAL && Source != JournalSource.REVERSAL;
    public int? ReversalOfId { get; set; }
    // Navigation
    public ICollection<JournalLine> Lines { get; set; } = [];
}

public sealed class JournalLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public string AccountCode { get; set; } = "";
    public string AccountName { get; set; } = "";
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    // Navigation
    public JournalEntry JournalEntry { get; set; } = null!;
}

public sealed class AccountingPeriod
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedBy { get; set; }
}

public sealed class APInvoice
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = "";
    public string? InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public DateOnly DueDate { get; set; }
    public bool IsPaid { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation
    public ICollection<APPayment> Payments { get; set; } = [];
}

public sealed class APPayment
{
    public int Id { get; set; }
    public int APInvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BANK;
    public string? Reference { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public int PaidBy { get; set; }
    // Navigation
    public APInvoice APInvoice { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════
//  SYSTEM — HỆ THỐNG
// ═══════════════════════════════════════════════════════

public sealed class SystemConfig
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int UpdatedBy { get; set; }
}
