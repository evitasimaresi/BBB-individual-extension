using BBB.Data;
using BBB.Models;
using BBB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;


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

        return View(tagGroups);

        // var users = _db.Users.ToList();
        // foreach (var xd in users)
        // {
        //     Debug.WriteLine(_db.Auths.FirstOrDefault(a => a.UserId == xd.Id).PasswordHash);
        // }


        // if (!int.TryParse(userId, out userID)) return View(false);

        // var user = _db.Users.FirstOrDefault(u => u.Id == userID);
        // if (user == null) return View(false);
        // if (user.Role.Name == "admin") return View(true);
        // return View(false);

    }

    public IActionResult Account()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        int userId = int.Parse(userIdStr);

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var borrowedCount = _db.BoardGameUsers
            .Count(x => x.UserId == userId && x.ReturnDate == null);

        var vm = new EditAccountModel
        {
            Username = user.Username,
            Email = user.Email,
            BorrowedCount = borrowedCount
        };

        return View(vm);
    }
    // POST: /Home/Account
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Account(EditAccountModel model)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        int userId = int.Parse(userIdStr);

        var user = _db.Users.Include(u => u.Auth).FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return RedirectToAction("Login", "Account");

        // update username + email
        user.Username = model.Username.Trim();
        user.Email = model.Email.Trim();

        // password update
        if (!string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                model.Error = "Password fields cannot be empty.";
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                model.Error = "Passwords do not match.";
            }
            else
            {
                // password hashing
                byte[] salt = RandomNumberGenerator.GetBytes(16);
                string saltBase64 = Convert.ToBase64String(salt);

                byte[] hash = PBKDF2Hasher.Hash(model.NewPassword, salt);
                string hashBase64 = Convert.ToBase64String(hash);

                user.Auth.PasswordHash = hashBase64;
                user.Auth.Token = saltBase64;
            }
        }

        if (model.Error == null)
        {
            _db.SaveChanges();
            model.Message = "Your information has been updated successfully.";
            HttpContext.Session.SetString("Username", user.Username);
        }

        return View("Account", model);
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
    // Testing the db queries

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
                g.StatusId
            })
            .ToList()
            .OrderBy(g => g.Title)
            .GroupBy(g => g.StatusId)
            .OrderBy(g => Array.IndexOf(statusOrder, g.Key))
            .SelectMany(g => g)
            .ToList();

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
