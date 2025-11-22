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
    public class ProjectJoinedDomainEventHandler : INotificationHandler<ProjectJoinedEvent>
    {
        private readonly ICapPublisher _capBus;
        public ProjectJoinedDomainEventHandler(ICapPublisher capBus)
        {
            _capBus = capBus;
        }
        public Task Handle(ProjectJoinedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectJoinIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Avatar = notification.Avatar,
                Contributor = notification.contributor
            };
            _capBus.PublishAsync("finbook.projectapi.projectjoined", @event);
            // Handle the ProjectJoinedEvent here
            return Task.CompletedTask;
        }
    }
}