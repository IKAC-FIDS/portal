using Microsoft.AspNet.Identity.EntityFramework;
using TES.Data;
using TES.Data.Domain;

namespace TES.Security
{
    public class CustomUserStore : UserStore<User, Role, long, UserLogin, UserRole, UserClaim>
    {
        public CustomUserStore(AppDataContext context)
            : base(context)
        {

        }
    }
}