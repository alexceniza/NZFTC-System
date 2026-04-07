using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class AdminLeaveDecisionViewModel
    {
        public int LeaveRequestId { get; set; }

        [Required]
        public string Decision { get; set; } = string.Empty;
    }
}