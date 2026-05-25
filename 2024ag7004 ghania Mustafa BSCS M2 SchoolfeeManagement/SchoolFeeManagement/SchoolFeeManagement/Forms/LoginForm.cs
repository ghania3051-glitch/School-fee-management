using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// LoginForm – the first screen the user sees.
    ///
    /// Layout: split-panel design.
    ///   Left  (blue)  – branding panel with app name and tagline.
    ///   Right (white) – credential inputs and login button.
    ///
    /// On successful authentication, the session variables are set and
    /// MainDashboardForm is shown. This form is hidden (not closed) so the
    /// application process stays alive; closing the dashboard exits the process.
    /// </summary>
    public class LoginForm : Form
    {
        private TextBox txtUsername = null!;
        private TextBox txtPassword = null!;
        private Label   lblError    = null!;

        public LoginForm()
        {
            InitializeComponents();
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.Text            = "School Fee Management System – Login";
            this.Size            = new Size(900, 560);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = ThemeHelper.VeryLightGrey;

            // ── LEFT PANEL — branding ─────────────────────────────
            var leftPanel = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 420,
                BackColor = ThemeHelper.PrimaryBlue
            };

            var lblIcon = new Label
            {
                Text     = "🎓",
                Font     = new Font("Segoe UI", 48),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(160, 100)
            };

            var lblAppName = new Label
            {
                Text      = "School Fee\nManagement\nSystem",
                Font      = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(80, 190),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTagline = new Label
            {
                Text      = "Efficient  •  Reliable  •  Secure",
                Font      = new Font("Segoe UI", 11, FontStyle.Italic),
                ForeColor = ThemeHelper.LightBlue,
                AutoSize  = true,
                Location  = new Point(95, 340)
            };

            var lblVersion = new Label
            {
                Text      = "Version 1.0  |  Visual Programming CS-412",
                Font      = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(150, 200, 255),
                AutoSize  = true,
                Location  = new Point(82, 470)
            };

            leftPanel.Controls.AddRange(new Control[] { lblIcon, lblAppName, lblTagline, lblVersion });

            // ── RIGHT PANEL — login form ──────────────────────────
            var rightPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ThemeHelper.White
            };

            var lblWelcome = new Label
            {
                Text      = "Welcome Back",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue,
                AutoSize  = true,
                Location  = new Point(90, 70)
            };

            var lblSignIn = new Label
            {
                Text      = "Sign in to your account to continue",
                Font      = new Font("Segoe UI", 10),
                ForeColor = ThemeHelper.MediumGrey,
                AutoSize  = true,
                Location  = new Point(90, 108)
            };

            // Horizontal divider
            var divider = new Panel
            {
                Location  = new Point(60, 138),
                Size      = new Size(330, 2),
                BackColor = ThemeHelper.LightGrey
            };

            // Username field
            var lblUser = new Label
            {
                Text      = "Username",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkGrey,
                AutoSize  = true,
                Location  = new Point(60, 158)
            };
            txtUsername = new TextBox
            {
                Location    = new Point(60, 178),
                Size        = new Size(330, 35),
                Font        = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor   = ThemeHelper.VeryLightGrey
            };

            // Password field
            var lblPass = new Label
            {
                Text      = "Password",
                Font      = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkGrey,
                AutoSize  = true,
                Location  = new Point(60, 228)
            };
            txtPassword = new TextBox
            {
                Location     = new Point(60, 248),
                Size         = new Size(330, 35),
                Font         = new Font("Segoe UI", 11),
                BorderStyle  = BorderStyle.FixedSingle,
                PasswordChar = '●',
                BackColor    = ThemeHelper.VeryLightGrey
            };

            // Error label (hidden until a failed login)
            lblError = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 9),
                ForeColor = ThemeHelper.DangerRed,
                AutoSize  = true,
                Location  = new Point(60, 296)
            };

            // Login button
            var btnLogin = new Button
            {
                Text      = "LOGIN",
                Location  = new Point(60, 322),
                Size      = new Size(330, 45),
                BackColor = ThemeHelper.PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Hint for markers / assessors
            var lblHint = new Label
            {
                Text      = "Default credentials:  admin  /  admin123",
                Font      = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = ThemeHelper.MediumGrey,
                AutoSize  = true,
                Location  = new Point(90, 382)
            };

            // Footer strip
            var footer = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 36,
                BackColor = ThemeHelper.LightGrey
            };
            var lblFooter = new Label
            {
                Text      = "School Fee Management System  –  Visual Programming CS-412",
                Font      = new Font("Segoe UI", 8),
                ForeColor = ThemeHelper.MediumGrey,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            footer.Controls.Add(lblFooter);

            rightPanel.Controls.AddRange(new Control[]
            {
                lblWelcome, lblSignIn, divider,
                lblUser, txtUsername,
                lblPass, txtPassword,
                lblError, btnLogin, lblHint, footer
            });

            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);

            // Allow pressing Enter to submit from either field
            txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };
        }

        // ────────────────────────────────────────────────────────────
        //  EVENT HANDLERS
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates credentials against the Users table using a parameterised query.
        /// On success: populates SessionHelper and opens the dashboard.
        /// On failure: shows an error label and clears the password field.
        /// </summary>
        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            lblError.Text = "";

            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Basic empty-field guard
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "⚠  Please enter both username and password.";
                return;
            }

            // Parameterised query – prevents SQL injection
            const string sql = "SELECT UserID, FullName, Role FROM Users WHERE Username=@u AND Password=@p";
            var parameters = new SQLiteParameter[]
            {
                new SQLiteParameter("@u", username),
                new SQLiteParameter("@p", password)
            };

            try
            {
                var dt = DatabaseHelper.ExecuteQuery(sql, parameters);

                if (dt.Rows.Count > 0)
                {
                    // Populate session state
                    SessionHelper.CurrentUserID   = Convert.ToInt32(dt.Rows[0]["UserID"]);
                    SessionHelper.CurrentUsername = username;
                    SessionHelper.CurrentFullName = dt.Rows[0]["FullName"].ToString()!;
                    SessionHelper.CurrentRole     = dt.Rows[0]["Role"].ToString()!;

                    // Open dashboard; hide (not close) this form
                    var dashboard = new MainDashboardForm();
                    dashboard.Show();
                    this.Hide();
                }
                else
                {
                    lblError.Text = "⚠  Invalid username or password. Please try again.";
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error during login:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
