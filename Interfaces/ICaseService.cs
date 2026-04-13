using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Interfaces
{
    public interface ICaseService
    {
        Task SubmitCaseAsync(int employeeId, CaseCreateViewModel model);
        Task<List<EmployeeCaseViewModel>> GetEmployeeCasesAsync(int employeeId);
        Task<List<AdminCaseViewModel>> GetAllCasesAsync();
        Task<bool> UpdateCaseStatusAsync(int adminId, CaseStatusUpdateViewModel model);
    }
}