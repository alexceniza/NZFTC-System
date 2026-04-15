using Microsoft.EntityFrameworkCore;
using NZFTC_Portal.Interfaces;
using NZFTC_Portal.Models;
using NZFTC_Portal.ViewModels;

namespace NZFTC_Portal.Services
{
    public class CaseService : ICaseService
    {
        private readonly AppDbContext _context;

        public CaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SubmitCaseAsync(int employeeId, CaseCreateViewModel model)
        {
            var newCase = new Cases
            {
                EmployeeId = employeeId,
                AdminId = null,
                CaseType = model.CaseType,
                SubmittedDate = DateOnly.FromDateTime(DateTime.Now),
                Subject = model.Subject,
                Description = model.Description,
                Status = "Open",
                AdminNote = null
            };

            _context.Cases.Add(newCase);
            await _context.SaveChangesAsync();
        }

        public async Task<List<EmployeeCaseViewModel>> GetEmployeeCasesAsync(int employeeId)
        {
            return await _context.Cases
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.SubmittedDate)
                .Select(c => new EmployeeCaseViewModel
                {
                    CaseId = c.CaseId,
                    CaseType = c.CaseType,
                    SubmittedDate = c.SubmittedDate,
                    Subject = c.Subject,
                    Description = c.Description,
                    Status = c.Status,
                    AdminNote = c.AdminNote
                })
                .ToListAsync();
        }

        public async Task<List<AdminCaseViewModel>> GetAllCasesAsync()
        {
            return await _context.Cases
                .Include(c => c.Employee)
                    .ThenInclude(e => e.User)
                .Include(c => c.Admin)
                    .ThenInclude(a => a.User)
                .OrderByDescending(c => c.SubmittedDate)
                .Select(c => new AdminCaseViewModel
                {
                    CaseId = c.CaseId,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = c.Employee.User.FullName,
                    EmployeeCode = c.Employee.EmployeeCode,
                    EmployeeEmail = c.Employee.User.Email,
                    Department = c.Employee.Department,
                    Position = c.Employee.Position,
                    CaseType = c.CaseType,
                    SubmittedDate = c.SubmittedDate,
                    Subject = c.Subject,
                    Description = c.Description,
                    Status = c.Status,
                    AdminNote = c.AdminNote,
                    ReviewedByAdmin = c.Admin != null ? c.Admin.User.FullName : null
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateCaseStatusAsync(int adminId, CaseStatusUpdateViewModel model)
        {
            var existingCase = await _context.Cases.FirstOrDefaultAsync(c => c.CaseId == model.CaseId);
            if (existingCase == null)
            {
                return false;
            }

            existingCase.AdminId = adminId;
            existingCase.Status = model.Status;
            existingCase.AdminNote = model.AdminNote;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}