$ErrorActionPreference = 'Stop'
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "     RUNNING PAYMENTS INTEGRATION TESTS      " -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Setup certificate trust bypass for local HTTPS testing
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# 1. Test Active C# Backend
Write-Host "Starting C# Backend..." -ForegroundColor Yellow
$csharpApiDir = Resolve-Path "$PSScriptRoot/../EcommerceCA/EcommerceCA.API"
$csharpProc = Start-Process dotnet -ArgumentList "run" -WorkingDirectory $csharpApiDir -PassThru -NoNewWindow

Write-Host "Waiting for C# Backend to listen on port 53638..." -ForegroundColor Yellow
$csharpPortOpen = $false
for ($i = 0; $i -lt 35; $i++) {
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", 53638)
        $csharpPortOpen = $true
        $tcpClient.Close()
        break
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $csharpPortOpen) {
    Write-Host " [FAIL] C# Backend port 53638 did not open in 35 seconds." -ForegroundColor Red
    Stop-Process -Id $csharpProc.Id -Force
    exit 1
}


$csharpSuccess = $false
try {
    Write-Host "Testing C# Create Order API..." -ForegroundColor Gray
    $createRes = Invoke-RestMethod -Uri "https://localhost:53638/api/payment/create-order" -Method Post -ContentType "application/json" -Body '{"amount": 100, "orderId": 1}'
    if ($createRes.orderId -and $createRes.keyId) {
        Write-Host " [OK] C# Create Order API succeeded. Order ID: $($createRes.orderId)" -ForegroundColor Green
        
        Write-Host "Testing C# Signature Verification API..." -ForegroundColor Gray
        $verifyRes = Invoke-RestMethod -Uri "https://localhost:53638/api/payment/verify" -Method Post -ContentType "application/json" -Body "{`"razorpay_order_id`": `"$($createRes.orderId)`", `"razorpay_payment_id`": `"pay_mock_123`", `"razorpay_signature`": `"sig_mock_123`", `"orderId`": 1}"
        if ($verifyRes.success -eq $true) {
            Write-Host " [OK] C# Signature Verification succeeded!" -ForegroundColor Green
            $csharpSuccess = $true
        } else {
            Write-Host " [FAIL] C# Signature Verification failed." -ForegroundColor Red
        }
    } else {
        Write-Host " [FAIL] C# Create Order API response invalid." -ForegroundColor Red
    }
} catch {
    Write-Host " [ERROR] C# API error: $_" -ForegroundColor Red
} finally {
    Write-Host "Stopping C# Backend..." -ForegroundColor Gray
    Stop-Process -Id $csharpProc.Id -Force
}

Write-Host ""

# 2. Test Node.js Backend
Write-Host "Starting Node.js Backend..." -ForegroundColor Yellow
$nodeMockDir = Resolve-Path "$PSScriptRoot/../../mocks/ycc-server-mock"
$nodeProc = Start-Process node -ArgumentList "server.js" -WorkingDirectory $nodeMockDir -PassThru -NoNewWindow

Write-Host "Waiting for Node.js Backend to listen on port 5000..." -ForegroundColor Yellow
$nodePortOpen = $false
for ($i = 0; $i -lt 15; $i++) {
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", 5000)
        $nodePortOpen = $true
        $tcpClient.Close()
        break
    } catch {
        Start-Sleep -Seconds 1
    }
}

if (-not $nodePortOpen) {
    Write-Host " [FAIL] Node.js Backend port 5000 did not open in 15 seconds." -ForegroundColor Red
    Stop-Process -Id $nodeProc.Id -Force
    exit 1
}


$nodeSuccess = $false
try {
    Write-Host "Testing Node.js Create Order API..." -ForegroundColor Gray
    $createRes = Invoke-RestMethod -Uri "http://localhost:5000/api/payment/create-order" -Method Post -ContentType "application/json" -Body '{"amount": 100}'
    if ($createRes.orderId -and $createRes.keyId) {
        Write-Host " [OK] Node.js Create Order API succeeded. Order ID: $($createRes.orderId)" -ForegroundColor Green
        
        Write-Host "Testing Node.js Signature Verification API..." -ForegroundColor Gray
        $verifyRes = Invoke-RestMethod -Uri "http://localhost:5000/api/payment/verify" -Method Post -ContentType "application/json" -Body "{`"razorpay_order_id`": `"$($createRes.orderId)`", `"razorpay_payment_id`": `"pay_mock_123`", `"razorpay_signature`": `"sig_mock_123`"}"
        if ($verifyRes.success -eq $true) {
            Write-Host " [OK] Node.js Signature Verification succeeded!" -ForegroundColor Green
            $nodeSuccess = $true
        } else {
            Write-Host " [FAIL] Node.js Signature Verification failed." -ForegroundColor Red
        }
    } else {
        Write-Host " [FAIL] Node.js Create Order API response invalid." -ForegroundColor Red
    }
} catch {
    Write-Host " [ERROR] Node.js API error: $_" -ForegroundColor Red
} finally {
    Write-Host "Stopping Node.js Backend..." -ForegroundColor Gray
    Stop-Process -Id $nodeProc.Id -Force
}

Write-Host "=============================================" -ForegroundColor Cyan
if ($csharpSuccess -and $nodeSuccess) {
    Write-Host "     ALL PAYMENT INTEGRATION TESTS PASSED!   " -ForegroundColor Green
} else {
    Write-Host "     SOME TESTS FAILED. CHECK LOGS.          " -ForegroundColor Red
}
Write-Host "=============================================" -ForegroundColor Cyan
