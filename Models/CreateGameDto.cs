namespace BBB.Models;

public class CreateGameDto
{
    public string? GameTitle { get; set; }
    public string? GameDesc { get; set; }
    public IFormFile? GameCover { get; set; }
    public string? GameCond { get; set; }
    public string? GameLink { get; set; }
}
