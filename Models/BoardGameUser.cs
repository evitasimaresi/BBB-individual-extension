namespace BBB.Models
{
    public class BoardGameUser
    {
        public int BoardGameId { get; set; }
        public virtual BoardGame BoardGame { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
