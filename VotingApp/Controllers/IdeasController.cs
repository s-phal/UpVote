using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;


// TODO order by vote count

namespace VotingApp.Controllers
{
    public class IdeasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;
        private readonly SignInManager<Member> _signInManager;

        public IdeasController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Ideas
        [Route("{string?}")]
        public IActionResult Index()
        {
            return Redirect("~/status/all");
        }

        // GET: Ideas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Idea == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CreatedDate,UpdatedDate,CategoryId,MemberId")] Idea idea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(idea);
                await _context.SaveChangesAsync();

                Vote vote = new Vote()
                {
                    IdeaId = idea.Id,
                    MemberId = idea.MemberId,
                };

                _context.Add(vote);
				await _context.SaveChangesAsync();

				return RedirectToAction("details","ideas", new { id = idea.Id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        // GET: Ideas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Idea == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea.FindAsync(id);
            if (idea == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedDate,UpdatedDate,CategoryId,MemberId")] Idea idea)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var getCurrentValueFromDB = await _context.Idea
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == idea.Id);

                    idea.CreatedDate = getCurrentValueFromDB.CreatedDate;

                    _context.Update(idea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IdeaExists(idea.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("details","ideas", new {id = idea.Id});
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }


        [Route("status/{statusTerm?}")]
        public async Task<IActionResult> DisplayStatus(string? statusTerm)
        {
            if (statusTerm == null)
            {
                return RedirectToAction("index", "ideas");
            }
            if (statusTerm == "all")
            {
                var getAll = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

                return View(getAll);
            }

            var getResults = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                .Where(i => i.CurrentStatus.ToLower() == statusTerm.ToLower())
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            if (getResults.Count() == 0)
            {
                return RedirectToAction("index", "ideas");
            }

            return View(getResults);
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(Idea idea, Comment comment)
        {
            Comment newComment = new Comment()
            {
                Body = comment.Body,
                IdeaId = idea.Id,
                MemberId = _userManager.GetUserId(User)
            };

            _context.Add(newComment);
            _context.Update(idea);
           await _context.SaveChangesAsync();

            return RedirectToAction("details", "ideas", new { id = idea.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModerateIdea(int id, Idea idea)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // we need to ensure that on post update, the
                    // CreatedDate property does not get overridden.
                    // use the current values from the database
                    // in place of the instantiated data.
                    var getCurrentValueFromDB = await _context.Idea
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == idea.Id);

                    idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
                    idea.CurrentStatus = "closed";
                    idea.IsModerated = true;

                    _context.Update(idea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IdeaExists(idea.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("details","ideas", new { id = id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVoteDetails(Idea idea)
        {
            // check to see if user has casted a vote yet:
            // find the row that matches both UserID and IdeaID
            // record a new vote if the row can not be found (user hasn't voted)
            var memberVoteCount = _context.Vote
                .Where(v => v.IdeaId == idea.Id && v.MemberId == _userManager.GetUserId(User))
                .ToList();

            if(memberVoteCount.Count() == 0)
            {                
                Vote vote = new Vote()
                {
                    IdeaId = idea.Id,
                    MemberId = idea.MemberId,                    
                };

                _context.Add(vote);
            }
            else
            {
                //TempData["DisplayMessage"] = "Error - you already voted!";
                return RedirectToAction("details", "ideas", new { id = idea.Id });

            }

            // we need to ensure that on post update, Idea
            // properties does not get overridden.
            // use the current values from the database
            // in place of the instantiated data.
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;


            _context.Update(idea);
            _context.SaveChanges();
            //TempData["DisplayMessage"] = "Vote recorded!";
            return RedirectToAction("details", "ideas", new { id = idea.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveVoteDetails(Idea idea)
        {
            var memberVoteCount =await _context.Vote
                .Where(v => v.IdeaId == idea.Id && v.MemberId == _userManager.GetUserId(User))
                .FirstOrDefaultAsync();

            if (memberVoteCount != null)
            {
                _context.Vote.Remove(memberVoteCount);
                await _context.SaveChangesAsync();
                //TempData["DisplayMessage"] = "Vote removed!";
                return RedirectToAction("details", "ideas", new { id = idea.Id });
            }

            return RedirectToAction("details", "ideas", new { id = idea.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddVoteIndex(Idea idea)
        {

            // check to see if user has casted a vote yet:
            // find the row that matches both UserID and IdeaID
            // record a new vote if the row can not be found (user hasn't voted)
            var memberVoteCount = _context.Vote
                .Where(v => v.IdeaId == idea.Id && v.MemberId == _userManager.GetUserId(User))
                .ToList();

            if (memberVoteCount.Count() == 0)
            {
                Vote vote = new Vote()
                {
                    IdeaId = idea.Id,
                    MemberId = idea.MemberId,
                };

                _context.Add(vote);
            }
            else
            {
                //TempData["DisplayMessage"] = "Error - you already voted!";
                return Redirect("~/status/all");

            }

            // we need to ensure that on post update, Idea
            // properties does not get overridden.
            // use the current values from the database
            // in place of the instantiated data.
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CurrentStatus= getCurrentValueFromDB.CurrentStatus;
            

            _context.Update(idea);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Vote casted!";
            return Redirect("~/status/all");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveVoteIndex(Idea idea)
        {
            var memberVoteCount = await _context.Vote
                .Where(v => v.IdeaId == idea.Id && v.MemberId == _userManager.GetUserId(User))
                .FirstOrDefaultAsync();

            if (memberVoteCount != null)
            {
                _context.Vote.Remove(memberVoteCount);
                await _context.SaveChangesAsync();
                TempData["DisplayMessage"] = "Vote removed!";
                return Redirect("~/status/all");
            }

            return Redirect("~/status/all");

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {

           _context.Add(comment);
            _context.SaveChanges();
            return RedirectToAction("Details", "ideas", new {id = comment.IdeaId} );
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReportSpam(Idea idea)
        {
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.Title = getCurrentValueFromDB.Title;
            idea.Description = getCurrentValueFromDB.Description;
            idea.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            idea.CategoryId = getCurrentValueFromDB.CategoryId;
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;
            idea.IsModerated = getCurrentValueFromDB.IsModerated;
            idea.SpamReports = idea.SpamReports + 1;

            _context.Update(idea);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Post has been reported.";
            return Redirect("~/status/all");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetSpamCounter(Idea idea)
        {
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.Title = getCurrentValueFromDB.Title;
            idea.Description = getCurrentValueFromDB.Description;
            idea.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            idea.CategoryId = getCurrentValueFromDB.CategoryId;
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;
            idea.IsModerated = getCurrentValueFromDB.IsModerated;
            idea.SpamReports = 0;

            _context.Update(idea);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Post has been reset.";
            return Redirect("~/status/all");
        }

        [Authorize]
        [HttpPost]        
        public async Task<IActionResult> ReportSpamComment(Comment comment)
        {
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            comment.MemberId = getCurrentValueFromDB.MemberId;
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.Body = getCurrentValueFromDB.Body;
            comment.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            comment.SpamReports = comment.SpamReports + 1;

            _context.Update(comment);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Comment has been reported.";
            return RedirectToAction("details","ideas", new { id = comment.IdeaId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetSpamCommentCounter(Comment comment)
        {
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            comment.MemberId = getCurrentValueFromDB.MemberId;
            comment.Body = getCurrentValueFromDB.Body;
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            comment.SpamReports = 0;

            _context.Update(comment);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Post has been reset.";
            return RedirectToAction("details", "ideas", new { id = comment.IdeaId});
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteComment(Comment comment)
        {
            var theComment = await _context.Comment.FindAsync(comment.Id);

            _context.Comment.Remove(theComment);
            await _context.SaveChangesAsync();
            return RedirectToAction("details", "ideas", new { id = comment.IdeaId });

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteIdea(int id)
        {
            if (_context.Idea == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Idea'  is null.");
            }
            var idea = await _context.Idea.FindAsync(id);
            if (idea != null)
            {
                _context.Idea.Remove(idea);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IdeaExists(int id)
        {
          return (_context.Idea?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
