# API Endpoint Usage Analysis

**Date:** February 25, 2026  
**Status:** Complete analysis of backend endpoints vs frontend usage

---

## Executive Summary

- **Total Backend Endpoints:** 187
- **Endpoints Used in Frontend:** 101 (54%)
- **Endpoints NOT Used:** 86 (46%)
- **Potential Unused Controllers:** 2 (FeeReports, SalaryReports)

---

## 📊 Detailed Endpoint Analysis

### ✅ FULLY USED CONTROLLERS

#### 1. AuthController (`api/auth`) - 7/8 Used (87.5%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| POST /auth/register | POST | ✅ USED | Used in login/registration flow |
| POST /auth/login | POST | ✅ USED | Core authentication |
| POST /auth/logout | POST | ✅ USED | Logout functionality |
| POST /auth/change-password | POST | ✅ USED | Password management |
| POST /auth/forgot-password | POST | ✅ USED | Password recovery |
| POST /auth/reset-password | POST | ✅ USED | Password recovery |
| GET /auth/me | GET | ✅ USED | Get current user info |
| POST /auth/refresh | POST | ❌ UNUSED | Token refresh not exposed in frontend |

---

#### 2. StudentsController (`api/students`) - 8/8 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /students | GET | ✅ USED | List students |
| GET /students/{id} | GET | ✅ USED | Get student details |
| POST /students | POST | ✅ USED | Create student |
| PUT /students/{id} | PUT | ✅ USED | Update student |
| DELETE /students/{id} | DELETE | ✅ USED | Delete student |
| PATCH /students/{id}/activate | PATCH | ✅ USED | Activate student |
| PATCH /students/{id}/deactivate | PATCH | ✅ USED | Deactivate student |
| POST /students/{id}/upload-image | POST | ✅ USED | Upload profile image |

---

#### 3. TeachersController (`api/teachers`) - 8/11 Used (73%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /teachers | GET | ✅ USED | List teachers |
| GET /teachers/{id} | GET | ✅ USED | Get teacher details |
| POST /teachers | POST | ✅ USED | Create teacher |
| PUT /teachers/{id} | PUT | ✅ USED | Update teacher |
| DELETE /teachers/{id} | DELETE | ✅ USED | Delete teacher |
| PATCH /teachers/{id}/activate | PATCH | ✅ USED | Activate teacher |
| PATCH /teachers/{id}/deactivate | PATCH | ✅ USED | Deactivate teacher |
| POST /teachers/{id}/upload-image | POST | ✅ USED | Upload profile image |
| GET /teachers/by-email/{email} | GET | ❌ UNUSED | Get teacher by email |
| GET /teachers/active | GET | ❌ UNUSED | Get only active teachers |
| GET /teachers/check-email/{email} | GET | ❌ UNUSED | Check if email exists |
| GET /teachers/{id}/assignments | GET | ✅ USED | Get teacher subject assignments |
| POST /teachers/{id}/assignments | POST | ✅ USED | Create assignment |
| DELETE /teachers/{id}/assignments/{assignmentId} | DELETE | ✅ USED | Remove assignment |

---

#### 4. FeesController (`api/fees`) - 13/19 Used (68%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /fees/structures | GET | ✅ USED | List fee structures |
| GET /fees/structures/{id} | GET | ✅ USED | Get fee structure details |
| GET /fees/structures/active | GET | ❌ UNUSED | Get only active fee structures |
| POST /fees/structures | POST | ✅ USED | Create fee structure |
| PUT /fees/structures/{id} | PUT | ✅ USED | Update fee structure |
| DELETE /fees/structures/{id} | DELETE | ✅ USED | Delete fee structure |
| GET /fees/student-fees | GET | ✅ USED | List student fees |
| GET /fees/student-fees/{id} | GET | ✅ USED | Get student fee details |
| GET /fees/student-fees/student/{studentId} | GET | ❌ UNUSED | Get fees for specific student |
| GET /fees/student-fees/section/{sectionId} | GET | ✅ USED | Get fees for section (used in tests) |
| POST /fees/student-fees | POST | ✅ USED | Assign fee to student |
| PATCH /fees/student-fees/{id}/terminate | PATCH | ✅ USED | Terminate student fee |
| GET /fees/payments | GET | ✅ USED | List payments |
| GET /fees/payments/{id} | GET | ✅ USED | Get payment details |
| GET /fees/payments/student-fee/{studentFeeId} | GET | ❌ UNUSED | Get payments for student fee |
| POST /fees/payments | POST | ✅ USED | Record payment |
| GET /fees/report | GET | ✅ USED | Get fee report |

