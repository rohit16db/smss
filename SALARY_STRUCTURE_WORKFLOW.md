# 💰 Salary Structure Module - Complete Workflow

## 📋 Table of Contents
1. [System Architecture](#system-architecture)
2. [Data Model](#data-model)
3. [Complete User Workflow](#complete-user-workflow)
4. [API Endpoints](#api-endpoints)
5. [Frontend Pages](#frontend-pages)
6. [Example Scenarios](#example-scenarios)

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         FRONTEND (React)                             │
├─────────────────────────────────────────────────────────────────────┤
│  SalaryStructurePage │ TeacherSalaryAssignmentPage │ BulkProcessing  │
└────────────────────────────────────┬────────────────────────────────┘
                                     │ (HTTP Requests)
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    BACKEND (ASP.NET Core + MediatR)                 │
├─────────────────────────────────────────────────────────────────────┤
│  Controllers → Handlers (Commands/Queries) → Business Logic         │
│              → Entity Framework Core → Database                     │
└────────────────────────────────────┬────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    DATABASE (PostgreSQL)                             │
├─────────────────────────────────────────────────────────────────────┤
│  SalaryStructures Table │ Teachers Table (linked via FK)            │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 💾 Data Model

### **SalaryStructure Entity** (Master Data)
```
┌─────────────────────────────────────────┐
│        SalaryStructure                  │
├─────────────────────────────────────────┤
│ Fields:                                 │
│  • Id (PK)                              │
│  • Name (e.g., "Senior Teacher")       │
│  • BaseSalary: 50,000                   │
│  • HRA: 5,000                           │
│  • DA: 3,000                            │
│  • MedicalAllowance: 1,000              │
│  • ConveyanceAllowance: 2,000           │
│  • OtherAllowances: 500                 │
│  • StandardDeduction: 2,500             │
│  • MinExperienceYears: 3                │
│  • ApplicableQualifications: "B.Ed"     │
│  • IsActive: true                       │
│  • EffectiveFromDate: 2026-01-01        │
│  • EffectiveToDate: NULL (ongoing)      │
│  • CreatedAt, UpdatedAt, CreatedBy      │
└─────────────────────────────────────────┘

Calculated Fields:
├─ TotalAllowances = HRA + DA + Medical + Conveyance + Other
│                  = 5000 + 3000 + 1000 + 2000 + 500 = 11,500
│
└─ GrossSalary = BaseSalary + TotalAllowances - StandardDeduction
               = 50,000 + 11,500 - 2,500 = 59,000
```

### **Teacher Entity** (Modified)
```
┌─────────────────────────────────────────┐
│           Teacher                       │
├─────────────────────────────────────────┤
│ New Fields (for salary):                │
│  • SalaryStructureId (FK): UUID         │
│  • SalaryStructure (Navigation)         │
│  • SalaryStructureEffectiveDate: date   │
│                                         │
│ Existing Fields:                        │
│  • Id, FirstName, LastName, Email       │
│  • ExperienceYears: 5                   │
│  • IsActive: true                       │
│  • ... other fields                     │
└─────────────────────────────────────────┘

Relationship:
Teacher (Many) ──FK: SalaryStructureId──> SalaryStructure (One)
                      (Optional, SET NULL on delete)
```

### **SalaryPayment Entity** (Created during bulk processing)
```
┌─────────────────────────────────────────┐
│        SalaryPayment                    │
├─────────────────────────────────────────┤
│ • Id, TeacherId (FK)                    │
│ • PeriodStartDate, PeriodEndDate        │
│ • BaseSalary: 50,000                    │
│ • Allowances: 11,500                    │
│ • Deductions: 2,500 + 3,000 (extra)     │
│ • NetSalary: 56,000                     │
│ • Status: Pending/Paid                  │
│ • CreatedAt                             │
└─────────────────────────────────────────┘
```

---

## 🔄 Complete User Workflow

### **PHASE 1: Create Salary Structures** 
*(Admin Portal)*

```
┌──────────────────────────────────────────────────────────────────┐
│ Step 1: Admin opens "Salary Structures" page                     │
│         → Clicks "New Structure" button                          │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 2: Fill form with salary components                        │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Name: "Senior Teacher"                                  │   │
│  │ Base Salary: 50,000                                     │   │
│  │ HRA: 5,000                                              │   │
│  │ DA: 3,000                                               │   │
│  │ Medical: 1,000                                          │   │
│  │ Conveyance: 2,000                                       │   │
│  │ Other: 500                                              │   │
│  │ Deduction: 2,500                                        │   │
│  │ Min Experience: 3 years                                 │   │
│  │ Effective From: 2026-01-01                              │   │
│  │ Gross Salary (auto-calculated): ₹59,000                │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 3: Submit form                                              │
│  Action: POST /api/v1/salarystructure                           │
│  Status: ✓ Salary structure created with ID: abc123xyz...       │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 4: View in table                                            │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Name       │ Base  │ Allow. │ Deduct │ Gross  │ MinExp │   │
│  ├─────────────────────────────────────────────────────────┤   │
│  │ Senior T.  │50k    │ 11.5k  │ 2.5k   │ 59k   │    3   │   │
│  │ Junior T.  │35k    │  8k    │ 2k     │ 41k   │    0   │   │
│  │ Expert T.  │65k    │ 14k    │ 3k     │ 76k   │    8   │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘

📤 API Flow:
   Frontend (Form) 
      ↓ POST {name, baseSalary, hra, da, ...}
   SalaryStructureController.Create()
      ↓ Dispatch CreateSalaryStructureCommand
   CreateSalaryStructureCommandHandler.Handle()
      ↓ Create entity instance
   DbContext.SalaryStructures.Add()
      ↓ Save to Database
   ✓ Return created SalaryStructure DTO to Frontend
```

---

### **PHASE 2: Assign Structures to Teachers**
*(Teacher Assignment Page)*

```
┌──────────────────────────────────────────────────────────────────┐
│ Step 1: Admin opens "Teacher Salary Assignment" page             │
│         → Views summary cards:                                   │
│           • Total Teachers: 25                                   │
│           • Assigned: 10                                         │
│           • Unassigned: 15                                       │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 2: Click "Assign Structure" button                          │
│         → Opens assignment dialog                                │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 3: Select unassigned teacher                               │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Select Teacher:                                         │   │
│  │ ├─ Rajesh Kumar (Exp: 5 years) ◀─ Select this         │   │
│  │ ├─ Priya Singh (Exp: 2 years)                          │   │
│  │ └─ Amit Patel (Exp: 10 years)                          │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 4: System filters applicable structures                     │
│  Backend Logic:                                                  │
│  GET /api/v1/salarystructure/applicable/rajesh-id              │
│     ↓ Query: MinExperienceYears <= 5                            │
│     ↓ Results:                                                  │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Select Salary Structure:                                │   │
│  │ ├─ Junior Teacher (MinExp: 0, Gross: 41k) ✓            │   │
│  │ ├─ Senior Teacher (MinExp: 3, Gross: 59k) ✓            │   │
│  │ └─ Expert Teacher (MinExp: 8, Gross: 76k) ✗ (Too high)│   │
│  │                                                         │   │
│  │ (Shows only structures matching experience level)      │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 5: Select structure & effective date                        │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Salary Structure: Senior Teacher ◀─ Selected           │   │
│  │ Effective From: 2026-02-01       ◀─ Assignment date    │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 6: Click "Assign" button                                    │
│  Action: POST /api/v1/salarystructure/assign-to-teacher        │
│  Payload: {teacherId, salaryStructureId, effectiveDate}        │
│  Status: ✓ Assigned successfully                               │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 7: View updated assignments table                           │
│  Teacher data updated:                                           │
│  Rajesh Kumar:                                                   │
│  ├─ Salary Structure: Senior Teacher                            │
│  ├─ Gross Salary: ₹59,000                                       │
│  ├─ Effective From: 2026-02-01                                  │
│  └─ Status: ✓ Assigned                                          │
│                                                                  │
│  Summary Updated:                                               │
│  • Total Teachers: 25                                           │
│  • Assigned: 11 (was 10)                                        │
│  • Unassigned: 14 (was 15)                                      │
└──────────────────────────────────────────────────────────────────┘

📤 API Flow:
   Frontend (Dialog)
      ↓ POST {teacherId, salaryStructureId, effectiveDate}
   SalaryStructureController.AssignToTeacher()
      ↓ Dispatch AssignSalaryStructureToTeacherCommand
   AssignSalaryStructureToTeacherCommandHandler.Handle()
      ↓ Load Teacher entity
      ↓ Update SalaryStructureId & EffectiveDate
   DbContext.SaveChanges()
      ↓ Save to Database
      ↓ Teacher now linked to SalaryStructure
   ✓ Return assignment confirmation
```

---

### **PHASE 3: Bulk Create Salary Payments**
*(Bulk Salary Processing Page)*

```
┌──────────────────────────────────────────────────────────────────┐
│ Step 1: Admin opens "Bulk Salary Processing" page                │
│         → Views ready teachers: 11 (with assigned structures)    │
│         → Summary:                                               │
│           • Teachers Ready: 11                                   │
│           • Est. Total Salary: ₹649,000                         │
│           • Est. Net Total: ₹600,000                            │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 2: Enter salary period & deductions                         │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Period Start Date: 2026-02-01                           │   │
│  │ Period End Date: 2026-02-28                             │   │
│  │ Fixed Deduction per Teacher: 500 (optional)             │   │
│  │                                                         │   │
│  │ Summary:                                                │   │
│  │ • Teachers Ready: 11                                    │   │
│  │ • Avg Base Salary: ₹59,000                              │   │
│  │ • Total Deductions: 5,500 (11 × 500)                    │   │
│  │ • Net Payable: ₹643,500                                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 3: Click "Create Salary Payments" button                    │
│  Action: POST /api/v1/salarystructure/bulk-create-salaries      │
│  Payload: {periodStartDate, periodEndDate, fixedDeductions}     │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 4: Backend processes (Handler Logic)                        │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ BulkCreateSalaryFromStructuresCommandHandler:           │   │
│  │                                                         │   │
│  │ FOR EACH teacher with assigned structure:              │   │
│  │  1. Fetch Teacher + SalaryStructure data               │   │
│  │  2. Calculate NetSalary:                               │   │
│  │     = GrossSalary - fixedDeductions                    │   │
│  │                                                         │   │
│  │  Example for Rajesh:                                   │   │
│  │  • GrossSalary: 59,000                                 │   │
│  │  • Deductions: 500                                     │   │
│  │  • NetSalary: 58,500                                   │   │
│  │                                                         │   │
│  │  3. Create SalaryPayment record:                       │   │
│  │     {TeacherId, Period, BaseSalary, Deductions,        │   │
│  │      NetSalary, Status: Pending}                       │   │
│  │  4. Save to database                                   │   │
│  │                                                         │   │
│  │ 5. Aggregate totals:                                   │   │
│  │   • TotalTeachers: 11                                  │   │
│  │   • TotalBaseSalary: ₹649,000                          │   │
│  │   • TotalDeductions: ₹5,500                            │   │
│  │   • TotalNetSalary: ₹643,500                           │   │
│  │                                                         │   │
│  │ 6. Return report to Frontend                           │   │
│  └─────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────┘
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│ Step 5: Success view with results                                │
│  ✓ Salary Payments Created Successfully                          │
│                                                                  │
│  Summary Stats:                                                  │
│  ┌────────┬────────────┬────────────┬────────────┐              │
│  │Teachers│Total Base │Total Deduct│Total Net   │              │
│  ├────────┼────────────┼────────────┼────────────┤              │
│  │   11   │ ₹649,000   │ ₹5,500     │ ₹643,500   │              │
│  └────────┴────────────┴────────────┴────────────┘              │
│                                                                  │
│  Detailed Payment Table:                                        │
│  ┌──────────────┬────────┬───────────┬──────────┐              │
│  │Teacher       │Base    │Deductions │Net       │              │
│  ├──────────────┼────────┼───────────┼──────────┤              │
│  │Rajesh Kumar  │59k     │500        │58,500    │              │
│  │Priya Singh   │41k     │500        │40,500    │              │
│  │Amit Patel    │59k     │500        │58,500    │              │
│  │...           │...     │...        │...       │              │
│  └──────────────┴────────┴───────────┴──────────┘              │
│                                                                  │
│  [Create Another Batch] button                                  │
└──────────────────────────────────────────────────────────────────┘

📤 API Flow:
   Frontend (Form)
      ↓ POST {periodStartDate, periodEndDate, fixedDeductions}
   SalaryStructureController.BulkCreateSalaries()
      ↓ Dispatch BulkCreateSalaryFromStructuresCommand
   BulkCreateSalaryFromStructuresCommandHandler.Handle()
      ↓ Query all active teachers with structures
      ↓ FOR EACH teacher:
         - Calculate NetSalary
         - Create SalaryPayment record
         - Add to database
      ↓ Calculate aggregates
   DbContext.SaveChanges()
      ↓ All salary records saved
   ✓ Return SalaryPaymentReportDto with results
```

---

## 🔌 API Endpoints

### **SalaryStructure Endpoints**

| Method | Endpoint | Purpose | Example |
|--------|----------|---------|---------|
| **GET** | `/api/v1/salarystructure` | List all structures | List for management table |
| **GET** | `/api/v1/salarystructure/{id}` | Get by ID | View structure details |
| **GET** | `/api/v1/salarystructure/applicable/{teacherId}` | Get applicable by teacher | Show eligible structures |
| **GET** | `/api/v1/salarystructure/teacher/{teacherId}/current` | Current assignment | Show assigned structure |
| **GET** | `/api/v1/salarystructure/teachers/assignments` | All assignments | View all teacher-structure links |
| **POST** | `/api/v1/salarystructure` | Create new | Create new structure |
| **PUT** | `/api/v1/salarystructure/{id}` | Update | Edit structure |
| **DELETE** | `/api/v1/salarystructure/{id}` | Delete | Remove structure |
| **POST** | `/api/v1/salarystructure/assign-to-teacher` | Assign | Link teacher to structure |
| **POST** | `/api/v1/salarystructure/bulk-create-salaries` | Bulk create | Generate salary payments |

---

## 🌐 Frontend Pages

### **1. Salary Structures Page** (`/salary-structures`)
**Purpose**: Create and manage salary scales

**Functions**:
- ✅ View all salary structures in table
- ✅ Create new salary structure (dialog form)
- ✅ Edit existing structure
- ✅ Delete structure
- ✅ Real-time gross salary calculation
- ✅ Status tracking (Active/Inactive)

**User Flow**:
```
Admin → View Structures → [New/Edit/Delete] → Table Updates
```

---

### **2. Teacher Salary Assignment Page** (`/teacher-salary-assignment`)
**Purpose**: Assign salary structures to teachers

**Functions**:
- ✅ View summary (Total, Assigned, Unassigned teachers)
- ✅ Assign structure to unassigned teacher
- ✅ Intelligent filtering (only show applicable structures by experience)
- ✅ Track all assignments with effective dates
- ✅ View gross salary per teacher

**User Flow**:
```
Admin → Select Teacher → Filter Applicable Structures 
        → Select Structure → Set Effective Date → Assign
        → View in Assignment Table
```

**Smart Filtering Logic**:
```
IF Teacher.ExperienceYears >= Structure.MinExperienceYears
  → Show Structure in dropdown
ELSE
  → Hide (not applicable)

Example:
Teacher with 5 years experience:
├─ Can assign: Junior (0+), Senior (3+) ✓
└─ Cannot assign: Expert (8+) ✗
```

---

### **3. Bulk Salary Processing Page** (`/bulk-salary-processing`)
**Purpose**: Generate salary payments in bulk

**Functions**:
- ✅ Select salary period (start & end date)
- ✅ Set fixed deductions per teacher
- ✅ Preview calculations
- ✅ Create all salary payments at once
- ✅ View detailed payment report with aggregates
- ✅ Create multiple batches

**User Flow**:
```
Admin → Enter Period & Deductions → Preview Calculations
        → Create Payments → View Results → Create Another Batch
```

**Calculation Example**:
```
Teacher: Rajesh Kumar
├─ Associated Structure: Senior Teacher
├─ Base Salary: 50,000
├─ Allowances: 11,500 (HRA+DA+etc)
├─ Standard Deduction: 2,500
├─ Gross Salary: 59,000 (50k+11.5k-2.5k)
├─ Fixed Deduction (input): 500
└─ Net Salary: 58,500 (59k-500)

Total for 11 teachers:
├─ Total Base: 649,000
├─ Total Deductions: 5,500 (11 × 500)
└─ Total Net: 643,500
```

---

## 📚 Example Scenarios

### **Scenario 1: New School Year Setup**

```
TIMELINE:

January 2026:
└─ Admin creates 4 salary structures:
   ├─ Junior Teacher (0+yrs): Base 35k, Gross 41k
   ├─ Senior Teacher (3+yrs): Base 50k, Gross 59k
   ├─ Expert Teacher (8+yrs): Base 65k, Gross 76k
   └─ Specialist Teacher (5+yrs): Base 60k, Gross 71k

February 2026:
└─ Admin assigns structures to 25 teachers:
   ├─ 5 teachers → Junior
   ├─ 12 teachers → Senior
   ├─ 6 teachers → Expert
   └─ 2 teachers → Specialist

February 1, 2026 - Salary Processing:
└─ Admin creates bulk salary for Feb:
   ├─ Period: 2026-02-01 to 2026-02-28
   ├─ Fixed Deduction: 500 (for PF/advance)
   └─ Output:
      • 25 salary payments created
      • Total salary: ₹1,488,500
      • All marked as "Pending" for review
      • Ready for approval & payment
```

---

### **Scenario 2: Mid-Year Promotion**

```
TIMELINE:

January 2026:
└─ Rajesh Kumar → Junior Teacher (0yrs)
   └─ Salary: 41,000

March 2026 - Rajesh gets promoted + 3 years experience:
└─ Admin reassigns:
   ├─ Opens "Teacher Salary Assignment"
   ├─ Searches: Rajesh Kumar
   ├─ Selected Structure: Senior Teacher (59,000)
   ├─ Effective From: 2026-03-15
   └─ Saved ✓

March 15, 2026 - Salary Processing:
└─ Admin creates salary:
   ├─ Period: 2026-03-15 to 2026-03-31 (partial)
   ├─ Rajesh's new salary included: 59,000
   └─ Rest of salary (3/15-3/14): 41,000
   
Final: Rajesh gets blended salary + promotion increase
```

---

### **Scenario 3: Structure Modification**

```
TIMELINE:

January 2026:
└─ Senior Teacher Structure:
   ├─ Base: 50,000
   ├─ HRA: 5,000
   ├─ Gross: 59,000

May 2026 - Annual increment:
└─ Admin edits structure:
   ├─ Open SalaryStructurePage
   ├─ Click Edit on "Senior Teacher"
   ├─ Update:
   │  ├─ Base: 50,000 → 52,500 (5% raise)
   │  ├─ HRA: 5,000 → 5,250
   │  └─ New Gross: 61,250 (shown real-time)
   ├─ Set Effective: 2026-05-01
   └─ Save ✓

June 2026 onwards:
└─ All teachers using "Senior Teacher" structure:
   └─ Get salary with new rates automatically
```

---

## 🔐 Data Integrity & Relationships

```
One-to-Many Relationship:
┌──────────────────────┐         ┌────────────────────┐
│  SalaryStructure     │◄────────┤  Teacher           │
├──────────────────────┤    FK   ├────────────────────┤
│ Id (PK)              │ (1:N)   │ Id (PK)            │
│ Name                 │         │ FirstName          │
│ BaseSalary           │         │ SalaryStructureId  │
│ ... fields           │         │ SalaryStructure(nav)
│ GrossSalary (calc)   │         │ ... other fields   │
└──────────────────────┘         └────────────────────┘

Deletion Handling (CASCADE SET NULL):
- Delete SalaryStructure → Teacher.SalaryStructureId = NULL
- Teacher remains in system but unassigned
- No salary generated until reassigned
```

---

## ⚙️ Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | React 18 + TypeScript | UI components & state |
| **State Management** | React Query | Server state, caching |
| **Styling** | Tailwind CSS | Modern responsive UI |
| **Backend** | ASP.NET Core 10 | API & business logic |
| **Pattern** | MediatR CQRS | Clean architecture |
| **ORM** | Entity Framework Core | Database abstraction |
| **Database** | PostgreSQL | Data persistence |
| **API Style** | RESTful + Commands | Standard HTTP + DDD |

---

## 📊 Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                  ADMIN USER                                 │
│            (SalaryStructurePage)                            │
└──────────────────────┬──────────────────────────────────────┘
                       │ CREATE/UPDATE/DELETE
                       ▼
┌─────────────────────────────────────────────────────────────┐
│            Frontend - React Components                      │
│  ├─ SalaryStructurePage                                     │
│  ├─ TeacherSalaryAssignmentPage                            │
│  └─ BulkSalaryProcessingPage                               │
└──────────────────────┬──────────────────────────────────────┘
                       │ HTTP POST/GET/PUT/DELETE
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         Backend - ASP.NET Core Controllers                  │
│         SalaryStructureController                           │
└──────────────────────┬──────────────────────────────────────┘
                       │ Dispatch Commands/Queries
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         MediatR Handler Layer (CQRS)                        │
│  ├─ CreateSalaryStructureCommandHandler                     │
│  ├─ AssignSalaryStructureToTeacherCommandHandler           │
│  ├─ BulkCreateSalaryFromStructuresCommandHandler           │
│  └─ GetSalaryStructureQueryHandlers (5 queries)            │
└──────────────────────┬──────────────────────────────────────┘
                       │ Business Logic & Calculations
                       ▼
┌─────────────────────────────────────────────────────────────┐
│    Entity Framework Core - DbContext                        │
│    (SalaryStructures, Teachers, SalaryPayments)            │
└──────────────────────┬──────────────────────────────────────┘
                       │ SQL Queries
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         PostgreSQL Database                                 │
│  ├─ SalaryStructures Table                                 │
│  ├─ Teachers Table                                         │
│  └─ SalaryPayments Table                                   │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Key Concepts

### **Gross Salary Calculation**
```
GrossSalary = BaseSalary + AllowanceTotal - StandardDeduction

AllowanceTotal = HRA + DA + MedicalAllowance 
                + ConveyanceAllowance + OtherAllowances

Example:
50,000 + (5000 + 3000 + 1000 + 2000 + 500) - 2,500 = 59,000
```

### **Net Salary Calculation (During Payment)**
```
NetSalary = GrossSalary - AdditionalDeductions

Example:
59,000 - 500 (fixed) = 58,500
```

### **Experience-Based Filtering**
```
Teacher Qualification Check:
IF Teacher.ExperienceYears >= Structure.MinExperienceYears
  → Structure is applicable
ELSE
  → Structure cannot be assigned

Ensures:
- Juniors don't get senior salaries
- Experts get appropriate salaries
- Fair experience-based compensation
```

---

## ✅ Workflow Summary

```
┌─────────────────┐
│  Start: Phase 1 │
│ Create Structure│
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────────┐
│ Admin creates salary structures     │
│ (form: components, allowances, etc) │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────┐
│   Phase 2       │
│ Assign Teachers │
└────────┬────────┘
         │
         ▼
┌──────────────────────────────────────┐
│ Admin assigns structures to teachers │
│ (experience-based filtering)         │
└────────┬─────────────────────────────┘
         │
         ▼
┌──────────────────────┐
│    Phase 3           │
│ Bulk Create Salaries │
└────────┬─────────────┘
         │
         ▼
┌────────────────────────────────────────┐
│ Admin creates salary payments for all  │
│ assigned teachers for a period         │
└────────┬─────────────────────────────────┘
         │
         ▼
┌─────────────────────────┐
│ End: Salary Payments    │
│ Created & Ready for Pay │
└─────────────────────────┘
```

---

## 🚀 Quick Start Example

**Day 1 - Create Structures**:
1. Go to `/salary-structures`
2. Click "New Structure"
3. Enter: Name="Teacher", Base=40000, HRA=4000, etc.
4. Submit → Structure created ✓

**Day 2 - Assign Teachers**:
1. Go to `/teacher-salary-assignment` 
2. Click "Assign Structure"
3. Select: Teacher "Rajesh", Structure "Teacher", Date "2026-02-01"
4. Submit → Assigned ✓

**Day 3 - Generate Salaries**:
1. Go to `/bulk-salary-processing`
2. Set: Period "2026-02-01 to 2026-02-28", Deduction "500"
3. Click "Create Salary Payments"
4. View results → All salaries generated ✓

---

## 🔗 Navigation
- Dashboard → Finance (dropdown) → Salary Structures
- Dashboard → Finance (dropdown) → Teacher Assignments  
- Dashboard → Finance (dropdown) → Bulk Processing
