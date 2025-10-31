namespace BBB.Models
{
    public class Status
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public virtual List<BoardGame> BoardGames { get; set; } = new List<BoardGame>();
    }
}
