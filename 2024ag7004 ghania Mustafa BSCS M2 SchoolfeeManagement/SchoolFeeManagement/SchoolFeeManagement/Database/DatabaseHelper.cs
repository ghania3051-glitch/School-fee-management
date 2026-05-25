using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace SchoolFeeManagement.Database
{
    /// <summary>
    /// DatabaseHelper – static utility class that centralises ALL database access.
    /// 
    /// Design decision: A single static helper (rather than a repository per entity)
    /// keeps the data-access layer thin and easy to follow for a course project while
    /// still separating DB concerns from UI code (Forms folder).
    /// 
    /// Database: SQLite via System.Data.SQLite (ADO.NET provider).
    /// The .sqlite file is created automatically next to the .exe on first run,
    /// so no separate database-server installation is required.
    /// </summary>
    public static class DatabaseHelper
    {
        // Path to the SQLite file – placed beside the running executable
        private static readonly string _dbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SchoolFeeDB.sqlite");

        // ADO.NET connection string
        private static string ConnectionString => $"Data Source={_dbPath};Version=3;";

        // ────────────────────────────────────────────────────────────
        //  INITIALISATION
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called once at application startup (Program.cs).
        /// Creates the database file if it does not exist, runs CREATE TABLE statements
        /// with IF NOT EXISTS guards (idempotent), and seeds default data.
        /// </summary>
        public static void InitializeDatabase()
        {
            // Create the physical file if missing
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using var conn = new SQLiteConnection(ConnectionString);
            conn.Open();

            // ── Table: Users ──────────────────────────────────────
            ExecSql(conn, @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserID    INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username  TEXT NOT NULL UNIQUE,
                    Password  TEXT NOT NULL,
                    FullName  TEXT NOT NULL,
                    Role      TEXT NOT NULL DEFAULT 'Admin',
                    CreatedAt TEXT DEFAULT (datetime('now'))
                )");

            // ── Table: Students ───────────────────────────────────
            ExecSql(conn, @"
                CREATE TABLE IF NOT EXISTS Students (
                    StudentID      INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentNumber  TEXT NOT NULL UNIQUE,
                    FirstName      TEXT NOT NULL,
                    LastName       TEXT NOT NULL,
                    DateOfBirth    TEXT,
                    Gender         TEXT,
                    Grade          TEXT NOT NULL,
                    ParentName     TEXT,
                    ParentPhone    TEXT,
                    Email          TEXT,
                    Address        TEXT,
                    EnrollmentDate TEXT DEFAULT (datetime('now')),
                    IsActive       INTEGER DEFAULT 1   -- soft-delete flag
                )");

            // ── Table: FeeTypes ───────────────────────────────────
            ExecSql(conn, @"
                CREATE TABLE IF NOT EXISTS FeeTypes (
                    FeeTypeID   INTEGER PRIMARY KEY AUTOINCREMENT,
                    FeeName     TEXT NOT NULL,
                    Amount      REAL NOT NULL,
                    Description TEXT,
                    IsActive    INTEGER DEFAULT 1
                )");

            // ── Table: Payments ───────────────────────────────────
            // Junction between Students and FeeTypes; each row is one payment transaction.
            ExecSql(conn, @"
                CREATE TABLE IF NOT EXISTS Payments (
                    PaymentID     INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentID     INTEGER NOT NULL,
                    FeeTypeID     INTEGER NOT NULL,
                    AmountPaid    REAL    NOT NULL,
                    PaymentDate   TEXT    DEFAULT (datetime('now')),
                    PaymentMethod TEXT    NOT NULL DEFAULT 'Cash',
                    ReceiptNumber TEXT    NOT NULL UNIQUE,
                    AcademicYear  TEXT    NOT NULL,
                    Term          TEXT    NOT NULL,
                    Notes         TEXT,
                    RecordedBy    INTEGER,
                    FOREIGN KEY (StudentID)  REFERENCES Students(StudentID),
                    FOREIGN KEY (FeeTypeID)  REFERENCES FeeTypes(FeeTypeID),
                    FOREIGN KEY (RecordedBy) REFERENCES Users(UserID)
                )");

            // ── Seed default admin user (password stored as plain text for simplicity) ──
            ExecSql(conn, @"
                INSERT OR IGNORE INTO Users (Username, Password, FullName, Role)
                VALUES ('admin', 'admin123', 'System Administrator', 'Admin')");

            // ── Seed default fee types ──
            ExecSql(conn, @"
                INSERT OR IGNORE INTO FeeTypes (FeeTypeID, FeeName, Amount, Description) VALUES
                (1, 'Tuition Fee',     5000.00, 'Regular tuition fee per term'),
                (2, 'Registration Fee', 500.00, 'Annual registration fee'),
                (3, 'Library Fee',      200.00, 'Library and resource fee'),
                (4, 'Sports Fee',       300.00, 'Sports and extracurricular activities'),
                (5, 'Exam Fee',         400.00, 'Examination administration fee')");

            // ── Seed sample students for demonstration ──
            ExecSql(conn, @"
                INSERT OR IGNORE INTO Students
                    (StudentNumber, FirstName, LastName, DateOfBirth, Gender,
                     Grade, ParentName, ParentPhone, Email, Address)
                VALUES
                    ('STU001','James',  'Smith',  '2010-03-15','Male',  'Grade 8','Robert Smith','0712345678','james.smith@email.com','123 Main St'),
                    ('STU002','Sarah',  'Johnson','2011-07-22','Female','Grade 7','Mary Johnson', '0723456789','sarah.j@email.com',    '456 Oak Ave'),
                    ('STU003','Michael','Brown',  '2009-11-05','Male',  'Grade 9','David Brown',  '0734567890','mbrown@email.com',     '789 Pine Rd')");
        }

        // ────────────────────────────────────────────────────────────
        //  PUBLIC QUERY HELPERS
        // ────────────────────────────────────────────────────────────

        /// <summary>Opens a new connection and returns it (caller must dispose).</summary>
        public static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Executes a SELECT query and returns results as a DataTable.
        /// Uses parameterized queries to prevent SQL injection.
        /// </summary>
        public static DataTable ExecuteQuery(string sql, SQLiteParameter[]? parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);

            using var adapter = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Executes an INSERT / UPDATE / DELETE statement.
        /// Returns the number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string sql, SQLiteParameter[]? parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        /// <summary>Executes a query that returns a single scalar value (e.g. COUNT, SUM).</summary>
        public static object? ExecuteScalar(string sql, SQLiteParameter[]? parameters = null)
        {
            using var conn = GetConnection();
            using var cmd = new SQLiteCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        // ────────────────────────────────────────────────────────────
        //  PRIVATE HELPERS
        // ────────────────────────────────────────────────────────────

        /// <summary>Executes a non-query SQL statement on an already-open connection (used during init).</summary>
        private static void ExecSql(SQLiteConnection conn, string sql)
        {
            using var cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }
    }
}
