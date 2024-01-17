using DNTScheduler;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TES.Data;
using TES.Data.Domain;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateBlockDocumentStatusTask : ScheduledTaskTemplate
    {
        public override string Name => "بروزرسانی وضعیت سند مسدودی پایانه ها";

        public override int Order => 3;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی وضعیت سند مسدودی پایانه ها"
                });

                var oneMonthAgo = DateTime.Today.AddMonths(1);
                var shouldUpdateTerminals = await dataContext.Terminals
                    .Where(x => x.StatusId == (byte)Enums.BlockDocumentStatus.Registered && x.BlockDocumentStatusChangedToRegistredDate >= oneMonthAgo)
                    .ToListAsync();

                shouldUpdateTerminals.ForEach(x => x.BlockDocumentStatusId = (byte)Enums.BlockDocumentStatus.Registered);

                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "پایان",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی وضعیت سند مسدودی پایانه ها",
                    Message = $"تعداد {shouldUpdateTerminals.Count} پایانه وضعیت سند مسدودی شان از ثبت شده به در انتظار پایش دوره ای تغییر پیدا کرد"
                });

                await dataContext.SaveChangesAsync();
            }
        }

        public override bool RunAt(DateTime utcNow)
        {
            // if (IsShuttingDown || Pause)
            //     return false;

            var now = utcNow.AddHours(3.5);

            return now.Hour == 3 && now.Minute == 0 && now.Second == 0; // هر شب ساعت 3 نیمه شب
        }        
    }
}