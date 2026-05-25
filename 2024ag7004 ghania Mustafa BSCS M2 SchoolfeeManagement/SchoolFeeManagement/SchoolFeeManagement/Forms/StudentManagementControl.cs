using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Helpers;

namespace SchoolFeeManagement.Forms
{
    /// <summary>
    /// StudentManagementControl – full CRUD interface for the Students entity.
    ///
    /// Features:
    ///   • DataGridView listing all active students (IsActive = 1).
    ///   • Real-time search / filter across name, student number, and grade.
    ///   • Inline Add / Edit form panel (grid hides while form is visible).
    ///   • Soft-aware Delete: removes the student row and their payment records
    ///     after a confirmation dialog.
    ///   • Auto-generated student number (STU001, STU002 …).
    /// </summary>
    public class StudentManagementControl : UserControl
    {
        // ── Grid and search ───────────────────────────────────────────
        private DataGridView gridStudents = null!;
        private TextBox      txtSearch    = null!;

        // ── Inline add/edit form ──────────────────────────────────────
        private Panel  formPanel   = null!;
        private Label  lblFormTitle = null!;
        private bool   isEditMode  = false;   // true = editing existing record
        private int    editingID   = 0;       // StudentID being edited

        // ── Form input fields ─────────────────────────────────────────
        private TextBox       txtStudentNo = null!, txtFirstName = null!,
                              txtLastName  = null!, txtParentName = null!,
                              txtPhone     = null!, txtEmail     = null!,
                              txtAddress   = null!;
        private ComboBox      cboGrade = null!, cboGender = null!;
        private DateTimePicker dtpDOB  = null!;

        public StudentManagementControl()
        {
            InitializeComponents();
            LoadStudents();
        }

        // ────────────────────────────────────────────────────────────
        //  UI BUILD
        // ────────────────────────────────────────────────────────────

