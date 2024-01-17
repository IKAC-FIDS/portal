using System;
using DNTScheduler;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Controllers;

namespace TES.Merchant.Web.UI.WebTasks
{
    public static class ScheduledTasksRegistry
    {
        public static void Init()
        {
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new InqueryFromPardakhtNovinTask());
          ScheduledTasksCoordinator.Current.AddScheduledTasks(new InqueryFromNewIrankishTask());
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new AcceptorInqueryFromPardakhNovinTask());
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new InqueryFromPardakhNovinForEditRequstTask());

            
            
      // ScheduledTasksCoordinator.Current.AddScheduledTasks(new InqueryFromIrankishTask());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new AcceptorInqueryFromIrankishTask());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new AcceptorInqueryFromFanavaTask());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new AcceptorInqueryFromParsianTask());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateParsianInfoTask());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateTerminalsWithRevokeRequest());
        //    ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateTerminalsWithChangeAccountRequestForNewIranKish());
            ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateTerminalsWithChangeAccountRequest());
            
            
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateBlockBranchPermissionTask());
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateInstalledTerminals());
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateTerminalForChangeAccountRequestWithBadResult());
            // ScheduledTasksCoordinator.Current.AddScheduledTasks(new UpdateInstalledAndRevokedTerimal());


            ScheduledTasksCoordinator.Current.OnUnexpectedException = (exception, scheduledTask) =>
            {
                //todo: log the exception.
                AppDataContext dataContext = new AppDataContext();
                TaskError a = new TaskError();
                a.TaskName = scheduledTask.Name;
                a.Exception = exception.Message;
                a.Date = DateTime.Now;
                a.HelpLink = exception.HelpLink;
                a.Source = exception.Source;
                a.StackTrace = exception.StackTrace;
                dataContext.TaskErrors.Add(a);
                dataContext.SaveChanges();
                // System.Diagnostics.Trace.WriteLine(scheduledTask.Name + ":" + exception.Message);
            };

            ScheduledTasksCoordinator.Current.Start();
        }

        public static void End()
        {
            ScheduledTasksCoordinator.Current.Dispose();
        }
    }
}