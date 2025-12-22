using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;

namespace BBB.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : Controller
{
    private readonly AppDbContext _db;

    public GamesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetGames([FromQuery] string? gameId = null)
    {
        var statusOrder = new[] { 1, 3, 2, 4 };

        var games = _db.BoardGames
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.Image,
                Tags = g.BoardGameTags.Select(bt => new
                {
                    Id = bt.Tag.Id,
                    Name = bt.Tag.Name,
                    TagGroupId = bt.Tag.TagGroupId,
                    TagGroupName = bt.Tag.TagGroup.Name,
                }),
                g.StatusId,
                StatusName = g.Status.Name
            })
            .ToList();

        if (!string.IsNullOrWhiteSpace(gameId))
        {
            var searchIds = SearchGameIds(gameId);
            games = games.Where(g => searchIds.Contains(g.Id)).ToList();
        }

        var result = games
            .OrderBy(g => g.Title)
            .GroupBy(g => g.StatusId)
            .OrderBy(g => Array.IndexOf(statusOrder, g.Key))
            .SelectMany(g => g)
            .ToList();

        return Json(result);
    }

    [HttpPost("borrow-requests")]
    public IActionResult CreateBorrowRequest([FromBody] BorrowGameRequest request)
    {
        var userId = HttpContext.Session.GetString("UserId");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }
        BoardGame? game = _db.BoardGames.FirstOrDefault(g => g.Id == request.GameId);

        if (game == null)
            return NotFound();

        if (game.StatusId != 1 && game.StatusId != 3)
            return Conflict(new { message = "Game not available for borrowing" });

        int userID;
        if (!int.TryParse(userId, out userID))
            return StatusCode(418, "I'm a teapot");

        if (_db.BoardGameUsers.FirstOrDefault(bgu =>
            bgu.UserId == userID &&
            bgu.BoardGameId == request.GameId &&
            (bgu.ReturnDate == null || DateTime.Now < bgu.ReturnDate)) != null)
            return StatusCode(420, "Game already borrowed");
        if (_db.BoardGameUsers.Count(bgu =>
            bgu.UserId == userID &&
            (bgu.ReturnDate == null || DateTime.Now < bgu.ReturnDate)) > 3)
            return StatusCode(419, "Too many games");

        game.StatusId = 3;

        _db.BoardGameUsers.Add(
            new()
            {
                BoardGameId = request.GameId,
                UserId = userID,
                BorrowDate = DateTime.Now
            }
        );

        _db.SaveChanges();

        return Ok(new { message = $"Game borrowed by {username}" });
    }

    private List<int> SearchGameIds(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<int>();

        var qTokens = System.Text.RegularExpressions.Regex
            .Split(query.Trim().ToLowerInvariant(), @"\W+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        var games = _db.BoardGames
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description
            })
            .ToList();

        var results = games
            .Where(g =>
            {
                var title = g.Title ?? string.Empty;
                var desc = g.Description ?? string.Empty;

                if (qTokens.Any(q => title.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                  || desc.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }

                var tokenScores = qTokens.Select(q =>
                {
                    int titleScore = Similarity(q, title);
                    int descScore = string.IsNullOrEmpty(desc) ? 0 : Similarity(q, desc);
                    return Math.Max(titleScore, descScore);
                }).ToArray();

                var avgScore = tokenScores.Length == 0 ? 0 : tokenScores.Average();

                return avgScore >= 60.0;
            })
            .Select(g => g.Id)
            .ToList();

        return results;
    }

    private int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int[,] d = new int[s.Length + 1, t.Length + 1];

        for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= t.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s.Length; i++)
        {
            for (int j = 1; j <= t.Length; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost
                );
            }
        }

        return d[s.Length, t.Length];
    }

    private int Similarity(string s, string t)
    {
        int distance = LevenshteinDistance(s.ToLower(), t.ToLower());
        int maxLen = Math.Max(s.Length, t.Length);
        return maxLen == 0 ? 100 : (int)((1.0 - (double)distance / maxLen) * 100);
    }

    public class BorrowGameRequest
    {
        public int GameId { get; set; }
    }
}
