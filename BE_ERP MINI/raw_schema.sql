IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE TABLE [AccountingPeriods] (
        [Id] int NOT NULL IDENTITY,
        [Year] int NOT NULL,
        [Month] int NOT NULL,
        [IsClosed] bit NOT NULL,
        [ClosedAt] datetime2 NULL,
        [ClosedBy] int NULL,
        CONSTRAINT [PK_AccountingPeriods] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE TABLE [JournalEntries] (
        [Id] int NOT NULL IDENTITY,
        [CreatedAt] datetime2 NOT NULL,
        [Source] nvarchar(30) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [CreatedBy] int NULL,
        [ReversalOfId] int NULL,
        CONSTRAINT [PK_JournalEntries] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE TABLE [StocktakeSessions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [CreatedBy] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_StocktakeSessions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE TABLE [SystemConfigs] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(1000) NOT NULL,
        [Description] nvarchar(500) NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [UpdatedBy] int NOT NULL,
        CONSTRAINT [PK_SystemConfigs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AccountingPeriods_Year_Month] ON [AccountingPeriods] ([Year], [Month]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_APInvoices_DueDate] ON [APInvoices] ([DueDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_APInvoices_IsPaid] ON [APInvoices] ([IsPaid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_APInvoices_SupplierName] ON [APInvoices] ([SupplierName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_APPayments_APInvoiceId] ON [APPayments] ([APInvoiceId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AttendanceRecords_EmployeeId_WorkDate] ON [AttendanceRecords] ([EmployeeId], [WorkDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Contracts_EmployeeId] ON [Contracts] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_EmployeeCode] ON [Employees] ([EmployeeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Employees_IsDeleted] ON [Employees] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Employees_Status] ON [Employees] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_CreatedAt] ON [JournalEntries] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_JournalEntries_Source] ON [JournalEntries] ([Source]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_JournalLines_AccountCode] ON [JournalLines] ([AccountCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_JournalLines_JournalEntryId] ON [JournalLines] ([JournalEntryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_EmployeeId] ON [LeaveRequests] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_Status] ON [LeaveRequests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Lots_ExpiryDate] ON [Lots] ([ExpiryDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Lots_ProductId] ON [Lots] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Lots_ReceiptId] ON [Lots] ([ReceiptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PayrollAdjustments_PayrollId] ON [PayrollAdjustments] ([PayrollId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PayrollRecords_EmployeeId_Year_Month] ON [PayrollRecords] ([EmployeeId], [Year], [Month]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PayrollRecords_Status] ON [PayrollRecords] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosRefunds_TransactionId] ON [PosRefunds] ([TransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosSessions_CashierId] ON [PosSessions] ([CashierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosSessions_Status] ON [PosSessions] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosTransactionLines_ProductId] ON [PosTransactionLines] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosTransactionLines_TransactionId] ON [PosTransactionLines] ([TransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosTransactions_CashierId] ON [PosTransactions] ([CashierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosTransactions_CreatedAt] ON [PosTransactions] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PosTransactions_SessionId] ON [PosTransactions] ([SessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Products_Barcode] ON [Products] ([Barcode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Products_CategoryCode] ON [Products] ([CategoryCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Products_IsDeleted] ON [Products] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Products_Sku] ON [Products] ([Sku]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Promotions_FromDate_ToDate] ON [Promotions] ([FromDate], [ToDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Promotions_IsActive] ON [Promotions] ([IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Promotions_ProductId] ON [Promotions] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrders_Status] ON [PurchaseOrders] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_PurchaseRequests_ProductId] ON [PurchaseRequests] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_SalaryHistories_EmployeeId] ON [SalaryHistories] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_ShrinkageRecords_ProductId] ON [ShrinkageRecords] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SystemConfigs_Key] ON [SystemConfigs] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_UserActionLogs_CreatedAt] ON [UserActionLogs] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_UserActionLogs_EntityType_EntityId] ON [UserActionLogs] ([EntityType], [EntityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_UserActionLogs_UserId] ON [UserActionLogs] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_Users_IsDeleted] ON [Users] ([IsDeleted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_WarehouseIssues_CreatedAt] ON [WarehouseIssues] ([CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_WarehouseIssues_ProductId] ON [WarehouseIssues] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_WarehouseReceiptLines_ProductId] ON [WarehouseReceiptLines] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_WarehouseReceiptLines_ReceiptId] ON [WarehouseReceiptLines] ([ReceiptId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    CREATE INDEX [IX_WarehouseReceipts_Status] ON [WarehouseReceipts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612183800_InitialIntSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260612183800_InitialIntSchema', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612194312_AddPasswordHash'
)
BEGIN
    ALTER TABLE [Users] ADD [PasswordHash] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260612194312_AddPasswordHash'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260612194312_AddPasswordHash', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260613064847_AddPasswordHashV2'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(255) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260613064847_AddPasswordHashV2'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260613064847_AddPasswordHashV2', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619155848_AddParentAuditId'
)
BEGIN
    ALTER TABLE [UserActionLogs] ADD [ParentAuditId] bigint NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260619155848_AddParentAuditId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260619155848_AddParentAuditId', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622004227_AddProductImageUrl'
)
BEGIN
    ALTER TABLE [Products] ADD [ImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260622004227_AddProductImageUrl'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260622004227_AddProductImageUrl', N'8.0.6');
END;
GO

COMMIT;
GO

