namespace BBB.Models
{
    public class EditAccountModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BorrowedCount { get; set; }

        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }

        public string? Message { get; set; }
        public string? Error { get; set; }
    }
}
