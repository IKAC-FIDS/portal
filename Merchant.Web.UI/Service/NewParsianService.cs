using EntityFramework.Extensions;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Utilities;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ParsianServiceReference;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Parsian;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;
using Terminal = TES.Data.Domain.Terminal;

namespace TES.Merchant.Web.UI.Service
{
    public class ParsianService : IDisposable
    {
        private readonly PosRequestServiceClient _client;

        public ParsianService()
        {
            _client = new PosRequestServiceClient();
            if (_client.ClientCredentials == null) return;
            _client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode =
                System.ServiceModel.Security.X509CertificateValidationMode.None;
            _client.ClientCredentials.UserName.UserName = "sarmayehbank";
            _client.ClientCredentials.UserName.Password = "Sb@123456";
        }

        #region NewMethods

        private const string BaseAddress = "https://TopiarService.Pec.ir";

        public string Maraz()
        {
            try
            {
                var reservationList = new LoginOutput();

                var input = new LoginRequestData
                {
                    Secret = "S@Rmay3", TokenDuration = 6000, UserName = "Service_sarmaye"
                };
                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                        "application/json");

                    ServicePointManager.ServerCertificateValidationCallback =
                        delegate(
                            object s,
                            X509Certificate certificate,
                            X509Chain chain,
                            SslPolicyErrors sslPolicyErrors
                        )
                        {
                            return true;
                        };
                    using (var response = httpClient.PostAsync(BaseAddress + "/UserManagement/Login", content).Result)
                    {
                        var apiResponse = response.Content.ReadAsStringAsync().Result;
                        reservationList = JsonConvert.DeserializeObject<LoginOutput>(apiResponse);
                    }
                }


                return "OK";
            }
            catch (Exception ex)

