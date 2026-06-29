using InterviewBank.API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InterviewBank.API.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Topic>                Topics                => Set<Topic>();
    public DbSet<Question>             Questions             => Set<Question>();
    public DbSet<MockInterviewSession> MockInterviewSessions => Set<MockInterviewSession>();
    public DbSet<SessionQuestion>      SessionQuestions      => Set<SessionQuestion>();
    public DbSet<RefreshToken>         RefreshTokens         => Set<RefreshToken>();
    public DbSet<LibraryQuestion>      LibraryQuestions      => Set<LibraryQuestion>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Topic ────────────────────────────────────────────────────────────
        builder.Entity<Topic>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(t => t.Name).IsUnique();
        });

        // ── Question ─────────────────────────────────────────────────────────
        builder.Entity<Question>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(q => q.Text).HasMaxLength(1000).IsRequired();
            e.Property(q => q.Source).HasMaxLength(500);
            e.Property(q => q.Difficulty).HasConversion<int>();
            e.Property(q => q.QuestionType).HasConversion<int>().HasDefaultValue(QuestionType.Essay);

            e.HasOne(q => q.Topic)
             .WithMany(t => t.Questions)
             .HasForeignKey(q => q.TopicId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(q => q.UserId);
            e.HasIndex(q => q.TopicId);
            e.HasIndex(q => q.Difficulty);
            e.HasIndex(q => q.IsPracticed);
            e.HasIndex(q => new { q.UserId, q.TopicId });
            e.HasIndex(q => q.NextReviewAt);
            e.Property(q => q.EaseFactor).HasDefaultValue(2.5);
        });

        // ── MockInterviewSession ──────────────────────────────────────────────
        builder.Entity<MockInterviewSession>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(s => s.UserId);
            e.HasIndex(s => s.CompletedAt);
        });

        // ── SessionQuestion ───────────────────────────────────────────────────
        builder.Entity<SessionQuestion>(e =>
        {
            e.HasKey(sq => sq.Id);
            e.Property(sq => sq.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(sq => sq.SelfAssessment).HasConversion<int?>();

            e.HasOne(sq => sq.Session)
             .WithMany(s => s.SessionQuestions)
             .HasForeignKey(sq => sq.SessionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(sq => sq.Question)
             .WithMany()
             .HasForeignKey(sq => sq.QuestionId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── LibraryQuestion ───────────────────────────────────────────────────
        builder.Entity<LibraryQuestion>(e =>
        {
            e.HasKey(q => q.Id);
            e.Property(q => q.Text).HasMaxLength(1000).IsRequired();
            e.Property(q => q.TopicName).HasMaxLength(100).IsRequired();
            e.Property(q => q.Difficulty).HasConversion<int>();
            e.HasIndex(q => q.TopicName);
            e.HasIndex(q => q.Difficulty);
        });

        // ── RefreshToken ──────────────────────────────────────────────────────
        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.Token).HasMaxLength(256).IsRequired();
            e.HasIndex(t => t.Token).IsUnique();

            e.HasOne(t => t.User)
             .WithMany()
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Computed properties must be ignored — they are not mapped columns
            e.Ignore(t => t.IsExpired);
            e.Ignore(t => t.IsRevoked);
            e.Ignore(t => t.IsActive);
        });
    }
}
