using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// FeePaymentControl – handles recording a new fee payment for a student.
    /// Displays a payment form on the left and recent payment history + receipt on the right.
    /// </summary>
    public class FeePaymentControl : UserControl
    {
        // ── Form fields ──────────────────────────────────────────────
        private ComboBox cboStudent = null!;
        private ComboBox cboFeeType = null!;
        private ComboBox cboPaymentMethod = null!;
        private ComboBox cboTerm = null!;
        private ComboBox cboAcademicYear = null!;
        private TextBox txtAmount = null!;
        private TextBox txtReceiptNo = null!;
        private TextBox txtNotes = null!;
        private Label lblFeeAmount = null!;
        private Label lblStudentInfo = null!;

        // ── Right-panel widgets ───────────────────────────────────────
        private Panel receiptPanel = null!;
        private Label lblReceiptContent = null!;
        private DataGridView gridHistory = null!;

        // ── Constructor ──────────────────────────────────────────────
        public FeePaymentControl()
        {
            InitializeComponents();
            LoadDropdowns();      // Populate student and fee-type combos from DB
            LoadRecentPayments(); // Show last 10 payments in the history grid
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.BackColor = ThemeHelper.VeryLightGrey;

            // Page header
            var header = ThemeHelper.CreateHeader("Fee Payment", "Record a new student fee payment");
            this.Controls.Add(header);

            // Left card – payment form
            var leftPanel = new Panel
            {
                Location = new Point(0, 85),
                Size = new Size(490, 590),
                BackColor = ThemeHelper.White,
                Padding = new Padding(20)
            };

            // Right card – history + receipt
            var rightPanel = new Panel
            {
                Location = new Point(500, 85),
                Size = new Size(460, 590),
                BackColor = ThemeHelper.White
            };

            BuildPaymentForm(leftPanel);
            BuildHistoryPanel(rightPanel);

            this.Controls.Add(leftPanel);
            this.Controls.Add(rightPanel);
        }

        /// <summary>Builds all input controls for the payment form inside the given parent panel.</summary>
        private void BuildPaymentForm(Panel parent)
        {
            // ── helpers for terse layout ──
            int y = 15;
            Label FL(string t)
            {
                var l = new Label
                {
                    Text = t,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = ThemeHelper.DarkGrey,
                    AutoSize = true,
                    Location = new Point(15, y)
                };
                y += 17;
                return l;
            }
            Control FT(Control c) { c.Location = new Point(15, y); y += 40; return c; }

            // Section title
            var titleLbl = new Label
            {
                Text = "Payment Details",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue,
                AutoSize = true,
                Location = new Point(15, 15)
            };
            y = 45;
            var line = new Panel { Location = new Point(15, y), Size = new Size(450, 1), BackColor = ThemeHelper.LightBlue };
            y += 15;

            // ── Student ──
            var lStudent = FL("Select Student *");
            cboStudent = new ComboBox { Size = new Size(450, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeHelper.NormalFont };
            cboStudent.SelectedIndexChanged += CboStudent_Changed;
            var cStudent = FT(cboStudent);

            lblStudentInfo = new Label
            {
                Text = "",
                Font = ThemeHelper.SmallFont,
                ForeColor = ThemeHelper.MediumGrey,
                AutoSize = true,
                Location = new Point(15, y)
            };
            y += 22;

            // ── Fee Type ──
            var lFee = FL("Fee Type *");
            cboFeeType = new ComboBox { Size = new Size(450, 28), DropDownStyle = ComboBoxStyle.DropDownList, Font = ThemeHelper.NormalFont };
            cboFeeType.SelectedIndexChanged += CboFeeType_Changed;
            var cFee = FT(cboFeeType);

            lblFeeAmount = new Label
            {
                Text = "Standard fee: —",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = ThemeHelper.MediumBlue,
                AutoSize = true,
                Location = new Point(15, y)
            };
            y += 22;

            // ── Amount ──
            var lAmount = FL("Amount to Pay (R) *");
            // Plain numeric text box – no N2 formatting to avoid parse errors
            txtAmount = new TextBox
            {
                Size = new Size(450, 28),
                Font = ThemeHelper.NormalFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            var cAmount = FT(txtAmount);

            // ── Payment Method ──
            var lMethod = FL("Payment Method *");
            cboPaymentMethod = new ComboBox
            {
                Size = new Size(210, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeHelper.NormalFont
            };
            cboPaymentMethod.Items.AddRange(new[] { "Cash", "Bank Transfer", "Credit Card", "Debit Card", "Mobile Payment", "Cheque" });
            cboPaymentMethod.SelectedIndex = 0;
            var cMethod = FT(cboPaymentMethod);

            // ── Term & Academic Year side-by-side ──
            var lTerm = new Label { Text = "Term *", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ThemeHelper.DarkGrey, AutoSize = true, Location = new Point(15, y) };
            var lYear = new Label { Text = "Academic Year *", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ThemeHelper.DarkGrey, AutoSize = true, Location = new Point(250, y) };
            y += 17;

            cboTerm = new ComboBox
            {
                Location = new Point(15, y),
                Size = new Size(200, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeHelper.NormalFont
            };
            cboTerm.Items.AddRange(new[] { "Term 1", "Term 2", "Term 3", "Term 4" });

            cboAcademicYear = new ComboBox
            {
                Location = new Point(250, y),
                Size = new Size(215, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = ThemeHelper.NormalFont
            };
            int yr = DateTime.Now.Year;
            for (int i = -1; i <= 2; i++)
                cboAcademicYear.Items.Add($"{yr + i}/{yr + i + 1}");
            cboAcademicYear.SelectedIndex = 1; // default = current year
            y += 42;

            // ── Receipt Number (auto-generated, read-only) ──
            var lReceipt = new Label { Text = "Receipt Number (auto)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ThemeHelper.DarkGrey, AutoSize = true, Location = new Point(15, y) };
            y += 17;
            txtReceiptNo = new TextBox
            {
                Location = new Point(15, y),
                Size = new Size(450, 28),
                Font = ThemeHelper.NormalFont,
                BorderStyle = BorderStyle.FixedSingle,
                Text = SessionHelper.GenerateReceiptNumber(),
                ReadOnly = true,
                BackColor = ThemeHelper.LightGrey
            };
            y += 40;

            // ── Notes ──
            var lNotes = new Label { Text = "Notes (optional)", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ThemeHelper.DarkGrey, AutoSize = true, Location = new Point(15, y) };
            y += 17;
            txtNotes = new TextBox
            {
                Location = new Point(15, y),
                Size = new Size(450, 55),
                Font = ThemeHelper.NormalFont,
                BorderStyle = BorderStyle.FixedSingle,
                Multiline = true
            };
            y += 68;

            // ── Process Payment button ──
            var btnSubmit = new Button
            {
                Text = "💳  Process Payment",
                Location = new Point(15, y),
                Size = new Size(220, 42),
                BackColor = ThemeHelper.SuccessGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += SubmitPayment_Click;

            // ── Clear button ──
            var btnClear = new Button
            {
                Text = "🔄  Clear",
                Location = new Point(250, y),
                Size = new Size(120, 42),
                BackColor = ThemeHelper.MediumGrey,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = ThemeHelper.BoldFont,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => ClearForm();

            // Add every control to the parent panel
            parent.Controls.AddRange(new Control[]
            {
                titleLbl, line,
                lStudent, cStudent, lblStudentInfo,
                lFee, cFee, lblFeeAmount,
                lAmount, cAmount,
                lMethod, cMethod,
                lTerm, lYear, cboTerm, cboAcademicYear,
                lReceipt, txtReceiptNo,
                lNotes, txtNotes,
                btnSubmit, btnClear
            });
        }

        /// <summary>Builds the recent-payments grid and receipt preview on the right panel.</summary>
        private void BuildHistoryPanel(Panel parent)
        {
            var lbl = new Label
            {
                Text = "Recent Payments",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue,
                AutoSize = true,
                Location = new Point(10, 15)
            };

            gridHistory = ThemeHelper.CreateStyledGrid();
            gridHistory.Location = new Point(5, 45);
            gridHistory.Size = new Size(450, 295);

            // Receipt preview area
            receiptPanel = new Panel
            {
                Location = new Point(5, 350),
                Size = new Size(450, 230),
                BackColor = Color.FromArgb(240, 248, 255),
                Visible = false,
                Padding = new Padding(10)
            };

            lblReceiptContent = new Label
            {
                Font = new Font("Courier New", 8.5f),
                ForeColor = ThemeHelper.DarkGrey,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            receiptPanel.Controls.Add(lblReceiptContent);
            parent.Controls.AddRange(new Control[] { lbl, gridHistory, receiptPanel });
        }

        // ────────────────────────────────────────────────────────────
        //  DATA LOADING
        // ────────────────────────────────────────────────────────────

        /// <summary>Populates the Student and FeeType combo boxes from the database.</summary>
        private void LoadDropdowns()
        {
            try
            {
                // Students
                cboStudent.Items.Clear();
                var students = DatabaseHelper.ExecuteQuery(
                    "SELECT StudentID, StudentNumber, FirstName || ' ' || LastName AS Name, Grade " +
                    "FROM Students WHERE IsActive=1 ORDER BY LastName, FirstName");
                foreach (DataRow row in students.Rows)
                    cboStudent.Items.Add(new ComboItem(
                        $"{row["StudentNumber"]} - {row["Name"]} ({row["Grade"]})",
                        Convert.ToInt32(row["StudentID"])));

                // Fee types
                cboFeeType.Items.Clear();
                var fees = DatabaseHelper.ExecuteQuery(
                    "SELECT FeeTypeID, FeeName, Amount FROM FeeTypes WHERE IsActive=1 ORDER BY FeeName");
                foreach (DataRow row in fees.Rows)
                    cboFeeType.Items.Add(new ComboItemFee(
                        row["FeeName"].ToString()!,
                        Convert.ToInt32(row["FeeTypeID"]),
                        Convert.ToDouble(row["Amount"])));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dropdowns:\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Refreshes the recent-payments grid (last 10 records).</summary>
        private void LoadRecentPayments()
        {
            try
            {
                string sql = @"
                    SELECT p.ReceiptNumber   AS 'Receipt',
                           s.FirstName || ' ' || s.LastName AS 'Student',
                           ft.FeeName        AS 'Fee Type',
                           'R' || printf('%.2f', p.AmountPaid) AS 'Amount',
                           p.Term,
                           DATE(p.PaymentDate) AS 'Date'
                    FROM   Payments p
                    JOIN   Students  s  ON p.StudentID  = s.StudentID
                    JOIN   FeeTypes  ft ON p.FeeTypeID  = ft.FeeTypeID
                    ORDER  BY p.PaymentDate DESC
                    LIMIT  10";

                gridHistory.DataSource = DatabaseHelper.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading payment history:\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────
        //  EVENT HANDLERS
        // ────────────────────────────────────────────────────────────

        /// <summary>When a student is selected, shows their grade and parent contact info.</summary>
        private void CboStudent_Changed(object? sender, EventArgs e)
        {
            if (cboStudent.SelectedItem is ComboItem item)
            {
                var dt = DatabaseHelper.ExecuteQuery(
                    "SELECT Grade, ParentName, ParentPhone FROM Students WHERE StudentID=@id",
                    new SQLiteParameter[] { new("@id", item.ID) });

                if (dt.Rows.Count > 0)
                    lblStudentInfo.Text =
                        $"Grade: {dt.Rows[0]["Grade"]}  |  " +
                        $"Parent: {dt.Rows[0]["ParentName"]}  |  " +
                        $"Phone: {dt.Rows[0]["ParentPhone"]}";
            }
        }

        /// <summary>When a fee type is selected, pre-fills the amount with its standard value.</summary>
        private void CboFeeType_Changed(object? sender, EventArgs e)
        {
            if (cboFeeType.SelectedItem is ComboItemFee item)
            {
                lblFeeAmount.Text = $"Standard fee: R{item.Amount:N2}";
                // Store as plain number (no commas) so TryParse always succeeds
                txtAmount.Text = item.Amount.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Validates inputs, inserts the payment record into the database,
        /// shows a receipt preview and refreshes the history grid.
        /// </summary>
        private void SubmitPayment_Click(object? sender, EventArgs e)
        {
            // ── Validation ────────────────────────────────────────
            if (cboStudent.SelectedItem == null)
            {
                ShowError("Please select a student."); return;
            }
            if (cboFeeType.SelectedItem == null)
            {
                ShowError("Please select a fee type."); return;
            }
            if (cboTerm.SelectedItem == null)
            {
                ShowError("Please select a term."); return;
            }
            if (cboAcademicYear.SelectedItem == null)
            {
                ShowError("Please select an academic year."); return;
            }

            // Parse amount – accept both "5000" and "5000.00" (invariant culture)
            string rawAmount = txtAmount.Text.Trim().Replace(",", "");
            if (!double.TryParse(rawAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out double amount) || amount <= 0)
            {
                ShowError("Please enter a valid positive amount (e.g. 5000 or 5000.00)."); return;
            }

            // ── Build parameters ─────────────────────────────────
            var student = (ComboItem)cboStudent.SelectedItem;
            var fee = (ComboItemFee)cboFeeType.SelectedItem;
            string receiptNo = txtReceiptNo.Text.Trim();

            var parameters = new SQLiteParameter[]
            {
                new("@sid",    student.ID),
                new("@fid",    fee.FeeTypeID),
                new("@amt",    amount),
                new("@method", cboPaymentMethod.SelectedItem?.ToString() ?? "Cash"),
                new("@rno",    receiptNo),
                new("@year",   cboAcademicYear.SelectedItem!.ToString()),
                new("@term",   cboTerm.SelectedItem!.ToString()),
                new("@notes",  txtNotes.Text.Trim()),
                new("@uid",    SessionHelper.CurrentUserID)
            };

            string sql = @"
                INSERT INTO Payments
                    (StudentID, FeeTypeID, AmountPaid, PaymentMethod,
                     ReceiptNumber, AcademicYear, Term, Notes, RecordedBy)
                VALUES
                    (@sid, @fid, @amt, @method, @rno, @year, @term, @notes, @uid)";

            try
            {
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save payment:\n{ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ── Post-save actions ────────────────────────────────
            ShowReceipt(student.Text, fee.Text, amount, receiptNo);
            LoadRecentPayments();

            // Generate a fresh receipt number for the next payment
            txtReceiptNo.Text = SessionHelper.GenerateReceiptNumber();
            txtNotes.Clear();

            MessageBox.Show("✅ Payment recorded successfully!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ────────────────────────────────────────────────────────────
        //  HELPERS
        // ────────────────────────────────────────────────────────────

        /// <summary>Renders a receipt preview in the right panel after a successful payment.</summary>
        private void ShowReceipt(string student, string feeType, double amount, string receiptNo)
        {
            string receipt =
                $"  ╔══════════════════════════════╗\n" +
                $"  ║      PAYMENT RECEIPT         ║\n" +
                $"  ╚══════════════════════════════╝\n\n" +
                $"  Receipt No : {receiptNo}\n" +
                $"  Date       : {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                $"  Student    : {student}\n" +
                $"  Fee Type   : {feeType}\n" +
                $"  Amount     : R{amount:N2}\n" +
                $"  Method     : {cboPaymentMethod.SelectedItem}\n" +
                $"  Term       : {cboTerm.SelectedItem}\n" +
                $"  Year       : {cboAcademicYear.SelectedItem}\n" +
                $"  Cashier    : {SessionHelper.CurrentFullName}\n\n" +
                $"  ──────────────────────────────\n" +
                $"  OFFICIAL RECEIPT - KEEP SAFE\n";

            lblReceiptContent.Text = receipt;
            receiptPanel.Visible = true;
        }

        /// <summary>Resets all form fields back to their default empty state.</summary>
        private void ClearForm()
        {
            cboStudent.SelectedIndex = -1;
            cboFeeType.SelectedIndex = -1;
            cboTerm.SelectedIndex = -1;
            txtAmount.Clear();
            txtNotes.Clear();
            lblStudentInfo.Text = "";
            lblFeeAmount.Text = "Standard fee: —";
            txtReceiptNo.Text = SessionHelper.GenerateReceiptNumber();
            receiptPanel.Visible = false;
        }

        private static void ShowError(string message) =>
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    // ──────────────────────────────────────────────────────────────
    //  Combo-box item wrappers
    // ──────────────────────────────────────────────────────────────

    /// <summary>Wraps a student record for use in a ComboBox.</summary>
    public class ComboItem
    {
        public string Text { get; }
        public int ID { get; }
        public ComboItem(string text, int id) { Text = text; ID = id; }
        public override string ToString() => Text;
    }

    /// <summary>Wraps a fee-type record (including standard amount) for use in a ComboBox.</summary>
    public class ComboItemFee
    {
        public string Text { get; }
        public int FeeTypeID { get; }
        public double Amount { get; }
        public ComboItemFee(string text, int id, double amount)
        {
            Text = text; FeeTypeID = id; Amount = amount;
        }
        public override string ToString() => Text;
    }
}
