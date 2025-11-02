using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Project.Domain.AggregatesModel;
using MediatR;

namespace Project.API.Applications.Commands
{
    public class CreateProjectCommand : IRequest<Project.Domain.AggregatesModel.Project>
    {
        public Project.Domain.AggregatesModel.Project Project { get; set; }
    }
}