using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class CaseCreateViewModel
    {
        [Required]
        [StringLength(100)]
        public string CaseType { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}