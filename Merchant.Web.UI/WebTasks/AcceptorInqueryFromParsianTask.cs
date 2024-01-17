using DNTScheduler;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TES.Common.Enumerations;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using EntityFramework.Extensions;
using TES.Common.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class AcceptorInqueryFromParsianTask : ScheduledTaskTemplate
    {
        // استعلام وضعیت نصب
        public override string Name => "استعلام وضعیت پایانه ها از پارسیان";

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

                var terminalData = await dataContext.Terminals
                    .Where(x => 
                     
                        x.PspId == (byte) PspCompany.Parsian && (!string.IsNullOrEmpty(x.ContractNo)
                                                                         || (x.NewParsian.HasValue &&
                                                                             x.NewParsian.Value))
                                                                     && x.StatusId !=
                                                                     (byte) Enums.TerminalStatus.Deleted
                                                                     && x.StatusId !=
                                                                     (byte) Enums.TerminalStatus.Installed
                                                                     && x.StatusId !=
                                                                     (byte) Enums.TerminalStatus.Revoked     &&    x.DeviceTypeId != 22  
                                                                     
                                                                   
                                                                 
                                                                     )
                    .Select(x => new
                    {
                        x.Id,
                        x.StatusId,
                        x.ContractNo,
                        x.TerminalNo,
                        x.NewParsian,
                      
                        x.TopiarId , 
                      
                        x.RevokeDate,
                     
                        x.InstallationDate,
                        x.MerchantProfileId,
                        x.MerchantProfile.Mobile , 
                        x.Address,
                        x.BranchId,
                        x.CityId,
                        x.BlockPrice,
                        x.Description,
                        x.Tel,
                        x.GuildId,
                        x.Title,
                      
                        x.BlockAccountNumber,
                        x.BlockDocumentNumber,
                        x.AccountNo,
                        x.BatchDate,
                        x.ContractDate,
                      
                        x.MarketerId,
                        x.PspId,
                        x.DeviceTypeId,
                    })
                    .ToListAsync();


                var successfullCount = 0;
                var faildTerminalIdList = new List<long>();

                foreach (var terminalInfo in terminalData)
                {
                    try
                    {
                        if (terminalInfo.NewParsian != null && terminalInfo.NewParsian.Value)
                        {
                            var result =
                                await parsianService.UpdateStatusForRequestedTerminal(terminalInfo.TopiarId.ToString(),
                                    (int) terminalInfo.Id);
                            if (string.IsNullOrEmpty(result.Error))
                            {
                                if (!string.IsNullOrEmpty(result.TerminalNo))
                                {
                                    dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                    {
                                        StatusId = result.StatusId, InstallStatus = result.InstallStatus,
                                        StepCodeTitle = result.StepCodeTitle,
                                        TerminalNo = result.TerminalNo,
                                        BatchDate = result.ShaparakRegisterDate != null
                                            ? DateTime.Parse(result.ShaparakRegisterDate)
                                            : (DateTime?) null,
                                        MerchantNo = result.AccecptorCode, StepCode = result.StepCode,
                                        InstallStatusId = result.InstallStatusId,

                                        ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {result.Error}",
                                        LastUpdateTime = DateTime.Now
                                    });
                                    
                                    AddTerminalToMongo.Add(new TerminalMongo()
                                    {
                                        
                                        TerminalNo =  result.TerminalNo,PhoneNumber = terminalInfo.Mobile,
                                        Address = terminalInfo.Address,
                                        Description = terminalInfo.Description,  
                                        Id = terminalInfo.Id,
                                        Tel = terminalInfo.Tel,
                                        Title = terminalInfo.Title,
                                        AccountNo = terminalInfo.AccountNo,
                                        BatchDate = terminalInfo.BatchDate.HasValue ? terminalInfo.BatchDate.ToString() : "",
                                        BlockPrice = terminalInfo.BlockPrice,
                                        BranchId = terminalInfo.BranchId,
                                        CityId = terminalInfo.CityId,
                                        ContractDate = terminalInfo.ContractDate.HasValue ? terminalInfo.ContractDate.ToString() : "",
                                        ContractNo = terminalInfo.ContractNo,
                                        GuildId = terminalInfo.GuildId,
                                        MarketerId = terminalInfo.MarketerId,
                                        PspId = terminalInfo.PspId,
                                        DeviceTypeId = terminalInfo.DeviceTypeId,
                                    });
                                    
                            }
                            else
                                {
                                    dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                    {
                                        StatusId = result.StatusId
                                        , InstallStatus = result.InstallStatus,
                                        StepCodeTitle = result.StepCodeTitle
                                        , StepCode = result.StepCode,
                                        MerchantNo = result.AccecptorCode
                                        , InstallStatusId = result.InstallStatusId,
                                        ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {result.Error}",
                                        LastUpdateTime = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(result.TerminalNo))
                                    dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                    {
                                        StatusId = result.StatusId, InstallStatus = result.InstallStatus,
                                        StepCodeTitle = result.StepCodeTitle, TerminalNo = result.TerminalNo,
                                        BatchDate = DateTime.Parse(result.ShaparakRegisterDate),
                                        MerchantNo = result.AccecptorCode, StepCode = result.StepCode,
                                        InstallStatusId = result.InstallStatusId,
                                        ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {result.Error}",
                                        LastUpdateTime = DateTime.Now
                                    });
                                else
                                {
                                    dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                    {
                                        StatusId = result.StatusId, InstallStatus = result.InstallStatus,
                                        StepCodeTitle = result.StepCodeTitle, StepCode = result.StepCode,
                                        MerchantNo = result.AccecptorCode, InstallStatusId = result.InstallStatusId,
                                        ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {result.Error}",
                                        LastUpdateTime = DateTime.Now
                                    });
                                }
                            }
                        }
                        else
                        {
                            var result =
                                  parsianService.UpdateStatus(terminalInfo.ContractNo, terminalInfo.StatusId).Result;
                            if (result.Item1)
                            {
                                dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                {
                                    StatusId = result.Item2,
                                    ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {result.Item3}",
                                    LastUpdateTime = DateTime.Now
                                });
                            }
                        }


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

         //  return true;
         return (now.Hour == 12 && now.Minute == 1 && now.Second == 1)    || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)     ;
        }
    }
}