            {
                if (ex.InnerException != null)
                    if (ex.InnerException.InnerException != null)
                        return ex.InnerException.InnerException.Message;
                return ex.StackTrace;
            }
        }

        public LoginOutput Login()
        {
            var reservationList = new LoginOutput();

            var input = new LoginRequestData
            {
                Secret = "S@Rmay3", TokenDuration = 6000, UserName = "Service_sarmaye"
            };
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                ServicePointManager.SecurityProtocol =  SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                        SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;;
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate
                    {
                        return true;
                    };
                using (var response = httpClient.PostAsync(BaseAddress + "/UserManagement/Login", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<LoginOutput>(apiResponse);
                }
            }


            return reservationList;
        }


        public bool NewAddAcceptor(long terminalId, List<UploadAttachmentRequestData> files       )
        {
            using (var dataContext = new AppDataContext())
            {
                var aa = dataContext.Terminals
                    .FirstOrDefault(x => x.Id == terminalId &&
                                         (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                          x.StatusId == (byte) Enums.TerminalStatus.NeedToReform) &&
                                         x.PspId == (byte) Enums.PspCompany.Parsian);

                var terminalInfo = dataContext.Terminals
                    .Where(x => x.Id == terminalId &&
                                (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                 x.StatusId == (byte) Enums.TerminalStatus.NeedToReform) &&
                                x.PspId == (byte) Enums.PspCompany.Parsian)
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        HomeTel = x.MerchantProfile.HomeTel,
                        Title = x.Title,
                        CityId = x.CityId,
                        GuildId = x.GuildId,
                        ShebaNo = x.ShebaNo,
                        TelCode = x.TelCode,
                        Address = x.Address,
                        PostCode = x.PostCode,

                        BranchId = x.BranchId,
                        StatusId = x.StatusId,
                        AccountNo = x.AccountNo,
                        StateId = x.City.StateId,
                        MarketerId = x.MarketerId,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        Mobile = x.MerchantProfile.Mobile,
                        ActivityTypeId = x.ActivityTypeId,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        IsMale = x.MerchantProfile.IsMale,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        HomePostCode = x.MerchantProfile.HomePostCode,
                        HomeAddress = x.MerchantProfile.HomeAddress,
                        BirthCrtfctSeriesNumber = x.MerchantProfile.BirthCrtfctSeriesNumber,
                        BirthCrtfctSerial = x.MerchantProfile.BirthCrtfctSerial,
                        PersianCharRefId = x.MerchantProfile.PersianCharRefId,
                        NationalCode = x.MerchantProfile.NationalCode.Replace(" ", "").Substring(0, 10),
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        RegionalMunicipalityId = x.RegionalMunicipalityId,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode.Replace(" ", ""),
                        SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                        ParentGuildId = x.Guild.ParentId,
                        TaxPayerCode = x.TaxPayerCode,
                        TopiarId = x.TopiarId
                    })
                    .First();


                try
                {
                    if (terminalInfo.IsLegalPersonality)
                    {
                        var input = new RequestTerminalّForCompanyInput
                        {
                            RequestData = new RequestTerminalForCompanyRequest {Request = new Request()}
                        };

                        input.RequestData.Request = new Request
                        {
                            TerminalCount = 1,
                            TermTypeRefId = TermTypeRefId.Physical,
                            RequestTerminalModelRefId =
                                (terminalInfo.DeviceTypeId == 1 || terminalInfo.DeviceTypeId == 2)
                                    ? TerminalModel.Dialup
                                    : ((terminalInfo.DeviceTypeId == 3 || terminalInfo.DeviceTypeId == 8)
                                        ? TerminalModel.GRPRS
                                        : TerminalModel.MobilePose),
                            PersonTypeRefId = PersonType.hoghoghiIrani
                        };

                        

                        input.RequestData.Company = new Company()
                        {
                            NationalId = terminalInfo.LegalNationalCode,
                            CompanyName = terminalInfo.Title,
                            CompanyNameEn = terminalInfo.EnglishTitle,
                            CompanyRegisterNo = terminalInfo.CompanyRegistrationNumber,
                            WorkPhone = terminalInfo.Tel,
                            WorkPostCode = terminalInfo.PostCode,
                            WorkAddress = terminalInfo.Address,
                        };
                        if (terminalInfo.CompanyRegistrationDate != null)
                            input.RequestData.Company.ComapnyFoundationDateDt =
                                terminalInfo.CompanyRegistrationDate.Value.ToString("yyyy-MM-dd");
                        input.RequestData.CompanyOwners = new List<CompanyOwners>();

                        input.RequestData.CompanyOwners.Add(new CompanyOwners()
                        {
                            NationalCode = terminalInfo.NationalCode,
                            Fname = terminalInfo.FirstName,
                            Lname = terminalInfo.LastName,
                            PositionId = 31372,
                            BirthDateDt = terminalInfo.Birthdate.ToString("yyyy-MM-dd"),
                            FnameEng = terminalInfo.EnglishFirstName,
                            LnameEng = terminalInfo.EnglishLastName,
                            FatherName = terminalInfo.FatherName,
                            FatherNameEn = terminalInfo.EnglishFatherName,
                        });

                        input.RequestData.Shop = new Shop
                        {
                            ShopName = terminalInfo.Title,
                            ShopNameEng = terminalInfo.EnglishTitle,
                            ShopSubMccRefId = (int) terminalInfo.GuildId,
                            ShopCityRefId = (int) terminalInfo.CityId,
                            ShopPostalCode = terminalInfo.PostCode,
                            ShopAddress = terminalInfo.Address,
                            ShopPhone = terminalInfo.Tel,
                            ShopMobNo = terminalInfo.Mobile,
                            ShopEmailAddress = "",
                            WebAddress = "",
                            WebIp = "",
                            WebPort = "",
                            WebNamadType = "",
                            WebNamadRegDateDt = "",
                            WebNamadExpDateDt = "",
                            TaxPayerCode = terminalInfo.TaxPayerCode,
                        };
                        if (terminalInfo.RegionalMunicipalityId != null)
                            input.RequestData.Shop.ShopRegionRefId = (ShopRegion) terminalInfo.RegionalMunicipalityId;

                        input.RequestData.Settlements = new Settlements
                        {
                            Iban = terminalInfo.ShebaNo, IsMainAccount = true
                        };


                        var Files = files.Select(b => new UploadAttachmentRequestData
                        {
                            ContentType = b.ContentType,
                            FileName = b.FileName,
                            Base64 = b.Base64
                        }).ToList();
                    
                        var requestTerminalّForCompany = RequestTerminalّForCompany(input,Files, (int) terminalId);

                        if (requestTerminalّForCompany != null)
                        {
                            if (requestTerminalّForCompany.IsSuccess)
                            {
                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    TopiarId = requestTerminalّForCompany.RequestResult.TopiarId.Value,
                                    StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,
                                    NewParsian = true,
                                    Description = $"{DateTime.Now.ToLongPersianDateTime()}  "
                                });
                                dataContext.SaveChanges();
                            }
                            else
                            {
                                if (requestTerminalّForCompany.ErrorList.Any())
                                {
                                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                    {
                                        StatusId = (byte) Enums.TerminalStatus.NeedToReform,

                                        Description = string.Join(",",
                                            requestTerminalّForCompany.ErrorList.Select(b =>
                                                b.ErrorId + " " + b.ErrorText)),
                                        ErrorComment = string.Join(",",
                                            requestTerminalّForCompany.ErrorList.Select(b => b.ErrorId + " " + b.ErrorText))
                                    });
                                    dataContext.SaveChanges();
                                }
                                else
                                {
                                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                    {
                                        StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,

                                        Description = string.Join(",",
                                            requestTerminalّForCompany.ErrorList.Select(b =>
                                                b.ErrorId + " " + b.ErrorText)),
                                        ErrorComment = string.Join(",",
                                            requestTerminalّForCompany.ErrorList.Select(b => b.ErrorId + " " + b.ErrorText))
                                    });
                                    dataContext.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            if (requestTerminalّForCompany.ErrorList.Any(b => b.ErrorId == "-1") &&
                                terminalInfo.TopiarId != null)
                            {
                                var s = UpdateStatusForRequestedTerminal(terminalInfo.TopiarId.Value.ToString(),
                                    (int) terminalId).Result;

                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,
                                    ErrorComment = s.Error, // requestTerminalّForCompany.Desc,

                                    NewParsian = true,
                                });
                                dataContext.SaveChanges();
                            }
                            else
                            {
                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                    ErrorComment = requestTerminalّForCompany.Desc,
                                    NewParsian = true,
                                });
                                dataContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        RequestTerminalّForPersonInput input = new RequestTerminalّForPersonInput();
                        input.RequestData = new RequestTerminalForPersonRequest();

                        input.RequestData.Request = new Request();
                        input.RequestData.Request.TerminalCount = 1;
                        input.RequestData.Request.TermTypeRefId = TermTypeRefId.Physical;
                        input.RequestData.Request.RequestTerminalModelRefId =
                            (terminalInfo.DeviceTypeId == 1 || terminalInfo.DeviceTypeId == 2)
                                ? TerminalModel.Dialup
                                : ((terminalInfo.DeviceTypeId == 3 || terminalInfo.DeviceTypeId == 8)
                                    ? TerminalModel.GRPRS
                                    : TerminalModel.MobilePose); //todo
                        input.RequestData.Request.PersonTypeRefId = PersonType.haghighiIrani;
                        input.RequestData.Person = new Person
                        {
                            NationalCode = terminalInfo.NationalCode,
                            FirstName = terminalInfo.FirstName,
                            LastName = terminalInfo.LastName,
                            GenderTypeRefId = terminalInfo.IsMale ? GenderType.Male : GenderType.Female,
                            CityBirth = terminalInfo.CityId.ToString(),
                            BirthDateDt = terminalInfo.Birthdate.ToString("yyyy-MM-dd"),
                            IssueDateDt = terminalInfo.BirthCertificateIssueDate.ToString("yyyy-MM-dd"),
                            FirstNameEn = terminalInfo.EnglishFirstName,
                            LastNameEn = terminalInfo.EnglishFirstName,
                            FatherName = terminalInfo.FatherName,
                            FatherNameEn = terminalInfo.EnglishFatherName,
                            HomePhone = terminalInfo.HomeTel,
                            HomePostCode = terminalInfo.HomePostCode,
                            HomeAddress = terminalInfo.HomeAddress,
                            CertNo =  terminalInfo.IdentityNumber == "0" ?  terminalInfo.NationalCode:  terminalInfo.IdentityNumber,
                            BirthCrtfctSerial = terminalInfo.BirthCrtfctSerial,
                            BirthCrtfctSeriesNumber = terminalInfo.BirthCrtfctSeriesNumber,
                            PersianCharRefId = GetPersianCharRefId(terminalInfo.PersianCharRefId) ,
                        };
                        input.RequestData.Shop = new Shop
                        {
                            ShopName = terminalInfo.Title,
                            ShopNameEng = terminalInfo.EnglishTitle,
                            ShopSubMccRefId = (int) terminalInfo.GuildId,
                            ShopCityRefId = (int) terminalInfo.CityId
                        };
                        if (terminalInfo.RegionalMunicipalityId != null)
                            input.RequestData.Shop.ShopRegionRefId = (ShopRegion) terminalInfo.RegionalMunicipalityId;
                        input.RequestData.Shop.ShopPostalCode = terminalInfo.PostCode;
                        input.RequestData.Shop.ShopAddress = terminalInfo.Address;
                        input.RequestData.Shop.ShopPhone = terminalInfo.Tel;
                        input.RequestData.Shop.ShopMobNo = terminalInfo.Mobile;
                        input.RequestData.Shop.ShopEmailAddress = "";
                        input.RequestData.Shop.WebAddress = "";
                        input.RequestData.Shop.WebIp = "";
                        input.RequestData.Shop.WebPort = "";
                        input.RequestData.Shop.WebNamadType = "";
                        input.RequestData.Shop.WebNamadRegDateDt = "";
                        input.RequestData.Shop.WebNamadExpDateDt = "";
                        input.RequestData.Shop.TaxPayerCode = terminalInfo.TaxPayerCode;
                        input.RequestData.Settlements = new Settlements
                        {
                            Iban = terminalInfo.ShebaNo, IsMainAccount = true
                        };
                         var Files = files.Select(b => new UploadAttachmentRequestData 
                        {
                            ContentType = b.ContentType,
                            FileName = b.FileName,
                            Base64 = b.Base64
                        }).ToList();
                        var requestTerminalّForPerson = RequestTerminalّForPerson(input,Files, (int) terminalId);


                        if (requestTerminalّForPerson.IsSuccess)
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                            {
                                TopiarId = requestTerminalّForPerson.RequestResult.TopiarId.Value,
                                StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,

                                NewParsian = true,
                                Description = $"{DateTime.Now.ToLongPersianDateTime()}  "
                            });
                            dataContext.SaveChanges();
                        }
                        else
                        {
                            if (requestTerminalّForPerson.ErrorList.Any(b => b.ErrorId == "-1") &&
                                terminalInfo.TopiarId != null)
                            {
                                var s = UpdateStatusForRequestedTerminal(terminalInfo.TopiarId.Value.ToString(),
                                    (int) terminalId).Result;

                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,
                                    ErrorComment = s.Error, // requestTerminalّForCompany.Desc,


                                    NewParsian = true,
                                });
                                dataContext.SaveChanges();
                            } else if (requestTerminalّForPerson.ErrorList.Any(b => b.ErrorId == "-1") &&
                                       terminalInfo.TopiarId == null)
                            {
                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                    ErrorComment = string.Join(",",requestTerminalّForPerson.ErrorList.Select(b=>b.ErrorText)), // requestTerminalّForCompany.Desc,
                                    NewParsian = true,
                                });
                                dataContext.SaveChanges();
                            }
                            else
                            {
                                dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                    ErrorComment = requestTerminalّForPerson.Desc +
                                                   requestTerminalّForPerson.ErrorList != null ? 
                                                   string.Join(",",requestTerminalّForPerson.ErrorList.Select(b=>b.ErrorText)) : "",
                                    NewParsian = true,
                                });
                                dataContext.SaveChanges();
                            }

                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception exception)
                {
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                    {
                        StatusId = (byte) Enums.TerminalStatus.NeedToReform, ErrorComment = "خطا در اتصال به وب سرویس"
                    });
                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
        }

        public int GetPersianCharRefId(string terminalInfoPersianCharRefId)
        {
            switch (terminalInfoPersianCharRefId)
            {
                case "الف":
                    return 1152071;
                case "ب":
                    return 1152072;
                case "ل":
                    return 1152073;
                case "د":
                    return 1152074;
                case "ر":
                    return 1152075;
                case "1":
                    return 1152076;
                case "2":
                    return 1152077;
                case "3":
                    return 1152078;
                case "4":
                    return 1152079;
                case "9":
                    return 1152080;
                case "10":
                    return 1152081;
                case "11":
                    return 1152082;
                default:
                    return 1152071;
                
            }
        }


        public UploadAttachmentResult UploadAttachment(UploadAttachmentInput input, int terminalId)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;


            var parsianRequest = new ParsianRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "UploadAttachmentInput",
                Module = "Merchant",
                TerminalId = terminalId   
            };
            var datacontext = new AppDataContext();
            datacontext.ParsianRequests.Add(parsianRequest);
            datacontext.SaveChanges();
            input.RequestCode = parsianRequest.Id;
          


            UploadAttachmentResult reservationList;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/UploadAttachment", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<UploadAttachmentResult>(apiResponse);
                }
            }


            parsianRequest.Result = JsonConvert.SerializeObject(reservationList);
            parsianRequest.Input = JsonConvert.SerializeObject(input);

            datacontext.SaveChanges();

            return reservationList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>TopiarId</returns>
        public TerminalResult RequestTerminalّForCompany(RequestTerminalّForCompanyInput input
            ,List<UploadAttachmentRequestData> Files   , int TerminalId)
        {
            List<string> FileRes = new List<string>();
            
            if ( Files.Any())

            {
              
                foreach (var uploadAttachment in Files)
                {
                    var up = new UploadAttachmentInput {RequestData = uploadAttachment};
                    var item = UploadAttachment(up,TerminalId);
                    if ( item.IsSuccess )
                    {
                        FileRes.Add(item.RequestResult.FileRef);
                    }
                  
                }

            }
            
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;


            var parsianRequest = new ParsianRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "RequestTerminal",
                Module = "Merchant",
                TerminalId = TerminalId
            };
            var datacontext = new AppDataContext();
            datacontext.ParsianRequests.Add(parsianRequest);
            datacontext.SaveChanges();
            input.RequestCode = parsianRequest.Id;
            input.RequestCode = TerminalId;

            input.RequestData.Attachments = FileRes.Select(a => new Attachment
            {
                FileRef =  a
            }).ToList();

            TerminalResult reservationList;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestTerminal", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<TerminalResult>(apiResponse);
                }
            }


            parsianRequest.Result = JsonConvert.SerializeObject(reservationList);
            parsianRequest.Input = JsonConvert.SerializeObject(input);

            datacontext.SaveChanges();

            return reservationList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>TopiarId</returns>
        public TerminalResult RequestTerminalّForPerson(RequestTerminalّForPersonInput input
           , List<UploadAttachmentRequestData> Files
            , int TerminalId)
        {
            
            
            List<string> FileRes = new List<string>();
            
            if ( Files.Any())

            {
              
                foreach (var uploadAttachment in  Files)
                {
                    var up = new UploadAttachmentInput();
                    up.RequestData = uploadAttachment;
                    var item = UploadAttachment(up,TerminalId);
                    if ( item.IsSuccess )
                    {
                        FileRes.Add(item.RequestResult.FileRef);
                    }
                 
                }

            }


            
            
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;
            TerminalResult reservationList;
            var newpaRequest = new ParsianRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "RequestTerminal",
                Module = "Merchant",
                TerminalId = TerminalId
            };

            var datacontext = new AppDataContext();
            datacontext.ParsianRequests.Add(newpaRequest);
            datacontext.SaveChanges();
            input.RequestCode = TerminalId;
            
            
            input.RequestData.Attachments = FileRes.Select(a => new Attachment
            {
                FileRef =  a
            }).ToList();
            
            

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");
                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestTerminal", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<TerminalResult>(apiResponse);
                }
            }

            newpaRequest.Result = JsonConvert.SerializeObject(reservationList);
            newpaRequest.Input = JsonConvert.SerializeObject(input);

            datacontext.SaveChanges();
            return reservationList;
        }

        public RequestChangeAccountInfoResult RequestChangeAccountInfo(long changerequestId,
            RequestChangeAccountInfoInput input, int TerminalId)
        {
            try
            {
                var token = Login();

                if (string.IsNullOrEmpty(token.LoginToken))
                    return null;

                input.RequestCode = (int) changerequestId + 100000;
                var newpaRequest = new ParsianRequest
                {
                    Input = JsonConvert.SerializeObject(input),
                    Method = "RequestChangeAccountInfo",
                    Module = "Merchant",
                    TerminalId = TerminalId
                };

                var datacontext = new AppDataContext();
                datacontext.ParsianRequests.Add(newpaRequest);
                datacontext.SaveChanges();


                using (var httpClient = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                        "application/json");

                    content.Headers.Add("LoginToken", token.LoginToken);
                    content.Headers.Add("SignPhrase", "");
                    using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestChangeIban", content)
                        .Result)
                    {
                        var apiResponse = response.Content.ReadAsStringAsync().Result;
                        var res = JsonConvert.DeserializeObject<RequestChangeAccountInfoResult>(apiResponse);

                        newpaRequest.Result = JsonConvert.SerializeObject(res);
                        newpaRequest.Input = JsonConvert.SerializeObject(input);


                        var d = datacontext.SaveChanges();


                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                RequestChangeAccountInfoResult x = new RequestChangeAccountInfoResult();
                x.IsSuccess = false;
                x.ErrorList = new List<ErrorList>();
                var xd = new ErrorList();
                xd.ErrorId = "1";
                xd.ErrorText = ex.Message;

                x.ErrorList.Add(xd);
                return x;
            }
        }

        public void UpdateBadData()
        {
            var datacontext2 = new AppDataContext();
            var data = datacontext2.ParsianRequests.Where(b => b.Method == "RequestChangeAccountInfo"
                                            && b.Result.Contains("\"TopiarId\":\"0\"") 
                                            && b.Input.Contains("uestCode\":30}")).ToList();


            var counter = 34;

            foreach (var VARIABLE in data)
            {
                try
                {
                    var token = Login();

                    if (string.IsNullOrEmpty(token.LoginToken))
                        continue;


                    var input = JsonConvert.DeserializeObject<RequestChangeAccountInfoInput>(VARIABLE.Input);


                    input.RequestCode = (int) counter;
                    var newpaRequest = new ParsianRequest
                    {
                        Input = JsonConvert.SerializeObject(input),
                        Method = "RequestChangeAccountInfo",
                        Module = "Merchant",
                        TerminalId = VARIABLE.TerminalId
                    };

                    var datacontext = new AppDataContext();
                    datacontext.ParsianRequests.Add(newpaRequest);
                    datacontext.SaveChanges();


                    using (var httpClient = new HttpClient())
                    {
                        var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                            "application/json");

                        content.Headers.Add("LoginToken", token.LoginToken);
                        content.Headers.Add("SignPhrase", "");
                        using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestChangeIban", content)
                            .Result)
                        {
                            var apiResponse = response.Content.ReadAsStringAsync().Result;
                            var res = JsonConvert.DeserializeObject<RequestChangeAccountInfoResult>(apiResponse);

                            newpaRequest.Result = JsonConvert.SerializeObject(res);
                            newpaRequest.Input = JsonConvert.SerializeObject(input);


                            var d = datacontext.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    RequestChangeAccountInfoResult x = new RequestChangeAccountInfoResult();
                    x.IsSuccess = false;
                    x.ErrorList = new List<ErrorList>();
                    var xd = new ErrorList();
                    xd.ErrorId = "1";
                    xd.ErrorText = ex.Message;

                    x.ErrorList.Add(xd);
                }

                counter = counter + 1;
            }
        }

        public void AssignDeviceToTerminal(AssignDeviceToTerminalInput input)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestInquery", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    JsonConvert.DeserializeObject<RequestInqueryResult>(apiResponse);
                }
            }
        }

        public RequestInqueryResult RequestInQuery(RequestInqueryInput input, int terminalId)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;
            RequestInqueryResult reservationList;
            var parsianRequest = new ParsianRequest();

            parsianRequest.Input = JsonConvert.SerializeObject(input);
            parsianRequest.Method = "RequestInquery";
            parsianRequest.Module = "Merchant";
            var datacontext = new AppDataContext();
            parsianRequest.TerminalId = terminalId;
            datacontext.ParsianRequests.Add(parsianRequest);
            datacontext.SaveChanges();
            input.RequestCode = terminalId;

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestInquery", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<RequestInqueryResult>(apiResponse);
                }
            }

            parsianRequest.Result = JsonConvert.SerializeObject(reservationList);
            parsianRequest.Input = JsonConvert.SerializeObject(input);

            datacontext.SaveChanges();
            return reservationList;
        }

        public List<NewParsianTerminal> MerchantTerminals(MerchantTerminalsInput input)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;
            RegisterConfirmOutput reservationList;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/DigitalReciept/MerchantTerminals", content)
                    .Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    reservationList = JsonConvert.DeserializeObject<RegisterConfirmOutput>(apiResponse);
                }
            }

            return reservationList.Terminals;
        }

        public List<NewParsianTerminal> RegisterConfirm(RegisterConfirmInput input)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;
            RegisterConfirmOutput registerConfirmOutput;
            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");


                using (var response = httpClient.PostAsync(BaseAddress + "/DigitalReciept/RegisterConfirm", content)
                    .Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    registerConfirmOutput = JsonConvert.DeserializeObject<RegisterConfirmOutput>(apiResponse);
                }
            }

            return registerConfirmOutput.Terminals;
        }

        public RevokeRequestResult RequestRevocationTerminal(RequestRevocationTerminalInput input, int terminalId)
        {
            var token = Login();
            if (string.IsNullOrEmpty(token.LoginToken))
                return null;
            var parsianRequest = new ParsianRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "RequestRevocationTerminal",
                Module = "Merchant",
                TerminalId = terminalId
            };

            var datacontext = new AppDataContext();
            datacontext.ParsianRequests.Add(parsianRequest);
            datacontext.SaveChanges();
            input.RequestCode = terminalId;

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");

                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");
                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestRevocationTerminal", content)
                    .Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    var d = JsonConvert.DeserializeObject<RevokeRequestResult>(apiResponse);
                    parsianRequest.Result = JsonConvert.SerializeObject(d);
                    parsianRequest.Input = JsonConvert.SerializeObject(input);
                    datacontext.SaveChanges();
                    return d;
                }
            }
        }
    public TerminalResult RequestChangeInfo(RequestChangeInfoInput2 input , int? TerminalId = 0)
        {
            try
            {
                var token = Login();
                if (string.IsNullOrEmpty(token.LoginToken))
                    return null;
                
                var newpaRequest = new ParsianRequestForInfo()
                {
                    Input = JsonConvert.SerializeObject(input),
                    Method = "RequestChangeInfo",
                    Module = "Merchant",
                    TerminalId =  TerminalId ,
                    StatusId = 0,
                    NationalCode = input.RequestData.NationalCode
                    
                };

                var datacontext = new AppDataContext();
                datacontext.ParsianRequestForInfo.Add(newpaRequest);
                datacontext.SaveChanges();
                
                using (var httpClient = new HttpClient())
                {
                   
                    var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                        "application/json");
                    content.Headers.Add("LoginToken", token.LoginToken);
                    content.Headers.Add("SignPhrase", "");
                    using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestChangeInfo", content)
                               .Result)
                    {
                        var apiResponse = response.Content.ReadAsStringAsync().Result;
                      var   reservationList = JsonConvert.DeserializeObject<TerminalResult>(apiResponse);
                      
                      newpaRequest.Result = JsonConvert.SerializeObject(reservationList);
                      newpaRequest.Input = JsonConvert.SerializeObject(input);
                      newpaRequest.NationalCode = input.RequestData.NationalCode;
                      if (reservationList.RequestResult.TopiarId == null)
                      {
                          newpaRequest.StatusId = 2;
                          newpaRequest.Error =  reservationList.ErrorList != null ? string.Join(",",
                              reservationList.ErrorList.Select(v => v.ErrorId + " - " + v.ErrorText).ToArray()) : "";
                      }
                      else
                      {
                          newpaRequest.StatusId = 3;
                          newpaRequest.TopiarId = reservationList.RequestResult.TopiarId;
                      }

                      var d = datacontext.SaveChanges();
                      
                        return reservationList;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public TerminalResult RequestChangeShopPost(RequestChangeShopPost input , int? TerminalId = 0)
        {
            try
            {
                var token = Login();
                if (string.IsNullOrEmpty(token.LoginToken))
                    return null;
                
                var newpaRequest = new ParsianRequestForInfo()
                {
                    Input = JsonConvert.SerializeObject(input),
                    Method = "RequestChangeInfo",
                    Module = "Merchant",
                    TerminalId =  TerminalId ,
                    StatusId = 0,
                    
                    
                };

                var datacontext = new AppDataContext();
                datacontext.ParsianRequestForInfo.Add(newpaRequest);
                datacontext.SaveChanges();
                
                using (var httpClient = new HttpClient())
                {
                   
                    var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                        "application/json");
                    content.Headers.Add("LoginToken", token.LoginToken);
                    content.Headers.Add("SignPhrase", "");
                    using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/RequestChangeInfo", content)
                               .Result)
                    {
                        var apiResponse = response.Content.ReadAsStringAsync().Result;
                      var   reservationList = JsonConvert.DeserializeObject<TerminalResult>(apiResponse);
                      
                      newpaRequest.Result = JsonConvert.SerializeObject(reservationList);
                      newpaRequest.Input = JsonConvert.SerializeObject(input);
                      
                      if (reservationList.RequestResult.TopiarId == null)
                      {
                          newpaRequest.StatusId = 2;
                          newpaRequest.Error =  reservationList.ErrorList != null ? string.Join(",",
                              reservationList.ErrorList.Select(v => v.ErrorId + " - " + v.ErrorText).ToArray()) : "";
                      }
                      else
                      {
                          newpaRequest.StatusId = 3;
                          newpaRequest.TopiarId = reservationList.RequestResult.TopiarId;
                      }

                      var d = datacontext.SaveChanges();
                      
                        return reservationList;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public TerminalInQueryOutPut TerminalInQuery(TerminalInqueryInput input, int terminalId)
        {
            var token = Login();


            var parsianRequest = new ParsianRequest
            {
                Input = JsonConvert.SerializeObject(input),
                Method = "TerminalInquery",
                Module = "Merchant",
                TerminalId = terminalId
            };
            var datacontext = new AppDataContext();
            datacontext.ParsianRequests.Add(parsianRequest);
            datacontext.SaveChanges();
            input.RequestCode = terminalId;

            using (var httpClient = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8,
                    "application/json");
                content.Headers.Add("LoginToken", token.LoginToken);
                content.Headers.Add("SignPhrase", "");
                using (var response = httpClient.PostAsync(BaseAddress + "/Merchant/TerminalInquery", content).Result)
                {
                    var apiResponse = response.Content.ReadAsStringAsync().Result;
                    var q = JsonConvert.DeserializeObject<TerminalInQueryOutPut>(apiResponse);
                    parsianRequest.Result = JsonConvert.SerializeObject(q);
                    parsianRequest.Input = JsonConvert.SerializeObject(input);
                    datacontext.SaveChanges();
                    return q;
                }
            }
        }

        #endregion


        #region OldMethods

        public bool IsUp()
        {
            try
            {
                _client.Open();
                _client.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AddAcceptor(long terminalId)
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalInfo = dataContext.Terminals
                    .Where(x => x.Id == terminalId &&
                                (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                 x.StatusId == (byte) Enums.TerminalStatus.NeedToReform) &&
                                x.PspId == (byte) Enums.PspCompany.Parsian)
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        Title = x.Title,
                        CityId = x.CityId,
                        GuildId = x.GuildId,
                        ShebaNo = x.ShebaNo,
                        TelCode = x.TelCode,
                        Address = x.Address,
                        PostCode = x.PostCode,
                        BranchId = x.BranchId,
                        StatusId = x.StatusId,
                        AccountNo = x.AccountNo,
                        StateId = x.City.StateId,
                        MarketerId = x.MarketerId,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        Mobile = x.MerchantProfile.Mobile,
                        ActivityTypeId = x.ActivityTypeId,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        IsMale = x.MerchantProfile.IsMale,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        NationalCode = x.MerchantProfile.NationalCode,
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        RegionalMunicipalityId = x.RegionalMunicipalityId,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                        SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                        ParentGuildId = x.Guild.ParentId
                    })
                    .First();

                var posRequest = GetPosRequest(terminalInfo);

                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                    var response = _client.RegisterPosRequest(JsonConvert.SerializeObject(posRequest));
                    if (response.ChannelState == 1)
                    {
                        string actionStateDescription = null;
                        switch (response.ActionState)
                        {
                            case 0:
                                actionStateDescription = "درخواست با موفقیت ثبت شد";
                                break;
                            case 100:
                                actionStateDescription = "کد درخواست بانک تکراری است";
                                break;
                            case 101:
                                actionStateDescription = "کد استان وارد شده معتبر نمیباشد";
                                break;
                            case 102:
                                actionStateDescription = "خطا در اجرای عملیات";
                                break;
                            case 103:
                                actionStateDescription = "اشکال در تطبیق کد شهر";
                                break;
                            case 104:
                                actionStateDescription = "کد صنف وارد شده معتبر نمیباشد";
                                break;
                            case 105:
                                actionStateDescription = "نام پدر وارد نشده است";
                                break;
                            case 106:
                                actionStateDescription = "کد بانک درخواست دهنده معتبر نمیباشد";
                                break;
                            case 107:
                                actionStateDescription = "کد سرپرستی حساب معتبر نمیباشد";
                                break;
                            case 108:
                                actionStateDescription = "کد شعبه حساب معتبر نمیباشد";
                                break;
                            case 109:
                                actionStateDescription = "کد شهر وارد شده معتبر نمیباشد";
                                break;
                            case 900:
                                actionStateDescription = "خطا در اجرای عملیات";
                                break;
                        }

                        if (response.ActionState == 0 && response.PosTrackIds?.FirstOrDefault() != null)
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                            {
                                ContractNo = response.PosTrackIds.Last().ToString(),
                                StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,
                                Description = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}"
                            });
                        else
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                            {
                                StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                ErrorComment =
                                    $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}{Environment.NewLine}{response.Description}"
                            });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                            {StatusId = (byte) Enums.TerminalStatus.NeedToReform, ErrorComment = response.Description});
                    }

                    return true;
                }
                catch (Exception exception)
                {
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal
                    {
                        StatusId = (byte) Enums.TerminalStatus.NeedToReform, ErrorComment = "خطا در اتصال به وب سرویس"
                    });
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
                    .Where(x => terminalIdList.Contains(x.Id) && x.StatusId == (byte) Enums.TerminalStatus.New &&
                                x.PspId == (byte) Enums.PspCompany.Parsian)
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        Title = x.Title,
                        CityId = x.CityId,
                        GuildId = x.GuildId,
                        ShebaNo = x.ShebaNo,
                        TelCode = x.TelCode,
                        Address = x.Address,
                        PostCode = x.PostCode,
                        BranchId = x.BranchId,
                        StatusId = x.StatusId,
                        AccountNo = x.AccountNo,
                        StateId = x.City.StateId,
                        MarketerId = x.MarketerId,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        ParentGuildId = x.Guild.ParentId,
                        Mobile = x.MerchantProfile.Mobile,
                        ActivityTypeId = x.ActivityTypeId,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        IsMale = x.MerchantProfile.IsMale,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        NationalCode = x.MerchantProfile.NationalCode,
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        RegionalMunicipalityId = x.RegionalMunicipalityId,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                        SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate
                    })
                    .ToList();

                foreach (var terminalInfo in terminalInfoList)
                {
                    try
                    {
                        var posRequest = GetPosRequest(terminalInfo);

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                               SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                        var response = _client.RegisterPosRequest(JsonConvert.SerializeObject(posRequest));
                        if (response.ChannelState == 1)
                        {
                            string actionStateDescription = null;
                            switch (response.ActionState)
                            {
                                case 0:
                                    actionStateDescription = "درخواست با موفقیت ثبت شد";
                                    break;
                                case 100:
                                    actionStateDescription = "کد درخواست بانک تکراری است";
                                    break;
                                case 101:
                                    actionStateDescription = "کد استان وارد شده معتبر نمیباشد";
                                    break;
                                case 102:
                                    actionStateDescription = "خطا در اجرای عملیات";
                                    break;
                                case 103:
                                    actionStateDescription = "اشکال در تطبیق کد شهر";
                                    break;
                                case 104:
                                    actionStateDescription = "کد صنف وارد شده معتبر نمیباشد";
                                    break;
                                case 105:
                                    actionStateDescription = "نام پدر وارد نشده است";
                                    break;
                                case 106:
                                    actionStateDescription = "کد بانک درخواست دهنده معتبر نمیباشد";
                                    break;
                                case 107:
                                    actionStateDescription = "کد سرپرستی حساب معتبر نمیباشد";
                                    break;
                                case 108:
                                    actionStateDescription = "کد شعبه حساب معتبر نمیباشد";
                                    break;
                                case 109:
                                    actionStateDescription = "کد شهر وارد شده معتبر نمیباشد";
                                    break;
                                case 900:
                                    actionStateDescription = "خطا در اجرای عملیات";
                                    break;
                            }

                            if (response.ActionState == 0)
                                dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                {
                                    ContractNo = response.PosTrackIds.Last().ToString(),
                                    StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch,
                                    ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}"
                                });
                            else
                                dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                                {
                                    StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                    ErrorComment =
                                        $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}{Environment.NewLine}{response.Description}"
                                });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                            {
                                StatusId = (byte) Enums.TerminalStatus.NeedToReform, ErrorComment = response.Description
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
                                StatusId = (byte) Enums.TerminalStatus.NeedToReform,
                                ErrorComment = "خطا در برقراری ارتباط با وب سرویس"
                            });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal
                            {
                                StatusId = (byte) Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message
                            });
                        }

                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            }
        }

        public async Task<InqueryAcceptorResult> UpdateTerminalInfo(string contractNo)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            var result = await _client.GetPosTermNoAsync(long.Parse(contractNo));

            if (result.ChannelState == 1 && result.ActionState == 0)
            {
                var description = result.Description;
                var revokeDate = result.UnInstallDate.ConvertToDate();
                var installationDate = result.InstallDate.ConvertToDate();
                var terminalNo = result.PosTermNo != 0 ? result.PosTermNo.ToString() : null;
                var merchantNo = result.CustomerId != 0 ? result.CustomerId.ToString() : null;

                if (string.IsNullOrEmpty(merchantNo) || string.IsNullOrEmpty(terminalNo))
                    return new InqueryAcceptorResult {IsSuccess = false};

                return new InqueryAcceptorResult
                {
                    IsSuccess = true,
                    RevokeDate = revokeDate,
                    MerchantNo = merchantNo,
                    TerminalNo = terminalNo,
                    Description = description,
                    InstallationDate = installationDate
                };
            }

            return new InqueryAcceptorResult {IsSuccess = false};
        }


        public UpdateStatusForRegisteredTerminalOutput
            UpdateStatusForRegisteredTerminal(string TerminalNo, int terminalId)
        {
            UpdateStatusForRegisteredTerminalOutput output = new UpdateStatusForRegisteredTerminalOutput();
            // async Task<(bool, byte, string)>  
            var statusDescription = string.Empty;
            byte statusId = 0;
            DateTime? InstallationDate = null;
            DateTime? RevokeDate = null;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;


            var TerminalInqueryInput = new TerminalInqueryInput();
            TerminalInqueryInput.RequestData = new TerminalInqueryRequestData();
            TerminalInqueryInput.RequestData.TerminalNumber = TerminalNo;

            var result =
                TerminalInQuery(TerminalInqueryInput, terminalId); // await _client.CheckPosStatusAsync(trackingCode);

            if (result.RequestResult == null  )
            {
                var qqq = string.Join(",",
                    result.ErrorList.Select(v => v.ErrorId + " - " + v.ErrorText).ToArray());
                output.IsSuccess = false;
                output.Error = qqq;
                output.StatusId = (byte) Enums.TerminalStatus.NeedToReform;
                return output;
                // return (false,  (byte) Enums.TerminalStatus.NotReturnedFromSwitch, $"{DateTime.Now.ToLongPersianDateTime()} - {qqq    }");
            }

            PersianCalendar pc = new PersianCalendar();

            if (result.RequestResult.InstallDate != "0000/00/00" && result.RequestResult.InstallDate != null)
            {
                var year = result.RequestResult.InstallDate.Split('/')[0];
                var month = result.RequestResult.InstallDate.Split('/')[1];
                var day = result.RequestResult.InstallDate.Split('/')[2];
                DateTime dt = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), pc);
                InstallationDate = dt;
            }

            if (result.RequestResult.UnistallDate != "0000/00/00" && result.RequestResult.UnistallDate != null)
            {
                var year = result.RequestResult.UnistallDate.Split('/')[0];
                var month = result.RequestResult.UnistallDate.Split('/')[1];
                var day = result.RequestResult.UnistallDate.Split('/')[2];
                DateTime dt = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), pc);
                RevokeDate = dt;
            }
            switch ((int) result.RequestResult.StatusCode)
            {
                case 0:
                    statusId = (byte) Enums.TerminalStatus.Allocated;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
                case 1:
                    statusId = (byte) Enums.TerminalStatus.Allocated;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;

                case 2:
                    statusId = (byte) Enums.TerminalStatus.Revoked;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
                case 3:

                    statusId = (byte) Enums.TerminalStatus.Installed;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
                case 4:
                    statusId = (byte) Enums.TerminalStatus.Installed;
                    statusDescription = "نصب شده";
                    break;
                case 5:
                    statusId = (byte) Enums.TerminalStatus.Deleted;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
                case 6:
                    statusId = (byte) Enums.TerminalStatus.ReadyForAllocation;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
                case 7:
                    statusId = (byte) Enums.TerminalStatus.WaitingForRevoke;
                    statusDescription = result.RequestResult.StatusTitle;
                    break;
            }

            output.IsSuccess = true;
            output.StatusId = statusId;
            output.Status = statusDescription;
            output.Error = "";
            output.InstallationDate = InstallationDate;
            output.RevokeDate = RevokeDate;
            return output;
            // return (true, statusId, $"{DateTime.Now.ToLongPersianDateTime()} - {statusDescription}");
        }
     public async Task<ParsianRequestedTerminalResult> ChangeInfoInquery(string topiarId,
            int terminalId)
        {
            var statusDescription = string.Empty;
            var ParsianRequestedTerminalResult = new ParsianRequestedTerminalResult();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            var requestInqueryInput = new RequestInqueryInput();
            requestInqueryInput.RequestData = new RequestInqueryRequestData();
            requestInqueryInput.RequestData.TopiarId = topiarId;
            var result = RequestInQuery(requestInqueryInput, terminalId);
            
            if (result.IsSuccess)
            {
                if (result.RequestResult.RequestError != null)
                    ParsianRequestedTerminalResult.Error = string.Join(",",
                        result.RequestResult.RequestError.Select(v => v.ErrorText).ToArray());

                if (result.ErrorList.Any())
                {
                    ParsianRequestedTerminalResult.IsComplete = false; return
                        ParsianRequestedTerminalResult;
                }
                if (result.RequestResult.StatusCode == 2 && result.RequestResult.Stepcode == 7)
                {
                    ParsianRequestedTerminalResult.IsComplete = true;
                    return ParsianRequestedTerminalResult;
                }

                else
                {
                    ParsianRequestedTerminalResult.IsComplete = false;
                }
                return
                    ParsianRequestedTerminalResult; // (true, statusId, $"{DateTime.Now.ToLongPersianDateTime()} - {statusDescription}");
            }
            else
            {
                ParsianRequestedTerminalResult.Error = string.Join(",",
                    result.ErrorList.Select(v => v.ErrorText).ToArray());
                ParsianRequestedTerminalResult.IsComplete = false;
                return ParsianRequestedTerminalResult;
            }
        }

        public async Task<ParsianRequestedTerminalResult> UpdateStatusForRequestedTerminal(string topiarId,
            int terminalId)
        {
            var statusDescription = string.Empty;
            var ParsianRequestedTerminalResult = new ParsianRequestedTerminalResult();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            var requestInqueryInput = new RequestInqueryInput();
            requestInqueryInput.RequestData = new RequestInqueryRequestData();
            requestInqueryInput.RequestData.TopiarId = topiarId;
            var result = RequestInQuery(requestInqueryInput, terminalId);
            if (result.IsSuccess)
            {
                if (result.RequestResult.RequestError != null)
                    ParsianRequestedTerminalResult.Error = string.Join(",",
                        result.RequestResult.RequestError.Select(v => v.ErrorText).ToArray());

                if (result.RequestResult.StatusCode == 2 && result.RequestResult.Stepcode == 7)
                {
                    var RegisteredTerminalResult =
                        UpdateStatusForRegisteredTerminal(result.RequestResult.RequestDetails.FirstOrDefault()
                            ?.TerminalNumber, terminalId); // await _client.CheckPosStatusAsync(trackingCode);
                    ParsianRequestedTerminalResult.StatusId = RegisteredTerminalResult.StatusId;
                    ParsianRequestedTerminalResult.StatusTitle = RegisteredTerminalResult.Status;
                    ParsianRequestedTerminalResult.Error = RegisteredTerminalResult.Error;
                    ParsianRequestedTerminalResult.InstallationDate = RegisteredTerminalResult.InstallationDate;
                    ParsianRequestedTerminalResult.InstallStatus = result.RequestResult.StatusTitle;
                    ParsianRequestedTerminalResult.InstallStatusId = result.RequestResult.StatusCode;
                    ParsianRequestedTerminalResult.StepCode = result.RequestResult.Stepcode;

                    ParsianRequestedTerminalResult.StepCodeTitle = result.RequestResult.StepTitle;
                    ParsianRequestedTerminalResult.AccecptorCode =
                        result.RequestResult.RequestDetails.FirstOrDefault().AcceptorCode;
                    ParsianRequestedTerminalResult.TerminalCreateDate =
                        result.RequestResult.RequestDetails.FirstOrDefault().TerminalCreateDate;
                    ParsianRequestedTerminalResult.ShaparakRegisterDate =
                        result.RequestResult.RequestDetails.FirstOrDefault().ShaparakRegisterDate;


                    ParsianRequestedTerminalResult.TerminalNo = result.RequestResult.RequestDetails.FirstOrDefault()
                        ?.TerminalNumber;
                    return ParsianRequestedTerminalResult;
                }

                //===> check is in cartables 
                if (result.RequestResult.StatusCode == 2 && result.RequestResult.Stepcode == 3)
                {
                    ParsianRequestedTerminalResult.InstallStatus = result.RequestResult.StatusTitle;
                    ParsianRequestedTerminalResult.InstallStatusId = result.RequestResult.StatusCode;
                    ParsianRequestedTerminalResult.StepCode = result.RequestResult.Stepcode;
                    ParsianRequestedTerminalResult.StepCodeTitle = result.RequestResult.StepTitle;
                    ParsianRequestedTerminalResult.StatusId = ( result.RequestResult.RequestDetails?.FirstOrDefault().CartableResult.ToLower() == "reject" ?
                          (byte) Enums.TerminalStatus.NeedToReform
                        : (byte) Enums.TerminalStatus.NotReturnedFromSwitch);

                    var error = string.IsNullOrEmpty(ParsianRequestedTerminalResult.Error) ?
                        (
                            string.Join(",",   result.RequestResult.RequestDetails?.Select(b=>b.Description))
                        ) : ParsianRequestedTerminalResult.Error;
                    
                    ParsianRequestedTerminalResult.StatusTitle =
                        $"{DateTime.Now.ToLongPersianDateTime()} - {   error   }";
                }
                else
                {
                    ParsianRequestedTerminalResult.InstallStatus = result.RequestResult.StatusTitle;
                    ParsianRequestedTerminalResult.InstallStatusId = result.RequestResult.StatusCode;
                    ParsianRequestedTerminalResult.StepCode = result.RequestResult.Stepcode;
                    ParsianRequestedTerminalResult.StepCodeTitle = result.RequestResult.StepTitle;
                    ParsianRequestedTerminalResult.StatusId = result.RequestResult.StatusCode == 3
                        ? (byte) Enums.TerminalStatus.NeedToReform
                        : (byte) Enums.TerminalStatus.NotReturnedFromSwitch;

                    var error = string.IsNullOrEmpty(ParsianRequestedTerminalResult.Error) ?
                        (
                            string.Join(",",  result.RequestResult.RequestDetails == null ? new [] {""} : 
                                result.RequestResult.RequestDetails?.Select(b=>b.Description))
                        ) : ParsianRequestedTerminalResult.Error;
                    
                    ParsianRequestedTerminalResult.StatusTitle =
                        $"{DateTime.Now.ToLongPersianDateTime()} - {   error   }";
                }
               

                   
                    //todo
                return
                    ParsianRequestedTerminalResult; // (true, statusId, $"{DateTime.Now.ToLongPersianDateTime()} - {statusDescription}");
            }
            else
            {
                ParsianRequestedTerminalResult.Error = string.Join(",",
                    result.ErrorList.Select(v => v.ErrorText).ToArray());
                return ParsianRequestedTerminalResult;
            }
        }


        // trackingCode = contractNo
        public async Task<(bool, byte, string)> UpdateStatus(string trackingCode, byte statusId)
        {
            var statusDescription = string.Empty;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;


            // var TerminalInqueryInput = new TerminalInqueryInput();
            // TerminalInqueryInput.RequestData = new TerminalInqueryRequestData();
            // TerminalInqueryInput.RequestData.TerminalNumber = trackingCode;

            //var result = TerminalInQuery(TerminalInqueryInput); // await _client.CheckPosStatusAsync(trackingCode);
            var result = await _client.CheckPosStatusAsync(Int64.Parse(trackingCode));

            switch ((int) result)
            {
                case 0:
                    statusId = (byte) Enums.TerminalStatus.SendToRepresentation;
                    statusDescription = "   ارسال به نمایندگی    ";
                    break;
                case 1:
                    statusId = (byte) Enums.TerminalStatus.Allocated;
                    statusDescription = "پایانه تخصیص یافته است";
                    break;

                case 2:
                    statusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                    statusDescription = "درخواست مشاهده شده و در دست پیگیری است";
                    break;
                case 3:

                    statusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                    statusDescription = "درخواست ثبت شده است";
                    break;
                case 4:
                    statusId = (byte) Enums.TerminalStatus.Installed;
                    statusDescription = "پایانه نصب و راه اندازی شده است";
                    break;
                case 5:
                    statusDescription = "پایانه تحت تعمیر می باشد";
                    break;
                case 6:
                    statusId = (byte) Enums.TerminalStatus.Revoked;
                    statusDescription = "پایانه جمع آوری شده است";
                    break;
                case 7:
                    statusId = (byte) Enums.TerminalStatus.Revoked;
                    statusDescription = "درخواست لغو شده است";
                    break;
                case 10:
                    //ToDo
                    // statusId = (byte)Enums.TerminalStatus.NeedToReform;
                    // var shaparakStatus = _client.CheckPosShaparakStatus(trackingCode);
                    // if (shaparakStatus.ChannelState == 1)
                    // {
                    //     var shaparakStatusDescription = shaparakStatus.Data.FirstOrDefault();
                    //     statusDescription = shaparakStatusDescription?.Error;
                    // }
                    break;

                // خطای شاپرکی - در این حالت بعد از عدد 8 کد خطای شاپرکی آورده می شود مثلاً 839 یعنی خطای 39 از سوی شاپرک

                case 85:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 5 در سامانه جامع شاپرک: فرمت فیلدها را ویرایش نمایید به ازای مقدار یک فیلد، مقداری یافت نشد";
                    break;
                case 86:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 6 در سامانه جامع شاپرک: فرمت فیلدها را ویرایش نمایید فرمت فیلد مذکور درست نمی باشد";
                    break;
                case 811:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 11 در سامانه جامع شاپرک: اطلاعات قبلاً به شاپرک ارسال شده است رکورد تکراری";
                    break;
                case 812:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "اطلاعات کد ملی و یا تاریخ تولد را ویرایش نمایید کد ملی و تاریخ تولد با هم مطابقت ندارد";
                    break;
                case 813:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 13 در سامانه جامع شاپرک: خطایی نامشخص سمت شاپرک رخ داده است خطای شاپرک";
                    break;
                case 817:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 17 در سامانه جامع شاپرک: نوع گروه را ویرایش نمایید نوع گروه اطلاعات خطا دارد";
                    break;
                case 829:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد29 در سامانه جامع شاپرک: محدودیت سنی لازم برای پذیرنده رعایت نشده است. اطلاعات پذیرنده را ویرایش نمایید";
                    break;
                case 830:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 30 در سامانه جامع شاپرک: کد ملی برای فردی است که فوت شده است اطلاعات پذیرنده را ویرایش نمایید پذیرنده در قید حیات نیست";
                    break;
                case 838:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 38 در سامانه جامع شاپرک: اطلاعات پذیرنده را مجدداً ارسال نمایید. استعلام کد پستی ناموفق انجام شده است";
                    break;
                case 839:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 39 در سامانه جامع شاپرک: کد پستی نادرست. از کد پستی محل اشتغال پذیرنده استعلام بگیرید";
                    break;
                case 843:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 43 در سامانه جامع شاپرک: اطلاعات فیلد مذکور را بررسی نمایید و از صحت اطلاعات اطمینان حاصل نمایید طول رشته فیلد مذکور کمتر / بیشتر از حد مجاز";
                    break;
                case 848:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 48 در سامانه جامع شاپرک: شماره شبا پذیرنده را بررسی نمایید شماره شبا مشکل دارد";
                    break;
                case 855:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 55 در سامانه جامع شاپرک: پذیرنده قدیمی می باشد بدون پایانه می باشد در صورت امکان پذیرنده جدید ثبت نمایید دیتای کانورت قبلی در هنگام درج دیتای جدید، یافت نمی شود";
                    break;
                case 858:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 58 در سامانه جامع شاپرک: اطلاعات پذیرنده را بررسی نمایید و از صحت اطلاعات اطمینان حاصل نمایید رنج فیلد مذکور نادرست است";
                    break;
                case 859:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 59 در سامانه جامع شاپرک: کد صنف یا کد زیر صنف و کد پستی را بررسی نمایید. در صورت امکان پذیرنده جدید ثبت نمایید اطلاعات صنف جدید یا کد پستی با اطلاعات قبلی مطابق نیست";
                    break;
                case 862:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 62 در سامانه جامع شاپرک: کد پستی را بررسی نمایید استان محل اشتغال پذیرنده با کد پستی مطابقت ندارد کد پستی با استان مطابقت ندارد";
                    break;
                case 863:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 63 در سامانه جامع شاپرک: کد پستی را بررسی نمایید شهر محل اشتغال پذیرنده با کد پستی مطابقت ندارد کد پستی با شهر مطابقت ندارد";
                    break;
                case 868:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription =
                        "خطای کد 68 در سامانه جامع شاپرک: پایانه جدید برای پذیرنده ثبت نمایید پایانه توسط نظارت غیرفعال شده است";
                    break;
                case 89999:
                    statusId = (byte) Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 9999 در سامانه جامع شاپرک: مدتی بعد مجدداً استعلام نمایید";
                    break;

                // کد خطاهای ارسال به شاپرک

                case 9110:
                    statusDescription = "کد 9110: در انتظار ارسال به شاپرک";
                    statusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 9120:
                    statusDescription = "کد 9120: در انتظار تایید شاپرک";
                    statusId = (byte) Enums.TerminalStatus.SendToShaparak;
                    break;
                case 9130:
                    statusDescription = "کد 9130: در انتظار تخصیص سریال";
                    statusId = (byte) Enums.TerminalStatus.ReadyForAllocation;
                    break;
                case 9121:
                    statusDescription = "کد 9121: در انتظار ارسال مجدد به شاپرک";
                    statusId = (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    break;
            }

            return (true, statusId, $"{DateTime.Now.ToLongPersianDateTime()} - {statusDescription}");
        }

        /// <summary>
        /// درخواست جمع آوری
        /// </summary>
        public async Task<SendRevokeRequestResponseModel> SendRevokeRequest(long revokeRequestId, string contractNo)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                var result = await _client.CancelPosRequestAsync(Convert.ToInt64(contractNo));

                switch (result)
                {
                    case 1:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                            Result = "کد 1: درخواست با موفقیت لغو گردید."
                        };
                    case 2:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                            Result = "کد 2: درخواست جمع آوری برای پایانه ثبت گردید."
                        };
                    case 500:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 500: درخواست یافت نشد."
                        };
                    case 501:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 501: درخواست یافت نشد."
                        };
                    case 502:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 502: امکان لغو درخواست وجود ندارد."
                        };
                    case 504:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 504: پایانه متناظر یا درخواست یافت نشد."
                        };
                    case 505:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 505: پایانه فعال نمی باشد."
                        };
                    case 506:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 506: درخواست فسخ می بایست از طریق نمایندگی ثبت شود."
                        };
                    case 507:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                            Result = "کد 507: درخواست فسخ قبلاً ثبت شده است."
                        };
                    case 200:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                            Result = "کد 503: خطا در اجرای عملیات لغو."
                        };
                    default:
                        return new SendRevokeRequestResponseModel
                        {
                            IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                            Result = "خطا. کد ناشناخته از سمت وب سرویس"
                        };
                }
            }
            catch (Exception exception)
            {
                exception.AddLogData("RevokeRequestId", revokeRequestId).LogNoContext();

                return new SendRevokeRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = "خطا در اتصال به وب سرویس"
                };
            }
        }


        /// <summary>
        /// درخواست جمع آوری
        /// </summary>
        public async Task<SendRevokeRequestResponseModel> NewSendRevokeRequest(int ReasonId, string TerrminalNo,
            int terminalId, int revokeRequestId)
        {
            try
            {
                RequestRevocationTerminalInput a = new RequestRevocationTerminalInput();
                a.RequestData = new RequestRevocationTerminalRequestData();
                a.RequestData.TerminalNumber = TerrminalNo;
                a.RequestData.RevokeReasonRefId = ReasonId == 1 ? 31838 : 31839;
                var ps = RequestRevocationTerminal(a, terminalId);
                if (ps.RequestResult != null)
                    return new SendRevokeRequestResponseModel
                    {
                        IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                        Result = ps.RequestResult.ResultText
                    };
                return new SendRevokeRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = string.Join(",",
                        ps.ErrorList.Select(v => v.ErrorId + " - " + v.ErrorText).ToArray())
                };


                // switch (result)
                // {
                //     case 1:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                //             Result = "کد 1: درخواست با موفقیت لغو گردید."
                //         };
                //     case 2:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                //             Result = "کد 2: درخواست جمع آوری برای پایانه ثبت گردید."
                //         };
                //     case 500:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 500: درخواست یافت نشد."
                //         };
                //     case 501:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 501: درخواست یافت نشد."
                //         };
                //     case 502:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 502: امکان لغو درخواست وجود ندارد."
                //         };
                //     case 504:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 504: پایانه متناظر یا درخواست یافت نشد."
                //         };
                //     case 505:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 505: پایانه فعال نمی باشد."
                //         };
                //     case 506:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 506: درخواست فسخ می بایست از طریق نمایندگی ثبت شود."
                //         };
                //     case 507:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //             Result = "کد 507: درخواست فسخ قبلاً ثبت شده است."
                //         };
                //     case 200:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                //             Result = "کد 503: خطا در اجرای عملیات لغو."
                //         };
                //     default:
                //         return new SendRevokeRequestResponseModel
                //         {
                //             IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                //             Result = "خطا. کد ناشناخته از سمت وب سرویس"
                //         };
                // }
            }
            catch (Exception exception)
            {
                exception.AddLogData("RevokeRequestId", revokeRequestId).LogNoContext();

                return new SendRevokeRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = "خطا در اتصال به وب سرویس"
                };
            }
        }


        /// <summary>
        /// درخواست تغییر حساب
        /// </summary>
        public async Task<SendChangeAccountRequestResponseModel> NewSendChangeAccountRequest(
            long changeAccountRequestId,
            string ownerFullName, string shebaNo, long branchId, string nationalCode, string terminalNo,
            string oldShebaNo,
            string TaxPayerCode, string AcceptorCode,
            byte[] fileData, int TerminalId, bool isLegalPersonality)
        {
            try
            {
                
                //todo ==>
                if (!isLegalPersonality)
                {
                    var dataContext = new AppDataContext();
                    var ter = dataContext.Terminals.FirstOrDefault(b => b.Id == TerminalId);
                    var MerchantProfileId = ter.MerchantProfileId;
                    var merchantProfile =
                          dataContext.MerchantProfiles.First(x => x.Id == MerchantProfileId);
                    var shebaNumber = ter.ShebaNo;
                    AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNumber, out var accountNumber2);
                    var primaryCustomerNumber = accountNumber2.Split('-')[2];
                    
                    // if (!TosanService.TryGetCustomerInfo(primaryCustomerNumber,
                    //         merchantProfile.CustomerNumber ?? primaryCustomerNumber, out var response, out var errorMessage))
                    // {
                    //    
                    // }
                    //
                  //  var incompleteCustomerInfoMessage = TosanService.GetIncompleteCustomerInfoMessage(response);
                 //   if (!string.IsNullOrEmpty(incompleteCustomerInfoMessage))
                 //   {
                         
                 //   }

                 //   var lastcode = dataContext.ParsianRequests.Where(b => b.Method == "RequestChangeInfo") .ToList();
                //    var j = lastcode.Max(b => JsonConvert.DeserializeObject<RequestChangeInfoInput2>(b.Input).RequestCode);
                var j = 504208;
                    var requestInqueryInput = new RequestChangeInfoInput2();
                    requestInqueryInput.RequestData = new RequestChangeInfoInputData2();
                    requestInqueryInput.RequestCode = j +  1;    
                    requestInqueryInput.RequestData.ChangeInfoTypeRefId = 31286;
                    requestInqueryInput.RequestData.PersonTypeRefId = 31220;
                 //   requestInqueryInput.RequestData.NationalCode =  response.NationalCode;
                  //  requestInqueryInput.RequestData.BirthCertificateNumber = response.IdentityNumber;
                  //  requestInqueryInput.RequestData.CellPhoneNumber = response.Mobile;
                  //  merchantProfile.BirthCrtfctSerial = response.certificateSerial;
                  //  if (response.certificateSeries != null)
                  //  {
                  //      merchantProfile.BirthCrtfctSeriesNumber = !string.IsNullOrEmpty(  response.certificateSeries  )  ?
                   //         response.certificateSeries.Split('-')[1] :null;
                   //     merchantProfile.PersianCharRefId  = !string.IsNullOrEmpty(  response.certificateSeries  ) ?
                   //         response.certificateSeries.Split('-')[0] : null;
                   // }
                    
                    
                    
                 //   requestInqueryInput.RequestData.PersianCharRefId =   GetPersianCharRefId( ! string .IsNullOrEmpty(response.certificateSeries)?
                 //       response.certificateSeries.Split('-')[0] : "");
                    requestInqueryInput.RequestData.BirthCertificateSeriesNumber =     merchantProfile.BirthCrtfctSeriesNumber  ; 
                    dataContext.SaveChanges();

                    if (string.IsNullOrEmpty(merchantProfile.BirthCrtfctSeriesNumber) ||
                        string.IsNullOrEmpty(merchantProfile.PersianCharRefId))
                    {
                     //   var result = RequestChangeInfo(requestInqueryInput, TerminalId);
                       // if (result == null || result.RequestResult.TopiarId == null)
                        //    throw new Exception("اصلاح اطلاعات انجام نشد");
                    }


                
                }
                AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNo, out var accountNumber);

                var input = new RequestChangeAccountInfoInput();

                input.RequestData = new RequestChangeAccountInfoInputData
                {
                    TaxPayerCode = TaxPayerCode, //todo
                    AcceptorCode = AcceptorCode, //todo
                    Ibans = new List<IbanData>()
                };


                var newIbanData = new IbanData
                {
                    Iban = shebaNo,
                    BranchCode = branchId.ToString(),
                    AddOrInActive = "1",
                    IsTerminal = "true",
                    IbanInfo = shebaNo,
                    IsMain = "true"
                };

                input.RequestData.Ibans.Add(newIbanData);

                var sssss = new IbanData
                {
                    Iban = oldShebaNo,
                    BranchCode = branchId.ToString(),
                    AddOrInActive = "0",
                    IsTerminal = "true",
                    IbanInfo = shebaNo,
                    IsMain = "false"
                };

                input.RequestData.Ibans.Add(sssss);
                input.RequestData.Terminals = new List<string> {terminalNo};


                var s = RequestChangeAccountInfo(changeAccountRequestId, input, TerminalId);

                if (s.IsSuccess)
                {
                    return new SendChangeAccountRequestResponseModel
                    {
                        IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                        TopiarId = s.RequestResult.TopiarId,
                        Result = "کد 1: درخواست با موفقیت ثبت شد.", RequestId = TerminalId
                    };
                }
                else
                {
                    return new SendChangeAccountRequestResponseModel
                    {
                        IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                        Error = s.ErrorList != null ? string.Join(",",
                            s.ErrorList.Select(v => v.ErrorId + "  -  " + v.ErrorText).ToArray()) : "",
                        Result = "خطا در پردازش توسط psp", RequestId = TerminalId
                    };
                }
                // var response = await _client.ChangeIbanRequestAsync(new ChangeIbanRequest
                // {
                //     IbanBank = 106,
                //     Iban = shebaNo,
                //     AccDocument = fileData,
                //     AccountNumber = accountNumber,
                //     AccountOwner = ownerFullName,
                //     TermNo = int.Parse(terminalNo),
                //     BranchCode = branchId.ToString(),
                //     NationalCode = nationalCode.Trim()
                // });
                //
                // if (response.ChannelState == 1)
                // {
                //     switch (response.ActionState)
                //     {
                //         case 1:
                //             return new SendChangeAccountRequestResponseModel
                //             {
                //                 IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                //                 Result = "کد 1: درخواست با موفقیت ثبت شد.", RequestId = response.RequestId
                //             };
                //         case -1:
                //             return new SendChangeAccountRequestResponseModel
                //             {
                //                 IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //                 Result = "کد -1: پایانه با کد ملی ارسال شده یافت نشد."
                //             };
                //         case -2:
                //             return new SendChangeAccountRequestResponseModel
                //             {
                //                 IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //                 Result = "کد -2: درخواست تغییر شبا در سیستم موجود می باشد."
                //             };
                //         case -3:
                //             return new SendChangeAccountRequestResponseModel
                //             {
                //                 IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //                 Result =
                //                     "کد -3: در اجرای فرمان مشکل وجود دارد. با تیم پشتیبانی پارسیان تماس حاصل فرمایید."
                //             };
                //         default:
                //             return new SendChangeAccountRequestResponseModel
                //             {
                //                 IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                //                 Result = "خطای ناشناخته در ارسال درخواست تغییر حساب به پارسیان"
                //             };
                //     }
                // }

                return new SendChangeAccountRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0"
                };
            }
            catch (Exception exception)
            {
                exception.AddLogData("ChangeAccountRequestId", changeAccountRequestId).LogNoContext();

                return new SendChangeAccountRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0" +
                             exception.Message
                };
            }
        }

        /// <summary>
        /// درخواست تغییر حساب
        /// </summary>
        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long changeAccountRequestId,
            string ownerFullName, string shebaNo, long branchId, string nationalCode, string terminalNo,
            byte[] fileData)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNo, out var accountNumber);

                var response = await _client.ChangeIbanRequestAsync(new ChangeIbanRequest
                {
                    IbanBank = 106,
                    Iban = shebaNo,
                    AccDocument = fileData,
                    AccountNumber = accountNumber,
                    AccountOwner = ownerFullName,
                    TermNo = int.Parse(terminalNo),
                    BranchCode = branchId.ToString(),
                    NationalCode = nationalCode.Trim()
                });

                if (response.ChannelState == 1)
                {
                    switch (response.ActionState)
                    {
                        case 1:
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = true, StatusId = (byte) Enums.RequestStatus.SentToPsp,
                                Result = "کد 1: درخواست با موفقیت ثبت شد.", RequestId = response.RequestId
                            };
                        case -1:
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                                Result = "کد -1: پایانه با کد ملی ارسال شده یافت نشد."
                            };
                        case -2:
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                                Result = "کد -2: درخواست تغییر شبا در سیستم موجود می باشد."
                            };
                        case -3:
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                                Result =
                                    "کد -3: در اجرای فرمان مشکل وجود دارد. با تیم پشتیبانی پارسیان تماس حاصل فرمایید."
                            };
                        default:
                            return new SendChangeAccountRequestResponseModel
                            {
                                IsSuccess = false, StatusId = (byte) Enums.RequestStatus.Rejected,
                                Result = "خطای ناشناخته در ارسال درخواست تغییر حساب به پارسیان"
                            };
                    }
                }

                return new SendChangeAccountRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0"
                };
            }
            catch (Exception exception)
            {
                exception.AddLogData("ChangeAccountRequestId", changeAccountRequestId).LogNoContext();

                return new SendChangeAccountRequestResponseModel
                {
                    IsSuccess = false, StatusId = (byte) Enums.RequestStatus.WebServiceError,
                    Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0"
                };
            }
        }

        public async Task<(string, string)> CheckChangeAccountRequest(long requestId)
        {
            var response =
                await _client.CheckChangeIbanStatusAsync(new ChangeIbanStatusRequest {RequestId = requestId});

            if (response.ActionState == 1 && response.ChannelState == 1)
            {
                var sheba = response.Data.Last().Iban;
                AccountNumberExtensions.TryGenerateAccountNumberFromSheba(sheba, out var accountNumber);
                return (sheba, accountNumber);
            }

            return (null, null);
        }

        private WsPosRequest GetPosRequest(TerminalInfo terminalInfo)
        {
            var posRequest = new WsPosRequest
            {
                ACC_STATEMENT_CODE = 1, // نحوه تسویه - همیشه یک ارسال شود
                ACC_TYPE_CODE = 1, // نوع حساب  یک: قرض الحسنه  دو: جاری  سه: پس انداز
                BAN_BANK_CODE = 106, // کد مکنا بانک
                BAN_BRANCH_CODE = (short) terminalInfo.BranchId, // کد مکنا شعبه
                BAN_ZONE_CODE = (short) terminalInfo.ParentBranchId, // کد مکنا سرپرستی
                CITY_CODE = (int) terminalInfo.CityId, // کد شهر شاپرکی محل نصب
                CoCa_Address = terminalInfo.Address, // آدرس محل نصب
                CoCa_AddressCode = terminalInfo.Address, // آدرس شاپرکی محل نصب - نیازی به فرمت شاپرکی نیست
                CoCa_City_Code = (int) terminalInfo.CityId, // کد شهر شاپرکی
                CoCa_Fax = "", // نمابر - اختیاری
                CoCa_Pos_Box = terminalInfo.PostCode, // کد پستی 10 رقمی محل کار
                CoCa_Tel = terminalInfo.TelCode + terminalInfo.Tel, // تلفن - اختیاری
                COCU_BIRTH_DATE = terminalInfo.IsLegalPersonality
                    ? int.Parse(terminalInfo.CompanyRegistrationDate.ToPersianDate().Replace("/", string.Empty))
                    : int.Parse(terminalInfo.Birthdate.ToPersianDate()
                        .Replace("/", string.Empty)), // تاریخ تولد / ثبت شرکت
                COCU_ConcatNAME = terminalInfo.IsLegalPersonality
                    ? terminalInfo.Title
                    : terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh() + " " +
                      terminalInfo.LastName.ApplyPersianYeKe()
                          .RemoveHamzeh(), // ترکیب نام و نام خانوادگی برای حقیقی  نام شرکت برای حقوقی
                COCU_Economic_National_Code = terminalInfo.IsLegalPersonality
                    ? terminalInfo.LegalNationalCode.Trim()
                    : terminalInfo.NationalCode
                        .Trim(), // حقیقی: کد ملی 10 رقمی  حقوقی: شماره ثبت شرکت 11 رقمی  اتباع خارجی: کد فراگیر
                COCU_FAMILY =
                    terminalInfo.IsLegalPersonality
                        ? terminalInfo.Title
                        : terminalInfo.LastName, // نام خانوادگی - در صورت حقوقی بودن نام شرکت
                COCU_FATHER_NAME = terminalInfo.IsLegalPersonality
                    ? terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh()
                    : terminalInfo.FatherName.ApplyPersianYeKe()
                        .RemoveHamzeh(), // نام پدر - در صورت حقوقی بودن نام مدیرعامل شرکت
                COCU_ID_CARD_NO = terminalInfo.IsLegalPersonality
                    ? terminalInfo.CompanyRegistrationNumber
                    : terminalInfo
                        .IdentityNumber, // شماره شناسنامه - برای حقوقی شماره ثبت شرکت و برای اتباع خارجی شماره ثبت شرکت
                COCU_ISSUE_CODE =
                    int.Parse(terminalInfo.BirthCertificateIssueDate.ToPersianDate()
                        .Replace("/",
                            string.Empty)), // ایرانی تاریخ صدور شناسنامه برای اتباع خارجی تاریخ انقضاء پاسپورت
                COCU_Mobile = terminalInfo.Mobile, // تلفن همراه 11 رقمی با صفر اول
                COCU_NAME = terminalInfo.IsLegalPersonality
                    ? terminalInfo.Title
                    : terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(), // نام - در صورت حقوقی بودن نام شرکت
                COCU_SEX_CODE =
                    terminalInfo.IsLegalPersonality
                        ? (short) 1
                        : (short) 2, // یک: مرد  دو: زن  در صورت حقیقی بودن یک ارسال شود
                COMC_Bank_Acc =
                    terminalInfo.AccountNo.Replace("-", string.Empty)
                        .Substring(5,
                            13), // شماره حساب - 13 رقم - 13 رقم از سمت راست ببریم. مهم نیست. شماره شبا مهم است
                Comc_Bank_IBAN = terminalInfo.ShebaNo, // شماره شبا - 26 کاراکتر به همراه آی آر
                CoMC_City_Part_Code =
                    Convert.ToInt16(terminalInfo.RegionalMunicipalityId), // منطقه شهرداری - اگر نداشتیم 0 بزنیم
                COMC_CUS_WORK_POSTCODE = terminalInfo.PostCode,
                COMC_STOR_NAME =
                    terminalInfo.Title.Substring(0,
                        terminalInfo.Title.Length > 25
                            ? 25
                            : terminalInfo.Title
                                .Length), // نام فروشگاه حداکثر 25 کاراکتر - اگر 25 کاراکتر بیشتر بود ببریم
                COMC_STOR_NAMEL = terminalInfo.EnglishTitle.Substring(0,
                    terminalInfo.EnglishTitle.Length > 25
                        ? 25
                        : terminalInfo.EnglishTitle
                            .Length), // نام انگلیسی فروشگاه حداکثر 25 کاراکتر - اگر 25 کاراکتر بیشتر بود ببریم
                COMC_STTLMNT_CODE = 1, // نوع تسویه - همیشه یک ارسال شود
                COUNTRY_CODE = 98, // عدد 98 برای ایرانیان و کد کشور برای اتباع خارجی
                CustomerType =
                    terminalInfo.IsLegalPersonality ? (short) 2 : (short) 1, // نوع پذیرنده  1: حقیقی  2: حقوقی
                OrganizationId =
                    "4749585234116348", // کد سازمان درخواست دهنده - این کد میبایست از طرف تجارت الکترونیک اعلام گردد
                RequestCode =
                    (int) terminalInfo
                        .Id, // کد یکتای درخواست - یک کد یکتا به ازای هر درخواست که سازمان درخواست کننده آن را ایجاد می نماید
                ShaparakTermGroup = terminalInfo.GuildId.ToString().PadLeft(8, '0'), // کد صنف شاپرکی
                STATE_CODE = (short) terminalInfo.StateId, // کد استان - مطابق شاپرک
                TermCount = 1, // تعداد پایانه درخواستی
                TermModel = terminalInfo.DeviceTypeId == (long) Enums.DeviceType.GPRS ||
                            terminalInfo.DeviceTypeId == (long) Enums.DeviceType.WIFI ||
                            terminalInfo.DeviceTypeId == (long) Enums.DeviceType.MPOS
                    ? 34
                    : 37, // مدل پایانه  37 برای ثابت و 34 برای سیار
                CoCa_Tel2 = "",
                COCU_FAMILY_ENG = terminalInfo.IsLegalPersonality
                    ? terminalInfo.EnglishTitle
                    : terminalInfo.EnglishLastName, // نام خانوادگی انگیلیسی - در صورت حقوقی بودن نام شرکت
                COCU_NAME_ENG = terminalInfo.IsLegalPersonality
                    ? terminalInfo.EnglishTitle
                    : terminalInfo.EnglishFirstName, // نام انگیلیسی - در صورت حقوقی بودن نام شرکت
                SignBirthDate = 0,
                Sign_First_Name = "",
                Sign_Last_Name = "",
                Sign_National_Code = "",
                Sign_Position = ""
            };

            if (terminalInfo.IsLegalPersonality)
            {
                posRequest.Sign_First_Name = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh();
                posRequest.Sign_Last_Name = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh();
                posRequest.Sign_National_Code = terminalInfo.NationalCode.Trim();
                posRequest.Sign_Position = terminalInfo.SignatoryPosition;
                posRequest.SignBirthDate = int.Parse(terminalInfo.Birthdate.ToPersianDate().Replace("/", string.Empty));
            }

            return posRequest;
        }

        #endregion

        public void Dispose()
        {
            _client.Close();
            ((IDisposable) _client).Dispose();
        }
    }

    public class UpdateStatusForRegisteredTerminalOutput
    {
        // async Task<(bool, byte, string)>  
        public bool IsSuccess { get; set; }
        public byte StatusId { get; set; }
        public string Error { get; set; }
        public string Status { get; set; }
        public DateTime? InstallationDate { get; set; }
        public  DateTime? RevokeDate { get; set; }
    }

    public class ParsianRequestedTerminalResult
    {
        public string StepTitle { get; set; }
        public string StatusTitle { get; set; }
        public string Error { get; set; }
        public byte StatusId { get; set; }
        public string InstallStatus { get; set; }
        public int InstallStatusId { get; set; }
        public int StepCode { get; set; }
        public string StepCodeTitle { get; set; }
        public string TerminalNo { get; set; }
        public string AccecptorCode { get; set; }
        public string TerminalCreateDate { get; set; }
        public string ShaparakRegisterDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public bool IsComplete { get; set; }
    }
}