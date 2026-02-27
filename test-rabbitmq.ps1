# Test script to publish a message to RabbitMQ
# Run this while your AccessLogWorker is running

$testMessage = @{
    SchemaVersion = 1
    TimestampUtc = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
    Method = "TEST"
    Path = "/test-from-script"
    StatusCode = 200
    DurationMs = 999
    UserId = "test-user-123"
    Username = "testuser"
    TraceId = [guid]::NewGuid().ToString()
    ClientIp = "127.0.0.1"
    UserAgent = "PowerShell-Test"
} | ConvertTo-Json

Write-Host "Publishing test message to RabbitMQ..." -ForegroundColor Cyan
Write-Host $testMessage

# Encode message as base64 for rabbitmqadmin
$base64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($testMessage))

# Use docker exec to publish directly via RabbitMQ
docker exec rabbitmq rabbitmqadmin publish exchange=accesslog.exchange routing_key=accesslog payload="$testMessage"

Write-Host "`nMessage published! Check your Worker console for logs." -ForegroundColor Green
Write-Host "You should see: '=== MESSAGE RECEIVED ===' followed by save confirmation" -ForegroundColor Yellow
