using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries
    {
        public Task<dynamic> GetProjectByUserIdAsync(int userId)
        {
            // 实现获取用户项目列表的逻辑
            throw new NotImplementedException();
        }

        public Task<dynamic> GetProjectDetailAsync(int userId, int projectId)
        {
            // 实现获取项目详情的逻辑
            throw new NotImplementedException();
        }
    }
}