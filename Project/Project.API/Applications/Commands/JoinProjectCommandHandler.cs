using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommandHandler : IRequestHandler<JoinProjectCommand, bool>
    {
        private readonly IProjectRepository _projectRepository;

        public JoinProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> Handle(JoinProjectCommand request, CancellationToken cancellationToken)
        {
            // Implementation for creating an order goes here.
            var project = await _projectRepository.GetAsync(request.ProjectContributor.ProjectId);
            if (project == null)
            {
                throw new Project.Domain.Exceptions.ProjectDomainException($"Project with id {request.ProjectContributor.ProjectId} not found.");
            }
            project.AddContributor(request.ProjectContributor);

            await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}