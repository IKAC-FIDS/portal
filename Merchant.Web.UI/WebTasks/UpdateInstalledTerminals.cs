using DNTScheduler;
using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class UpdateInstalledTerminals : ScheduledTaskTemplate
    {
        public override string Name => "بروزرسانی پایانه های نصب شده";

        public override int Order => 3;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalData = await dataContext.Terminals
                   .Where(x =>
                     //  x.PspId == (byte)Enums.PspCompany.Parsian &&
                         //          &&  x.StatusId  != (byte)Enums.TerminalStatus.Revoked 
                    
                        x.StatusId != (byte)Enums.TerminalStatus.Deleted)
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.PspId,
                        x.StatusId,
                        x.ContractNo,
                        x.TerminalNo,
                        x.NewParsian
                    })
                    .ToListAsync();

                var parsianData = terminalData.Where(x =>
                    
                    x.PspId == (byte)Enums.PspCompany.Parsian  && !string.IsNullOrEmpty( 
                    x.TerminalNo )   ).ToList();
                var irankishData = terminalData.Where(x =>  x.PspId == (byte)Enums.PspCompany.IranKish ).ToList();
                var fanavaData = terminalData.Where(x => x.PspId == (byte)Enums.PspCompany.Fanava ).ToList();

                if (irankishData.Any())
                {
                    var irankishStopwatch = new Stopwatch();
                    irankishStopwatch.Start();

                    var irankishSuccessfullCount = 0;
                    var irankishFaildTerminalIdList = new List<long>();

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "شروع",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده ایران کیش"
                    });

                    await dataContext.SaveChangesAsync();

                    using (var irankishService = new IranKishService())
                    {
                        foreach (var terminalInfo in irankishData)
                        {
                            try
                            {
                                var inqueryResult = await irankishService.TryInqueryAcceptor(terminalInfo.TerminalNo, terminalInfo.Id, terminalInfo.StatusId);
                                if (inqueryResult != null && inqueryResult.IsSuccess)
                                {
                                    await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
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

                                    irankishSuccessfullCount++;
                                }
                            }
                            catch
                            {
                                irankishFaildTerminalIdList.Add(terminalInfo.Id);
                            }
                        }
                    }

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "پایان",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده ایران کیش",
                        Message = $"مجموع:{irankishData.Count}, موارد موفق:{irankishSuccessfullCount}, زمان سپری شده:{irankishStopwatch.ElapsedMilliseconds / 1000 }, شماره پیگیری پایانه های ناموفق:{string.Join(",", irankishFaildTerminalIdList)}"
                    });

                    await dataContext.SaveChangesAsync();
                }

                if (parsianData.Any())
                {
                    var parsianStopwatch = new Stopwatch();
                    parsianStopwatch.Start();

                    var parsianSuccessfullCount = 0;
                    var parsianFaildTerminalIdList = new List<long>();

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "شروع",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده پارسیان"
                    });

                    await dataContext.SaveChangesAsync();

                    using (var parsianService = new ParsianService())
                    {
                        foreach (var terminalInfo in parsianData  )
                        {
                            try
                            {

                                if (true)
                                {
                                    var result =   parsianService.UpdateStatusForRegisteredTerminal( terminalInfo.TerminalNo, (int)terminalInfo.Id  );
                                    if (result.IsSuccess)
                                    {
                                        await dataContext.Terminals
                                            .Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
                                        {
                                            StatusId = result.StatusId,
                                            Description = result.Status,
                                            ErrorComment =  result.Error,
                                            InstallationDate = result.InstallationDate,
                                           
                                            LastUpdateTime = DateTime.Now
                                        });

                                        // if ((result.StatusId == (byte) Enums.TerminalStatus.WaitingForRevoke
                                        //      || result.StatusId == (byte) Enums.TerminalStatus.Revoked)
                                        //     && result.RevokeDate.HasValue
                                        // )
                                        // {
                                        //     await dataContext.Terminals
                                        //         .Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
                                        //         {
                                        //           RevokeDate =  result.RevokeDate,
                                        //             LastUpdateTime = DateTime.Now
                                        //         });
                                        //
                                        // }
                                        parsianSuccessfullCount++;
                                    }
                                    else
                                    {
                                        await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
                                        {
                                            StatusId = result.StatusId,
                                            Description = result.Status,
                                            ErrorComment =  result.Error,
                                            LastUpdateTime = DateTime.Now
                                        });

                                    }
                                }
                                // else
                                // {
                                //     var result = await parsianService.UpdateStatus( terminalInfo.TerminalNo ,terminalInfo.StatusId );
                                //     if (result.Item1)
                                //     {
                                //         await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).UpdateAsync(x => new Terminal
                                //         {
                                //             StatusId = result.Item2,
                                //             Description = result.Item3,
                                //             LastUpdateTime = DateTime.Now
                                //         });
                                //
                                //         parsianSuccessfullCount++;
                                //     }   
                                // }
                                
                               
                            }
                            catch(Exception ex)
                            {
                                parsianFaildTerminalIdList.Add(terminalInfo.Id);
                            }
                        }
                    }

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "پایان",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده پارسیان",
                        Message = $"مجموع:{parsianData.Count}, موارد موفق:{parsianSuccessfullCount}, زمان سپری شده:{parsianStopwatch.ElapsedMilliseconds / 1000 }, شماره پیگیری پایانه های ناموفق:{string.Join(",", parsianFaildTerminalIdList)}"
                    });

                    await dataContext.SaveChangesAsync();
                }

                if (fanavaData.Any())
                {
                    var fanavaStopwatch = new Stopwatch();
                    fanavaStopwatch.Start();

                    var fanavaSuccessfullCount = 0;
                    var fanavaFaildTerminalIdList = new List<long>();

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "شروع",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده فن آوا"
                    });

                    await dataContext.SaveChangesAsync();

                    using (var fanavaService = new FanavaService())
                    {
                        foreach (var terminalInfo in fanavaData)
                        {
                            try
                            {
                                var result = await fanavaService.TryInqueryAcceptor(terminalInfo.ContractNo, terminalInfo.Id, terminalInfo.StatusId);

                                if (result.IsSuccess)
                                {
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
                                    fanavaSuccessfullCount++;
                                }
                            }
                            catch
                            {
                                fanavaFaildTerminalIdList.Add(terminalInfo.Id);
                            }
                        }
                    }

                    dataContext.AutomatedTaskLogs.Add(new AutomatedTaskLog
                    {
                        ActivityTitle = "پایان",
                        ExecutionTime = DateTime.Now,
                        TaskName = "دریافت اطلاعات پایانه های نصب شده فن آوا",
                        Message = $"مجموع:{fanavaData.Count}, موارد موفق:{fanavaSuccessfullCount}, زمان سپری شده:{fanavaStopwatch.ElapsedMilliseconds / 1000 }, شماره پیگیری پایانه های ناموفق:{string.Join(",", fanavaFaildTerminalIdList)}"
                    });

                    await dataContext.SaveChangesAsync();
                }
            }
        }

        public override bool RunAt(DateTime utcNow)
        {

        
             var now = utcNow.AddHours(4.5);
            
              return now.Hour == 20 && now.Minute == 0 && now.Second == 0; // راس ساعت 20:00 صبح هر روز
        }
    }
}