using System.Drawing;
using System.Windows.Forms;

namespace SchoolFeeManagement.Helpers
{
    /// <summary>
    /// ThemeHelper – central repository for all UI styling constants and factory methods.
    ///
    /// Design decision: Instead of hard-coding colours and fonts in each form, every
    /// styled control is created through this class. Changing a single colour here
    /// updates the entire application, satisfying the DRY principle and making the
    /// blue-and-grey theme consistent across all six screens.
    /// </summary>
    public static class ThemeHelper
    {
        // ── Primary Blue palette ──────────────────────────────────────
        public static readonly Color PrimaryBlue   = Color.FromArgb(25, 84, 150);
        public static readonly Color DarkBlue      = Color.FromArgb(15, 52, 96);
        public static readonly Color MediumBlue    = Color.FromArgb(41, 121, 204);
        public static readonly Color LightBlue     = Color.FromArgb(173, 216, 230);
        public static readonly Color AccentBlue    = Color.FromArgb(0, 150, 255);

        // ── Grey palette ─────────────────────────────────────────────
        public static readonly Color DarkGrey      = Color.FromArgb(52, 58, 64);
        public static readonly Color MediumGrey    = Color.FromArgb(108, 117, 125);
        public static readonly Color LightGrey     = Color.FromArgb(233, 236, 239);
        public static readonly Color VeryLightGrey = Color.FromArgb(248, 249, 250);
        public static readonly Color White         = Color.White;

        // ── Status colours ────────────────────────────────────────────
        public static readonly Color SuccessGreen  = Color.FromArgb(40, 167, 69);
        public static readonly Color DangerRed     = Color.FromArgb(220, 53, 69);
        public static readonly Color WarningOrange = Color.FromArgb(255, 193, 7);

        // ── Fonts ─────────────────────────────────────────────────────
        public static readonly Font HeaderFont    = new Font("Segoe UI", 18, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new Font("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font NormalFont    = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font BoldFont      = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font SmallFont     = new Font("Segoe UI", 9,  FontStyle.Regular);

        // ────────────────────────────────────────────────────────────
        //  FACTORY METHODS – Reusable styled controls
        // ────────────────────────────────────────────────────────────

        /// <summary>Creates a dark-blue top banner with a title and optional subtitle line.</summary>
        public static Panel CreateHeader(string title, string subtitle = "")
        {
            var panel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = PrimaryBlue,
                Padding   = new Padding(20, 10, 20, 10)
            };

            var lblTitle = new Label
            {
                Text      = title,
                Font      = HeaderFont,
                ForeColor = White,
                AutoSize  = true,
                Location  = new System.Drawing.Point(20, 12)
            };
            panel.Controls.Add(lblTitle);

            if (!string.IsNullOrEmpty(subtitle))
            {
                var lblSub = new Label
                {
                    Text      = subtitle,
                    Font      = SmallFont,
                    ForeColor = LightBlue,
                    AutoSize  = true,
                    Location  = new System.Drawing.Point(22, 48)
                };
                panel.Controls.Add(lblSub);
            }

            return panel;
        }

        /// <summary>Creates a primary (blue) action button.</summary>
        public static Button CreatePrimaryButton(string text, int width = 130, int height = 38)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                BackColor = PrimaryBlue,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font      = BoldFont,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>Creates a secondary (grey) button for neutral actions like Refresh or Cancel.</summary>
        public static Button CreateSecondaryButton(string text, int width = 130, int height = 38)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                BackColor = MediumGrey,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font      = BoldFont,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>Creates a danger (red) button for destructive actions like Delete.</summary>
        public static Button CreateDangerButton(string text, int width = 130, int height = 38)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                BackColor = DangerRed,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font      = BoldFont,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>Creates a success (green) button for positive actions like Add or Save.</summary>
        public static Button CreateSuccessButton(string text, int width = 130, int height = 38)
        {
            var btn = new Button
            {
                Text      = text,
                Width     = width,
                Height    = height,
                BackColor = SuccessGreen,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font      = BoldFont,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        /// <summary>
        /// Creates a styled DataGridView with blue column headers and alternating row colours.
        /// All grids in the application are created through this method for consistency.
        /// </summary>
        public static DataGridView CreateStyledGrid()
        {
            var grid = new DataGridView
            {
                BackgroundColor        = White,
                BorderStyle            = BorderStyle.None,
                RowHeadersVisible      = false,
                AllowUserToAddRows     = false,
                AllowUserToDeleteRows  = false,
                ReadOnly               = true,
                SelectionMode          = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode    = DataGridViewAutoSizeColumnsMode.Fill,
                Font                   = SmallFont,
                GridColor              = LightGrey,
                CellBorderStyle        = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None
            };

            // Blue column headers
            grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = White;
            grid.ColumnHeadersDefaultCellStyle.Font      = BoldFont;
            grid.ColumnHeadersDefaultCellStyle.Padding   = new Padding(5);
            grid.ColumnHeadersHeight                     = 38;
            grid.EnableHeadersVisualStyles               = false;

            // Row styles
            grid.DefaultCellStyle.BackColor          = White;
            grid.DefaultCellStyle.ForeColor          = DarkGrey;
            grid.DefaultCellStyle.SelectionBackColor = LightBlue;
            grid.DefaultCellStyle.SelectionForeColor = DarkGrey;
            grid.DefaultCellStyle.Padding            = new Padding(5, 4, 5, 4);

            // Zebra striping
            grid.AlternatingRowsDefaultCellStyle.BackColor = VeryLightGrey;

            return grid;
        }

        /// <summary>
        /// Creates a coloured summary card (Panel) used on the dashboard to display a KPI metric.
        /// </summary>
        /// <param name="x">Left position inside parent panel.</param>
        /// <param name="y">Top position inside parent panel.</param>
        /// <param name="w">Card width.</param>
        /// <param name="h">Card height.</param>
        /// <param name="title">Small label below the value (e.g. "Total Students").</param>
        /// <param name="value">Large bold number displayed on the card.</param>
        /// <param name="color">Background colour (one of the Blue/Grey palette colours).</param>
        public static Panel CreateCardPanel(int x, int y, int w, int h,
                                            string title, string value, Color color)
        {
            var card = new Panel
            {
                Location  = new System.Drawing.Point(x, y),
                Size      = new System.Drawing.Size(w, h),
                BackColor = color,
                Padding   = new Padding(15)
            };

            var lblVal = new Label
            {
                Text      = value,
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = White,
                AutoSize  = true,
                Location  = new System.Drawing.Point(15, 15)
            };

            var lblTitle = new Label
            {
                Text      = title,
                Font      = SmallFont,
                ForeColor = Color.FromArgb(200, 230, 255),
                AutoSize  = true,
                Location  = new System.Drawing.Point(15, 58)
            };

            card.Controls.Add(lblVal);
            card.Controls.Add(lblTitle);
            return card;
        }
    }
}
