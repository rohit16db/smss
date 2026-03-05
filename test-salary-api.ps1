#!/usr/bin/env pwsh

# Salary Structure API Test Script
# This script helps test the salary structure creation endpoint

param(
    [string]$ApiUrl = "http://localhost:5208",
    [string]$Token = ""
)

#region Configuration
$ApiEndpoint = "$ApiUrl/api/v1/salarystructure"
$Headers = @{
    "Content-Type" = "application/json"
}

if ($Token) {
    $Headers["Authorization"] = "Bearer $Token"
}
#endregion

#region Helper Functions
function Test-Salary {
    param(
        [string]$Name,
        [decimal]$BaseSalary,
        [decimal]$HRA = 0,
        [decimal]$DA = 0,
        [string]$EffectiveFromDate,
        [string]$Description = "",
        [decimal]$MedicalAllowance = 0,
        [decimal]$ConveyanceAllowance = 0,
        [decimal]$OtherAllowances = 0,
        [decimal]$StandardDeduction = 0,
        [int]$MinExperienceYears = 0,
        [string]$ApplicableQualifications = "",
        [string]$EffectiveToDate = $null
    )

    $body = @{
        name = $Name
        baseSalary = $BaseSalary
        hra = $HRA
        da = $DA
        medicalAllowance = $MedicalAllowance
        conveyanceAllowance = $ConveyanceAllowance
        otherAllowances = $OtherAllowances
        standardDeduction = $StandardDeduction
        minExperienceYears = $MinExperienceYears
        effectiveFromDate = $EffectiveFromDate
    }

    if ($Description) {
        $body.description = $Description
    }
    if ($ApplicableQualifications) {
        $body.applicableQualifications = $ApplicableQualifications
    }
    if ($EffectiveToDate) {
        $body.effectiveToDate = $EffectiveToDate
    }

    $jsonBody = $body | ConvertTo-Json -Depth 10

    Write-Host "Testing Salary Structure Creation" -ForegroundColor Cyan
    Write-Host "=================================" -ForegroundColor Cyan
    Write-Host "`nEndpoint: $ApiEndpoint" -ForegroundColor Yellow
    Write-Host "`nRequest Body:" -ForegroundColor Yellow
    Write-Host $jsonBody -ForegroundColor White

    try {
        $response = Invoke-WebRequest -Uri $ApiEndpoint `
            -Method POST `
            -Headers $Headers `
            -Body $jsonBody `
            -ErrorAction Stop

        Write-Host "`n✅ SUCCESS (201 Created)" -ForegroundColor Green
        Write-Host "`nResponse:" -ForegroundColor Green
        $responseBody = $response.Content | ConvertFrom-Json
        $responseBody | ConvertTo-Json | Write-Host
        return $responseBody
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        Write-Host "`n❌ ERROR ($statusCode)" -ForegroundColor Red
        
        if ($_.ErrorDetails) {
            Write-Host "`nError Details:" -ForegroundColor Red
            $errorBody = $_.ErrorDetails.Message | ConvertFrom-Json
            $errorBody | ConvertTo-Json -Depth 10 | Write-Host -ForegroundColor Red
        } else {
            Write-Host $_.Exception.Message -ForegroundColor Red
        }
        return $null
    }
}

