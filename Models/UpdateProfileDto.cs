namespace BBB.Models
{
    public class UpdateProfileDto
    {
        public required string Username { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
