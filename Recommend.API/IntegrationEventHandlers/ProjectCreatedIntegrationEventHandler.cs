using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Recommend.API.IntegrationEvents;
using Recommend.API.Data;
using Recommend.API.Models;

namespace Recommend.API.IntegrationEventHandlers
{

    public class ProjectCreatedIntegrationEventHandler : ICapSubscribe
    {
        private readonly RecommendDBContext _dbContext;
        public ProjectCreatedIntegrationEventHandler(RecommendDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [CapSubscribe("finbook.projectapi.projectcreated")]
        public Task Handle(ProjectCreatedIntegrationEvent @event)
        {
            // Handle the ProjectCreatedIntegrationEvent here
            return Task.CompletedTask;
        }
    }
}