using BBB.Data;
using BBB.Models;
using BBB.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly AppDbContext _db;

    public AccountController(AppDbContext context)
    {
        _db = context;
    }

    [HttpGet("index")]
    public IActionResult Index()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        int userId;

        if (!int.TryParse(userIdStr, out userId))
            return StatusCode(418, "I'm a teapot");

        var vm = GetEditAccountModel(userId);
        if (vm == null)
        {
            return Unauthorized();
        }

        return Ok(vm);
    }

    // POST: /Home/Account
    [HttpPost("account")]
    [ValidateAntiForgeryToken]
    public IActionResult Account(EditAccountModel model)
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        Debug.WriteLine($"{model.BorrowedCount} XD");
        int userId;

        if (!int.TryParse(userIdStr, out userId))
            return StatusCode(418, "I'm a teapot");

        var user = _db.Users.Include(u => u.Auth).FirstOrDefault(u => u.Id == userId);

        if (user == null)
            return Unauthorized();

        // update username + email
        user.Username = model.Username.Trim();
        string? error = null;
        string? message = null;
        // handle password change
        if (!string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            if (string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                error = "Password fields cannot be empty.";
            }
            else if (model.NewPassword != model.ConfirmPassword)
            {
                error = "Passwords do not match.";
            }
            else if (model.NewPassword.Length < 8 || !model.NewPassword.Any(char.IsDigit))
            {
                error = "Password must be at least 8 characters long and contain at least one number.";
            }
            else
            {
                // update hashed password + salt
                byte[] salt = RandomNumberGenerator.GetBytes(16);
                string saltBase64 = Convert.ToBase64String(salt);

                byte[] hash = PBKDF2Hasher.Hash(model.NewPassword, salt);
                string hashBase64 = Convert.ToBase64String(hash);

                user.Auth.PasswordHash = hashBase64;
                user.Auth.Token = saltBase64;
            }
        }

        if (error == null)
        {
            _db.SaveChanges();
            message = "Your information has been updated successfully.";
            HttpContext.Session.SetString("Username", user.Username);
        }
        var vm = GetEditAccountModel(userId);
        if (vm == null) return BadRequest();
        vm.Error = error;
        vm.Message = message;
        return Ok(vm);
    }

    [HttpGet("games")]
    public IActionResult GetGames()
    {
        var statusOrder = new[] { 1, 3, 2, 4 };

        var userId = HttpContext.Session.GetString("UserId");
        var username = HttpContext.Session.GetString("Username");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(); // User not logged in
        }
        int userID;

        if (!int.TryParse(userId, out userID))
            return StatusCode(418, "I'm a teapot");


        var games = _db.BoardGames
            .Where(
                g => _db.BoardGameUsers.FirstOrDefault(
                    bgu => bgu.UserId == userID &&
                    (bgu.ReturnDate == null || bgu.ReturnDate > DateTime.Now) &&
                    g.Id == bgu.BoardGameId
                ) != null
            )
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

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        string username = request.Username;
        string password = request.Password;

        User? user = _db.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return StatusCode(418, "I'm a teapot");
        }
        Auth? auth = _db.Auths.FirstOrDefault(a => a.UserId == user.Id);
        if (auth == null)
        {
            return StatusCode(418, "I'm a teapot");
        }

        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            if (PBKDF2Hasher.Verify(password, auth.PasswordHash, auth.Token ?? ""))
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                return Ok(new { success = true, redirectUrl = "/" });
            }
        }

        return StatusCode(418, "I'm a teapot");
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    [HttpGet("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    // [HttpGet("register")]
    // public IActionResult Register()
    // {
    //     return Ok();
    // }

    [HttpPost("register")]
    public ActionResult AddUser(string userName, string userEmail, string userPassword)
    {
        var pattern = @"^[A-Za-z0-9._%+-]+@student\.sdu\.dk$";
        bool isMatch = Regex.IsMatch(userEmail, pattern, RegexOptions.IgnoreCase);

        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(userEmail) && !string.IsNullOrWhiteSpace(userPassword) && isMatch)
        {

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string saltBase64 = Convert.ToBase64String(salt);

            byte[] hash = PBKDF2Hasher.Hash(userPassword, salt);
            string hashBase64 = Convert.ToBase64String(hash);

            var user = new User { Username = userName, Email = userEmail, Auth = new Auth { PasswordHash = hashBase64, Token = saltBase64 }, RoleId = 2 };
            _db.Users.Add(user);
            _db.SaveChanges();
            return Ok();
        }

        return BadRequest("Invalid input");
        // return RedirectToAction("Login", "Account");
    }

    public class Credentials
    {
        public required string userName { get; set; }
        public required string userEmail { get; set; }
    }

    [HttpPost("check-availability")]
    public JsonResult CheckUserAvailability([FromBody] Credentials creds)
    {
        bool usernameTaken = _db.Users.Any(u => u.Username == creds.userName);
        bool emailTaken = _db.Users.Any(u => u.Email == creds.userEmail);

        return Json(new
        {
            usernameTaken,
            emailTaken
        });
    }

    private EditAccountModel? GetEditAccountModel(int userId)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return null;
        var borrowedCount = _db.BoardGameUsers
            .Count(x => x.UserId == userId && x.ReturnDate == null);

        var vm = new EditAccountModel
        {
            Username = user.Username,
            Email = user.Email,
            BorrowedCount = borrowedCount
        };
        return vm;
    }
}