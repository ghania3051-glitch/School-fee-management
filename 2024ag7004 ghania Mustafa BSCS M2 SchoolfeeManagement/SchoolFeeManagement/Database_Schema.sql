-- ============================================================
-- School Fee Management System - Database Schema
-- SQLite Database: SchoolFeeDB.sqlite
-- Note: The application auto-creates this on first run.
--       This script is provided as documentation reference.
-- ============================================================

CREATE TABLE IF NOT EXISTS Users (
    UserID      INTEGER PRIMARY KEY AUTOINCREMENT,
    Username    TEXT NOT NULL UNIQUE,
    Password    TEXT NOT NULL,
    FullName    TEXT NOT NULL,
    Role        TEXT NOT NULL DEFAULT 'Admin',
    CreatedAt   TEXT DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Students (
    StudentID       INTEGER PRIMARY KEY AUTOINCREMENT,
    StudentNumber   TEXT NOT NULL UNIQUE,
    FirstName       TEXT NOT NULL,
    LastName        TEXT NOT NULL,
    DateOfBirth     TEXT,
    Gender          TEXT,
    Grade           TEXT NOT NULL,
    ParentName      TEXT,
    ParentPhone     TEXT,
    Email           TEXT,
    Address         TEXT,
    EnrollmentDate  TEXT DEFAULT (datetime('now')),
    IsActive        INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS FeeTypes (
    FeeTypeID   INTEGER PRIMARY KEY AUTOINCREMENT,
    FeeName     TEXT NOT NULL,
    Amount      REAL NOT NULL,
    Description TEXT,
    IsActive    INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Payments (
    PaymentID       INTEGER PRIMARY KEY AUTOINCREMENT,
    StudentID       INTEGER NOT NULL,
    FeeTypeID       INTEGER NOT NULL,
    AmountPaid      REAL NOT NULL,
    PaymentDate     TEXT DEFAULT (datetime('now')),
    PaymentMethod   TEXT NOT NULL DEFAULT 'Cash',
    ReceiptNumber   TEXT NOT NULL UNIQUE,
    AcademicYear    TEXT NOT NULL,
    Term            TEXT NOT NULL,
    Notes           TEXT,
    RecordedBy      INTEGER,
    FOREIGN KEY (StudentID)  REFERENCES Students(StudentID),
    FOREIGN KEY (FeeTypeID)  REFERENCES FeeTypes(FeeTypeID),
    FOREIGN KEY (RecordedBy) REFERENCES Users(UserID)
);

-- ============================================================
-- Default Seed Data (auto-inserted on first run)
-- ============================================================

-- Default admin user
INSERT OR IGNORE INTO Users (Username, Password, FullName, Role)
VALUES ('admin', 'admin123', 'System Administrator', 'Admin');

-- Default fee types
INSERT OR IGNORE INTO FeeTypes (FeeTypeID, FeeName, Amount, Description) VALUES
(1, 'Tuition Fee',     5000.00, 'Regular tuition fee per term'),
(2, 'Registration Fee', 500.00, 'Annual registration fee'),
(3, 'Library Fee',      200.00, 'Library and resource fee'),
(4, 'Sports Fee',       300.00, 'Sports and extracurricular activities'),
(5, 'Exam Fee',         400.00, 'Examination administration fee');

-- Sample students
INSERT OR IGNORE INTO Students (StudentNumber, FirstName, LastName, DateOfBirth, Gender, Grade, ParentName, ParentPhone, Email, Address) VALUES
('STU001', 'James',   'Smith',   '2010-03-15', 'Male',   'Grade 8', 'Robert Smith', '0712345678', 'james.smith@email.com', '123 Main St'),
('STU002', 'Sarah',   'Johnson', '2011-07-22', 'Female', 'Grade 7', 'Mary Johnson', '0723456789', 'sarah.j@email.com',    '456 Oak Ave'),
('STU003', 'Michael', 'Brown',   '2009-11-05', 'Male',   'Grade 9', 'David Brown',  '0734567890', 'mbrown@email.com',     '789 Pine Rd');
