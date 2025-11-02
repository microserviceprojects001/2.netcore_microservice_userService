using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Domain.SeedWork;
using Project.Domain.AggregatesModel;
using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        public IUnitOfWork UnitOfWork => throw new NotImplementedException();

        public Task<ProjectEntity> AddAsync(ProjectEntity project)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectEntity> GetAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public Task<ProjectEntity> UpdateAsync(ProjectEntity project)
        {
            throw new NotImplementedException();
        }
    }
}