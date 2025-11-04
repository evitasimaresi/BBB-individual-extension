using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

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
        return View();
    }

    public IActionResult Account()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult ToBeAdminPanel()
    {
        return View();
    }

    //came with the default project:
    public IActionResult Privacy()
    {
        return View();
    }

    // Testing the db queries
    public async Task<IActionResult> Test()
    {
        var foo = await _db.BoardGameUsers.Where(x => x.UserId == 4).ToListAsync();
        var lines = foo.Select(x => $"{x.BoardGame.Title} - Borrowed on: {x.BorrowDate:yyyy-MM-dd} - Returned on: {x.ReturnDate:yyyy-MM-dd}");
        return Content(string.Join("\n", lines), "text/plain");

        /*
        var usernames = await _db.BoardGames.Where(x => x.Title == "Catan").FirstAsync();
        var usr = usernames.BoardGameTags.Select(x => x.Tag.Name);
        return Content(string.Join("\n", usr), "text/plain");
        */
    }
    public IActionResult Register()
    {
        return View();
    }
    // Testing the db queries
    public async Task<IActionResult> Test()
    {
        var foo = await _db.BoardGameUsers.Where(x => x.UserId == 4).ToListAsync();
        var lines = foo.Select(x => $"{x.BoardGame.Title} - Borrowed on: {x.BorrowDate:yyyy-MM-dd} - Returned on: {x.ReturnDate:yyyy-MM-dd}");
        return Content(string.Join("\n", lines), "text/plain");

        /*
        var usernames = await _db.BoardGames.Where(x => x.Title == "Catan").FirstAsync();
        var usr = usernames.BoardGameTags.Select(x => x.Tag.Name);
        return Content(string.Join("\n", usr), "text/plain");
        */
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }



    [HttpGet]
    public IActionResult GetGames()
    {
        var games = _db.BoardGames.Select(g => new
        {
            g.Id,
            g.Title,
            g.Description,
            g.Link
        }).ToList();

        return Json(games);
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

        if (game == null) return StatusCode(418, "I'm a teapot");
        
        if (game.StatusId != 1 && game.StatusId != 3) return Conflict();
        
        int userID;
        if (!int.TryParse(userId, out userID)) return StatusCode(418, "I'm a teapot");


        if (_db.BoardGameUsers.Count(bgu => bgu.UserId == userID && (bgu.ReturnDate == null || DateTime.Now < bgu.ReturnDate)) > 3) return Unauthorized();

        game.StatusId = 3;

        _db.BoardGameUsers.Add(
            new()
            {
                BoardGameId = request,
                UserId = userID,
                BorrowDate = DateTime.Now
            }
        );



        return Ok(new { message = $"Game borrowed by {username}" });
    }


}
