-- ERP MINI - COMPLETE DATABASE SCRIPT
-- Tao database moi trong SSMS, chon database do, roi chay file nay

CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
GO

BEGIN TRANSACTION;
GO

    CREATE TABLE [AccountingPeriods] (
        [Id] int NOT NULL IDENTITY,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [IsClosed] bit NOT NULL,
        [ClosedAt] datetime2 NULL,
        [ClosedBy] int NULL,
        CONSTRAINT [PK_AccountingPeriods] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [APInvoices] (
        [Id] int NOT NULL IDENTITY,
        [SupplierName] nvarchar(200) NOT NULL,
        [InvoiceNumber] nvarchar(50) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [InvoiceDate] date NOT NULL,
        [DueDate] date NOT NULL,
        [IsPaid] bit NOT NULL,
        [PurchaseOrderId] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_APInvoices] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [Employees] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeCode] nvarchar(20) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [NationalId] nvarchar(20) NULL,
        [Department] nvarchar(100) NOT NULL,
        [Position] nvarchar(100) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [HireDate] date NOT NULL,
        [TerminationDate] date NULL,
        [BaseSalary] decimal(18,2) NOT NULL,
        [MealAllowance] decimal(18,2) NOT NULL,
        [AttendanceAllowance] decimal(18,2) NOT NULL,
        [AnnualLeaveBalance] decimal(5,1) NOT NULL,
        [BankAccountNumber] nvarchar(30) NULL,
        [BankName] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        [IsDeleted] bit NOT NULL,
        [Version] int NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [JournalEntries] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [Source] nvarchar(30) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CreatedBy] int NULL,
        [ReversalOfId] int NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [PosSessions] (
        [Id] int NOT NULL IDENTITY,
        [CashierId] int NOT NULL,
        [OpeningFloat] decimal(18,2) NOT NULL,
        [ClosingCash] decimal(18,2) NULL,
        [OpenedAt] datetime2 NOT NULL,
        [ClosedAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_PosSessions] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [Products] (
        [Id] int NOT NULL IDENTITY,
        [Sku] nvarchar(50) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [CategoryCode] nvarchar(30) NOT NULL,
        [Unit] nvarchar(20) NOT NULL,
        [Barcode] nvarchar(50) NULL,
        [MinStockLevel] decimal(10,3) NOT NULL,
        [AverageCost] decimal(18,4) NOT NULL,
        [SalePrice] decimal(18,2) NOT NULL,
        [IsFresh] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        [IsDeleted] bit NOT NULL,
        [Version] int NOT NULL,
        CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [PurchaseOrders] (
        [Id] int NOT NULL IDENTITY,
        [SupplierName] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [CreatedBy] int NOT NULL,
        [ApprovedBy] int NULL,
        [ApprovedAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        [OverrideReason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [StocktakeSessions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [CreatedBy] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_StocktakeSessions] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [SystemConfigs] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(1000) NOT NULL,
        [Description] nvarchar(500) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] int NOT NULL,
        CONSTRAINT [PK_SystemConfigs] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [UserActionLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] int NULL,
        [UserName] nvarchar(100) NULL,
        [UserRole] nvarchar(50) NOT NULL,
        [Action] nvarchar(30) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [EntityLabel] nvarchar(255) NULL,
        [Summary] nvarchar(500) NULL,
        [OldValueJson] nvarchar(max) NULL,
        [NewValueJson] nvarchar(max) NULL,
        [ChangedFieldsJson] nvarchar(max) NULL,
        [Reason] nvarchar(max) NULL,
        [Source] nvarchar(50) NOT NULL,
        [SessionId] nvarchar(100) NULL,
        [DeviceInfo] nvarchar(255) NULL,
        [IpAddress] nvarchar(80) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_UserActionLogs] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Username] nvarchar(100) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [Email] nvarchar(200) NOT NULL,
        [PhoneNumber] nvarchar(30) NULL,
        [Role] nvarchar(50) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] int NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] int NULL,
        [IsDeleted] bit NOT NULL,
        [Version] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [WarehouseReceipts] (
        [Id] int NOT NULL IDENTITY,
        [SupplierName] nvarchar(200) NOT NULL,
        [PoReference] nvarchar(50) NULL,
        [CreatedBy] int NOT NULL,
        [ApprovedBy] int NULL,
        [ApprovedAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WarehouseReceipts] PRIMARY KEY ([Id])
    );
