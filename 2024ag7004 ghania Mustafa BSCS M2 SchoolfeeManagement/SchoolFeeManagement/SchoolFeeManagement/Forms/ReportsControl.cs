using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// ReportsControl – five analytical report views for financial oversight.
    ///
    /// Each report button triggers a different SQL aggregation query and
    /// displays the result in the shared DataGridView below.
    ///
    /// Reports available:
    ///   1. Revenue by Fee Type  – totals per fee category
    ///   2. Revenue by Grade     – breakdown per school grade
    ///   3. Monthly Revenue      – month-by-month income trend
    ///   4. Top Paying Students  – ranked by total amount paid
    ///   5. Term Summary         – revenue per academic term
    /// </summary>
    public class ReportsControl : UserControl
    {
        private DataGridView gridReport  = null!;
        private Label        lblTitle    = null!;
        private Label        lblSummary  = null!;

        public ReportsControl()
        {
            InitializeComponents();
            LoadRevenueByFeeType(); // default report on first load
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.BackColor = ThemeHelper.VeryLightGrey;

            var header = ThemeHelper.CreateHeader("Reports & Analytics",
                "Financial summaries and student fee analysis");
            this.Controls.Add(header);

            // ── Report-type selector buttons ──────────────────────
            var btnPanel = new Panel
            {
                Location  = new Point(0, 82),
                Size      = new Size(980, 52),
                BackColor = ThemeHelper.White
            };

            var reports = new (string Label, Action Handler)[]
            {
                ("💰  Fee Type",     LoadRevenueByFeeType),
                ("👥  By Grade",     LoadRevenueByGrade),
                ("📅  Monthly",      LoadMonthlyRevenue),
                ("🏆  Top Students", LoadTopStudents),
                ("📊  Term Summary", LoadTermSummary)
            };

            int x = 10;
            foreach (var r in reports)
            {
                var handler = r.Handler; // capture loop variable correctly
                var btn = new Button
                {
                    Text      = r.Label,
                    Location  = new Point(x, 10),
                    Size      = new Size(165, 33),
                    BackColor = ThemeHelper.PrimaryBlue,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor    = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => handler();
                btnPanel.Controls.Add(btn);
                x += 175;
            }
            this.Controls.Add(btnPanel);

            // ── Report title and summary labels ───────────────────
            lblTitle = new Label
            {
                Location  = new Point(10, 146),
                AutoSize  = true,
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue
            };
            lblSummary = new Label
            {
                Location  = new Point(10, 176),
                AutoSize  = true,
                Font      = ThemeHelper.NormalFont,
                ForeColor = ThemeHelper.MediumGrey
            };

            // ── Results grid ──────────────────────────────────────
            gridReport = ThemeHelper.CreateStyledGrid();
            gridReport.Location = new Point(0, 202);
            gridReport.Size     = new Size(980, 430);

            this.Controls.AddRange(new Control[] { lblTitle, lblSummary, gridReport });
        }

        // ────────────────────────────────────────────────────────────
        //  REPORT QUERIES
        // ────────────────────────────────────────────────────────────

        /// <summary>Report 1: Total revenue grouped by fee category, sorted highest first.</summary>
        private void LoadRevenueByFeeType()
        {
            lblTitle.Text = "💰  Revenue by Fee Type";

            const string sql = @"
                SELECT ft.FeeName                                    AS 'Fee Type',
                       COUNT(p.PaymentID)                            AS 'Transactions',
                       'R' || printf('%.2f', SUM(p.AmountPaid))     AS 'Total Collected',
                       'R' || printf('%.2f', AVG(p.AmountPaid))     AS 'Average per Transaction'
                FROM   Payments  p
                JOIN   FeeTypes ft ON p.FeeTypeID = ft.FeeTypeID
                GROUP  BY ft.FeeName
                ORDER  BY SUM(p.AmountPaid) DESC";

            gridReport.DataSource = DatabaseHelper.ExecuteQuery(sql);

            var grandTotal  = DatabaseHelper.ExecuteScalar("SELECT IFNULL(SUM(AmountPaid),0) FROM Payments") ?? 0;
            var txnCount    = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Payments") ?? 0;
            lblSummary.Text = $"Grand Total: R{Convert.ToDouble(grandTotal):N2}   |   Total Transactions: {txnCount}";
        }

        /// <summary>Report 2: Revenue and student count per school grade.</summary>
        private void LoadRevenueByGrade()
        {
            lblTitle.Text = "👥  Revenue by Grade";

            const string sql = @"
                SELECT s.Grade,
                       COUNT(DISTINCT s.StudentID)                   AS 'Students Enrolled',
                       COUNT(p.PaymentID)                            AS 'Payments Made',
                       'R' || printf('%.2f', IFNULL(SUM(p.AmountPaid),0)) AS 'Total Collected'
                FROM   Students s
                LEFT   JOIN Payments p ON s.StudentID = p.StudentID
                WHERE  s.IsActive = 1
                GROUP  BY s.Grade
                ORDER  BY s.Grade";

            gridReport.DataSource = DatabaseHelper.ExecuteQuery(sql);
            lblSummary.Text = "Revenue breakdown per grade level (includes students with no payments)";
        }

        /// <summary>Report 3: Monthly revenue trend (newest month first).</summary>
        private void LoadMonthlyRevenue()
        {
            lblTitle.Text = "📅  Monthly Revenue";

            const string sql = @"
                SELECT strftime('%Y-%m', PaymentDate)               AS 'Month',
                       COUNT(*)                                      AS 'Transactions',
                       'R' || printf('%.2f', SUM(AmountPaid))       AS 'Revenue',
                       'R' || printf('%.2f', AVG(AmountPaid))       AS 'Avg per Transaction'
                FROM   Payments
                GROUP  BY strftime('%Y-%m', PaymentDate)
                ORDER  BY strftime('%Y-%m', PaymentDate) DESC";

            gridReport.DataSource = DatabaseHelper.ExecuteQuery(sql);
            lblSummary.Text = "Monthly revenue trends – most recent first";
        }

        /// <summary>Report 4: Top 20 students ranked by total amount paid.</summary>
        private void LoadTopStudents()
        {
            lblTitle.Text = "🏆  Top Paying Students";

            const string sql = @"
                SELECT s.StudentNumber                               AS 'Student No.',
                       s.FirstName || ' ' || s.LastName             AS 'Name',
                       s.Grade,
                       COUNT(p.PaymentID)                           AS 'Payments',
                       'R' || printf('%.2f', SUM(p.AmountPaid))    AS 'Total Paid',
                       DATE(MAX(p.PaymentDate))                     AS 'Last Payment'
                FROM   Students s
                JOIN   Payments p ON s.StudentID = p.StudentID
                GROUP  BY s.StudentID
                ORDER  BY SUM(p.AmountPaid) DESC
                LIMIT  20";

            gridReport.DataSource = DatabaseHelper.ExecuteQuery(sql);
            lblSummary.Text = "Top 20 students ranked by total amount paid (students with no payments are excluded)";
        }

        /// <summary>Report 5: Revenue summary grouped by academic year and term.</summary>
        private void LoadTermSummary()
        {
            lblTitle.Text = "📊  Term Summary";

            const string sql = @"
                SELECT AcademicYear                                  AS 'Academic Year',
                       Term,
                       COUNT(*)                                      AS 'Transactions',
                       COUNT(DISTINCT StudentID)                     AS 'Students Paid',
                       'R' || printf('%.2f', SUM(AmountPaid))       AS 'Total Revenue'
                FROM   Payments
                GROUP  BY AcademicYear, Term
                ORDER  BY AcademicYear DESC, Term";

            gridReport.DataSource = DatabaseHelper.ExecuteQuery(sql);
            lblSummary.Text = "Revenue and participation summary per academic term";
        }
    }
}
