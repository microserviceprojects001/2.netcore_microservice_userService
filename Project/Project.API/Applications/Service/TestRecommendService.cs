using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Service
{
    public class TestRecommendService : IRecommendService
    {
        public Task<bool> IsProjectInRecommendAsync(int projectId, int userId)
        {
            // 这里可以添加实际的推荐逻辑
            return Task.FromResult(true);
        }
    }
}