using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using TES.Security;

namespace TES.Web.Core
{
    public sealed class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public CustomAuthorizeAttribute(params DefaultRoles[] roles)
        {
            if (roles != null && roles.Any())
            {
                Roles = string.Join(",", roles);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext context)
        {
            var user = context.HttpContext.User.Identity;
            if (user != null && user.IsAuthenticated && user.GetPasswordExpirationDate() <= DateTime.Now)
            {
                if (context.HttpContext.Request.IsAjaxRequest())
                {
                    //todo: create ajax redirect
                    var result = new JsonAppResult(false) { AuthenticationIsRequired = false };
                    result.AddWarningMessage("با توجه به گذشت بیش از 3 ماه از آخرین تغییر رمز شما، لازم است نسبت به تغییر رمز عبور خود اقدام نمایید.", "عدم دسترسی");
                    context.Result = result;
                }
                else
                {
                    context.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "ChangePassword" }));
                }
            }
            else if (context.HttpContext.Request.IsAjaxRequest())
            {
                var result = new JsonAppResult(false) { AuthenticationIsRequired = true };
                result.AddWarningMessage("متاسفانه شما دسترسی لازم را ندارید.", "عدم دسترسی");
                context.Result = result;
            }
            else
            {
                base.HandleUnauthorizedRequest(context);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var hasAccess = base.AuthorizeCore(httpContext);
            if (!hasAccess)
            {
                return false;
            }

            var user = httpContext.User.Identity;
            if (user != null && user.IsAuthenticated)
            {
                if (user.GetPasswordExpirationDate() <= DateTime.Now)
                {
                    hasAccess = false;
                }
            }

            return hasAccess;
        }
    }
}