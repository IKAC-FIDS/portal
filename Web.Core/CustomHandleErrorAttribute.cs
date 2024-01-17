using Microsoft.AspNet.Identity;
using StackExchange.Exceptional;
using System.Web;
using System.Web.Mvc;

namespace TES.Web.Core
{
    public class CustomHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
            {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500)
            {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception))
            {
                return;
            }

            if (!filterContext.HttpContext.Request.Browser.Crawler)
            {
                filterContext.Exception.AddLogData("User Id", filterContext.HttpContext.User.Identity.GetUserId());
                filterContext.Exception.Log(filterContext.HttpContext.ApplicationInstance.Context);

                // if the request is AJAX return JSON else view.
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonAppResult(false).AddDangerMessage("خطا در اجرای درخواست شما", filterContext.Exception.Message,filterContext.Exception.StackTrace);
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = View,
                        MasterName = Master,
                        TempData = filterContext.Controller.TempData
                    };
                }
            }

            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}