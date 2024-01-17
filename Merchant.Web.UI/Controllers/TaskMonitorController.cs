 
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using DNTScheduler;
using TES.Common.Extensions;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TaskMonitorController : BaseController
    {
             private readonly AppDataContext _dataContext;

        public TaskMonitorController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            
            return View();
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(BlockDocumentSearchViewModel viewModel, bool retriveTotalPageCount,
            int page, CancellationToken cancellationToken)
        {

            var q = ScheduledTasksCoordinator.Current.ScheduledTasks.Select(x => new
            {
                TaskName = x.Name,
                LastRunTime = x.LastRun,
                LastRunWasSuccessful = x.IsLastRunSuccessful,
                IsPaused = x.Pause,
                IsShutDown = x.IsShuttingDown,
                IsRunning = x.IsRunning,
               
            }).ToList(); 
            var totalRowsCount = 0; 
            return JsonSuccessResult(new {q, q.Count});
             
        }

    }
}