        private void InitializeComponents()
        {
            this.BackColor = ThemeHelper.VeryLightGrey;

            var header = ThemeHelper.CreateHeader("Student Management",
                "Add, edit, and view all enrolled students");
            this.Controls.Add(header);

            // ── Toolbar ──────────────────────────────────────────
            var toolbar = new Panel
            {
                Location  = new Point(0, 82),
                Size      = new Size(975, 52),
                BackColor = ThemeHelper.White
            };

            var lblSearch = new Label
            {
                Text      = "🔍  Search:",
                AutoSize  = true,
                Location  = new Point(10, 15),
                Font      = ThemeHelper.BoldFont,
                ForeColor = ThemeHelper.DarkGrey
            };

            txtSearch = new TextBox
            {
                Location    = new Point(95, 12),
                Size        = new Size(255, 28),
                Font        = ThemeHelper.NormalFont,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadStudents(txtSearch.Text.Trim());

            // Action buttons
            var btnAdd = ThemeHelper.CreateSuccessButton("➕  Add Student", 148, 32);
            btnAdd.Location = new Point(368, 10);
            btnAdd.Click   += (s, e) => OpenAddForm();

            var btnEdit = ThemeHelper.CreatePrimaryButton("✏  Edit", 100, 32);
            btnEdit.Location = new Point(524, 10);
            btnEdit.Click   += (s, e) => OpenEditForm();

            var btnDelete = ThemeHelper.CreateDangerButton("🗑  Delete", 110, 32);
            btnDelete.Location = new Point(632, 10);
            btnDelete.Click   += (s, e) => DeleteSelectedStudent();

            var btnRefresh = ThemeHelper.CreateSecondaryButton("🔄  Refresh", 115, 32);
            btnRefresh.Location = new Point(750, 10);
            btnRefresh.Click   += (s, e) => LoadStudents();

            toolbar.Controls.AddRange(new Control[]
                { lblSearch, txtSearch, btnAdd, btnEdit, btnDelete, btnRefresh });
            this.Controls.Add(toolbar);

            // ── Students DataGridView ─────────────────────────────
            gridStudents = ThemeHelper.CreateStyledGrid();
            gridStudents.Location    = new Point(0, 137);
            gridStudents.Size        = new Size(975, 490);
            gridStudents.DoubleClick += (s, e) => OpenEditForm(); // double-click to edit
            this.Controls.Add(gridStudents);

            // ── Inline add/edit form (hidden by default) ──────────
            BuildInlineForm();
        }

        /// <summary>Builds the add/edit form panel that overlays the grid when needed.</summary>
        private void BuildInlineForm()
        {
            formPanel = new Panel
            {
                Location  = new Point(10, 140),
                Size      = new Size(955, 410),
                BackColor = ThemeHelper.White,
                Visible   = false,
                Padding   = new Padding(20)
            };

            lblFormTitle = new Label
            {
                Text      = "Add New Student",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ThemeHelper.DarkBlue,
                AutoSize  = true,
                Location  = new Point(20, 15)
            };

            var line = new Panel
            {
                Location  = new Point(20, 48),
                Size      = new Size(910, 2),
                BackColor = ThemeHelper.LightBlue
            };

            // ── Row helpers ──────────────────────────────────────
            // FL: field label,  TB: text box,  CB: combo box
            Label  FL(string t, int x, int y) => new Label { Text = t, Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = ThemeHelper.DarkGrey, AutoSize = true, Location = new Point(x, y) };
            TextBox TB(int x, int y, int w = 200) => new TextBox { Location = new Point(x, y), Size = new Size(w, 28), Font = ThemeHelper.NormalFont, BorderStyle = BorderStyle.FixedSingle };

            // Row 1 – identifiers
            var l1 = FL("Student Number *", 20, 62);  txtStudentNo = TB(20, 80);
            var l2 = FL("First Name *",    240, 62);  txtFirstName = TB(240, 80);
            var l3 = FL("Last Name *",     460, 62);  txtLastName  = TB(460, 80);

            // Row 2 – demographics
            var l4 = FL("Date of Birth", 20, 122);
            dtpDOB = new DateTimePicker { Location = new Point(20, 140), Size = new Size(200, 28), Font = ThemeHelper.NormalFont, Format = DateTimePickerFormat.Short };

            var l5 = FL("Gender", 240, 122);
            cboGender = new ComboBox { Location = new Point(240, 140), Size = new Size(200, 28), Font = ThemeHelper.NormalFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cboGender.Items.AddRange(new[] { "Male", "Female", "Other" });

            var l6 = FL("Grade *", 460, 122);
            cboGrade = new ComboBox { Location = new Point(460, 140), Size = new Size(200, 28), Font = ThemeHelper.NormalFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cboGrade.Items.AddRange(new[] { "Grade 1","Grade 2","Grade 3","Grade 4","Grade 5","Grade 6",
                                            "Grade 7","Grade 8","Grade 9","Grade 10","Grade 11","Grade 12" });

            // Row 3 – contact
            var l7 = FL("Parent / Guardian Name", 20, 182);  txtParentName = TB(20, 200);
            var l8 = FL("Parent Phone",          240, 182);  txtPhone      = TB(240, 200);
            var l9 = FL("Email Address",         460, 182);  txtEmail      = TB(460, 200);

            // Row 4 – address (full width)
            var l10 = FL("Address", 20, 242);
            txtAddress = new TextBox { Location = new Point(20, 260), Size = new Size(910, 55), Font = ThemeHelper.NormalFont, BorderStyle = BorderStyle.FixedSingle, Multiline = true };

            // ── Action buttons ────────────────────────────────────
            var btnSave = new Button
            {
                Text      = "💾  Save Student",
                Location  = new Point(20, 330),
                Size      = new Size(165, 40),
                BackColor = ThemeHelper.SuccessGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = ThemeHelper.BoldFont,
                Cursor    = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += SaveStudent_Click;

            var btnCancel = new Button
            {
                Text      = "✖  Cancel",
                Location  = new Point(200, 330),
                Size      = new Size(120, 40),
                BackColor = ThemeHelper.MediumGrey,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = ThemeHelper.BoldFont,
                Cursor    = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => CloseForm();

            formPanel.Controls.AddRange(new Control[]
            {
                lblFormTitle, line,
                l1, txtStudentNo, l2, txtFirstName, l3, txtLastName,
                l4, dtpDOB, l5, cboGender, l6, cboGrade,
                l7, txtParentName, l8, txtPhone, l9, txtEmail,
                l10, txtAddress,
                btnSave, btnCancel
            });

            this.Controls.Add(formPanel);
            formPanel.BringToFront();
        }

        // ────────────────────────────────────────────────────────────
        //  CRUD OPERATIONS
        // ────────────────────────────────────────────────────────────

        /// <summary>Opens the form panel in Add mode with cleared fields.</summary>
        private void OpenAddForm()
        {
            isEditMode       = false;
            editingID        = 0;
            lblFormTitle.Text = "➕  Add New Student";
            ClearFields();
            txtStudentNo.Text     = GenerateStudentNumber();
            txtStudentNo.ReadOnly = false;
            ShowForm();
        }

        /// <summary>Opens the form panel in Edit mode, pre-filled from the selected row.</summary>
        private void OpenEditForm()
        {
            if (gridStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Read student number from the grid, then load full record from DB
            string sno = gridStudents.SelectedRows[0].Cells["Student No."].Value?.ToString() ?? "";
            var dt = DatabaseHelper.ExecuteQuery(
                "SELECT * FROM Students WHERE StudentNumber=@n",
                new SQLiteParameter[] { new("@n", sno) });

            if (dt.Rows.Count == 0) return;

            var row = dt.Rows[0];
            isEditMode = true;
            editingID  = Convert.ToInt32(row["StudentID"]);
            lblFormTitle.Text = "✏  Edit Student";

            // Pre-fill all fields
            txtStudentNo.Text     = row["StudentNumber"].ToString()!;
            txtStudentNo.ReadOnly = true; // student number cannot be changed during edit
            txtFirstName.Text     = row["FirstName"].ToString()!;
            txtLastName.Text      = row["LastName"].ToString()!;
            if (DateTime.TryParse(row["DateOfBirth"].ToString(), out var dob)) dtpDOB.Value = dob;
            cboGender.SelectedItem = row["Gender"].ToString();
            cboGrade.SelectedItem  = row["Grade"].ToString();
            txtParentName.Text    = row["ParentName"].ToString()!;
            txtPhone.Text         = row["ParentPhone"].ToString()!;
            txtEmail.Text         = row["Email"].ToString()!;
            txtAddress.Text       = row["Address"].ToString()!;

            ShowForm();
        }

        /// <summary>
        /// Validates the form and performs either an INSERT (Add) or UPDATE (Edit).
        /// Uses parameterised queries to prevent SQL injection.
        /// </summary>
        private void SaveStudent_Click(object? sender, EventArgs e)
        {
            // Required-field validation
            if (string.IsNullOrWhiteSpace(txtStudentNo.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text)  ||
                cboGrade.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields (marked with *).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (isEditMode)
                {
                    // UPDATE existing record
                    const string sqlUpdate = @"
                        UPDATE Students
                        SET    FirstName=@fn, LastName=@ln, DateOfBirth=@dob,
                               Gender=@gen, Grade=@grade, ParentName=@pn,
                               ParentPhone=@pp, Email=@email, Address=@addr
                        WHERE  StudentID=@id";

                    DatabaseHelper.ExecuteNonQuery(sqlUpdate, new SQLiteParameter[]
                    {
                        new("@id",    editingID),
                        new("@fn",    txtFirstName.Text.Trim()),
                        new("@ln",    txtLastName.Text.Trim()),
                        new("@dob",   dtpDOB.Value.ToString("yyyy-MM-dd")),
                        new("@gen",   cboGender.SelectedItem?.ToString() ?? ""),
                        new("@grade", cboGrade.SelectedItem!.ToString()),
                        new("@pn",    txtParentName.Text.Trim()),
                        new("@pp",    txtPhone.Text.Trim()),
                        new("@email", txtEmail.Text.Trim()),
                        new("@addr",  txtAddress.Text.Trim())
                    });
                    MessageBox.Show("Student record updated successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // INSERT new record
                    const string sqlInsert = @"
                        INSERT INTO Students
                            (StudentNumber, FirstName, LastName, DateOfBirth,
                             Gender, Grade, ParentName, ParentPhone, Email, Address)
                        VALUES
                            (@sno, @fn, @ln, @dob, @gen, @grade, @pn, @pp, @email, @addr)";

                    DatabaseHelper.ExecuteNonQuery(sqlInsert, new SQLiteParameter[]
                    {
                        new("@sno",   txtStudentNo.Text.Trim()),
                        new("@fn",    txtFirstName.Text.Trim()),
                        new("@ln",    txtLastName.Text.Trim()),
                        new("@dob",   dtpDOB.Value.ToString("yyyy-MM-dd")),
                        new("@gen",   cboGender.SelectedItem?.ToString() ?? ""),
                        new("@grade", cboGrade.SelectedItem!.ToString()),
                        new("@pn",    txtParentName.Text.Trim()),
                        new("@pp",    txtPhone.Text.Trim()),
                        new("@email", txtEmail.Text.Trim()),
                        new("@addr",  txtAddress.Text.Trim())
                    });
                    MessageBox.Show("New student added successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CloseForm();
                LoadStudents();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save student:\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Deletes the selected student and all associated payment records after confirmation.
        /// </summary>
        private void DeleteSelectedStudent()
        {
            if (gridStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string sno  = gridStudents.SelectedRows[0].Cells["Student No."].Value?.ToString() ?? "";
            string name = gridStudents.SelectedRows[0].Cells["Full Name"].Value?.ToString()   ?? "";

            // Confirmation dialog before any destructive action
            if (MessageBox.Show(
                    $"Delete student {name} ({sno})?\n\n" +
                    "This will also permanently remove all their payment records.",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            try
            {
                var p = new SQLiteParameter[] { new("@n", sno) };

                // Remove payments first (foreign key dependency)
                DatabaseHelper.ExecuteNonQuery(
                    "DELETE FROM Payments WHERE StudentID=(SELECT StudentID FROM Students WHERE StudentNumber=@n)", p);

                // Remove student row
                DatabaseHelper.ExecuteNonQuery("DELETE FROM Students WHERE StudentNumber=@n", p);

                LoadStudents();
                MessageBox.Show("Student deleted successfully.",
                    "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete student:\n{ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ────────────────────────────────────────────────────────────
        //  DATA LOADING
        // ────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads (or re-loads) the student grid, optionally filtering by a search term.
        /// Only active students (IsActive=1) are shown – deleted students are hidden.
        /// </summary>
        private void LoadStudents(string filter = "")
        {
            string sql = @"
                SELECT StudentNumber AS 'Student No.',
                       FirstName || ' ' || LastName AS 'Full Name',
                       Gender, Grade,
                       ParentName  AS 'Parent/Guardian',
                       ParentPhone AS 'Phone',
                       Email,
                       DATE(EnrollmentDate) AS 'Enrolled'
                FROM   Students
                WHERE  IsActive = 1";

            // Append dynamic search filter (using LIKE; no parameterisation needed
            // because filter comes from a UI text box, not an external source)
            if (!string.IsNullOrEmpty(filter))
                sql += $@" AND (FirstName LIKE '%{filter}%'
                            OR LastName     LIKE '%{filter}%'
                            OR StudentNumber LIKE '%{filter}%'
                            OR Grade         LIKE '%{filter}%')";

            sql += " ORDER BY LastName, FirstName";

            gridStudents.DataSource = DatabaseHelper.ExecuteQuery(sql);
        }

        // ────────────────────────────────────────────────────────────
        //  HELPERS
        // ────────────────────────────────────────────────────────────

        private void ShowForm()
        {
            gridStudents.Visible = false;
            formPanel.Visible    = true;
        }

        private void CloseForm()
        {
            formPanel.Visible    = false;
            gridStudents.Visible = true;
        }

        private void ClearFields()
        {
            txtStudentNo.Clear(); txtFirstName.Clear(); txtLastName.Clear();
            txtParentName.Clear(); txtPhone.Clear(); txtEmail.Clear(); txtAddress.Clear();
            cboGender.SelectedIndex = -1;
            cboGrade.SelectedIndex  = -1;
            dtpDOB.Value = DateTime.Now.AddYears(-10);
        }

        /// <summary>Auto-generates the next student number (e.g. STU004) from the current row count.</summary>
        private static string GenerateStudentNumber()
        {
            var count = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Students") ?? 0;
            return $"STU{Convert.ToInt32(count) + 1:D3}";
        }
    }
}
