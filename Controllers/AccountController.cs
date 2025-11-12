using BBB.Data;
using BBB.Models;
using BBB.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BBB.Services;
using System.Security.Cryptography;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {   
        
        User? user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            ViewBag.Error = "Invalid username or password";
            return RedirectToAction("Login", "Account");
        }
        Auth? auth = _context.Auths.FirstOrDefault(a => a.UserId == user.Id);
        if (auth == null)
        {
            ViewBag.Error = "Invalid username or password";
            return RedirectToAction("Login", "Account");
        }

        if (PBKDF2Hasher.Verify(password, auth.PasswordHash, auth.Token ?? ""))
        {
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Error = "Invalid username or password";
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public ActionResult AddUser(string userName, string userEmail, string userPassword)
    {
        if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(userEmail) && !string.IsNullOrWhiteSpace(userPassword))
        {
            
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            string saltBase64 = Convert.ToBase64String(salt);
            
            byte[] hash = PBKDF2Hasher.Hash(userPassword, salt);
            string hashBase64 = Convert.ToBase64String(hash);

            var user = new User { Username = userName, Email = userEmail, Auth = new Auth { PasswordHash = hashBase64, Token=saltBase64 }, RoleId = 2 };
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        return RedirectToAction("Login", "Account");
    }
}