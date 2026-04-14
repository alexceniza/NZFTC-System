using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _context;

        public PayrollService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<EmployeePayrollViewModel>> GetEmployeePayrollRecordsAsync(int employeeId)
        {
            return await _context.PayrollRecords
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new EmployeePayrollViewModel
                {
                    PayrollId = p.PayrollId,
                    BaseSalary = p.BaseSalary,
                    TaxRate = p.TaxRate,
                    Deductions = p.Deductions,
                    NetPay = p.NetPay,
                    PayDate = p.PayDate
                })
                .ToListAsync();
        }

        public async Task<List<AdminPayrollViewModel>> GetAllPayrollRecordsAsync()
        {
            return await _context.PayrollRecords
                .Include(p => p.Employee)
                    .ThenInclude(e => e.User)
                .Include(p => p.Admin)
                    .ThenInclude(a => a.User)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new AdminPayrollViewModel
                {
                    PayrollId = p.PayrollId,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = p.Employee.User.FullName,
                    EmployeeCode = p.Employee.EmployeeCode,
                    BaseSalary = p.BaseSalary,
                    TaxRate = p.TaxRate,
                    Deductions = p.Deductions,
                    NetPay = p.NetPay,
                    PayDate = p.PayDate,
                    ManagedByAdmin = p.Admin != null ? p.Admin.User.FullName : null
                })
                .ToListAsync();
        }

        public async Task<bool> CreatePayrollRecordAsync(PayrollCreateViewModel model, int adminId)
        {
            var employeeExists = await _context.Employees.AnyAsync(e => e.UserId == model.EmployeeId);
            if (!employeeExists)
            {
                return false;
            }

            decimal taxAmount = model.BaseSalary * model.TaxRate;
            decimal netPay = model.BaseSalary - taxAmount - model.Deductions;

            var payroll = new PayrollRecord
            {
                EmployeeId = model.EmployeeId,
                AdminId = adminId,
                BaseSalary = model.BaseSalary,
                TaxRate = model.TaxRate,
                Deductions = model.Deductions,
                NetPay = netPay,
                PayDate = DateOnly.FromDateTime(model.PayDate)
            };

            _context.PayrollRecords.Add(payroll);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<LatestPayslipViewModel?> GetLatestPayslipAsync(int employeeId)
        {
            var latestPayroll = await _context.PayrollRecords
                .Where(p => p.EmployeeId == employeeId)
                .OrderByDescending(p => p.PayDate)
                .Select(p => new LatestPayslipViewModel
                {
                    BasicSalary = p.BaseSalary,
                    Allowances = 0,
                    Deductions = p.Deductions,
                    NetPay = p.NetPay,
                    PayDate = p.PayDate
                })
                .FirstOrDefaultAsync();

            return latestPayroll;
        }
    }
}