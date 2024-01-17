using System.Web.Mvc;
using Portal.IService;
using Portal.Service;
using Unity;
using Unity.Mvc5;

namespace App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();
            
            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();
            
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            container.RegisterType<IBasicService, BasicService>();     
            container.RegisterType<ICustomerService, CustomerService>();     
        }
    }
}