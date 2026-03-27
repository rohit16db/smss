# School Management System (SMS) - Comprehensive Product Documentation

## 1. Executive Summary
The School Management System (SMS) is a modern, scalable, and cost-effective digital solution designed for small to medium-sized educational institutions. It streamlines administrative efficiency, automates financial tracking, and simplifies academic reporting through a centralized, user-friendly interface.

**Key Objectives:**
*   **Centralized Data**: Single source of truth for students, staff, and academics.
*   **Financial Automation**: Automated fee management and payroll processing.
*   **Academic Excellence**: Simple tools for exam management and professional report card generation.
*   **Operational Transparency**: Real-time analytics and attendance tracking.

---

## 2. System Architecture & Technology Stack
The SMS is built using industry-standard "Clean Architecture" and modern web technologies to ensure maintainability and high performance.

### Backend (Core Engine)
*   **Framework**: ASP.NET Core 10.0
*   **Design Pattern**: **CQRS (Command Query Responsibility Segregation)** using MediatR. This separates "read" and "write" operations for better scalability.
*   **Database**: **PostgreSQL 15** with Entity Framework Core (EF Core) using snake_case naming conventions for transparency.
*   **Validation**: **FluentValidation** for robust, real-time input checking.
*   **Reporting**: **QuestPDF** for high-performance, layout-accurate PDF generation (Report Cards, Receipts).
*   **Security**: **JWT (JSON Web Token)** based authentication with role-based authorization.

### Frontend (User Interface)
*   **Framework**: **React 19** with TypeScript.
*   **Build Tool**: **Vite 7** for lightning-fast development and hot-module replacement (HMR).
*   **UI Library**: **Material UI (MUI) v5** for a premium, responsive dashboard experience.
*   **Data Fetching**: **TanStack React Query v5** for efficient server-state management and caching.

---

## 3. User Roles & Permissions
The system employs **Role-Based Access Control (RBAC)** to ensure users only see and interact with relevant data.

| Role | Responsibilities | Key Access |
| :--- | :--- | :--- |
| **Admin** | Full system oversight and configuration. | Settings, Users, All Modules |
| **Accountant** | Financial management and payroll. | Fees, Salary, Financial Reports |
| **Clerk** | Daily administrative tasks and records. | Student Enrollment, Attendance |
| **Teacher** | Academic management and classroom data. | Marks Entry, Student Reports |

---

## 4. Functional Modules Guide

### 📂 Student Management
Allows managing the entire student lifecycle from enrollment to promotion.
*   **Enrollment**: Comprehensive forms capturing student details, guardian info, and profile images.
*   **Roll Number Management**: Automated and manual roll number assignment.
*   **Promotions**: Advanced tools to migrate students between academic years.
*   **Profiles**: Centralized view of attendance, fees, and academic performance.

### 🏫 Academic Management
The foundation of the school's structure.
*   **Academic Years**: Define active sessions (e.g., 2025-26).
*   **Classes & Sections**: Hierarchical organization of the school.
*   **Subjects & Departments**: Domain-specific categorization for staff and curriculum.

### 📅 Attendance System
Real-time tracking for both students and staff.
*   **Daily Marking**: Simple grid view for marking attendance by class/section.
*   **Reporting**: Monthly summaries and individual attendance trends for students/staff.

### 📝 Exam & Result Management
Automates the grading cycle.
*   **Exam Setup**: Define exams (Mid-term, Final) and link to subjects/classes.
*   **Marks Entry**: Dedicated interfaces for teachers to enter marks efficiently.
*   **Report Cards**: Generate professional, branded PDF report cards with automated grade calculation and class positions.

### 💰 Financial Management
Robust tools for handling school revenue and expenses.
*   **Fee Structures**: Flexible definition of fees (Monthly, Annual, One-time).
*   **Fee Payments**: Record payments (Cash, Online, Check) and generate instant PDF receipts.
*   **Bulk Assignment**: Assign common fee structures to entire sections with one click.
*   **Payroll**: Manage staff salary structures, process bulk salaries, and track payments.

---

## 5. Developer Guide (Extension)
The system is designed for extension by developers following these patterns:

*   **Adding a Feature**:
    1.  Define **Domain Entity** in `SMS.Domain`.
    2.  Implement **MediatR Commands/Queries** in `SMS.Application`.
    3.  Create **API Controller** in `SMS.API`.
    4.  Build the **UI Page** using MUI components and React Query hooks.
