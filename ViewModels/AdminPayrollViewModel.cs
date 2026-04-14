using System;

namespace NZFTC_Portal.ViewModels
{
    public class AdminPayrollViewModel
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public DateOnly PayDate { get; set; }
        public string? ManagedByAdmin { get; set; }
    }
}