using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recommend.API.Dtos;
namespace Recommend.API.Service
{
    public interface IContactService
    {
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Contact>> GetUserContactsAsync(int userId);
    }
}