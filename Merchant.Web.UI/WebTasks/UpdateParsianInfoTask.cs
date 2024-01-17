using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler;
using StackExchange.Exceptional;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateParsianInfoTask : ScheduledTaskTemplate
    {
        public override string Name => "استعلام وضعیت درخواست تغییر اطلاعات هویتی پارسیان";
        public override int Order => 3;
        public override async Task RunAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var dataContext = new AppDataContext())
            using (var parsianService = new ParsianService())
            {
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "استعلام وضعیت پایانه ها از پارسیان"
                });
            
                var terminalData = await dataContext.ParsianRequestForInfo
                    .Where(x => 
                                 
                        x.StatusId == 3
                                                                               
                                                                             
                    )
                                
                    .ToListAsync();
            
            
                var successfullCount = 0;
                var faildTerminalIdList = new List<long>();
            
                foreach (var terminalInfo in terminalData)
                {
                    try
                    { 
                        var result =
                            await parsianService.ChangeInfoInquery(terminalInfo.TopiarId.ToString(),
                                (int) terminalInfo.Id);
                        if (!string.IsNullOrEmpty(result.Error) && result.IsComplete)
                        {
                            terminalInfo.StatusId = 4;
                        }
                        else if( !string.IsNullOrEmpty(result.Error))
                        {
                            terminalInfo.StatusId = 2;
                            terminalInfo.Error = result.Error;
                        }

                        dataContext.SaveChanges();
                        successfullCount++;
                    }
                    catch (Exception exception)
                    {
                        faildTerminalIdList.Add(terminalInfo.Id);
                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            
                stopwatch.Stop();
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "پایان",
                    ExecutionTime = DateTime.Now,
                    TaskName = "استعلام وضعیت پایانه ها از پارسیان",
                    Message =
                        $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000}, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                });
            
                var e = dataContext.SaveChangesAsync().Result;
            }
        }
        public override bool RunAt(DateTime utcNow)
        {
            var now = utcNow.ToLocalTime();


            return (now.Hour == 12 && now.Minute == 1 && now.Second == 1)
                   || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)
                ;
        }
    }
}