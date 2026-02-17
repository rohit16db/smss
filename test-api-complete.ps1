# Detailed API Endpoint Testing Script
param(
    [string]$BaseUrl = "http://localhost:5208/api"
)

$ErrorActionPreference = 'Continue'
$testResults = @()

function Add-TestResult {
    param($Endpoint, $Method, $Status, $StatusCode, $Error, $Details)
    $script:testResults += [PSCustomObject]@{
        Endpoint = $Endpoint
        Method = $Method
        Status = $Status
        StatusCode = $StatusCode
        Error = $Error
        Details = $Details
    }
}

function Test-DetailedEndpoint {
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
        Write-Host "[✓] $Description" -ForegroundColor Green
        Add-TestResult -Endpoint $Url -Method $Method -Status "PASS" -StatusCode 200 -Details $Description
        return @{ Success = $true; Data = $response; StatusCode = 200 }
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
        
        Write-Host "[✗] $Description - $errorMessage" -ForegroundColor Red
        Add-TestResult -Endpoint $Url -Method $Method -Status "FAIL" -StatusCode $statusCode -Error $errorMessage -Details $Description
        return @{ Success = $false; Error = $errorMessage; StatusCode = $statusCode }
    }
}

Write-Host "`n==================================================================" -ForegroundColor Cyan
Write-Host "           SMS Backend API - Complete Endpoint Testing" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Test Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "==================================================================`n" -ForegroundColor Cyan

# ========================================
# 1. HEALTH CHECK ENDPOINTS
# ========================================
Write-Host "`n[1/9] HEALTH CHECK ENDPOINTS" -ForegroundColor Cyan
Write-Host "--------------------------------------" -ForegroundColor Gray

$result = Test-DetailedEndpoint -Url "http://localhost:5208/api/v1/Health" -Method "GET" -Description "GET /api/v1/Health - Check API health status"

# ========================================
# 2. AUTHENTICATION ENDPOINTS
# ========================================
Write-Host "`n[2/9] AUTHENTICATION ENDPOINTS" -ForegroundColor Cyan
Write-Host "--------------------------------------" -ForegroundColor Gray

$loginBody = @{
    username = "admin"
    password = "Admin@123"
}
$result = Test-DetailedEndpoint -Url "$BaseUrl/Auth/login" -Method "POST" -Body $loginBody -Description "POST /api/Auth/login - User login"

$token = $null
if ($result.Success -and $result.Data) {
    $token = $result.Data.accessToken
    Write-Host "  ℹ Token obtained for subsequent tests" -ForegroundColor DarkGray
}

