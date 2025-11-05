using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectViewerEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.ProjectViewer>
    {
        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.ProjectViewer> builder)
        {
            builder.ToTable("ProjectViewers");

            builder.HasKey(pv => pv.Id);

            builder.Property(pv => pv.ProjectId)
                .IsRequired();

            builder.Property(pv => pv.UserId)
                .IsRequired();

            builder.Property(pv => pv.UserName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(pv => pv.Avatar)
                .HasMaxLength(200);

            builder.Property(pv => pv.CreatedTime)
                .IsRequired();
        }
    }
}