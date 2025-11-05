using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Domain.SeedWork;
using Project.Domain.AggregatesModel;
using Microsoft.EntityFrameworkCore;

using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repositories
{

    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectContext _context;
        public IUnitOfWork UnitOfWork => _context;

        public ProjectRepository(ProjectContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectEntity> AddAsync(ProjectEntity project)
        {
            if (project.IsTransient())
            {
                return (await _context.Projects.AddAsync(project)).Entity;
            }
            else
            {
                return project;
            }
        }

        public async Task<ProjectEntity> GetAsync(int projectId)
        {
            var project = await _context.Projects.Include(p => p.Properties)
                              .Include(p => p.Viewers)
                              .Include(x => x.Contributors)
                              .Include(x => x.VisibleRule)
                              .FirstOrDefaultAsync(p => p.Id == projectId);
            return project;
        }
        // 方法签名：public Task<ProjectEntity> UpdateAsync(...) - 期望返回 Task<ProjectEntity>
        // 实际返回：_context.Update(project).Entity - 返回的是
        // ProjectEntity 实体类型不匹配：
        // ProjectEntity
        // 不能隐式转换为 Task<ProjectEntity>

        //加上 async 关键字后就可以的原因是因为 async 方法会自动将返回值包装成 Task。
        //手动包装的等价写法
        // public Task<ProjectEntity> UpdateAsync(ProjectEntity project)
        // {
        //     _context.Update(project);
        //     return Task.FromResult(project); // 手动包装
        // }

        public async Task<ProjectEntity> UpdateAsync(ProjectEntity project)
        {
            return _context.Update(project).Entity;
            //  return Task.FromResult(project);
        }
    }
}