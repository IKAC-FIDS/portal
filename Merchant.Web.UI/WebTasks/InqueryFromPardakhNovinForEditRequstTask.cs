using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DNTScheduler;
using EntityFramework.Extensions;
using StackExchange.Exceptional;
using TES.Common.Enumerations;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using TerminalStatus = TES.Common.Enumerations.TerminalStatus;

namespace TES.Merchant.Web.UI.WebTasks
{
    public class InqueryFromPardakhNovinForEditRequstTask : ScheduledTaskTemplate
    {
        public override string Name => "دریافت اطلاعات درخواست ویرایش پایانه ها از پرداخت نوین";

        public override int Order => 5;

        public override async Task RunAsync()
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalData = await dataContext.Terminals.Include(b=>b.TerminalDocuments)
                     
                    .Where(x => x.PspId == (byte) PspCompany.PardakhNovin
                              
                                &&  x.PardakhtEditNovinSaveId != null
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
                        x.PardakhtNovinSaveId,
                        x.Title,
                        x.StatusId,
                        x.BlockAccountNumber,
                        x.BlockDocumentNumber,
                        x.AccountNo,
                        x.BatchDate,
                        x.ContractDate,
                        x.ContractNo 
                        ,x.MarketerId,
                        x.FollowupCode,
                        x.PspId,
                        x.DeviceTypeId,
                        x.TerminalDocuments,
                        x.MerchantProfile.MerchantProfileDocuments
                       
                    })
                    .ToListAsync();

                using (var pardakhtNovinService = new Service.PardakhtNovinService())
                {
                    foreach (var terminalInfo in terminalData)
                    {
                        try
                        {
                            var result = pardakhtNovinService.Inquery(terminalInfo.FollowupCode,terminalInfo.Id);

                            if (result.Status == PardakthNovinStatus.Successed &&
                                (result.Data.WorkFlowValue == "EndOfFlow" ||  result.Data.WorkFlowValue == "Confirmation" || 
                                 result.Data.WorkFlowValue == "CompletingDocuments"))
                            {


                                var status = Common.Enumerations.TerminalStatus.Installed;


                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = status.ToByte(),
                                        TerminalNo = result.Data.TerminalID,
                                        MerchantNo = result.Data.MerchantID,
                                        RevokeDate = result.Data.InstallationRollBackDate,
                                        InstallationDate = result.Data.InstallationDate,

                                    });




                                //todo ==> send files ======>
                                AddTerminalToMongo.Add(new TerminalMongo()
                                {
                                    TerminalNo = result.Data.TerminalID, PhoneNumber = terminalInfo.Mobile,
                                    Address = terminalInfo.Address,
                                    Description = terminalInfo.Description,
                                    Id = terminalInfo.Id,
                                    Tel = terminalInfo.Tel,
                                    Title = terminalInfo.Title,
                                    AccountNo = terminalInfo.AccountNo,
                                    BatchDate =
                                        terminalInfo.BatchDate.HasValue ? terminalInfo.BatchDate.ToString() : "",
                                    BlockPrice = terminalInfo.BlockPrice,
                                    BranchId = terminalInfo.BranchId,
                                    CityId = terminalInfo.CityId,
                                    ContractDate = terminalInfo.ContractDate.HasValue
                                        ? terminalInfo.ContractDate.ToString()
                                        : "",
                                    ContractNo = terminalInfo.ContractNo,
                                    GuildId = terminalInfo.GuildId,
                                    MarketerId = terminalInfo.MarketerId,
                                    PspId = terminalInfo.PspId,
                                    DeviceTypeId = terminalInfo.DeviceTypeId,


                                });
                            }
                            else if (
                                result.Status == PardakthNovinStatus.Successed &&
                                (result.Data.WorkFlowValue == "Canceling" ||
                                 result.Data.WorkFlowValue == "Canceling")

                            )
                            {
                                var status = Common.Enumerations.TerminalStatus.Revoked;


                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = status.ToByte(),
                                        //   TerminalNo = result.Data.TerminalID,
                                        //  MerchantNo = result.Data.MerchantID,
                                        //  RevokeDate = result.Data.InstallationRollBackDate,
                                        //   InstallationDate = result.Data.InstallationDate,

                                    });


                            }

                            else if (
                                result.Status == PardakthNovinStatus.Successed &&
                                (result.Data.WorkFlowValue == "SwitchError" ||
                                 result.Data.WorkFlowValue == "Correction" || 
                                 result.Data.WorkFlowValue == "ShaparakError")

                            )
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Common.Enumerations.TerminalStatus.NeedToReform.ToByte(),
                                        ErrorComment = result.Data.FlowMessage
                                    });
                            }

                            else if (result.Status == PardakthNovinStatus.Successed &&
                                     result.Data.WorkFlowValue == "Agency")
                            {
                                var    status = Common.Enumerations.TerminalStatus.ReadyForAllocation;
                              

                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = status.ToByte(),
                                        TerminalNo = result.Data.TerminalID,
                                        MerchantNo = result.Data.MerchantID,
                                        RevokeDate = result.Data.InstallationRollBackDate,
                                        InstallationDate = result.Data.InstallationDate,
                                        
                                    });



                            }
                            else if(result.Status == PardakthNovinStatus.Successed)
                            {
                                await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        
                                        Description =  result.Data.WorkFlowCaption
                                    });
                            }
                            // else if (result.Status == 1)
                            // {
                            //     await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                            //         .UpdateAsync(x => new Terminal
                            //         {
                            //             TerminalNo = result.Terminal,
                            //             MerchantNo = result.Acceptor
                            //         });
                            //     
                            //  
                            // }
                            // else if (result.Status == 4)
                            // {
                            //     await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                            //         .UpdateAsync(x => new Terminal
                            //         {
                            //             TerminalNo = result.Terminal,
                            //             MerchantNo = result.Acceptor,
                            //             StatusId = Common.Enumerations.TerminalStatus.SendToShaparak.ToByte()
                            //         });
                            //     //
                            //   
                            // }
                            // else if (result.Status == 3 || result.Status == 5)
                            // {
                            //     var errorComment = result.Status == 3
                            //         ? "عدم تایید تعریف پذیرنده در شاپرک"
                            //         : "رد درخواست در کارت اعتباری ایران کیش";
                            //
                            //     if (!string.IsNullOrEmpty(result.ShaparakResponseFa))
                            //     {
                            //         errorComment = errorComment + " " + result.ShaparakResponseFa;
                            //     }
                            //     await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                            //         .UpdateAsync(x => new Terminal
                            //         {
                            //             StatusId = Common.Enumerations.TerminalStatus.NeedToReform.ToByte(),
                            //             ErrorComment = errorComment
                            //         });
                            // }
                            // else if (result.Status == -1)
                            // {
                            //     var errorComment = result.Description;
                            //     if (!string.IsNullOrEmpty(result.ShaparakResponseFa))
                            //     {
                            //         errorComment = errorComment + " " + result.ShaparakResponseFa;
                            //     }
                            //     await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                            //         .UpdateAsync(x => new Terminal
                            //         {
                            //             StatusId = Common.Enumerations.TerminalStatus.NeedToReform.ToByte(),
                            //             ErrorComment = errorComment
                            //         });
                            // }
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
            //     return  true;
            return (now.Hour == 12 && now.Minute == 12 && now.Second == 1)  || (now.Hour == 8 && now.Minute == 1 && now.Second == 1)  ;
        }
    }
}