using DNTScheduler;
using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateTerminalsWithRevokeRequest : ScheduledTaskTemplate
    {
        public override string Name => "استعلام وضعیت پایانه هایی که درخواست جمع آوری داشته اند";

        public override int Order => 3;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                // Edited in 99/12/27 by razavi
                var terminalNoList = dataContext.RevokeRequests
                    .Where(x => x.StatusId != (byte) Enums.RequestStatus.Done
                                && x.StatusId != (byte) Enums.TerminalStatus.Revoked
                                && x.StatusId != (byte) Enums.TerminalStatus.Deleted
                             
                    )
                    .Select(x => x.TerminalNo)
                    .Distinct()
                    .ToList();

                var terminalData = await dataContext.Terminals
                    .Where(x => 
                        terminalNoList.Contains(x.TerminalNo))
                    .Select(x => new
                    {
                        x.Id,
                        x.PspId,
                        x.StatusId,
                        x.ContractNo,
                        x.TerminalNo,
                        x.RevokreRequestSavedId
                    })
                    .ToListAsync();

                var fanavaData = terminalData.Where(x => x.PspId == (byte) Enums.PspCompany.Fanava).ToList();
                var parsianData = terminalData.Where(x => x.PspId == (byte) Enums.PspCompany.Parsian).ToList();
                var irankishData = terminalData.Where(x => x.PspId == (byte) Enums.PspCompany.IranKish).ToList();
                var pardakhtNovinDAta = terminalData.Where(x => x.PspId == (byte) Enums.PspCompany.PardakhNovin).ToList();

                  if (pardakhtNovinDAta.Any())
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                        foreach (var terminalInfo in pardakhtNovinDAta)
                        {
                            try
                            
                            {
                                //todo
                                var inqueryResult =   pardakhtNovinService.RevokRequestInquery(terminalInfo.TerminalNo,
                                    terminalInfo.Id, terminalInfo.StatusId , terminalInfo.RevokreRequestSavedId);
                                if (inqueryResult.Data != null && inqueryResult.Data.WorkFlowCaption == "EndOfFlow")
                                {
                                    await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x =>
                                        new Terminal
                                        {
                                            
                                            StatusId =  (byte) Enums.TerminalStatus.Revoked,
                                           
                                            RevokeDate = DateTime.Parse(  inqueryResult.Data.RollBackDate),
                                        
                                        });
   
                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.Done.ToByte()});
                                }
                                else
                                if (inqueryResult.Data != null && 
                                    
                                    (
                                     inqueryResult.Data.WorkFlowCaption == "ShaparakError"))
                                {
                                  
   
                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.ShaparkError.ToByte()});
                                }
                                else
                                if (inqueryResult.Data != null && 
                                    
                                    (
                                     inqueryResult.Data.WorkFlowCaption == "SwitchError"))
                                {
                                  
   
                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.SwitchError.ToByte()});
                                }
                            }
                            catch (Exception exception)
                            {
                                exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                            }
                        }
                    }
                }

                
                  
                if (irankishData.Any())
                {
                    using (var irankishService = new IranKishService())
                    {
                        foreach (var terminalInfo in irankishData)
                        {
                            try
                            {
                                var inqueryResult = await irankishService.TryInqueryAcceptor(terminalInfo.TerminalNo,
                                    terminalInfo.Id, terminalInfo.StatusId);
                                if (inqueryResult != null && inqueryResult.IsSuccess &&
                                    inqueryResult.StatusId == Enums.TerminalStatus.Revoked.ToByte())
                                {
                                    await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x =>
                                        new Terminal
                                        {
                                            ShebaNo = inqueryResult.ShebaNo,
                                            StatusId = inqueryResult.StatusId,
                                            AccountNo = inqueryResult.AccountNo,
                                            RevokeDate = inqueryResult.RevokeDate,
                                            Description = inqueryResult.Description,
                                            ErrorComment = inqueryResult.ErrorComment,
                                            LastUpdateTime = inqueryResult.LastUpdateTime,
                                            InstallationDate = inqueryResult.InstallationDate
                                        });

                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.Done.ToByte()});
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            catch (Exception exception)
                            {
                                exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                            }
                        }
                    }
                }

                
                if (parsianData.Any())
                {
                    using (var parsianService = new ParsianService())
                    {
                        foreach (var terminalInfo in parsianData)
                        {
                            try
                            {
                                var result = parsianService.UpdateStatusForRegisteredTerminal(terminalInfo.TerminalNo,
                                    (int) terminalInfo.Id);
                                if (result.IsSuccess)
                                {
                                    await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x =>
                                        new Terminal
                                        {
                                            StatusId = result.StatusId,
                                            Description = result.Status,
                                            ErrorComment = result.Error,
                                            InstallationDate = result.InstallationDate,
                                            LastUpdateTime = DateTime.Now
                                        });

                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == terminalInfo.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.Done.ToByte()});

                                    // parsianSuccessfullCount++;
                                }
                                else
                                {
                                    await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x =>
                                        new Terminal
                                        {
                                            StatusId = result.StatusId,
                                            Description = result.Status,
                                            ErrorComment = result.Error,
                                            LastUpdateTime = DateTime.Now
                                        });
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
                                var result = await fanavaService.TryInqueryAcceptor(terminalInfo.ContractNo,
                                    terminalInfo.Id, terminalInfo.StatusId);

                                if (result != null && result.IsSuccess &&
                                    result.TerminalStatus == (byte) Enums.TerminalStatus.Revoked)
                                {
                                    var commandText = $@"UPDATE psp.Terminal SET 
                                                     ErrorComment = NULL,
                                                     ShebaNo = '{result.ShebaNo}', 
                                                     AccountNo = '{result.AccountNo}',  
                                                     LastUpdateTime = '{DateTime.Now}', 
                                                     TerminalNo = '{result.TerminalNo}',
                                                     MerchantNo = '{result.MerchantNo}',
                                                     ContractDate = '{result.ContractDate}',
                                                     BatchDate = {result.BatchDate.ConvertToDbReadyDateTime()},
                                                     StatusId = {result.TerminalStatus ?? terminalInfo.StatusId},
                                                     InstallationDate = {result.InstallationDate.ConvertToDbReadyDateTime()},
                                                     RevokeDate = {result.RevokeDate.ConvertToDbReadyDateTime()} WHERE Id = {terminalInfo.Id};";

                                    await dataContext.Database.ExecuteSqlCommandAsync(commandText);
                                    await dataContext.RevokeRequests.Where(x => x.TerminalNo == result.TerminalNo)
                                        .UpdateAsync(x => new RevokeRequest
                                            {StatusId = Enums.RequestStatus.Done.ToByte()});
                                }
                            }
                            catch (Exception exception)
                            {
                                exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                            }
                        }
                    }
                }

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