using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;


namespace Project.Domain.Events
{
    public class ProjectCreatedEvent : INotification
    {
        public Project.Domain.AggregatesModel.Project project { get; set; }
    }
}