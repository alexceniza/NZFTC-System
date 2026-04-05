using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace NZFTC_Portal.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Cases> CasesTbls { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeRecord> EmployeeRecords { get; set; }

    public virtual DbSet<Holiday> Holidays { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<PayrollRecord> PayrollRecords { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;database=employee_management_system;user=root;password=SD106-2", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.45-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("admins");

            entity.HasIndex(e => e.AdminCode, "admin_code").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AdminCode)
                .HasMaxLength(50)
                .HasColumnName("admin_code");

            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.UserId)
                .HasConstraintName("fk_admin_user");
        });

        modelBuilder.Entity<Cases>(entity =>
        {
            entity.HasKey(e => e.CaseId).HasName("PRIMARY");

            entity.ToTable("cases_tbl");

            entity.HasIndex(e => e.AdminId, "idx_case_admin");

            entity.HasIndex(e => e.EmployeeId, "idx_case_employee");

            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.AdminNote)
                .HasColumnType("text")
                .HasColumnName("admin_note");
            entity.Property(e => e.CaseType)
                .HasMaxLength(100)
                .HasColumnName("case_type");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Open'")
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(150)
                .HasColumnName("subject");
            entity.Property(e => e.SubmittedDate).HasColumnName("submitted_date");

            entity.HasOne(d => d.Admin).WithMany(p => p.CasesTbls)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_case_admin");

            entity.HasOne(d => d.Employee).WithMany(p => p.CasesTbls)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_case_employee");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("employees");

            entity.HasIndex(e => e.EmployeeCode, "employee_code").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .HasColumnName("department");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(50)
                .HasColumnName("employee_code");
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(50)
                .HasColumnName("employment_status");
            entity.Property(e => e.JoinDate).HasColumnName("join_date");
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .HasColumnName("position");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .HasConstraintName("fk_employee_user");
        });

        modelBuilder.Entity<EmployeeRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId).HasName("PRIMARY");

            entity.ToTable("employee_records");

            entity.HasIndex(e => e.EmployeeId, "employee_id").IsUnique();

            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(150)
                .HasColumnName("emergency_contact");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EmploymentHistory)
                .HasColumnType("text")
                .HasColumnName("employment_history");
            entity.Property(e => e.PerformanceEvaluation)
                .HasColumnType("text")
                .HasColumnName("performance_evaluation");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(30)
                .HasColumnName("phone_number");
            entity.Property(e => e.TrainingRecord)
                .HasColumnType("text")
                .HasColumnName("training_record");

            entity.HasOne(d => d.Employee).WithOne(p => p.EmployeeRecord)
                .HasForeignKey<EmployeeRecord>(d => d.EmployeeId)
                .HasConstraintName("fk_employee_record_employee");
        });

        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.HasKey(e => e.HolidayId).HasName("PRIMARY");

            entity.ToTable("holidays");

            entity.HasIndex(e => new { e.HolidayName, e.HolidayDate }, "holiday_name").IsUnique();

            entity.HasIndex(e => e.HolidayDate, "idx_holiday_date");

            entity.Property(e => e.HolidayId).HasColumnName("holiday_id");
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.HolidayName)
                .HasMaxLength(150)
                .HasColumnName("holiday_name");
        });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveRequestId).HasName("PRIMARY");

            entity.ToTable("leave_requests");

            entity.HasIndex(e => e.AdminId, "idx_leave_admin");

            entity.HasIndex(e => e.EmployeeId, "idx_leave_employee");

            entity.Property(e => e.LeaveRequestId).HasColumnName("leave_request_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.LeaveType)
                .HasMaxLength(50)
                .HasColumnName("leave_type");
            entity.Property(e => e.Reason)
                .HasColumnType("text")
                .HasColumnName("reason");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Pending'")
                .HasColumnName("status");

            entity.HasOne(d => d.Admin).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_leave_admin");

            entity.HasOne(d => d.Employee).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_leave_employee");
        });

        modelBuilder.Entity<PayrollRecord>(entity =>
        {
            entity.HasKey(e => e.PayrollId).HasName("PRIMARY");

            entity.ToTable("payroll_records");

            entity.HasIndex(e => e.AdminId, "idx_payroll_admin");

            entity.HasIndex(e => e.EmployeeId, "idx_payroll_employee");

            entity.Property(e => e.PayrollId).HasColumnName("payroll_id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.BaseSalary)
                .HasPrecision(10, 2)
                .HasColumnName("base_salary");
            entity.Property(e => e.Deductions)
                .HasPrecision(10, 2)
                .HasColumnName("deductions");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.NetPay)
                .HasPrecision(10, 2)
                .HasColumnName("net_pay");
            entity.Property(e => e.PayDate).HasColumnName("pay_date");
            entity.Property(e => e.TaxRate)
                .HasPrecision(5, 2)
                .HasColumnName("tax_rate");

            entity.HasOne(d => d.Admin).WithMany(p => p.PayrollRecords)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_payroll_admin");

            entity.HasOne(d => d.Employee).WithMany(p => p.PayrollRecords)
                .HasForeignKey(d => d.EmployeeId)
                .HasConstraintName("fk_payroll_employee");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
