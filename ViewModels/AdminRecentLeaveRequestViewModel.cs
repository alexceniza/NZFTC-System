namespace NZFTC_Portal.ViewModels
{
    public class AdminRecentLeaveRequestViewModel
    {
        public int LeaveRequestId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}