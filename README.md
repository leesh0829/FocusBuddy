# FocusBuddy

FocusBuddy is a Windows desktop utility for personal focus analytics.
It tracks foreground window usage, categorizes activity, stores data in local SQLite, and offers a non-destructive Focus Mode.

## Stack

- .NET 8 WPF
- MVVM via CommunityToolkit.Mvvm
- SQLite via Microsoft.Data.Sqlite
- Win32 API interop for foreground window tracking
- LiveChartsCore for charts
- Serilog file logging
- DI via Microsoft.Extensions.DependencyInjection

## Features

- Automatic active-window tracking (1-second polling)
- Category engine with JSON-driven rules
- Dashboard:
  - Today total usage time
  - Pie chart by category
  - Bar chart for last 7 days
  - Top 5 apps today
- Focus Mode:
  - Reminder popup for blacklisted apps
  - Optional auto-minimize for distracting apps
- System tray operation:
  - Open dashboard
  - Toggle focus mode
  - Exit app
- Data location:
  - Database: `%AppData%/FocusBuddy/focusbuddy.db`
  - Settings: `%AppData%/FocusBuddy/settings.json`
  - Logs: `%AppData%/FocusBuddy/focusbuddy.log`

## Project Structure

```text
FocusBuddy/
  Models/
  ViewModels/
  Views/
  Services/
  Data/
  Helpers/
  Config/
```

## Required NuGet Packages

- CommunityToolkit.Mvvm
- LiveChartsCore.SkiaSharpView.WPF
- Microsoft.Data.Sqlite
- Microsoft.Extensions.DependencyInjection
- Serilog
- Serilog.Sinks.File

## Open / Build / Run (Windows)

### 1) Where to open the project
Open the repository root folder (the folder that contains `FocusBuddy.csproj`).

- Visual Studio 2022:
  - `File > Open > Folder...` and select this repo root, **or**
  - `File > Open > Project/Solution...` and choose `FocusBuddy.csproj`.
- VS Code:
  - `File > Open Folder...` and select this repo root.

### 2) Build from terminal
Run these commands at repo root:

```powershell
dotnet restore
dotnet build -c Release
```

### 3) Run

```powershell
dotnet run
```

## 빠른 한국어 안내 (열기/빌드/실행)

1. **어디서 열기**  
   `FocusBuddy.csproj` 파일이 있는 폴더(현재 저장소 루트)를 Visual Studio 또는 VS Code에서 엽니다.

2. **빌드** (저장소 루트 터미널)

```powershell
dotnet restore
dotnet build -c Release
```

3. **실행**

```powershell
dotnet run
```

4. **실행 파일 위치(빌드 후)**

```text
bin/Release/net8.0-windows/
```

## Publish single-file executable

```powershell
dotnet publish -c Release -r win-x64 `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true
```

Published output is located under:

```text
bin/Release/net8.0-windows/win-x64/publish/
```
