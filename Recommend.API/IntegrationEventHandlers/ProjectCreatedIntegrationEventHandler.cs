using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Recommend.API.IntegrationEvents;
using Recommend.API.Data;
using Recommend.API.Models;
using Recommend.API.Service;
using Recommend.API.Dtos;

namespace Recommend.API.IntegrationEventHandlers
{


    public class ProjectCreatedIntegrationEventHandler : ICapSubscribe
    {
        private readonly IUserService _userService;
        private readonly IContactService _contactService;
        private readonly RecommendDBContext _dbContext;
        private readonly ILogger<ProjectCreatedIntegrationEventHandler> _logger;
        public ProjectCreatedIntegrationEventHandler(
                RecommendDBContext dbContext,
                IUserService userService,
                ILogger<ProjectCreatedIntegrationEventHandler> logger,
                IContactService contactService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _contactService = contactService;
            _logger = logger;
        }

        [CapSubscribe("finbook.projectapi.projectcreated")]
        public async Task Handle(ProjectCreatedIntegrationEvent @event)
        {
            try
            {
                var fromUser = await _userService.GetBaseUserInfoAsync(@event.UserId);
                var contacts = await _contactService.GetUserContactsAsync(@event.UserId);
                foreach (var contact in contacts)
                {
                    var recommend = new ProjectRecommend
                    {

                        FromUserId = @event.UserId,
                        Company = @event.Company,
                        Tags = @event.Tags ?? string.Empty, // 处理可能的空值

                        ProjectId = @event.ProjectId,
                        ProjectAvatar = @event.ProjectAvatar ?? string.Empty,
                        FinState = @event.FinStage ?? string.Empty, // 处理可能的空值

                        RecommendTime = DateTime.Now,
                        CreatedTime = @event.CreatedTime,

                        Introduction = @event.Introduction ?? string.Empty,
                        RecommendType = EnumRecommendType.Friend,
                        FromUserName = fromUser.Name ?? string.Empty,
                        FromUserAvatar = fromUser.Avatar ?? string.Empty,
                        UserId = contact.UserId
                    };
                    _dbContext.ProjectRecommends.Add(recommend);
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"An error occurred, when recommend service handle projectcreated event: {ex.ToString()}");
            }

        }
    }
}