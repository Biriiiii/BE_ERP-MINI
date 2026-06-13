using BE_ERP_MINI.Domain;

namespace BE_ERP_MINI.Contracts;

// ── HR ───────────────────────────────────────────────────────────────────────
public sealed record CreateEmployeeRequest(string FullName, string Department, string Position, Role Role, decimal BaseSalary, decimal MealAllowance, decimal AttendanceAllowance);
public sealed record UpdateEmployeeRequest(string? FullName, string? Department, string? Position, Role? Role, EmployeeStatus? Status, decimal? BaseSalary, decimal? MealAllowance, decimal? AttendanceAllowance, int? Version, string? Reason);
public sealed record AttendanceRequest(int EmployeeId, DateTime Timestamp);
public sealed record CreateLeaveRequest(int EmployeeId, string LeaveType, DateOnly FromDate, DateOnly ToDate, string Reason);
public sealed record GeneratePayrollRequest(int EmployeeId, int Year, int Month, decimal UnpaidLeaveDays, decimal LateMinutes, decimal ApprovedOtHours);
public sealed record ApproveRequest(int ApproverId);

// ── INVENTORY ────────────────────────────────────────────────────────────────
public sealed record CreateProductRequest(string Name, string CategoryCode, string Unit, string? Barcode, decimal MinStockLevel, decimal AverageCost, decimal SalePrice);
public sealed record UpdateProductRequest(string? Name, string? CategoryCode, string? Unit, string? Barcode, decimal? MinStockLevel, decimal? AverageCost, decimal? SalePrice, int? Version, string? Reason);
public sealed record CreateReceiptRequest(string SupplierName, int CreatedBy, List<CreateReceiptLineRequest> Lines);
public sealed record CreateReceiptLineRequest(int ProductId, decimal Quantity, decimal UnitCost, DateOnly? ExpiryDate);
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

// ── ACCOUNTING ────────────────────────────────────────────────────────────────
public sealed record CreateJournalRequest(string Description, List<JournalLineRequest> Lines);
public sealed record JournalLineRequest(string AccountCode, string AccountName, decimal Debit, decimal Credit);
public sealed record APPaymentRequest(decimal Amount, PaymentMethod Method, string? Reference);
