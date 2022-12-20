using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class SpamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpamsController(ApplicationDbContext context) 
        {
            _context= context;
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
            return Redirect("~/ideas/");
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
            TempData["DisplayMessage"] = "Comment has been reset.";
            return RedirectToAction("details", "ideas", new { id = comment.IdeaId });
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
            return RedirectToAction("details", "ideas", new { id = comment.IdeaId });
        }
    }
}
