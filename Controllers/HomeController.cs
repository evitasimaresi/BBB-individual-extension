using BBB.Data;
using BBB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BBB.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        int userID;

        bool isAdmin = false;
        if (int.TryParse(userId, out userID))
        {
            var user = _db.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == userID);
            isAdmin = user?.Role?.Name == "admin";
        }
        ViewBag.IsAdmin = isAdmin;

        var tagGroups = _db.TagGroups
            .Include(tg => tg.Tags)
            .OrderBy(tg => tg.Name)
            .ToList();

        var statuses = _db.Statuses
            .OrderBy(s => s.Id)
            .ToList();

        ViewBag.Statuses = statuses;

        return View(tagGroups);
    }

    [HttpGet]
    public IActionResult GetStatuses()
    {
        var statuses = _db.Statuses
            .Select(s => new
            {
                s.Id,
                s.Name
            })
            .ToList();

        return Json(statuses);
    }

    //came with the default project:
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}