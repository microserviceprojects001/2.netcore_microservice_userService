using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectContributorEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.ProjectContributor>
    {

        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.ProjectContributor> builder)
        {
            builder.ToTable("ProjectContributors");

            builder.HasKey(pc => pc.Id);

            builder.Property(pc => pc.ProjectId)
                .IsRequired();

            builder.Property(pc => pc.UserId)
                .IsRequired();

            builder.Property(pc => pc.UserName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(pc => pc.Avatar)
                .HasMaxLength(200);

            builder.Property(pc => pc.CreatedTime)
                .IsRequired();
        }
    }
}