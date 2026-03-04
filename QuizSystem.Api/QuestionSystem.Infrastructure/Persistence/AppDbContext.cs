using System;
using Microsoft.EntityFrameworkCore;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        /*modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);*/

        modelBuilder.Entity<Answer>(answer =>
        {
            answer.HasKey(a => a.Id);

            answer.OwnsMany(a => a.Values, values =>
            {
                values.WithOwner()
                    .HasForeignKey("AnswerId");

                values.Property<Guid>("Id");
                values.HasKey("Id");

                values.Property(v => v.TextValue)
                    .HasMaxLength(1000);

                values.Property(v => v.OptionId);

                values.ToTable("AnswerValues");
            });
        });

        modelBuilder.Entity<Question>(question =>
        {
            question.HasKey(q => q.Id);

            question.OwnsOne(q => q.Text, text =>
            {
                text.Property(t => t.Value)
                    .HasColumnName("Text")
                    .HasMaxLength(500)
                    .IsRequired();
            });
            question.HasQueryFilter(q => !q.IsDeleted);
        });

        modelBuilder.Entity<Answer>(answer =>
        {
            answer.HasKey(a => a.Id);

            answer.Property(a => a.QuestionId).IsRequired();
            answer.Property(a => a.RespondentId).IsRequired();

            answer.OwnsMany(a => a.Values, values =>
            {
                values.WithOwner()
                    .HasForeignKey("AnswerId");

                values.Property<Guid>("Id");
                values.HasKey("Id");

                values.Property(v => v.TextValue)
                    .HasMaxLength(1000);

                values.Property(v => v.OptionId);

                values.ToTable("AnswerValues");
            });
        });

        modelBuilder.Entity<QuestionOption>(option =>
        {
            option.HasKey(o => o.Id);
            option.Property(o => o.Text)
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<QuestionGroup>(group =>
        {
            group.HasKey(g => g.Id);
            group.Property(g => g.Name)
                .HasMaxLength(200)
                .IsRequired();

            group.Property(q => q.OrganisationId)
                .HasMaxLength(36)
                .IsRequired(false);
        });

        modelBuilder.Entity<Folder>(folder =>
        {
            folder.HasKey(f => f.Id);
            folder.Property(f => f.Name)
                .HasMaxLength(200)
                .IsRequired();
            folder.HasQueryFilter(f => !f.IsDeleted);
        });

        modelBuilder.Entity<QuizAttempt>()
        .HasMany(q => q.Answers)
        .WithOne(a => a.QuizAttempt)
        .HasForeignKey(a => a.QuizAttemptId);


    }

}

