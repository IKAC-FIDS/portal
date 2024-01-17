using DNTScheduler;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TES.Common.Enumerations;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class AcceptorInqueryFromFanavaTask : ScheduledTaskTemplate
    {
        public override string Name => "استعلام وضعیت پایانه ها از فن آوا";

        public override int Order => 1;

        public override async Task RunAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var dataContext = new AppDataContext())
            using (var fanavaService = new FanavaService())
            {
                dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                {
                    ActivityTitle = "شروع",
                    ExecutionTime = DateTime.Now,
                    TaskName = "استعلام وضعیت پایانه ها از فن آوا"
                });

                var terminalData = await dataContext.Terminals
                    .Where(x => x.PspId == (long) PspCompany.Fanava
                            &&    x.DeviceTypeId != 22  
                                && x.StatusId != (byte) Enums.TerminalStatus.Deleted
                                &&  (x.StatusId == (byte) Enums.TerminalStatus.Installed ||
                                     x.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                                && x.StatusId != (byte) Enums.TerminalStatus.Revoked)
                    .Select(x => new
                    {
                        x.Id,
                        x.StatusId,
                        x.RevokeDate,
                        x.TerminalNo,
                        x.ContractNo,
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
                        var result = await fanavaService.TryInqueryAcceptor(terminalInfo.ContractNo, terminalInfo.Id,
                            terminalInfo.StatusId);

                        if (result.IsSuccess)
                        {
                            if (terminalInfo.StatusId == (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch &&
                                result.TerminalStatus.HasValue && result.TerminalStatus.Value == terminalInfo.StatusId)
                            {
                                if (!string.IsNullOrEmpty(result.ErrorComment))
                                {
                                    await dataContext.Database.ExecuteSqlCommandAsync(
                                        $@"UPDATE psp.Terminal SET ErrorComment = N'{result.ErrorComment}' WHERE Id = {terminalInfo.Id}");
                                }

                                successfullCount++;
                                continue;
                            }

                            var commandText = $@"UPDATE psp.Terminal SET 
                                                     ErrorComment = NULL,
                                                     ShebaNo = '{result.ShebaNo}', 
                                                     AccountNo = '{result.AccountNo}',  
                                                     TerminalNo = '{result.TerminalNo}',
                                                     MerchantNo = '{result.MerchantNo}',
                                                     ContractDate = '{result.ContractDate}',
                                                     LastUpdateTime = '{DateTime.Now}', 
                                                     BatchDate = {result.BatchDate.ConvertToDbReadyDateTime()},
                                                     StatusId = {result.TerminalStatus ?? terminalInfo.StatusId},
                                                     InstallationDate = {result.InstallationDate.ConvertToDbReadyDateTime()},
                                                     RevokeDate = {result.RevokeDate.ConvertToDbReadyDateTime()} WHERE Id = {terminalInfo.Id};";

                            await dataContext.Database.ExecuteSqlCommandAsync(commandText);
                            AddTerminalToMongo.Add(new TerminalMongo()
                            {
                                
                                
                                TerminalNo =  result.TerminalNo,
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
                    TaskName = "استعلام وضعیت پایانه ها از فن آوا",
                    Message =
                        $"مجموع:{terminalData.Count}, موارد موفق:{successfullCount}, زمان سپری شده:{stopwatch.ElapsedMilliseconds / 1000}, شماره پیگیری پایانه های ناموفق:{string.Join(",", faildTerminalIdList)}"
                });

                await dataContext.SaveChangesAsync();
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