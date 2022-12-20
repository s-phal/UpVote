using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            if (comment.Body == null)
            {
                return RedirectToAction("Details", "ideas", new { slug = idea.Slug });
            }
            if (comment.Body.Length > 300)
            {
                TempData["DisplayMessage"] = "Error - Comment must not exceed 300 characters.";
                return RedirectToAction("Details", "ideas", new { slug = idea.Slug });
            }
            _context.Add(comment);
            _context.SaveChanges();


            Notification notification = new Notification()
            {
                Description = "posted_comment",
                Subject = idea.Title,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IdeaId = idea.Id,
                MemberId = idea.MemberId,
                NotificationOwnerId = _userManager.GetUserId(User)
            };
            _context.Add(notification);
            _context.SaveChanges();
            TempData["DisplayMessage"] = "Comment has been posted!";
            return RedirectToAction("Details", "ideas", new { slug = idea.Slug});
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(Comment comment)
        {
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.SpamReports = getCurrentValueFromDB.SpamReports;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            TempData["DisplayMessage"] = "Comment Updated!";
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModerateComment(Comment comment)
        {
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);

            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.SpamReports = getCurrentValueFromDB.SpamReports;
            comment.IsModerated = true;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            TempData["DisplayMessage"] = "Comment Updated!";
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteComment(Comment comment)
        {
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);

            var theComment = await _context.Comment.FindAsync(comment.Id);

            _context.Comment.Remove(theComment);
            await _context.SaveChangesAsync();
            TempData["DisplayMessage"] = "Comment has been removed.";
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });

        }

    }
}
