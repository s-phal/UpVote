using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Diagnostics;
using System.Linq;
using VotingApp.Data;
using VotingApp.Migrations;
using VotingApp.Models;
using X.PagedList;

namespace VotingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<Member> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }




        public IActionResult Redirect()
        {
            return Redirect("~/ideas/?s=all");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
