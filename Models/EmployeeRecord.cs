using System;
using System.Collections.Generic;

namespace NZFTC_Portal.Models;

public partial class EmployeeRecord
{
    public int RecordId { get; set; }

    public int EmployeeId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmploymentHistory { get; set; }

    public string? PerformanceEvaluation { get; set; }

    public string? TrainingRecord { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
