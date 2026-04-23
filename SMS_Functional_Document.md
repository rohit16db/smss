# School Management System (SMS) — Comprehensive Functional Document

> **Audience**: New administrators, developers, and end-users setting up or using the SMS application for the first time.

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Technology Stack](#2-technology-stack)
3. [Environment Setup](#3-environment-setup)
4. [Initial Configuration (First-Time UI Setup)](#4-initial-configuration-first-time-ui-setup)
5. [User Roles & Access Control](#5-user-roles--access-control)
6. [Navigation Guide](#6-navigation-guide)
7. [Feature Reference](#7-feature-reference)
   - 7.1 Dashboard
   - 7.2 Academic Year Management
   - 7.3 School Settings
   - 7.4 Department Management
   - 7.5 Class & Section Management
   - 7.6 Subject Management
   - 7.7 Student Management
   - 7.8 Staff Management
   - 7.9 Fee Management
   - 7.10 Attendance Tracking
   - 7.11 Exam & Marks Management
   - 7.12 Report Cards & Analytics
   - 7.13 Timetable Management
   - 7.14 Salary & Payroll
   - 7.15 Transport Management
   - 7.16 Inventory Management
   - 7.17 Holiday Management
   - 7.18 Notification System
   - 7.19 Reports & Analytics
   - 7.20 Student Promotion
   - 7.21 Roll Number Management
8. [Recommended Setup Order (Checklist)](#8-recommended-setup-order-checklist)

---

## 1. System Overview

SMS is a **cost-effective, scalable school management system** for small to medium-sized schools (200–1,000+ students). It digitizes administrative operations including:

- Student enrollment & profile management
- Fee structure definition, payment collection, and receipt generation
- Staff/teacher profiles, salary structures, and payroll processing
- Attendance tracking (students & staff)
- Examination management, marks entry, and report cards
- Timetable scheduling
- Transport route & vehicle management
- Inventory tracking
- Holiday calendar management
- Notification templates (SMS/WhatsApp)
- Financial and attendance reports with dashboards

The system uses **Role-Based Access Control (RBAC)** with four roles: **Admin**, **Accountant**, **Clerk**, and **Teacher/Staff**.

---

## 2. Technology Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | React 19, TypeScript, Vite 7, Material UI v5, TailwindCSS, React Router 7, TanStack React Query v5, Axios |
| **Backend** | ASP.NET Core 10 Web API, MediatR (CQRS), FluentValidation |
| **Database** | PostgreSQL 15, Entity Framework Core 10, snake_case naming |
| **Auth** | JWT (access + refresh tokens), BCrypt password hashing |
| **DevOps** | Docker Compose, Nginx (production), dotnet watch / Vite HMR (dev) |

---

## 3. Environment Setup

### 3.1 Docker Compose (Recommended — 3 Commands)

**Prerequisites**: Docker Desktop 20.10+

```bash
git clone <repository-url> && cd SMS
cp .env.example .env
# Edit .env → set DATABASE_PASSWORD and JWT_SECRET (min 32 chars)
docker-compose up -d
```

**Access Points**:
| Service | URL |
|---------|-----|
| Frontend App | http://localhost:5173 |
| Backend API | http://localhost:5208/health |
| Swagger Docs | http://localhost:5208/swagger |
| PostgreSQL | localhost:5432 |

### 3.2 Manual Setup (Without Docker)

**Prerequisites**: .NET SDK 10+, Node.js 20 LTS+, PostgreSQL 15+, EF Core CLI

1. **Database**: Create a PostgreSQL database named `school_management_db`
2. **Backend secrets**:
   ```bash
   cd backend
   dotnet user-secrets init --project src/SMS.API
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Host=localhost;Port=5432;Database=school_management_db;Username=postgres;Password=YOUR_PASSWORD" \
     --project src/SMS.API
   dotnet user-secrets set "JwtSettings:SecretKey" "your-super-secret-jwt-key-min-32-chars-long" \
     --project src/SMS.API
   ```
3. **Run migrations**: `dotnet ef database update --project src/SMS.Infrastructure --startup-project src/SMS.API`
4. **Start backend**: `dotnet run --project src/SMS.API` → runs on port 5208
5. **Start frontend**: `cd frontend && npm install && npm run dev` → runs on port 5173

### 3.3 Key Environment Variables

| Variable | Purpose | Example |
|----------|---------|---------|
| `DATABASE_PASSWORD` | PostgreSQL password | `SecurePass123!` |
| `JWT_SECRET` | Token signing key (≥32 chars) | `your-super-secret-jwt-key-...` |
| `JWT_EXPIRY_HOURS` | Token lifetime | `8` |
| `VITE_API_BASE_URL` | Frontend → Backend URL | `http://localhost:5000` |
| `CORS_ALLOWED_ORIGINS` | Allowed frontend origins | `http://localhost:5173` |

---

## 4. Initial Configuration (First-Time UI Setup)

After the application is running, follow these steps **in order** from the UI to make the system usable.

### Step 1 — Register the Admin User

1. Open `http://localhost:5173` → you will be redirected to `/login`
2. Use the **Register** endpoint via Swagger (`POST /api/auth/register`) or the registration flow to create the first user:
   - **Username**: e.g., `admin`
   - **Email**: admin email
   - **Password**: strong password
   - **Role**: `Admin` (role value = 1)
3. Log in at `/login` with the credentials

### Step 2 — Configure School Settings

1. Click your **profile icon** (top-right) → **⚙️ Settings** (Admin only)
2. Navigate to `/admin/settings` — four tabs to configure:

| Tab | Fields to Set |
|-----|---------------|
| **📋 Basic Information** | School Name *, School Code *, Address, City, State, Postal Code, Phone, Email, Website, Established Date |
| **🎨 Branding** | Upload School Logo (PNG/JPG/GIF, max 5MB), set Primary/Secondary/Accent Colors, Header Text, Footer Text |
| **⚡ Preferences** | Date Format (dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd), Currency Code (e.g., INR), Currency Symbol (e.g., ₹) |
| **🔔 Notifications** | Configure SMS/WhatsApp notification templates |

3. Click **💾 Save Changes** after each tab

### Step 3 — Create Academic Year(s)

1. Go to **Profile → 📅 Academic Years** (`/admin/academic-years`)
2. Create at least one academic year:
   - **Name**: e.g., `2026-2027`
   - **Start Date**: session start
   - **End Date**: session end
   - **Is Current**: mark as active
3. The active academic year appears in the **"Current Session"** dropdown in the header

### Step 4 — Create Departments

1. Navigate to **Academic → 🏢 Departments** (`/departments`)
2. Add departments (e.g., Mathematics, Science, English, Administration)
3. Departments are later assigned to staff members

### Step 5 — Create Classes & Sections

1. Navigate to **Academic → 📚 Classes** (`/classes`)
2. Add classes (e.g., "Grade 1", "Grade 2", ... "Grade 12")
3. Within each class, create **Sections** (e.g., Section A, Section B)
4. Classes and sections are required for student enrollment

### Step 6 — Create Subjects

1. Navigate to **Academic → 📖 Subjects** (`/subjects`)
2. Add subjects (e.g., Mathematics, English, Science, Hindi)
3. Subjects are linked to exams and timetable entries

### Step 7 — Add Staff Members

1. Navigate to **Academic → 👨‍🏫 Staff Management** (`/staff`)
2. Add staff with:
   - Personal info (name, email, phone, date of birth, address)
   - Department assignment
   - Designation (e.g., "Senior Math Teacher")
   - Role Type (Admin/Accountant/Clerk/Teacher)
   - Joining date, experience years
   - Basic salary, qualifications

### Step 8 — Enroll Students

1. Navigate to **Academic → 👨‍🎓 Students** (`/students`)
2. Add students with:
   - Personal info, guardian details, enrollment number
   - Assign to Class, Section, and Academic Year (creates an Enrollment record)
   - Upload profile image

### Step 9 — Set Up Fee Structures (if managing fees)

1. Navigate to **Finance → 💰 Fees** (`/fees`)
2. Create a fee structure with categories (Tuition, Lab, Library, etc.)
3. Assign fees to students

### Step 10 — Configure Grade System (for exams)

- Grade configurations (A, B, C, D, F with percentage ranges) are set up for the school and used in report card generation.

---

## 5. User Roles & Access Control

The system has **4 roles** with different access levels:

| Role | ID | Access |
|------|----|--------|
| **Admin** | 1 | Full access to everything. Can manage settings, academic years, users, holidays, and all modules. |
| **Accountant** | 2 | Finance modules: Fees, Salary, Payroll, Fee Reports, Salary Reports, Budget vs Actual. |
| **Clerk** | 3 | Academic modules: Students, Staff, Departments, Classes, Subjects, Roll Numbers, Student Promotion, Attendance, Transport, Inventory. |
| **Teacher (Staff)** | 4 | Limited access: Attendance (own classes), Exams (marks entry), Timetable (view), Salary (view own). |

### Role-to-Module Access Matrix

| Module | Admin | Accountant | Clerk | Staff |
|--------|:-----:|:----------:|:-----:|:-----:|
| Dashboard | ✅ | ✅ | ✅ | ✅ |
| Students | ✅ | ❌ | ✅ | ❌ |
| Staff Management | ✅ | ❌ | ✅ | ❌ |
| Departments | ✅ | ❌ | ✅ | ❌ |
| Classes | ✅ | ❌ | ✅ | ❌ |
| Subjects | ✅ | ❌ | ✅ | ❌ |
| Fees | ✅ | ✅ | ❌ | ❌ |
| Attendance | ✅ | ❌ | ✅ | ✅ |
| Exams & Marks | ✅ | ❌ | ✅ | ✅ |
| Salary (View) | ✅ | ✅ | ❌ | ✅ |
| Payroll | ✅ | ✅ | ❌ | ❌ |
| Salary Structures | ✅ | ✅ | ❌ | ❌ |
| Reports | ✅ | ✅ | ✅ | ❌ |
| Settings | ✅ | ❌ | ❌ | ❌ |
| Academic Years | ✅ | ❌ | ❌ | ❌ |
| Holidays | ✅ | ❌ | ❌ | ❌ |
| Transport | ✅ | ❌ | ✅ | ❌ |
| Inventory | ✅ | ❌ | ✅ | ❌ |
| Timetable | ✅ | ❌ | ✅ | ✅ |
| Roll Numbers | ✅ | ❌ | ✅ | ❌ |
| Student Promotion | ✅ | ❌ | ✅ | ❌ |

---

## 6. Navigation Guide

The application header has **5 main dropdown menus** + user profile:

### 🎓 Academic Menu
Students, Staff Management, Departments, Classes, Subjects, Exams, Timetable, Transport Management, Inventory Management, Roll Numbers, Holidays, Student Promotion

### 💰 Finance Menu
Fees, Fee Report, Salary, Payroll

### 💼 Payroll Menu
Salary Structures, Staff Assignments, Bulk Processing, Payment Management

### 📊 Reports Menu
Outstanding Fees, Salary Comparison, Budget vs Actual, Attendance Reports

### 👤 User Profile Menu
Academic Years (Admin), Settings (Admin), Change Password, Logout

### 📅 Session Selector
A **"Current Session"** dropdown in the header lets you switch between academic years. All data (enrollment, fees, attendance, exams) is filtered by the selected academic year.

---

## 7. Feature Reference

### 7.1 Dashboard (`/`)

The home page shows:
- **Summary cards**: Total Students, Total Staff, Active Fee Structures, Today's Attendance
- **Module quick-access cards**: Click any card to navigate to that module
- **Dashboard Overview**: Financial summary (collection rate), attendance trends
- **Charts**: Fee Collection Chart, Attendance Trend Chart
- **System Status**: Health check indicator

---

### 7.2 Academic Year Management (`/admin/academic-years`)

**Access**: Admin only

| Action | How |
|--------|-----|
| Create Year | Click "Add", enter Name (e.g., "2026-2027"), Start Date, End Date |
| Set Active | Mark one year as "Is Current" — this becomes the default session |
| Switch Session | Use the "Current Session" dropdown in the header to switch context |

> [!IMPORTANT]
> All data (enrollments, fees, attendance, exams, holidays) is scoped to the selected academic year. Always ensure the correct session is selected before entering data.

---

### 7.3 School Settings (`/admin/settings`)

**Access**: Admin only

**Four configuration tabs**:

1. **Basic Information**: School name, code, address, contact info, established date
2. **Branding**: Logo upload (PNG/JPG/GIF ≤5MB), color scheme (primary, secondary, accent), header/footer text for reports
3. **Preferences**: Date format, currency code & symbol
4. **Notifications**: Configure notification templates for SMS/WhatsApp

---

### 7.4 Department Management (`/departments`)

**Access**: Admin, Clerk

- **Create** departments (e.g., Mathematics, Science, Administration)
- **Edit** department name and details
- **Delete** departments (if no staff assigned)
- Departments are assigned to Staff members for organizational grouping

---

### 7.5 Class & Section Management (`/classes`)

**Access**: Admin, Clerk

| Action | How |
|--------|-----|
| Create Class | Add class name (e.g., "Grade 10") |
| Add Sections | Within each class, add sections (e.g., "A", "B", "C") |
| Edit/Delete | Modify class names, toggle active status |

> [!TIP]
> Classes + Sections form the organizational unit for student enrollment. A student is enrolled in a specific Class + Section for a given Academic Year.

---

### 7.6 Subject Management (`/subjects`)

**Access**: Admin, Clerk

- Create subjects with name and details
- Subjects are used in: Exams (defining exam subjects), Timetable (scheduling), Staff Assignments (teacher-subject mapping)

---

### 7.7 Student Management (`/students`)

**Access**: Admin, Clerk

#### Adding a Student
1. Click **"Add Student"**
2. Fill in: First Name, Last Name, Email, Phone, Date of Birth, Address, City, State, Postal Code
3. **Guardian Info**: Guardian Name, Guardian Phone, Guardian Email
4. **Enrollment**: Select Class, Section, Academic Year → auto-generates Enrollment Number
5. **Profile Image**: Upload student photo
6. Click **Save**

#### Features
| Feature | Description |
|---------|-------------|
| Search & Filter | Search by name, filter by class/section, active status |
| Pagination | Server-side pagination for large student lists |
| Edit Profile | Update student info, change class/section |
| View Enrollments | See enrollment history across academic years |
| Profile Image | Upload/crop student profile photos |

---

### 7.8 Staff Management (`/staff`)

**Access**: Admin, Clerk

#### Adding Staff
1. Click **"Add Staff"**
2. **Personal Info**: First Name, Last Name, Email, Phone, Date of Birth, Address, Gender
3. **Employment**: Department, Designation, Role Type, Joining Date, Experience Years
4. **Salary**: Basic Salary amount
5. **Qualifications**: Add educational qualifications (degree, institution, year)

#### Features
| Feature | Description |
|---------|-------------|
| Directory View | Paginated list with search and filters |
| Staff Assignments | Assign staff to classes/sections for an academic year |
| Profile Image | Upload staff photo |
| Qualifications | Track educational qualifications |
| Salary Structure Link | Assign a salary structure to a staff member |

---

### 7.9 Fee Management (`/fees`)

**Access**: Admin, Accountant

This is one of the most complex modules with multiple sub-features:

#### Step 1: Create Fee Structure
- **Name**: e.g., "Regular Monthly 2026"
- **Academic Year**: Select the year
- **Frequency**: Monthly, Quarterly, or Yearly
- **Categories**: Add fee categories with amounts:
  - Tuition Fee: ₹2,000
  - Lab Fee: ₹500
  - Library Fee: ₹200
  - etc.
- **Total Amount**: Auto-calculated from categories

#### Step 2: Assign Fees to Students
- Select students (by class/section) and assign a fee structure
- Creates a `StudentFee` record with: Structure Amount + Transport Fee = Total Amount
- Track: Paid Amount, Balance Amount

#### Step 3: Record Payments
- For each student fee, record payments:
  - Amount Paid, Payment Date, Payment Method (Cash/Check/Bank Transfer)
  - Auto-generates a unique Receipt Number
  - Optional notes
- Payment is immutable (cannot be edited, only reversed)

#### Fee Reports
| Report | Path | Description |
|--------|------|-------------|
| Fee Report | `/fee-report` | Daily/monthly collection summary |
| Fee Reports | `/fee-reports` | Detailed fee analytics |
| Outstanding Fees | `/outstanding-fees` | Students with unpaid balances |
| Download Fee Statement | Per-student | PDF export of fee details with category breakdown |

---

### 7.10 Attendance Tracking (`/attendance`)

**Access**: Admin, Clerk, Staff

#### Student Attendance
1. Select **Class** and **Section**
2. Select **Date**
3. Mark each student: **Present**, **Absent**, **Late**, **Half Day**, or **Excused**
4. Save attendance for the day

#### Staff Attendance
- Similar flow for marking staff attendance
- Tracks: Present, Absent, Late, On Leave

#### Attendance Reports (`/attendance-reports`)
**Access**: Admin, Clerk
- View attendance trends over time
- Filter by class, section, date range
- Student-wise and class-wise attendance percentages

---

### 7.11 Exam & Marks Management (`/exams`)

**Access**: Admin, Clerk, Staff

#### Creating an Exam
1. Click **"Create Exam"**
2. Fill in: Name (e.g., "Mid-Term 2026"), Description, Start Date, End Date
3. Set Total Marks (default: 100), Pass Marks (default: 40)
4. Select Academic Year
5. **Assign Classes**: Select which classes take this exam
6. **Assign Subjects**: Select subjects and set max marks per subject
7. Status workflow: **Draft** → **Published** → **Completed**

#### Marks Entry (`/exams/:examId/marks`)
1. Select a published exam
2. Select Class and Subject
3. Enter marks for each student
4. System validates: marks ≤ max marks for the subject

---

### 7.12 Report Cards & Analytics

#### Report Cards (`/exams/:examId/report-cards`)
- Generate report cards for an exam
- Shows per-student breakdown: subject-wise marks, total, percentage, grade
- Uses Grade Configuration (A/B/C/D/F with percentage boundaries)

#### Report Card Detail (`/report-cards/:examId/:studentId`)
- Individual student report card with full details
- Includes school branding (logo, header/footer from Settings)

#### Performance Analytics (`/exams/:examId/analytics`)
- Class-wise performance charts
- Subject-wise analysis
- Pass/fail statistics
- Grade distribution

---

### 7.13 Timetable Management (`/timetable`)

**Access**: Admin, Clerk, Staff

- Create time slots (e.g., 8:00–8:45, 8:45–9:30, etc.)
- Assign timetable entries: Class + Section + Day + Time Slot + Subject + Teacher
- View timetable in a weekly grid format
- Filter by class/section

---

### 7.14 Salary & Payroll

This spans multiple pages:

#### Salary Structures (`/salary-structures`)
**Access**: Admin, Accountant

Create templates defining pay components:
| Component | Description |
|-----------|-------------|
| Base Salary | Core pay amount |
| HRA | House Rent Allowance |
| DA | Dearness Allowance |
| Medical Allowance | Medical benefits |
| Conveyance Allowance | Transport benefits |
| Other Allowances | Miscellaneous |
| Standard Deduction | PF, tax, etc. |

- **Gross Salary** = Base + All Allowances − Standard Deduction
- Set minimum experience years and applicable qualifications
- Set effective from/to dates

#### Staff Salary Assignment (`/staff-salary-assignment`)
- Assign a salary structure to a staff member with an effective date

#### Salary View (`/salary`)
**Access**: Admin, Accountant, Staff (own salary)
- View salary details for staff members

#### Bulk Salary Processing (`/bulk-salary-processing`)
**Access**: Admin, Accountant
- Process salary for multiple staff members at once for a given period

#### Salary Payments (`/salary-payments`)
**Access**: Admin, Accountant
- Record individual salary payments
- Status workflow: **Pending** → **Approved** → **Paid** (or Cancelled/On Hold)
- Payment methods: Cash, Bank Transfer, Cheque, Mobile Payment

#### Payroll (`/payroll`)
**Access**: Admin, Accountant
- Monthly payroll overview and processing dashboard

---

### 7.15 Transport Management (`/transport`)

**Access**: Admin, Clerk

#### Routes
- Create transport routes with: Route Name, Description, Monthly Fee
- Add **Stops** to each route (pickup/drop points)

#### Vehicles
- Register vehicles with details
- Assign a vehicle to a route

#### Student Assignments
- Assign students to transport routes
- Transport fee is automatically included in the student's fee calculation

---

### 7.16 Inventory Management (`/inventory`)

**Access**: Admin, Clerk

#### Categories
- Create inventory categories (e.g., Uniforms, Books, Stationery, Lab Equipment)

#### Items
- Add items: Name, SKU, Description, Category, Unit Price, Reorder Level
- Track total quantity

#### Movement Logs (Transactions)
- Record stock in/out transactions
- Track inventory changes over time
- Pagination and search for large inventories

---

### 7.17 Holiday Management (`/holidays`)

**Access**: Admin only

- Add holidays for the current academic year
- Fields: Name, Date, Description, Type (National, Religious, School Event)
- Holidays are used by the attendance module to skip/mark those dates

---

### 7.18 Notification System

#### Notification Templates (Settings → Notifications)
- Create templates with placeholders like `{{StudentName}}`, `{{Amount}}`
- **Channels**: SMS or WhatsApp
- **Categories**: Fees, Transport, Attendance, General
- Templates can be activated/deactivated

#### Notification History
- Track sent notifications with status, recipient, and timestamp

---

### 7.19 Reports & Analytics

| Report | Path | Access | Description |
|--------|------|--------|-------------|
| Fee Report | `/fee-report` | Admin, Accountant | Fee collection summary |
| Fee Reports | `/fee-reports` | Admin, Accountant | Detailed fee analytics |
| Outstanding Fees | `/outstanding-fees` | Admin, Accountant | Unpaid fee tracking |
| Salary Reports | `/salary-reports` | Admin, Accountant | Salary disbursement summary |
| Staff Salary Comparison | `/staff-salary-comparison` | Admin, Accountant | Compare salaries across staff |
| Budget vs Actual | `/budget-vs-actual` | Admin, Accountant | Budget planning vs actuals |
| Attendance Reports | `/attendance-reports` | Admin, Clerk | Attendance trends and summaries |
| Performance Analytics | `/exams/:id/analytics` | Admin, Clerk, Staff | Exam performance analysis |

---

### 7.20 Student Promotion (`/students/promote`)

**Access**: Admin, Clerk

- Bulk promote students from one class/section to the next for a new academic year
- Select source class/section and target class/section
- Creates new enrollment records in the target academic year

---

### 7.21 Roll Number Management (`/roll-numbers`)

**Access**: Admin, Clerk

- Assign or auto-generate roll numbers for students within a class/section
- Roll numbers are tied to enrollments (class + section + academic year)

---

## 8. Recommended Setup Order (Checklist)

Follow this exact order when setting up SMS in a new environment:

```
[ ] 1.  Deploy application (Docker or manual setup)
[ ] 2.  Register Admin user (via API/Swagger: POST /api/auth/register with Role=Admin)
[ ] 3.  Login to the application
[ ] 4.  Configure School Settings (Admin → Settings)
        [ ] Basic Information (School Name, Code, Address, Contact)
        [ ] Branding (Logo, Colors, Header/Footer Text)
        [ ] Preferences (Date Format, Currency)
[ ] 5.  Create Academic Year (Admin → Academic Years)
        [ ] Set one year as "Is Current"
[ ] 6.  Verify correct session is selected in the header dropdown
[ ] 7.  Create Departments (Academic → Departments)
[ ] 8.  Create Classes with Sections (Academic → Classes)
[ ] 9.  Create Subjects (Academic → Subjects)
[ ] 10. Add Staff Members (Academic → Staff Management)
        [ ] Assign departments and designations
        [ ] Add qualifications
[ ] 11. Configure Grade System (if using exams)
[ ] 12. Enroll Students (Academic → Students)
        [ ] Assign to Class, Section, Academic Year
[ ] 13. Assign Roll Numbers (Academic → Roll Numbers)
[ ] 14. Create Fee Structures (Finance → Fees)
        [ ] Define categories and amounts
        [ ] Assign fees to students
[ ] 15. Set Up Transport Routes (Academic → Transport) — if applicable
        [ ] Create routes with stops
        [ ] Register vehicles
        [ ] Assign students to routes
[ ] 16. Create Salary Structures (Payroll → Salary Structures) — if applicable
        [ ] Define pay components
        [ ] Assign structures to staff
[ ] 17. Configure Timetable (Academic → Timetable) — if applicable
        [ ] Define time slots
        [ ] Create timetable entries
[ ] 18. Add Holidays (Academic → Holidays)
[ ] 19. Set Up Notification Templates (Settings → Notifications) — if applicable
[ ] 20. Create additional user accounts for Accountant, Clerk, and Teachers
        [ ] Use POST /api/auth/register with appropriate Role values
```

> [!IMPORTANT]
> **Dependencies**: Students cannot be enrolled without Classes & Sections. Fees cannot be assigned without Fee Structures. Exams need Subjects. Follow the order above to avoid issues.

---

## Authentication Quick Reference

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/auth/register` | POST | Register new user (Username, Email, Password, FirstName, LastName, Role) |
| `/api/auth/login` | POST | Login (Username, Password) → returns JWT access token + refresh token |
| `/api/auth/refresh` | POST | Refresh expired access token |
| `/api/auth/me` | GET | Get current user info (requires auth) |
| `/api/auth/change-password` | POST | Change password (requires auth) |
| `/api/auth/forgot-password` | POST | Request password reset email |
| `/api/auth/reset-password` | POST | Reset password with token |
| `/api/auth/logout` | POST | Logout and revoke refresh token |

**Role values for registration**: `Admin = 1`, `Accountant = 2`, `Clerk = 3`, `Teacher = 4`

---

## API Documentation

Full interactive API documentation is available at **http://localhost:5208/swagger** when the backend is running. This includes all endpoints, request/response models, and the ability to test API calls directly.

---

*Last updated: 2026-04-23*
