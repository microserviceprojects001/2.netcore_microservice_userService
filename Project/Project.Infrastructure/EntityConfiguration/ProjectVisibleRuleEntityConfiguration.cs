using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectVisibleRuleEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.ProjectVisibleRule>
    {
        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.ProjectVisibleRule> builder)
        {
            builder.ToTable("ProjectVisibleRules");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.ProjectId)
                .IsRequired();

            builder.Property(p => p.Visible)
                .IsRequired();

            builder.Property(p => p.Tags)
                .HasMaxLength(500);
        }
    }
}