using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq; 
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using Serilog;
namespace TES.Web.Core
{
    public class BaseController : Controller
    {
        public long CurrentUserId => User.Identity.GetUserId<long>();
        public long? CurrentUserBranchId => User.Identity.GetBranchId();

        public JsonAppResult JsonSuccessResult(object data = null)
        {
            return new JsonAppResult(true, data);
        }

        public JsonAppResult JsonUnsuccessResult(object data = null)
        {
            return new JsonAppResult(false, data);
        }

        public JsonAppResult JsonSuccessMessage(string successMessage = null, object data = null)
        {
            var result = new JsonAppResult(true, data);
            result.AddSuccessMessage(string.IsNullOrWhiteSpace(successMessage)
                ? "عملیات با موفقیت انجام شد."
                : successMessage);
            return result;
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            AppDataContext _dataContext2 = new AppDataContext();
            var message = _dataContext2.Messages.ToList();
            ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) TES.Common.Enumerations.MessageStatus.Open
                                                     && (d.UserId == User.Identity.GetUserId<long>() ||
                                                         d.ReviewerUserId == User.Identity.GetUserId<long>()
                                                         || User.IsMessageManagerUser()));
            ViewBag.InProgressMessage = message.Count(d =>
                d.StatusId == (int) TES.Common.Enumerations.MessageStatus.UnderReview
                && (d.UserId == User.Identity.GetUserId<long>() || d.ReviewerUserId == User.Identity.GetUserId<long>()
                                                                || User.IsMessageManagerUser()));
            var cardmessage = _dataContext2.CardRequest.ToList();
            ViewBag.ReadyForDeliverCardRequst = cardmessage.Count(d =>
                d.StatusId == (int) Common.Enumerations.CardRequestStatus.ReadyForDeliver
                && (d.UserId == User.Identity.GetUserId<long>() || d.ReviewerUserId == User.Identity.GetUserId<long>()
                                                                || User.IsCardRequestManager()));
            ViewBag.InProgressCardRequstMessage = cardmessage.Count(d =>
                d.StatusId == (int) Common.Enumerations.CardRequestStatus.UnderReview
                && (d.UserId == User.Identity.GetUserId<long>() || d.ReviewerUserId == User.Identity.GetUserId<long>()
                                                                || User.IsCardRequestManager()));
            ViewBag.OpenCardRequstMessage = cardmessage.Count(d =>
                d.StatusId == (int) Common.Enumerations.CardRequestStatus.Open
                && (d.UserId == User.Identity.GetUserId<long>() || d.ReviewerUserId == User.Identity.GetUserId<long>()
                                                                || User.IsCardRequestManager()));
            ViewBag.UserId = User.Identity.GetUserId<long>();
            base.OnActionExecuted(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
           var  m_exePath = Server.MapPath("~/logs/myapp.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(m_exePath, rollingInterval: RollingInterval.Minute)
                .CreateLogger();
            Log.Error(  filterContext.Exception.Message + " \n " + filterContext.Exception.StackTrace);  
            var msg = filterContext.Exception.Message;
            
        }

        public JsonAppResult JsonErrorMessage(string errorMessage = null, object data = null)
        {
            var result = new JsonAppResult(false, data);
            result.AddDangerMessage(string.IsNullOrWhiteSpace(errorMessage) ? "" : errorMessage);

            return result;
        }

        public JsonAppResult JsonErrorMessage(IEnumerable<string> errorMessages, string title = null,
            object data = null)
        {
            var result = new JsonAppResult(false, data);
            var messages = errorMessages as string[] ?? errorMessages.ToArray();

            if (messages.Any())
            {
                foreach (var message in messages)
                {
                    result.AddDangerMessage(message, title);
                }
            }

            return result;
        }

        public JsonAppResult JsonWarningMessage(IList<string> warningMessages, string title = null, object data = null)
        {
            var result = new JsonAppResult(false, data);

            if (warningMessages != null && warningMessages.Any())
            {
                foreach (var message in warningMessages)
                {
                    result.AddWarningMessage(message, title);
                }
            }

            return result;
        }

        public JsonAppResult JsonWarningMessage(string warningMessage, string title = null, object data = null)
        {
            var result = new JsonAppResult(false, data);
            if (!string.IsNullOrWhiteSpace(warningMessage))
            {
                result.AddWarningMessage(warningMessage, title);
            }

            return result;
        }

        public JsonAppResult JsonInfoMessage(string infoMessage, string title = null, object data = null)
        {
            var result = new JsonAppResult(false, data);
            if (!string.IsNullOrWhiteSpace(infoMessage))
            {
                result.AddInfoMessage(infoMessage, title);
            }

            return result;
        }

        public JsonAppResult JsonSuccessMessage(MessageType messageType, string message = null, object data = null)
        {
            var result = new JsonAppResult(true, data);
            var messageResult = string.IsNullOrWhiteSpace(message) ? "عملیات انجام شد." : message;

            switch (messageType)
            {
                case MessageType.Success:
                    result.AddSuccessMessage(messageResult);
                    break;
                case MessageType.Info:
                    result.AddInfoMessage(messageResult);
                    break;
                case MessageType.Warning:
                    result.AddWarningMessage(messageResult);
                    break;
                case MessageType.Danger:
                    result.AddDangerMessage(messageResult);
                    break;
            }

            return result;
        }

        public JsonNetResult JsonNetResult(object data)
        {
            return new JsonNetResult {Data = data};
        }

        public ActionResult MessageView(string message)
        {
            return RedirectToAction("CustomError", "Error", new {message});
        }

        // TempData Messages
        public void AddSuccessMessage(string message = null)
        {
            AddMessage(new MessageInfo(message ?? "عملیات با موفقیت انجام شد.", MessageType.Success));
        }

        public void AddInformationMessage(string message)
        {
            AddMessage(new MessageInfo(message, MessageType.Info));
        }

        public void AddWarningMessage(string message)
        {
            AddMessage(new MessageInfo(message, MessageType.Warning));
        }

        public void AddDangerMessage(string message)
        {
            AddMessage(new MessageInfo(message, MessageType.Danger));
        }

        private void AddMessage(MessageInfo message)
        {
            var val = TempData["__Messages"] as IList<MessageInfo>;
            if (val == null)
            {
                val = new List<MessageInfo>();
                TempData["__Messages"] = val;
            }

            val.Add(message);
        }
    }
}