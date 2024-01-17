using System.Linq;
using StackExchange.Exceptional;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class ErrorController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public ErrorController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [CustomAuthorize(DefaultRoles.Administrator)]
        public Task Manage() => ExceptionalModule.HandleRequestAsync(System.Web.HttpContext.Current);

        public ActionResult NotFound()
        {
            
            return Request.IsAjaxRequest() ? (ActionResult) PartialView("_NotFound") : View();
        }

        public ActionResult AccessDenied()
        {
            
            return View();
        }

        public ActionResult ServerError()
        {
            
            return View();
        }

        public ActionResult Disable()
        {
        

            return View();
        }

        public ActionResult CustomError(string message)
        {
            var messafge = _dataContext.Messages.ToList();
            ViewBag.OpenMessage = messafge.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
                                                      && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                          || User.IsMessageManagerUser()));
            return Request.IsAjaxRequest() ? (ActionResult) PartialView("_CustomError", message) : View(message);
        }
    }
}