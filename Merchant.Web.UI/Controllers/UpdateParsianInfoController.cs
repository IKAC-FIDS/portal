
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Stimulsoft.Base.Json;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class UpdateParsianInfoController : Controller
    {
        
        
        public ActionResult Run()
        {
            var dataContext = new AppDataContext();
           var temp3 =  dataContext._tempSarmayeh.Where(b=> string.IsNullOrEmpty(b.Serial)).ToList();
            var temp = temp3.Select(a=>a.TerminalNo).ToList();
            var terminals = dataContext.Terminals.Where(b =>  b.Id == 349457   ).ToList();
      

          
            var groupBy = terminals.GroupBy(b => b.ShebaNo);
            var count = terminals.Count;
            var t = 0;
            Console.WriteLine("========>" +  terminals.Count);
           


                 foreach (var item in groupBy)
                 {

                     try
                     {
                         Console.WriteLine($"========>{++t}({count})");


                         var terminal = item.FirstOrDefault();

                         var merchantProfileId = terminal?.MerchantProfileId;
                         var merchantProfile =
                             dataContext.MerchantProfiles.FirstOrDefault(x => x.Id == merchantProfileId);

                         if (merchantProfile == null)
                             continue;
                         if (merchantProfile.IsLegalPersonality)
                             continue;

                         var shebaNumber = terminal?.ShebaNo;
                         AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNumber, out var accountNumber2);
                         var primaryCustomerNumber = accountNumber2.Split('-')[2];

                         if (!TosanService.TryGetCustomerInfo(primaryCustomerNumber,
                                 merchantProfile.CustomerNumber ?? primaryCustomerNumber, out var response,
                                 out var errorMessage))
                         {
                         }

                         var incompleteCustomerInfoMessage = TosanService.GetIncompleteCustomerInfoMessage(response);
                         if (!string.IsNullOrEmpty(incompleteCustomerInfoMessage))
                         {
                             temp3.FirstOrDefault(b => b.TerminalNo == terminal.TerminalNo).InComplete = true;
                             continue;
                         }



                         if (response == null)
                             continue;

                         merchantProfile.BirthCrtfctSerial = response.certificateSerial;


                         if (response.certificateSeries != null)
                         {
                             merchantProfile.BirthCrtfctSeriesNumber = !string.IsNullOrEmpty(response.certificateSeries)
                                 ? response.certificateSeries.Split('-')[1]
                                 : null;
                             merchantProfile.PersianCharRefId = !string.IsNullOrEmpty(response.certificateSeries)
                                 ? response.certificateSeries.Split('-')[0]
                                 : null;
                         }




                         // using (var parsianService = new ParsianService())
                         // {
                         //     var result = parsianService.RequestChangeInfo(requestInqueryInput);
                         //     if (result == null || result.RequestResult.TopiarId == null)
                         //     { 
                         //            log.Input = JsonConvert.SerializeObject(requestInqueryInput);
                         //            log.Method = "RequestChangeInfo";
                         //            log.Result = JsonConvert.SerializeObject(result);
                         //            log.TerminalId = (int) terminal.Id;
                         //            log.ShebaNo = item.Key;
                         //            log.MerchantProfileId = (int) merchantProfile.Id;
                         //            log.Failed = true;
                         //            
                         //
                         //                
                         //         dataContext.SaveChanges();
                         //         
                         //     }
                         //     else
                         //     {
                         //
                         //           log.TopiarId = result.RequestResult.TopiarId;
                         //             log.Input = JsonConvert.SerializeObject(requestInqueryInput);
                         //             log.Method = "RequestChangeInfo";
                         //             log.Result = JsonConvert.SerializeObject(result);
                         //             log.TerminalId = (int) terminal.Id;
                         //             log.ShebaNo = item.Key;
                         //             log.MerchantProfileId = (int) merchantProfile.Id;
                         //
                         //
                         //        
                         //         dataContext.SaveChanges();
                         //     }
                         // } 
                     }
                     catch (Exception ex)
                     {
                         continue;
                     }
             
                 }
                 dataContext.SaveChanges();
      

             return new JsonResult( );
        }

       public ActionResult Inquiry()
        {
           
            var dataContext = new AppDataContext();
            var parsianrequest = dataContext.ParsianRequestForInfo.Where(b =>
              b.TopiarId == 7031490 && 
             !b.Failed.HasValue || !b.Failed.Value   ).ToList();
            foreach (var VARIABLE in parsianrequest)
            {
                using (var parsianService = new ParsianService())
                {
                    var requestInqueryInput = new RequestInqueryInput();
                    requestInqueryInput.RequestData = new RequestInqueryRequestData();
                    requestInqueryInput.RequestData.TopiarId =  VARIABLE.TopiarId.ToString();
                    var result = parsianService.RequestInQuery(requestInqueryInput, VARIABLE.TerminalId.Value );
                    if (result.RequestResult.RequestError != null  )
                    {
                        VARIABLE.Failed = true;
                        
                    }
                     
                }
            }

            dataContext.SaveChanges();
           
            
            return new JsonResult( );
        }
    }
}