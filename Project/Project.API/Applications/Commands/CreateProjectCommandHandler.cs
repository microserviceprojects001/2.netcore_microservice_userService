using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Project.Domain.AggregatesModel.Project>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<Project.Domain.AggregatesModel.Project> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            // Implementation for creating an order goes here.
            await _projectRepository.AddAsync(request.Project);
            await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return request.Project;
        }
    }
}