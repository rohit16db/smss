#!/usr/bin/env pwsh

# Start backend server in background
Write-Host "Starting backend server..."
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "D:\practice\SMS\backend\src\SMS.API" -PassThru -RedirectStandardOutput "D:\practice\SMS\server.log" -RedirectStandardError  "D:\practice\SMS\server-error.log"

Write-Host "Waiting for server to start..."
Start-Sleep -Seconds 8

Write-Host "Testing login endpoint..."
try {
    $loginBody = @{"email"="admin@sms.com";"password"="Admin@123"} | ConvertTo-Json
    $loginResp = Invoke-WebRequest -Uri "http://localhost:5208/api/auth/login" -Method POST -Headers @{"Content-Type"="application/json"} -Body $loginBody -ErrorAction Stop
    $loginData = $loginResp.Content | ConvertFrom-Json
    $token = $loginData.data.accessToken
    Write-Host "✓ Login successful. Token: $($token.Substring(0, 30))..."
    
    Write-Host ""
    Write-Host "Testing students endpoint..."
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5208/api/students?pageNumber=1&pageSize=10&isActive=true" `
            -Headers @{"Authorization"="Bearer $token"} -ErrorAction Stop
        Write-Host "✓ Status: $($response.StatusCode)"
        Write-Host "Response: $($response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 3)"
    }
    catch {
        Write-Host "✗ Students endpoint error:"
        Write-Host "StatusCode: $($_.Exception.Response.StatusCode)"
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorStream)
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error Body: $errorBody"
    }
}
catch {
    Write-Host "✗ Login failed:"
    Write-Host $_.Exception.Message
    if ($_.Exception.Response) {
        Write-Host "StatusCode: $($_.Exception.Response.StatusCode)"
    }
}

Write-Host ""
Write-Host "Stopping server..."
Stop-Process -Id $process.Id -Force
Write-Host "Done!"
