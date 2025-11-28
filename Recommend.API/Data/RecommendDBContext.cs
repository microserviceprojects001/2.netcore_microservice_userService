using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Recommend.API.Models;

namespace Recommend.API.Data
{
    public class RecommendDBContext : DbContext
    {
        public RecommendDBContext(DbContextOptions<RecommendDBContext> options) : base(options)
        {
        }

        public DbSet<ProjectRecommend> ProjectRecommends { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectRecommend>().ToTable("ProjectRecommends")
                .HasKey(pr => pr.Id);


            base.OnModelCreating(modelBuilder);
            // Configure your entity mappings here
        }

        // Define DbSets for your entities here
    }
}