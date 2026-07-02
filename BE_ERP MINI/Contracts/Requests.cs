using BE_ERP_MINI.Domain;

namespace BE_ERP_MINI.Contracts;

// ── HR ───────────────────────────────────────────────────────────────────────
public sealed record CreateEmployeeRequest(string FullName, string Department, string Position, Role Role, decimal BaseSalary, decimal MealAllowance, decimal AttendanceAllowance, string? NationalId, string? BankAccountNumber, string? BankName, decimal AnnualLeaveBalance = 12, DateOnly? HireDate = null, DateOnly? TerminationDate = null);
public sealed record UpdateEmployeeRequest(string? FullName, string? Department, string? Position, Role? Role, EmployeeStatus? Status, decimal? BaseSalary, decimal? MealAllowance, decimal? AttendanceAllowance, string? NationalId, string? BankAccountNumber, string? BankName, decimal? AnnualLeaveBalance, DateOnly? HireDate, DateOnly? TerminationDate, int? Version, string? Reason);
public sealed record AttendanceRequest(int EmployeeId, DateTime Timestamp);
public sealed record CreateLeaveRequest(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason);
public sealed record UpdateLeaveRequest(DateOnly? FromDate, DateOnly? ToDate, string? LeaveType, string? Reason, string? Status);
public sealed record GeneratePayrollRequest(int EmployeeId, int Year, int Month, decimal UnpaidLeaveDays, decimal LateMinutes, decimal ApprovedOtHours);
public sealed record ApproveRequest(int ApproverId);
public sealed record ApproveOtRequest(int ApproverId, decimal OtHours);

public sealed record SetAttendanceStatusRequest(string EmployeeCode, DateOnly WorkDate, string Status);

// ── INVENTORY ────────────────────────────────────────────────────────────────
public sealed record CreateCategoryRequest(string Code, string Name);
public sealed record UpdateCategoryRequest(string? Code, string? Name);
public sealed record CreateBrandRequest(string Code, string Name);
public sealed record UpdateBrandRequest(string? Code, string? Name);
public sealed record CreateProductRequest(string Name, string CategoryCode, string Unit, string? Barcode, decimal MinStockLevel, decimal AverageCost, decimal SalePrice, string? ImageUrl, string? Brand, string? Supplier, string? Sku, bool IsFresh = false);
public sealed record UpdateProductRequest(string? Name, string? CategoryCode, string? Unit, string? Barcode, decimal? MinStockLevel, decimal? AverageCost, decimal? SalePrice, string? ImageUrl, string? Brand, string? Supplier, string? Sku, bool? IsFresh, int? Version, string? Reason);
public sealed record CreateReceiptRequest(string SupplierName, int CreatedBy, string? Notes, int PaymentStatus, List<CreateReceiptLineRequest> Lines);
public sealed record CreateReceiptLineRequest(int ProductId, decimal Quantity, decimal UnitCost, DateOnly? ManufacturingDate, DateOnly? ExpiryDate, string? Notes);
public sealed record CreateSalesInvoiceRequest(string CustomerName, string? CustomerPhone, int? CustomerId, int CreatedBy, string? Notes, int PaymentStatus, int PaymentMethod, List<CreateSalesInvoiceLineRequest> Lines);
public sealed record CreateSalesInvoiceLineRequest(int ProductId, decimal Quantity, decimal UnitPrice, decimal DiscountAmount, string? Notes);
public sealed record SwitchPaymentRequest(int PaymentMethod, int PaymentStatus);
public sealed record CreateCustomerRequest(string Name, string? Phone, string? Email, string? Address, string? Notes);
public sealed record UpdateCustomerRequest(string? Name, string? Phone, string? Email, string? Address, string? Notes);
public sealed record IssueStockRequest(int ProductId, decimal Quantity, bool Sale, int? CreatedBy, string? Reason);
public sealed record CreateShrinkageRequest(int ProductId, decimal Quantity, int CreatedBy, string Reason);
public sealed record CreateStocktakeRequest(string Name, int CreatedBy);
public sealed record CreatePurchaseRequestRequest(int ProductId, decimal Quantity, int CreatedBy, string Reason);

// ── PURCHASING ────────────────────────────────────────────────────────────────
public sealed record CreatePurchaseOrderRequest(string SupplierName, decimal Amount, int? CreatedBy, bool OwnerOverride, string? OverrideReason);

// ── POS ───────────────────────────────────────────────────────────────────────
public sealed record OpenPosSessionRequest(int CashierId, decimal OpeningFloat);
public sealed record ClosePosSessionRequest(int SessionId, int CashierId, decimal ClosingCash);
public sealed record CreatePosTransactionRequest(int SessionId, int CashierId, PaymentMethod PaymentMethod, List<CreatePosTransactionLineRequest> Lines);
public sealed record CreatePosTransactionLineRequest(int ProductId, decimal Quantity);
public sealed record CreatePosRefundRequest(int TransactionId, int ProductId, decimal Quantity, string Reason);

// ── ACCOUNTING ────────────────────────────────────────────────────────────────
public sealed record CreateJournalRequest(string Description, List<JournalLineRequest> Lines);
public sealed record JournalLineRequest(string AccountCode, string AccountName, decimal Debit, decimal Credit);
public sealed record APPaymentRequest(decimal Amount, PaymentMethod Method, string? Reference);
