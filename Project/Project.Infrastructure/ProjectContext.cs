using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Domain.SeedWork;
using MediatR;

namespace Project.Infrastructure
{
    public class ProjectContext : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;

        public DbSet<Domain.AggregatesModel.Project> Projects { get; set; }

        // 只保留这一个构造函数
        public ProjectContext(DbContextOptions<ProjectContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new EntityConfiguration.ProjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfiguration.ProjectVisibleRuleEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfiguration.ProjectViewerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfiguration.ProjectContributorEntityConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfiguration.ProjectPropertyEntityConfiguration());


            // 配置实体映射
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this); // 如果领域事件发生异常，下面的数据库操作也不会执行了
            await SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}