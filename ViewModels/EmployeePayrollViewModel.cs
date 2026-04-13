using System;

namespace NZFTC_Portal.ViewModels
{
    public class EmployeePayrollViewModel
    {
        public int PayrollId { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public DateOnly PayDate { get; set; }
    }
}