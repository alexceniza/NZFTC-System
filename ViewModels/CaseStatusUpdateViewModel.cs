using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class CaseStatusUpdateViewModel
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public string? AdminNote { get; set; }
    }
}