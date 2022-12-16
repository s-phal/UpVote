using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Diagnostics;
using System.Linq;
using VotingApp.Data;
using VotingApp.Migrations;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Member> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }


        [Route("search/{searchTerm?}")]
        public async Task<IActionResult> Search(string? searchTerm)
        {
            if (searchTerm == null)
            {
                return RedirectToAction("index", "ideas");
            }
            if (searchTerm == "topVoted")
            {
                var mostVotes = await _context.Vote
                    .GroupBy(v => v.IdeaId)
                    .OrderByDescending(gp => gp.Count())
                    .Select(g => g.Key)
                    .ToListAsync();

                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(idea => mostVotes.Contains(idea.Id))
                    .OrderBy(idea => mostVotes.IndexOf(idea.Id))
                    .ToList();

                return View(searchResults);

            }
            if (searchTerm == "mostComments")
            {
                var mostComments = await _context.Comment
                    .GroupBy(c => c.IdeaId)
                    .OrderByDescending(gp => gp.Count())
                    .Select(g => g.Key)
                    .ToListAsync();

                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(idea => mostComments.Contains(idea.Id))
                    .OrderBy(idea => mostComments.IndexOf(idea.Id))
                    .ToList();

                return View(searchResults);
            }
            if (searchTerm == "myIdeas")
            {
                var searchResults = await _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(i => i.MemberId == _userManager.GetUserId(User))
                    .ToListAsync();
                if(searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - you currently do not have any ideas.";
                    return View(searchResults);

                }

                return View(searchResults);
            }


            if (searchTerm == "spam")
            {

                var getComments = await _context.Comment
                    .Where(c => c.SpamReports != 0)
                    .Select(i => i.IdeaId)
                    .ToListAsync();

                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Where(i => getComments.Contains(i.Id) || i.SpamReports != 0)
                    .ToListAsync();


                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "There are currently 0 spam reported.";
                    return View(searchResults);

                }
                return View(searchResults);
            }

            if (searchTerm == "spamOrig")
            {
                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Include(i => i.Votes)
                    .Include(i => i.Category)
                    .Where(i => i.SpamReports != 0)
                    .ToListAsync();

                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "There are currently 0 spam reported.";
                    return View(searchResults);

                }
                return View(searchResults);
            }



            if (searchTerm != null)
            {
                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Include(i => i.Votes)
                    .Include(i => i.Category)
                    .Where(i => i.Title.ToLower().Contains(searchTerm.ToLower()) ||
                                i.Description.ToLower().Contains(searchTerm.ToLower()))
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - your query returned no results.";
                    return View(searchResults);
                }
                return View(searchResults);
            }

            return RedirectToAction("index","ideas");
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
