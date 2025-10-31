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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
