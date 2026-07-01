-- ══════════════════════════════════════════════════════════════
-- ERP MINI — SEED DATA SCRIPT
-- Chay file nay SAU KHI da chay database_script.sql (schema)
-- Mat khau mac dinh cho tat ca user: 123456
-- ══════════════════════════════════════════════════════════════

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
INSERT INTO [Products] ([Id],[Sku],[Name],[CategoryCode],[Unit],[Barcode],[MinStockLevel],[AverageCost],[SalePrice],[IsFresh],[CreatedBy],[CreatedAt],[IsDeleted],[Version]) VALUES
(1,  'THIT-0001', N'Thit Heo Thai Lat',     'THIT',  'kg',   '8930000000011', 10,  130000,  200000, 1, 1, GETDATE(), 0, 1),
(2,  'FMCG-0001', N'Gao ST25',              'FMCG',  'kg',   '8930000000028', 50,  45000,   62000,  0, 1, GETDATE(), 0, 1),
(3,  'FMCG-0002', N'Dau An Neptune 1L',     'FMCG',  'chai', '8930000000035', 20,  45000,   62000,  0, 1, GETDATE(), 0, 1),
(4,  'NUOC-0001', N'Nuoc Suoi Aquafina 500ml','NUOC','chai', '8930000000042', 100, 3500,    7000,   0, 1, GETDATE(), 0, 1),
(5,  'BIA-0001',  N'Bia Heineken 330ml',    'BIA',   'lon',  '8934673123456', 24,  15000,   22000,  0, 1, GETDATE(), 0, 1),
(6,  'SUA-0001',  N'Sua Vinamilk 1L',       'SUA',   'hop',  '8930000000059', 30,  28000,   38000,  0, 1, GETDATE(), 0, 1),
(7,  'THIT-0002', N'Ga Nguyen Con',         'THIT',  'kg',   '8930000000066', 8,   85000,   130000, 1, 1, GETDATE(), 0, 1),
(8,  'FMCG-0003', N'Mi Hao Hao Tom Chua Cay','FMCG','goi',  '8930000000073', 200, 3000,    5000,   0, 1, GETDATE(), 0, 1),
(9,  'RAU-0001',  N'Rau Muong',             'RAU',   'bo',   '8930000000080', 15,  8000,    15000,  1, 1, GETDATE(), 0, 1),
(10, 'RAU-0002',  N'Ca Chua',               'RAU',   'kg',   '8930000000097', 10,  15000,   25000,  1, 1, GETDATE(), 0, 1);
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
