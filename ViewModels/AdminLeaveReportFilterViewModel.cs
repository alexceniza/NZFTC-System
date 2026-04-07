using System;
using System.Collections.Generic;

namespace NZFTC_Portal.ViewModels
{
    public class AdminLeaveReportFilterViewModel
    {
        public string? Status { get; set; }
        public string? LeaveType { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<AdminLeaveReportRowViewModel> Results { get; set; } = new();

        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int DeclinedCount { get; set; }
    }

    public class AdminLeaveReportRowViewModel
    {
        public int LeaveRequestId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReviewedByAdmin { get; set; }
        public int TotalDays { get; set; }
    }
}