using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using TES.Data.Domain;

namespace TES.Security
{
    public class AppClaimsIdentityFactory : ClaimsIdentityFactory<User, long>
    {
        public override async Task<ClaimsIdentity> CreateAsync(UserManager<User, long> manager, User user, string authenticationType)
        {
            var identity = await base.CreateAsync(manager, user, authenticationType);
            identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));
            identity.AddClaim(new Claim("BranchId", user.OrganizationUnitId?.ToString() ?? string.Empty));
            identity.AddClaim(new Claim("BranchTitle", user.OrganizationUnitId.HasValue ? user.OrganizationUnit.Title : string.Empty));
            identity.AddClaim(new Claim("PasswordExpirationDate", user.PasswordExpirationDate.ToString(CultureInfo.InvariantCulture)));

            return identity;
        }
    }
}