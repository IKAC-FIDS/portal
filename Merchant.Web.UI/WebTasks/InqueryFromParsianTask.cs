using DNTScheduler;
using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class InqueryFromParsianTask : ScheduledTaskTemplate
    {
        public override string Name => "دریافت اطلاعات پایانه ها از پارسیان";

        public override int Order => 6;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var terminalData = await dataContext.Terminals 
                    .Where(x => 
                    
                        x.PspId == (byte)Enums.PspCompany.Parsian && 
                                !string.IsNullOrEmpty(x.ContractNo)    &&    x.DeviceTypeId != 22   &&   
                                x.StatusId != (byte)Enums.TerminalStatus.Deleted && 
                                ((x.StatusId == (byte)Enums.TerminalStatus.Installed && !x.InstallationDate.HasValue) || 
                                (x.StatusId == (byte)Enums.TerminalStatus.Revoked && !x.RevokeDate.HasValue) || 
                                string.IsNullOrEmpty(x.TerminalNo)))
                      .Select(x => new
                    {
                        x.Id, x.ContractNo ,x.MerchantProfile.Mobile ,
                        
                        x.StatusId,
                        x.RevokeDate,
                        x.TerminalNo,
                       
                        x.InstallationDate,
                        x.MerchantProfileId,
                      
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

                using (var parsianService = new ParsianService())
                {
                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "شروع",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه ها از پارسیان"
                    });

                    foreach (var terminalInfo in terminalData)
                    {
                        try
                        {
                            var inqueryResult = await parsianService.UpdateTerminalInfo(terminalInfo.ContractNo);
                            if (inqueryResult.IsSuccess)
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
                                {
                                    RevokeDate = inqueryResult.RevokeDate,
                                    Description = inqueryResult.Description,
                                    MerchantNo = inqueryResult.MerchantNo.ToString(),
                                    TerminalNo = inqueryResult.TerminalNo.ToString(),
                                    InstallationDate = inqueryResult.InstallationDate
                                });
                                AddTerminalToMongo.Add(new TerminalMongo()
                                {
                                    TerminalNo =  inqueryResult.TerminalNo,
                                    PhoneNumber = terminalInfo.Mobile,
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
                                successfullCount++;
                            }
                            else
                            {
                                faildTerminalIdList.Add(terminalInfo.Id);
                            }
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
                        TaskName = "دریافت اطلاعات پایانه ها از پارسیان",
                        Message = $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000 }, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                    });

                    await dataContext.SaveChangesAsync();
                }
            }
        }

        public override bool RunAt(DateTime utcNow)
        { 
           // return true;
            var now = utcNow.AddHours(3.5);

           return now.Hour >= 6 && now.Hour < 17 && now.Minute == 40 && now.Second == 0; // هر یک ساعت یکبار راس دقیقه 40 - بین ساعت 6 صبح تا 5 بعدازظهر 
        }
    }
}