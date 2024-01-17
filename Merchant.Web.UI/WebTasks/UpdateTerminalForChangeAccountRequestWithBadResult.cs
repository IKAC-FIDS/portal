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
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateTerminalForChangeAccountRequestWithBadResult : ScheduledTaskTemplate
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
                    .Where(x => x.StatusId != (byte) Enums.RequestStatus.Done)
                    .Select(x => new {x.TerminalNo, x.RequestId, x.TopiarId, x.ShebaNo, x.AccountNo})
                    .ToList();

                var terminalNoList = changeAccountRequestsInfo.Select(x => new 
                {
                    
                      x.TerminalNo,
                      x.TopiarId
                }
            ).ToList();

                var xms =   dataContext.Terminals.ToList()
                    .Where(x =>
                        terminalNoList.Select(b => b.TerminalNo).Contains(x.TerminalNo)

                        && x.StatusId == (byte) Enums.TerminalStatus.Revoked
                       
                    ).ToList();
                var terminalData = xms
                    .Select(x => new
                    {
                        x.Id,
                        x.PspId,
                      ChangeTopiarId= terminalNoList.FirstOrDefault(a=>a.TerminalNo == x.TerminalNo).TopiarId,
                        x.ShebaNo,
                        x.StatusId,
                        x.ContractNo,
                        x.NewParsian,
                        x.TerminalNo,
                        x.TopiarId
                    })
                    .ToList();

                var fanavaData = terminalData.Where(x => x.PspId == (byte)Enums.PspCompany.Fanava).ToList();
                 var parsianData =
                     
                     terminalData.Where(x => x.PspId == (byte)Enums.PspCompany.Parsian 
                     
                     
                     ).ToList();
                var irankishData = terminalData.Where(x => x.PspId == (byte)Enums.PspCompany.IranKish).ToList();

                if (irankishData.Any())
                {
                    using (var irankishService = new IranKishService())
                    {
                        foreach (var terminalInfo in irankishData)
                        {
                            try
                            {
                                var inqueryResult = await irankishService.TryInqueryAcceptor(terminalInfo.TerminalNo, terminalInfo.Id, terminalInfo.StatusId);
                                if (inqueryResult != null && inqueryResult.IsSuccess)
                                {
                                    successfullCount++;
                                    if (inqueryResult.ShebaNo == terminalInfo.ShebaNo)
                                    {
                                        await dataContext.ChangeAccountRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo && x.ShebaNo == inqueryResult.ShebaNo).UpdateAsync(x => new Data.Domain.ChangeAccountRequest { StatusId = Enums.RequestStatus.Done.ToByte() });
                                    }
                                    else
                                    {
                                        var requestedShebaNo = changeAccountRequestsInfo.Where(x => x.TerminalNo == terminalInfo.TerminalNo).Select(x => x.ShebaNo).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(requestedShebaNo) && requestedShebaNo == inqueryResult.ShebaNo)
                                        {
                                            await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal { AccountNo = inqueryResult.AccountNo, ShebaNo = inqueryResult.ShebaNo });
                                            await dataContext.ChangeAccountRequests.Where(x => x.TerminalNo == inqueryResult.TerminalNo && x.ShebaNo == inqueryResult.ShebaNo).UpdateAsync(x => new Data.Domain.ChangeAccountRequest { StatusId = Enums.RequestStatus.Done.ToByte() });
                                        }
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

                if (  parsianData.Any())
                {
                  
                    using (var parsianService = new ParsianService())
                    {
                        foreach (var terminalInfo in parsianData)
                        {
                            try
                            {
                                var qqqqqq =
                                    changeAccountRequestsInfo.FirstOrDefault(x =>
                                        x.TerminalNo == terminalInfo.TerminalNo);
                                
                                var requestId = qqqqqq?.RequestId;
                                if (requestId.HasValue )
                                {
                                    var requestInqueryInput = new RequestInqueryInput();
                                    requestInqueryInput.RequestData = new RequestInqueryRequestData();
                                    requestInqueryInput.RequestData.TopiarId = terminalInfo.ChangeTopiarId.ToString();
                                    var result =   parsianService.RequestInQuery( requestInqueryInput,(int)terminalInfo.Id);
                                    var ParsianRequestedTerminalResult = new ParsianRequestedTerminalResult();

                                    
                                    if (result.IsSuccess)
                                    {
                                        if (result.RequestResult.RequestError != null)
                                            ParsianRequestedTerminalResult.Error = string.Join(",",
                                                result.RequestResult.RequestError.Select(v => v.ErrorText).ToArray());

                                        if (result.RequestResult.StatusCode == 2 && result.RequestResult.Stepcode == 7)
                                        {
                                            await dataContext.ChangeAccountRequests
                                                .Where(x => x.TerminalNo == terminalInfo.TerminalNo &&
                                                            x.ShebaNo == qqqqqq.ShebaNo).UpdateAsync(x =>
                                                    new Data.Domain.ChangeAccountRequest
                                                        {StatusId = Enums.RequestStatus.Done.ToByte()});
                                            await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                                .UpdateAsync(x => new Data.Domain.Terminal
                                                    {ShebaNo = qqqqqq.ShebaNo, AccountNo = qqqqqq.AccountNo});
                                        }
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                            }
                        }
                    }
                }

                if (fanavaData.Any())
                {
                    using (var fanavaService = new FanavaService())
                    {
                        foreach (var terminalInfo in fanavaData)
                        {
                            try
                            {
                                var result = await fanavaService.TryInqueryAcceptor(terminalInfo.ContractNo, terminalInfo.Id, terminalInfo.StatusId);

                                if (result.IsSuccess)
                                {
                                    successfullCount++;
                                    if (terminalInfo.ShebaNo == result.ShebaNo)
                                    {
                                        await dataContext.ChangeAccountRequests.Where(x => x.TerminalNo == result.TerminalNo && x.ShebaNo == result.ShebaNo).UpdateAsync(x => new Data.Domain.ChangeAccountRequest { StatusId = Enums.RequestStatus.Done.ToByte() });
                                    }
                                    else
                                    {
                                        var requestedShebaNo = changeAccountRequestsInfo.Where(x => x.TerminalNo == terminalInfo.TerminalNo).Select(x => x.ShebaNo).FirstOrDefault();
                                        if (!string.IsNullOrEmpty(requestedShebaNo) && requestedShebaNo == result.ShebaNo)
                                        {
                                            await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal { AccountNo = result.AccountNo, ShebaNo = result.ShebaNo });
                                            await dataContext.ChangeAccountRequests.Where(x => x.TerminalNo == result.TerminalNo && x.ShebaNo == result.ShebaNo).UpdateAsync(x => new Data.Domain.ChangeAccountRequest { StatusId = Enums.RequestStatus.Done.ToByte() });
                                        }
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
                    Message = $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000 }, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                });

                await dataContext.SaveChangesAsync();
            }
        }

        public override bool RunAt(DateTime utcNow)
        {

       // return true;
   var now = utcNow.AddHours(3.5);

            return now.Hour == 5 ; // هر یک ساعت یکبار راس دقیقه 50 - بین ساعت 6 صبح تا 5 بعدازظهر 
        }
    }
}