using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.ProjectProperty>
    {
        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.ProjectProperty> builder)
        {
            builder.ToTable("ProjectProperties");

            builder.HasKey(pp => new { pp.ProjectId, pp.Key, pp.Value });

            // builder.Property(pp => pp.ProjectId)
            //     .IsRequired();

            builder.Property(pp => pp.Key)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(pp => pp.Value)
                .HasMaxLength(500)
                .IsRequired();
        }
    }
}