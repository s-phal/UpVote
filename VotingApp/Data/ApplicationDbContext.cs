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
        public DbSet<VotingApp.Models.Category> Category { get; set; } = default!;
        public DbSet<VotingApp.Models.Idea> Idea { get; set; } = default!;
        public DbSet<VotingApp.Models.Vote> Vote { get; set; } = default!;
        public DbSet<VotingApp.Models.Comment> Comment { get; set; } = default!;

    }
}