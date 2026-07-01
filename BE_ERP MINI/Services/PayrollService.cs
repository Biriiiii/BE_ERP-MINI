using BE_ERP_MINI.Data;
using BE_ERP_MINI.Domain;
using Microsoft.EntityFrameworkCore;

namespace BE_ERP_MINI.Services;

public sealed class PayrollService(AppDbContext db)
{
    private const decimal WorkingDays = 26m;

    /// <summary>
    /// Tính lương tháng theo công thức BRD mục 3.4.
    /// TC-HR-001: Gross = Lương CB + Phụ cấp + OT - Trừ nghỉ KP - Trừ đi trễ
    /// </summary>
    public async Task<PayrollRecord> GenerateAsync(
        int employeeId, int year, int month,
        decimal unpaidLeaveDays, decimal lateMinutes, decimal approvedOtHours)
    {
        var employee = await db.Employees.SingleOrDefaultAsync(x => x.Id == employeeId && !x.IsDeleted)
            ?? throw new InvalidOperationException("Khong tim thay nhan vien.");

        var daySalary          = employee.BaseSalary / WorkingDays;
        var hourSalary         = daySalary / 8m;
        var unpaidDeduction    = daySalary * unpaidLeaveDays;
        var lateDeduction      = hourSalary * (lateMinutes / 60m);
        var otPay              = hourSalary * 1.5m * approvedOtHours;
        var attendanceAllowance= unpaidLeaveDays > 0 ? 0 : employee.AttendanceAllowance;

        var gross = employee.BaseSalary
                  + employee.MealAllowance
                  + attendanceAllowance
                  + otPay
                  - unpaidDeduction
                  - lateDeduction;

        // BHXH 8% + BHYT 1.5% + BHTN 1% = 10.5%
        var insurance    = gross * 0.105m;
        // Giảm trừ bản thân 11tr, chưa tính giảm trừ người phụ thuộc
        var taxableIncome= gross - insurance - 11_000_000m;
        var tax          = taxableIncome <= 0 ? 0 : CalculatePIT(taxableIncome);

        var existing = await db.PayrollRecords.SingleOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year && x.Month == month);
        if (existing != null)
        {
            if (existing.Status != ApprovalStatus.PENDING)
                throw new InvalidOperationException("Bảng lương đã được duyệt hoặc chốt, không thể tính lại.");
            
            existing.BaseSalary = employee.BaseSalary;
            existing.MealAllowance = employee.MealAllowance;
            existing.AttendanceAllowance = attendanceAllowance;
            existing.OtPay = decimal.Round(otPay, 0);
            existing.UnpaidLeaveDeduction = decimal.Round(unpaidDeduction, 0);
            existing.LateDeduction = decimal.Round(lateDeduction, 0);
            existing.Gross = decimal.Round(gross, 0);
            existing.Insurance = decimal.Round(insurance, 0);
            existing.PersonalIncomeTax = decimal.Round(tax, 0);
            existing.Net = decimal.Round(gross - insurance - tax, 0);
            
            await db.SaveChangesAsync();
            return existing;
        }

        var payroll = new PayrollRecord
        {
            EmployeeId          = employeeId,
            Year                = year,
            Month               = month,
            BaseSalary          = employee.BaseSalary,
            MealAllowance       = employee.MealAllowance,
            AttendanceAllowance = attendanceAllowance,
            OtPay               = decimal.Round(otPay, 0),
            UnpaidLeaveDeduction= decimal.Round(unpaidDeduction, 0),
            LateDeduction       = decimal.Round(lateDeduction, 0),
            Gross               = decimal.Round(gross, 0),
            Insurance           = decimal.Round(insurance, 0),
            PersonalIncomeTax   = decimal.Round(tax, 0),
            Net                 = decimal.Round(gross - insurance - tax, 0),
            Status              = ApprovalStatus.PENDING,
            CreatedBy           = employeeId
        };

        db.PayrollRecords.Add(payroll);
        await db.SaveChangesAsync();
        return payroll;
    }

    private static decimal CalculatePIT(decimal income)
    {
        ReadOnlySpan<(decimal Limit, decimal Rate)> brackets =
        [
            (5_000_000,   0.05m),
            (10_000_000,  0.10m),
            (18_000_000,  0.15m),
            (32_000_000,  0.20m),
            (52_000_000,  0.25m),
            (80_000_000,  0.30m),
            (decimal.MaxValue, 0.35m)
        ];
        decimal tax = 0, previous = 0;
        foreach (var (limit, rate) in brackets)
        {
            if (income <= previous) break;
            var taxable = Math.Min(income, limit) - previous;
            tax     += taxable * rate;
            previous = limit;
        }
        return tax;
    }
}
