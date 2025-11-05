using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.Project>
    {

        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);

            // builder.Property(p => p.UserId).IsRequired();
            // builder.Property(p => p.Avatar).HasMaxLength(256);
            // builder.Property(p => p.Company).HasMaxLength(128).IsRequired();
            // builder.Property(p => p.OriginBPFile).HasMaxLength(512);
            // builder.Property(p => p.FormatBPFile).HasMaxLength(512);
            // builder.Property(p => p.ShowSecurityInfo).IsRequired();
            // builder.Property(p => p.ProvinceId).IsRequired();
            // builder.Property(p => p.Provice).HasMaxLength(64);
            // builder.Property(p => p.CityId).IsRequired();
            // builder.Property(p => p.City).HasMaxLength(64);
        }

    }
}