using System;
using System.Drawing;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// MainDashboardForm – the shell window shown after a successful login.
    ///
    /// Responsibilities:
    ///   • Renders the persistent top bar and sidebar navigation.
    ///   • Acts as a host: swapping UserControls into the content panel
    ///     when the user clicks a sidebar button (a simple manual "router").
    ///   • Loads the dashboard summary view by default.
    ///
    /// Navigation pattern: rather than opening new Form windows for each section,
    /// this form loads UserControl subclasses into a Dock.Fill panel. This avoids
    /// multiple windows cluttering the taskbar and gives a single-page-app feel.
    /// </summary>
    public class MainDashboardForm : Form
    {
        private Panel  contentPanel = null!;  // swappable content area
        private Button btnActive    = null!;  // tracks the currently highlighted sidebar button

        public MainDashboardForm()
        {
            InitializeComponents();
            ShowDashboard(); // default landing view
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.Text        = "School Fee Management System";
            this.Size        = new Size(1200, 750);
            this.MinimumSize = new Size(1050, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor   = ThemeHelper.VeryLightGrey;

            // ── TOP BAR ───────────────────────────────────────────
            var topBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 58,
                BackColor = ThemeHelper.DarkBlue
            };

            var lblTitle = new Label
            {
                Text      = "🎓   School Fee Management System",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(235, 14)
            };

            var lblUserInfo = new Label
            {
                Text      = $"👤  {SessionHelper.CurrentFullName}     |     {DateTime.Now:dddd, d MMMM yyyy}",
                Font      = new Font("Segoe UI", 9),
                ForeColor = ThemeHelper.LightBlue,
                AutoSize  = true,
                Location  = new Point(780, 20)
            };

            topBar.Controls.Add(lblTitle);
            topBar.Controls.Add(lblUserInfo);

            // ── SIDEBAR ───────────────────────────────────────────
            var sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 225,
                BackColor = ThemeHelper.DarkBlue
            };

            BuildSidebar(sidebar);

            // ── CONTENT AREA ──────────────────────────────────────
            contentPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ThemeHelper.VeryLightGrey
            };

            // Add in reverse Z-order (Fill panel must be added before Left panel)
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebar);
            this.Controls.Add(topBar);

            // Exiting the dashboard = full application exit
            this.FormClosing += (s, e) =>
            {
                SessionHelper.Clear();
                Application.Exit();
            };
        }

        /// <summary>Populates the sidebar with navigation buttons and a logout button.</summary>
        private void BuildSidebar(Panel sidebar)
        {
            // (label, tag, yPosition) for each nav item
            var items = new (string Label, string Tag, int Y)[]
            {
                ("🏠   Dashboard",      "dashboard", 10),
                ("👥   Students",       "students",  60),
                ("💰   Fee Payment",    "payment",  110),
                ("📋   View Payments",  "payments", 160),
                ("📊   Reports",        "reports",  210),
            };

            // Separator line above Logout
            var sep = new Panel
            {
                Location  = new Point(15, 272),
                Size      = new Size(195, 1),
                BackColor = Color.FromArgb(60, 100, 150)
            };
            sidebar.Controls.Add(sep);

            // Logout button (separate so it doesn't get the active highlight)
            var btnLogout = MakeSidebarButton("🚪   Logout", "logout", 285);
            btnLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Are you sure you want to logout?",
                    "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
                {
                    SessionHelper.Clear();
                    new LoginForm().Show();
                    this.Hide();
                }
            };
            sidebar.Controls.Add(btnLogout);

            // Navigation buttons
            foreach (var item in items)
            {
                var btn = MakeSidebarButton(item.Label, item.Tag, item.Y);
                sidebar.Controls.Add(btn);

                // Highlight Dashboard as the default active button
                if (item.Tag == "dashboard")
                {
                    btnActive = btn;
                    HighlightButton(btn);
                }
            }
        }

        /// <summary>Creates a single flat sidebar navigation button.</summary>
        private Button MakeSidebarButton(string label, string tag, int y)
        {
            var btn = new Button
            {
                Text      = label,
                Tag       = tag,
                Location  = new Point(0, y),
                Size      = new Size(225, 48),
                BackColor = ThemeHelper.DarkBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(18, 0, 0, 0),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize          = 0;
            btn.FlatAppearance.MouseOverBackColor  = ThemeHelper.MediumBlue;
            btn.Click += SidebarButton_Click;
            return btn;
        }

        // ────────────────────────────────────────────────────────────
        //  EVENT HANDLERS
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Handles all sidebar navigation clicks.
        /// Highlights the clicked button and loads the appropriate UserControl.
        /// </summary>
        private void SidebarButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;

            HighlightButton(btn);

            switch (btn.Tag?.ToString())
            {
                case "dashboard": ShowDashboard(); break;
                case "students":  LoadControl(new StudentManagementControl()); break;
                case "payment":   LoadControl(new FeePaymentControl()); break;
                case "payments":  LoadControl(new ViewPaymentsControl()); break;
                case "reports":   LoadControl(new ReportsControl()); break;
            }
        }

        // ────────────────────────────────────────────────────────────
        //  NAVIGATION HELPERS
        // ────────────────────────────────────────────────────────────

        /// <summary>Swaps any UserControl into the content panel (disposes the previous one).</summary>
        private void LoadControl(UserControl control)
        {
            contentPanel.Controls.Clear();
            control.Dock = DockStyle.Fill;
            contentPanel.Controls.Add(control);
        }

        /// <summary>Highlights the given sidebar button and restores the previously active one.</summary>
        private void HighlightButton(Button btn)
        {
            if (btnActive != null)
                btnActive.BackColor = ThemeHelper.DarkBlue;

            btn.BackColor = ThemeHelper.MediumBlue;
            btnActive     = btn;
        }

        /// <summary>
        /// Builds and displays the home dashboard:
        /// four KPI cards + recent payments DataGridView.
        /// </summary>
        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();

            // Page header
            var header = ThemeHelper.CreateHeader("Dashboard",
                $"Welcome back, {SessionHelper.CurrentFullName}!");
            contentPanel.Controls.Add(header);

            // ── KPI cards ─────────────────────────────────────────
            // Each card queries the database for a single scalar value
            var totalStudents  = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students WHERE IsActive=1")  ?? 0;
            var totalPayments  = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Payments")                   ?? 0;
            var totalRevenue   = DatabaseHelper.ExecuteScalar("SELECT IFNULL(SUM(AmountPaid),0) FROM Payments")  ?? 0;
            var todayPayments  = DatabaseHelper.ExecuteScalar(
                "SELECT COUNT(*) FROM Payments WHERE DATE(PaymentDate)=DATE('now')") ?? 0;

            var card1 = ThemeHelper.CreateCardPanel(10,  100, 218, 110, "Total Students",   totalStudents.ToString(), ThemeHelper.PrimaryBlue);
            var card2 = ThemeHelper.CreateCardPanel(240, 100, 218, 110, "Total Payments",   totalPayments.ToString(), ThemeHelper.MediumBlue);
            var card3 = ThemeHelper.CreateCardPanel(470, 100, 240, 110, "Total Revenue",    $"R{Convert.ToDouble(totalRevenue):N2}", ThemeHelper.DarkBlue);
            var card4 = ThemeHelper.CreateCardPanel(722, 100, 218, 110, "Today's Payments", todayPayments.ToString(), ThemeHelper.MediumGrey);

            contentPanel.Controls.AddRange(new Control[] { card1, card2, card3, card4 });

            // ── Recent payments grid ──────────────────────────────
            var lblRecent = new Label
            {
                Text      = "Recent Payments",
                Font      = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue,
                AutoSize  = true,
                Location  = new Point(10, 228)
            };
            contentPanel.Controls.Add(lblRecent);

            var grid = ThemeHelper.CreateStyledGrid();
            grid.Location = new Point(10, 258);
            grid.Size     = new Size(940, 345);

            const string sql = @"
                SELECT p.ReceiptNumber          AS 'Receipt No.',
                       s.FirstName || ' ' || s.LastName AS 'Student',
                       s.Grade,
                       ft.FeeName               AS 'Fee Type',
                       'R' || printf('%.2f', p.AmountPaid) AS 'Amount',
                       p.PaymentMethod          AS 'Method',
                       p.Term,
                       p.AcademicYear           AS 'Acad. Year',
                       DATE(p.PaymentDate)      AS 'Date'
                FROM   Payments p
                JOIN   Students s  ON p.StudentID  = s.StudentID
                JOIN   FeeTypes ft ON p.FeeTypeID  = ft.FeeTypeID
                ORDER  BY p.PaymentDate DESC
                LIMIT  15";

            grid.DataSource = DatabaseHelper.ExecuteQuery(sql);
            contentPanel.Controls.Add(grid);
        }
    }
}
