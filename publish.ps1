$ErrorActionPreference = "Stop"

dotnet publish .\ClockWidget\ClockWidget.csproj `
  -c Release `
  -o .\dist

if ($LASTEXITCODE -ne 0) {
  throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "Published to .\dist\ClockWidget.exe"
