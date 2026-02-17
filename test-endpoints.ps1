# SMS Backend API Testing Script
param(
    [string]$BaseUrl = "http://localhost:5208/api"
)

$ErrorActionPreference = 'Continue'
$testResults = @()
$passCount = 0
$failCount = 0

function Test-Endpoint {
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
        Write-Host "[PASS] $Method $Url" -ForegroundColor Green
        $script:passCount++
        return @{ Success = $true; Data = $response }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $errorMessage = $_.Exception.Message
        Write-Host "[FAIL] $Method $Url - $errorMessage" -ForegroundColor Red
        $script:failCount++
        return @{ Success = $false; Error = $errorMessage }
    }
}

Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "  SMS Backend API Test Suite" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl`n" -ForegroundColor Yellow

# 1. Health Check
Write-Host "`n[Test 1] Health Check" -ForegroundColor Cyan
$result = Test-Endpoint -Url "$BaseUrl/../api/v1/Health" -Method "GET"

# 2. Auth - Login
Write-Host "`n[Test 2] Authentication" -ForegroundColor Cyan
$loginBody = @{
    username = "admin"
    password = "Admin@123"
}
$result = Test-Endpoint -Url "$BaseUrl/Auth/login" -Method "POST" -Body $loginBody
$token = $null
if ($result.Success -and $result.Data) {
    $token = $result.Data.accessToken
    Write-Host "  Token obtained successfully" -ForegroundColor Gray
} else {
    Write-Host "  Failed to obtain token, skipping authenticated tests" -ForegroundColor Yellow
}

