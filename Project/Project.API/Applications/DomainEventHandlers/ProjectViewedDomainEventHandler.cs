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
    public class ProjectViewedDomainEventHandler : INotificationHandler<ProjectViewerEvent>
    {
        private readonly ICapPublisher _capBus;
        public ProjectViewedDomainEventHandler(ICapPublisher capBus)
        {
            _capBus = capBus;
        }

        public Task Handle(ProjectViewerEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectViewedIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Avatar = notification.Avatar,
                Viewer = notification.viewer
            };
            _capBus.PublishAsync("finbook.projectapi.projectviewed", @event);
            // Handle the ProjectViewerEvent here
            return Task.CompletedTask;
        }
    }
}