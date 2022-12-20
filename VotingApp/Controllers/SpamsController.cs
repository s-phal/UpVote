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
            // add +1 to current value of SpamReports
            // preserve all other data
            idea.SpamReports = idea.SpamReports + 1;

            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, Title, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            // use the data in its current state
            // to populate the properties accordingly
            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.Title = getCurrentValueFromDB.Title;
            idea.Description = getCurrentValueFromDB.Description;
            idea.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            idea.CategoryId = getCurrentValueFromDB.CategoryId;
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;
            idea.IsModerated = getCurrentValueFromDB.IsModerated;
            idea.Slug = getCurrentValueFromDB.Slug;

            // track the idea
            // write changes to the table
            _context.Update(idea);
            _context.SaveChanges();

            // notify user of action
            TempData["DisplayMessage"] = "Post has been reported.";
            return Redirect("~/");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetSpamCounter(Idea idea)
        {
            // resets the SpamReport
            // preserve all other data
            idea.SpamReports = 0;

            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, Title, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Idea
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == idea.Id);

            // use the data in its current state
            // to populate the properties accordingly
            idea.Slug = getCurrentValueFromDB.Slug;
            idea.MemberId = getCurrentValueFromDB.MemberId;
            idea.CreatedDate = getCurrentValueFromDB.CreatedDate;
            idea.Title = getCurrentValueFromDB.Title;
            idea.Description = getCurrentValueFromDB.Description;
            idea.UpdatedDate = getCurrentValueFromDB.UpdatedDate;
            idea.CategoryId = getCurrentValueFromDB.CategoryId;
            idea.CurrentStatus = getCurrentValueFromDB.CurrentStatus;
            idea.IsModerated = getCurrentValueFromDB.IsModerated;

            // track the idea
            // write to the table
            _context.Update(idea);
            _context.SaveChanges();

            //notify user of action
            TempData["DisplayMessage"] = "Post has been reset.";
            return Redirect("~/");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReportSpamComment(Comment comment)
        {
            // add +1 to current value of SpamReports
            // preserve all other data
            comment.SpamReports = comment.SpamReports + 1;

            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, Body, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            // use the data in its current state
            // to populate the properties accordingly
            comment.MemberId = getCurrentValueFromDB.MemberId;
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.Body = getCurrentValueFromDB.Body;
            comment.UpdatedDate = getCurrentValueFromDB.UpdatedDate;

            // track the comment
            // write changes to the table
            _context.Update(comment);
            _context.SaveChanges();

            // notify user of action
            TempData["DisplayMessage"] = "Comment has been reported.";

            // this line is for referencing the redirect
            var idea = await _context.Idea.FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ResetSpamCommentCounter(Comment comment)
        {
            // resets the SpamReport
            // preserve all other data
            comment.SpamReports = 0;

            // when editing a row we want to preserve certain
            // data in its previous state. CreatedDate, Body, etc
            // should remain the same. without this logic, each new instance
            // will add new default values.

            // get row using the given id
            var getCurrentValueFromDB = await _context.Comment
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == comment.Id);

            // use the data in its current state
            // to populate the properties accordingly
            comment.MemberId = getCurrentValueFromDB.MemberId;
            comment.Body = getCurrentValueFromDB.Body;
            comment.CreatedDate = getCurrentValueFromDB.CreatedDate;
            comment.UpdatedDate = getCurrentValueFromDB.UpdatedDate;

            // track the comment
            // write changes to the table
            _context.Update(comment);
            _context.SaveChanges();

            // notify user of action
            TempData["DisplayMessage"] = "Comment has been reset.";

            // this line is used as a reference when redirecting
            var idea = await _context.Idea
                .FirstOrDefaultAsync(i => i.Id == comment.IdeaId);
            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

    }
}
