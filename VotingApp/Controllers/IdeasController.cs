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
        public async Task<IActionResult> Index()
        {
            var ideas = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                //.OrderByDescending() TODO Order by vote count
                .ToListAsync();
                


            return View(ideas);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        // GET: Ideas/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name");
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Ideas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CreatedDate,PostedDate,CategoryId,MemberId")] Idea idea)
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

        // POST: Ideas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,CreatedDate,PostedDate,CategoryId,MemberId")] Idea idea)
        {
            if (id != idea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        // GET: Ideas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Idea == null)
            {
                return NotFound();
            }

            var idea = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        [Authorize]
        public async Task<IActionResult> AddVote(Idea idea)
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
                TempData["DisplayMessage"] = "You already Voted!";
                return RedirectToAction("index", "ideas");
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

            _context.Update(idea);
                _context.SaveChanges();
            TempData["DisplayMessage"] = "Vote recorded!";
            return RedirectToAction("index", "ideas");


        }

        [Authorize]
        public async Task<IActionResult> RemoveVote(Idea idea)
        {
            var memberVoteCount =await _context.Vote
                .Where(v => v.IdeaId == idea.Id && v.MemberId == _userManager.GetUserId(User))
                .FirstOrDefaultAsync();

            if (memberVoteCount != null)
            {
                _context.Vote.Remove(memberVoteCount);
                await _context.SaveChangesAsync();
                TempData["DisplayMessage"] = "Vote removed!";
                return RedirectToAction("index", "ideas");
            }

            return RedirectToAction("index", "ideas");



        }





        // POST: Ideas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
