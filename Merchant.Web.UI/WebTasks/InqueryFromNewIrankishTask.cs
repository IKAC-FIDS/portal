﻿using DNTScheduler;
using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TES.Common.Enumerations;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.IranKishServiceRefrence;
using TES.Merchant.Web.UI.Service;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class InqueryFromNewIrankishTask : ScheduledTaskTemplate
    {
        public override string Name => "دریافت اطلاعات پایانه ها از ایرانکیش";

        public override int Order => 5;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
           
                var terminalData = await dataContext.Terminals.Include(b=>b.TerminalDocuments)
                     
                    .Where(x => 
                    
                    
                        x.PspId == (byte) PspCompany.IranKish
                                && (x.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch
                                    || x.StatusId == (byte) Enums.TerminalStatus.SendToShaparak
                                    || x.StatusId == (byte) Enums.TerminalStatus.NeedToReform)
                                && x.StatusId != (byte) Enums.TerminalStatus.Deleted
                                && x.StatusId != (byte) Enums.TerminalStatus.Revoked     &&    x.DeviceTypeId != 22  
                                 
                    )
                    .Select(x => new
                    {
                        x.Id,
                        x.RevokeDate,
                        x.InstallationDate,
                        x.MerchantProfile.Mobile,
                        x.Address,
                        x.BranchId,
                        x.CityId,
                        x.BlockPrice,
                        x.Description,
                        x.Tel,
                        x.GuildId,
                        x.Title,
                        x.StatusId,
                        x.BlockAccountNumber,
                        x.BlockDocumentNumber,
                        x.AccountNo,
                        x.BatchDate,
                        x.ContractDate,
                        x.ContractNo 
                        ,x.MarketerId,
                        x.PspId,
                        x.DeviceTypeId,
                        x.TerminalDocuments,
                        x.MerchantProfile.MerchantProfileDocuments
                       
                    })
                    .ToListAsync();

                using (var irankishService = new Service.NewIranKishService())
                {
                    foreach (var terminalInfo in terminalData)
                    {
                        try
                        {
                            var result = irankishService.Inquery(terminalInfo.Id.ToString());

                            if (result.status && result.data.status ==   7)
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        ErrorComment = result.data.documnentStatus + " " + result.data.description,
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte()
                                    });  
                            }
                            else if (result.status && result.data != null && result.data.status == 2)
                            {
                                // var status = Enums.TerminalStatus.ReadyForAllocation;
                                // if (!result.data.terminal.FirstOrDefault().mountDate.HasValue && result.MountDate.HasValue)
                                // {
                                //     status = Enums.TerminalStatus.Installed;
                                // }
                                //
                                // if (terminalInfo.RevokeDate.HasValue)
                                // {
                                //     status = Enums.TerminalStatus.Revoked;
                                // }
                                //
                                // await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                //     .UpdateAsync(x => new Terminal
                                //     {
                                //         StatusId = status.ToByte(),
                                //         TerminalNo = result.Terminal,
                                //         MerchantNo = result.Acceptor,
                                //         RevokeDate = result.DisMountDate,
                                //         InstallationDate = result.MountDate,
                                //         
                                //     });
                                // //todo ==> send files =====>
                                //
                                //
                                //
                                //
                                // //todo ==> send files ======>
                                // AddTerminalToMongo.Add(new TerminalMongo()
                                // {
                                //     TerminalNo =  result.Terminal,PhoneNumber = terminalInfo.Mobile,
                                //     Address = terminalInfo.Address,
                                //     Description = terminalInfo.Description,  
                                //     Id = terminalInfo.Id,
                                //     Tel = terminalInfo.Tel,
                                //     Title = terminalInfo.Title,
                                //     AccountNo = terminalInfo.AccountNo,
                                //     BatchDate = terminalInfo.BatchDate.HasValue ? terminalInfo.BatchDate.ToString() : "",
                                //     BlockPrice = terminalInfo.BlockPrice,
                                //     BranchId = terminalInfo.BranchId,
                                //     CityId = terminalInfo.CityId,
                                //     ContractDate = terminalInfo.ContractDate.HasValue ? terminalInfo.ContractDate.ToString() : "",
                                //     ContractNo = terminalInfo.ContractNo,
                                //    GuildId = terminalInfo.GuildId,
                                //    MarketerId = terminalInfo.MarketerId,
                                //    PspId = terminalInfo.PspId,
                                //    DeviceTypeId = terminalInfo.DeviceTypeId,
                                //   
                                //     
                                // });
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        TerminalNo = result.data.terminal.FirstOrDefault().termianlNo,
                                        MerchantNo = result.data.accountNo,
                                    });

                            }
                            else if (result.data.status == 1)
                            {
                             //   await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                             //       .UpdateAsync(x => new Terminal        
                             //       {
                              //          TerminalNo = result.data.terminal.FirstOrDefault().termianlNo,
                              //          MerchantNo = result.data.accountNo,
                               //     });
                                
                             
                            }
                            else if (result.data.status == 4)
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        TerminalNo = result.data.terminal.FirstOrDefault().termianlNo,
                                        MerchantNo = result.data.accountNo,
                                        StatusId = Enums.TerminalStatus.SendToShaparak.ToByte()
                                    });
                                
                              
                            }
                            else if (result.data.status == 3 || result.data.status == 5)
                            {
                                ///// OK ===============================================================
                                var errorComment = result.data.status == 3
                                    ? "عدم تایید تعریف پذیرنده در شاپرک"
                                    : "رد درخواست در کارت اعتباری ایران کیش";
                            
                                if (!string.IsNullOrEmpty(result.data.description))
                                {
                                    errorComment = errorComment + " " + result.data.description;
                                }
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte(),
                                        ErrorComment = errorComment
                                    });
                            }
                            else if (result.data.status == -1)
                            {
                                var errorComment = result.data.description;
                                if (!string.IsNullOrEmpty(result.data.description))
                                {
                                    errorComment = errorComment + " " + result.data.description;
                                }
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte(),
                                        ErrorComment = errorComment
                                    });
                            }                      
                            else if (result.data.status == 6)
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NotReturnedFromSwitch.ToByte(),
                                        ErrorComment = ""
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
        }

        public override bool RunAt(DateTime utcNow)
        {
            var now = utcNow.ToLocalTime();
            return  true;
         // return (now.Hour == 12 && now.Minute == 1 && now.Second == 1)  || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)  ;
        }
    }
}