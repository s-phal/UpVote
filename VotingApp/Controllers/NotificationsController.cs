using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingApp.Data;
using VotingApp.Models;

namespace VotingApp.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteNotifications()
        {
            var notificationList = await _context.Notification.Where(n => n.MemberId == _userManager.GetUserId(User)).ToListAsync();

            _context.Notification.RemoveRange(notificationList);
            await _context.SaveChangesAsync();
            return Redirect("~/status/all");

        }
    }
}
