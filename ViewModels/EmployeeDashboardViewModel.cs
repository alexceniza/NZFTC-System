using NZFTC_Portal.Models;

namespace NZFTC_Portal.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        // Leave Balances
        public LeaveBalanceItemViewModel? AnnualLeave { get; set; }
        public LeaveBalanceItemViewModel? SickLeave { get; set; }
        public LeaveBalanceItemViewModel? PersonalLeave { get; set; }

        // Latest Payslip
        public LatestPayslipViewModel? LatestPayslip { get; set; }

        // Holidays
        public List<Holiday>? HolidayPreview { get; set; }
        public List<Holiday>? HolidayFullList { get; set; }
    }
}