if ($token) {
    $authHeaders = @{ "Authorization" = "Bearer $token" }
    
    # Test /me endpoint
    $result = Test-Endpoint -Url "$BaseUrl/Auth/me" -Method "GET" -Headers $authHeaders
    
    # 3. Dashboard
    Write-Host "`n[Test 3] Dashboard" -ForegroundColor Cyan
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    $dashUrl = "$BaseUrl/../api/v1/Dashboard/summary" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-Endpoint -Url $dashUrl -Method "GET" -Headers $authHeaders
    
    # 4. Students
    Write-Host "`n[Test 4] Students" -ForegroundColor Cyan
    $studentsUrl = "$BaseUrl/Students" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $studentsUrl -Method "GET" -Headers $authHeaders
    
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $studentId = $result.Data.items[0].id
        $result = Test-Endpoint -Url "$BaseUrl/Students/$studentId" -Method "GET" -Headers $authHeaders
    }
    
    # Test Create Student
    $newStudent = @{
        firstName = "TestStudent"
        lastName = "AutoTest"
        email = "test_" + (Get-Random) + "@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "2010-01-01"
        address = "123 Test St"
        city = "Test City"
        state = "Test State"
        postalCode = "12345"
        guardianName = "Test Guardian"
        guardianPhone = "9876543210"
        guardianEmail = "guardian_" + (Get-Random) + "@test.com"
    }
    $result = Test-Endpoint -Url "$BaseUrl/Students" -Method "POST" -Body $newStudent -Headers $authHeaders
    
    if ($result.Success) {
        $createdStudentId = $result.Data.id
        # Test Delete
        $result = Test-Endpoint -Url "$BaseUrl/Students/$createdStudentId" -Method "DELETE" -Headers $authHeaders
    }
    
    # 5. Teachers
    Write-Host "`n[Test 5] Teachers" -ForegroundColor Cyan
    $teachersUrl = "$BaseUrl/Teachers" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $teachersUrl -Method "GET" -Headers $authHeaders
    
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $teacherId = $result.Data.items[0].id
        $result = Test-Endpoint -Url "$BaseUrl/Teachers/$teacherId" -Method "GET" -Headers $authHeaders
        
        if ($result.Data.email) {
            $teacherEmail = $result.Data.email
            $result = Test-Endpoint -Url "$BaseUrl/Teachers/by-email/$teacherEmail" -Method "GET" -Headers $authHeaders
        }
    }
    
    # Test Create Teacher
    $newTeacher = @{
        firstName = "TestTeacher"
        lastName = "AutoTest"
        email = "teacher_" + (Get-Random) + "@test.com"
        phoneNumber = "1234567890"
        dateOfBirth = "1990-01-01"
        address = "123 Test St"
        city = "Test City"
        state = "Test State"
        postalCode = "12345"
        hireDate = (Get-Date).ToString("yyyy-MM-dd")
        employeeId = "EMP" + (Get-Random -Minimum 1000 -Maximum 9999)
        specialization = "Computer Science"
        qualification = "Master's Degree"
    }
    $result = Test-Endpoint -Url "$BaseUrl/Teachers" -Method "POST" -Body $newTeacher -Headers $authHeaders
    
    if ($result.Success) {
        $createdTeacherId = $result.Data.id
        # Test Delete
        $result = Test-Endpoint -Url "$BaseUrl/Teachers/$createdTeacherId" -Method "DELETE" -Headers $authHeaders
    }
    
    # 6. Fees
    Write-Host "`n[Test 6] Fees" -ForegroundColor Cyan
    $feesUrl = "$BaseUrl/Fees/structures" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $feesUrl -Method "GET" -Headers $authHeaders
    
    if ($result.Success -and $result.Data.items -and $result.Data.items.Count -gt 0) {
        $feeId = $result.Data.items[0].id
        $result = Test-Endpoint -Url "$BaseUrl/Fees/structures/$feeId" -Method "GET" -Headers $authHeaders
    }
    
    $result = Test-Endpoint -Url "$BaseUrl/Fees/structures/active" -Method "GET" -Headers $authHeaders
    
    $paymentsUrl = "$BaseUrl/Fees/payments" + "?pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $paymentsUrl -Method "GET" -Headers $authHeaders
    
    # 7. Attendance
    Write-Host "`n[Test 7] Attendance" -ForegroundColor Cyan
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    
    $attUrl = "$BaseUrl/Attendance/students/history" + "?startDate=$startDate" + "&endDate=$endDate" + "&pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $attUrl -Method "GET" -Headers $authHeaders
    
    $attUrl2 = "$BaseUrl/Attendance/teachers/history" + "?startDate=$startDate" + "&endDate=$endDate" + "&pageNumber=1" + "&pageSize=10"
    $result = Test-Endpoint -Url $attUrl2 -Method "GET" -Headers $authHeaders
    
    # 8. Payroll
    Write-Host "`n[Test 8] Payroll" -ForegroundColor Cyan
    $payUrl = "$BaseUrl/../api/v1/Payroll/report" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-Endpoint -Url $payUrl -Method "GET" -Headers $authHeaders
    
    $bonusUrl = "$BaseUrl/../api/v1/Payroll/bonus-eligibility" + "?startDate=$startDate" + "&endDate=$endDate" + "&bonusThresholdPercentage=90"
    $result = Test-Endpoint -Url $bonusUrl -Method "GET" -Headers $authHeaders
    
    $attSumUrl = "$BaseUrl/../api/v1/Payroll/attendance-summary" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-Endpoint -Url $attSumUrl -Method "GET" -Headers $authHeaders
    
    # 9. Salary
    Write-Host "`n[Test 9] Salary" -ForegroundColor Cyan
    $salUrl = "$BaseUrl/../api/v1/Salary/period/report" + "?startDate=$startDate" + "&endDate=$endDate"
    $result = Test-Endpoint -Url $salUrl -Method "GET" -Headers $authHeaders
    
    $result = Test-Endpoint -Url "$BaseUrl/../api/v1/Salary/pending" -Method "GET" -Headers $authHeaders
    $result = Test-Endpoint -Url "$BaseUrl/../api/v1/Salary/summary" -Method "GET" -Headers $authHeaders
}

# Summary
Write-Host "`n================================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Total Tests: $($passCount + $failCount)" -ForegroundColor Yellow
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

if ($passCount + $failCount -gt 0) {
    $successRate = [math]::Round(($passCount / ($passCount + $failCount)) * 100, 2)
    Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } elseif ($successRate -ge 50) { "Yellow" } else { "Red" })
}

Write-Host "================================================`n" -ForegroundColor Cyan
