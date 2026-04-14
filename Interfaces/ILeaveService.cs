using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Interfaces
{
    public interface ILeaveService
    {
        Task SubmitLeaveRequestAsync(int employeeId, LeaveRequestCreateViewModel model);
        Task<List<LeaveRequest>> GetEmployeeLeaveRequestsAsync(int employeeId);
        Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync();
        Task<bool> ApproveOrDeclineAsync(int leaveRequestId, int adminId, string decision);
        Task<AdminLeaveReportFilterViewModel> GetAdminLeaveReportAsync(AdminLeaveReportFilterViewModel filter);
        Task<EmployeeLeaveBalanceViewModel> GetEmployeeLeaveBalanceAsync(int employeeId);
    }
}