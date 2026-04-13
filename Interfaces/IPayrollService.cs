using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Interfaces
{
    public interface IPayrollService
    {
        Task<List<EmployeePayrollViewModel>> GetEmployeePayrollRecordsAsync(int employeeId);
        Task<List<AdminPayrollViewModel>> GetAllPayrollRecordsAsync();
        Task<bool> CreatePayrollRecordAsync(PayrollCreateViewModel model, int adminId);
        Task<LatestPayslipViewModel?> GetLatestPayslipAsync(int employeeId);
    }
}