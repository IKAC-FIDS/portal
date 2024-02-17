using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Ajax.Utilities;
using PdfSharp.Pdf.IO;
using RestSharp;
using Stimulsoft.Base.Json;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.IranKishServiceRefrence;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.ViewModels.newirankish;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using AcceptorTypes = TES.Merchant.Web.UI.IranKishServiceRefrence.AcceptorTypes;
using AccountEntity = TES.Merchant.Web.UI.ViewModels.newirankish.AccountEntity;
using Document = TES.Merchant.Web.UI.ViewModels.PardakhtNovin.Document;
using Enums = TES.Common.Enumerations;
using InqueryAcceptorResult = TES.Merchant.Web.UI.Service.Models.Irankish.InqueryAcceptorResult;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using PdfReader = PdfSharp.Pdf.IO.PdfReader;
using System.Reflection;
using static System.Net.WebRequestMethods;

namespace TES.Merchant.Web.UI.Service
{
    public class NewIranKishService : IDisposable
    {
     //   private string RestIranKishUrl = "http://192.168.10.102:5290";
        private string RestIranKishUrl = "http://127.0.0.1:5290";

        public NewIranKishService()
        {
        }

        private string getIrankishDocType(long terminalDocumentDocumentType)
        {
            switch (terminalDocumentDocumentType)
            {
                case 0:
                    return "10";
                case 1:
                    return "15";
                case 9:
                    return "15";
                case 10:
                    return "11";
                case 11:
                    return "11";

                case 13:
                    return "11";
                case 14:
                    return "11";
                case 15:
                    return "10";//=> todo 10
                case 16:
                    return "13";
                case 17:
                    return "13";
            }

            return "55";
        }

        public void Dispose()
        {
        }

        public bool AddAcceptor(long terminalId)
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalInfo = dataContext.Terminals
                    .Select(x => new
                    {
                        x.Title,
                        x.MerchantProfile.Birthdate,
                        x.AccountNo,
                        x.IsVirtualStore,
                        x.GuildId,
                        x.ShaparakAddressFormat,
                        BranchTitle = x.Branch.Title,
                        StateCode = x.City.State.Code,
                        x.CityId,
                        x.MerchantProfile.IsLegalPersonality,
                        x.MerchantProfile.FirstName,
                        x.MerchantProfile.LastName,
                        x.MerchantProfile.CompanyRegistrationDate,
                        x.ShebaNo,
                        x.MerchantProfile.LegalNationalCode,
                        x.MerchantProfile.Mobile,
                        x.MerchantProfile.NationalityId,
                        x.Tel,
                        x.MerchantProfile.NationalCode,
                        x.Id,
                        x.PostCode,
                        DeviceTypeCode = x.DeviceType.Code,
                        x.BranchId,
                        x.StatusId,
                        x.MarketerId,
                        x.MerchantProfile.IsMale,
                        x.MerchantProfile.FatherName,
                        x.Address,
                        x.MerchantProfile.CompanyRegistrationNumber,
                        x.DeviceTypeId,
                        x.ActivityTypeId,
                        x.PspId,
                        x.MerchantProfile.IdentityNumber,
                        x.MerchantProfile.BirthCrtfctSerial,
                        ParentGuildId = x.Guild.ParentId,
                        NationalityCode = x.MerchantProfile.Nationality.Code,
                        x.TelCode,
                        x.TaxPayerCode,
                        x.Email,
                        x.WebUrl,
                        x.MerchantProfile.PersianCharRefId,
                        x.MerchantProfile.BirthCrtfctSeriesNumber
                    })
                    .First(x => x.Id == terminalId);

