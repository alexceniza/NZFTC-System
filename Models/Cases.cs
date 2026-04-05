using System;
using System.Collections.Generic;

namespace NZFTC_Portal.Models;

public partial class Cases
{
    public int CaseId { get; set; }

    public int EmployeeId { get; set; }

    public int? AdminId { get; set; }

    public string CaseType { get; set; } = null!;

    public DateOnly SubmittedDate { get; set; }

    public string Subject { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public string? AdminNote { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
