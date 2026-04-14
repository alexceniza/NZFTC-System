namespace NZFTC_Portal.ViewModels
{
    public class LeaveBalanceItemViewModel
    {
        public string LeaveType { get; set; } = string.Empty;
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
    }

    public class EmployeeLeaveBalanceViewModel
    {
        public List<LeaveBalanceItemViewModel> LeaveBalances { get; set; } = new();
    }
}