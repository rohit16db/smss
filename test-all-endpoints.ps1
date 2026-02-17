# SMS Backend API Comprehensive Testing Script
# Tests all endpoints and reports results

$baseUrl = "http://localhost:5208/api"
$v1BaseUrl = "http://localhost:5208/api/v1"
$token = $null
$testResults = @()
$passCount = 0
$failCount = 0

# Colors for output
$ErrorActionPreference = 'Continue'

function Write-TestResult {
    param(
        [string]$Endpoint,
        [string]$Method,
        [string]$Status,
        [string]$Details
    )
    
    $result = [PSCustomObject]@{
        Endpoint = $Endpoint
        Method = $Method
        Status = $Status
        Details = $Details
    }
    
    $script:testResults += $result
    
    if ($Status -eq "PASS") {
        $script:passCount++
        Write-Host "✓ " -ForegroundColor Green -NoNewline
    } else {
        $script:failCount++
        Write-Host "✗ " -ForegroundColor Red -NoNewline
    }
    Write-Host "$Method $Endpoint - $Details"
}

function Invoke-ApiTest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [object]$Body = $null,
        [hashtable]$Headers = @{},
        [string]$Description = ""
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json"
            Headers = $Headers
            ErrorAction = 'Stop'
        }
        
        if ($Body -and ($Method -ne "GET")) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }
        
        $response = Invoke-RestMethod @params
        return @{
            Success = $true
            StatusCode = 200
            Data = $response
            Error = $null
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorMessage = $_.Exception.Message
        
        if ($_.ErrorDetails.Message) {
            try {
                $errorDetails = $_.ErrorDetails.Message | ConvertFrom-Json
                $errorMessage = $errorDetails.message ?? $errorDetails.title ?? $errorMessage
            } catch {
                $errorMessage = $_.ErrorDetails.Message
            }
        }
        
        return @{
            Success = $false
            StatusCode = $statusCode
            Data = $null
            Error = $errorMessage
        }
    }
}

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "   SMS Backend API Testing Suite" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host "Start Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "==================================================`n" -ForegroundColor Cyan

# ===================================
# 1. HEALTH CHECK
# ===================================
Write-Host "`n[1/9] Testing Health Endpoints..." -ForegroundColor Cyan

$result = Invoke-ApiTest -Url "$v1BaseUrl/Health" -Method "GET" -Description "Get health status"
if ($result.Success -and $result.Data.status) {
    Write-TestResult -Endpoint "/api/v1/Health" -Method "GET" -Status "PASS" -Details "Status: $($result.Data.status), DB: $($result.Data.database)"
} else {
    Write-TestResult -Endpoint "/api/v1/Health" -Method "GET" -Status "FAIL" -Details $result.Error
}

# ===================================
# 2. AUTHENTICATION TESTS
# ===================================
Write-Host "`n[2/9] Testing Authentication Endpoints..." -ForegroundColor Cyan

# Test Login (we need this for protected endpoints)
$loginData = @{
    username = "admin"
    password = "Admin@123"
}

$result = Invoke-ApiTest -Url "$baseUrl/Auth/login" -Method "POST" -Body $loginData
if ($result.Success -and $result.Data.token) {
    $script:token = $result.Data.token
    Write-TestResult -Endpoint "/api/Auth/login" -Method "POST" -Status "PASS" -Details "Token received successfully"
} else {
    Write-TestResult -Endpoint "/api/Auth/login" -Method "POST" -Status "FAIL" -Details $result.Error
}

