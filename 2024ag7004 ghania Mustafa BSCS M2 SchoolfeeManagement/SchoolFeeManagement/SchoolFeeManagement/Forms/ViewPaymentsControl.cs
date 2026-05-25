using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// ViewPaymentsControl – browse, filter, and manage all payment records.
    ///
    /// Features:
    ///   • DataGridView listing all Payments joined with Students and FeeTypes.
    ///   • Real-time text search (student name, student number, receipt number).
    ///   • Term and Academic Year combo filters.
    ///   • Delete selected payment record (with confirmation).
    ///   • Export currently visible rows to a CSV file.
    /// </summary>
    public class ViewPaymentsControl : UserControl
    {
        private DataGridView gridPayments = null!;
        private TextBox      txtSearch    = null!;
        private ComboBox     cboTerm      = null!;
        private ComboBox     cboYear      = null!;
        private Label        lblCount     = null!;

        public ViewPaymentsControl()
        {
            InitializeComponents();
            LoadPayments();
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.BackColor = ThemeHelper.VeryLightGrey;

            var header = ThemeHelper.CreateHeader("Payment Records",
                "View, search, filter and export all fee payment transactions");
            this.Controls.Add(header);

            // ── Toolbar ──────────────────────────────────────────
            var toolbar = new Panel
            {
                Location  = new Point(0, 82),
                Size      = new Size(980, 55),
                BackColor = ThemeHelper.White
            };

            var lblSearch = new Label { Text = "🔍  Search:", AutoSize = true, Location = new Point(10, 17), Font = ThemeHelper.BoldFont, ForeColor = ThemeHelper.DarkGrey };
            txtSearch = new TextBox { Location = new Point(90, 14), Size = new Size(210, 28), Font = ThemeHelper.NormalFont, BorderStyle = BorderStyle.FixedSingle };
            txtSearch.TextChanged += (s, e) => LoadPayments();

            var lblTerm = new Label { Text = "Term:", AutoSize = true, Location = new Point(315, 17), Font = ThemeHelper.BoldFont, ForeColor = ThemeHelper.DarkGrey };
            cboTerm = new ComboBox { Location = new Point(355, 14), Size = new Size(110, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeHelper.NormalFont };
            cboTerm.Items.AddRange(new[] { "All Terms", "Term 1", "Term 2", "Term 3", "Term 4" });
            cboTerm.SelectedIndex = 0;
            cboTerm.SelectedIndexChanged += (s, e) => LoadPayments();

            var lblYear = new Label { Text = "Year:", AutoSize = true, Location = new Point(478, 17), Font = ThemeHelper.BoldFont, ForeColor = ThemeHelper.DarkGrey };
            cboYear = new ComboBox { Location = new Point(516, 14), Size = new Size(130, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeHelper.NormalFont };
            cboYear.Items.Add("All Years");
            int yr = DateTime.Now.Year;
            for (int i = -2; i <= 2; i++) cboYear.Items.Add($"{yr + i}/{yr + i + 1}");
            cboYear.SelectedIndex = 0;
            cboYear.SelectedIndexChanged += (s, e) => LoadPayments();

            var btnExport = ThemeHelper.CreatePrimaryButton("📋  Export CSV", 140, 32);
            btnExport.Location = new Point(662, 12);
            btnExport.Click   += ExportToCSV;

            var btnDelete = ThemeHelper.CreateDangerButton("🗑  Delete", 110, 32);
            btnDelete.Location = new Point(810, 12);
            btnDelete.Click   += DeletePayment;

            lblCount = new Label { Text = "Loading…", AutoSize = true, Location = new Point(10, 39), Font = ThemeHelper.SmallFont, ForeColor = ThemeHelper.MediumGrey };

            toolbar.Controls.AddRange(new Control[]
                { lblSearch, txtSearch, lblTerm, cboTerm, lblYear, cboYear,
                  btnExport, btnDelete, lblCount });
            this.Controls.Add(toolbar);

            // ── Payments DataGridView ─────────────────────────────
            gridPayments = ThemeHelper.CreateStyledGrid();
            gridPayments.Location = new Point(0, 140);
            gridPayments.Size     = new Size(980, 490);
            this.Controls.Add(gridPayments);
        }

        // ────────────────────────────────────────────────────────────
        //  DATA LOADING
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Queries the database and populates the grid, applying search text
        /// and combo-box filters dynamically.
        /// </summary>
        private void LoadPayments()
        {
            string search = txtSearch.Text.Trim();
            string term   = cboTerm.SelectedItem?.ToString() ?? "All Terms";
            string year   = cboYear.SelectedItem?.ToString() ?? "All Years";

            // Build the base query – always hide the internal PaymentID from the user
            string sql = @"
                SELECT p.PaymentID           AS _ID,
                       p.ReceiptNumber       AS 'Receipt No.',
                       s.StudentNumber       AS 'Student No.',
                       s.FirstName || ' ' || s.LastName AS 'Student Name',
                       s.Grade,
                       ft.FeeName            AS 'Fee Type',
                       'R' || printf('%.2f', p.AmountPaid) AS 'Amount',
                       p.PaymentMethod       AS 'Method',
                       p.Term,
                       p.AcademicYear        AS 'Acad. Year',
                       DATE(p.PaymentDate)   AS 'Date',
                       p.Notes
                FROM   Payments p
                JOIN   Students s  ON p.StudentID  = s.StudentID
                JOIN   FeeTypes ft ON p.FeeTypeID  = ft.FeeTypeID
                WHERE  1=1";

            // Text search filter
            if (!string.IsNullOrEmpty(search))
                sql += $@" AND (s.FirstName LIKE '%{search}%'
                             OR s.LastName   LIKE '%{search}%'
                             OR s.StudentNumber LIKE '%{search}%'
                             OR p.ReceiptNumber LIKE '%{search}%')";

            // Term filter
            if (term != "All Terms")
                sql += $" AND p.Term = '{term}'";

            // Academic year filter
            if (year != "All Years")
                sql += $" AND p.AcademicYear = '{year}'";

            sql += " ORDER BY p.PaymentDate DESC";

            var dt = DatabaseHelper.ExecuteQuery(sql);
            gridPayments.DataSource = dt;

            // Hide the internal _ID column from the user
            if (gridPayments.Columns.Contains("_ID"))
                gridPayments.Columns["_ID"]!.Visible = false;

            lblCount.Text = $"Showing {dt.Rows.Count} record(s)";
        }

        // ────────────────────────────────────────────────────────────
        //  EVENT HANDLERS
        // ────────────────────────────────────────────────────────────

        /// <summary>Deletes the selected payment record after a confirmation prompt.</summary>
        private void DeletePayment(object? sender, EventArgs e)
        {
            if (gridPayments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a payment record to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string receipt = gridPayments.SelectedRows[0].Cells["Receipt No."].Value?.ToString() ?? "";

            if (MessageBox.Show(
                    $"Delete payment record {receipt}?\nThis action cannot be undone.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            try
            {
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Payments WHERE ReceiptNumber=@r",
                    new SQLiteParameter[] { new("@r", receipt) });

                LoadPayments();
                MessageBox.Show("Payment record deleted.", "Deleted",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete record:\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Exports all currently visible grid rows to a comma-separated CSV file.
        /// The user chooses the save path via a SaveFileDialog.
        /// </summary>
        private void ExportToCSV(object? sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Filter   = "CSV File|*.csv",
                FileName = $"Payments_{DateTime.Now:yyyyMMdd}.csv",
                Title    = "Export Payments to CSV"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            var sb = new StringBuilder();

            // Header row (visible columns only)
            foreach (DataGridViewColumn col in gridPayments.Columns)
                if (col.Visible) sb.Append($"{col.HeaderText},");
            sb.AppendLine();

            // Data rows
            foreach (DataGridViewRow row in gridPayments.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                    if (gridPayments.Columns[cell.ColumnIndex].Visible)
                        sb.Append($"\"{cell.Value}\",");
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
            MessageBox.Show($"Export complete!\n{dlg.FileName}",
                "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
