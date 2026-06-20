using BE_ERP_MINI.Contracts;
using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using BE_ERP_MINI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Controllers;

[ApiController]
[Route("api/hr")]
public sealed class HrController(
    AppDbContext db,
    ErpContext context,
    PayrollService payrollService,
    UserActionLogService actionLog) : ControllerBase
{
    // ─── Nhân viên ─────────────────────────────────────────────────────────────

    [HttpGet("employees")]
    public async Task<IActionResult> Employees()
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT, Role.STORE_MANAGER, Role.WAREHOUSE_STAFF);
        return Ok(await db.Employees.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.EmployeeCode).ToListAsync());
    }

    [HttpGet("employees/{id:int}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT, Role.STORE_MANAGER);
        var emp = await db.Employees.AsNoTracking()
            .Include(x => x.Contracts)
            .Include(x => x.SalaryHistories)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        return emp is null ? NotFound("Khong tim thay nhan vien.") : Ok(emp);
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var year   = DateTime.Today.Year;
        var prefix = $"NV-{year}-";
        // Đếm theo prefix code, không phải HireDate — tránh trùng với seed data
        var seq = await db.Employees.CountAsync(x => x.EmployeeCode.StartsWith(prefix)) + 1;
        var employee = new Employee
        {
            EmployeeCode        = $"NV-{year}-{seq:000}",
            FullName            = request.FullName,
            Department          = request.Department,
            Position            = request.Position,
            Role                = request.Role,
            BaseSalary          = request.BaseSalary,
            MealAllowance       = request.MealAllowance,
            AttendanceAllowance = request.AttendanceAllowance,
            CreatedBy           = context.Current(Request).UserId ?? 0
        };

        await using var tx = await db.Database.BeginTransactionAsync();
        db.Employees.Add(employee);
        await db.SaveChangesAsync();  // cần SaveChanges trước để có employee.Id
        await actionLog.AddLogAsync(Request, "CREATE", "Employee", employee.Id.ToString(),
            $"Tao nhan vien {employee.EmployeeCode}", null, employee, employee.FullName);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPatch("employees/{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (employee is null) return NotFound("Khong tim thay nhan vien.");
        if (request.Version.HasValue && request.Version.Value != employee.Version)
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac. Vui long tai lai va thu lai.");

        var oldValue  = new { employee.FullName, employee.Department, employee.Position, employee.Role, employee.BaseSalary, employee.MealAllowance, employee.AttendanceAllowance, employee.Status };
        var oldSalary = employee.BaseSalary;

        employee.FullName            = request.FullName            ?? employee.FullName;
        employee.Department          = request.Department          ?? employee.Department;
        employee.Position            = request.Position            ?? employee.Position;
        employee.Role                = request.Role                ?? employee.Role;
        employee.BaseSalary          = request.BaseSalary          ?? employee.BaseSalary;
        employee.MealAllowance       = request.MealAllowance       ?? employee.MealAllowance;
        employee.AttendanceAllowance = request.AttendanceAllowance ?? employee.AttendanceAllowance;
        if (request.Status is { } s && s == EmployeeStatus.TERMINATED && employee.Status != EmployeeStatus.TERMINATED)
        { employee.Status = EmployeeStatus.TERMINATED; employee.TerminationDate = DateOnly.FromDateTime(DateTime.Today); }
        else if (request.Status.HasValue) employee.Status = request.Status.Value;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = context.Current(Request).UserId;
        employee.Version++;

        if (request.BaseSalary.HasValue && request.BaseSalary.Value != oldSalary)
        {
            db.SalaryHistories.Add(new SalaryHistory
            {
                EmployeeId    = employee.Id,
                OldBaseSalary = oldSalary,
                NewBaseSalary = employee.BaseSalary,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
                Reason        = request.Reason ?? "Cap nhat luong",
                CreatedBy     = context.Current(Request).UserId ?? 0
            });
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "UPDATE", "Employee", employee.Id.ToString(),
            $"Cap nhat nhan vien {employee.EmployeeCode}", oldValue, employee, employee.FullName, request.Reason);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(employee);
    }

    [HttpDelete("employees/{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id, [FromQuery] string reason, [FromQuery] int? version)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        if (string.IsNullOrWhiteSpace(reason)) return BadRequest("Xoa mem bat buoc nhap ly do.");
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        if (employee is null) return NotFound("Khong tim thay nhan vien.");
        if (version.HasValue && version.Value != employee.Version)
            return UnprocessableEntity("Du lieu da bi thay doi boi nguoi khac. Vui long tai lai va thu lai.");

        var oldValue = new { employee.EmployeeCode, employee.FullName, employee.Status };
        employee.IsDeleted       = true;
        employee.DeletedAt       = DateTime.UtcNow;
        employee.DeletedBy       = context.Current(Request).UserId;
        employee.Status          = EmployeeStatus.TERMINATED;
        employee.TerminationDate = DateOnly.FromDateTime(DateTime.Today);
        employee.Version++;

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "SOFT_DELETE", "Employee", id.ToString(),
            $"Xoa mem nhan vien {employee.EmployeeCode}", oldValue, null, employee.FullName, reason);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return NoContent();
    }

    [HttpGet("contracts/expiring")]
    public async Task<IActionResult> GetExpiringContracts()
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var in30Days = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
        var contracts = await db.Contracts
            .Include(x => x.Employee)
            .Where(x => x.Status == ContractStatus.ACTIVE && x.EndDate <= in30Days && !x.Employee.IsDeleted)
            .Select(x => new {
                x.Id, x.ContractType, x.StartDate, x.EndDate,
                EmployeeName = x.Employee.FullName,
                EmployeeCode = x.Employee.EmployeeCode
            })
            .ToListAsync();
        return Ok(contracts);
    }

    // ─── Chấm công ─────────────────────────────────────────────────────────────

    [HttpPost("attendance/checkin")]
    public async Task<IActionResult> CheckIn(AttendanceRequest request)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == request.EmployeeId);
        if (employee is null) return NotFound("Khong tim thay nhan vien.");
        if (employee.Status == EmployeeStatus.TERMINATED) return BadRequest("Nhan vien da nghi viec.");

        var workDate = DateOnly.FromDateTime(request.Timestamp);
        if (await db.AttendanceRecords.AnyAsync(x => x.EmployeeId == request.EmployeeId && x.WorkDate == workDate))
            return BadRequest("Nhan vien da check-in trong ngay nay.");

        var shiftStart  = request.Timestamp.Date.AddHours(8);
        var lateMinutes = request.Timestamp > shiftStart.AddMinutes(10) ? (int)(request.Timestamp - shiftStart).TotalMinutes : 0;
        var record = new AttendanceRecord
        {
            EmployeeId  = request.EmployeeId,
            WorkDate    = workDate,
            CheckInAt   = request.Timestamp,
            LateMinutes = lateMinutes,
            Status      = lateMinutes > 0 ? AttendanceStatus.LATE : AttendanceStatus.CHECKED_IN
        };
        db.AttendanceRecords.Add(record);
        await db.SaveChangesAsync();
        return Ok(record);
    }

    [HttpPost("attendance/checkout")]
    public async Task<IActionResult> CheckOut(AttendanceRequest request)
    {
        var workDate = DateOnly.FromDateTime(request.Timestamp);
        var record   = await db.AttendanceRecords.SingleOrDefaultAsync(x => x.EmployeeId == request.EmployeeId && x.WorkDate == workDate);
        if (record is null || record.CheckInAt is null) return BadRequest("Chua co check-in cho ngay nay.");
        if (record.CheckOutAt is not null) return BadRequest("Nhan vien da check-out.");

        var shiftEnd = request.Timestamp.Date.AddHours(17);
        record.CheckOutAt        = request.Timestamp;
        record.WorkedHours       = decimal.Round((decimal)(request.Timestamp - record.CheckInAt.Value).TotalHours, 2);
        record.EarlyLeaveMinutes = request.Timestamp < shiftEnd.AddMinutes(-15) ? (int)(shiftEnd - request.Timestamp).TotalMinutes : 0;
        record.Status            = record.EarlyLeaveMinutes > 0 ? AttendanceStatus.EARLY_LEAVE : AttendanceStatus.CHECKED_OUT;
        await db.SaveChangesAsync();
        return Ok(record);
    }

    [HttpPost("attendance/process-missing-checkouts")]
    public async Task<IActionResult> ProcessMissingCheckouts()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var missingRecords = await db.AttendanceRecords
            .Where(x => x.WorkDate < today && x.CheckOutAt == null && x.Status != AttendanceStatus.MISSING_CHECKOUT)
            .ToListAsync();
        
        foreach (var r in missingRecords) r.Status = AttendanceStatus.MISSING_CHECKOUT;
        await db.SaveChangesAsync();
        return Ok(new { ProcessedCount = missingRecords.Count });
    }

    [HttpPatch("attendance/{id:int}/approve-ot")]
    public async Task<IActionResult> ApproveOt(int id, ApproveOtRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var record = await db.AttendanceRecords.SingleOrDefaultAsync(x => x.Id == id);
        if (record is null) return NotFound("Khong tim thay ban ghi cham cong.");
        
        var old = new { record.HasApprovedOt, record.OtHours };
        record.HasApprovedOt = true;
        record.OtHours = request.OtHours;

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "APPROVE", "AttendanceRecord", record.Id.ToString(),
            $"Duyet {request.OtHours}h OT cho ban ghi cham cong ngay {record.WorkDate}",
            old, new { record.HasApprovedOt, record.OtHours });
            
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(record);
    }

    // ─── Nghỉ phép ─────────────────────────────────────────────────────────────

    [HttpPost("leave-requests")]
    public async Task<IActionResult> CreateLeave(CreateLeaveRequest request)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == request.EmployeeId && !x.IsDeleted);
        if (employee is null) return NotFound("Khong tim thay nhan vien.");
        var days = (decimal)(request.ToDate.DayNumber - request.FromDate.DayNumber + 1);
        if (request.LeaveType == "ANNUAL")
        {
            if (employee.AnnualLeaveBalance < days)
                return BadRequest($"So ngay phep khong du (con {employee.AnnualLeaveBalance}, yeu cau {days}).");
            employee.AnnualLeaveBalance -= days;
        }
        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId, CreatedBy = request.EmployeeId,
            LeaveType  = request.LeaveType,  FromDate   = request.FromDate,
            ToDate     = request.ToDate,     Days       = days,
            Reason     = request.Reason
        };
        db.LeaveRequests.Add(leave);
        await db.SaveChangesAsync();
        return Ok(leave);
    }

    [HttpPatch("leave-requests/{id:int}/approve")]
    public async Task<IActionResult> ApproveLeave(int id, ApproveRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        var leave = await db.LeaveRequests.SingleOrDefaultAsync(x => x.Id == id);
        if (leave is null) return NotFound();
        var currentUserRole = context.Current(Request).Role;
        if (leave.CreatedBy == request.ApproverId && currentUserRole != Role.OWNER) return Forbid("Ban khong the duyet phieu do chinh minh tao.");

        var old = leave.Status;
        leave.Status     = ApprovalStatus.APPROVED;
        leave.ApprovedBy = request.ApproverId;
        leave.ApprovedAt = DateTime.UtcNow;

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, "APPROVE", "LeaveRequest", leave.Id.ToString(),
            $"Duyet don nghi phep NV {leave.EmployeeId}",
            new { Status = old.ToString() }, new { Status = leave.Status.ToString() });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(leave);
    }

    // ─── Lương ─────────────────────────────────────────────────────────────────

    [HttpPost("payroll/generate")]
    public async Task<IActionResult> GeneratePayroll(GeneratePayrollRequest request)
    {
        context.Require(Request, Role.OWNER, Role.STORE_MANAGER);
        try { return Ok(await payrollService.GenerateAsync(request.EmployeeId, request.Year, request.Month, request.UnpaidLeaveDays, request.LateMinutes, request.ApprovedOtHours)); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
    }

    [HttpPatch("payroll/{id:int}/approve")]
    public async Task<IActionResult> ApprovePayroll(int id, ApproveRequest request)
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT);
        var payroll = await db.PayrollRecords.SingleOrDefaultAsync(x => x.Id == id);
        if (payroll is null) return NotFound("Khong tim thay bang luong.");
        if (payroll.Status == ApprovalStatus.LOCKED) return BadRequest("Bang luong da lock.");

        var current   = context.Current(Request);
        var oldStatus = payroll.Status;
        payroll.ApprovedBy = request.ApproverId;
        payroll.ApprovedAt = DateTime.UtcNow;
        payroll.Status     = current.Role == Role.OWNER ? ApprovalStatus.LOCKED : ApprovalStatus.APPROVED;

        if (payroll.Status == ApprovalStatus.LOCKED)
        {
            db.JournalEntries.Add(new JournalEntry
            {
                Source = JournalSource.PAYROLL, Description = $"Payroll {payroll.Month}/{payroll.Year}",
                CreatedBy = current.UserId,
                Lines =
                [
                    new JournalLine { AccountCode = "642", AccountName = "Chi phi quan ly", Debit = payroll.Gross },
                    new JournalLine { AccountCode = "334", AccountName = "Phai tra nguoi lao dong", Credit = payroll.Gross }
                ]
            });
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        await actionLog.AddLogAsync(Request, payroll.Status == ApprovalStatus.LOCKED ? "LOCK" : "APPROVE",
            "PayrollRecord", payroll.Id.ToString(), $"Duyet bang luong {payroll.Month}/{payroll.Year}",
            new { Status = oldStatus.ToString() }, new { Status = payroll.Status.ToString() });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(payroll);
    }

    [HttpGet("payroll/me")]
    public async Task<IActionResult> MyPayroll()
    {
        var current = context.Current(Request);
        if (current.UserId is null) return BadRequest("Thieu header X-User-Id.");
        return Ok(await db.PayrollRecords.AsNoTracking().Where(x => x.EmployeeId == current.UserId.Value).ToListAsync());
    }

    [HttpGet("payroll/list")]
    public async Task<IActionResult> PayrollList()
    {
        context.Require(Request, Role.OWNER, Role.ACCOUNTANT, Role.STORE_MANAGER);
        return Ok(await db.PayrollRecords.AsNoTracking().OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToListAsync());
    }
}