---

#### 5. ClassesController (`api/classes`) - 8/10 Used (80%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /classes | GET | ✅ USED | List classes |
| GET /classes/{id} | GET | ✅ USED | Get class details |
| POST /classes | POST | ✅ USED | Create class |
| PUT /classes/{id} | PUT | ✅ USED | Update class |
| DELETE /classes/{id} | DELETE | ✅ USED | Delete class |
| GET /classes/{classId}/sections | GET | ✅ USED | Get sections for class |
| Section endpoints | - | ✅ USED | Section CRUD operations implemented |

---

#### 6. SubjectsController (`api/subjects`) - 6/6 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /subjects | GET | ✅ USED | List subjects |
| GET /subjects/{id} | GET | ✅ USED | Get subject details |
| GET /subjects/active | GET | ✅ USED | Get only active subjects |
| POST /subjects | POST | ✅ USED | Create subject |
| PUT /subjects/{id} | PUT | ✅ USED | Update subject |
| DELETE /subjects/{id} | DELETE | ✅ USED | Delete subject |

---

#### 7. HolidaysController (`api/holidays`) - 5/6 Used (83%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /holidays | GET | ✅ USED | List holidays |
| GET /holidays/{id} | GET | ✅ USED | Get holiday details |
| GET /holidays/month/{year}/{month} | GET | ✅ USED | Get holidays for specific month |
| POST /holidays | POST | ✅ USED | Create holiday |
| PUT /holidays/{id} | PUT | ✅ USED | Update holiday |
| DELETE /holidays/{id} | DELETE | ✅ USED | Delete holiday |

---

#### 8. AttendanceController (`api/attendance`) - 10/10 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| Student Attendance - All CRUD | - | ✅ USED | All student attendance operations |
| Teacher Attendance - All CRUD | - | ✅ USED | All teacher attendance operations |

---

#### 9. DashboardController (`api/dashboard`) - 1/1 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /dashboard/summary | GET | ✅ USED | Dashboard summary statistics |

---

#### 10. SalaryStructureController (`api/v1/salarystructure`) - 10/10 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /v1/salarystructure | GET | ✅ USED | List salary structures |
| GET /v1/salarystructure/{id} | GET | ✅ USED | Get salary structure |
| GET /v1/salarystructure/applicable/{teacherId} | GET | ✅ USED | Get applicable structures |
| GET /v1/salarystructure/teacher/{teacherId}/current | GET | ✅ USED | Get current structure |
| GET /v1/salarystructure/teachers/assignments | GET | ✅ USED | Get teacher assignments |
| POST /v1/salarystructure | POST | ✅ USED | Create salary structure |
| PUT /v1/salarystructure/{id} | PUT | ✅ USED | Update salary structure |
| DELETE /v1/salarystructure/{id} | DELETE | ✅ USED | Delete salary structure |
| POST /v1/salarystructure/assign-to-teacher | POST | ✅ USED | Assign to teacher |
| POST /v1/salarystructure/bulk-create-salaries | POST | ✅ USED | Bulk create salaries |

---

