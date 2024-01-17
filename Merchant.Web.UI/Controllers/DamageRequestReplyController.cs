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
    public class DamageRequestReplayController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public DamageRequestReplayController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        [AjaxOnly]
     
        public async Task<ActionResult> Create(CreateMessageReplyViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.PostedFiles.Any(x => x != null) && viewModel.PostedFiles.Any(x => !x.IsValidFile() || !x.IsValidFormat(".jpg,.jpeg,.pdf,.docx,.png") || x.ContentLength > 1 * 1024 * 1024))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var query = _dataContext.DamageRequest.Where(x => x.Id == viewModel.MessageId);

            if ( User.IsBranchUser())
            {
                 
                    query = query.Where(x =>  x.OrganizationUnitId == CurrentUserBranchId);
                
            }

            var message = await query.FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return JsonErrorMessage("درخواست مورد نظر برای شما غیر قابل دسترسی می باشد");
            }

            var messageReply = new DamageRequestReply()
            {
                Body = viewModel.Body,
                UserId = CurrentUserId,
                CreationDate = DateTime.Now,
                DamageRequestId = viewModel.MessageId
            };

            if (viewModel.PostedFiles.Any())
            {
                foreach (var item in viewModel.PostedFiles.Where(x => x != null && x.IsValidFile()))
                {
                    messageReply.MessageReplyDocuments.Add(new DamageRequestReplyDocument() { DamageRequestReplyId = messageReply.Id, FileData = item.ToByteArray(), FileName = item.FileName });
                }
            }

            _dataContext.DamageRequestReply.Add(messageReply);

            message.LastReplySeen = false;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}