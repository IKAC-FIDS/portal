using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class AutomatedTaskLogController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public AutomatedTaskLogController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Manage()
        {
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
           //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                              || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview 
           //                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                    || User.IsMessageManagerUser()));
           //  
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetData(GetAutomatedTaskLogDataViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.AutomatedTaskLogs.AsQueryable();
                
            var totalRowsCount = 0;

            if (viewModel.RetriveTotalPageCount)
            {
                totalRowsCount = await query.CountAsync(cancellationToken);
            }

            var result = await query
                .OrderByDescending(x => x.Id)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .Select(x => new
                {
                    x.Id,
                    x.Message,
                    x.TaskName,
                    x.ExecutionTime,
                    x.ActivityTitle
                })
                .ToListAsync(cancellationToken);

            var rows = result.Select(x => new
            {
                x.Id,
                x.Message,
                x.TaskName,
                x.ActivityTitle,
                ExecutionTime = x.ExecutionTime.ToPersianDateTime(),
                RelativeExecutionTime = x.ExecutionTime.ToRelativeDate()
            })
            .ToList();

            return JsonSuccessResult(new { rows, totalRowsCount });
        }
    }
}