function Show-Examples {
    Write-Host "`nSALARY STRUCTURE API TEST EXAMPLES" -ForegroundColor Cyan
    Write-Host "==================================`n" -ForegroundColor Cyan

    Write-Host "Example 1: Minimum Required Fields" -ForegroundColor Yellow
    Write-Host '.\test-salary-api.ps1 -Token "your_token"' -ForegroundColor White
    Write-Host "This runs the basic test case below`n"

    Write-Host "Example 2: Custom Test" -ForegroundColor Yellow
    Write-Host @'
$result = & .\test-salary-api.ps1 `
  -Token "your_jwt_token_here" `
  -ApiUrl "http://localhost:5208"
'@ -ForegroundColor White
    Write-Host ""

    Write-Host "Example 3: Test with Full Details" -ForegroundColor Yellow
    Write-Host @'
Test-Salary `
  -Name "Senior Science Teacher" `
  -BaseSalary 75000 `
  -HRA 15000 `
  -DA 10000 `
  -MedicalAllowance 2000 `
  -ConveyanceAllowance 2000 `
  -OtherAllowances 3000 `
  -StandardDeduction 5000 `
  -MinExperienceYears 5 `
  -ApplicableQualifications "B.Sc, B.Ed, M.A" `
  -EffectiveFromDate "2024-04-01" `
  -EffectiveToDate "2025-03-31" `
  -Description "Salary for experienced science teachers"
'@ -ForegroundColor White
    Write-Host ""
}

#endregion

#region Main Test Cases
Write-Host "`n╔════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  SALARY STRUCTURE API - TEST SCRIPT             ║" -ForegroundColor Magenta
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Magenta

Write-Host "`nAPI Configuration:" -ForegroundColor Cyan
Write-Host "  URL: $ApiEndpoint" -ForegroundColor White
Write-Host "  Auth: $(if ($Token) { 'Enabled' } else { 'Disabled (Optional)' })" -ForegroundColor White

# Run Test 1
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "TEST 1: Minimum Required Fields" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
$result1 = Test-Salary `
    -Name "Junior Teacher" `
    -BaseSalary 40000 `
    -EffectiveFromDate (Get-Date -Format "yyyy-MM-01").ToString()

# Run Test 2
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "TEST 2: Complete Structure" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
$result2 = Test-Salary `
    -Name "Senior Teacher - Science" `
    -Description "For experienced science teachers with 5+ years" `
    -BaseSalary 75000 `
    -HRA 15000 `
    -DA 10000 `
    -MedicalAllowance 2000 `
    -ConveyanceAllowance 2000 `
    -OtherAllowances 3000 `
    -StandardDeduction 5000 `
    -MinExperienceYears 5 `
    -ApplicableQualifications "B.Sc, B.Ed, M.A" `
    -EffectiveFromDate "2024-04-01" `
    -EffectiveToDate "2025-03-31"

# Show Results Summary
Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta
Write-Host "TEST SUMMARY" -ForegroundColor Magenta
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Magenta

$successCount = 0
if ($result1) { $successCount++ }
if ($result2) { $successCount++ }

Write-Host "`nPassed: $successCount / 2" -ForegroundColor $(if ($successCount -eq 2) { 'Green' } else { 'Yellow' })

if ($successCount -eq 2) {
    Write-Host "`n✅ All tests passed! API is working correctly." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Check the error details above." -ForegroundColor Yellow
    Show-Examples
}

#endregion

#region Common Issues
Write-Host "`n╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  TROUBLESHOOTING COMMON 400 ERRORS              ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan

Write-Host @"
Common Causes of 400 Bad Request:

1. ❌ DATE FORMAT ISSUE (Most Common)
   - Wrong: "2024-01-15T00:00:00Z"  (datetime with time)
   - Right: "2024-01-15"             (date only: yyyy-MM-dd)

2. ❌ MISSING REQUIRED FIELDS
   - Must provide: name, baseSalary, effectiveFromDate
   - Example: $(Get-Date -Format "yyyy-MM-01")

3. ❌ BaseSalary NOT POSITIVE
   - Wrong: baseSalary = 0
   - Right: baseSalary = 50000.00  (must be > 0)

4. ❌ NEGATIVE ALLOWANCES/DEDUCTIONS
   - Wrong: hra = -5000
   - Right: hra = 5000  (must be >= 0)

5. ❌ STRING LENGTH EXCEEDED
   - Name: Max 100 characters
   - Description: Max 500 characters
   - Qualifications: Max 500 characters

6. ❌ MISSING AUTHORIZATION
   - Add: -Token "your_jwt_token_here"
   - Requires: SalaryManageAccess policy

SOLUTION:
Use the test cases above with correct format, or see:
SALARY-STRUCTURE-API-400-FIX.md for detailed examples.
"@ -ForegroundColor Yellow

Write-Host "`n" -ForegroundColor Cyan
