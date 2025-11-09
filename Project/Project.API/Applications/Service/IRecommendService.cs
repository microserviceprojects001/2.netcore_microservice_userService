using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Service
{

    /// <summary>
    /// 通过调用推荐服务，判断某个项目是否在推荐列表中，先用假的接口，夸微服务调用是通过服务注册和发现来完成的
    /// </summary>
    public interface IRecommendService
    {
        Task<bool> IsProjectInRecommendAsync(int projectId, int userId);
    }
}