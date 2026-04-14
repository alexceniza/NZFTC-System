using System.Collections.Generic;

namespace NZFTC_Portal.ViewModels
{
    public class EmployeeMyInfoViewModel
    {
        public string EmployeeId { get; set; } = "--";
        public string FullName { get; set; } = "--";
        public string ContactNumber { get; set; } = "--";
        public string EmergencyContact { get; set; } = "--";
        public string EmailAddress { get; set; } = "--";
        public string Department { get; set; } = "--";
        public string Position { get; set; } = "--";
        public string JoinDate { get; set; } = "--";
        public string Status { get; set; } = "Active";

        public List<EmployeeCaseViewModel> Cases { get; set; } = new();
    }
}