# 1. Find the most recently built nupkg file
$packagePath = Join-Path -Path ".\GeoLib\bin\Release" -ChildPath "*.nupkg"

Write-Host $packagePath

$latestPackage = Get-ChildItem -Path $packagePath | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $latestPackage) {
    Write-Error "No .nupkg files found in GeoLib\bin\Release directory"
    exit 1
}

Write-Host "Found package: $($latestPackage.Name)"

# 2. Read API key from environment variable
$apiKey = $env:ST_NG_PKG_TOKEN

if (-not $apiKey) {
    Write-Error "ST_NG_PKG_TOKEN environment variable not set"
    exit 1
}

# 3. Push the package to NuGet.org
Write-Host "Publishing package to NuGet.org..."
dotnet nuget push $latestPackage.FullName --api-key $apiKey --source https://api.nuget.org/v3/index.json

if ($LASTEXITCODE -eq 0) {
    Write-Host "Package published successfully!" -ForegroundColor Green
} else {
    Write-Error "Failed to publish package. Exit code: $LASTEXITCODE"
}