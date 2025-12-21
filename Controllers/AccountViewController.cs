using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("Account")]
public class AccountViewController : Controller
{
    private readonly AppDbContext _db;

    public AccountViewController(AppDbContext context)
    {
        _db = context;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet()]
    public IActionResult Account()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userIdStr, out int userId))
            return RedirectToAction("Login", "Home");

        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return Unauthorized();

        var borrowedCount = _db.BoardGameUsers.Count(x => x.UserId == userId && x.ReturnDate == null);

        var vm = new EditAccountModel
        {
            Username = user.Username,
            Email = user.Email,
            BorrowedCount = borrowedCount
        };

        return View("~/Views/Account/Account.cshtml", vm);
    }
}