using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        return View();
    }

    // for editing games:

    // GET game
    [HttpGet]
    public IActionResult GetOneGame(int gameId)
    {
        BoardGame? oneGame = _db.BoardGames.FirstOrDefault(g => g.Id == gameId);
        
        if (oneGame == null)
            return Json(null);

        BoardGame result = new BoardGame
        {
            Id = oneGame.Id,
            Title = oneGame.Title,
            Description = oneGame.Description,
            Image = oneGame.Image,
            Condition = oneGame.Condition,
            Link = oneGame.Link
        };

        return Json(result);
    }

    // POST updated game
    [HttpPost]
    public IActionResult EditGame(BoardGame oneGame)
    {
        if (ModelState.IsValid)
        {
            _db.Entry(oneGame).State = EntityState.Modified;
            _db.SaveChanges();
            return Ok();
        }
        return View(oneGame);
    }

    // POST delete game
    [HttpPost]
    public IActionResult DeleteGame(int Id)
    {
        var oneGame = _db.BoardGames.Find(Id);
        if (oneGame != null)
        {
            _db.BoardGames.Remove(oneGame);
            _db.SaveChanges();
        }

        // should probably return an http message
        return Ok();
    }
}