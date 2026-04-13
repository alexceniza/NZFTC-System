using System;

namespace NZFTC_Portal.ViewModels
{
    public class EmployeeCaseViewModel
    {
        public int CaseId { get; set; }
        public string CaseType { get; set; } = string.Empty;
        public DateOnly SubmittedDate { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
    }
}