if ($token) {
    $authHeaders = @{ "Authorization" = "Bearer $token" }
    
    # Note: /me endpoint might be broken - we'll test it anyway
    $result = Test-DetailedEndpoint -Url "$BaseUrl/Auth/me" -Method "GET" -Headers $authHeaders -Description "GET /api/Auth/me - Get current user info"
    
    # ========================================
    # 3. DASHBOARD ENDPOINTS
    # ========================================
    Write-Host "`n[3/9] DASHBOARD ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    $dashUrl = "http://localhost:5208/api/v1/Dashboard/summary" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-DetailedEndpoint -Url $dashUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Dashboard/summary - Get dashboard summary with KPIs"
    
    # ========================================
    # 4. STUDENTS ENDPOINTS
    # ========================================
    Write-Host "`n[4/9] STUDENTS ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $studentsUrl = "$BaseUrl/Students" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-DetailedEndpoint -Url $studentsUrl -Method "GET" -Headers $authHeaders -Description "GET /api/Students - Get all students (paginated)"
    
    $studentId = $null
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $studentId = $result.Data.items[0].id
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Students/$studentId" -Method "GET" -Headers $authHeaders -Description "GET /api/Students/{id} - Get student by ID"
    } else {
        Write-Host "  ℹ No students found, skipping Get By ID test" -ForegroundColor DarkGray
        Add-TestResult -Endpoint "$BaseUrl/Students/{id}" -Method "GET" -Status "SKIP" -Details "No students available to test"
    }
    
    # Test Create Student
    $newStudent = @{
        firstName = "TestStudent"
        lastName = "APITest"
        email = "teststudent_" + (Get-Random) + "@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "2010-01-01T00:00:00"
        address = "123 Test Street"
        city = "TestCity"
        state = "TestState"
        postalCode = "12345"
        guardianName = "Test Guardian"
        guardianPhone = "9876543210"
        guardianEmail = "guardian_" + (Get-Random) + "@test.com"
    }
    $result = Test-DetailedEndpoint -Url "$BaseUrl/Students" -Method "POST" -Body $newStudent -Headers $authHeaders -Description "POST /api/Students - Create new student"
    
    $createdStudentId = $null
    if ($result.Success -and $result.Data) {
        $createdStudentId = $result.Data.id
        Write-Host "  ℹ Created student ID: $createdStudentId" -ForegroundColor DarkGray
        
        # Test Update Student
        $updateStudent = $newStudent.Clone()
        $updateStudent.id = $createdStudentId
        $updateStudent.firstName = "UpdatedStudent"
        $updateStudent.address = "456 Updated Street"
        
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Students/$createdStudentId" -Method "PUT" -Body $updateStudent -Headers $authHeaders -Description "PUT /api/Students/{id} - Update student"
        
        # Test Delete Student
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Students/$createdStudentId" -Method "DELETE" -Headers $authHeaders -Description "DELETE /api/Students/{id} - Delete student"
    }
    
    # ========================================
    # 5. TEACHERS ENDPOINTS
    # ========================================
    Write-Host "`n[5/9] TEACHERS ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $teachersUrl = "$BaseUrl/Teachers" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-DetailedEndpoint -Url $teachersUrl -Method "GET" -Headers $authHeaders -Description "GET /api/Teachers - Get all teachers (paginated)"
    
    $teacherId = $null
    $teacherEmail = $null
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $teacherId = $result.Data.items[0].id
        $teacherEmail = $result.Data.items[0].email
        
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Teachers/$teacherId" -Method "GET" -Headers $authHeaders -Description "GET /api/Teachers/{id} - Get teacher by ID"
        
        if ($teacherEmail) {
            $result = Test-DetailedEndpoint -Url "$BaseUrl/Teachers/by-email/$teacherEmail" -Method "GET" -Headers $authHeaders -Description "GET /api/Teachers/by-email/{email} - Get teacher by email"
        }
    } else {
        Write-Host "  ℹ No teachers found, skipping individual tests" -ForegroundColor DarkGray
    }
    
    # Test Create Teacher
    $newTeacher = @{
        firstName = "TestTeacher"
        lastName = "APITest"
        email = "testteacher_" + (Get-Random) + "@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "1990-01-01T00:00:00"
        address = "123 Test Street"
        city = "TestCity"
        state = "TestState"
        postalCode = "12345"
        hireDate = (Get-Date).ToString("yyyy-MM-ddT00:00:00")
        employeeId = "EMP" + (Get-Random -Minimum 1000 -Maximum 9999)
        specialization = "Computer Science"
        qualification = "Masters Degree"
    }
    $result = Test-DetailedEndpoint -Url "$BaseUrl/Teachers" -Method "POST" -Body $newTeacher -Headers $authHeaders -Description "POST /api/Teachers - Create new teacher"
    
    $createdTeacherId = $null
    if ($result.Success -and $result.Data) {
        $createdTeacherId = $result.Data.id
        Write-Host "  ℹ Created teacher ID: $createdTeacherId" -ForegroundColor DarkGray
        
        # Test Update Teacher
        $updateTeacher = $newTeacher.Clone()
        $updateTeacher.firstName = "UpdatedTeacher"
        $updateTeacher.specialization = "Mathematics"
        
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Teachers/$createdTeacherId" -Method "PUT" -Body $updateTeacher -Headers $authHeaders -Description "PUT /api/Teachers/{id} - Update teacher"
        
        # Test Delete Teacher
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Teachers/$createdTeacherId" -Method "DELETE" -Headers $authHeaders -Description "DELETE /api/Teachers/{id} - Delete teacher"
    }
    
    # ========================================
    # 6. FEES ENDPOINTS
    # ========================================
    Write-Host "`n[6/9] FEES ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $feesUrl = "$BaseUrl/Fees/structures" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-DetailedEndpoint -Url $feesUrl -Method "GET" -Headers $authHeaders -Description "GET /api/Fees/structures - Get all fee structures"
    
    $feeId = $null
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $feeId = $result.Data.items[0].id
        $result = Test-DetailedEndpoint -Url "$BaseUrl/Fees/structures/$feeId" -Method "GET" -Headers $authHeaders -Description "GET /api/Fees/structures/{id} - Get fee structure by ID"
    }
    
    $result = Test-DetailedEndpoint -Url "$BaseUrl/Fees/structures/active" -Method "GET" -Headers $authHeaders -Description "GET /api/Fees/structures/active - Get active fee structures"
    
    # Note: The /payments endpoint might not exist or use different URL pattern
    # Let's try different variations
    Write-Host "  ℹ Testing fee payments endpoints (may not be implemented)" -ForegroundColor DarkGray
    $result = Test-DetailedEndpoint -Url "$BaseUrl/Fees/payments" -Method "GET" -Headers $authHeaders -Description "GET /api/Fees/payments - Get fee payments"
    
    # ========================================
    # 7. ATTENDANCE ENDPOINTS
    # ========================================
    Write-Host "`n[7/9] ATTENDANCE ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    
    $attUrl = "$BaseUrl/Attendance/students/history" + "?startDate=$startDate" + "&endDate=$endDate" + "&pageNumber=1" + "&pageSize=10"
    $result = Test-DetailedEndpoint -Url $attUrl -Method "GET" -Headers $authHeaders -Description "GET /api/Attendance/students/history - Get student attendance history"
    
    $attUrl2 = "$BaseUrl/Attendance/teachers/history" + "?startDate=$startDate" + "&endDate=$endDate" + "&pageNumber=1" + "&pageSize=10"
    $result = Test-DetailedEndpoint -Url $attUrl2 -Method "GET" -Headers $authHeaders -Description "GET /api/Attendance/teachers/history - Get teacher attendance history"
    
    # Test by date query
    $today = (Get-Date).ToString("yyyy-MM-dd")
    $attByDateUrl = "$BaseUrl/Attendance/students/by-date" + "?classId=test-class" + "&date=$today"
    $result = Test-DetailedEndpoint -Url $attByDateUrl -Method "GET" -Headers $authHeaders -Description "GET /api/Attendance/students/by-date - Get attendance by date and class"
    
    # ========================================
    # 8. PAYROLL ENDPOINTS
    # ========================================
    Write-Host "`n[8/9] PAYROLL ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    
    $payUrl = "http://localhost:5208/api/v1/Payroll/report" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-DetailedEndpoint -Url $payUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Payroll/report - Get teacher payroll report"
    
    $bonusUrl = "http://localhost:5208/api/v1/Payroll/bonus-eligibility" + "?startDate=$startDate" + "&endDate=$endDate" + "&bonusThresholdPercentage=90"
    $result = Test-DetailedEndpoint -Url $bonusUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Payroll/bonus-eligibility - Get bonus eligibility"
    
    $attSumUrl = "http://localhost:5208/api/v1/Payroll/attendance-summary" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-DetailedEndpoint -Url $attSumUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Payroll/attendance-summary - Get attendance summary"
    
    # ========================================
    # 9. SALARY ENDPOINTS
    # ========================================
    Write-Host "`n[9/9] SALARY ENDPOINTS" -ForegroundColor Cyan
    Write-Host "--------------------------------------" -ForegroundColor Gray
    
    $salUrl = "http://localhost:5208/api/v1/Salary/period/report" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-DetailedEndpoint -Url $salUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Salary/period/report - Get salary payments by period"
    
    $result = Test-DetailedEndpoint -Url "http://localhost:5208/api/v1/Salary/pending" -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Salary/pending - Get pending salary payments"
    
    $result = Test-DetailedEndpoint -Url "http://localhost:5208/api/v1/Salary/summary" -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Salary/summary - Get salary summary"
    
    # Test with teacher ID if available
    if ($teacherId) {
        $teacherSalUrl = "http://localhost:5208/api/v1/Salary/teacher/$teacherId"
        $result = Test-DetailedEndpoint -Url $teacherSalUrl -Method "GET" -Headers $authHeaders -Description "GET /api/v1/Salary/teacher/{id} - Get teacher salary history"
    }
}

