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
            // get the idea row using the given ID
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);

            // display error message accordingly.
            // views will check for TempData and will display
            // if keys are passed through
            if (comment.Body == null)
            {               
                TempData["DisplayMessage"] = "Error - Comments can not be empty";
                return RedirectToAction("Details", "ideas", new { slug = idea.Slug });
            }
            if (comment.Body.Length > 300)
            {
                TempData["DisplayMessage"] = "Error - Comment must not exceed 300 characters.";
                return RedirectToAction("Details", "ideas", new { slug = idea.Slug });
            }

            // track the commment
            // write to the database
            _context.Add(comment);
            _context.SaveChanges();

            // create a new instance of a Notification
            // and write it to the database
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

            // display message on current page
            TempData["DisplayMessage"] = "Comment has been posted!";
            return RedirectToAction("Details", "ideas", new { slug = idea.Slug});
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(Comment comment)
        {
            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, SpamReports, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            // use the data in its current state
            // to populate the properties accordingly
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.SpamReports = getCurrentValueFromDB.SpamReports;

            // track the comment then
            // write to the database
            _context.Update(comment);
            await _context.SaveChangesAsync();


            // display the message on the current page
            TempData["DisplayMessage"] = "Comment Updated!";

            // this line is used for reference when redirecting
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModerateComment(Comment comment)
        {

            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, SpamReports, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            // use the data in its current state
            // to populate the properties accordingly
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = DateTime.UtcNow;
            comment.SpamReports = getCurrentValueFromDB.SpamReports;
            comment.IsModerated = true;

            // track the comment then
            // write to the database
            _context.Update(comment);
            await _context.SaveChangesAsync();

            // display the message on current page
            TempData["DisplayMessage"] = "Comment Updated!";

            // this line is used for reference when redirecting
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteComment(Comment comment)
        {
            // get the comment row using the given Id
            var theComment = await _context.Comment.FindAsync(comment.Id);

            // track the row for deletion then write to the database
            _context.Comment.Remove(theComment);
            await _context.SaveChangesAsync();

            // display message on current page
            TempData["DisplayMessage"] = "Comment has been removed.";

            // this line is used for reference when redirecting
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

    }
}
