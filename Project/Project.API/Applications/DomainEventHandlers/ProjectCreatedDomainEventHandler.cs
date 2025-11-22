using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Project.Domain.Events;
using MediatR;
using DotNetCore.CAP;
using Project.API.Applications.IntegrationEvents;

namespace Project.API.Applications.DomainEventHandlers
{


    public class ProjectCreatedDomainEventHandler : INotificationHandler<ProjectCreatedEvent>
    {
        private readonly ICapPublisher _capBus;
        public ProjectCreatedDomainEventHandler(ICapPublisher capBus)
        {
            _capBus = capBus;
        }
        public Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectCreatedIntegrationEvent
            {
                ProjectId = notification.project.Id,
                UserId = notification.project.UserId,
                CreatedTime = DateTime.UtcNow
            };
            _capBus.PublishAsync("finbook.projectapi.projectcreated", @event);
            // Handle the ProjectCreatedEvent here
            return Task.CompletedTask;
        }
    }
}