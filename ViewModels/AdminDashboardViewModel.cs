namespace NZFTC_Portal.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int PendingLeaveApprovals { get; set; }
        public int EmployeeCount { get; set; }
        public int OpenCases { get; set; }

        public List<AdminRecentLeaveRequestViewModel> RecentLeaveRequests { get; set; } = new();
    }
}