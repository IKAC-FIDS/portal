using System;
using System.Threading.Tasks;

namespace TES.Web.Core.Services.Contracts
{
    public interface IUserActivityLog
    {
        Task LogUserActivity(
            string address,
            string category,
            string name,
            DateTime activityTime,
            string data,
            long? userId,
            string userAgent,
            string userIp);
    }
}