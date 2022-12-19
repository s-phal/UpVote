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

                Notification notification = new Notification()
                {
                    Description = "added_idea",
                    Subject = idea.Title,
                    IdeaId = idea.Id,
                    MemberId = idea.MemberId,
                    NotificationOwnerId = idea.MemberId

                };
                _context.Add(notification);

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

            Notification notification = new Notification()
            {
                Description = "status_changed",
                Subject = idea.Title,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IdeaId = idea.Id,
                MemberId = idea.MemberId,
                NotificationOwnerId = _userManager.GetUserId(User)
            };
            _context.Add(notification);
            _context.SaveChanges();

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
