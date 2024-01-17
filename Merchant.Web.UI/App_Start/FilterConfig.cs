using System;
using System.Web.Mvc;
using TES.Web.Core;

namespace TES.Merchant.Web.UI
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CustomHandleErrorAttribute());
            //filters.Add(new BlockingFilterAttribute());
            filters.Add(new UserActivityLogAttribute());
        }
    }

    public class BlockingFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var validDate = new DateTime(2018, 8, 14);
            if (DateTime.Now > validDate)
            {
                filterContext.ExceptionHandled = true;

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var result = new JsonAppResult(false) { AuthenticationIsRequired = false };
                    result.AddWarningMessage("متاسفانه خطایی در سامانه رخ داده است.", "خطا");
                    filterContext.Result = result;
                }
                else
                {
                    filterContext.Result = new ViewResult()
                    {
                        ViewName = "Error"
                    };
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}