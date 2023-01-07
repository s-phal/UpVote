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
            // pagination properties
            // posts per page
            int pageSize = 8; 
            int pageNumber = (page ?? 1);

            // redirect based on given POSTS data
            if (searchTerm == null)
            {
                return RedirectToAction("~/");
            }
            if (searchTerm == "topVoted")
            {
                // from Votes table, get and sort rows by vote count
                var mostVotes = await _context.Vote
                    .GroupBy(v => v.IdeaId)
                    .OrderByDescending(gp => gp.Count())
                    .Select(g => g.Key)
                    .ToListAsync();

                // from Ideas table, get rows that reference IDs from mostVotes
                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(idea => mostVotes.Contains(idea.Id))
                    .OrderBy(idea => mostVotes.IndexOf(idea.Id))
                    .ToList();

                // passed to pagination method in View, neccessary for dynamic hrefs
                TempData["ModelName"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));


            }
            if (searchTerm == "mostComments")
            {
                // from Comments table, get and sort rows by comment count
                var mostComments = await _context.Comment
                    .GroupBy(c => c.IdeaId)
                    .OrderByDescending(gp => gp.Count())
                    .Select(g => g.Key)
                    .ToListAsync();

                // from Ideas table, get rows that reference IDs from mostComments
                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(idea => mostComments.Contains(idea.Id))
                    .OrderBy(idea => mostComments.IndexOf(idea.Id))
                    .ToList();

                // passed to pagination method in View, neccessary for dynamic hrefs
                TempData["ModelName"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }
            if (searchTerm == "myIdeas")
            {
                // get ideas that belongs to logged in user
                var searchResults = _context.Idea
                    .Include(i => i.Votes)
                    .Include(i => i.Comments)
                    .Include(i => i.Member)
                    .Include(i => i.Category)
                    .Where(i => i.MemberId == _userManager.GetUserId(User))
                    .ToList();

                // display message if no ideas found.
                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - you currently do not have any ideas.";
                    TempData["ModelName"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));
                }
                // passed to pagination method in View, neccessary for dynamic hrefs
                TempData["ModelName"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));


            }
            if (searchTerm == "spam")
            {
                // get all rows that has SpamReports
                var getComments = await _context.Comment
                    .Where(c => c.SpamReports != 0)
                    .Select(i => i.IdeaId)
                    .ToListAsync();

                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Include(i => i.Votes)
                    .Where(i => getComments.Contains(i.Id) || i.SpamReports != 0)
                    .ToListAsync();

                // display message if none found
                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "There are currently 0 spam reported.";
                    TempData["ModelName"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));
                }
                // passed to pagination method in View, neccessary for dynamic hrefs
                TempData["ModelName"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }
            if (searchTerm != null)
            {
                // logic for search input field
                // convert both searchTerm and target
                // to lowercase for case sensitivity
                // search fields that contains searchTerm
                var searchResults = await _context.Idea
                    .Include(i => i.Comments)
                    .Include(i => i.Votes)
                    .Include(i => i.Category)
                    .Where(i => i.Title.ToLower().Contains(searchTerm.ToLower()) || i.Category.Name.ToLower().Contains(searchTerm.ToLower()) ||  
                                i.Description.ToLower().Contains(searchTerm.ToLower()))
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                // display message if searchTerm is not found
                if (searchResults.Count() == 0)
                {
                    TempData["DisplayMessage"] = "Error - your query returned no results.";

                    // passed to pagination method in View, neccessary for dynamic hrefs
                    TempData["ModelName"] = searchTerm;
                    return View(searchResults.ToPagedList(pageNumber, pageSize));

                }

                // passed to pagination method in View, neccessary for dynamic hrefs
                TempData["ModelName"] = searchTerm;
                return View(searchResults.ToPagedList(pageNumber, pageSize));

            }

            // redirect to homepage if nothing matches
            return RedirectToAction("~/");
        }
    }
}
