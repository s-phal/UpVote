using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Idea()
        {
            return View();
        }

        [Route("search/{searchTerm?}")]
        public async Task<IActionResult> Search(string? searchTerm)
        {
            if(searchTerm == null)
            {
                return RedirectToAction("index", "ideas");
            }


            var searchResults = await _context.Idea
                .Where(i => i.Title.ToLower().Contains(searchTerm.ToLower()) ||
                            i.Description.ToLower().Contains(searchTerm.ToLower()))
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            if(searchResults.Count() == 0)
            {
                TempData["DisplayMessage"] = "Error - your query returned no results.";
                return View(searchResults);

            }

            return View(searchResults);
        }

        public IActionResult Redirect()
        {
            return Redirect("~/status/all");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
