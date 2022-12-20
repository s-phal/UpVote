using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class IdeasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Member> _userManager;

        public IdeasController(ApplicationDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // START PAGE
        [Route("ideas/{s?}")]
        public async Task<IActionResult> DisplayStatus(string? s, int? page)
        {
            int pageSize = 8; // Views per page
            int pageNumber = (page ?? 1); // If no parameter is given, defaults to 1


            if (s == "create")
            {
                return RedirectToAction("create", "ideas");
            }
            if (s == "all" || s == null)
            {
                var getAll = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

                if (s == null)
                {
                    TempData["StatusTerm"] = "";
                }
                else
                {
                    TempData["StatusTerm"] = s;
                }
                return View(getAll.ToPagedList(pageNumber, pageSize));
            }

            var getResults = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                .Where(i => i.CurrentStatus.ToLower() == s.ToLower())
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            if (getResults.Count() == 0)
            {
                return Redirect("~/?s=all");

            }

            TempData["StatusTerm"] = s;
            return View(getResults.ToPagedList(pageNumber, pageSize));
        }


        [Route("~/")]
        public IActionResult Index()
        {
            return Redirect("~/ideas/?s=all");

        }

        // GET: Ideas/Details/5
        [Route("ideas/details/{slug?}")]
        public async Task<IActionResult> Details(string? slug, int? id)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }


            var idea = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(m => m.Slug == slug);
            if(slug.All(char.IsDigit))
            {
                var ideaId = await _context.Idea
                .Include(i => i.Category)
                .Include(i => i.Member)
                .Include(i => i.Votes)
                .Include(i => i.Comments)
                    .ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(idea => idea.Id.ToString() == slug);
                return View(ideaId);

            }
            if (idea == null)
            {
                return NotFound();
            }

            return View(idea);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/ideas/create")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,CreatedDate,UpdatedDate,CategoryId,MemberId")] Idea idea)
        {
            if (ModelState.IsValid)
            {
                string slug = GenerateSlug(idea);
                if(SlugExist(slug) == true)
                {
                    ModelState.AddModelError("Title", "Error, Slug already exists. Please choose another title.");
                    ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Name", idea.CategoryId);
                    ViewData["MemberId"] = new SelectList(_context.Set<Member>(), "Id", "Id", idea.MemberId);
                    return View(idea);
                }
                if (SlugExist(slug) == false)
                {
                    idea.Slug = slug;
                }

                if (idea.CategoryId == 0)
                {
                    TempData["DisplayMessage"] = "Error - Please choose a category";
                    return View(idea);

                }
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

                return RedirectToAction("details", "ideas", new { slug = idea.Slug });
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        private bool SlugExist(string slug)
        {
            return _context.Idea.Any(Post => Post.Slug == slug);
        }

        public string GenerateSlug(Idea idea)
        {
            if (idea.Title == null) return "";
            const int maxlen = 80;
            var len = idea.Title.Length;
            var prevdash = false;
            var sb = new StringBuilder(len);
            char c;
            for (int i = 0; i < len; i++)
            {
                c = idea.Title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                        c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if (c == '#')
                {
                    if (i > 0)
                        if (idea.Title[i - 1] == 'C' || idea.Title[i - 1] == 'F')
                            sb.Append("-sharp");
                }
                else if (c == '+')
                {
                    sb.Append("-plus");
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (sb.Length == maxlen) break;
            }
            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        private string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
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
                return RedirectToAction("details", "ideas", new { id = idea.Id });
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }


        [HttpPost]
        [Route("ideas/setstatus")]
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

            return RedirectToAction("details", "ideas", new { slug = idea.Slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ideas/moderateidea/")]
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

                    idea.Slug = getCurrentValueFromDB.Slug;
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
                return RedirectToAction("details", "ideas", new { slug = idea.Slug });
            }
            ViewData["CategoryId"] = new SelectList(_context.Category, "Id", "Id", idea.CategoryId);
            ViewData["MemberId"] = new SelectList(_context.Users, "Id", "Id", idea.MemberId);
            return View(idea);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ideas/delete")]
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
