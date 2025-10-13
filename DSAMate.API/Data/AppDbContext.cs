using DSAMate.API.Data.Domains;
using Microsoft.EntityFrameworkCore;

namespace DSAMate.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions) { }
        public DbSet<Question> Questions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Save Difficulty and Topic as strings
            modelBuilder.Entity<Question>()
                .Property(q => q.Difficulty)
                .HasConversion<string>();
            modelBuilder.Entity<Question>()
                .Property(q => q.Topic)
                .HasConversion<string>();
        }
    }
}
