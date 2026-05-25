namespace SchoolFeeManagement.Helpers
{
    /// <summary>
    /// SessionHelper – static class that holds the currently logged-in user's details.
    /// 
    /// Because Windows Forms is single-threaded and this is a desktop app (not a web app),
    /// a simple static class is a safe, straightforward way to share session state across forms
    /// without passing objects through constructors.
    /// 
    /// Call Clear() on logout to wipe all session data.
    /// </summary>
    public static class SessionHelper
    {
        /// <summary>Primary key of the logged-in user from the Users table.</summary>
        public static int CurrentUserID { get; set; }

        /// <summary>Login username (e.g. "admin").</summary>
        public static string CurrentUsername { get; set; } = string.Empty;

        /// <summary>Display name shown in the top bar and on receipts.</summary>
        public static string CurrentFullName { get; set; } = string.Empty;

        /// <summary>Role string – "Admin" or future roles (e.g. "Cashier").</summary>
        public static string CurrentRole { get; set; } = string.Empty;

        /// <summary>Returns true only when a user has successfully logged in.</summary>
        public static bool IsLoggedIn => CurrentUserID > 0;

        /// <summary>Resets all session values; called on logout.</summary>
        public static void Clear()
        {
            CurrentUserID = 0;
            CurrentUsername = string.Empty;
            CurrentFullName = string.Empty;
            CurrentRole = string.Empty;
        }

        /// <summary>
        /// Generates a unique receipt number using the current timestamp.
        /// Format: RCP + yyyyMMddHHmmss  (e.g. RCP20240915143022)
        /// This guarantees uniqueness as long as no two payments are processed
        /// in the same second, which is acceptable for a school admin desktop app.
        /// </summary>
        public static string GenerateReceiptNumber() =>
            "RCP" + DateTime.Now.ToString("yyyyMMddHHmmss");

        /// <summary>Returns the current academic year string, e.g. "2025/2026".</summary>
        public static string GetCurrentAcademicYear()
        {
            int year = DateTime.Now.Year;
            return $"{year}/{year + 1}";
        }
    }
}
