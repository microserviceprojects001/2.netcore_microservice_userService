using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Queries
{
    public interface IProjectQueries
    {
        Task<dynamic> GetProjectByUserIdAsync(int userId);

        Task<dynamic> GetProjectDetailAsync(int userId, int projectId);
    }
}