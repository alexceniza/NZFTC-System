using System;
using System.Collections.Generic;

namespace NZFTC_Portal.Models;

public partial class Employee
{
    public int UserId { get; set; }

    public string EmployeeCode { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateOnly JoinDate { get; set; }

    public string EmploymentStatus { get; set; } = null!;

    public virtual ICollection<Cases> CasesTbls { get; set; } = new List<Cases>();

    public virtual EmployeeRecord? EmployeeRecord { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public virtual ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();

    public virtual User User { get; set; } = null!;
}
