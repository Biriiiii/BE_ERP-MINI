# 🏪 BE ERP MINI — Backend API Siêu Thị

> **Backend ERP hệ thống quản lý siêu thị nhỏ** — xây dựng bằng ASP.NET Core 8 + SQL Server + Entity Framework Core.  
> Bao gồm đầy đủ: Nhân sự · Kho hàng · Bán hàng (POS) · Kế toán · Mua hàng · Audit Trail

---

## 📋 Mục lục

- [Kiến trúc hệ thống](#kiến-trúc-hệ-thống)
- [Tech Stack](#tech-stack)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Cài đặt & Chạy](#cài-đặt--chạy)
- [Cơ sở dữ liệu](#cơ-sở-dữ-liệu)
- [Xác thực (JWT)](#xác-thực-jwt)
- [API Endpoints](#api-endpoints)
- [Phân quyền (RBAC)](#phân-quyền-rbac)
- [Seed Data mặc định](#seed-data-mặc-định)
- [Business Rules](#business-rules)

---

## Kiến trúc hệ thống

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT / SWAGGER                       │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTP + JWT Bearer
┌───────────────────────────▼─────────────────────────────────┐
│              ASP.NET Core 8 Web API                         │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────┐   │
│  │ Controllers │  │   Services   │  │  ErpContext(RBAC) │   │
│  └──────┬──────┘  └──────┬───────┘  └──────────────────┘   │
│         └────────────────┘                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              AppDbContext (EF Core)                  │   │
│  └──────────────────────────┬───────────────────────────┘   │
└─────────────────────────────┼───────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────┐
│               SQL Server — Database ERPMINI                  │
│   27 bảng: HR · Inventory · POS · Accounting · Audit        │
└─────────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Thành phần | Công nghệ |
|---|---|
| **Framework** | ASP.NET Core 8.0 (Web API) |
| **ORM** | Entity Framework Core 8.0 |
| **Database** | SQL Server (LocalDB / Express / Full) |
| **Auth** | JWT Bearer (HS256) |
| **Password** | BCrypt.Net-Next |
| **Docs** | Swagger / OpenAPI (Swashbuckle) |
| **Migration** | EF Core Migrations |

---

## Cấu trúc thư mục

```
BE_ERP MINI/
├── Controllers/            # API Endpoints
│   ├── AuthController.cs       # Đăng nhập, xem thông tin user
│   ├── HrController.cs         # Nhân sự, chấm công, lương
│   ├── InventoryController.cs  # Kho hàng, sản phẩm, phiếu nhập
│   ├── PosController.cs        # Bán hàng, ca thu ngân
│   ├── AccountingController.cs # Kế toán, bút toán, công nợ
│   ├── PurchaseController.cs   # Yêu cầu & đơn mua hàng
│   ├── UsersController.cs      # Quản lý tài khoản
│   └── UserActionLogsController.cs # Audit trail
│
├── Domain/                 # Entities & Enums
│   ├── Entities.cs             # 25+ entity classes
│   ├── AppUser.cs              # Người dùng hệ thống
│   ├── UserActionLog.cs        # Log thao tác
│   └── Enums.cs                # Role, Status, PaymentMethod...
│
├── Data/                   # Database Layer
│   ├── AppDbContext.cs          # EF Core context + Fluent API
│   ├── AppDatabaseInitializer.cs # Seed data
│   └── Migrations/              # EF Migrations
│
├── Services/               # Business Logic
│   ├── AuthService.cs           # JWT token generation
│   ├── ErpContext.cs            # Đọc claims từ JWT
│   ├── InventoryService.cs      # FEFO, tính giá vốn bình quân
│   ├── AccountingService.cs     # Bút toán, kiểm tra công nợ
│   ├── PayrollService.cs        # Tính lương (TC-HR-001)
│   ├── UserActionLogService.cs  # Ghi audit log
│   └── AuditExportService.cs    # Export CSV audit
│
├── Contracts/
│   └── Requests.cs              # Request DTOs
│
├── appsettings.json        # Cấu hình DB, JWT
├── Program.cs              # DI, Middleware, Pipeline
└── test_all.http           # File test API (REST Client)
```

---

## Cài đặt & Chạy

### Yêu cầu
- **.NET 8 SDK** — [tải tại dotnet.microsoft.com](https://dotnet.microsoft.com/download)
- **SQL Server** (Express / LocalDB / Developer Edition)
- **SQL Server Management Studio** (SSMS) — tuỳ chọn

### Bước 1: Clone & cấu hình

```bash
git clone <repo-url>
cd "BE_ERP MINI"
```

Mở `appsettings.json`, sửa connection string theo môi trường:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=ERPMINI;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "BE_ERP_MINI_SUPER_SECRET_KEY_1234567890",
    "Issuer": "ErpMiniIssuer",
    "Audience": "ErpMiniAudience",
    "ExpiryMinutes": 1440
  }
}
```

### Bước 2: Tạo database & chạy migrations

```bash
# Cài EF Tools (một lần duy nhất)
dotnet tool install --global dotnet-ef

# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialSchema --output-dir Data/Migrations

# Tạo database + bảng
dotnet ef database update
```

### Bước 3: Chạy ứng dụng

```bash
dotnet run
```

App sẽ tự động:
1. Chạy migrations còn pending
2. Seed dữ liệu mẫu (nếu DB trống)
3. Khởi động tại `https://localhost:7039`

**Swagger UI:** `https://localhost:7039/swagger`

---

## Cơ sở dữ liệu

### Sơ đồ bảng

```
📦 ERPMINI (27 bảng)
│
├── 🔐 SYSTEM
│   ├── Users               # Tài khoản đăng nhập
│   └── SystemConfigs       # Cấu hình hệ thống (key-value)
│
├── 🛡️ AUDIT
│   └── UserActionLogs      # Lịch sử mọi thao tác CREATE/UPDATE/DELETE
│
├── 👥 HR
│   ├── Employees           # Hồ sơ nhân viên
│   ├── Contracts           # Hợp đồng lao động
│   ├── SalaryHistories     # Lịch sử thay đổi lương
│   ├── LeaveRequests       # Đơn nghỉ phép
│   ├── AttendanceRecords   # Chấm công check-in/out
│   ├── PayrollRecords      # Bảng lương tháng
│   └── PayrollAdjustments  # Thưởng/phạt điều chỉnh
│
├── 📦 INVENTORY
│   ├── Products            # Danh mục hàng hoá (SKU)
│   ├── Lots                # Lô hàng (quản lý HSD — FEFO)
│   ├── WarehouseReceipts   # Phiếu nhập kho
│   ├── WarehouseReceiptLines
│   ├── WarehouseIssues     # Phiếu xuất kho
│   ├── ShrinkageRecords    # Ghi nhận hao hụt
│   ├── StocktakeSessions   # Phiên kiểm kho
│   ├── PurchaseRequests    # Yêu cầu mua hàng
│   └── PurchaseOrders      # Đơn đặt hàng
│
├── 🛒 POS
│   ├── PosSessions         # Ca thu ngân
│   ├── PosTransactions     # Hoá đơn bán hàng
│   ├── PosTransactionLines # Chi tiết từng mặt hàng
│   ├── PosRefunds          # Hoàn trả hàng
│   └── Promotions          # Chương trình khuyến mãi
│
└── 💰 ACCOUNTING
    ├── JournalEntries      # Bút toán kế toán
    ├── JournalLines        # Dòng nợ/có
    ├── AccountingPeriods   # Kỳ kế toán (tháng)
    ├── APInvoices          # Hoá đơn công nợ phải trả
    └── APPayments          # Thanh toán công nợ
```

### Reset database

```bash
dotnet ef database drop --force
dotnet ef database update
# Khởi động app → seed data tự chạy
```

---

## Xác thực (JWT)

### Đăng nhập

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "owner",
  "password": "123456"
}
```

**Response:**
```json
{
  "token": "eyJhbGci...",
  "fullName": "Chu Sieu Thi",
  "role": "OWNER"
}
```

### Sử dụng token

Thêm header vào mọi request:
```
Authorization: Bearer eyJhbGci...
```

> **Swagger:** Click 🔒 **Authorize** → dán token (không cần "Bearer ")

### Xem thông tin user hiện tại

```http
GET /api/auth/me
Authorization: Bearer {token}
```

---

## API Endpoints

### 🔑 Auth

| Method | Route | Mô tả | Auth |
|---|---|---|---|
| POST | `/api/auth/login` | Đăng nhập, lấy JWT | ❌ |
| GET | `/api/auth/me` | Xem thông tin user | ✅ |

---

### 👥 HR — Nhân sự

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/hr/employees` | Danh sách nhân viên | OWNER, MGR, WAREHOUSE |
| GET | `/api/hr/employees/{id}` | Chi tiết nhân viên | OWNER, MGR |
| POST | `/api/hr/employees` | Tạo nhân viên | OWNER, MGR |
| PATCH | `/api/hr/employees/{id}` | Cập nhật nhân viên | OWNER, MGR |
| DELETE | `/api/hr/employees/{id}?reason=...` | Xoá mềm | OWNER, MGR |
| POST | `/api/hr/attendance/checkin` | Check-in | Tất cả |
| POST | `/api/hr/attendance/checkout` | Check-out | Tất cả |
| POST | `/api/hr/leave-requests` | Tạo đơn nghỉ phép | Tất cả |
| PATCH | `/api/hr/leave-requests/{id}/approve` | Duyệt đơn | OWNER, MGR |
| POST | `/api/hr/payroll/generate` | Tạo bảng lương | OWNER, MGR |
| PATCH | `/api/hr/payroll/{id}/approve` | Duyệt bảng lương | OWNER, ACCOUNTANT |
| GET | `/api/hr/payroll/me` | Bảng lương của tôi | ✅ |
| GET | `/api/hr/payroll/list` | Toàn bộ bảng lương | OWNER, ACCOUNTANT, MGR |

**Request tạo nhân viên:**
```json
{
  "fullName": "Nguyen Van A",
  "department": "Sale",
  "position": "Staff",
  "role": "EMPLOYEE",
  "baseSalary": 8000000,
  "mealAllowance": 500000,
  "attendanceAllowance": 300000
}
```

---

### 📦 Inventory — Kho hàng

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/inventory/products` | Danh sách sản phẩm | ✅ |
| GET | `/api/inventory/products?keyword=gao` | Tìm kiếm | ✅ |
| GET | `/api/inventory/products?category=FMCG` | Lọc theo loại | ✅ |
| POST | `/api/inventory/products` | Tạo sản phẩm | OWNER, MGR, WAREHOUSE |
| PATCH | `/api/inventory/products/{id}` | Cập nhật | OWNER, MGR, WAREHOUSE |
| DELETE | `/api/inventory/products/{id}?reason=...` | Xoá mềm | OWNER, MGR |
| GET | `/api/inventory/stock-balance` | Tồn kho hiện tại | ✅ |
| GET | `/api/inventory/expiry-alerts?days=2` | Cảnh báo HSD | ✅ |
| POST | `/api/inventory/receipts` | Tạo phiếu nhập | OWNER, WAREHOUSE |
| PATCH | `/api/inventory/receipts/{id}/approve` | Duyệt phiếu nhập | OWNER, MGR |
| POST | `/api/inventory/issues` | Xuất kho | ✅ |
| POST | `/api/inventory/shrinkage` | Ghi hao hụt | OWNER, MGR, WAREHOUSE |
| POST | `/api/inventory/stocktake` | Tạo phiên kiểm kho | OWNER, MGR |

---

### 🛒 POS — Bán hàng

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/pos/products/lookup?barcode=...` | Tra cứu theo barcode | ✅ |
| GET | `/api/pos/promotions/active` | Khuyến mãi đang chạy | ✅ |
| POST | `/api/pos/session/open` | Mở ca thu ngân | OWNER, CASHIER |
| POST | `/api/pos/session/close` | Đóng ca | OWNER, CASHIER |
| POST | `/api/pos/transactions` | Tạo hoá đơn bán hàng | OWNER, CASHIER |

**Request tạo hoá đơn:**
```json
{
  "sessionId": 1,
  "cashierId": 4,
  "paymentMethod": "CASH",
  "lines": [
    { "productId": 1, "quantity": 1.5 },
    { "productId": 2, "quantity": 2 }
  ]
}
```

---

### 🛍️ Purchasing — Mua hàng

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/purchasing/requests` | Danh sách yêu cầu | OWNER, MGR, WAREHOUSE |
| POST | `/api/purchasing/requests` | Tạo yêu cầu | ✅ |
| GET | `/api/purchasing/orders` | Danh sách đơn mua | OWNER, MGR, ACCOUNTANT |
| POST | `/api/purchasing/orders` | Tạo đơn mua | OWNER, MGR |
| PATCH | `/api/purchasing/orders/{id}/approve` | Duyệt đơn | OWNER |

> **Lưu ý:** PO > 10.000.000đ phải được OWNER duyệt. Không thể tạo PO cho NCC đang có công nợ quá hạn.

---

### 💰 Accounting — Kế toán

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/accounting/journal` | Nhật ký kế toán | OWNER, ACCOUNTANT |
| POST | `/api/accounting/journal` | Tạo bút toán thủ công | OWNER, ACCOUNTANT |
| POST | `/api/accounting/journal/{id}/reverse?reason=...` | Đảo ngược bút toán | OWNER, ACCOUNTANT |
| GET | `/api/accounting/ap-invoices` | Danh sách công nợ | OWNER, ACCOUNTANT |
| POST | `/api/accounting/ap-invoices/{id}/pay` | Thanh toán công nợ | OWNER, ACCOUNTANT |
| GET | `/api/accounting/periods` | Danh sách kỳ kế toán | OWNER, ACCOUNTANT |
| POST | `/api/accounting/periods/{id}/close` | Đóng kỳ kế toán | OWNER, ACCOUNTANT |

---

### 🛡️ Audit — Lịch sử thao tác

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/audit/entity/{type}/{id}` | Lịch sử 1 bản ghi | OWNER, ACCOUNTANT, MGR |
| GET | `/api/audit/user/{userId}` | Thao tác của 1 user | OWNER, ACCOUNTANT |
| GET | `/api/audit/feed` | Feed thay đổi real-time | OWNER, ACCOUNTANT |
| GET | `/api/audit/export` | Export CSV | OWNER, ACCOUNTANT |

---

### 👤 Users — Tài khoản

| Method | Route | Mô tả | Quyền |
|---|---|---|---|
| GET | `/api/users` | Danh sách users | OWNER, MGR, ACCOUNTANT |
| GET | `/api/users/{id}` | Chi tiết user | OWNER, MGR, ACCOUNTANT |
| POST | `/api/users` | Tạo user | OWNER, MGR |
| PATCH | `/api/users/{id}` | Cập nhật user | OWNER, MGR |
| DELETE | `/api/users/{id}?reason=...` | Xoá mềm | OWNER |

---

## Phân quyền (RBAC)

| Role | Mô tả | Quyền chính |
|---|---|---|
| `OWNER` | Chủ siêu thị | Toàn quyền |
| `STORE_MANAGER` | Quản lý cửa hàng | HR, Kho, POS (không kế toán) |
| `ACCOUNTANT` | Kế toán | Kế toán, Audit, Lương |
| `WAREHOUSE_STAFF` | Thủ kho | Xem/tạo phiếu nhập xuất |
| `CASHIER` | Thu ngân | POS, ca thu ngân |
| `SALES_STAFF` | Nhân viên bán | Xem sản phẩm, tra barcode |
| `EMPLOYEE` | Nhân viên thường | Xem thông tin cá nhân |

> Gửi qua HTTP Header `X-Role` (fallback khi không dùng JWT) hoặc JWT Claim `role`.

---

## Seed Data mặc định

Khi DB trống, app tự seed:

### Tài khoản (mật khẩu: `123456`)

| ID | Username | Role |
|---|---|---|
| 1 | `owner` | OWNER |
| 2 | `manager` | STORE_MANAGER |
| 3 | `warehouse` | WAREHOUSE_STAFF |
| 4 | `cashier` | CASHIER |
| 5 | `accountant` | ACCOUNTANT |

### Sản phẩm mẫu

| ID | SKU | Tên | Giá bán |
|---|---|---|---|
| 1 | THIT-0001 | Thịt Heo Thái Lát | 200.000đ/kg |
| 2 | FMCG-0001 | Gạo ST25 | 62.000đ/kg |
| 3 | FMCG-0002 | Dầu Ăn Neptune 1L | 62.000đ/chai |

---

## Business Rules

### Inventory
- **FEFO** (First Expired First Out): Lô hàng gần hết hạn xuất trước
- **Block bán hàng hết HSD**: POS từ chối sản phẩm đã hết hạn
- **Cảnh báo tồn kho thấp**: Alert khi `Quantity < MinStockLevel`
- **Giá vốn bình quân**: Cập nhật tự động khi duyệt phiếu nhập

### Payroll (TC-HR-001)
```
Gross = Lương CB + Phụ cấp cơm + Phụ cấp chuyên cần
      + OT Pay - Trừ nghỉ không phép - Trừ đi trễ
BHXH = Gross × 10.5%
TNCN = (Gross - BHXH - 11.000.000) × thuế suất
Net  = Gross - BHXH - TNCN + Thưởng - Phạt
```

### Accounting
- **Double-entry**: Mọi bút toán phải có Debit = Credit
- **Auto journal**: POS bán hàng, nhập kho, hao hụt, lương → tự tạo bút toán
- **Đảo ngược**: Bút toán sai → tạo reversal, không sửa trực tiếp
- **Khóa kỳ**: Kỳ đã đóng không thể tạo thêm bút toán

### Purchase Order
- PO > 10.000.000đ → cần OWNER duyệt
- NCC có công nợ quá hạn → block tạo PO

---

## Commands thường dùng

```bash
# Chạy app
dotnet run

# Thêm migration mới
dotnet ef migrations add <TenMigration> --output-dir Data/Migrations

# Apply lên DB
dotnet ef database update

# Xoá & tạo lại DB
dotnet ef database drop --force && dotnet ef database update

# Build kiểm tra lỗi
dotnet build
```

---

## Liên hệ & Đóng góp

Dự án này được xây dựng theo tài liệu nghiệp vụ **ERP_Sieu_Thi_v2_AuditTrail.docx**.  
Mọi thay đổi nghiệp vụ cần cập nhật tài liệu song song.
