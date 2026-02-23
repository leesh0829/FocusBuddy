# FocusBuddy

FocusBuddy is a production-structured Windows desktop utility for personal focus analytics.
It tracks foreground window usage, categorizes activity, stores data in local SQLite, and offers a non-destructive Focus Mode.

## Stack

- .NET 8 WPF
- MVVM via CommunityToolkit.Mvvm
- SQLite via Microsoft.Data.Sqlite
- Win32 API interop for foreground window tracking
- Serilog file logging
- DI via Microsoft.Extensions.DependencyInjection

## Features

- Automatic active-window tracking (1-second polling)
- Category engine with JSON-driven rules
- Dashboard:
  - Today total usage time
  - Category usage breakdown
  - Last 7 days totals
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
- Microsoft.Data.Sqlite
- Microsoft.Extensions.DependencyInjection
- Serilog
- Serilog.Sinks.File

## Build

```bash
dotnet restore
dotnet build -c Release
```

## Run

```bash
dotnet run
```

## Build MSI installer (Visual Studio Installer Projects)

1. Install the **Visual Studio Installer Projects** extension in Visual Studio.
2. Open `FocusBuddy.sln` and set **Release** configuration.
3. Build the `FocusBuddy Setup` project (or build the whole solution).
4. The MSI will be generated at `Setup/Release/FocusBuddy.msi` (Debug build: `Setup/Debug/FocusBuddy.msi`).

### Command line (Developer Command Prompt for VS)

```bat
devenv FocusBuddy.sln /Build Release
```

## Publish single-file executable

```bash
dotnet publish -c Release -r win-x64 \
  -p:PublishSingleFile=true \
  -p:SelfContained=true \
  -p:IncludeNativeLibrariesForSelfExtract=true
```

Published output is located under:

```text
bin/Release/net8.0-windows/win-x64/publish/
```
