using DSAMate.API.Data.Domains;
using DSAMate.API.Models.Domains;
using Microsoft.EntityFrameworkCore;

namespace DSAMate.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions) { }
        public required DbSet<Question> Questions { get; set; }
        public required DbSet<UserQuestionStatus> UserQuestionStatuses { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Save Difficulty and Topic as strings
            modelBuilder.Entity<Question>()
                .Property(q => q.Difficulty)
                .HasConversion<string>();

            modelBuilder.Entity<Question>()
                .Property(q => q.Topic)
                .HasConversion<string>();

            // Index on Title
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.Title)
                .IsUnique();

            // Index on Topic
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.Topic)
                .HasDatabaseName("IX_Questions_Topic");

            // Index on Difficulty
            modelBuilder.Entity<Question>()
                .HasIndex(q => q.Difficulty)
                .HasDatabaseName("IX_Questions_Difficulty");

            // Indexing for (userId, questionId) in UserQuestionStatus table
            modelBuilder.Entity<UserQuestionStatus>()
                .HasKey(uqs => new { uqs.UserId, uqs.QuestionId });

            // For question -> user realtion
            modelBuilder.Entity<UserQuestionStatus>()
                .HasOne(uqs => uqs.Question)
                .WithMany()
                .HasForeignKey(uqs => uqs.QuestionId) 
                .OnDelete(DeleteBehavior.Cascade);  // If a Question is deleted, delete their statuses too
        }
    }
}
