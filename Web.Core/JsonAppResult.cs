using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TES.Web.Core
{
    public class JsonAppResult : ActionResult
    {
        private readonly JsonNetResult _jsonResult = new JsonNetResult();

        public JsonAppResult(bool success, object data = null)
        {
            Data = data;
            Success = success;
            AuthenticationIsRequired = false;
            Messages = new List<MessageInfo>();
        }

        public object Data { get; set; }
        public bool Success { get; set; }
        public List<MessageInfo> Messages { get; }
        public bool AuthenticationIsRequired { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            _jsonResult.Data = new
            {
                Data,
                Success,
                AuthenticationIsRequired,
                Messages = Messages.GroupBy(x => new { x.MessageType, x.Title })
                .Select(x => new { Messages = x.Select(y => y.Message), x.Key.MessageType, x.Key.Title })
                .ToList()
            };

            _jsonResult.ExecuteResult(context);
        }

        public JsonAppResult AddSuccessMessage(string message, string title = null)
        {
            Messages.Add(new MessageInfo(message, MessageType.Success, title));

            return this;
        }

        public JsonAppResult AddInfoMessage(string message, string title = null)
        {
            Messages.Add(new MessageInfo(message, MessageType.Info, title));

            return this;
        }

        public JsonAppResult AddWarningMessage(string message, string title = null)
        {
            Messages.Add(new MessageInfo(message, MessageType.Warning, title));

            return this;
        }

        public JsonAppResult AddDangerMessage(string message, string title = null,string Data = "")
        {
            Messages.Add(new MessageInfo(message, MessageType.Danger, title , Data));

            return this;
        }
    }
}