using DNTScheduler;
using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateTerminalsWithChangeAccountRequestForNewIranKish : ScheduledTaskTemplate
    {
        public override string Name => "استعلام وضعیت پایانه هایی که درخواست تغییر حساب داشته اند";

        public override int Order => 3;

        public override async Task RunAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var successfullCount = 0;
            var faildTerminalIdList = new List<long>();

            using (var dataContext = new AppDataContext())
            {
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی وضعیت درخواست های تغییر حساب"
                });

                var changeAccountRequestsInfo = dataContext.ChangeAccountRequests
                    .Where(x =>   x.StatusId != (byte)Enums.RequestStatus.Done)
                    .Select(x => new {x.Id,x.PardakhtNovinTrackId, x.TerminalNo, x.RequestId, x.TopiarId, x.ShebaNo, x.AccountNo })
                    .ToList();

                var terminalNoList = changeAccountRequestsInfo.Select(x => new
                    {
                        x.TerminalNo,
                        x.TopiarId,
                        x.Id,
                        x.PardakhtNovinTrackId
                    }
                ).ToList();

                var xms = dataContext.Terminals.ToList()
                    .Where(x =>
                        terminalNoList.Select(b => b.TerminalNo).Contains(x.TerminalNo)
                        && x.StatusId != (byte)Enums.TerminalStatus.Revoked
                        && x.StatusId != (byte)Enums.TerminalStatus.Deleted
                        && x.DeviceTypeId != 22 && x.PspId ==  (byte)Enums.PspCompany.IranKish
                    ).ToList();
                var terminalData = xms
                    .Select(x => new terminalDataDto
                    {
                        Id =  x.Id,
                        PspId =    x.PspId,
                        ChangeTopiarId = terminalNoList.FirstOrDefault(a => a.TerminalNo == x.TerminalNo).TopiarId,
                        ShebaNo =    x.ShebaNo,
                        StatusId =   x.StatusId,
                        ContractNo = x.ContractNo,
                        NewParsian =  x.NewParsian,
                        TerminalNo =   x.TerminalNo,
                        TopiarId =  x.TopiarId
                    })
                    .ToList();

               
                var irankishData = terminalData.Where(x => x.PspId == (byte)Enums.PspCompany.IranKish).ToList();
            

                if (irankishData.Any())
                {
                    using (var irankishService = new NewIranKishService())
                    {
                        foreach (var terminalInfo in irankishData)
                        {
                            try
                            {
                                var inqueryResult = irankishService.AccountInquiry(terminalInfo.Id.ToString());
                                if (inqueryResult != null && inqueryResult.status)
                                {
                                    successfullCount++;

                                    var requestedShebaNo = changeAccountRequestsInfo
                                        .Where(x =>
                                            x.TerminalNo == terminalInfo.TerminalNo) 
                                        .FirstOrDefault();
                                    if (!string.IsNullOrEmpty(requestedShebaNo.ShebaNo) &&
                                        inqueryResult.data.accountList.Any(a=>a.status == 8 && a.iban == requestedShebaNo.ShebaNo))
                                    {
                                        await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                            .UpdateAsync(x => new Terminal
                                            {
                                                AccountNo = requestedShebaNo.AccountNo, ShebaNo = requestedShebaNo.ShebaNo
                                            });
                                        await dataContext.ChangeAccountRequests
                                            .Where(x => x.TerminalNo == requestedShebaNo.TerminalNo &&
                                                        x.ShebaNo == requestedShebaNo.ShebaNo).UpdateAsync(x =>
                                                new Data.Domain.ChangeAccountRequest
                                                    { StatusId = Enums.RequestStatus.Done.ToByte() });
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                faildTerminalIdList.Add(terminalInfo.Id);
                                exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                            }
                        }
                    }
                }

         
                stopwatch.Stop();
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "پایان",
                    ExecutionTime = DateTime.Now,
                    TaskName = "بروزرسانی وضعیت درخواست های تغییر حساب",
                    Message =
                        $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000}, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                });

                await dataContext.SaveChangesAsync();
            }
        }

        public override bool RunAt(DateTime utcNow)
        {
            var now = utcNow.ToLocalTime();

            //       return true;
         return (now.Hour == 12 && now.Minute == 1 && now.Second == 1)      || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)   ;
        }
    }
}