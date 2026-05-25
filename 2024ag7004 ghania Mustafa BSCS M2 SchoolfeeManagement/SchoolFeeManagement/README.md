#  School Fee Management System
### C# Windows Forms Desktop Application | University Submission

---

##  Project Overview

A fully functional **School Fee Management System** built with **C# Windows Forms** (.NET 6) and **SQLite** database. The system allows school administrators to manage student records, record fee payments, view transaction histories, and generate reports — all with a clean blue and grey themed UI.

---

##  Features Implemented

| Feature | Status |
|---------|--------|
| User Login / Authentication | ✅ Complete |
| Add / Edit / Delete Students | ✅ Complete |
| Fee Payment Recording | ✅ Complete |
| View All Payments (with filters) | ✅ Complete |
| Reports & Analytics | ✅ Complete |
| Logout Functionality | ✅ Complete |
| Blue & Grey Color Theme | ✅ Complete |
| SQLite Database Integration | ✅ Complete |
| Receipt Generation | ✅ Complete |
| CSV Export | ✅ Complete |

---

##  Technology Stack

| Component | Technology |
|-----------|-----------|
| Language | C# (.NET 6) |
| UI Framework | Windows Forms (WinForms) |
| Database | SQLite (via System.Data.SQLite) |
| IDE | Visual Studio 2022 |
| DB Connection | ADO.NET with SQLiteConnection |

---

##  Project Structure

```
SchoolFeeManagement/
├── SchoolFeeManagement.sln           ← Visual Studio Solution
├── Database_Schema.sql               ← Reference SQL script
├── README.md                         ← This file
└── SchoolFeeManagement/
    ├── SchoolFeeManagement.csproj    ← Project file (.NET 6 WinForms)
    ├── Program.cs                    ← Entry point, DB init
    ├── Database/
    │   └── DatabaseHelper.cs         ← All DB operations (CRUD)
    ├── Helpers/
    │   ├── ThemeHelper.cs            ← Blue/grey UI theme
    │   └── SessionHelper.cs          ← Login session state
    └── Forms/
        ├── LoginForm.cs              ← Login screen
        ├── MainDashboardForm.cs      ← Main window + sidebar nav
        ├── StudentManagementControl.cs ← Add/Edit/View students
        ├── FeePaymentControl.cs      ← Record payments + receipts
        ├── ViewPaymentsControl.cs    ← Browse/filter/export payments
        └── ReportsControl.cs         ← Analytics & summaries
```

---

##  How to Run

### Prerequisites
- Visual Studio 2022 (Community or higher)
- .NET 6 SDK installed
- Internet connection (first build only — to restore NuGet packages)

### Steps

1. **Open the solution**
   - Double-click `SchoolFeeManagement.sln`
   - OR: File → Open → Project/Solution → select the `.sln` file

2. **Restore NuGet packages**
   - Visual Studio does this automatically on build
   - OR: Right-click Solution → "Restore NuGet Packages"

3. **Build and Run**
   - Press `F5` or click the green ▶ Run button
   - The SQLite database is **auto-created** on first launch

4. **Login**
   - Username: `admin`
   - Password: `admin123`

>  The database file `SchoolFeeDB.sqlite` is created automatically in the same directory as the `.exe` file. No manual database setup is required.

---

##  Database Schema

The SQLite database contains 4 tables:

### `Users`
| Column | Type | Description |
|--------|------|-------------|
| UserID | INTEGER PK | Auto-increment |
| Username | TEXT UNIQUE | Login username |
| Password | TEXT | Login password |
| FullName | TEXT | Display name |
| Role | TEXT | Admin/User role |

### `Students`
| Column | Type | Description |
|--------|------|-------------|
| StudentID | INTEGER PK | Auto-increment |
| StudentNumber | TEXT UNIQUE | e.g. STU001 |
| FirstName, LastName | TEXT | Student name |
| DateOfBirth | TEXT | ISO date format |
| Gender | TEXT | Male/Female/Other |
| Grade | TEXT | Grade 1–12 |
| ParentName, ParentPhone | TEXT | Guardian contact |
| Email, Address | TEXT | Contact info |

### `FeeTypes`
| Column | Type | Description |
|--------|------|-------------|
| FeeTypeID | INTEGER PK | Auto-increment |
| FeeName | TEXT | e.g. Tuition Fee |
| Amount | REAL | Default fee amount |

### `Payments`
| Column | Type | Description |
|--------|------|-------------|
| PaymentID | INTEGER PK | Auto-increment |
| StudentID | INTEGER FK | Links to Students |
| FeeTypeID | INTEGER FK | Links to FeeTypes |
| AmountPaid | REAL | Actual paid amount |
| PaymentMethod | TEXT | Cash/Card/etc. |
| ReceiptNumber | TEXT UNIQUE | Auto-generated |
| AcademicYear | TEXT | e.g. 2024/2025 |
| Term | TEXT | Term 1–4 |

---

## 🎨 UI Design

- **Color Theme**: Blue (#195496) + Dark Blue (#0F3460) + Grey (#6C7582)
- **All forms share consistent styling** via `ThemeHelper.cs`
- **Sidebar navigation** on the main dashboard
- **DataGridViews** styled with blue headers
- **Card widgets** on the dashboard for key metrics

---

## 👩‍💻 Default Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |

---

## 📊 Reports Available

1. **Revenue by Fee Type** — totals per fee category
2. **Revenue by Grade** — breakdown per grade level
3. **Monthly Revenue** — trend analysis by month
4. **Top Paying Students** — ranked list by total paid
5. **Term Summary** — revenue per academic term

---

## 📝 Notes for Submission

- The database is **SQLite** (file-based, no server setup needed)
- All database operations use **parameterized queries** to prevent SQL injection
- The system uses **ADO.NET** with `System.Data.SQLite` NuGet package
- **No external configuration** required — just open and run
- Sample data (3 students, 5 fee types) is seeded automatically

---

*Developed for University Submission — School Fee Management System*
*C# Windows Forms | .NET 6 | SQLite | Visual Studio 2022*
