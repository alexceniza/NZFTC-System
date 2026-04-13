using System;

namespace NZFTC_Portal.ViewModels
{
    public class AdminCaseViewModel
    {
        public int CaseId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;
        public DateOnly SubmittedDate { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public string? ReviewedByAdmin { get; set; }
    }
}