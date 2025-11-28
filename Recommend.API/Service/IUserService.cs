using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Recommend.API.Dtos;

namespace Recommend.API.Service;

public interface IUserService
{
    /// <summary>
    /// 获取用户的基本信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<UserIdentity> GetBaseUserInfoAsync(int userId);
}