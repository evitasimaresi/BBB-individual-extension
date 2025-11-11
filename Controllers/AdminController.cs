using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using BBB.Data;
using BBB.Models;
using Microsoft.EntityFrameworkCore;

public class AdminController : Controller
{
    
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }
    public IActionResult GameForm()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userId, out var userID))
            return RedirectToAction("Index", "Home");

        var user = _db.Users
            .Where(u => u.Id == userID)
            .Select(u => new { u.Role.Name })
            .FirstOrDefault();

        if (user == null || user.Name != "admin")
            return RedirectToAction("Index", "Home");

        return View();
    }
    
    [HttpPost]
    public async Task<ActionResult> AddGame(string gameTitle, string gameDesc, IFormFile gameCover)
    {
        string relativePath = "";

        if (gameCover != null && gameCover.Length > 0)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(gameCover.FileName);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(folderPath);

            string savePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await gameCover.CopyToAsync(stream);
            }

            relativePath = "/Images/" + fileName;
        }
        
        if (!string.IsNullOrWhiteSpace(gameTitle))
        {
            var game = new BoardGame { Title = gameTitle, Description = gameDesc, Image = relativePath, StatusId = 1 };
            _db.BoardGames.Add(game);
            _db.SaveChanges();
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ApproveReturn()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userId, out var userID))
            return RedirectToAction("Index", "Home");

        var user = _db.Users
            .Where(u => u.Id == userID)
            .Select(u => new { u.Role.Name })
            .FirstOrDefault();

        if (user == null || user.Name != "admin")
            return RedirectToAction("Index", "Home");

        var requests = _db.BoardGameUsers
            .Include(r => r.User)
            .Include(r => r.BoardGame)
            .Where(r => r.ReturnDate == null)
            .Where(r => r.BoardGame.StatusId == 3)      // All the unresolved borrow requests
            .OrderBy(r => r.BoardGame.Id)               // Group by BoardGame.Id (lowest first)
            .ThenBy(r => r.BorrowDate)                  // Sort by earliest BorrowDate
            .ThenBy(r => r.Id)                          // Tiebreaker: lower BoardGameUser.Id first
            .ToList();

        return View(requests);
    }

    [HttpPost]
    public IActionResult SaveApproveReturn([FromBody] List<BoardGameUserDecisionDto> decisions)
    {
        var bruh = "bruh";
        bruh = "67";
        foreach (var decision in decisions)
        {
            // db update
        }

        // _db.SaveChanges();

        return Ok();
    }
}