GO

    CREATE TABLE [APPayments] (
        [Id] int NOT NULL IDENTITY,
        [APInvoiceId] int NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] nvarchar(20) NOT NULL,
        [Reference] nvarchar(100) NULL,
        [PaidAt] datetime2 NOT NULL,
        [PaidBy] int NOT NULL,
        CONSTRAINT [PK_APPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_APPayments_APInvoices_APInvoiceId] FOREIGN KEY ([APInvoiceId]) REFERENCES [APInvoices] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [AttendanceRecords] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [WorkDate] date NOT NULL,
        [CheckInAt] datetime2 NULL,
        [CheckOutAt] datetime2 NULL,
        [WorkedHours] decimal(5,2) NOT NULL,
        [LateMinutes] int NOT NULL,
        [EarlyLeaveMinutes] int NOT NULL,
        [HasApprovedOt] bit NOT NULL,
        [OtHours] decimal(5,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_AttendanceRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AttendanceRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [Contracts] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [ContractType] nvarchar(30) NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NULL,
        [BaseSalary] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        CONSTRAINT [PK_Contracts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Contracts_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [LeaveRequests] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [LeaveType] nvarchar(20) NOT NULL,
        [FromDate] date NOT NULL,
        [ToDate] date NOT NULL,
        [Days] decimal(5,1) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedBy] int NOT NULL,
        [ApprovedBy] int NULL,
        [ApprovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [PayrollRecords] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [BaseSalary] decimal(18,2) NOT NULL,
        [MealAllowance] decimal(18,2) NOT NULL,
        [AttendanceAllowance] decimal(18,2) NOT NULL,
        [OtPay] decimal(18,2) NOT NULL,
        [UnpaidLeaveDeduction] decimal(18,2) NOT NULL,
        [LateDeduction] decimal(18,2) NOT NULL,
        [Gross] decimal(18,2) NOT NULL,
        [Insurance] decimal(18,2) NOT NULL,
        [PersonalIncomeTax] decimal(18,2) NOT NULL,
        [Net] decimal(18,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ApprovedBy] int NULL,
        [ApprovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        CONSTRAINT [PK_PayrollRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollRecords_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [SalaryHistories] (
        [Id] int NOT NULL IDENTITY,
        [EmployeeId] int NOT NULL,
        [OldBaseSalary] decimal(18,2) NOT NULL,
        [NewBaseSalary] decimal(18,2) NOT NULL,
        [EffectiveDate] date NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        CONSTRAINT [PK_SalaryHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SalaryHistories_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [JournalLines] (
        [Id] int NOT NULL IDENTITY,
        [JournalEntryId] int NOT NULL,
        [AccountCode] nvarchar(10) NOT NULL,
        [AccountName] nvarchar(100) NOT NULL,
        [Debit] decimal(18,2) NOT NULL,
        [Credit] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_JournalLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_JournalLines_JournalEntries_JournalEntryId] FOREIGN KEY ([JournalEntryId]) REFERENCES [JournalEntries] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [PosTransactions] (
        [Id] int NOT NULL IDENTITY,
        [SessionId] int NOT NULL,
        [CashierId] int NOT NULL,
        [PaymentMethod] nvarchar(20) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [VatAmount] decimal(18,2) NOT NULL,
        [CostAmount] decimal(18,4) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PosTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PosTransactions_PosSessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [PosSessions] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [Promotions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [ProductId] int NULL,
        [CategoryCode] nvarchar(30) NULL,
        [DiscountType] nvarchar(20) NOT NULL,
        [DiscountValue] decimal(10,4) NOT NULL,
        [FromDate] date NOT NULL,
        [ToDate] date NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        CONSTRAINT [PK_Promotions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Promotions_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id])
    );
GO

    CREATE TABLE [PurchaseRequests] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [CreatedBy] int NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PurchaseRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseRequests_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [ShrinkageRecords] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [Cost] decimal(18,4) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [CreatedBy] int NOT NULL,
        [ApprovedBy] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ShrinkageRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ShrinkageRecords_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [WarehouseIssues] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [Type] nvarchar(30) NOT NULL,
        [Cost] decimal(18,4) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [CreatedBy] int NOT NULL,
        [ApprovedBy] int NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WarehouseIssues] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WarehouseIssues_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [Lots] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [ReceiptId] int NULL,
        [LotNumber] nvarchar(50) NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [UnitCost] decimal(18,4) NOT NULL,
        [ExpiryDate] date NULL,
        [ReceivedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Lots] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Lots_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Lots_WarehouseReceipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [WarehouseReceipts] ([Id])
    );
GO

    CREATE TABLE [WarehouseReceiptLines] (
        [Id] int NOT NULL IDENTITY,
        [ReceiptId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [UnitCost] decimal(18,4) NOT NULL,
        [ExpiryDate] date NULL,
        CONSTRAINT [PK_WarehouseReceiptLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WarehouseReceiptLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WarehouseReceiptLines_WarehouseReceipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [WarehouseReceipts] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [PayrollAdjustments] (
        [Id] int NOT NULL IDENTITY,
        [PayrollId] int NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] int NOT NULL,
        CONSTRAINT [PK_PayrollAdjustments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PayrollAdjustments_PayrollRecords_PayrollId] FOREIGN KEY ([PayrollId]) REFERENCES [PayrollRecords] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [PosRefunds] (
        [Id] int NOT NULL IDENTITY,
        [TransactionId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [RefundAmount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ProcessedBy] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PosRefunds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PosRefunds_PosTransactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [PosTransactions] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE TABLE [PosTransactionLines] (
        [Id] int NOT NULL IDENTITY,
        [TransactionId] int NOT NULL,
        [ProductId] int NOT NULL,
        [Quantity] decimal(10,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_PosTransactionLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PosTransactionLines_PosTransactions_TransactionId] FOREIGN KEY ([TransactionId]) REFERENCES [PosTransactions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PosTransactionLines_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
    );
GO

    CREATE UNIQUE INDEX [IX_AccountingPeriods_Year_Month] ON [AccountingPeriods] ([Year], [Month]);
GO

    CREATE INDEX [IX_APInvoices_DueDate] ON [APInvoices] ([DueDate]);
GO

    CREATE INDEX [IX_APInvoices_IsPaid] ON [APInvoices] ([IsPaid]);
GO

    CREATE INDEX [IX_APInvoices_SupplierName] ON [APInvoices] ([SupplierName]);
GO

    CREATE INDEX [IX_APPayments_APInvoiceId] ON [APPayments] ([APInvoiceId]);
GO

    CREATE UNIQUE INDEX [IX_AttendanceRecords_EmployeeId_WorkDate] ON [AttendanceRecords] ([EmployeeId], [WorkDate]);
GO

    CREATE INDEX [IX_Contracts_EmployeeId] ON [Contracts] ([EmployeeId]);
GO

    CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);
GO

    CREATE INDEX [IX_Employees_IsDeleted] ON [Employees] ([IsDeleted]);
GO

    CREATE INDEX [IX_Employees_Status] ON [Employees] ([Status]);
GO

    CREATE INDEX [IX_JournalEntries_CreatedAt] ON [JournalEntries] ([CreatedAt]);
GO

    CREATE INDEX [IX_JournalEntries_Source] ON [JournalEntries] ([Source]);
GO

    CREATE INDEX [IX_JournalLines_AccountCode] ON [JournalLines] ([AccountCode]);
GO

    CREATE INDEX [IX_JournalLines_JournalEntryId] ON [JournalLines] ([JournalEntryId]);
GO

    CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);
GO

    CREATE INDEX [IX_LeaveRequests_Status] ON [LeaveRequests] ([Status]);
GO

    CREATE INDEX [IX_Lots_ExpiryDate] ON [Lots] ([ExpiryDate]);
GO

    CREATE INDEX [IX_Lots_ProductId] ON [Lots] ([ProductId]);
GO

    CREATE INDEX [IX_Lots_ReceiptId] ON [Lots] ([ReceiptId]);
GO

    CREATE INDEX [IX_PayrollAdjustments_PayrollId] ON [PayrollAdjustments] ([PayrollId]);
GO

    CREATE UNIQUE INDEX [IX_PayrollRecords_EmployeeId_Year_Month] ON [PayrollRecords] ([EmployeeId], [Year], [Month]);
GO

    CREATE INDEX [IX_PayrollRecords_Status] ON [PayrollRecords] ([Status]);
GO

    CREATE INDEX [IX_PosRefunds_TransactionId] ON [PosRefunds] ([TransactionId]);
GO

    CREATE INDEX [IX_PosSessions_CashierId] ON [PosSessions] ([CashierId]);
GO

    CREATE INDEX [IX_PosSessions_Status] ON [PosSessions] ([Status]);
GO

    CREATE INDEX [IX_PosTransactionLines_ProductId] ON [PosTransactionLines] ([ProductId]);
GO

    CREATE INDEX [IX_PosTransactionLines_TransactionId] ON [PosTransactionLines] ([TransactionId]);
GO

    CREATE INDEX [IX_PosTransactions_CashierId] ON [PosTransactions] ([CashierId]);
GO

    CREATE INDEX [IX_PosTransactions_CreatedAt] ON [PosTransactions] ([CreatedAt]);
GO

    CREATE INDEX [IX_PosTransactions_SessionId] ON [PosTransactions] ([SessionId]);
GO

    CREATE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]);
GO

    CREATE INDEX [IX_Products_CategoryCode] ON [Products] ([CategoryCode]);
GO

    CREATE INDEX [IX_Products_IsDeleted] ON [Products] ([IsDeleted]);
GO

    CREATE UNIQUE INDEX [IX_Products_Sku] ON [Products] ([Sku]);
GO

    CREATE INDEX [IX_Promotions_FromDate_ToDate] ON [Promotions] ([FromDate], [ToDate]);
GO

    CREATE INDEX [IX_Promotions_IsActive] ON [Promotions] ([IsActive]);
GO

    CREATE INDEX [IX_Promotions_ProductId] ON [Promotions] ([ProductId]);
GO

    CREATE INDEX [IX_PurchaseOrders_Status] ON [PurchaseOrders] ([Status]);
GO

    CREATE INDEX [IX_PurchaseRequests_ProductId] ON [PurchaseRequests] ([ProductId]);
GO

    CREATE INDEX [IX_SalaryHistories_EmployeeId] ON [SalaryHistories] ([EmployeeId]);
GO

    CREATE INDEX [IX_ShrinkageRecords_ProductId] ON [ShrinkageRecords] ([ProductId]);
GO

    CREATE UNIQUE INDEX [IX_SystemConfigs_Key] ON [SystemConfigs] ([Key]);
GO

    CREATE INDEX [IX_UserActionLogs_CreatedAt] ON [UserActionLogs] ([CreatedAt]);
GO

    CREATE INDEX [IX_UserActionLogs_EntityType_EntityId] ON [UserActionLogs] ([EntityType], [EntityId]);
GO

    CREATE INDEX [IX_UserActionLogs_UserId] ON [UserActionLogs] ([UserId]);
GO

    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO

    CREATE INDEX [IX_Users_IsDeleted] ON [Users] ([IsDeleted]);
GO

    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
GO

    CREATE INDEX [IX_WarehouseIssues_CreatedAt] ON [WarehouseIssues] ([CreatedAt]);
GO

    CREATE INDEX [IX_WarehouseIssues_ProductId] ON [WarehouseIssues] ([ProductId]);
GO

    CREATE INDEX [IX_WarehouseReceiptLines_ProductId] ON [WarehouseReceiptLines] ([ProductId]);
GO

    CREATE INDEX [IX_WarehouseReceiptLines_ReceiptId] ON [WarehouseReceiptLines] ([ReceiptId]);
GO

    CREATE INDEX [IX_WarehouseReceipts_Status] ON [WarehouseReceipts] ([Status]);
GO
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

    ALTER TABLE [Users] ADD [PasswordHash] nvarchar(max) NOT NULL DEFAULT N'';
GO
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(255) NOT NULL;
GO
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

    ALTER TABLE [UserActionLogs] ADD [ParentAuditId] bigint NULL;
GO
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(max) NULL;
GO
GO

COMMIT;
GO

-- ══════════════════════════════════════════════════════════════
-- ERP MINI — SEED DATA SCRIPT
-- Chay file nay SAU KHI da chay database_script.sql (schema)
-- Mat khau mac dinh cho tat ca user: 123456
-- ══════════════════════════════════════════════════════════════

-- ── XOA DU LIEU CU (neu co) ────────────────────────────────
-- Xoa theo thu tu FK: con truoc, cha sau
DELETE FROM [PosRefunds];
DELETE FROM [PosTransactionLines];
DELETE FROM [PosTransactions];
DELETE FROM [PosSessions];
DELETE FROM [PayrollAdjustments];
DELETE FROM [PayrollRecords];
DELETE FROM [LeaveRequests];
DELETE FROM [AttendanceRecords];
DELETE FROM [SalaryHistories];
DELETE FROM [Contracts];
DELETE FROM [JournalLines];
DELETE FROM [JournalEntries];
DELETE FROM [APPayments];
DELETE FROM [APInvoices];
DELETE FROM [WarehouseReceiptLines];
DELETE FROM [Lots];
DELETE FROM [WarehouseReceipts];
DELETE FROM [ShrinkageRecords];
DELETE FROM [WarehouseIssues];
DELETE FROM [PurchaseRequests];
DELETE FROM [Promotions];
DELETE FROM [Products];
DELETE FROM [PurchaseOrders];
DELETE FROM [StocktakeSessions];
DELETE FROM [Employees];
DELETE FROM [Users];
DELETE FROM [UserActionLogs];
DELETE FROM [SystemConfigs];
DELETE FROM [AccountingPeriods];
GO

BEGIN TRANSACTION;
GO

-- ── SYSTEM CONFIGS ──────────────────────────────────────────
SET IDENTITY_INSERT [SystemConfigs] ON;
INSERT INTO [SystemConfigs] ([Id],[Key],[Value],[UpdatedBy],[UpdatedAt]) VALUES
(1, 'store_name',            N'Sieu Thi Minh Phat',  1, GETDATE()),
(2, 'vat_rate',              '0.10',                  1, GETDATE()),
(3, 'po_approval_threshold', '10000000',              1, GETDATE()),
(4, 'annual_leave_days',     '12',                    1, GETDATE());
SET IDENTITY_INSERT [SystemConfigs] OFF;
GO

-- ── USERS ───────────────────────────────────────────────────
-- Password = 123456 (BCrypt hash)
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id],[Username],[FullName],[Email],[PasswordHash],[Role],[IsActive],[CreatedBy],[CreatedAt],[IsDeleted],[Version]) VALUES
(1, 'owner',     N'Chu Sieu Thi',      'owner@erpmini.vn',     '$2a$11$K8mXxQZv6tYPz1Q3p5u8XeJz7VR9kDEYkG2QLXB5rJHGmNwXq3W6m', 'OWNER',          1, 1, GETDATE(), 0, 1),
(2, 'manager',   N'Quan Ly Cua Hang',  'manager@erpmini.vn',   '$2a$11$K8mXxQZv6tYPz1Q3p5u8XeJz7VR9kDEYkG2QLXB5rJHGmNwXq3W6m', 'STORE_MANAGER',  1, 1, GETDATE(), 0, 1),
(3, 'warehouse', N'Thu Kho Hung',      'warehouse@erpmini.vn', '$2a$11$K8mXxQZv6tYPz1Q3p5u8XeJz7VR9kDEYkG2QLXB5rJHGmNwXq3W6m', 'WAREHOUSE_STAFF',1, 1, GETDATE(), 0, 1),
(4, 'cashier',   N'Thu Ngan Lan',      'cashier@erpmini.vn',   '$2a$11$K8mXxQZv6tYPz1Q3p5u8XeJz7VR9kDEYkG2QLXB5rJHGmNwXq3W6m', 'CASHIER',        1, 1, GETDATE(), 0, 1),
(5, 'accountant',N'Ke Toan Hoa',       'accountant@erpmini.vn','$2a$11$K8mXxQZv6tYPz1Q3p5u8XeJz7VR9kDEYkG2QLXB5rJHGmNwXq3W6m', 'ACCOUNTANT',     1, 1, GETDATE(), 0, 1);
SET IDENTITY_INSERT [Users] OFF;
GO

-- ── EMPLOYEES ───────────────────────────────────────────────
SET IDENTITY_INSERT [Employees] ON;
INSERT INTO [Employees] ([Id],[EmployeeCode],[FullName],[Department],[Position],[Role],[Status],[HireDate],[BaseSalary],[MealAllowance],[AttendanceAllowance],[AnnualLeaveBalance],[CreatedBy],[CreatedAt],[IsDeleted],[Version]) VALUES
(1, 'NV-2026-001', N'Chu Sieu Thi',     N'Management', N'Owner',           'OWNER',          'ACTIVE', '2020-01-01', 20000000, 0,      0,   12.0, 1, GETDATE(), 0, 1),
(2, 'NV-2026-002', N'Quan Ly Cua Hang', N'Store',      N'Store Manager',   'STORE_MANAGER',  'ACTIVE', '2021-03-01', 12000000, 0,      0,   12.0, 1, GETDATE(), 0, 1),
(3, 'NV-2026-003', N'Thu Kho Hung',     N'Warehouse',  N'Warehouse Staff', 'WAREHOUSE_STAFF','ACTIVE', '2022-06-01',  8000000, 0,      0,   12.0, 1, GETDATE(), 0, 1),
(4, 'NV-2026-004', N'Thu Ngan Lan',     N'POS',        N'Cashier',         'CASHIER',        'ACTIVE', '2023-01-15',  7000000, 500000, 300000, 12.0, 1, GETDATE(), 0, 1),
(5, 'NV-2026-005', N'Ke Toan Hoa',     N'Accounting', N'Accountant',      'ACCOUNTANT',     'ACTIVE', '2021-08-01', 10000000, 0,      0,   12.0, 1, GETDATE(), 0, 1);
SET IDENTITY_INSERT [Employees] OFF;
GO

-- ── CONTRACTS ───────────────────────────────────────────────
SET IDENTITY_INSERT [Contracts] ON;
INSERT INTO [Contracts] ([Id],[EmployeeId],[ContractType],[StartDate],[EndDate],[BaseSalary],[Notes],[Status],[CreatedAt],[CreatedBy]) VALUES
(1, 1, 'INDEFINITE',  '2020-01-01', NULL,         20000000, N'Hop dong khong thoi han - Chu sieu thi', 'ACTIVE', GETDATE(), 1),
(2, 2, 'DEFINITE',    '2024-03-01', '2026-08-31', 12000000, N'Hop dong 2 nam',                        'ACTIVE', GETDATE(), 1),
(3, 3, 'DEFINITE',    '2025-06-01', '2026-07-15', 8000000,  N'Hop dong 1 nam - sap het han',           'ACTIVE', GETDATE(), 1),
(4, 4, 'PROBATION',   '2026-01-15', '2026-07-15', 7000000,  N'Thu viec 6 thang',                       'ACTIVE', GETDATE(), 1),
(5, 5, 'INDEFINITE',  '2021-08-01', NULL,         10000000, N'Hop dong khong thoi han',                'ACTIVE', GETDATE(), 1);
SET IDENTITY_INSERT [Contracts] OFF;
GO

-- ── PRODUCTS ────────────────────────────────────────────────
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([Id],[Sku],[Name],[CategoryCode],[Unit],[Barcode],[MinStockLevel],[AverageCost],[SalePrice],[IsFresh],[ImageUrl],[CreatedBy],[CreatedAt],[IsDeleted],[Version]) VALUES
(1,  'THIT-0001', N'Thit Heo Thai Lat',     'THIT',  'kg',   '8930000000011', 10,  130000,  200000, 1, 'https://i.imgur.com/pork_slice.jpg',    1, GETDATE(), 0, 1),
(2,  'FMCG-0001', N'Gao ST25',              'FMCG',  'kg',   '8930000000028', 50,  45000,   62000,  0, 'https://i.imgur.com/gao_st25.jpg',     1, GETDATE(), 0, 1),
(3,  'FMCG-0002', N'Dau An Neptune 1L',     'FMCG',  'chai', '8930000000035', 20,  45000,   62000,  0, 'https://i.imgur.com/dau_neptune.jpg',  1, GETDATE(), 0, 1),
(4,  'NUOC-0001', N'Nuoc Suoi Aquafina 500ml','NUOC','chai', '8930000000042', 100, 3500,    7000,   0, 'https://i.imgur.com/aquafina.jpg',     1, GETDATE(), 0, 1),
(5,  'BIA-0001',  N'Bia Heineken 330ml',    'BIA',   'lon',  '8934673123456', 24,  15000,   22000,  0, 'https://i.imgur.com/heineken.jpg',     1, GETDATE(), 0, 1),
(6,  'SUA-0001',  N'Sua Vinamilk 1L',       'SUA',   'hop',  '8930000000059', 30,  28000,   38000,  0, 'https://i.imgur.com/vinamilk.jpg',     1, GETDATE(), 0, 1),
(7,  'THIT-0002', N'Ga Nguyen Con',         'THIT',  'kg',   '8930000000066', 8,   85000,   130000, 1, 'https://i.imgur.com/ga_nguyen_con.jpg',1, GETDATE(), 0, 1),
(8,  'FMCG-0003', N'Mi Hao Hao Tom Chua Cay','FMCG','goi',  '8930000000073', 200, 3000,    5000,   0, 'https://i.imgur.com/mi_haohao.jpg',   1, GETDATE(), 0, 1),
(9,  'RAU-0001',  N'Rau Muong',             'RAU',   'bo',   '8930000000080', 15,  8000,    15000,  1, 'https://i.imgur.com/rau_muong.jpg',   1, GETDATE(), 0, 1),
(10, 'RAU-0002',  N'Ca Chua',               'RAU',   'kg',   '8930000000097', 10,  15000,   25000,  1, 'https://i.imgur.com/ca_chua.jpg',     1, GETDATE(), 0, 1);
SET IDENTITY_INSERT [Products] OFF;
GO

-- ── LOTS (Ton kho theo lo) ──────────────────────────────────
SET IDENTITY_INSERT [Lots] ON;
INSERT INTO [Lots] ([Id],[ProductId],[Quantity],[UnitCost],[ExpiryDate],[ReceivedAt]) VALUES
(1,  1, 15.000, 130000, CAST(DATEADD(day,2,GETDATE()) AS date),  GETDATE()),
(2,  1, 20.000, 132000, CAST(DATEADD(day,5,GETDATE()) AS date),  GETDATE()),
(3,  2, 45.000, 45000,  NULL, GETDATE()),
(4,  3, 30.000, 45000,  NULL, GETDATE()),
(5,  4, 120.000, 3500,  CAST(DATEADD(day,90,GETDATE()) AS date), GETDATE()),
(6,  5, 48.000, 15000,  CAST(DATEADD(day,180,GETDATE()) AS date),GETDATE()),
(7,  6, 25.000, 28000,  CAST(DATEADD(day,30,GETDATE()) AS date), GETDATE()),
(8,  7, 10.000, 85000,  CAST(DATEADD(day,3,GETDATE()) AS date),  GETDATE()),
(9,  8, 300.000, 3000,  CAST(DATEADD(day,365,GETDATE()) AS date),GETDATE()),
(10, 9, 20.000, 8000,   CAST(DATEADD(day,1,GETDATE()) AS date),  GETDATE()),
(11, 10, 18.000, 15000, CAST(DATEADD(day,3,GETDATE()) AS date),  GETDATE());
SET IDENTITY_INSERT [Lots] OFF;
GO

-- ── WAREHOUSE RECEIPTS (Phieu nhap kho) ─────────────────────
SET IDENTITY_INSERT [WarehouseReceipts] ON;
INSERT INTO [WarehouseReceipts] ([Id],[SupplierName],[CreatedBy],[Status],[CreatedAt],[ApprovedBy],[ApprovedAt]) VALUES
(1, N'NCC Anh Tuan',       3, 'APPROVED', DATEADD(day,-10,GETDATE()), 1, DATEADD(day,-9,GETDATE())),
(2, N'NCC Minh Thanh',     3, 'APPROVED', DATEADD(day,-5,GETDATE()),  2, DATEADD(day,-4,GETDATE())),
(3, N'NCC Ba Huan',        3, 'PENDING',  GETDATE(), NULL, NULL);
SET IDENTITY_INSERT [WarehouseReceipts] OFF;
GO

SET IDENTITY_INSERT [WarehouseReceiptLines] ON;
INSERT INTO [WarehouseReceiptLines] ([Id],[ReceiptId],[ProductId],[Quantity],[UnitCost],[ExpiryDate]) VALUES
(1, 1, 1, 15, 130000, CAST(DATEADD(day,2,GETDATE()) AS date)),
(2, 1, 2, 45, 45000,  NULL),
(3, 2, 4, 120, 3500,  CAST(DATEADD(day,90,GETDATE()) AS date)),
(4, 2, 5, 48,  15000, CAST(DATEADD(day,180,GETDATE()) AS date)),
(5, 3, 7, 15,  85000, CAST(DATEADD(day,10,GETDATE()) AS date));
SET IDENTITY_INSERT [WarehouseReceiptLines] OFF;
GO

-- ── AP INVOICES (Cong no nha cung cap) ──────────────────────
SET IDENTITY_INSERT [APInvoices] ON;
INSERT INTO [APInvoices] ([Id],[SupplierName],[InvoiceNumber],[Amount],[InvoiceDate],[DueDate],[IsPaid],[CreatedAt]) VALUES
(1, N'NCC Anh Tuan',   'INV-001', 5000000,  CAST(DATEADD(day,-30,GETDATE()) AS date), CAST(DATEADD(day,-20,GETDATE()) AS date), 0, GETDATE()),
(2, N'NCC Minh Thanh', 'INV-002', 1140000,  CAST(DATEADD(day,-5,GETDATE()) AS date),  CAST(DATEADD(day,25,GETDATE()) AS date),  0, GETDATE()),
(3, N'NCC Ba Huan',    'INV-003', 3200000,  CAST(DATEADD(day,-2,GETDATE()) AS date),  CAST(DATEADD(day,28,GETDATE()) AS date),  0, GETDATE());
SET IDENTITY_INSERT [APInvoices] OFF;
GO

-- ── PURCHASE ORDERS ─────────────────────────────────────────
SET IDENTITY_INSERT [PurchaseOrders] ON;
INSERT INTO [PurchaseOrders] ([Id],[SupplierName],[Amount],[CreatedBy],[Status],[CreatedAt],[ApprovedBy],[ApprovedAt]) VALUES
(1, N'NCC Anh Tuan',   5000000,  1, 'APPROVED', DATEADD(day,-12,GETDATE()), 1, DATEADD(day,-11,GETDATE())),
(2, N'NCC Minh Thanh', 1140000,  2, 'APPROVED', DATEADD(day,-6,GETDATE()),  1, DATEADD(day,-5,GETDATE())),
(3, N'NCC Ba Huan',    8500000,  3, 'PENDING',  GETDATE(), NULL, NULL);
SET IDENTITY_INSERT [PurchaseOrders] OFF;
GO

-- ── ACCOUNTING PERIODS ──────────────────────────────────────
SET IDENTITY_INSERT [AccountingPeriods] ON;
INSERT INTO [AccountingPeriods] ([Id],[Year],[Month],[IsClosed],[ClosedAt],[ClosedBy]) VALUES
(1, YEAR(DATEADD(month,-5,GETDATE())), MONTH(DATEADD(month,-5,GETDATE())), 1, GETDATE(), 1),
(2, YEAR(DATEADD(month,-4,GETDATE())), MONTH(DATEADD(month,-4,GETDATE())), 1, GETDATE(), 1),
(3, YEAR(DATEADD(month,-3,GETDATE())), MONTH(DATEADD(month,-3,GETDATE())), 1, GETDATE(), 1),
(4, YEAR(DATEADD(month,-2,GETDATE())), MONTH(DATEADD(month,-2,GETDATE())), 1, GETDATE(), 1),
(5, YEAR(DATEADD(month,-1,GETDATE())), MONTH(DATEADD(month,-1,GETDATE())), 1, GETDATE(), 1),
(6, YEAR(GETDATE()),                   MONTH(GETDATE()),                   0, NULL, NULL);
SET IDENTITY_INSERT [AccountingPeriods] OFF;
GO

-- ── POS SESSIONS ────────────────────────────────────────────
SET IDENTITY_INSERT [PosSessions] ON;
INSERT INTO [PosSessions] ([Id],[CashierId],[OpeningFloat],[ClosingCash],[OpenedAt],[ClosedAt],[Status]) VALUES
(1, 4, 500000, 1850000, DATEADD(hour,-8,GETDATE()), DATEADD(hour,-1,GETDATE()), 'APPROVED'),
(2, 4, 500000, NULL,    GETDATE(), NULL, 'PENDING');
SET IDENTITY_INSERT [PosSessions] OFF;
GO

-- ── POS TRANSACTIONS ────────────────────────────────────────
SET IDENTITY_INSERT [PosTransactions] ON;
INSERT INTO [PosTransactions] ([Id],[SessionId],[CashierId],[PaymentMethod],[TotalAmount],[VatAmount],[CostAmount],[DiscountAmount],[CreatedAt]) VALUES
(1, 1, 4, 'CASH',  300000, 27273, 195000, 0, DATEADD(hour,-7,GETDATE())),
(2, 1, 4, 'CASH',  524000, 47636, 270000, 0, DATEADD(hour,-6,GETDATE())),
(3, 1, 4, 'CARD',  200000, 18182, 130000, 0, DATEADD(hour,-5,GETDATE())),
(4, 1, 4, 'CASH',  62000,  5636,  45000,  0, DATEADD(hour,-4,GETDATE()));
SET IDENTITY_INSERT [PosTransactions] OFF;
GO

SET IDENTITY_INSERT [PosTransactionLines] ON;
INSERT INTO [PosTransactionLines] ([Id],[TransactionId],[ProductId],[Quantity],[UnitPrice],[DiscountAmount]) VALUES
(1, 1, 1, 1.500, 200000, 0),
(2, 2, 2, 2.000, 62000,  0),
(3, 2, 1, 2.000, 200000, 0),
(4, 3, 4, 10.000, 7000,  0),
(5, 3, 5, 6.000, 22000,  0),
(6, 4, 3, 1.000, 62000,  0);
SET IDENTITY_INSERT [PosTransactionLines] OFF;
GO

-- ── JOURNAL ENTRIES (But toan ke toan) ──────────────────────
SET IDENTITY_INSERT [JournalEntries] ON;
INSERT INTO [JournalEntries] ([Id],[CreatedAt],[Source],[Description],[CreatedBy]) VALUES
(1, DATEADD(day,-9,GETDATE()),  'PO_RECEIPT', N'Nhap kho NCC Anh Tuan (Receipt 1)',  1),
(2, DATEADD(day,-4,GETDATE()),  'PO_RECEIPT', N'Nhap kho NCC Minh Thanh (Receipt 2)',2),
(3, DATEADD(hour,-7,GETDATE()), 'POS_SALE',   N'Ban hang POS #1',                    NULL),
(4, DATEADD(hour,-7,GETDATE()), 'POS_COGS',   N'Gia von POS #1',                     NULL),
(5, DATEADD(hour,-6,GETDATE()), 'POS_SALE',   N'Ban hang POS #2',                    NULL);
SET IDENTITY_INSERT [JournalEntries] OFF;
GO

SET IDENTITY_INSERT [JournalLines] ON;
INSERT INTO [JournalLines] ([Id],[JournalEntryId],[AccountCode],[AccountName],[Debit],[Credit]) VALUES
-- Receipt 1: No 156 / Co 331
(1,  1, '156',  N'Hang hoa',       5000000, 0),
(2,  1, '331',  N'Phai tra NCC',   0,       5000000),
-- Receipt 2: No 156 / Co 331
(3,  2, '156',  N'Hang hoa',       1140000, 0),
(4,  2, '331',  N'Phai tra NCC',   0,       1140000),
-- POS Sale 1: No 111 / Co 511+3331
(5,  3, '111',  N'Tien mat',       300000,  0),
(6,  3, '511',  N'Doanh thu',      0,       272727),
(7,  3, '3331', N'Thue GTGT',      0,       27273),
-- POS COGS 1: No 632 / Co 156
(8,  4, '632',  N'Gia von hang ban',195000, 0),
(9,  4, '156',  N'Hang hoa',       0,       195000),
-- POS Sale 2
(10, 5, '111',  N'Tien mat',       524000,  0),
(11, 5, '511',  N'Doanh thu',      0,       476364),
(12, 5, '3331', N'Thue GTGT',      0,       47636);
SET IDENTITY_INSERT [JournalLines] OFF;
GO

-- ── ATTENDANCE RECORDS ──────────────────────────────────────
SET IDENTITY_INSERT [AttendanceRecords] ON;
INSERT INTO [AttendanceRecords] ([Id],[EmployeeId],[WorkDate],[CheckInAt],[CheckOutAt],[WorkedHours],[LateMinutes],[EarlyLeaveMinutes],[HasApprovedOt],[OtHours],[Status]) VALUES
(1, 1, CAST(DATEADD(day,-1,GETDATE()) AS date), DATEADD(day,-1,DATEADD(hour,8,CAST(CAST(GETDATE() AS date) AS datetime2))), DATEADD(day,-1,DATEADD(hour,17,CAST(CAST(GETDATE() AS date) AS datetime2))), 8.00, 0, 0, 0, 0, 'CHECKED_OUT'),
(2, 2, CAST(DATEADD(day,-1,GETDATE()) AS date), DATEADD(day,-1,DATEADD(hour,8,CAST(CAST(GETDATE() AS date) AS datetime2))), DATEADD(day,-1,DATEADD(hour,18,CAST(CAST(GETDATE() AS date) AS datetime2))), 9.00, 0, 0, 1, 1.00, 'CHECKED_OUT'),
(3, 4, CAST(DATEADD(day,-1,GETDATE()) AS date), DATEADD(day,-1,DATEADD(hour,7,CAST(CAST(GETDATE() AS date) AS datetime2))), DATEADD(day,-1,DATEADD(hour,16,CAST(CAST(GETDATE() AS date) AS datetime2))), 8.00, 0, 0, 0, 0, 'CHECKED_OUT'),
(4, 3, CAST(GETDATE() AS date), DATEADD(hour,8,CAST(CAST(GETDATE() AS date) AS datetime2)), NULL, 0, 0, 0, 0, 0, 'CHECKED_IN');
SET IDENTITY_INSERT [AttendanceRecords] OFF;
GO

-- ── LEAVE REQUESTS ──────────────────────────────────────────
SET IDENTITY_INSERT [LeaveRequests] ON;
INSERT INTO [LeaveRequests] ([Id],[EmployeeId],[LeaveType],[FromDate],[ToDate],[Days],[Reason],[Status],[CreatedBy],[CreatedAt]) VALUES
(1, 3, 'ANNUAL', CAST(DATEADD(day,7,GETDATE()) AS date), CAST(DATEADD(day,9,GETDATE()) AS date), 3.0, N'Nghi phep nam ve que', 'PENDING', 3, GETDATE());
SET IDENTITY_INSERT [LeaveRequests] OFF;
GO

-- ── PROMOTIONS ──────────────────────────────────────────────
SET IDENTITY_INSERT [Promotions] ON;
INSERT INTO [Promotions] ([Id],[Name],[ProductId],[CategoryCode],[DiscountType],[DiscountValue],[FromDate],[ToDate],[IsActive],[CreatedAt],[CreatedBy]) VALUES
(1, N'Giam gia Heineken mua he',  5, NULL,  'PERCENT', 0.1000, CAST(GETDATE() AS date), CAST(DATEADD(day,30,GETDATE()) AS date), 1, GETDATE(), 1),
(2, N'Khuyen mai do tuoi',        NULL, 'THIT', 'PERCENT', 0.0500, CAST(GETDATE() AS date), CAST(DATEADD(day,7,GETDATE()) AS date),  1, GETDATE(), 1);
SET IDENTITY_INSERT [Promotions] OFF;
GO

-- ── SALARY HISTORIES ────────────────────────────────────────
SET IDENTITY_INSERT [SalaryHistories] ON;
INSERT INTO [SalaryHistories] ([Id],[EmployeeId],[OldBaseSalary],[NewBaseSalary],[EffectiveDate],[Reason],[CreatedAt],[CreatedBy]) VALUES
(1, 2, 10000000, 12000000, '2025-03-01', N'Tang luong dinh ky nam 2025', GETDATE(), 1),
(2, 4, 6000000,  7000000,  '2026-01-15', N'Het thu viec, tang luong chinh thuc', GETDATE(), 1);
SET IDENTITY_INSERT [SalaryHistories] OFF;
GO

COMMIT;
GO

PRINT N'=== SEED DATA IMPORTED SUCCESSFULLY ==='
PRINT N'Accounts: owner / manager / warehouse / cashier / accountant'
PRINT N'Password: 123456'
GO