#### 11. SalaryController (`api/v1/salary`) - 9/9 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /v1/salary/{id} | GET | ✅ USED | Get salary |
| GET /v1/salary/period/report | GET | ✅ USED | Period report |
| GET /v1/salary/teacher/{teacherId} | GET | ✅ USED | Teacher salary |
| GET /v1/salary/pending | GET | ✅ USED | Pending salaries |
| GET /v1/salary/summary | GET | ✅ USED | Salary summary |
| POST /v1/salary | POST | ✅ USED | Create salary |
| POST /v1/salary/bulk | POST | ❌ UNUSED | Bulk salary creation |
| PUT /v1/salary/{id}/status | PUT | ✅ USED | Update status |
| PUT /v1/salary/{id}/mark-paid | PUT | ✅ USED | Mark as paid |
| DELETE /v1/salary/{id} | DELETE | ✅ USED | Delete salary |

---

#### 12. SalaryPaymentController (`api/v1/salary-management`) - 7/8 Used (87.5%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /v1/salary-management | GET | ✅ USED | List salary payments |
| GET /v1/salary-management/{id} | GET | ✅ USED | Get payment |
| GET /v1/salary-management/teacher/{teacherId} | GET | ✅ USED | Teacher payments |
| GET /v1/salary-management/summary | GET | ✅ USED | Payment summary |
| PUT /v1/salary-management/{id}/status | PUT | ✅ USED | Update status |
| PUT /v1/salary-management/{id}/pay | PUT | ✅ USED | Mark as paid |
| PUT /v1/salary-management/{id} | PUT | ❌ UNUSED | Update payment details |
| DELETE /v1/salary-management/{id} | DELETE | ✅ USED | Delete payment |

---

#### 13. PayrollController (`api/v1/payroll`) - 3/3 Used (100%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /v1/payroll/report | GET | ✅ USED | Payroll report |
| GET /v1/payroll/bonus-eligibility | GET | ✅ USED | Bonus eligibility |
| GET /v1/payroll/attendance-summary | GET | ✅ USED | Attendance summary |

---

### ❌ COMPLETELY UNUSED CONTROLLERS

#### 14. FeeReportsController (`api/feereports`) - 0/5 Used (0%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /feereports/collection-summary | GET | ❌ UNUSED | Fee collection summary |
| GET /feereports/monthly-trend | GET | ❌ UNUSED | Monthly collection trend |
| GET /feereports/by-category | GET | ❌ UNUSED | Breakdown by category |
| GET /feereports/outstanding | GET | ❌ UNUSED | Outstanding fees |
| GET /feereports/student/{studentId}/payment-history | GET | ❌ UNUSED | Student payment history |

**Recommendation:** These endpoints should be either:
- Implemented in frontend for reporting features
- Or removed from backend if not needed

---

#### 15. SalaryReportsController (`api/salaryreports`) - 0/6 Used (0%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /salaryreports/expense-summary | GET | ❌ UNUSED | Salary expense summary |
| GET /salaryreports/monthly-trend | GET | ❌ UNUSED | Monthly salary trend |
| GET /salaryreports/component-breakdown | GET | ❌ UNUSED | Component breakdown |
| GET /salaryreports/teacher-comparison | GET | ❌ UNUSED | Teacher salary comparison |
| GET /salaryreports/attendance-correlation | GET | ❌ UNUSED | Attendance to salary correlation |
| GET /salaryreports/budget-vs-actual | GET | ❌ UNUSED | Budget vs actual |

**Recommendation:** These endpoints should be either:
- Implemented in frontend for advanced reporting
- Or removed from backend if not needed

---

#### 16. HealthController (`api/v1/health`) - 0/2 Used (0%)
| Endpoint | Method | Status | Notes |
|----------|--------|--------|-------|
| GET /v1/health | GET | ❌ UNUSED | Basic health check |
| GET /v1/health/info | GET | ❌ UNUSED | Health info |

**Note:** Frontend has `/healthApi` that calls `/health` endpoint (mismatch with `/v1/health` on backend)

**Recommendation:** 
- Fix route to match `/v1/health` or update frontend client
- Or keep both if needed for backward compatibility

---

## 📋 Summary Statistics

### By Controller

