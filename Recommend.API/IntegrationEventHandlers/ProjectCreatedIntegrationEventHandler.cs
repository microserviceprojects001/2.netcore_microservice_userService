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
        public ProjectCreatedIntegrationEventHandler(RecommendDBContext dbContext, IUserService userService,
                                                     IContactService contactService)
        {
            _dbContext = dbContext;
            _userService = userService;
            _contactService = contactService;
        }

        [CapSubscribe("finbook.projectapi.projectcreated")]
        public async Task Handle(ProjectCreatedIntegrationEvent @event)
        {
            var fromUser = await _userService.GetBaseUserInfoAsync(@event.UserId);
            var contacts = await _contactService.GetUserContactsAsync(@event.UserId);
            foreach (var contact in contacts)
            {
                var recommend = new ProjectRecommend
                {

                    FromUserId = @event.UserId,
                    Company = @event.Company,
                    Tags = @event.Tags,

                    ProjectId = @event.ProjectId,
                    ProjectAvatar = @event.ProjectAvatar,
                    FinState = @event.FinStage,

                    RecommendTime = DateTime.Now,
                    CreatedTime = @event.CreatedTime,

                    Introduction = @event.Introduction,
                    RecommendType = EnumRecommendType.Friend,
                    FromUserName = fromUser.Name,
                    FromUserAvatar = fromUser.Avatar,
                    UserId = contact.UserId,
                };
                _dbContext.ProjectRecommends.Add(recommend);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}