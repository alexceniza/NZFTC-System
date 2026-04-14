namespace NZFTC_Portal.ViewModels
{
    public class LatestPayslipViewModel
    {
        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public DateOnly PayDate { get; set; }
    }
}