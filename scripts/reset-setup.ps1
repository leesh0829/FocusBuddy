param(
    [switch]$CleanUserData
)

$ErrorActionPreference = "Stop"

Write-Host "[1/5] Checking .NET SDK..."
$dotnetVersion = dotnet --version
Write-Host "Detected .NET SDK: $dotnetVersion"

Write-Host "[2/5] Cleaning build outputs..."
Get-ChildItem -Path . -Directory -Filter bin -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path . -Directory -Filter obj -Recurse | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "[3/5] Clearing NuGet local caches..."
dotnet nuget locals all --clear

Write-Host "[4/5] Restoring and building FocusBuddy..."
dotnet restore FocusBuddy.sln
dotnet build FocusBuddy.sln -c Release

if ($CleanUserData) {
    Write-Host "[5/5] Removing local app data (%AppData%/FocusBuddy)..."
    $appDataPath = Join-Path $env:APPDATA "FocusBuddy"

    if (Test-Path $appDataPath) {
        Remove-Item -Path $appDataPath -Recurse -Force
        Write-Host "Removed: $appDataPath"
    }
    else {
        Write-Host "No local data found at: $appDataPath"
    }
}
else {
    Write-Host "[5/5] Skipped local data cleanup. Use -CleanUserData to wipe app data."
}

Write-Host "Setup reset complete."
