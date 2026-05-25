using System;
using System.Windows.Forms;
using SchoolFeeManagement.Database;
using SchoolFeeManagement.Forms;

namespace SchoolFeeManagement
{
    /// <summary>
    /// Program – application entry point.
    /// 
    /// Responsibilities:
    ///   1. Enable Windows visual styles (modern look on Windows 10/11).
    ///   2. Initialise the SQLite database (creates file + tables if absent).
    ///   3. Launch the LoginForm as the first visible window.
    /// </summary>
    internal static class Program
    {
        [STAThread] // Required for Windows Forms (single-threaded apartment model)
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Ensure the SQLite database file and schema exist before any form loads
                DatabaseHelper.InitializeDatabase();

                // Start with the login screen
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"A fatal error occurred during startup:\n\n{ex.Message}\n\n" +
                    "Please ensure the application folder is writable and " +
                    "the System.Data.SQLite library is present.",
                    "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
