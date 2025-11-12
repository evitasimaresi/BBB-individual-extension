using BBB.Data;
using BBB.Models;
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

        var users = _db.Users.ToList();
        foreach (var xd in users)
        {
            // var auth = _db.Auths.FirstOrDefault(a => a.UserId == xd.Id);
            // if (auth == null) continue;

            // // Generate random 16-byte salt
            // byte[] salt = RandomNumberGenerator.GetBytes(16);
            // string saltBase64 = Convert.ToBase64String(salt);

            // // Hash the existing plain password using static PBKDF2Hasher
            // byte[] hash = Services.PBKDF2Hasher.Hash(auth.PasswordHash, salt);
            // string hashBase64 = Convert.ToBase64String(hash);

            // // Store back in DB
            // auth.Token = saltBase64;           // salt
            // auth.PasswordHash = hashBase64;    // hashed password
            // _db.Entry(auth).State = EntityState.Modified;

            Debug.WriteLine(_db.Auths.FirstOrDefault(a => a.UserId == xd.Id).PasswordHash);
        }

        // // Save changes
        // _db.SaveChanges();





        if (!int.TryParse(userId, out userID)) return View(false);

        var user = _db.Users.FirstOrDefault(u => u.Id == userID);
        if (user == null) return View(false);
        if (user.Role.Name == "admin") return View(true);
        return View(false);

    }

    public IActionResult Account()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out var userId))
        {
            Console.WriteLine("No user session found. Redirecting to Login.");
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            Console.WriteLine($"User not found for ID: {userId}");
            return RedirectToAction("Login", "Account");
        }

        var borrowedCount = _db.BoardGameUsers
            .Count(x => x.UserId == userId && x.ReturnDate == null);

        var vm = new EditAccountModel
        {
            Username = user.Username,
            Email = user.Email,
            BorrowedCount = borrowedCount
        };

        Console.WriteLine($"Loaded account info for user: {user.Username}");
        return View(vm);
    }
    // POST: /Home/Account
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Account(EditAccountModel model)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out var userId))
        {
            Console.WriteLine("Invalid user session. Redirecting to Login.");
            return RedirectToAction("Login", "Account");
        }

        var user = _db.Users.Include(u => u.Auth).FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            Console.WriteLine($"User with ID {userId} not found.");
            return RedirectToAction("Login", "Account");
        }

        // Update username & email
        user.Username = model.Username?.Trim() ?? user.Username;
        user.Email = model.Email?.Trim() ?? user.Email;

        Console.WriteLine($"Attempting to update user: {user.Username}");

        // Password change (if filled)
        if (!string.IsNullOrWhiteSpace(model.NewPassword) || !string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) || string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                model.Error = "Password fields cannot be empty.";
                Console.WriteLine("Password fields were empty.");
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                model.Error = "Passwords do not match.";
                Console.WriteLine("Password mismatch detected.");
            }
            else
            {
                var auth = _db.Auths.FirstOrDefault(a => a.UserId == user.Id);
                if (auth != null)
                {
                    auth.PasswordHash = model.NewPassword;
                    Console.WriteLine("Password successfully updated.");
                }
            }
        }

        if (model.Error == null)
        {
            _db.SaveChanges();
            Console.WriteLine($"User info updated successfully for {user.Username}.");

            HttpContext.Session.SetString("Username", user.Username);
            model.Message = "Your information has been updated successfully.";
        }

        model.BorrowedCount = _db.BoardGameUsers.Count(x => x.UserId == userId && x.ReturnDate == null);

        return View(model);
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
        var games = _db.BoardGames
            .Where(g => g.StatusId == 1 || g.StatusId == 3)
            .Select(g => new
            {
                g.Id,
                g.Title,
                g.Description,
                g.Image
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

        // TOO MANY BOARD GAMES REQUESTED/BORROWED
        if (_db.BoardGameUsers.Count(bgu =>
                bgu.UserId == userID &&
                (bgu.ReturnDate == null || DateTime.Now < bgu.ReturnDate)
            ) > 3)
            return Unauthorized();

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