                try
                {
                    var acceptorEntity = new AddAcceptorAnddocumentRequest
                    {
                        IdentifierNumber = terminalInfo.IdentityNumber == "0"
                            ? terminalInfo.NationalCode.Trim()
                            : terminalInfo.IdentityNumber,
                        IdentifierSerial = terminalInfo.BirthCrtfctSerial,
                        IdentifierLetterPart = "0",//old value terminalInfo.PersianCharRefId
                        IdentifierNumberPart = 0,//old value int.Parse(terminalInfo.BirthCrtfctSeriesNumber),
                        Group = 0,
                        BankId = 6830,
                        EntityType = terminalInfo.IsLegalPersonality
                            ? EntityTypes.LocalLegalAcceptor
                            : EntityTypes.LocalRealAcceptor,
                        FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                        LastName = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                        LegalEntityTitle = terminalInfo.Title,
                        LegalNationalId = terminalInfo.LegalNationalCode?.Trim(),
                        Mobile = terminalInfo.Mobile,
                        Phone = (terminalInfo.TelCode + terminalInfo.Tel).Replace("-", "").Replace(" ", ""),
                        Zipcode = terminalInfo.PostCode.Trim(),
                        AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg : AcceptorTypes.Pos,
                        Bussiness = terminalInfo.ParentGuildId.HasValue
                            ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0')
                            : string.Empty,
                        Activity = terminalInfo.GuildId.ToString().PadLeft(8, '0'),
                        Province = terminalInfo.StateCode,
                        City = terminalInfo.CityId.ToString(),
                        TerminalType = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS
                            ? "BTP"
                            : terminalInfo
                                .DeviceTypeCode, // ایرانکیش ام پوس نداره و اگر ام پوس بود باید به صورت بلوتوث فرستاده شود
                        Qty = 1,
                        IsPcPos = false,
                        TrackId = terminalInfo.Id.ToString(),
                        MerchantName = terminalInfo.Title.ApplyPersianYeKe().RemoveHamzeh(),
                        IsSwitchTerminal = false,
                        AcceptorCeoBirthdate = terminalInfo.Birthdate,
                        ENamadStatus = false,
                        TaxFollowupCode = terminalInfo.TaxPayerCode,
                        IsVirtual = terminalInfo.IsVirtualStore.HasValue ? terminalInfo.IsVirtualStore.Value : false,
                        accounts = new List<AccountEntity>()
                        {
                            new AccountEntity()
                            {
                                Bank = "6830",
                                Iban = terminalInfo.ShebaNo,
                                Branch = "0" + terminalInfo.BranchId.ToString(),
                                OwnerName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                                OwnerFamily = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                                Account   = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0')
                            }
                        },
                        Address = terminalInfo.ShaparakAddressFormat,
                        //  Branch = "0" + terminalInfo.BranchId.ToString(),
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,
                        TechEmail = terminalInfo.Email ?? "sldflsf@lkfsak.com",
                        FoundationDate = terminalInfo.CompanyRegistrationDate.ToString(),
                        // Iban = terminalInfo.ShebaNo,
                        Nationality = terminalInfo.NationalityCode,
                        RealNationalId = terminalInfo.NationalCode.Trim(),
                        LicenseNumber = terminalInfo.CompanyRegistrationNumber,
                    };

                    acceptorEntity.Documents = new List<irankishDocument>();
                    byte[] malekiyat_10 = CreatePDF2();
                    byte[] taahod_15 = CreatePDF2();
                    byte[] hoviyati_11 = CreatePDF2();
                    byte[] sherkati_13 = CreatePDF2();

                    var outputDoc_11 = PdfReader.Open(new MemoryStream(malekiyat_10), PdfDocumentOpenMode.Import);
                    var outputDoc_11_file = false;
                    var outputDoc_15 = PdfReader.Open(new MemoryStream(taahod_15), PdfDocumentOpenMode.Import);
                    var outputDoc_15_file = false;

                    var outputDoc_10 = PdfReader.Open(new MemoryStream(hoviyati_11), PdfDocumentOpenMode.Import);
                    var outputDoc_10_file = false;

                    var outputDoc_13 = PdfReader.Open(new MemoryStream(sherkati_13), PdfDocumentOpenMode.Import);
                    var outputDoc_13_file = false;



                    var t = dataContext.Terminals.FirstOrDefault(va => va.Id == terminalInfo.Id);
                    foreach (var terminalDocument in t.TerminalDocuments.DistinctBy(a => a.DocumentTypeId))
                    {
                        var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                        switch (am)
                        {
                            case "11":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {

                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_11.AddPage(pp);
                                        outputDoc_11_file = true;
                                    }
                                }
                                break;
                            // case "15":
                            //     using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                            //                PdfDocumentOpenMode.Import))
                            //     { 
                            //         //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                            //         foreach (var pp in pdfDocument.Pages)
                            //         {
                            //             outputDoc_15.AddPage(pp);
                            //             outputDoc_15_file = true;
                            //
                            //         }
                            //     }
                            //     break;
                            case "10":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_10.AddPage(pp);
                                        outputDoc_10_file = true;

                                    }
                                }
                                break;
                            case "13":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_13.AddPage(pp);
                                        outputDoc_13_file = true;
                                    }
                                }
                                break;
                        };

                    }

                    foreach (var terminalDocument in t.MerchantProfile.MerchantProfileDocuments.DistinctBy(a => a.DocumentTypeId))
                    {

                        var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                        switch (am)
                        {
                            case "11":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_11.AddPage(pp);
                                        outputDoc_11_file = true;

                                    }
                                }
                                break;
                            // case "15":
                            //     using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                            //                PdfDocumentOpenMode.Import))
                            //     { 
                            //         //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                            //         foreach (var pp in pdfDocument.Pages)
                            //         {
                            //             outputDoc_15.AddPage(pp);
                            //             outputDoc_15_file = true;
                            //
                            //         }
                            //     }
                            //     break;
                            case "10":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_10.AddPage(pp);
                                        outputDoc_10_file = true;

                                    }
                                }
                                break;
                            case "13":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_13.AddPage(pp);
                                        outputDoc_13_file = true;

                                    }
                                }
                                break;
                        };
                    }

                  //   string directoryPath = Environment.CurrentDirectory + "\\Images\\";
                     string directoryPath = "E:\\TesLog\\";
                    // directoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Images\\";
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                   

                   // string filename = Path.Combine(directoryPath + "outputDoc_11_", terminalId.ToString(), ".pdf");

                      string filename = $@"E:\TesLog\outputDoc_11_{terminalId}.pdf";
                    // string filename = $@"E:\\TesLog\\outputDoc_11_{terminalId}.pdf";
                    if (outputDoc_11.Pages.Count != 1)
                    {
                        outputDoc_11.Pages.RemoveAt(0);
                        outputDoc_11.Save(filename);
                    }

                       filename =$@"E:\TesLog\outputDoc_13_{terminalId}.pdf";
                  
                    if (outputDoc_13.Pages.Count != 1)
                    {
                        outputDoc_13.Pages.RemoveAt(0);
                        outputDoc_13.Save(filename);
                    }

                      filename =$@"E:\TesLog\outputDoc_10_{terminalId}.pdf";
                  
                    if (outputDoc_10.Pages.Count != 1)
                    {
                        outputDoc_10.Pages.RemoveAt(0);
                        outputDoc_10.Save(filename);
                    }

                    // filename = $@"E:\\zer\\outputDoc_15_{terminalId}.pdf";
                    // if (outputDoc_15.Pages.Count != 1)
                    // {
                    //     outputDoc_15.Pages.RemoveAt(0);
                    //     outputDoc_15.Save(filename);
                    // }

                    //13

                    if (outputDoc_13_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_13.Save(stream, true);
                            sherkati_13 = stream.ToArray();

                        }

                        var a13 = new AddDocumentRequest
                        {
                            //  TrackingCode = result.data.documentTrackingCode,
                            DocumentType = "13",
                            BankId = 6830,
                            File = Convert.ToBase64String(sherkati_13, 0, sherkati_13.Length)
                        };
                        if (acceptorEntity.EntityType == EntityTypes.LocalLegalAcceptor)
                        {
                            acceptorEntity.Documents.Add(new irankishDocument()
                            {
                                File = a13.File,
                                DocumentType = 13,
                            });
                        }

                    }
                    //11
                    if (outputDoc_11_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_11.Save(stream, true);
                            hoviyati_11 = stream.ToArray();

                        }

                        var a11 = new AddDocumentRequest
                        {
                            //  TrackingCode = result.data.documentTrackingCode,
                            DocumentType = "11",
                            BankId = 6830,
                            File = Convert.ToBase64String(hoviyati_11, 0, hoviyati_11.Length)
                        };
                        acceptorEntity.Documents.Add(new irankishDocument()
                        {
                            File = a11.File,
                            DocumentType = 11,
                        });
                        // using (var irankishService = new NewIranKishService())
                        // {
                        //     var k = irankishService.AddDocument(a11, (int)terminalId);
                        //     var m = k.Result;
                        // }
                    }

                    //10
                    if (outputDoc_10_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_10.Save(stream, true);
                            malekiyat_10 = stream.ToArray();

                        }

                        var a10 = new AddDocumentRequest
                        {
                            //  TrackingCode = result.data.documentTrackingCode,
                            DocumentType = "10",
                            BankId = 6830,
                            File = Convert.ToBase64String(malekiyat_10, 0, malekiyat_10.Length)
                        };

                        if (acceptorEntity.EntityType == EntityTypes.LocalRealAcceptor)
                        {
                            acceptorEntity.Documents.Add(new irankishDocument()
                            {
                                File = a10.File,
                                DocumentType = 10,
                            });
                        }
                    }
                    //
                    // //15
                    // if (outputDoc_15_file)
                    // {
                    //     using (MemoryStream stream = new MemoryStream())
                    //     {
                    //         outputDoc_15.Save(stream, true);
                    //         taahod_15 = stream.ToArray();
                    //
                    //     }
                    //
                    //     var a15 = new AddDocumentRequest
                    //     {
                    //       //  TrackingCode = result.data.documentTrackingCode,
                    //         DocumentType = "15",
                    //         BankId = 6830,
                    //         File = Convert.ToBase64String(taahod_15, 0, taahod_15.Length)
                    //     };
                    //     acceptorEntity.Documents.Add( new irankishDocument()
                    //     {
                    //         File = a15.File,
                    //         DocumentType = 15, 
                    //     });
                    //    
                    // }
                    //

                    var client = new RestClient($"{RestIranKishUrl}/api/v1/acceptors/addAcceptorAnddocument")
                    {
                        Timeout = -1
                    };
                    var request = new RestRequest(Method.POST);

                    request.AddParameter("application/json", JsonConvert.SerializeObject(acceptorEntity),
                        ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    var result = JsonConvert.DeserializeObject<NewIrankishAddAcceptorResposne>(response.Content);


                    if (result.status && result.data != null && result.data.status.ToLower() == "true")
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                    }

                    else if (result.data != null && result.data.errors.Any())
                    {
                        var errors = result.data.errors.Select(x =>
                            $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine} Code: {x.Code}{Environment.NewLine} PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                        {
                            StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                            ErrorComment = string.Join(Environment.NewLine, errors)
                        });
                    }

                    //todo ==> add irankish request
                    var irankishRequest = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(acceptorEntity),
                        Result = JsonConvert.SerializeObject(result),
                        TerminalId = (int)terminalInfo.Id,
                        Method = "_client.AddAcceptor",
                        Module = "_client.AddAcceptor",
                        psptrackingCode = result.data?.psptrackingCode,
                        documentTrackingCode = result.data?.documentTrackingCode,
                        indicator = result.data?.indicator
                    };
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();




                    return true;
                }
                catch (Exception exception)
                {
                    var exceptionType = exception.GetType();

                    if (exceptionType == typeof(EndpointNotFoundException) ||
                        exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                      //  { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                        { StatusId = (byte)Enums.TerminalStatus.New, ErrorComment = exception.Message });
                    }

                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
        }


        public GetActivitiesResponse[] IsUp()
        {
            return null;
        }

        public async Task<InqueryAcceptorResult> TryInqueryAcceptor(string terminalNo, long terminalId, byte statusId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                var result = new AcceptorInqueryResponse();
                var status = statusId;
                switch (result.TerminalStatusCode)
                {
                    case "1": // تعريف ترمينال
                        status = Enums.TerminalStatus.NotReturnedFromSwitch.ToByte();
                        break;
                    case "2": // ارسال فايل تعريف ترمينال به شاپرک
                        status = Enums.TerminalStatus.NotReturnedFromSwitch.ToByte();
                        break;
                    case "3": // تاييد  تعريف ترمينال در شاپرک
                        status = Enums.TerminalStatus.ReadyForAllocation.ToByte();
                        break;
                    case "4": // عدم تاييد تعريف ترمينال در  شاپرک
                        status = Enums.TerminalStatus.NeedToReform.ToByte();
                        break;
                    case "5": // ارسال فايل تعريف ترمينال به خدمات
                        status = Enums.TerminalStatus.ReadyForAllocation.ToByte();
                        break;
                    case "6": // تاييد تعريف ترمينال در  خدمات
                        status = Enums.TerminalStatus.ReadyForAllocation.ToByte();
                        break;
                    case "7": // عدم تاييد تعريف ترمينال در خدمات
                        status = Enums.TerminalStatus.NeedToReform.ToByte();
                        break;
                    case "8": // کد باز
                        status = Enums.TerminalStatus.ReadyForAllocation.ToByte();
                        break;
                    case "12": // نصب
                        status = Enums.TerminalStatus.Installed.ToByte();
                        break;
                    case "13": // درخواست غير فعال سازي
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "14": // ارسال فايل غير فعال سازي به شاپرک
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "15": // تاييد غير فعال سازي در شاپرک
                        status = Enums.TerminalStatus.Revoked.ToByte();
                        break;
                    case "16": // ارسال فايل غير فعال سازي به خدمات
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "17": // تاييد   غير فعال سازي در خدمات
                        status = Enums.TerminalStatus.Revoked.ToByte();
                        break;
                    case "18": // عدم تاييد غير فعال سازي در شاپرک
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "19": // عدم تاييد غير فعال سازي در خدمات
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "20": // درخواست فعال سازي مجدد
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "21": // ارسال فايل فعال سازي مجدد به شاپرک
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "22": // تاييد فعال سازي  مجدد در شاپرک
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "23": // ارسال فايل فعال سازي مجدد به خدمات
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "24": // تاييد فعال سازي مجدد در خدمات
                        status = Enums.TerminalStatus.WaitingForRevoke.ToByte();
                        break;
                    case "25": // عدم تاييد فعال سازي مجدد در شاپرک
                        status = Enums.TerminalStatus.Revoked.ToByte();
                        break;
                    case "26": // عدم تاييد فعال سازي مجدد در خدمات
                        status = Enums.TerminalStatus.Revoked.ToByte();
                        break;
                    case "27": // غير فعال
                        status = Enums.TerminalStatus.Revoked.ToByte();
                        break;
                }

                var lastAccount = result.Accounts.Any(x => x.AccountStatus == "8")
                    ? result.Accounts.Last(x => x.AccountStatus == "8")
                    : result.Accounts.Last(); // 8 فعال
                var accountNumber = lastAccount.Iban.Substring(8, 18);
                return new InqueryAcceptorResult
                {
                    TerminalNo = terminalNo,
                    IsSuccess = true,
                    StatusId = status,
                    LastUpdateTime = DateTime.Now,
                    RevokeDate = result.DisMountDate,
                    InstallationDate = result.MountDate,
                    ShebaNo = lastAccount.Iban,
                    ErrorComment = result.TerminalStatusDescription,
                    Description = result.TerminalStatusCode + " - " + result.TerminalStatusDescription,
                    AccountNo =
                        $"{accountNumber.Substring(0, 4)}-{accountNumber.Substring(4, 3)}-{accountNumber.Substring(7, 8)}-{accountNumber.Substring(15, 3)}"
                };
            }
            catch (Exception exception)
            {
                exception.AddLogData("TerminalId", terminalId).LogNoContext();
                return new InqueryAcceptorResult { IsSuccess = false };
            }
        }

        /// <summary>
        /// درخواست جمع آوری
        /// </summary>
        public async Task<SendRevokeRequestResponseModel> SendRevokeRequest(long revokeRequestId, string terminalNo,
            string description, long id)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                // عدد 72 یعنی انصراف به درخواست بانک
                var result =
                    new RefuseAcceptorResponse(); // await _client.RefuseRequestAsync(terminalNo, description, 72);

                var a = new RefuseAcceptorRequest
                {
                    ReasonId = 72,
                    Description = description,
                    TerminalNo = terminalNo
                };

                var client = new RestClient($"{RestIranKishUrl}/api/v1/acceptors/refuse")
                {
                    Timeout = -1
                };
                var request = new RestRequest(Method.POST);

                request.AddParameter("application/json", JsonConvert.SerializeObject(a),
                    ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);


                result = Newtonsoft.Json.JsonConvert.DeserializeObject<RefuseAcceptorResponse>(response.Content);
                //


                using (var dataContext = new AppDataContext())
                {
                    var result2 = JsonConvert.DeserializeObject<IrankishInqueryResponse>(response.Content);
                    var irankishRequest2 = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(a),
                        Result = JsonConvert.SerializeObject(result),
                        TerminalId = (int)id,
                        Method = "_client.refuse",
                        Module = "_client.refuse",
                        psptrackingCode = null,
                        documentTrackingCode = null,
                        indicator = null
                    };
                    dataContext.IrankishRequest.Add(irankishRequest2);
                    dataContext.SaveChanges();
                }


                if (result.status && result.Data != null)
                {
                    return new SendRevokeRequestResponseModel
                    {
                        IsSuccess = true,
                        StatusId = (byte)Enums.RequestStatus.SentToPsp,
                        Result = result.Data.message
                    };
                }


                return new SendRevokeRequestResponseModel
                {
                    IsSuccess = false,
                    StatusId = (byte)Enums.RequestStatus.WebServiceError,
                    Result = result.Data.message
                };
            }
            catch (Exception exception)
            {
                exception.AddLogData("RevokeRequestId", revokeRequestId).LogNoContext();

                return new SendRevokeRequestResponseModel
                {
                    IsSuccess = false,
                    StatusId = (byte)Enums.RequestStatus.WebServiceError,
                    Result = "خطا در اتصال به وب سرویس"
                };
            }
        }

        /// <summary>
        /// درخواست تغییر حساب
        /// </summary>
        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long id,
            string oldAccountNumber,
            string newAccountNumber,
            string newShebaNumber,
            string firstName,
            string lastName,
            string merchantNumber,
            long branchId,
            byte[] fileData, string oldShebaNo, string acceptorCode)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;


                var k = new NewChangeAccountRequest();
                k.AcceptorNo = oldAccountNumber;
                k.CurrentIban = oldShebaNo;
                k.NewAccountNo = newAccountNumber;
                k.AcceptorNo = merchantNumber;
                k.OwnerFamily = lastName;
                k.OwnerName = firstName;
                k.BranchCode = branchId.ToString();
                var base64String = Convert.ToBase64String(fileData, 0, fileData.Length);
                k.ChangeAccountDocument = base64String;

                using (var dataContext = new AppDataContext())
                {
                   // var client = new RestClient($"{RestIranKishUrl}/api/v1/accounts")
                    //var client = new RestClient($"{RestIranKishUrl}/api/v1/Account/ChangeAccountByIban")
                   // var client = new RestClient($"{RestIranKishUrl}/api/Account/ChangeAccountByIban/")
                  //  var client = new RestClient($"{RestIranKishUrl}/api/v1/Accounts")
                    var client = new RestClient("http://127.0.0.1:5290/api/v1/accounts")
                    {
                        Timeout = -1
                    };
                 //   var request = new RestRequest(Method.PATCH);
                    var request = new RestRequest(Method.PATCH);

                    request.AddParameter("application/json", JsonConvert.SerializeObject(k),
                        ParameterType.RequestBody);
                    
                    IRestResponse response = client.Execute(request);

                    var result =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<ChangeAccountByIbanResponse>(response.Content);
                    var irankishRequest = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(k),
                        Result = JsonConvert.SerializeObject(response.Content),
                        TerminalId = (int)id,
                        Method = "_client.ChangeAccount",
                        Module = "_client.ChangeAccount",
                        psptrackingCode = null,
                        documentTrackingCode = null,
                        indicator = null
                    };
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();

                    if(result!=null)
                    {
                        if (result != null || result.Status && (result.Description == "درخواست با موفقیت ثبت شد." ||
                                         result.Description == "درخواست تغییر حساب قبلا ثبت شده است."))
                        {
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = true,
                                StatusId = Enums.RequestStatus.SentToPsp.ToByte(),
                                Result = result.Description
                            };
                        }
                    }
                    else
                    {
                        throw new Exception("خطا در اتصال به وب سرویس.");
                    }
                   
                   

                    return new SendChangeAccountRequestResponseModel
                    {
                        IsSuccess = false,
                        StatusId = Enums.RequestStatus.NeedToReform.ToByte(),
                        Result = result.Description
                    };
                }
            }
            catch (Exception exception)
            {
                exception.AddLogData("TerminalId", id).LogNoContext();

                return new SendChangeAccountRequestResponseModel
                {
                    IsSuccess = false,
                    StatusId = Enums.RequestStatus.WebServiceError.ToByte(),
                    Result = "خطا در اتصال به وب سرویس."
                };
            }
        }

        public bool EditAcceptor(long terminalId)
        {
            Console.WriteLine($"start ====>{DateTime.Now}");
            var inqueryResult = Inquery(terminalId.ToString());

            Console.WriteLine($"End ====>{DateTime.Now}");

            if (!inqueryResult.status)
            {
                return AddAcceptor(terminalId);
            }

            using (var dataContext = new AppDataContext())
            {
                var terminalInfo = dataContext.Terminals
                    .Select(x => new
                    {
                        x.Title,
                        x.MerchantProfile.Birthdate,
                        x.AccountNo,
                        x.GuildId,
                        x.ShaparakAddressFormat,
                        BranchTitle = x.Branch.Title,
                        StateCode = x.City.State.Code,
                        x.CityId,
                        x.MerchantProfile.IsLegalPersonality,
                        x.MerchantProfile.FirstName,
                        x.MerchantProfile.LastName,
                        x.MerchantProfile.CompanyRegistrationDate,
                        x.ShebaNo,
                        x.MerchantProfile.LegalNationalCode,
                        x.MerchantProfile.Mobile,
                        x.MerchantProfile.NationalityId,
                        x.Tel,
                        x.MerchantProfile.NationalCode,
                        x.Id,
                        x.PostCode,
                        DeviceTypeCode = x.DeviceType.Code,
                        x.BranchId,
                        x.StatusId,
                        x.MarketerId,
                        x.MerchantProfile.IsMale,
                        x.MerchantProfile.FatherName,
                        x.Address,
                        x.MerchantProfile.CompanyRegistrationNumber,
                        x.DeviceTypeId,
                        x.ActivityTypeId,
                        x.PspId,
                        ParentGuildId = x.Guild.ParentId,
                        NationalityCode = x.MerchantProfile.Nationality.Code,
                        x.TelCode,
                        x.TerminalDocuments,
                        x.MerchantProfile,
                        x.MerchantNo,
                        x.Email,
                        x.WebUrl,
                    })
                    .First(x => x.Id == terminalId);

                try
                {





                    byte[] malekiyat_10 = CreatePDF2();
                    byte[] taahod_15 = CreatePDF2();
                    byte[] hoviyati_11 = CreatePDF2();
                    byte[] sherkati_13 = CreatePDF2();

                    var outputDoc_11 = PdfReader.Open(new MemoryStream(malekiyat_10), PdfDocumentOpenMode.Import);
                    var outputDoc_11_file = false;
                    var outputDoc_15 = PdfReader.Open(new MemoryStream(taahod_15), PdfDocumentOpenMode.Import);
                    var outputDoc_15_file = false;

                    var outputDoc_10 = PdfReader.Open(new MemoryStream(hoviyati_11), PdfDocumentOpenMode.Import);
                    var outputDoc_10_file = false;

                    var outputDoc_13 = PdfReader.Open(new MemoryStream(sherkati_13), PdfDocumentOpenMode.Import);
                    var outputDoc_13_file = false;




                    foreach (var terminalDocument in terminalInfo.TerminalDocuments)
                    {
                        var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                        switch (am)
                        {
                            case "11":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {

                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_11.AddPage(pp);
                                        outputDoc_11_file = true;
                                    }
                                }
                                break;

                            case "10":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_10.AddPage(pp);
                                        outputDoc_10_file = true;

                                    }
                                }
                                break;
                            case "13":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_13.AddPage(pp);
                                        outputDoc_13_file = true;
                                    }
                                }
                                break;
                        };

                    }

                    foreach (var terminalDocument in terminalInfo.MerchantProfile.MerchantProfileDocuments)
                    {

                        var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                        switch (am)
                        {
                            case "11":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_11.AddPage(pp);
                                        outputDoc_11_file = true;

                                    }
                                }
                                break;

                            case "10":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_10.AddPage(pp);
                                        outputDoc_10_file = true;

                                    }
                                }
                                break;
                            case "13":
                                using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                           PdfDocumentOpenMode.Import))
                                {
                                    //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                    foreach (var pp in pdfDocument.Pages)
                                    {
                                        outputDoc_13.AddPage(pp);
                                        outputDoc_13_file = true;

                                    }
                                }
                                break;
                        };
                    }


                    string filename = $@"E:\\TesLog\\outputDoc_11_{terminalId}.pdf";
                    if (outputDoc_11.Pages.Count != 1)
                    {
                        outputDoc_11.Pages.RemoveAt(0);
                        outputDoc_11.Save(filename);
                    }

                    filename = $@"E:\\TesLog\\outputDoc_13_{terminalId}.pdf";
                    if (outputDoc_13.Pages.Count != 1)
                    {
                        outputDoc_13.Pages.RemoveAt(0);
                        outputDoc_13.Save(filename);
                    }

                    filename = $@"E:\\TesLog\\outputDoc_10_{terminalId}.pdf";
                    if (outputDoc_10.Pages.Count != 1)
                    {
                        outputDoc_10.Pages.RemoveAt(0);
                        outputDoc_10.Save(filename);
                    }



                    if (outputDoc_13_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_13.Save(stream, true);
                            sherkati_13 = stream.ToArray();

                        }

                        var a13 = new AddDocumentRequest
                        {
                            TrackingCode = terminalId.ToString(),
                            DocumentType = "13",
                            BankId = 6830,
                            File = Convert.ToBase64String(sherkati_13, 0, sherkati_13.Length)
                        };
                        if (terminalInfo.MerchantProfile.IsLegalPersonality)
                        {

                            using (var irankishService = new NewIranKishService())
                            {
                                var k = irankishService.AddDocument(a13, (int)terminalId);
                                var m = k.Result;
                            }
                        }

                    }
                    //11
                    if (outputDoc_11_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_11.Save(stream, true);
                            hoviyati_11 = stream.ToArray();

                        }

                        var a11 = new AddDocumentRequest
                        {
                            TrackingCode = terminalId.ToString(),
                            DocumentType = "11",
                            BankId = 6830,
                            File = Convert.ToBase64String(hoviyati_11, 0, hoviyati_11.Length)
                        };



                        using (var irankishService = new NewIranKishService())
                        {
                            var k = irankishService.AddDocument(a11, (int)terminalId);
                            var m = k.Result;
                        }


                    }

                    //10
                    if (outputDoc_10_file)
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            outputDoc_10.Save(stream, true);
                            malekiyat_10 = stream.ToArray();

                        }

                        var a10 = new AddDocumentRequest
                        {
                            TrackingCode = terminalId.ToString(),
                            DocumentType = "10",
                            BankId = 6830,
                            File = Convert.ToBase64String(malekiyat_10, 0, malekiyat_10.Length)
                        };

                        if (!terminalInfo.MerchantProfile.IsLegalPersonality)
                        {

                            using (var irankishService = new NewIranKishService())
                            {
                                var k = irankishService.AddDocument(a10, (int)terminalId);
                                var m = k.Result;
                            }
                        }
                    }

                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                        new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });

                    return true;

                    var editedAcceptorEntity = new UpdateAcceptorRequest
                    {
                        AcceptorNo = terminalInfo.MerchantNo,
                        AcceptorCeoBirthdate = terminalInfo.Birthdate.ToString(),
                        AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg : AcceptorTypes.Pos,
                        TrackId = terminalInfo.Id.ToString(),
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,
                        TechEmail = terminalInfo.Email,
                        BankId = 6830,
                        Activity = terminalInfo.GuildId.ToString(),
                        Address = terminalInfo.ShaparakAddressFormat,
                        Bussiness = terminalInfo.ParentGuildId.HasValue
                            ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0')
                            : string.Empty,
                        City = terminalInfo.CityId.ToString(),
                        EntityType = terminalInfo.IsLegalPersonality
                            ? EntityTypes.LocalLegalAcceptor
                            : EntityTypes.LocalRealAcceptor,
                        FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                        FoundationDate = terminalInfo.CompanyRegistrationDate.ToString(),
                        LastName = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                        LegalEntityTitle = terminalInfo.Title,
                        LegalNationalId = terminalInfo.LegalNationalCode?.Trim(),
                        MerchantName = terminalInfo.Title.ApplyPersianYeKe(),
                        Mobile = terminalInfo.Mobile,
                        Nationality = terminalInfo.NationalityCode,
                        Phone = (terminalInfo.TelCode + terminalInfo.Tel).Replace("-", "").Replace(" ", ""),
                        Province = terminalInfo.StateCode,
                        RealNationalId = terminalInfo.NationalCode.Trim(),
                        Zipcode = terminalInfo.PostCode,
                        LicenseNumber = terminalInfo.CompanyRegistrationNumber
                    };


                    var client = new RestClient($"{RestIranKishUrl}/api/v1/acceptors/update")
                    {
                        Timeout = -1
                    };
                    var request = new RestRequest(Method.POST);

                    request.AddParameter("application/json", JsonConvert.SerializeObject(editedAcceptorEntity),
                        ParameterType.RequestBody);
                    IRestResponse response = client.Execute(request);
                    var result = JsonConvert.DeserializeObject<IrankishEditAcceptorResponse>(response.Content);


                    if (result.status && result.data != null)
                    {
                        if (result.data.status.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                                new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });

                            return true;
                        }

                        var errors = result.data.errors.Select(x =>
                            $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                        {
                            StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                            ErrorComment = string.Join(Environment.NewLine, errors)
                        });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { ErrorComment = "خطا در وب سرویس" });
                    }

                    var irankishRequest = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(editedAcceptorEntity),
                        Result = JsonConvert.SerializeObject(result),
                        TerminalId = (int)terminalInfo.Id,
                        Method = "_client.EditAcceptor",
                        Module = "_client.EditAcceptor"
                    };
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();
                    return true;
                }
                catch (Exception exception)
                {
                    var exceptionType = exception.GetType();

                    if (exceptionType == typeof(EndpointNotFoundException) ||
                        exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                        { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                    }

                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
        }


        public bool EditAcceptorRequest(long terminalId)
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalInfo = dataContext.Terminals
                    .Select(x => new
                    {
                        x.Title,
                        x.MerchantProfile.Birthdate,
                        x.AccountNo,
                        x.GuildId,
                        x.ShaparakAddressFormat,
                        BranchTitle = x.Branch.Title,
                        StateCode = x.City.State.Code,
                        x.CityId,
                        x.MerchantProfile.IsLegalPersonality,
                        x.MerchantProfile.FirstName,
                        x.MerchantProfile.LastName,
                        x.MerchantProfile.CompanyRegistrationDate,
                        x.ShebaNo,
                        x.MerchantProfile.LegalNationalCode,
                        x.MerchantProfile.Mobile,
                        x.MerchantProfile.NationalityId,
                        x.Tel,
                        x.MerchantProfile.NationalCode,
                        x.Id,
                        x.PostCode,
                        DeviceTypeCode = x.DeviceType.Code,
                        x.BranchId,
                        x.StatusId,
                        x.MarketerId,
                        x.MerchantProfile.IsMale,
                        x.MerchantProfile.FatherName,
                        x.Address,
                        x.MerchantProfile.CompanyRegistrationNumber,
                        x.DeviceTypeId,
                        x.ActivityTypeId,
                        x.PspId,
                        x.MerchantProfile.IdentityNumber,
                        x.MerchantProfile.BirthCrtfctSerial,
                        ParentGuildId = x.Guild.ParentId,
                        NationalityCode = x.MerchantProfile.Nationality.Code,
                        x.TelCode,
                        x.TaxPayerCode,
                        x.Email,
                        x.WebUrl,
                    })
                    .First(x => x.Id == terminalId);

                try
                {
                    var acceptorEntity = new EditedAcceptorRequestEntity
                    {
                        AcceptorCeoBirthdate = terminalInfo.Birthdate,
                        //    AcceptorType = AcceptorTypes.Pos,

                        //شماره شناسنامه
                        IdentifierNumber = terminalInfo.IdentityNumber == "0"
                            ? terminalInfo.NationalCode.Trim()
                            : terminalInfo.IdentityNumber,

                        TechEmail = terminalInfo.Email,
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,

                        Account = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0'),
                        Activity = terminalInfo.GuildId.ToString().PadLeft(8, '0'),
                        Address = terminalInfo.ShaparakAddressFormat,
                        //    Branch = terminalInfo.BranchId.ToString(),
                        Bussiness = terminalInfo.ParentGuildId.HasValue
                            ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0')
                            : string.Empty,
                        City = terminalInfo.CityId.ToString(),

                        //  EntityType= terminalInfo.IsLegalPersonality ? EntityTypes.LocalLegalAcceptor : EntityTypes.LocalRealAcceptor,
                        FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                        FoundationDate = terminalInfo.CompanyRegistrationDate,
                        Iban = terminalInfo.ShebaNo,
                        IsPcPos = false,
                        IsSwitchTerminal = false,
                        LastName = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                        LegalEntityTitle = terminalInfo.Title,
                        LegalNationalId = terminalInfo.LegalNationalCode?.Trim(),
                        MerchantName = terminalInfo.Title.ApplyPersianYeKe(),
                        Mobile = terminalInfo.Mobile,
                        Nationality = terminalInfo.NationalityCode,
                        Phone = (terminalInfo.TelCode + terminalInfo.Tel).Replace("-", "").Replace(" ", ""),
                        Province = terminalInfo.StateCode,
                        Qty = 1,
                        RealNationalId = terminalInfo.NationalCode.Trim(),
                        //  TerminalType = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS ? "BTP" : terminalInfo.DeviceTypeCode, // ایرانکیش ام پوس نداره و اگر ام پوس بود باید به صورت بلوتوث فرستاده شود
                        TrackId = terminalInfo.Id.ToString(),
                        Zipcode = terminalInfo.PostCode.Trim(),
                        //  TaxFollowupCode   =   terminalInfo.TaxPayerCode,
                        licenseNumber = terminalInfo.CompanyRegistrationNumber
                    };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                    var result = new EditAcceptorresponse(); // _client.EditAcceptorRequest(acceptorEntity);

                    var tt = JsonConvert.SerializeObject(acceptorEntity);
                    if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        return true;
                    }

                    var errors = result.Errors.Select(x =>
                        $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                    {
                        StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                        ErrorComment = string.Join(Environment.NewLine, errors)
                    });

                    return true;
                }
                catch (Exception exception)
                {
                    var exceptionType = exception.GetType();

                    if (exceptionType == typeof(EndpointNotFoundException) ||
                        exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x =>
                            new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                        { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                    }

                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
        }

        public void AddAcceptorList(List<long> terminalIdList)
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalInfoList = dataContext.Terminals
                    .Select(x => new
                    {
                        x.Title,
                        x.MerchantProfile.Birthdate,
                        x.AccountNo,
                        x.GuildId,
                        x.ShaparakAddressFormat,
                        BranchTitle = x.Branch.Title,
                        StateCode = x.City.State.Code,
                        x.CityId,
                        x.MerchantProfile.IsLegalPersonality,
                        x.MerchantProfile.FirstName,
                        x.MerchantProfile.LastName,
                        x.MerchantProfile.CompanyRegistrationDate,
                        x.ShebaNo,
                        x.MerchantProfile.LegalNationalCode,
                        x.MerchantProfile.Mobile,
                        x.MerchantProfile.NationalityId,
                        x.Tel,
                        x.MerchantProfile.NationalCode,
                        x.Id,
                        x.PostCode,
                        DeviceTypeCode = x.DeviceType.Code,
                        x.BranchId,
                        x.StatusId,
                        x.MarketerId,
                        x.MerchantProfile.IsMale,
                        x.MerchantProfile.FatherName,
                        x.Address,
                        x.MerchantProfile.CompanyRegistrationNumber,
                        x.DeviceTypeId,
                        x.ActivityTypeId,
                        x.PspId,
                        ParentGuildId = x.Guild.ParentId,
                        NationalityCode = x.MerchantProfile.Nationality.Code,
                        x.TelCode,
                        x.Email,
                        x.WebUrl,
                        x.IsVirtualStore
                    })
                    .Where(x => terminalIdList.Contains(x.Id) && x.StatusId == (byte)Enums.TerminalStatus.New &&
                                x.PspId == (byte)Enums.PspCompany.IranKish)
                    .ToList();

                foreach (var terminalInfo in terminalInfoList)
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                               SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                        var result = new AddAcceptorresponse();
                        // _client.AddAcceptor(new AcceptorEntity
                        // {
                        //     AcceptorCeoBirthdate = terminalInfo.Birthdate,
                        //     AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg : AcceptorTypes.Pos,
                        //     Account = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0'),
                        //     Activity = terminalInfo.GuildId.ToString(),
                        //     Address = terminalInfo.ShaparakAddressFormat,
                        //     Branch = terminalInfo.BranchId.ToString(),
                        //     Bussiness = terminalInfo.ParentGuildId.HasValue
                        //         ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0')
                        //         : string.Empty,
                        //     City = terminalInfo.CityId.ToString(),
                        //     Email = terminalInfo.Email,
                        //     WebUrl = terminalInfo.WebUrl,
                        //     IsVirtual = terminalInfo.IsVirtualStore,
                        //
                        //     TechEmail = terminalInfo.Email,
                        //     EntityType = terminalInfo.IsLegalPersonality
                        //         ? EntityTypes.LocalLegalAcceptor
                        //         : EntityTypes.LocalRealAcceptor,
                        //     FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                        //     FoundationDate = terminalInfo.CompanyRegistrationDate,
                        //     Iban = terminalInfo.ShebaNo,
                        //     IsPcPos = false,
                        //     IsSwitchTerminal = false,
                        //     LastName = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                        //     LegalEntityTitle = terminalInfo.Title,
                        //     LegalNationalId = terminalInfo.LegalNationalCode.Trim(),
                        //     MerchantName = terminalInfo.Title.ApplyPersianYeKe(),
                        //     Mobile = terminalInfo.Mobile,
                        //     Nationality = terminalInfo.NationalityCode,
                        //     Phone = (terminalInfo.TelCode + terminalInfo.Tel).Replace("-", "").Replace(" ", ""),
                        //     Province = terminalInfo.StateCode,
                        //     Qty = 1,
                        //     RealNationalId = terminalInfo.NationalCode.Trim(),
                        //     TerminalType = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS
                        //         ? "BTP"
                        //         : terminalInfo.DeviceTypeCode,
                        //     TrackId = terminalInfo.Id.ToString(),
                        //     Zipcode = terminalInfo.PostCode,
                        //     licenseNumber = terminalInfo.CompanyRegistrationNumber,
                        // });

                        if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x =>
                                new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        }
                        else
                        {
                            var errors = result.Errors.Select(x =>
                                $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                            {
                                StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                                ErrorComment = string.Join(Environment.NewLine, errors)
                            });
                        }
                    }
                    catch (Exception exception)
                    {
                        var exceptionType = exception.GetType();

                        if (exceptionType == typeof(EndpointNotFoundException) ||
                            exceptionType == typeof(TimeoutException) ||
                            exceptionType == typeof(CommunicationException))
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                            {
                                StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                                ErrorComment = "خطا در برقراری ارتباط با وب سرویس"
                            });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                            {
                                StatusId = (byte)Enums.TerminalStatus.NeedToReform,
                                ErrorComment = exception.Message
                            });
                        }

                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            }
        }

        public AccountInquiryResponse AccountInquiry(string termiknalId)
        {
            using (var dataContext = new AppDataContext())
            {
                var body = new
                {
                    TrackingCode = termiknalId,
                    BankId = 6830
                };

                HttpClient client = new HttpClient();


                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{RestIranKishUrl}/api/v1/inquiry/account"),
                    Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"),
                };

                var response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                var responseBody = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var result = JsonConvert.DeserializeObject<AccountInquiryResponse>(responseBody);
                var irankishRequest = new IrankishRequest
                {
                    Input = JsonConvert.SerializeObject(body),
                    Result = JsonConvert.SerializeObject(result),
                    TerminalId = int.Parse(termiknalId),
                    Method = "_client.AccountInquiry",
                    Module = "_client.AccountInquiry",
                    psptrackingCode = null,
                    documentTrackingCode = null,
                    indicator = null
                };
                dataContext.IrankishRequest.Add(irankishRequest);
                dataContext.SaveChanges();
                return result;
            }

            return new AccountInquiryResponse(); // _client.Inquery(trackId, 6830);
        }

        public IrankishInqueryResponse Inquery(string terminalId)
        {
            using (var dataContext = new AppDataContext())
            {
                // var k = dataContext.IrankishRequest.FirstOrDefault(a =>
                //     a.TerminalId.ToString() == terminalId && a.Method == "_client.AddAcceptor");
                if (true)
                {
                    var body = new
                    {
                        trackId = terminalId,
                        BankId = 6830
                    };


                    var client2 = new RestClient("https://mmsnew.irankish.com/api/Authentication/authenticate");
                    client2.Timeout = -1;
                    var request2 = new RestRequest(Method.POST);
                    request2.AddHeader("Content-Type", "application/json");
                    request2.AddHeader("Cookie",
                        "iTunesLib=!ixGa+xYBcgFdjknxwt9Facj0Tk4nz0rNs1+PAL2blUH0xDh64wx1yzk/dsEd3oAIFRtzh8ms/sVsVOY=");
                    request2.AddParameter("application/json",
                        "{\r\n    \"username\": \"sarmayeh\",\r\n    \"password\" : \"67c2d5\"\r\n}",
                        ParameterType.RequestBody);
                    IRestResponse response2 = client2.Execute(request2);
                    var au = JsonConvert.DeserializeObject<AuthenTicationResponse>(response2.Content);
                    if(au==null || !au.status || string.IsNullOrEmpty(au.data.jwtToken)) throw new Exception();
                    //if (!au.status || string.IsNullOrEmpty(au.data.jwtToken))
                    //    throw new Exception();
                    ///


                    var client =
                        new RestClient(
                            $"https://mmsnew.irankish.com/api/Information/Inquiry?trackId={terminalId}&bankId=6830");
                    client.Timeout = -1;
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", $"Bearer {au.data.jwtToken}");
                    request.AddHeader("Cookie",
                        "iTunesLib=!ixGa+xYBcgFdjknxwt9Facj0Tk4nz0rNs1+PAL2blUH0xDh64wx1yzk/dsEd3oAIFRtzh8ms/sVsVOY=");
                    IRestResponse response = client.Execute(request);
                    Console.WriteLine(response.Content);

                    var result = JsonConvert.DeserializeObject<IrankishInqueryResponse>(response.Content);
                    var irankishRequest = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(body),
                        Result = JsonConvert.SerializeObject(result),
                        TerminalId = int.Parse(terminalId),
                        Method = "_client.inquiry",
                        Module = "_client.inquiry",
                        psptrackingCode = null,
                        documentTrackingCode = result.data?.documentTrackingCode.ToString(),
                        indicator = result.data?.indicator
                    };
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();
                    return result;
                }
            }

            return new IrankishInqueryResponse(); // _client.Inquery(trackId, 6830);
        }

        public async Task<AddDocumentresponse> AddDocument(AddDocumentRequest documentEntity, int longId)
        {
            using (var dataContext = new AppDataContext())
            {
                var client = new RestClient($"{RestIranKishUrl}/api/v1/document")
                {
                    Timeout = -1
                };
                var request = new RestRequest(Method.POST);

                request.AddParameter("application/json", JsonConvert.SerializeObject(documentEntity),
                    ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                var irankishRequest = new IrankishRequest
                {
                    Input = JsonConvert.SerializeObject(documentEntity),
                    Result = JsonConvert.SerializeObject(response.Content),
                    TerminalId = longId,
                    Method = "_client.AddDocument",
                    Module = "_client.AddDocument",
                    psptrackingCode = null,
                    documentTrackingCode = null,
                    indicator = null
                };
                dataContext.IrankishRequest.Add(irankishRequest);
                dataContext.SaveChanges();

                var result = JsonConvert.DeserializeObject<AddDocumentresponse>(response.Content);


                return result;
            }
        }


        private byte[] CreatePDF2()
        {
            iTextSharp.text.Document doc = new iTextSharp.text.Document(PageSize.LETTER, 50, 50, 50, 50);

            using (MemoryStream output = new MemoryStream())
            {
                iTextSharp.text.pdf.PdfWriter wri = iTextSharp.text.pdf.PdfWriter.GetInstance(doc, output);
                doc.Open();

                var header = new Paragraph(" ") { Alignment = Element.ALIGN_CENTER };
                var paragraph = new Paragraph("   ");
                var phrase = new Phrase(" ");
                var chunk = new Chunk(" ");


                doc.Add(header);
                doc.Add(paragraph);
                doc.Add(phrase);
                doc.Add(chunk);

                doc.Close();
                return output.ToArray();
            }

        }


    }
}