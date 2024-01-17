using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler;
using EntityFramework.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class AcceptorInqueryFromPardakhNovinTask : ScheduledTaskTemplate
    {
        public override string Name => "استعلام وضعیت پایانه ها از ایرانکیش";

        public override int Order => 2;

        public override async Task RunAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var dataContext = new AppDataContext())
            {
                var successfullCount = 0;
                var faildTerminalIdList = new List<long>();

                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "استعلام وضعیت پایانه ها از ایرانکیش"
                });

                var terminalData = await dataContext.Terminals
                    .Where(x => x.PspId == (byte) Common.Enumerations.PspCompany.PardakhNovin &&
                                x.StatusId != (byte) Common.Enumerations.TerminalStatus.Deleted    &&    x.DeviceTypeId != 22  
                                && x.StatusId != (byte) Common.Enumerations.TerminalStatus.Installed
                                && x.StatusId != (byte) Common.Enumerations.TerminalStatus.Revoked
                                && !string.IsNullOrEmpty(x.TerminalNo)
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.StatusId,
                        x.TerminalNo
                    })
                    .ToListAsync();

                using (var irankishService = new PardakhtNovinService())
                {
                    foreach (var terminalInfo in terminalData)
                    {
                        //todo
                        Console.WriteLine(DateTime.Now);
                        var inqueryResult =   irankishService.RevokRequestInquery(terminalInfo.TerminalNo,
                            terminalInfo.Id, terminalInfo.StatusId,0);
                        Console.WriteLine(DateTime.Now);

                        
                    }
                }

                stopwatch.Stop();
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "پایان",
                    ExecutionTime = DateTime.Now,
                    TaskName = "استعلام وضعیت پایانه ها از ایرانکیش",
                    Message =
                        $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000}, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                });

                await dataContext.SaveChangesAsync();
            }
        }

        public override bool RunAt(DateTime utcNow)
        {
         
            var now = utcNow.ToLocalTime();
 
            //  return true;
            return (now.Hour == 12 && now.Minute == 1 && now.Second == 1)
                   || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)
                ;
        }
    }
}