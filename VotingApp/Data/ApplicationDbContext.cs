using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VotingApp.Models;

namespace VotingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<Member>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}