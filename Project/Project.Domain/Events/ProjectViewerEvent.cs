using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.Domain.Events
{
    public class ProjectViewerEvent : INotification
    {
        public ProjectViewer viewer { get; set; }
    }
}