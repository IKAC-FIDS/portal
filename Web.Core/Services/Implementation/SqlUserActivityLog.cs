using Dapper;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TES.Web.Core.Services.Contracts;

namespace TES.Web.Core.Services.Implementation
{
    public class SqlUserActivityLog : IUserActivityLog
    {
        private readonly string _connectionString;

        public SqlUserActivityLog(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task LogUserActivity(string address, string category, string name, DateTime activityTime, string data, long? userId, string userAgent, string userIP)
        {
            var parameters = new
            {
                Address = address,
                Category = category,
                Name = name,
                activityTime,
                Data = data,
                userId,
                UserAgent = userAgent,
                UserIP = userIP
            };

            var sqlCmd = @"INSERT INTO log.UserActivityLog (Address, Category, Name, ActivityTime, Data, UserId, UserAgent, UserIP) VALUES (@Address, @Category, @Name, @ActivityTime, @Data, @UserId, @UserAgent, @UserIP);";
            using (var cnn = new SqlConnection(_connectionString))
            {
                await cnn.ExecuteAsync(sqlCmd, parameters);
            }
        }
    }
}