*   **Coding Standards**:
    *   **Backend**: Use PascalCase for C# and snake_case for PostgreSQL.
    *   **Frontend**: Use camelCase for TSX and PascalCase for Components.
    *   **Architecture**: Never allow `SMS.Domain` to depend on other layers.

---

## 6. Training Scenarios (How-To)

### Scenario A: Enrolling a New Student (Student Management)
1.  Navigate to **Students** from the sidebar.
2.  Click the **"Add Student"** button.
3.  Fill in Student Details (Name, DOB, Gender, Address).
4.  Provide **Guardian Information** (Phone/Email).
5.  *(Optional)* Upload a profile picture to appear on ID cards.
6.  Click **"Save"**. The system will automatically generate a unique Student ID.

### Scenario B: Setting Up Academic Year & Classes (Academic Management)
1.  Navigate to **Academic Year Management** in Settings.
2.  Create a new year (e.g., "2025-2026") and set it as **"Active"**.
3.  Go to **Class Management**.
4.  Define Classes (e.g., Grade 1, Grade 2) and assign **Sections** (e.g., Section A, B).
5.  Assign **Subjects** to each class to build the curriculum foundation.

### Scenario C: Recording Daily Class Attendance (Attendance System)
1.  Go to the **Attendance** page.
2.  Select the **Class** and **Section**.
3.  Choose the **Date** (defaults to today).
4.  The list of students will appear; by default, all are marked "Present".
5.  Toggle the status to "Absent" or "Late" for specific students.
6.  Click **"Save All"**.

### Scenario D: Configuring Fees & Bulk Assignment (Financial/Fees)
1.  Navigate to **Fees** -> **Fee Structures**.
2.  Create a structure (e.g., "Standard Monthly Fee") and add **Categories** (Tuition, Development, Library).
3.  Go to **Bulk Assign Fees**.
4.  Select the Fee Structure and the **Target Section** (e.g., Grade 1 - A).
5.  Click **"Process Assign"** to apply fees to all students in that group simultaneously.

### Scenario E: Recording Fee Payments & Receipts (Financial/Fees)
1.  Search for a student in **Outstanding Fees**.
2.  Click **"Record Payment"** next to their due fee.
3.  Enter the **Amount Paid** and **Payment Method** (Cash, UPI, Bank).
4.  Click **"Complete Payment"**.
5.  Click **"Download Receipt"** to provide a PDF record to the parent.

### Scenario F: Setting Up Exams & Marks (Exam/Result Management)
1.  In the **Exams** module, click **"Create Exam"** (e.g., "Term 1 Final").
2.  Assign the **Classes** and **Subjects** participating in this exam.
3.  Once the exam is conducted, go to **Marks Entry**.
4.  Select the Class and Subject to load the student list.
5.  Enter marks for each student and click **"Finalize"**.

### Scenario G: Generating Professional Report Cards (Exam/Result Management)
1.  Navigate to **Report Cards** module.
2.  Select the **Exam** and **Class/Section**.
3.  Click on a specific student or **"Generate Bulk for Class"**.
4.  The system calculates **Grades, Total Marks, and Class Position** automatically.
5.  Click **"Download PDF"** to get the printable, branded report cards.

### Scenario H: Managing Staff & Payroll (Staff/Payroll)
1.  Add new staff in the **Staff Directory**.
2.  Navigate to **Salary Structure** and assign a compensation package to the employee.
3.  At the end of the month, go to **Bulk Salary Processing**.
4.  Review the attendance-based adjustments and click **"Generate Monthly Payroll"**.
5.  Process payments and download **Salary Slips** for staff records.

### Scenario I: Timetable & Holiday Setup (Administrative Tools)
1.  Define **Time Slots** (e.g., 9:00 AM - 9:45 AM) in Settings.
2.  Navigate to **Timetable Management**.
3.  Drag and drop Subjects/Teachers into the grid for each Class.
4.  In the **Holidays** module, mark upcoming breaks or public holidays to automatically exclude them from attendance tracking.

---

## 🚀 Recommended Next Steps
*   **Setup**: For local development, refer to [INSTALLATION-WINDOWS.md](INSTALLATION-WINDOWS.md) or [DOCKER.md](DOCKER.md).
*   **Reference**: Explore the interactive [Swagger Documentation](http://localhost:5208/swagger) when the API is running.
*   **Architecture**: Review the project [Constitution](.specify/memory/constitution.md) for core principles.

---
*Documentation Version: 1.0.0 | Updated: March 2026*
