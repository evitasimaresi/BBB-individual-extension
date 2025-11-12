namespace BBB.Models
{
    // It does not exist in the database — it’s just for transferring data between Controller and View.
    public class EditAccountModel
    {
        // User info bruh
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BorrowedCount { get; set; }

        // Password parts bruh
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

        // Message feedback bruh
        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
