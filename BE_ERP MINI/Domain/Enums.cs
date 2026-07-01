namespace BE_ERP_MINI.Domain;

public enum Role
{
    OWNER,
    ACCOUNTANT,
    STORE_MANAGER,
    WAREHOUSE_STAFF,
    CASHIER,
    SALES_STAFF,
    EMPLOYEE
}

public enum EmployeeStatus    { ACTIVE, TERMINATED }
public enum ContractStatus    { ACTIVE, EXPIRED, TERMINATED }
public enum ApprovalStatus    { DRAFT, PENDING, APPROVED, REJECTED, LOCKED }
public enum PaymentMethod     { CASH, BANK, CARD, EWALLET, CREDIT }
public enum AttendanceStatus  { CHECKED_IN, CHECKED_OUT, LATE, EARLY_LEAVE, MISSING_CHECKOUT, ABSENT, LEAVE, WFH }
public enum InventoryIssueType{ SALE, RETURN_SUPPLIER, INTERNAL_USE, SHRINKAGE }
public enum ShrinkageType     { EXPIRED, DAMAGED, LOST, OTHER }
public enum PromotionType     { PERCENT, FIXED_AMOUNT }
public enum ReceiptPaymentStatus { UNPAID = 1, PARTIAL = 2, PAID = 3 }

public enum JournalSource
{
    MANUAL,
    POS_SALE,
    POS_COGS,
    PO_RECEIPT,
    PO_CASH,
    SHRINKAGE,
    PAYROLL,
    PAYROLL_PAY,
    AP_PAYMENT,
    AR_RECEIPT,
    REVERSAL
}
