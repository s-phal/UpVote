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
using X.PagedList;

namespace VotingApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public CategoriesController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [Route("categories/{categoryName?}")]
        public async Task<IActionResult> Index(string? categoryName, int? page)
        {
            // pagination properties
            // posts per page
            int pageSize = 8;
            int pageNumber = (page ?? 1); 

            // redirect based on given POSTS data
            if (categoryName == "createNewCategory")
            {
                return Redirect("~/categories/create");
            }
            if (categoryName == "viewAllCategories")
            {
                return Redirect("~/categories/viewall");
            }
            if (categoryName == null)
            {
                // passed to pagination method in View, neccessary dynamic hrefs
                TempData["ModelName"] = "all";
                return Redirect("~/");
            }
           
            // grab all ideas with given category name
            var idea = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Comments)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Where(c => c.Category.Name.ToLower() == categoryName.ToLower())
                .ToListAsync();
            
            // redirect to homepage if no categories were found
            if (idea.Count() == 0)
            {
                return Redirect("~/");
            }

            // passed to pagination method in View, neccessary for dynamic hrefs
            TempData["ModelName"] = categoryName;
            return View(idea.ToPagedList(pageNumber, pageSize));
        }

        [Authorize]
        [Route("categories/viewall")]
        public IActionResult ViewAll()
        {
            // renders view with list of all categories
            var categories = _context.Category;
            return View(categories);
        }

        [Authorize]
        [Route("categories/create")]
        public async Task<IActionResult> Create()
        {
            // get current user properties
            var user = await _userManager.GetUserAsync(User);

            // redirect if user is not in admin role
            if (user.UserRole != "admin") 
            { 
                return Redirect("~/"); 
            }

            return View();
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("categories/create")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
        {
            // get current user properties
            var user = await _userManager.GetUserAsync(User);

            // redirect if user is not in admin role
            if (user.UserRole != "admin")
            {
                return Redirect("~/");
            }

            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                // redirect to categories list after creation
                return Redirect("~/categories/viewall");
            }
            return View(category);
        }


        // POST: Categories/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Route("categories/delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Category == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Category'  is null.");
            }
            // find category row using given ID
            // if found, remove the row
            // write changes to database
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
               
                _context.Category.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ViewAll","Categories");
        }

        private bool CategoryExists(int id)
        {
          return (_context.Category?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
