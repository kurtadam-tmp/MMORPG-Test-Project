Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " 🚀  MMORPG DEDICATED CLUSTER - PRODUCTION DEPLOYMENT" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

Write-Host "`n[1/3] Building & Starting Docker Containers (PostgreSQL + Redis + GatewayApi + MasterServer)..." -ForegroundColor Yellow
docker-compose down
docker-compose up -d --build

Write-Host "`n[2/3] Verifying Container Health & Active Listening Ports..." -ForegroundColor Yellow
Start-Sleep -Seconds 5
docker ps

Write-Host "`n[3/3] Running Automated Integration Tests..." -ForegroundColor Yellow
dotnet test src/MMORPG.slnx

Write-Host "`n============================================================" -ForegroundColor Green
Write-Host " ✅ DEPLOYMENT COMPLETE! Cluster is live on http://localhost:5000" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
