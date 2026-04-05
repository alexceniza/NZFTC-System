DROP DATABASE IF EXISTS employee_management_system;
CREATE DATABASE employee_management_system;
USE employee_management_system;

-- =========================================================
-- 1. USERS (Abstract User)
-- =========================================================
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(150) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL
);

-- =========================================================
-- 2. ADMINS
-- inherits from users
-- =========================================================
CREATE TABLE admins (
    user_id INT PRIMARY KEY,
    admin_code VARCHAR(50) NOT NULL UNIQUE,
    CONSTRAINT fk_admin_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- =========================================================
-- 3. EMPLOYEES
-- inherits from users
-- =========================================================
CREATE TABLE employees (
    user_id INT PRIMARY KEY,
    employee_code VARCHAR(50) NOT NULL UNIQUE,
    department VARCHAR(100) NOT NULL,
    position VARCHAR(100) NOT NULL,
    join_date DATE NOT NULL,
    employment_status VARCHAR(50) NOT NULL,
    CONSTRAINT fk_employee_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- =========================================================
-- 4. EMPLOYEE RECORDS
-- one employee owns one employee record
-- =========================================================
CREATE TABLE employee_records (
    record_id INT AUTO_INCREMENT PRIMARY KEY,
    employee_id INT NOT NULL UNIQUE,
    phone_number VARCHAR(30),
    address VARCHAR(255),
    emergency_contact VARCHAR(150),
    employment_history TEXT,
    performance_evaluation TEXT,
    training_record TEXT,
    CONSTRAINT fk_employee_record_employee
        FOREIGN KEY (employee_id) REFERENCES employees(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

-- =========================================================
-- 5. LEAVE REQUESTS
-- employee submits leave request
-- admin reviews leave request
-- =========================================================
CREATE TABLE leave_requests (
    leave_request_id INT AUTO_INCREMENT PRIMARY KEY,
    employee_id INT NOT NULL,
    admin_id INT NULL,
    leave_type VARCHAR(50) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    reason TEXT,
    status VARCHAR(30) NOT NULL DEFAULT 'Pending',
    CONSTRAINT fk_leave_employee
        FOREIGN KEY (employee_id) REFERENCES employees(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_leave_admin
        FOREIGN KEY (admin_id) REFERENCES admins(user_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT chk_leave_dates CHECK (end_date >= start_date)
);

-- =========================================================
-- 6. PAYROLL RECORDS
-- admin manages payroll
-- employee receives payroll
-- =========================================================
CREATE TABLE payroll_records (
    payroll_id INT AUTO_INCREMENT PRIMARY KEY,
    employee_id INT NOT NULL,
    admin_id INT NULL,
    base_salary DECIMAL(10,2) NOT NULL,
    tax_rate DECIMAL(5,2) NOT NULL,
    deductions DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    net_pay DECIMAL(10,2) NOT NULL,
    pay_date DATE NOT NULL,
    CONSTRAINT fk_payroll_employee
        FOREIGN KEY (employee_id) REFERENCES employees(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_payroll_admin
        FOREIGN KEY (admin_id) REFERENCES admins(user_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

-- =========================================================
-- 7. CASES
-- employee submits case
-- admin updates case
-- =========================================================
CREATE TABLE cases (
    case_id INT AUTO_INCREMENT PRIMARY KEY,
    employee_id INT NOT NULL,
    admin_id INT NULL,
    case_type VARCHAR(100) NOT NULL,
    submitted_date DATE NOT NULL,
    subject VARCHAR(150) NOT NULL,
    description TEXT,
    status VARCHAR(50) NOT NULL DEFAULT 'Open',
    admin_note TEXT,
    CONSTRAINT fk_case_employee
        FOREIGN KEY (employee_id) REFERENCES employees(user_id)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT fk_case_admin
        FOREIGN KEY (admin_id) REFERENCES admins(user_id)
        ON DELETE SET NULL
        ON UPDATE CASCADE
);

-- =========================================================
-- 8. HOLIDAYS
-- =========================================================
CREATE TABLE holidays (
    holiday_id INT AUTO_INCREMENT PRIMARY KEY,
    holiday_name VARCHAR(150) NOT NULL,
    holiday_date DATE NOT NULL,
    UNIQUE (holiday_name, holiday_date)
);

-- =========================================================
-- INDEXES
-- =========================================================
CREATE INDEX idx_leave_employee ON leave_requests(employee_id);
CREATE INDEX idx_leave_admin ON leave_requests(admin_id);

CREATE INDEX idx_payroll_employee ON payroll_records(employee_id);
CREATE INDEX idx_payroll_admin ON payroll_records(admin_id);

CREATE INDEX idx_case_employee ON cases(employee_id);
CREATE INDEX idx_case_admin ON cases(admin_id);

CREATE INDEX idx_holiday_date ON holidays(holiday_date);