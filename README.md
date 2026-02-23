# FocusBuddy

> A friendly focus companion that shows where your day actually goes.

**FocusBuddy** is a Windows desktop app that automatically tracks which apps you actively use,
then helps you reduce distractions when it’s time to focus.

It is designed less like a developer analytics tool and more like a
**personal productivity partner you can use every day**.

---

## Why FocusBuddy?

Use FocusBuddy if you want to:

- catch the “just 5 minutes” pattern that turns into an hour on distracting apps
- understand where your time went at the end of the day
- stay on track during study or deep work sessions
- improve productivity with real data, not just guesswork

---

## Core Features

### 1) Automatic activity tracking
No start/stop timer needed. FocusBuddy tracks your active foreground window automatically.

### 2) Category-based insights
Your app usage is grouped into categories so you can quickly spot patterns like:
- Work vs. Entertainment
- Productive vs. Distracting time

### 3) Clear daily dashboard
- Total usage time today
- Usage breakdown by category
- Last 7 days trend
- Top 5 most-used apps today

### 4) Focus Mode
When distracting apps are opened during focus time, FocusBuddy can:
- show a reminder popup
- optionally auto-minimize selected distracting apps

### 5) System tray quick controls
Run quietly in the background and control it from the tray:
- Open Dashboard
- Toggle Focus Mode
- Exit App

---

## Who is it for?

FocusBuddy is a great fit for:

- students building consistent study routines
- remote workers who want better attention control
- anyone who feels “busy all day” but struggles to see real progress
- users who want lightweight, automatic time-awareness without complexity

---

## Where is data stored?

FocusBuddy stores data locally on your machine.

- Database: `%AppData%/FocusBuddy/focusbuddy.db`
- Settings: `%AppData%/FocusBuddy/settings.json`
- Logs: `%AppData%/FocusBuddy/focusbuddy.log`

---

## Quick Start

```bash
dotnet restore
dotnet build -c Release
dotnet run
```

---

## Tech Stack (brief)

- .NET 8 WPF
- MVVM (CommunityToolkit.Mvvm)
- SQLite (Microsoft.Data.Sqlite)
- Serilog

> More detailed developer architecture and deployment docs can be split into `docs/` later.
