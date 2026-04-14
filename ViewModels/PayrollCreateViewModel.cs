using System;
using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class PayrollCreateViewModel
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal BaseSalary { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal TaxRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Deductions { get; set; }

        [Required]
        public DateTime PayDate { get; set; }
    }
}