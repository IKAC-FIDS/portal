using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class UserActivityLogController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public UserActivityLogController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult Index(long userId)
        {
            ViewBag.UserId = userId;
            return PartialView("_Index");
        }

        [AjaxOnly]
        public async Task<ActionResult> GetData(GetUserActivityLogDataViewModel viewModel)
        {
            var query = _dataContext.UserActivityLogs.Where(x => x.UserId == viewModel.UserId);

            if (viewModel.FromDate.HasValue)
                query = query.Where(x => x.ActivityTime >= viewModel.FromDate);

            if (viewModel.ToDate.HasValue)
                query = query.Where(x => x.ActivityTime <= viewModel.ToDate);

            var totalRowsCount = 0;

            if (viewModel.RetriveTotalPageCount)
                totalRowsCount = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.ActivityTime)
                .Skip((viewModel.Page - 1) * 10)
                .Take(10)
                .Select(x => new
                {
                    x.Data,
                    x.Name,
                    x.UserIP,
                    x.Address,
                    x.Category,
                    x.UserAgent,
                    x.ActivityTime,
                })
                .ToListAsync();

            var rows = data
                .Select(x => new
                {
                    x.Data,
                    x.Name,
                    x.UserIP,
                    x.Address,
                    x.Category,
                    x.UserAgent,
                    ActivityTime = x.ActivityTime.ToPersianDateTime()
                })
                .ToList();

            return JsonSuccessResult(new { rows, totalRowsCount });
        }
    }
}