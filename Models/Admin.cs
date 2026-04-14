using System;
using System.Collections.Generic;

namespace NZFTC_Portal.Models;

public partial class Admin
{
    public int UserId { get; set; }

    public string AdminCode { get; set; } = null!;

    public virtual ICollection<Cases> Cases { get; set; } = new List<Cases>();

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    public virtual User User { get; set; } = null!;
}
