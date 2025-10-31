namespace BBB.Models
{
    public class BoardGameTag
    {
        public int BoardGameId { get; set; }
        public virtual BoardGame BoardGame { get; set; } = null!;

        public int TagId { get; set; }
        public virtual Tag Tag { get; set; } = null!;
    }
}
