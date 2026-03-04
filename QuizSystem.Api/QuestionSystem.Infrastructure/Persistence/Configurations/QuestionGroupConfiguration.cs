using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizSystem.Api.QuestionSystem.Domain.Entities;

namespace QuizSystem.Api.QuestionSystem.Infrastructure.Persistence.Configurations;

public class QuestionGroupConfiguration : IEntityTypeConfiguration<QuestionGroup>
{
    // Configuration code for QuestionGroup entity goes here
    public void Configure(EntityTypeBuilder<QuestionGroup> builder)
    {
        builder.ToTable("QuestionGroups");

        builder.HasKey(qg => qg.Id);

        builder.Property(qg => qg.Name)
            .IsRequired()
            .HasMaxLength(200);

    }
}
