namespace BBB.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TagGroupId { get; set; }
        public virtual TagGroup TagGroup { get; set; } = null!;

        public virtual ICollection<BoardGameTag> BoardGameTags { get; set; } = new List<BoardGameTag>();
    }
}
