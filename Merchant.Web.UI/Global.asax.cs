using StructureMap.Web;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Security.Claims;
using System.Web.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using TES.Data;
using TES.Data.Interceptor;
using TES.Web.Core;
using TES.Web.Core.Services.Contracts;
using TES.Web.Core.Services.Implementation;
using TES.Merchant.Web.UI.WebTasks;
using StackExchange.Exceptional;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            DbInterception.Add(new YeKeInterceptor());

            MvcHandler.DisableMvcResponseHeader = true;
            AntiForgeryConfig.CookieName = "f";
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new CSharpRazorViewEngine());
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(IocConfig.AppObjectFactory.Container));
            IocConfig.AppObjectFactory.Container.Configure(x =>
            {
                x.For<IUserActivityLog>().HybridHttpOrThreadLocalScoped().Use(() => new SqlUserActivityLog(WebConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString));
                x.For<AppDataContext>().HybridHttpOrThreadLocalScoped().Use<AppDataContext>();
            });

            ModelBinders.Binders.Add(typeof(DateTime), new PersianDateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new PersianDateTimeModelBinder());

            Exceptional.Configure(setting =>
            {
                setting.LogFilters.Form.Add("password", "******");
                setting.Store.Type = "SQL";
                setting.Store.ApplicationName = "TES";
                setting.Store.TableName = "log.Error";
                setting.Store.ConnectionString = WebConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString;
            });

            //Database.SetInitializer(new DropCreateDatabaseAlways<AppDataContext>());
            //using (var context = new AppDataContext())
            //{
            //    context.Database.Initialize(force: true);
            //}

            Database.SetInitializer<AppDataContext>(null);
 
            ScheduledTasksRegistry.Init();
 
        }
    }
}