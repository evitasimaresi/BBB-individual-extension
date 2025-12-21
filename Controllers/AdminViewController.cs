using Microsoft.AspNetCore.Mvc;
using BBB.Data;
using BBB.Models;
using Microsoft.EntityFrameworkCore;

[Route("Admin")]
public class AdminViewController : Controller
{

    public string baseURL = "~/Views/Admin/";
    private readonly AppDbContext _db;

    public AdminViewController(AppDbContext db)
    {
        _db = db;
    }

    private bool AdminCheck()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userId, out var userID))
            return false;

        var user = _db.Users
            .Where(u => u.Id == userID)
            .Select(u => new { u.Role.Name })
            .FirstOrDefault();

        if (user == null || user.Name != "admin")
            return false;

        return true;
    }

    [HttpGet("GameForm")]
    public IActionResult GameForm()
    {
        if (!AdminCheck()) return RedirectToAction("Index", "Home");

        var tagGroups = _db.TagGroups
            .Include(tg => tg.Tags)
            .OrderBy(tg => tg.Name)
            .ToList();

        return View($"{baseURL}GameForm.cshtml", tagGroups);
    }

    [HttpGet("ApproveForm")]
    public IActionResult ApproveForm()
    {
        if (!AdminCheck()) return RedirectToAction("Index", "Home");

        var requests = _db.BoardGameUsers
            .Include(r => r.User)
            .Include(r => r.BoardGame)
            .Where(r => r.ReturnDate == null)
            .Where(r => r.BoardGame.StatusId == 3)
            .OrderBy(r => r.BoardGame.Id)
            .ThenBy(r => r.BorrowDate)
            .ThenBy(r => r.Id)
            .ToList();

        return View($"{baseURL}ApproveForm.cshtml", requests);
    }

    [HttpGet("ReturnForm")]
    public IActionResult ReturnForm()
    {
        if (!AdminCheck()) return RedirectToAction("Index", "Home");

        var borrowed = _db.BoardGameUsers
            .Include(r => r.User)
            .Include(r => r.BoardGame)
            .Where(r => r.ReturnDate == null)
            .Where(r => r.BoardGame.StatusId == 2)
            .OrderBy(r => r.BorrowDate)
            .ToList();

        return View($"{baseURL}ReturnForm.cshtml", borrowed);
    }
}