# ========================================
# SUMMARY AND REPORTING
# ========================================
Write-Host "`n==================================================================" -ForegroundColor Cyan
Write-Host "                          TEST SUMMARY" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$skipCount = ($testResults | Where-Object { $_.Status -eq "SKIP" }).Count
$totalCount = $testResults.Count

Write-Host "`nTotal Tests: $totalCount" -ForegroundColor Yellow
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Skipped: $skipCount" -ForegroundColor Gray

if ($totalCount -gt 0) {
    $successRate = [math]::Round(($passCount / $totalCount) * 100, 2)
    $color = if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 50) { "Yellow" } else { "Red" }
    Write-Host "`nSuccess Rate: $successRate%" -ForegroundColor $color
}

# Export detailed results
$timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$csvFile = "api-test-results-$timestamp.csv"
$testResults | Export-Csv -Path $csvFile -NoTypeInformation
Write-Host "`nDetailed results exported to: $csvFile" -ForegroundColor Green

# Show failures
if ($failCount -gt 0) {
    Write-Host "`n------------------------------------------------------------------" -ForegroundColor Red
    Write-Host "FAILED TESTS:" -ForegroundColor Red
    Write-Host "------------------------------------------------------------------" -ForegroundColor Red
    $testResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "`n$($_.Method) $($_.Endpoint)" -ForegroundColor Yellow
        Write-Host "  Status Code: $($_.StatusCode)" -ForegroundColor Gray
        Write-Host "  Error: $($_.Error)" -ForegroundColor Red
        Write-Host "  Description: $($_.Details)" -ForegroundColor Gray
    }
}

Write-Host "`n==================================================================" -ForegroundColor Cyan
Write-Host "Testing completed at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "==================================================================`n" -ForegroundColor Cyan
