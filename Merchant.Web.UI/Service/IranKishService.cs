using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; 
using System.ServiceModel; 
using System.Threading.Tasks; 
using Stimulsoft.Base.Json;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.IranKishServiceRefrence;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Irankish;
using AcceptorTypes = TES.Merchant.Web.UI.IranKishServiceRefrence.AcceptorTypes;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Service
{
    public class IranKishService : IDisposable
    {
        private readonly DefineMerchantsClient _client; 

        public IranKishService( )
        {
          
            _client = new DefineMerchantsClient();
            _client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode
                = System.ServiceModel.Security.X509CertificateValidationMode.None;
            _client.ClientCredentials.UserName.UserName = "sarmayeh";
            _client.ClientCredentials.UserName.Password = "67c2d5";
            
        }

        public void Dispose()
        {
            ((IDisposable)_client).Dispose();
        }

        public  GetActivitiesResponse[] IsUp()
        {
            
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                var getActivityInfoResult =   _client.GetActivityInfo(0);
                return getActivityInfoResult ;
            
        }

        public async Task<InqueryAcceptorResult> TryInqueryAcceptor(string terminalNo, long terminalId, byte statusId)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                var result = await _client.AcceptorInqueryAsync(terminalNo);
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

                var lastAccount = result.Accounts.Any(x => x.AccountStatus == "8") ? result.Accounts.Last(x => x.AccountStatus == "8") : result.Accounts.Last(); // 8 فعال
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
                    AccountNo = $"{accountNumber.Substring(0, 4)}-{accountNumber.Substring(4, 3)}-{accountNumber.Substring(7, 8)}-{accountNumber.Substring(15, 3)}"
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
        public async Task<SendRevokeRequestResponseModel> SendRevokeRequest(long revokeRequestId, string terminalNo, string description)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                // عدد 72 یعنی انصراف به درخواست بانک
                var result = await _client.RefuseRequestAsync(terminalNo, description, 72);

                if (result.Result)
                {
                    return new SendRevokeRequestResponseModel { IsSuccess = true, StatusId = (byte)Enums.RequestStatus.SentToPsp, Result = result.Message };
                }

                var irankishRequest = new IrankishRequest
                {
                    Input = JsonConvert.SerializeObject(new { terminalNo = terminalNo}),
                    Result = JsonConvert.SerializeObject(result),
                    Method = "_client.EditAcceptor",
                    Module = "_client.EditAcceptor"
                };
                using (var dataContext = new AppDataContext())
                {
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();
                }

                return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.NeedToReform, Result = result.Message };
            }
            catch (Exception exception)
            {
                exception.AddLogData("RevokeRequestId", revokeRequestId).LogNoContext();

                return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = "خطا در اتصال به وب سرویس" };
            }
        }

        /// <summary>
        /// درخواست تغییر حساب
        /// </summary>
        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long id, string oldAccountNumber,
            string newAccountNumber,
            string newShebaNumber,
            string firstName,
            string lastName,
            string merchantNumber,
            long branchId,
            byte[] fileData)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                var result =   _client.ChangeAccountNO(oldAccountNumber.Replace("-", ""),
                    newAccountNumber.Replace("-", ""),
                    newShebaNumber,
                    1,
                    merchantNumber,
                    fileData,
                    fileData,
                    null,
                    null,
                    lastName.RemoveHamzeh(),
                    firstName.RemoveHamzeh(),
                    branchId.ToString());

                if (result == "درخواست با موفقیت ثبت شد." || result == "درخواست تغییر حساب قبلا ثبت شده است.")
                {
                    return new SendChangeAccountRequestResponseModel { IsSuccess = true, StatusId = Enums.RequestStatus.SentToPsp.ToByte(), Result = result };
                }

                var input = new
                {
                    oldAccountNumber = oldAccountNumber.Replace("-", ""),
                    newAccountNumber = newAccountNumber.Replace("-", ""),
                    newShebaNumber = newShebaNumber,
                    accountType = 1,
                    merchantNumber,
                    fileData,


                    lastName = lastName.RemoveHamzeh(),
                    firstName = firstName.RemoveHamzeh(),
                    branchId = branchId.ToString()

                };
                var irankishRequest = new IrankishRequest
                {
                    
                    Input = JsonConvert.SerializeObject(input ),
                    Result = JsonConvert.SerializeObject(result),
                    TerminalId = (int)id,
                    Method = "_client.ChangeAccountNo",
                    Module = "_client.ChangeAccountNo"
                };
                using (var dataContext = new AppDataContext())
                {
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();
                }
                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = Enums.RequestStatus.NeedToReform.ToByte(), Result = result };
            }
            catch (Exception exception)
            {
                exception.AddLogData("TerminalId", id).LogNoContext();

                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = Enums.RequestStatus.WebServiceError.ToByte(), Result = "خطا در اتصال به وب سرویس." };
            }
        }

        public bool EditAcceptor(long terminalId)
        {
            Console.WriteLine($"start ====>{DateTime.Now}");
            var inqueryResult = _client.Inquery(terminalId.ToString(), 6830);
            Console.WriteLine($"End ====>{DateTime.Now}");

            if (string.IsNullOrEmpty(inqueryResult.Acceptor))
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
                    x.MerchantNo,
                    x.Email,
                    x.WebUrl,
                })
                .First(x => x.Id == terminalId);

                try
                {
                    var editedAcceptorEntity = new EditedAcceptorEntity
                    {
                        AcceptorNo = terminalInfo.MerchantNo,
                        AcceptorCeoBirthdate = terminalInfo.Birthdate,
                        AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg : AcceptorTypes.Pos,
                        
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,
                        TechEmail = terminalInfo.Email,
                        
                        Activity = terminalInfo.GuildId.ToString(),
                        Address = terminalInfo.ShaparakAddressFormat,
                        Bussiness = terminalInfo.ParentGuildId.HasValue ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0') : string.Empty,
                        City = terminalInfo.CityId.ToString(),
                        EntityType = terminalInfo.IsLegalPersonality ? EntityTypes.LocalLegalAcceptor : EntityTypes.LocalRealAcceptor,
                        FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                        FoundationDate = terminalInfo.CompanyRegistrationDate,
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
                        licenseNumber = terminalInfo.CompanyRegistrationNumber
                    };

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                    var result = _client.EditAcceptor(editedAcceptorEntity);

                    if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });

                        return true;
                    }

                    var errors = result.Errors.Select(x => $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = string.Join(Environment.NewLine, errors) });

                    
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

                    if (exceptionType == typeof(EndpointNotFoundException) || exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                    }

                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
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
                    x.WebUrl
                })
                .First(x => x.Id == terminalId);

                try
                {
                 
                    var acceptorEntity = new AcceptorEntity
                    {
                        AcceptorCeoBirthdate = terminalInfo.Birthdate,
                        AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg :  AcceptorTypes.Pos,
                        //شماره شناسنامه
                        IdentifierNumber  = terminalInfo.IdentityNumber == "0" ?  terminalInfo.NationalCode.Trim() :  terminalInfo.IdentityNumber,
                        IsVirtual = terminalInfo.IsVirtualStore,
                        Account = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0'),
                        Activity = terminalInfo.GuildId.ToString().PadLeft(8, '0'),
                        Address = terminalInfo.ShaparakAddressFormat,
                        Branch = terminalInfo.BranchId.ToString(),
                        Bussiness = terminalInfo.ParentGuildId.HasValue ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0') : string.Empty,
                        City = terminalInfo.CityId.ToString(),
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,
                        TechEmail = terminalInfo.Email,
                        EntityType = terminalInfo.IsLegalPersonality ? EntityTypes.LocalLegalAcceptor : EntityTypes.LocalRealAcceptor,
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
                        TerminalType = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS ? "BTP" : terminalInfo.DeviceTypeCode, // ایرانکیش ام پوس نداره و اگر ام پوس بود باید به صورت بلوتوث فرستاده شود
                        TrackId = terminalInfo.Id.ToString(),
                        Zipcode = terminalInfo.PostCode.Trim(),
                        TaxFollowupCode   =   terminalInfo.TaxPayerCode,
                        licenseNumber = terminalInfo.CompanyRegistrationNumber
                        
                    };

 
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                    var result = _client.AddAcceptor(acceptorEntity);

                    var tt  = JsonConvert.SerializeObject(acceptorEntity);
                    if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        return true;
                    }

                    var errors = result.Errors.Select(x => $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = string.Join(Environment.NewLine, errors) });

                    //todo ==> add irankish request
                    var irankishRequest = new IrankishRequest
                    {
                        Input = JsonConvert.SerializeObject(acceptorEntity),
                        Result = JsonConvert.SerializeObject(result),
                        TerminalId = (int)terminalInfo.Id,
                        Method = "_client.AddAcceptor",
                        Module = "_client.AddAcceptor"
                    };
                    dataContext.IrankishRequest.Add(irankishRequest);
                    dataContext.SaveChanges();
                    return true;
                }
                catch (Exception exception)
                {
                    var exceptionType = exception.GetType();

                    if (exceptionType == typeof(EndpointNotFoundException) || exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                    }

                    exception.AddLogData("TerminalId", terminalId).LogNoContext();

                    return false;
                }
            }
        }

        
          public bool EditAcceptorRequest (long terminalId)
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
                        IdentifierNumber  = terminalInfo.IdentityNumber == "0" ?  terminalInfo.NationalCode.Trim() :  terminalInfo.IdentityNumber,
                     
                        TechEmail = terminalInfo.Email,
                        Email = terminalInfo.Email,
                        WebUrl = terminalInfo.WebUrl,
                        
                        Account = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0'),
                        Activity = terminalInfo.GuildId.ToString().PadLeft(8, '0'),
                        Address = terminalInfo.ShaparakAddressFormat,
                    //    Branch = terminalInfo.BranchId.ToString(),
                        Bussiness = terminalInfo.ParentGuildId.HasValue ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0') : string.Empty,
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

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                    var result = _client.EditAcceptorRequest(acceptorEntity);

                    var tt  = JsonConvert.SerializeObject(acceptorEntity);
                    if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        return true;
                    }

                    var errors = result.Errors.Select(x => $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = string.Join(Environment.NewLine, errors) });

                    return true;
                }
                catch (Exception exception)
                {
                    var exceptionType = exception.GetType();

                    if (exceptionType == typeof(EndpointNotFoundException) || exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
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
                        x.WebUrl ,
                        x.IsVirtualStore
                    })
                    .Where(x => terminalIdList.Contains(x.Id) && x.StatusId == (byte)Enums.TerminalStatus.New && x.PspId == (byte)Enums.PspCompany.IranKish)
                    .ToList();

                foreach (var terminalInfo in terminalInfoList)
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                        var result = _client.AddAcceptor(new AcceptorEntity
                        {
                            AcceptorCeoBirthdate = terminalInfo.Birthdate,
                            AcceptorType = terminalInfo.DeviceTypeId == 22 ? AcceptorTypes.Ipg : AcceptorTypes.Pos,
                            Account = terminalInfo.AccountNo.Replace("-", "").PadLeft(19, '0'),
                            Activity = terminalInfo.GuildId.ToString(),
                            Address = terminalInfo.ShaparakAddressFormat,
                            Branch = terminalInfo.BranchId.ToString(),
                            Bussiness = terminalInfo.ParentGuildId.HasValue ? terminalInfo.ParentGuildId.ToString().PadLeft(4, '0') : string.Empty,
                            City = terminalInfo.CityId.ToString(),
                            Email = terminalInfo.Email ,
                            WebUrl = terminalInfo.WebUrl,
                            IsVirtual = terminalInfo.IsVirtualStore,

                            TechEmail = terminalInfo.Email,
                            EntityType = terminalInfo.IsLegalPersonality ? EntityTypes.LocalLegalAcceptor : EntityTypes.LocalRealAcceptor,
                            FirstName = terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(),
                            FoundationDate = terminalInfo.CompanyRegistrationDate,
                            Iban = terminalInfo.ShebaNo,
                            IsPcPos = false,
                            IsSwitchTerminal = false,
                            LastName = terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(),
                            LegalEntityTitle = terminalInfo.Title,
                            LegalNationalId = terminalInfo.LegalNationalCode.Trim(),
                            MerchantName = terminalInfo.Title.ApplyPersianYeKe(),
                            Mobile = terminalInfo.Mobile,
                            Nationality = terminalInfo.NationalityCode,
                            Phone = (terminalInfo.TelCode + terminalInfo.Tel).Replace("-", "").Replace(" ", ""),
                            Province = terminalInfo.StateCode,
                            Qty = 1,
                            RealNationalId = terminalInfo.NationalCode.Trim(),
                            TerminalType = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS ? "BTP" : terminalInfo.DeviceTypeCode,
                            TrackId = terminalInfo.Id.ToString(),
                            Zipcode = terminalInfo.PostCode,
                            licenseNumber = terminalInfo.CompanyRegistrationNumber,
                        });

                        if (result.Status.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        }
                        else
                        {
                            var errors = result.Errors.Select(x => $"{DateTime.Now.ToPersianDateTime()}{Environment.NewLine}Code: {x.Code}{Environment.NewLine}PersianDescription: {x.PersianDescription}{Environment.NewLine}Description: {x.Description}");
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = string.Join(Environment.NewLine, errors) });
                        }
                    }
                    catch (Exception exception)
                    {
                        var exceptionType = exception.GetType();

                        if (exceptionType == typeof(EndpointNotFoundException) || exceptionType == typeof(TimeoutException) || exceptionType == typeof(CommunicationException))
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = "خطا در برقراری ارتباط با وب سرویس" });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = exception.Message });
                        }

                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            }
        }

        public InqueryResponse Inquery(string trackId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

            return _client.Inquery(trackId, 6830);
        }

        public  async Task<AddDocumentresponse>   AddDocument(DocumentEntity documentEntity)
        {
           
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

             
            var result = await _client.AddDocumentAsync( documentEntity);

            var l = result.Status;
             
            return result;
        }
    }
}