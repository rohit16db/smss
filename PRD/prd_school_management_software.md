# Product Requirements Document (PRD)

## 1. Product Overview

### Product Name
School Management Software (SMS)

### Purpose
To design and build a **cost-effective, scalable school management system** for a small school (~200 students) that digitizes administrative operations and can scale as the school grows.

### Target Users
- **Primary User:** School Administrator / Owner
- **Future Users (Phase 2+):** Accountant, Office Staff, Teachers, Parents, Students

### User Access (Phase 1)
- Admin-only access
- Single or very limited concurrent users

---

## 2. Goals & Objectives

### Business Goals
- Replace manual / Excel-based school administration
- Reduce operational errors in fees and salary handling
- Maintain very low monthly operational cost
- Ensure long-term scalability without system rewrite

### Technical Goals
- Use modern, stable technologies
- Maintain full control over infrastructure and data
- Enable easy migration to higher hosting environments

---

## 3. Technology Stack (Fixed)

### Backend
- ASP.NET Core Web API (.NET 8)
- Clean Architecture / Modular Monolith
- Entity Framework Core
- JWT-based Authentication

### Frontend
- React (Admin Dashboard)
- Vite
- Material UI / Ant Design
- React Query

### Database
- PostgreSQL
- EF Core Migrations

### Hosting (Initial)
- Backend: Render / Railway / DigitalOcean
- Database: Supabase Postgres / Managed Postgres
- Frontend: Netlify / Vercel

---

## 4. Functional Requirements (Phase 1 – MVP)

### 4.1 User & Authentication
- Admin login
- Secure password storage
- Session management using JWT

---

### 4.2 Student Management
- Create, update, view, and deactivate students
- Student profile:
  - Name, DOB, Gender
  - Class / Section
  - Parent / Guardian contact details
  - Admission date
- Student status: Active / Inactive / Passed Out

---

### 4.3 Teacher & Staff Management
- Teacher profile management
- Qualification and experience details
- Joining date
- Assigned classes / subjects
- Active / Inactive status

---

### 4.4 Course / Class Management
- Class / Grade creation
- Subject management
- Teacher assignment to classes
- Academic year support

---

### 4.5 Fee Management
- Fee structure definition (monthly / quarterly / yearly)
- Fee categories (tuition, transport, misc.)
- Student-wise fee assignment
- Fee payment entry
- Paid / Pending / Overdue tracking
- Manual receipt number entry

---

### 4.6 Salary Management
- Salary structure per teacher/staff
- Monthly salary calculation
- Salary payment tracking
- Paid / Pending status
- Salary history

---

### 4.7 Dashboard & Reports
- Total students count
- Monthly fee collection summary
- Outstanding fee list
- Monthly salary expense summary

---

## 5. Additional Features (Recommended Enhancements)

### 5.1 Attendance Management (Phase 2)
- Daily student attendance
- Teacher attendance
- Monthly attendance reports

### 5.2 Exam & Marks Management (Phase 2)
- Exam creation
- Subject-wise marks entry
- Simple report card generation

### 5.3 Notifications (Phase 3)
- Fee due reminders
- Salary payment notifications
- Admin alerts

### 5.4 Document Management
- Upload student documents
- Teacher certificates

### 5.5 Role-Based Access Control (Future)
- Admin
- Accountant
- Clerk
- Teacher

---

## 6. Non-Functional Requirements

### Performance
- Support up to 1,000 students without architecture change
- Low latency for admin operations

### Security
- HTTPS mandatory
- Encrypted passwords
- Role-based access (future-ready)

### Reliability
- Automated daily database backups
- Error logging & monitoring

### Scalability
- Vertical scaling initially
- Horizontal scaling later via containerization

---

## 7. Data Model (High-Level Entities)

- Users
- Students
- Teachers
- Classes
- Subjects
- StudentClasses
- Fees
- FeePayments
- Salaries
- SalaryPayments
- Attendance

---

## 8. Deployment Strategy

### Phase 1
- Single backend API
- Single PostgreSQL instance
- Static frontend hosting

### Phase 2
- Enable caching (Redis)
- Introduce background jobs

### Phase 3
- Separate read/write workloads
- Parent & student portals

---

## 9. Development Milestones

### Week 1
- Architecture & DB design
- Auth + core entities

### Week 2
- Fee & salary management
- Reports

### Week 3
- UI polish
- Deployment

### Week 4
- Security hardening
- Documentation

---

## 10. Success Metrics

- Admin can manage school without Excel
- Fee and salary errors reduced
- Monthly operational cost < ₹1,000
- System supports growth without rewrite

---

## 11. Future Roadmap

- Parent mobile app
- Online fee payment integration
- SMS / WhatsApp notifications
- Multi-school support

---

**End of PRD**

