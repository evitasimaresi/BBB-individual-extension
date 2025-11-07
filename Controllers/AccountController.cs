using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        var user = _context.Users.FirstOrDefault(u => u.Username == username && _context.Auths.FirstOrDefault(a => a.UserId == u.Id).PasswordHash  == password);

        if (user != null)
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
            var user = new User { Username = userName, Email = userEmail, Auth = new Auth { PasswordHash = userPassword }, RoleId = 2 };
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        return RedirectToAction("Login", "Account");
    }
}