# Test Get Current User (requires auth)
if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    $result = Invoke-ApiTest -Url "$baseUrl/Auth/me" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/Auth/me" -Method "GET" -Status "PASS" -Details "User: $($result.Data.username)"
    } else {
        Write-TestResult -Endpoint "/api/Auth/me" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# Test Register (optional - might create duplicate users)
# Uncomment if you want to test registration
<#
$registerData = @{
    username = "testuser_$(Get-Random)"
    email = "test_$(Get-Random)@test.com"
    password = "Test@123"
    firstName = "Test"
    lastName = "User"
    role = "Student"
}

$result = Invoke-ApiTest -Url "$baseUrl/Auth/register" -Method "POST" -Body $registerData
if ($result.Success) {
    Write-TestResult -Endpoint "/api/Auth/register" -Method "POST" -Status "PASS" -Details "User registered successfully"
} else {
    Write-TestResult -Endpoint "/api/Auth/register" -Method "POST" -Status "FAIL" -Details $result.Error
}
#>

# ===================================
# 3. DASHBOARD TESTS
# ===================================
Write-Host "`n[3/9] Testing Dashboard Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    $endDate = Get-Date
    $startDate = $endDate.AddDays(-30)
    $startDateStr = $startDate.ToString('yyyy-MM-dd')
    $endDateStr = $endDate.ToString('yyyy-MM-dd')
    
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Dashboard/summary?startDate=$startDateStr&endDate=$endDateStr" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/v1/Dashboard/summary" -Method "GET" -Status "PASS" -Details "Dashboard data retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Dashboard/summary" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 4. STUDENTS TESTS
# ===================================
Write-Host "`n[4/9] Testing Students Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    # Get all students
    $url = "$baseUrl/Students?pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url -Method "GET" -Headers $headers
    if ($result.Success) {
        $studentCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Students (GET All)" -Method "GET" -Status "PASS" -Details "Retrieved $studentCount students"
        
        # If students exist, test Get By ID
        if ($result.Data.items -and $result.Data.items.Count -gt 0) {
            $studentId = $result.Data.items[0].id
            $getResult = Invoke-ApiTest -Url "$baseUrl/Students/$studentId" -Method "GET" -Headers $headers
            if ($getResult.Success) {
                Write-TestResult -Endpoint "/api/Students/{id}" -Method "GET" -Status "PASS" -Details "Student details retrieved"
            } else {
                Write-TestResult -Endpoint "/api/Students/{id}" -Method "GET" -Status "FAIL" -Details $getResult.Error
            }
        }
    } else {
        Write-TestResult -Endpoint "/api/Students (GET All)" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Test Create Student
    $newStudent = @{
        firstName = "TestStudent"
        lastName = "API_$(Get-Random)"
        email = "teststudent_$(Get-Random)@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "2010-01-01"
        address = "123 Test St"
        city = "Test City"
        state = "Test State"
        postalCode = "12345"
        guardianName = "Test Guardian"
        guardianPhone = "9876543210"
        guardianEmail = "guardian_$(Get-Random)@test.com"
    }
    
    $result = Invoke-ApiTest -Url "$baseUrl/Students" -Method "POST" -Body $newStudent -Headers $headers
    if ($result.Success) {
        $createdStudentId = $result.Data.id
        Write-TestResult -Endpoint "/api/Students (CREATE)" -Method "POST" -Status "PASS" -Details "Student created: $createdStudentId"
        
        # Test Update
        $updateStudent = @{
            id = $createdStudentId
            firstName = "UpdatedStudent"
            lastName = "API_Updated"
            email = $newStudent.email
            phoneNumber = "1234567890"
            dateOfBirth = "2010-01-01"
            address = "456 Updated St"
            city = "Updated City"
            state = "Updated State"
            postalCode = "54321"
            guardianName = "Updated Guardian"
            guardianPhone = "9876543210"
            guardianEmail = $newStudent.guardianEmail
        }
        
        $updateResult = Invoke-ApiTest -Url "$baseUrl/Students/$createdStudentId" -Method "PUT" -Body $updateStudent -Headers $headers
        if ($updateResult.Success) {
            Write-TestResult -Endpoint "/api/Students/{id} (UPDATE)" -Method "PUT" -Status "PASS" -Details "Student updated successfully"
        } else {
            Write-TestResult -Endpoint "/api/Students/{id} (UPDATE)" -Method "PUT" -Status "FAIL" -Details $updateResult.Error
        }
        
        # Test Delete
        $deleteResult = Invoke-ApiTest -Url "$baseUrl/Students/$createdStudentId" -Method "DELETE" -Headers $headers
        if ($deleteResult.Success) {
            Write-TestResult -Endpoint "/api/Students/{id} (DELETE)" -Method "DELETE" -Status "PASS" -Details "Student deleted successfully"
        } else {
            Write-TestResult -Endpoint "/api/Students/{id} (DELETE)" -Method "DELETE" -Status "FAIL" -Details $deleteResult.Error
        }
    } else {
        Write-TestResult -Endpoint "/api/Students (CREATE)" -Method "POST" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 5. TEACHERS TESTS
# ===================================
Write-Host "`n[5/9] Testing Teachers Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    # Get all teachers
    $url = "$baseUrl/Teachers?pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url -Method "GET" -Headers $headers
    if ($result.Success) {
        $teacherCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Teachers (GET All)" -Method "GET" -Status "PASS" -Details "Retrieved $teacherCount teachers"
        
        # If teachers exist, test Get By ID
        if ($result.Data.items -and $result.Data.items.Count -gt 0) {
            $teacherId = $result.Data.items[0].id
            $getResult = Invoke-ApiTest -Url "$baseUrl/Teachers/$teacherId" -Method "GET" -Headers $headers
            if ($getResult.Success) {
                Write-TestResult -Endpoint "/api/Teachers/{id}" -Method "GET" -Status "PASS" -Details "Teacher details retrieved"
            } else {
                Write-TestResult -Endpoint "/api/Teachers/{id}" -Method "GET" -Status "FAIL" -Details $getResult.Error
            }
            
            # Test Get By Email
            $teacherEmail = $result.Data.items[0].email
            if ($teacherEmail) {
                $emailResult = Invoke-ApiTest -Url "$baseUrl/Teachers/by-email/$teacherEmail" -Method "GET" -Headers $headers
                if ($emailResult.Success) {
                    Write-TestResult -Endpoint "/api/Teachers/by-email/{email}" -Method "GET" -Status "PASS" -Details "Teacher found by email"
                } else {
                    Write-TestResult -Endpoint "/api/Teachers/by-email/{email}" -Method "GET" -Status "FAIL" -Details $emailResult.Error
                }
            }
        }
    } else {
        Write-TestResult -Endpoint "/api/Teachers (GET All)" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Test Create Teacher
    $newTeacher = @{
        firstName = "TestTeacher"
        lastName = "API_$(Get-Random)"
        email = "testteacher_$(Get-Random)@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "1990-01-01"
        address = "123 Test St"
        city = "Test City"
        state = "Test State"
        postalCode = "12345"
        hireDate = (Get-Date).ToString("yyyy-MM-dd")
        employeeId = "EMP$(Get-Random -Minimum 1000 -Maximum 9999)"
        specialization = "Computer Science"
        qualification = "Master's Degree"
    }
    
    $result = Invoke-ApiTest -Url "$baseUrl/Teachers" -Method "POST" -Body $newTeacher -Headers $headers
    if ($result.Success) {
        $createdTeacherId = $result.Data.id
        Write-TestResult -Endpoint "/api/Teachers (CREATE)" -Method "POST" -Status "PASS" -Details "Teacher created: $createdTeacherId"
        
        # Test Update
        $updateTeacher = $newTeacher.Clone()
        $updateTeacher.firstName = "UpdatedTeacher"
        $updateTeacher.specialization = "Mathematics"
        
        $updateResult = Invoke-ApiTest -Url "$baseUrl/Teachers/$createdTeacherId" -Method "PUT" -Body $updateTeacher -Headers $headers
        if ($updateResult.Success) {
            Write-TestResult -Endpoint "/api/Teachers/{id} (UPDATE)" -Method "PUT" -Status "PASS" -Details "Teacher updated successfully"
        } else {
            Write-TestResult -Endpoint "/api/Teachers/{id} (UPDATE)" -Method "PUT" -Status "FAIL" -Details $updateResult.Error
        }
        
        # Test Delete
        $deleteResult = Invoke-ApiTest -Url "$baseUrl/Teachers/$createdTeacherId" -Method "DELETE" -Headers $headers
        if ($deleteResult.Success) {
            Write-TestResult -Endpoint "/api/Teachers/{id} (DELETE)" -Method "DELETE" -Status "PASS" -Details "Teacher deleted successfully"
        } else {
            Write-TestResult -Endpoint "/api/Teachers/{id} (DELETE)" -Method "DELETE" -Status "FAIL" -Details $deleteResult.Error
        }
    } else {
        Write-TestResult -Endpoint "/api/Teachers (CREATE)" -Method "POST" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 6. FEES TESTS
# ===================================
Write-Host "`n[6/9] Testing Fees Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    #url = "$baseUrl/Fees/structures?pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url
    $result = Invoke-ApiTest -Url "$baseUrl/Fees/structures?pageNumber=1&pageSize=10" -Method "GET" -Headers $headers
    if ($result.Success) {
        $feeCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Fees/structures (GET All)" -Method "GET" -Status "PASS" -Details "Retrieved $feeCount fee structures"
        
        # If fee structures exist, test Get By ID
        if ($result.Data.items -and $result.Data.items.Count -gt 0) {
            $feeId = $result.Data.items[0].id
            $getResult = Invoke-ApiTest -Url "$baseUrl/Fees/structures/$feeId" -Method "GET" -Headers $headers
            if ($getResult.Success) {
                Write-TestResult -Endpoint "/api/Fees/structures/{id}" -Method "GET" -Status "PASS" -Details "Fee structure retrieved"
            } else {
                Write-TestResult -Endpoint "/api/Fees/structures/{id}" -Method "GET" -Status "FAIL" -Details $getResult.Error
            }
        }
    } else {
        Write-TestResult -Endpoint "/api/Fees/structures (GET All)" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get active fee structures
    $result = Invoke-ApiTest -Url "$baseUrl/Fees/structures/active" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/Fees/structures/active" -Method "GET" -Status "PASS" -Details "Active fee structures retrieved"
    } else {
        Write-TestResult -Endpoint "/api/Fees/structures/active" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    url = "$baseUrl/Fees/payments?pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url
    # Get fee payments
    $result = Invoke-ApiTest -Url "$baseUrl/Fees/payments?pageNumber=1&pageSize=10" -Method "GET" -Headers $headers
    if ($result.Success) {
        $paymentCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Fees/payments (GET All)" -Method "GET" -Status "PASS" -Details "Retrieved $paymentCount fee payments"
    } else {
        Write-TestResult -Endpoint "/api/Fees/payments (GET All)" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 7. ATTENDANCE TESTS
# ===================================
Write-Host "`n[7/9] Testing Attendance Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    # Get student attendance history
    $startDateStr = $startDate.ToString('yyyy-MM-dd')
    $endDateStr = $endDate.ToString('yyyy-MM-dd')
    $url = "$baseUrl/Attendance/students/history?startDate=$startDateStr&endDate=$endDateStr&pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url
    $startDate = $endDate.AddDays(-30)
    $result = Invoke-ApiTest -Url "$baseUrl/Attendance/students/history?startDate=$($startDate.ToString('yyyy-MM-dd'))&endDate=$($endDate.ToString('yyyy-MM-dd'))&pageNumber=1&pageSize=10" -Method "GET" -Headers $headers
    if ($result.Success) {
        $attendanceCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Attendance/students/history" -Method "GET" -Status "PASS" -Details "Retrieved $attendanceCount student attendance records"
    }url = "$baseUrl/Attendance/teachers/history?startDate=$startDateStr&endDate=$endDateStr&pageNumber=1&pageSize=10"
    $result = Invoke-ApiTest -Url $url
        Write-TestResult -Endpoint "/api/Attendance/students/history" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get teacher attendance history
    $result = Invoke-ApiTest -Url "$baseUrl/Attendance/teachers/history?startDate=$($startDate.ToString('yyyy-MM-dd'))&endDate=$($endDate.ToString('yyyy-MM-dd'))&pageNumber=1&pageSize=10" -Method "GET" -Headers $headers
    if ($result.Success) {
        $attendanceCount = $result.Data.totalCount ?? 0
        Write-TestResult -Endpoint "/api/Attendance/teachers/history" -Method "GET" -Status "PASS" -Details "Retrieved $attendanceCount teacher attendance records"
    } else {
        Write-TestResult -Endpoint "/api/Attendance/teachers/history" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 8. PAYROLL TESTS
# ===================================
Write-Host "`n[8/9] Testing Payroll Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    
    $endDate = Get-Date
    $url = "$v1BaseUrl/Payroll/report?startDate=$startDateOnly&endDate=$endDateOnly"
    $result = Invoke-ApiTest -Url $url
    $startDateOnly = $startDate.ToString("yyyy-MM-dd")
    $endDateOnly = $endDate.ToString("yyyy-MM-dd")
    
    # Get payroll report
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Payroll/report?startDate=$startDateOnly&endDate=$endDateOnly" -Method "GET" -Headers $headers
    if ($result.Success) {
     url = "$v1BaseUrl/Payroll/bonus-eligibility?startDate=$startDateOnly&endDate=$endDateOnly&bonusThresholdPercentage=90"
    $result = Invoke-ApiTest -Url $url
    } else {
        Write-TestResult -Endpoint "/api/v1/Payroll/report" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get bonus eligibility
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Payroll/bonus-eligibility?startDate=$startDateOnly&endDate=$endDateOnly&bonusThresholdPercentage=90" -Method "GET" -Headers $headers
    iurl = "$v1BaseUrl/Payroll/attendance-summary?startDate=$startDateOnly&endDate=$endDateOnly"
    $result = Invoke-ApiTest -Url $url
        Write-TestResult -Endpoint "/api/v1/Payroll/bonus-eligibility" -Method "GET" -Status "PASS" -Details "Bonus eligibility retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Payroll/bonus-eligibility" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get attendance summary
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Payroll/attendance-summary?startDate=$startDateOnly&endDate=$endDateOnly" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/v1/Payroll/attendance-summary" -Method "GET" -Status "PASS" -Details "Attendance summary retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Payroll/attendance-summary" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# 9. SALARY TESTS
# ===================================
Write-Host "`n[9/9] Testing Salary Endpoints..." -ForegroundColor Cyan

if ($token) {
    $headers = @{ "Authorization" = "Bearer $token" }
    url = "$v1BaseUrl/Salary/period/report?startDate=$startDateOnly&endDate=$endDateOnly"
    $result = Invoke-ApiTest -Url $url
    $endDate = Get-Date
    $startDate = $endDate.AddDays(-30)
    $startDateOnly = $startDate.ToString("yyyy-MM-dd")
    $endDateOnly = $endDate.ToString("yyyy-MM-dd")
    
    # Get salary payments by period
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Salary/period/report?startDate=$startDateOnly&endDate=$endDateOnly" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/v1/Salary/period/report" -Method "GET" -Status "PASS" -Details "Salary period report retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Salary/period/report" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get pending salaries
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Salary/pending" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/v1/Salary/pending" -Method "GET" -Status "PASS" -Details "Pending salaries retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Salary/pending" -Method "GET" -Status "FAIL" -Details $result.Error
    }
    
    # Get salary summary
    $result = Invoke-ApiTest -Url "$v1BaseUrl/Salary/summary" -Method "GET" -Headers $headers
    if ($result.Success) {
        Write-TestResult -Endpoint "/api/v1/Salary/summary" -Method "GET" -Status "PASS" -Details "Salary summary retrieved"
    } else {
        Write-TestResult -Endpoint "/api/v1/Salary/summary" -Method "GET" -Status "FAIL" -Details $result.Error
    }
}

# ===================================
# SUMMARY
# ===================================
Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host "   Test Results Summary" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Total Tests: $($passCount + $failCount)" -ForegroundColor Yellow
Wtimestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$resultsFile = "test-results-$timestamp
Write-Host "Failed: $failCount" -ForegroundColor Red
$successRate = if (($passCount + $failCount) -gt 0) { [math]::Round(($passCount / ($passCount + $failCount)) * 100, 2) } else { 0 }
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 50) { "Yellow" } else { "Red" })
Write-Host "End Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "==================================================`n" -ForegroundColor Cyan

# Export results to CSV
$resultsFile = "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
$testResults | Export-Csv -Path $resultsFile -NoTypeInformation
Write-Host "Results exported to: $resultsFile" -ForegroundColor Green

# Display failed tests if any
if ($failCount -gt 0) {
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $testResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "  - $($_.Method) $($_.Endpoint): $($_.Details)" -ForegroundColor Red
    }
}

Write-Host "`nTesting completed!" -ForegroundColor Cyan
