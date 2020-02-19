using JoggingTracker.DataAccess.DbEntities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JoggingTracker.DataAccess
{
    public class JoggingTrackerDataContext : IdentityDbContext<UserDb>
    {
        public virtual DbSet<RunDb> Runs { get; set; }

        public JoggingTrackerDataContext(DbContextOptions<JoggingTrackerDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RunDb>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<RunDb>()
                .HasOne<UserDb>()
                .WithMany()
                .HasForeignKey(x => x.UserId);
        }
    }
}