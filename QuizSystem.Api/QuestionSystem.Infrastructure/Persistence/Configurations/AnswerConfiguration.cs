using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Persistence.Configurations;

public sealed class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.ToTable("Answers");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.QuestionId)
                .IsRequired();

        builder.Property(a => a.RespondentId)
                .IsRequired();

        // THIS IS THE KEY CHANGE:
        // Map the collection of AnswerValue as an owned entity
        builder.OwnsMany(a => a.Values, values =>
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
    }
}