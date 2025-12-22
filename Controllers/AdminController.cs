using Microsoft.AspNetCore.Mvc;
using BBB.Data;
using BBB.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }




    [HttpPost("games")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> CreateGame([FromForm] CreateGameDto dto, [FromForm] IFormCollection form)
    {
        if (!IsAdminCheck()) return Forbid();

        string relativePath = "";
        var tagIds = new List<int>();

        foreach (var key in form.Keys)
        {
            if (key.StartsWith("tag_"))
            {
                var value = form[key].ToString();
                if (int.TryParse(value, out int tagId))
                {
                    tagIds.Add(tagId);
                }
            }
        }

        Console.WriteLine($"Parsed tag IDs: {string.Join(", ", tagIds)}");

        if (dto.GameCover != null && dto.GameCover.Length > 0)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(dto.GameCover.FileName);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(folderPath);

            string savePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await dto.GameCover.CopyToAsync(stream);
            }

            relativePath = "/Images/" + fileName;
        }

        BoardGame game = null;

        if (!string.IsNullOrWhiteSpace(dto.GameTitle))
        {
            game = new BoardGame
            {
                Title = dto.GameTitle,
                Description = dto.GameDesc,
                Image = relativePath,
                Condition = dto.GameLink,
                Link = dto.GameCond,
                StatusId = 1
            };
            _db.BoardGames.Add(game);
            _db.SaveChanges();

            if (tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _db.Attach(new Tag { Id = tagId });
                    _db.BoardGameTags.Add(new BoardGameTag
                    {
                        BoardGameId = game.Id,
                        TagId = tagId
                    });
                }
                _db.SaveChanges();
            }

            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, new { success = true, message = "Game added successfully", id = game.Id });
        }

        return BadRequest("Game title is required");
    }

    [HttpPatch("borrow-requests")]
    public IActionResult UpdateBorrowRequests([FromBody] List<BoardGameUserDecisionDto> decisions)
    {
        if (!IsAdminCheck()) return Forbid();

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
        var requestMap = new Dictionary<int, List<int>>();
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
            else if (countApprove == 0)
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

        // All validations passed, update db
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

    [HttpPatch("returns")]
    public IActionResult ProcessReturns([FromBody] List<ReturnDto> results)
    {
        if (!IsAdminCheck()) return Forbid();

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

    // Get a single game
    [HttpGet("games/{id}")]
    public IActionResult GetGame(int id)
    {
        if (!IsAdminCheck()) return Forbid();

        BoardGame? oneGame = _db.BoardGames.FirstOrDefault(g => g.Id == id);

        if (oneGame == null)
            return NotFound();

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

    [HttpPut("games/{id}")]
    [Consumes("multipart/form-data")]
    public IActionResult UpdateGame(int id, [FromForm] CreateGameDto dto)
    {
        if (!IsAdminCheck()) return Forbid();
        string relativePath = "";

        if (dto.GameCover != null && dto.GameCover.Length > 0)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(dto.GameCover.FileName);

            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            Directory.CreateDirectory(folderPath);

            string savePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                dto.GameCover.CopyTo(stream);
            }

            relativePath = "/Images/" + fileName;
        }

        var game = _db.BoardGames.FirstOrDefault(x => x.Id == id);
        if (game == null)
            return NotFound();

        game.Title = dto.GameTitle;
        game.Description = dto.GameDesc;
        if (relativePath != "") game.Image = relativePath;

        _db.SaveChanges();

        return Ok(new { success = true, message = "Game updated successfully" });
    }

    [HttpDelete("games/{id}")]
    public IActionResult DeleteGame(int id)
    {
        if (!IsAdminCheck()) return Forbid();

        var oneGame = _db.BoardGames.FirstOrDefault(x => x.Id == id);
        if (oneGame == null)
            return NotFound();

        if (oneGame.StatusId != 1 && oneGame.StatusId != 4)
            return Conflict("Cannot delete a game with this status.");

        _db.BoardGames.Remove(oneGame);
        _db.SaveChanges();

        return NoContent();
    }

    private bool IsAdminCheck()
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
}