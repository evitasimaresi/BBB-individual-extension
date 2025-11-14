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

    /* Approve Form */ 
    public IActionResult ApproveForm()
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
    public IActionResult SaveApproveForm([FromBody] List<BoardGameUserDecisionDto> decisions)
    {
        if (decisions == null || !decisions.Any())
            return BadRequest("No decisions received.");

        // Check for duplicate BoardGameUserIds in the incoming decisions
        var duplicateIds = decisions.GroupBy(d => d.BoardGameUserId)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();
        if (duplicateIds.Any())
            return BadRequest($"Duplicate BoardGameUserIds found: {string.Join(", ", duplicateIds)}");

        // Fetch all unresolved borrow requests
        var requests = _db.BoardGameUsers
            .Include(r => r.User)
            .Include(r => r.BoardGame)
            .Where(r => r.ReturnDate == null)
            .Where(r => r.BoardGame.StatusId == 3)
            .OrderBy(r => r.BoardGame.Id)
            .ThenBy(r => r.BorrowDate)
            .ThenBy(r => r.Id)
            .ToList();
        
        // Check count
        if (requests.Count != decisions.Count)
            return BadRequest($"Mismatch in number of requests and decisions: Db requests: {requests.Count} Decisions: {decisions.Count}");
        
        // Validate linked entities
        var requestMap = new Dictionary<int, List<int>>(); // BoardGameId -> Descision
        foreach (var r in requests)
        {
            // Find all decisions for this BoardGameUser
            var decisionResults = decisions
                .Where(d => d.BoardGameUserId == r.Id)
                .Select(d => d.Result)
                .ToList();

            if (!requestMap.ContainsKey(r.BoardGame.Id))
                requestMap[r.BoardGame.Id] = new List<int>();

            requestMap[r.BoardGame.Id].AddRange(decisionResults);
        }

        foreach (var boardGameId in requestMap.Keys)
        {
            var values = requestMap[boardGameId];

            var countApprove = values.Count(v => v == 1);
            var countDeny = values.Count(v => v == 2);
            var countNone = values.Count(v => v == 0);

            bool isValid = false;

            if (countNone == values.Count)
                isValid = true;
            else if (countApprove == 1 && countDeny == values.Count - 1 && countNone == 0)
                isValid = true;

            if (!isValid)
            {
                var errorResponse = new
                {
                    BoardGameId = boardGameId,
                    Decisions = values,
                    Message = "Invalid decision data: either multiple approvals or a mix of approve/deny/unmarked detected."
                };

                return BadRequest(errorResponse);
            }
        }

        // All validations passed, update database
        foreach (var decision in decisions)
        {
            var bgu = _db.BoardGameUsers
                .Include(x => x.BoardGame)
                .FirstOrDefault(x => x.Id == decision.BoardGameUserId);

            if (bgu == null)
                return BadRequest($"BoardGameUser with ID {decision.BoardGameUserId} not found.");

            if (decision.Result == 2)
            {
                _db.BoardGameUsers.Remove(bgu);
            }
            else if (decision.Result == 1)
            {
                // Mark the related board game as borrowed
                bgu.BoardGame.StatusId = 2;
            }
        }

        _db.SaveChanges();
        return Ok();
    }

    /* Return Form */
    public IActionResult ReturnForm()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (!int.TryParse(userId, out var uid))
            return RedirectToAction("Index", "Home");

        var user = _db.Users
            .Where(u => u.Id == uid)
            .Select(u => u.Role.Name)
            .FirstOrDefault();

        if (user != "admin")
            return RedirectToAction("Index", "Home");

        var borrowed = _db.BoardGameUsers
            .Include(r => r.User)
            .Include(r => r.BoardGame)
            .Where(r => r.ReturnDate == null)
            .Where(r => r.BoardGame.StatusId == 2)
            .OrderBy(r => r.BorrowDate)
            .ToList();

        return View(borrowed);
    }

    [HttpPost]
    public IActionResult SaveReturnForm([FromBody] List<ReturnDto> results)
    {
        if (results == null || !results.Any())
            return BadRequest("No data received.");

        foreach (var r in results)
        {
            if (!r.Returned)
                continue;

            var bgu = _db.BoardGameUsers
                .Include(x => x.BoardGame)
                .FirstOrDefault(x => x.Id == r.BoardGameUserId);

            if (bgu == null)
                return BadRequest($"Borrow record {r.BoardGameUserId} not found.");

            bgu.ReturnDate = DateTime.Now;
            bgu.BoardGame.StatusId = 1;
        }

        _db.SaveChanges();
        return Ok();
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