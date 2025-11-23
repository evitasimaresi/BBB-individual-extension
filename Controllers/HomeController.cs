using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BBB.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly AppDbContext _db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        int userID;

        bool isAdmin = false;
        if (int.TryParse(userId, out userID))
        {
            var user = _db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userID);
            isAdmin = user?.Role?.Name == "admin";
        }
        ViewBag.IsAdmin = isAdmin;

        var tagGroups = _db.TagGroups
            .Include(tg => tg.Tags)
            .OrderBy(tg => tg.Name)
            .ToList();

        var statuses = _db.Statuses
            .OrderBy(s => s.Id)
            .ToList();

        ViewBag.Statuses = statuses;

        return View(tagGroups);
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = _db.Statuses
            .Select(s => new
            {
                s.Id,
                s.Name
            })
            .ToList();

        return Json(statuses);
    }
    
    //came with the default project:
    public IActionResult Privacy()
    {
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult GetGames()
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
            .ToList()
            .OrderBy(g => g.Title)
            .GroupBy(g => g.StatusId)
            .OrderBy(g => Array.IndexOf(statusOrder, g.Key))
            .SelectMany(g => g)
            .ToList();

        return Json(games);
    }

    // fuzzy search functions
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

    // convert distance to similarity % 0-100
    private int Similarity(string s, string t)
    {
        int distance = LevenshteinDistance(s.ToLower(), t.ToLower());
        int maxLen = Math.Max(s.Length, t.Length);
        return maxLen == 0 ? 100 : (int)((1.0 - (double)distance / maxLen) * 100);
    }

    [HttpGet]
    public IActionResult SearchGames([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is null or white space");

        // Tokenize query into words
        var qTokens = System.Text.RegularExpressions.Regex
            .Split(query.Trim().ToLowerInvariant(), @"\W+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToArray();

        // Get trimmed games data
        var games = _db.BoardGames
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description
            })
            .ToList();

        // If any token is a substring of title or description, treat as a match.
        // Otherwise compute average per token fuzzy score against title/description.
        var results = games
            .Where(g =>
            {
                var title = g.Title ?? string.Empty;
                var desc = g.Description ?? string.Empty;

                // fast substring hit for any token
                if (qTokens.Any(q => title.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                                  || desc.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return true;
                }

                // compute average best match per token
                var tokenScores = qTokens.Select(q =>
                {
                    // compare token against full title/description
                    int titleScore = Similarity(q, title);
                    int descScore = string.IsNullOrEmpty(desc) ? 0 : Similarity(q, desc);
                    return Math.Max(titleScore, descScore);
                }).ToArray();

                var avgScore = tokenScores.Length == 0 ? 0 : tokenScores.Average();

                // threshold
                return avgScore >= 60.0;
            })
            .Select(g => g.Id)
            .ToList();

        return Json(results);
    }

    [HttpPost]
    public IActionResult BorrowGame([FromBody] int request)
    {
        // Get user info from session
        var userId = HttpContext.Session.GetString("UserId");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(); // User not logged in
        }
        BoardGame? game = _db.BoardGames.FirstOrDefault(g => g.Id == request);

        // GAME NOT FOUND        
        if (game == null)
            return StatusCode(418, "I'm a teapot");

        // GAME NOT AVAILABLE
        if (game.StatusId != 1 && game.StatusId != 3)
            return Conflict();

        int userID;
        // USER NOT FOUND
        if (!int.TryParse(userId, out userID))
            return StatusCode(418, "I'm a teapot");

        if (_db.BoardGameUsers.FirstOrDefault(bgu =>
            bgu.UserId == userID &&
            bgu.BoardGameId == request &&
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
                BoardGameId = request,
                UserId = userID,
                BorrowDate = DateTime.Now
            }
        );

        _db.SaveChanges();

        return Ok(new { message = $"Game borrowed by {username}" });
    }
}