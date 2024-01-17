using EntityFramework.Extensions;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Fanava;
using TES.Web.Core;
using Enums = TES.Common.Enumerations;
using FanavaReference = TES.Merchant.Web.UI.FanavaServiceReference;

namespace TES.Merchant.Web.UI.Service
{
    public class FanavaService : IDisposable
    {
        private readonly FanavaReference.MerchantContractClient _client;
        private readonly FanavaReference.WSInputLogin _login;

        public FanavaService()
        {
            _client = new FanavaReference.MerchantContractClient();
            _client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            _login = new FanavaReference.WSInputLogin
            {
                Username = "t.sarmayeh",
                Password = "FANtes123456@"
            };
        }

        public void Dispose()
        {
            _client.Close();
            ((IDisposable)_client).Dispose();
        }

        public async Task<bool> IsUp() //برای چک کردن بالا بودن سرویس استفاده می شود
        {
            try
            {
                var findGendersResult = await _client.FindGendersAsync(_login);

                return findGendersResult.Type == FanavaReference.MessageType.Succeed;
            }
            catch
            {
                return false;
            }
        }

        public async Task<InqueryAcceptorResult> TryInqueryAcceptor(string contractNo, long terminalId, byte statusId)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            var result = await _client.FindMerchantContractByContractNoAsync(new FanavaReference.WSInputFindAccount
            {
                ContractNo = contractNo,
                Login = _login
            });

            if (result.Type != FanavaReference.MessageType.Succeed)
            {
                var errorMessage = GetErrorMessageFromOutput(result);
                var exception = new Exception(errorMessage);
                exception.AddLogData("TerminalId", terminalId).LogNoContext();

                return new InqueryAcceptorResult { IsSuccess = false, ErrorComment = errorMessage };
            }

            var resultAsMerchantContract = result as FanavaReference.WSOutputResultMerchantContract;

            byte terminalStatus = statusId;
            string errorComment = string.Empty;
            switch (resultAsMerchantContract.MerchantContract.Status)
            {
                case 1: // ثبت شده توسط نماینده
                    terminalStatus = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 2: // ثبت شده توسط بانک
                    terminalStatus = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 4: // در حال بررسی واحد پذیرندگان
                    terminalStatus = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 8: // تایید شده توسط واحد پذیرندگان
                    terminalStatus = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 16: // ارسال شده به سویچ
                    terminalStatus = (byte)Enums.TerminalStatus.SendToShaparak;
                    break;
                case 32: // آماده تخصیص
                    terminalStatus = (byte)Enums.TerminalStatus.ReadyForAllocation;
                    break;
                case 64: // دریافت شده از سویچ ناموفق
                    terminalStatus = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    break;
                case 128: // نیاز به بررسی
                    terminalStatus = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    break;
                case 256: // تخصیص داده شده
                    terminalStatus = (byte)Enums.TerminalStatus.Allocated;
                    break;
                case 512: // تست شده
                    terminalStatus = (byte)Enums.TerminalStatus.Test;
                    break;
                case 1024: // نصب شده
                    terminalStatus = (byte)Enums.TerminalStatus.Installed;
                    break;
                case 2048: // ابطال شده
                    terminalStatus = (byte)Enums.TerminalStatus.Revoked;
                    break;
                case 4096: // در انتظار ابطال
                    terminalStatus = (byte)Enums.TerminalStatus.WaitingForRevoke;
                    break;
                //case 8192: // اصلاح درخواست شده
                //    terminalStatus =
                //    break;
                //case 65536: // تغییر حساب درخواست شده
                //    terminalStatus = 
                //    break;
                case 131072: // درخواست ابطال فرمی
                    terminalStatus = (byte)Enums.TerminalStatus.WaitingForRevoke;
                    break;
                case 262144: // درخواست ابطال فایلی
                    terminalStatus = (byte)Enums.TerminalStatus.WaitingForRevoke;
                    break;
                //case 524288: // در حال بررسی واحد پذیرندگان نامعتبر سوییچ
                //    terminalStatus = 
                //    break;
                case 1048576: // حذف شده
                    terminalStatus = (byte)Enums.TerminalStatus.Deleted;
                    break;
                //case 2097152: // تایید شده توسط واحد پذیرندگان نامعتبر سوییچ
                //    terminalStatus = (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                //    break;
                //case 4194304: // ترمینال IPG ارسال شده برای پذیرنده
                //    terminalStatus = (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                //    break;
                case 8388608: // نامعتبر وب سرویس آتا
                    terminalStatus = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    errorComment = FindShaparakErrors(contractNo);
                    break;
                case 16777216: // تایید نامعتبر وب سرویس آتا
                    terminalStatus = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    errorComment = FindShaparakErrors(contractNo);
                    break;
                case 33554432: // ابطال نوع سوم
                    terminalStatus = (byte)Enums.TerminalStatus.Revoked;
                    break;
                    //case 67108864: // عدم احراز مدارک
                    //    terminalStatus = 
                    //    break;
            }

