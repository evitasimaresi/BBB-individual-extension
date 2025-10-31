namespace BBB.Models
{
    public class BoardGame
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Condition { get; set; }
        public string? Link { get; set; }
        public DateTime LastChange { get; set; }
        
        public int StatusId { get; set; }
        public virtual Status Status { get; set; } = null!;

        public virtual ICollection<BoardGameUser> BoardGameUsers { get; set; } = new List<BoardGameUser>();
        public virtual ICollection<BoardGameTag> BoardGameTags { get; set; } = new List<BoardGameTag>();
    }
}
