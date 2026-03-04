using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Type)
            .IsRequired();

        builder.Property(q => q.FolderId)
            .IsRequired();

        builder.OwnsOne(q => q.Text, text =>
        {
            text.Property(t => t.Value)
                .HasColumnName("Text")
                .IsRequired()
                .HasMaxLength(500);
        });

        builder.HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey("QuestionId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