            AccountNumberExtensions.TryGenerateAccountNumberFromSheba(resultAsMerchantContract.MerchantContract.AllShaba, out var accountNumber);
            return new InqueryAcceptorResult
            {
                IsSuccess = true,
                AccountNo = accountNumber,
                ErrorComment = errorComment,
                TerminalStatus = terminalStatus,
                ShebaNo = resultAsMerchantContract.MerchantContract.AllShaba,
                TerminalNo = resultAsMerchantContract.MerchantContract.TerminalNo,
                MerchantNo = resultAsMerchantContract.MerchantContract.AcceptorId,
                ContractDate = resultAsMerchantContract.MerchantContract.ContractDateTime,
                BatchDate = resultAsMerchantContract.MerchantContract.SwitchRecievedDateTime,
                InstallationDate = resultAsMerchantContract.MerchantContract.InstalledDateTime,
                RevokeDate = !string.IsNullOrEmpty(resultAsMerchantContract.MerchantContract.DisabledDateTimeShamsi) ? resultAsMerchantContract.MerchantContract.DisabledDateTimeShamsi.ToMiladiDate() : (DateTime?)null
            };
        }

        /// <summary>
        /// درخواست تغییر حساب
        /// </summary>
        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long id, string terminalNo, string contractNo, string newAccountNumber, string oldShebaNumber, string newShebaNumber)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

            try
            {
                var findMerchantContractByContractNoResult = await _client.FindMerchantContractByContractNoAsync(new FanavaReference.WSInputFindAccount
                {
                    Login = _login,
                    ContractNo = contractNo
                });

                var resultAsMerchantContract = findMerchantContractByContractNoResult as FanavaReference.WSOutputResultMerchantContract;


                var xyz = resultAsMerchantContract.MerchantContract.AllAccountNo;
                var result =   _client.ChangeAccountList(new FanavaReference.WSInputChangeAccount
                {
                    Login = _login,
                    ChangeAccountRequests = new[]
                    {
                        new FanavaReference.ChangeAccountRequestEventReport
                        {
                            TerminalNo = terminalNo,
                            AccountOld = xyz,
                            AccountNew = newAccountNumber.Replace("-", ""),
                            ShabaNew = newShebaNumber,
                            ShabaOld = oldShebaNumber,
                            ChangeAccountReason = "بنابر درخواست پذیرنده"
                        }
                    }
                });

                if (result.Type == FanavaReference.MessageType.Succeed)
                {
                    return new SendChangeAccountRequestResponseModel { IsSuccess = true, StatusId = Enums.RequestStatus.SentToPsp.ToByte(), Result = result.Message };
                }

                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = Enums.RequestStatus.NeedToReform.ToByte(), Result = GetErrorMessageFromOutput(result) };
            }
            catch (Exception exception)
            {
                exception.AddLogData("ChangeAccountRequestId", id).LogNoContext();

                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = Enums.RequestStatus.WebServiceError.ToByte(), Result = "خطا در اتصال به وب سرویس." };
            }
        }

        /// <summary>
        /// درخواست جمع آوری
        /// </summary>
        public async Task<SendRevokeRequestResponseModel> SendRevokeRequest(long revokeRequestId, string terminalNo, string description)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

            try
            {
                var result = await _client.AddDisableRequestOneAsync(new FanavaReference.WSInputDisableRequest
                {
                    Login = _login,
                    DisableRequest = new FanavaReference.WSInputDisableRequestField
                    {
                        DisableReason = 1,
                        TerminalNo = terminalNo,
                        ApplicantDesc = description
                    }
                });

                if (result.Type == FanavaReference.MessageType.Succeed)
                {
                    return new SendRevokeRequestResponseModel { IsSuccess = true, StatusId = (byte)Enums.RequestStatus.SentToPsp, Result = result.Message };
                }

                return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.NeedToReform, Result = result.Message };
            }
            catch (Exception exception)
            {
                exception.AddLogData("RevokeRequestId", revokeRequestId).LogNoContext();

                return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = "خطا در اتصال به وب سرویس." };
            }
        }

        public bool EditAcceptor(long terminalId)
        {
            using (var dataContext = new AppDataContext())
            {
                var terminalInfo = dataContext.Terminals
                    .Where(x => x.Id == terminalId && x.StatusId == (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        Title = x.Title,
                        CityId = x.CityId,
                        Address = x.Address,
                        ShebaNo = x.ShebaNo,
                        GuildId = x.GuildId,
                        PostCode = x.PostCode,
                        BranchId = x.BranchId,
                        AccountNo = x.AccountNo,
                        ContractNo = x.ContractNo,
                        MerchantNo = x.MerchantNo,
                        ContractDate = x.ContractDate,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        EnglishAddress = x.EnglishAddress,
                        ActivityTypeId = x.ActivityTypeId,
                        Mobile = x.MerchantProfile.Mobile,
                        IsMale = x.MerchantProfile.IsMale,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        HomeAddress = x.MerchantProfile.HomeAddress,
                        NationalCode = x.MerchantProfile.NationalCode,
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                        TacNumber = x.TaxPayerCode
                    })
                    .First();

                try
                {
                    var merchantContractAdd = GetMerchantContractInput(terminalInfo, true);

                    if (merchantContractAdd == null)
                    {
                        return false;
                    }

                    var result = _client.ChangeMerchantContractWithShaparakCoding(merchantContractAdd);

                    if (result.Type == FanavaReference.MessageType.Succeed)
                    {
                        var resultAsMerchantContract = result as FanavaReference.WSOutputResultMerchantContract;
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ContractNo = resultAsMerchantContract.MerchantContract.ContractNo, StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });

                        return true;
                    }

                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ErrorComment = GetErrorMessageFromOutput(result) });

                    return false;
                }
                catch (Exception exception)
                {
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, ErrorComment = "خطا در اتصال به وب سرویس" });
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
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        Title = x.Title,
                        CityId = x.CityId,
                        Address = x.Address,
                        ShebaNo = x.ShebaNo,
                        GuildId = x.GuildId,
                        PostCode = x.PostCode,
                        BranchId = x.BranchId,
                        AccountNo = x.AccountNo,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        EnglishAddress = x.EnglishAddress,
                        ActivityTypeId = x.ActivityTypeId,
                        Mobile = x.MerchantProfile.Mobile,
                        IsMale = x.MerchantProfile.IsMale,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        HomeAddress = x.MerchantProfile.HomeAddress,
                        NationalCode = x.MerchantProfile.NationalCode,
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                        TacNumber = x.TaxPayerCode
                    })
                    .First(x => x.Id == terminalId);

                try
                {
                    var merchantContractInput = GetMerchantContractInput(terminalInfo);

                    if (merchantContractInput == null)
                    {
                        return false;
                    }

                    var result = _client.AddMerchantContractWithShaparakCoding(merchantContractInput);

                    if (result.Type == FanavaReference.MessageType.Succeed)
                    {
                        var resultAsMerchantContract = result as FanavaReference.WSOutputResultMerchantContract;
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ContractNo = resultAsMerchantContract.MerchantContract.ContractNo, StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });

                        return true;
                    }

                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = GetErrorMessageFromOutput(result) });

                    return false;
                }
                catch (Exception exception)
                {
                    dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = "خطا در اتصال به وب سرویس" });
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
                    .Where(x => terminalIdList.Contains(x.Id) && x.StatusId == (byte)Enums.TerminalStatus.New && x.PspId == (byte)Enums.PspCompany.Fanava)
                    .Select(x => new TerminalInfo
                    {
                        Id = x.Id,
                        Tel = x.Tel,
                        Title = x.Title,
                        CityId = x.CityId,
                        Address = x.Address,
                        ShebaNo = x.ShebaNo,
                        GuildId = x.GuildId,
                        PostCode = x.PostCode,
                        BranchId = x.BranchId,
                        AccountNo = x.AccountNo,
                        DeviceTypeId = x.DeviceTypeId,
                        EnglishTitle = x.EnglishTitle,
                        EnglishAddress = x.EnglishAddress,
                        ActivityTypeId = x.ActivityTypeId,
                        Mobile = x.MerchantProfile.Mobile,
                        IsMale = x.MerchantProfile.IsMale,
                        ParentBranchId = x.Branch.ParentId,
                        LastName = x.MerchantProfile.LastName,
                        Birthdate = x.MerchantProfile.Birthdate,
                        FirstName = x.MerchantProfile.FirstName,
                        FatherName = x.MerchantProfile.FatherName,
                        HomeAddress = x.MerchantProfile.HomeAddress,
                        NationalCode = x.MerchantProfile.NationalCode,
                        ShaparakAddressFormat = x.ShaparakAddressFormat,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        EnglishLastName = x.MerchantProfile.EnglishLastName,
                        EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                        EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                        LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                        IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                        CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                        CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                        BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                        TacNumber = x.TaxPayerCode
                    })
                    .ToList();

                foreach (var terminalInfo in terminalInfoList)
                {
                    try
                    {
                        var merchantContractAdd = GetMerchantContractInput(terminalInfo);

                        if (merchantContractAdd == null)
                        {
                            continue;
                        }

                        var result = _client.AddMerchantContractWithShaparakCoding(merchantContractAdd);

                        if (result.Type == FanavaReference.MessageType.Succeed)
                        {
                            var resultAsMerchantContract = result as FanavaReference.WSOutputResultMerchantContract;
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { ContractNo = resultAsMerchantContract.MerchantContract.ContractNo, StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = GetErrorMessageFromOutput(result) });
                        }
                    }
                    catch (Exception exception)
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = "خطا در اتصال به وب سرویس" });
                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            }
        }

        // Helpers
        private int GetBusinessType(byte activityTypeId)
        {
            //switch (activityTypeId)
            //{
            //    case 1:
            //        return 0;
            //    case 2:
            //        return 2;
            //    case 3:
            //        return 1;
            //    default:
            //        return 0;
            //}

            return 0; // فقط فیزیکی میفرستیم سمت فن آوا
        }

        private int? GetDeviceTypeId(long deviceTypeId)
        {
            switch (deviceTypeId)
            {
                case 1:
                    return 1;
                case 3:
                    return 2;
                case 7:
                    return 128;
                case 8:
                    return 4;
                case 9:
                    return 8;
                default:
                    return null;
            }
        }

        private FanavaReference.WSInputMerchantContractAdd GetMerchantContractInput(TerminalInfo terminalInfo, bool? isForEdit = false)
        {
            using (var dataContext = new AppDataContext())
            {
                long? deviceTypeId = GetDeviceTypeId(terminalInfo.DeviceTypeId);

                if (!deviceTypeId.HasValue)
                {
                    dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = "نوع دستگاه معتبر نمی باشد." });

                    return null;
                }

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

                var person = new FanavaReference.Person
                {
                    VitalStatus = 0, // در قید حیات صفر
                    FK_Nationality = 1, // ایران یک
                    Mobile = terminalInfo.Mobile,
                    LName = terminalInfo.LastName.RemoveHamzeh(),
                    FName = terminalInfo.FirstName.RemoveHamzeh(),
                    DateBirth = terminalInfo.Birthdate,
                    FatherName = terminalInfo.FatherName.RemoveHamzeh(),
                    HomeAddress = terminalInfo.HomeAddress,
                    LNameEn = terminalInfo.EnglishLastName,
                    FNameEn = terminalInfo.EnglishFirstName,
                    BirthCertNo = terminalInfo.IdentityNumber,
                    FatherNameEn = terminalInfo.EnglishFatherName,
                    NationalCode = terminalInfo.NationalCode.Trim(),
                    DateBirthIssue = terminalInfo.BirthCertificateIssueDate,
                    DateBirthShamsi = terminalInfo.Birthdate.ToPersianDate(),
                    Gender = terminalInfo.IsMale ? 1 : 0 // زن صفر  مرد یک
                };

                var bank = new FanavaReference.BankActor { Code = "058", ActorType = 2 };
                var shobe = new FanavaReference.BankActor { Code = terminalInfo.ParentBranchId.ToString(), ActorType = 1024, Parent = bank };
                var account = new FanavaReference.Account
                {
                    FK_BankId = 247095,
                    Shaba = terminalInfo.ShebaNo,
                    AccountNo = terminalInfo.AccountNo,
                    AccountOwner = new FanavaReference.AccountOwner { Person = person },
                    BankActor = new FanavaReference.BankActor { Code = terminalInfo.BranchId.ToString(), ActorType = 512, Parent = shobe }
                };

                var merchant = new FanavaReference.Merchant
                {
                    OwnershipType = 0,
                    Tel = terminalInfo.Tel,
                    FK_StructureId = "ES-017",
                    Moblie = terminalInfo.Mobile,
                    ZipCode = terminalInfo.PostCode,
                    NameEn = terminalInfo.EnglishTitle,
                    MerchantAddress = terminalInfo.Address,
                    Name = terminalInfo.Title.ApplyPersianYeKe(),
                    MerchantAddressEn = terminalInfo.EnglishAddress,
                    MerchantAddressSh = terminalInfo.ShaparakAddressFormat,
                    BusinessType = GetBusinessType(terminalInfo.ActivityTypeId),
                    Category = new FanavaReference.Category { Code = terminalInfo.GuildId.ToString().PadLeft(8, '0') },
                    Location = new FanavaReference.Location { ShaparakCode = terminalInfo.CityId.ToString() },
                    IsMerchantLegal = terminalInfo.IsLegalPersonality ? 1 : 0,
                    MerchantPeople = new List<FanavaReference.MerchantPerson>
                        {
                            new FanavaReference.MerchantPerson { FK_PositionId = 3, Person = person }
                        }.ToArray()
                };

                if (terminalInfo.IsLegalPersonality)
                {
                    merchant.MerchantLegal = new FanavaReference.MerchantLegal
                    {
                        CompanyNationalId = terminalInfo.LegalNationalCode.Trim(),
                        CompanyRegisterNo = terminalInfo.CompanyRegistrationNumber,
                        CompanyRegisterDateShamsi = terminalInfo.CompanyRegistrationDate.ToPersianDate()
                    };
                }

                var findAgentResult = _client.FindAgentByCityShaparakCodeAndBankId(new FanavaReference.WSInputFindPspByCityShaparakCode
                {
                    Login = _login,
                    BankId = 247095,
                    CityShaparakCode = terminalInfo.CityId.ToString()
                });

                var agentId = (findAgentResult as FanavaReference.WSOutputPspActorRequestList).PspActors.First().Id;

                var merchantcontract = new FanavaReference.MerchantContract
                {
                    MarketerType = 2,
                    TaxNumber = terminalInfo.TacNumber,
                    Merchant = merchant,
                    FK_AgentId = agentId,
                    FK_SwitchId = 486548,
                    FK_MarketerId = 247095,
                    FK_BankContractId = 65,
                    PosModel = deviceTypeId.Value,
                    WSRequestId = Guid.NewGuid().ToString(),
                    MchContractAccounts = new List<FanavaReference.MchContractAccount>
                        {
                            new FanavaReference.MchContractAccount
                            {
                                Percentage = 100, //درصد تسهیم حساب 
                                Account = account
                            }
                        }.ToArray(),
                    MchCtrDocuments = new FanavaReference.MchCtrDocument[0]
                };

                if (isForEdit == true)
                {
                    merchantcontract.ContractNo = terminalInfo.ContractNo;
                    merchantcontract.AcceptorId = terminalInfo.MerchantNo;
                  
                }

                return new FanavaReference.WSInputMerchantContractAdd
                {
                    Login = _login,
                    MerchantContract = merchantcontract   
                    
                  
                     
                };
            }
        }

        private string GetErrorMessageFromOutput(FanavaReference.WSOutput output)
        {
            string errorComment;
            var errorResult = output as FanavaReference.WSOutputErrorList;
            if (errorResult.WSErrors.Any())
            {
                var errors = errorResult.WSErrors.Select(x => $"ErrorCode: {x.ErrorCode} \n\r Message: {x.Message}");
                errorComment = string.Join(Environment.NewLine, errors);
            }
            else
            {
                errorComment = errorResult.Message;
            }

            return errorComment;
        }

        private string FindShaparakErrors(string contractNo)
        {
            var result = _client.FindShaparakInValidListByContractNo(new FanavaReference.WSInputFindShaparakInvalidList()
            {
                Login = _login,
                MerchantContracts = new List<FanavaReference.MerchantContract>
                {
                    new FanavaReference.MerchantContract { ContractNo = contractNo }
                }.ToArray()
            });

            var errorComment = string.Empty;
            if (result.Type == FanavaReference.MessageType.Succeed)
            {
                var shaparakErrors = (result as FanavaReference.WSOutputShaparakList)?.ShaparakFailureReport.Select(x => x.ShpMessege).ToList();
                if (shaparakErrors != null && shaparakErrors.Any())
                {
                    errorComment = string.Join(Environment.NewLine, shaparakErrors);
                }
            }

            return errorComment;
        }

        public class TerminalInfo
        {
            public long Id { get; set; }
            public string MerchantNo { get; set; }
            public string ContractNo { get; set; }
            public long DeviceTypeId { get; set; }
            public string TerminalNo { get; set; }
            public string Title { get; set; }
            public string EnglishTitle { get; set; }
            public long BranchId { get; set; }
            public string AccountNo { get; set; }
            public string ShebaNo { get; set; }
            public byte StatusId { get; set; }
            public long CityId { get; set; }
            public string Tel { get; set; }
            public string Address { get; set; }
            public string PostCode { get; set; }
            public DateTime? ContractDate { get; set; }
            public DateTime SubmitTime { get; set; }
            public long UserId { get; set; }
            public long GuildId { get; set; }
            public byte ActivityTypeId { get; set; }
            public string ShaparakAddressFormat { get; set; }
            public string EnglishAddress { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string EnglishFirstName { get; set; }
            public string EnglishLastName { get; set; }
            public string NationalCode { get; set; }
            public bool IsMale { get; set; }
            public string Mobile { get; set; }
            public string HomeAddress { get; set; }
            public bool IsLegalPersonality { get; set; }
            public string FatherName { get; set; }
            public string IdentityNumber { get; set; }
            public DateTime Birthdate { get; set; }
            public string CompanyRegistrationNumber { get; set; }
            public DateTime? CompanyRegistrationDate { get; set; }
            public string LegalNationalCode { get; set; }
            public DateTime BirthCertificateIssueDate { get; set; }
            public string EnglishFatherName { get; set; }
            public long? ParentBranchId { get; set; }
            public string TacNumber { get; set; }
        }
    }
}