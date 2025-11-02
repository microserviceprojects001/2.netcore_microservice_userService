using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Domain.AggregatesModel;
using MediatR;

namespace Project.API.Applications.Commands
{
    public class ViewProjectCommandHandler : IRequestHandler<ViewProjectCommand, bool>
    {
        private readonly IProjectRepository _projectRepository;

        public ViewProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task<bool> Handle(ViewProjectCommand request, CancellationToken cancellationToken)
        {
            // Implementation for creating an order goes here.
            var project = await _projectRepository.GetAsync(request.ProjectId);
            if (project == null)
            {
                throw new Project.Domain.Exceptions.ProjectDomainException($"Project with id {request.ProjectId} not found.");
            }
            project.AddViewer(request.UserId, request.UserName, request.Avatar);

            await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}