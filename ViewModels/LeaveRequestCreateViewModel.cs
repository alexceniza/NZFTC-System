using System;
using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class LeaveRequestCreateViewModel
    {
        [Required]
        [Display(Name = "Leave Type")]
        public string LeaveType { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}