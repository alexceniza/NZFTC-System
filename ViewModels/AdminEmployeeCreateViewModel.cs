using System.ComponentModel.DataAnnotations;

namespace NZFTC_Portal.ViewModels
{
    public class AdminEmployeeCreateViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Employee";

        [Required]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string EmergencyContact { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? Position { get; set; }

        public string? TrainingRecord { get; set; }

        public string? EmploymentHistory { get; set; }

        public string? ConfirmationCode { get; set; }
    }
}