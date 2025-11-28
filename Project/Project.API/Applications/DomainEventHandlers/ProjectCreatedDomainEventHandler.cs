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
                UserId = notification.project.UserId,
                CreatedTime = DateTime.Now,
                ProjectId = notification.project.Id,
                Company = notification.project.Company,
                Introduction = notification.project.Introduction,
                FinStage = notification.project.FinStage,
                ProjectAvatar = notification.project.Avatar,
                Tags = notification.project.Tags,
            };
            _capBus.PublishAsync("finbook.projectapi.projectcreated", @event);
            // Handle the ProjectCreatedEvent here
            return Task.CompletedTask;
        }
    }
}