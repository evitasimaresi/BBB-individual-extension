namespace BBB.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public virtual Auth Auth { get; set; } = null!;

        public virtual ICollection<BoardGameUser> BoardGameUsers { get; set; } = new List<BoardGameUser>();
    }
}
