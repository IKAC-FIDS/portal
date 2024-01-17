using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.IocConfig;
using TES.Web.Core.Services.Contracts;

namespace TES.Web.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UserActivityLogAttribute : ActionFilterAttribute
    {
        private class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public ShouldSerializeContractResolver(params string[] ignoreProperties)
            {
                IgnorePropertyList = ignoreProperties;
            }

            private string[] IgnorePropertyList { get; }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (IgnorePropertyList.Contains(property.PropertyName, StringComparer.OrdinalIgnoreCase))
                {
                    property.ShouldSerialize = instance => false;
                }

                return property;
            }
        }

        public UserActivityLogAttribute()
        {
            Enable = true;
            IgnoreActionParameters = null;
        }

        private bool Enable { get; }
        public string IgnoreActionParameters { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!Enable)
            {
                return;
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                var logAllUserActivity = System.Web.Configuration.WebConfigurationManager.AppSettings["LogAllUserActivity"];
                if (logAllUserActivity != null)
                {
                    if (bool.TryParse(logAllUserActivity, out var result))
                    {
                        if (!result)
                        {
                            return;
                        }
                    }
                }
            }
            
            try
            {
                var activityLogIsEnabled = System.Web.Configuration.WebConfigurationManager.AppSettings["ActivityLogIsEnabled"];
                if (activityLogIsEnabled != null)
                {
                    if (bool.TryParse(activityLogIsEnabled, out var result))
                    {
                        if (!result)
                        {
                            return;
                        }
                    }
                }

                var logService = AppObjectFactory.Container.TryGetInstance<IUserActivityLog>();

                string data = null;
                if (context.ActionParameters.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(IgnoreActionParameters))
                    {
                        var ignoreParameters = IgnoreActionParameters.Split(',');                        
                        var jsonResolver = new ShouldSerializeContractResolver(ignoreParameters);
                        var serializerSettings = new JsonSerializerSettings
                        {
                            ContractResolver = jsonResolver
                        };
                        
                        data = JsonConvert.SerializeObject(context.ActionParameters, Formatting.None, serializerSettings);
                    }
                    else
                    {
                        data = JsonConvert.SerializeObject(context.ActionParameters, Formatting.None);
                    }
                }

                var httpMethod = context.HttpContext.Request.HttpMethod;
                var url = context.HttpContext.Request.Url?.ToString() ?? string.Empty;
                var userAgent = context.RequestContext.HttpContext.Request.UserAgent;
                var action = context.RouteData.GetRequiredString("action");
                var controller = context.RouteData.GetRequiredString("controller");
                var currentUserID = context.HttpContext.User.Identity.GetUserId<long>();

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        logService.LogUserActivity($"{httpMethod}: {url}", controller, action, DateTime.Now, data, currentUserID, userAgent, context.HttpContext.Request.UserHostAddress);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            catch
            {
                // ignored
            }
        }
    }
}