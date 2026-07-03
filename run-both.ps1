# Run both NexusArena.API and NexusArena.Web simultaneously

Write-Host "🚀 Starting NexusArena.API and NexusArena.Web..." -ForegroundColor Green
Write-Host ""

# Start API in background
Write-Host "📡 Starting API on port 5092..." -ForegroundColor Cyan
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --project NexusArena.API"

# Wait a bit for API to start
Start-Sleep -Seconds 3

# Start Web in background
Write-Host "🌐 Starting Web on port 5046..." -ForegroundColor Cyan
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --project NexusArena.Web"

Write-Host ""
Write-Host "✅ Both projects are now running!" -ForegroundColor Green
Write-Host ""
Write-Host "API:  http://localhost:5092/swagger" -ForegroundColor Yellow
Write-Host "Web:  http://localhost:5046" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press Ctrl+C in each window to stop the projects" -ForegroundColor Gray