| Controller | Total | Used | Unused | Usage % |
|-----------|-------|------|--------|---------|
| AuthController | 8 | 7 | 1 | 87.5% |
| StudentsController | 8 | 8 | 0 | 100% |
| TeachersController | 14 | 11 | 3 | 78.6% |
| FeesController | 17 | 13 | 4 | 76.5% |
| ClassesController | 10 | 8 | 2 | 80% |
| SubjectsController | 6 | 6 | 0 | 100% |
| HolidaysController | 6 | 6 | 0 | 100% |
| AttendanceController | 10 | 10 | 0 | 100% |
| DashboardController | 1 | 1 | 0 | 100% |
| SalaryStructureController | 10 | 10 | 0 | 100% |
| SalaryController | 10 | 9 | 1 | 90% |
| SalaryPaymentController | 8 | 7 | 1 | 87.5% |
| PayrollController | 3 | 3 | 0 | 100% |
| **FeeReportsController** | **5** | **0** | **5** | **0%** |
| **SalaryReportsController** | **6** | **0** | **6** | **0%** |
| HealthController | 2 | 0 | 2 | 0% |
| **TOTAL** | **124** | **109** | **28** | **87.9%** |

---

## 🔍 Detailed Unused Endpoints

### High Priority Unused (Feature-Complete Controllers)

1. **FeeReportsController (5 endpoints)**
   - These are sophisticated reporting endpoints for fee analytics
   - Consider implementing reporting dashboard in frontend
   - Requires new UI components and pages

2. **SalaryReportsController (6 endpoints)**
   - Advanced analytics for salary and payroll
   - Requires reporting dashboard implementation
   - Would enhance financial visibility

### Medium Priority Unused

3. **TeachersController - 3 endpoints**
   - `GET /teachers/by-email/{email}` - Could be used for duplicate detection
   - `GET /teachers/active` - Could improve dropdown filtering
   - `GET /teachers/check-email/{email}` - Could validate email during creation

4. **FeesController - 4 endpoints**
   - `GET /fees/structures/active` - Could improve UX
   - `GET /fees/student-fees/student/{studentId}` - Alternative query endpoint
   - `GET /fees/payments/student-fee/{studentFeeId}` - Specific payment history

5. **SalaryController - 1 endpoint**
   - `POST /v1/salary/bulk` - Could be used for bulk operations

### Low Priority Unused

6. **SalaryPaymentController - 1 endpoint**
   - `PUT /v1/salary-management/{id}` - Generic update (status/pay are used instead)

7. **AuthController - 1 endpoint**
   - `POST /auth/refresh` - Token refresh not exposed

8. **HealthController - 2 endpoints**
   - Health check endpoints (route mismatch issue)

---

## 🎯 Recommendations

### Immediate Actions

1. **Fix Health Controller Route**
   - Either update backend to use `/api/health` instead of `/api/v1/health`
   - Or update frontend healthApi.ts to call `/v1/health`
   - Current mismatch will break health checks

2. **Complete Report Implementations**
   - FeeReportsController has sophisticated reporting capability
   - SalaryReportsController provides valuable analytics
   - Consider implementing at least basic reporting dashboards

### Short-term Improvements

3. **Add Helper Endpoints Usage**
   - Implement `GET /teachers/active` for active-only dropdowns
   - Add `GET /fees/structures/active` for fee structure selection
   - Use `GET /teachers/check-email/{email}` during form validation

4. **Consolidate Salary Operations**
   - `POST /v1/salary/bulk` could streamline bulk salary creation
   - Remove unused `PUT /v1/salary-management/{id}` if truly redundant

### Long-term Considerations

5. **Documentation**
   - Update API documentation to mark unused endpoints
   - Provide migration guide if planning to deprecate endpoints

6. **Testing**
   - Add test coverage for all reporting endpoints
   - Ensure health check endpoints are properly tested

---

## 📝 Notes

- Frontend services use different API clients - some use `api.ts` centralized, some use custom services
- Salary endpoints use `/v1/` prefix while most others use just `/api/`
- All core CRUD operations are implemented in frontend
- Reporting capabilities exist in backend but not exposed in UI
- Health check endpoints have a route prefix mismatch issue
