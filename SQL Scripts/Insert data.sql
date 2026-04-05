USE employee_management_system;

-- =========================
-- USERS (Password for all users: SD106-2)
-- =========================
INSERT INTO users (user_id, full_name, email, password_hash, role) VALUES
(1, 'Alex Ceniza', 'alex.ceniza@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee'),
(2, 'Richard Kainuku', 'richard.kainuku@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee'),
(3, 'Jonathon Kim', 'jonathon.kim@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Admin'),
(4, 'Alice Thompson', 'admin@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Admin'),
(5, 'John Smith', 'john.smith@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee'),
(6, 'Maria Garcia', 'maria.garcia@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee'),
(7, 'David Lee', 'david.lee@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee'),
(8, 'Sophie Brown', 'sophie.brown@nzftc.com', 'AQAAAAIAAYagAAAAEFMq2NPqLg+VY92IRDRdY4nBh6CRxjOdvW0DbGNzo0GuLeW6U2B9Jq/eal0a4YYYXw==', 'Employee');


-- =========================
-- EMPLOYEES (MATCHES YOUR TABLE)
-- =========================
INSERT INTO employees 
(user_id, employee_code, department, position, join_date, employment_status) 
VALUES
(1, 'EMP001', 'IT', 'Software Developer', '2023-01-10', 'Active'),
(2, 'EMP002', 'IT', 'Frontend Developer', '2023-02-15', 'Active'),
(3, 'ADM001', 'Management', 'System Administrator', '2022-08-01', 'Active'),
(4, 'ADM002', 'Management', 'System Administrator', '2022-09-01', 'Active'),
(5, 'EMP003', 'Finance', 'Accountant', '2021-07-10', 'Active'),
(6, 'EMP004', 'HR', 'HR Specialist', '2023-01-20', 'Active'),
(7, 'EMP005', 'Operations', 'Operations Officer', '2022-11-05', 'Active'),
(8, 'EMP006', 'IT', 'QA Engineer', '2023-05-12', 'Active');


-- =========================
-- EMPLOYEE RECORDS
-- =========================
INSERT INTO employee_records 
(employee_id, phone_number, address, emergency_contact, performance_evaluation, training_record) 
VALUES
(1, '0210000001', 'Auckland, NZ', 'Lotte Doe', 'Exceeds expectations in backend development and teamwork.', 'Completed ASP.NET Core MVC training and secure coding workshop.'),
(2, '0210000002', 'Auckland, NZ', 'River Moe', 'Strong frontend performance with consistent delivery of UI tasks.', 'Completed advanced CSS, JavaScript, and accessibility training.'),
(3, '0210000003', 'Auckland, NZ', 'Tyler Cen', 'Perfect attendance.', 'Completed leadership trainings.'),
(4, '0210000004', 'Wellington, NZ', 'Kole Kim', 'Demonstrates effective leadership and strong system administration skills.', 'Completed leadership development and cloud infrastructure training.'),
(5, '0210000005', 'Wellington, NZ', 'Anna Smith', 'Accurate and reliable in financial reporting and payroll support.', 'Completed payroll compliance and Excel reporting training.'),
(6, '0210000006', 'Christchurch, NZ', 'Carlos Garcia', 'Performs well in staff coordination and documentation tasks.', 'Completed HR policy, onboarding, and workplace relations training.'),
(7, '0210000007', 'Hamilton, NZ', 'Kevin Lee', 'Consistently meets operational targets and handles process issues well.', 'Completed operations planning and health and safety training.'),
(8, '0210000008', 'Tauranga, NZ', 'Emma Brown', 'Shows attention to detail in testing and defect reporting.', 'Completed software testing, QA documentation, and regression testing training.');


-- =========================
-- HOLIDAYS
-- =========================
INSERT INTO holidays (holiday_name, holiday_date) VALUES
('New Year''s Day', '2026-01-01'),
('Waitangi Day', '2026-02-06'),
('Good Friday', '2026-04-03'),
('Easter Monday', '2026-04-06'),
('ANZAC Day', '2026-04-25'),
('King''s Birthday', '2026-06-01'),
('Matariki', '2026-06-19'),
('Labour Day', '2026-10-26'),
('Christmas Day', '2026-12-25'),
('Boxing Day', '2026-12-26');


-- =========================
-- LEAVE REQUESTS
-- =========================
INSERT INTO leave_requests 
(employee_id, leave_type, start_date, end_date, reason, status) 
VALUES
(1, 'Annual Leave', '2026-04-10', '2026-04-12', 'Vacation', 'Pending'),
(2, 'Sick Leave', '2026-03-20', '2026-03-22', 'Flu', 'Approved'),
(5, 'Annual Leave', '2026-05-01', '2026-05-05', 'Family event', 'Declined');


-- =========================
-- PAYROLL RECORDS
-- =========================
INSERT INTO payroll_records 
(employee_id, base_salary, tax_rate, deductions, net_pay, pay_date) 
VALUES
(1, 6500.00, 0.20, 200.00, 5000.00, '2026-03-31'),
(2, 6000.00, 0.18, 150.00, 4750.00, '2026-03-31'),
(5, 5500.00, 0.18, 150.00, 4400.00, '2026-03-31'),
(6, 5000.00, 0.15, 100.00, 4150.00, '2026-03-31'),
(7, 4800.00, 0.15, 120.00, 3960.00, '2026-03-31'),
(8, 5200.00, 0.17, 130.00, 4200.00, '2026-03-31');


-- =========================
-- CASES (GRIEVANCE / RESIGNATION)
-- =========================
INSERT INTO cases 
(employee_id, case_type, subject, description, status, submitted_date) 
VALUES
(1, 'Grievance', 'Workload Issue', 'Too much workload assigned', 'Open', '2026-03-15'),
(2, 'Complaint', 'System Bug', 'Portal is slow', 'In Progress', '2026-03-18'),
(5, 'Resignation', 'Leaving Company', 'Relocating overseas', 'Closed', '2026-02-10');