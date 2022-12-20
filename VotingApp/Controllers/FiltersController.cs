using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;
using X.PagedList;

namespace VotingApp.Controllers
{
    public class FiltersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public FiltersController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Route("search/{searchTerm?}")]
        [Route("filters/{searchTerm?}")]
        public async Task<IActionResult> Search(string? searchTerm, int? page)
        {
            int pageSize = 8; // Views per page
            int pageNumber = (page ?? 1); // If no parameter is given, defaults to 1

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
                TempData["StatusTerm"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));


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
                TempData["StatusTerm"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }
            if (searchTerm == "myIdeas")
            {
                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(i => i.MemberId == _userManager.GetUserId(User))
                    .ToList();

                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - you currently do not have any ideas.";
                    TempData["StatusTerm"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));


                }
                TempData["StatusTerm"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));


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
                    TempData["StatusTerm"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));


                }
                TempData["StatusTerm"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }
            if (searchTerm != null)
            {
                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Include(i => i.Votes)
                    .Include(i => i.Category)
                    .Where(i => i.Title.ToLower().Contains(searchTerm.ToLower()) || i.Category.Name.ToLower().Contains(searchTerm.ToLower()) ||  
                                i.Description.ToLower().Contains(searchTerm.ToLower()))
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - your query returned no results.";
                    TempData["StatusTerm"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));

                }
                TempData["StatusTerm"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }

            return RedirectToAction("index", "ideas");
        }
    }
}
