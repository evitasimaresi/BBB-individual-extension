namespace BBB.Models
{
    public class TagGroup
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
