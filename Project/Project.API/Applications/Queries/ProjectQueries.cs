using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries
    {
        private readonly string _connectionString;
        public ProjectQueries(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<dynamic> GetProjectByUserIdAsync(int userId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var sql = @"select
                            Projects.Id,
                            Projects.Avatar,
                            Projects.Company,
                            Projects.FinStage,
                            Projects.Introduction,
                            Projects.ShowSecurityInfo,
                            Projects.CreateTime 
                            FROM Projects 
                            where Projects.UserId = @userId";
            var result = await connection.QueryAsync<dynamic>(sql, new { userId });
            return result;
        }

        public async Task<dynamic> GetProjectDetailAsync(int projectId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();
            var sql = @"select
                            Projects.Company,
                            Projects.City,
                            Projects.AreaName,
                            Projects.Provice,
                            Projects.FinStage,
                            Projects.FinMoney,
                            Projects.Valuation,
                            Projects.FinPercentage,
                            Projects.Introduction,
                            Projects.UserId,
                            Projects.Income,
                            Projects.Revenue,
                            Projects.UserName,
                            Projects.Avatar,
                            Projects.BrokerageOptions,
                            ProjectVisibleRules.Tags,
                            ProjectVisibleRules.Visible
                            FROM Projects INNER JOIN ProjectVisibleRules 
                            on Projects.Id = ProjectVisibleRules.ProjectId
                            where Projects.Id = @projectId";
            var result = connection.QueryAsync<dynamic>(sql, new { projectId });
            return result;
        }
    }
}