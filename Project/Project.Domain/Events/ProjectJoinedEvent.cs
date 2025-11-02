using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.Domain.Events
{
    public class ProjectJoinedEvent : INotification
    {
        public ProjectContributor contributor { get; set; }
    }
}