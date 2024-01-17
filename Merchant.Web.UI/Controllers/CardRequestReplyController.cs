using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize]
    public class CardRequestReplyController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public CardRequestReplyController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Create(CreateMessageReplyViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.PostedFiles.Any(x => x != null) && viewModel.PostedFiles.Any(x => !x.IsValidFile() || !x.IsValidFormat(".jpg,.jpeg,.pdf,.docx,.png") || x.ContentLength > 1 * 1024 * 1024))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var query = _dataContext.CardRequest.Where(x => x.Id == viewModel.MessageId);

            if (!User.IsAdmin() && !User.IsMessageManagerUser())
            {
                if (User.IsAcceptorsExpertUser())
                {
                    query = query.Where(x => !x.ReviewerUserId.HasValue || x.ReviewerUserId == CurrentUserId);
                }
                else
                {
                    query = query.Where(x => x.UserId == CurrentUserId);
                }
            }

            var message = await query.FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return JsonErrorMessage("تیکت مورد نظر برای شما غیر قابل دسترسی می باشد");
            }

            var messageReply = new CardRequestReply()
            {
                Body = viewModel.Body,
                UserId = CurrentUserId,
                CreationDate = DateTime.Now,
                MessageId = viewModel.MessageId
            };

            if (viewModel.PostedFiles.Any())
            {
                foreach (var item in viewModel.PostedFiles.Where(x => x != null && x.IsValidFile()))
                {
                    messageReply.MessageReplyDocuments.Add(new CardRequestReplyDocument() { FileData = item.ToByteArray(), FileName = item.FileName });
                }
            }

            _dataContext.CardRequestReplies.Add(messageReply);

            message.LastReplySeen = false;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}