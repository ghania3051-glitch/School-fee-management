# DEPLOYMENT GUIDE
## School Fee Management System — Visual Programming (CS-412)

---

## System Requirements

| Component | Requirement |
|-----------|-------------|
| Operating System | Windows 10 / Windows 11 (64-bit) |
| .NET Runtime | .NET 6 Desktop Runtime (or SDK) |
| Visual Studio | Visual Studio 2022 (Community or higher) |
| Database | SQLite — **no separate server install needed** |
| RAM | 2 GB minimum |
| Disk Space | ~50 MB (including .NET if already installed) |

---

## Step-by-Step Installation (Another Machine)

### Option A — Run from Visual Studio (Development)

1. **Install prerequisites**
   - Download and install [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
   - Download and install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/) — include the **.NET desktop development** workload

2. **Extract the ZIP**
   - Unzip `SchoolFeeManagement.zip` to any folder (e.g. `C:\Projects\SchoolFeeManagement`)

3. **Open the solution**
   - Double-click `SchoolFeeManagement.sln`
   - Visual Studio opens the project

4. **Restore NuGet packages**
   - Visual Studio does this automatically on first build
   - OR: right-click the solution → **Restore NuGet Packages**
   - The only external package is `System.Data.SQLite` (v1.0.118)

5. **Build and run**
   - Press **F5** (Debug) or **Ctrl+F5** (without debugger)
   - The database file `SchoolFeeDB.sqlite` is created automatically in the output directory

---

### Option B — Run the Published Executable (No Visual Studio)

> If a published/self-contained build is included in the ZIP:

1. Install [.NET 6 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) (if not already installed)
2. Navigate to the `publish/` folder
3. Double-click `SchoolFeeManagement.exe`
4. The database is auto-created on first run

---

## Database Setup

**No manual setup required.** The application uses SQLite with an auto-generated file.

- On first launch, `SchoolFeeDB.sqlite` is created beside the `.exe`
- All tables (`Users`, `Students`, `FeeTypes`, `Payments`) are created automatically
- Sample data (3 students, 5 fee types) is seeded automatically

### Manual Recreation (if needed)

If you need to rebuild the database from scratch:
1. Delete `SchoolFeeDB.sqlite` from the application folder
2. Restart the application — it will recreate everything

Alternatively, run the provided `Database_Schema.sql` script using any SQLite tool (e.g. [DB Browser for SQLite](https://sqlitebrowser.org/)).

---

## Default Login Credentials

| Field | Value |
|-------|-------|
| Username | `admin` |
| Password | `admin123` |

---

## Environment Variables / Configuration

None required. All settings are embedded in the application. The database path is resolved at runtime relative to the executable location.

---

## Firewall / Network

No network access is required. The application is fully offline / local.

---

## Troubleshooting

| Problem | Solution |
|---------|---------|
| "Could not load file or assembly System.Data.SQLite" | Rebuild the solution to restore NuGet packages |
| Database not found / can't create | Ensure the application folder has write permission |
| Blank DataGridViews | Check that the database was created — look for `SchoolFeeDB.sqlite` next to the `.exe` |
| Login fails with correct credentials | Delete `SchoolFeeDB.sqlite` and relaunch to re-seed the admin user |

---

## Cross-Platform Notes

This project targets **net6.0-windows** with Windows Forms. It runs on:
- Windows 10 (version 1903+) ✅
- Windows 11 ✅
- Windows Server 2019 / 2022 ✅

Linux/macOS are not supported because Windows Forms requires the Windows Desktop Runtime.
