using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class VotesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public VotesController(ApplicationDbContext context, UserManager<Member> userManager) 
        {
            _context = context;        
            _userManager = userManager;
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
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;


            _context.Update(idea);
            _context.SaveChanges();

            Notification notification = new Notification()
            {
                Description = "upvoted",
                Subject = idea.Title,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IdeaId = idea.Id,
                MemberId = idea.MemberId,
                NotificationOwnerId = _userManager.GetUserId(User)
            };
            _context.Add(notification);
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
        public async Task<IActionResult> AddVoteDetails(Idea idea)
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

            Notification notification = new Notification()
            {
                Description = "upvoted",
                Subject = idea.Title,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IdeaId = idea.Id,
                MemberId = idea.MemberId,
                NotificationOwnerId = _userManager.GetUserId(User)
            };
            _context.Add(notification);
            _context.SaveChanges();
            //TempData["DisplayMessage"] = "Vote recorded!";
            return RedirectToAction("details", "ideas", new { id = idea.Id });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveVoteDetails(Idea idea)
        {
            var memberVoteCount = await _context.Vote
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





    }
}
