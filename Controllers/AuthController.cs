using BBB.Data;
using BBB.Models;
using BBB.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class AuthController : Controller
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext context)
    {
        _db = context;
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

    [HttpPost("logout")]
    public IActionResult logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult GetMe()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            return StatusCode(418, "I'm a teapot");
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return StatusCode(418, "I'm a teapot");
        }

        string? roleName = user.Role?.Name;

        return Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            roleId = user.RoleId,
            roleName = roleName
        });
    }

    [HttpPost("register")]
    public ActionResult Register([FromBody] RegisterRequest request)
    {
        var pattern = @"^[A-Za-z0-9._%+-]+@student\.sdu\.dk$";
        bool isMatch = Regex.IsMatch(request.UserEmail, pattern, RegexOptions.IgnoreCase);

        if (!string.IsNullOrWhiteSpace(request.UserName) && !string.IsNullOrWhiteSpace(request.UserEmail) && !string.IsNullOrWhiteSpace(request.UserPassword) && isMatch)
        {

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string saltBase64 = Convert.ToBase64String(salt);

            byte[] hash = PBKDF2Hasher.Hash(request.UserPassword, salt);
            string hashBase64 = Convert.ToBase64String(hash);

            var user = new User { Username = request.UserName, Email = request.UserEmail, Auth = new Auth { PasswordHash = hashBase64, Token = saltBase64 }, RoleId = 2 };
            _db.Users.Add(user);
            _db.SaveChanges();
            return Ok();
        }

        return BadRequest("Invalid input");
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string UserName { get; set; }
        public required string UserEmail { get; set; }
        public required string UserPassword { get; set; }
    }
}
