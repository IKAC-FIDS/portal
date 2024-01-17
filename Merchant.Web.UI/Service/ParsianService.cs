using EntityFramework.Extensions;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ParsianServiceReference;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Parsian;
using TES.Web.Core;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Service
{
    public class OldParsianService : IDisposable
    {
        private readonly PosRequestServiceClient _client;

        public OldParsianService()
        {
            _client = new PosRequestServiceClient();
            _client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            _client.ClientCredentials.UserName.UserName = "sarmayehbank";
            _client.ClientCredentials.UserName.Password = "Sb@123456";
        }

        public void Dispose()
        {
            _client.Close();
            ((IDisposable)_client).Dispose();
        }

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
                    .Where(x => x.Id == terminalId && (x.StatusId == (byte)Enums.TerminalStatus.New || x.StatusId == (byte)Enums.TerminalStatus.NeedToReform) && x.PspId == (byte)Enums.PspCompany.Parsian)
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
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { ContractNo = response.PosTrackIds.Last().ToString(), StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch, Description = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}" });
                        else
                            dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}{Environment.NewLine}{response.Description}" });
                    }
                    else
                    {
                        dataContext.Terminals.Where(x => x.Id == terminalId).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = response.Description });
                    }

                    return true;
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
                    .Where(x => terminalIdList.Contains(x.Id) && x.StatusId == (byte)Enums.TerminalStatus.New && x.PspId == (byte)Enums.PspCompany.Parsian)
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

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
                                dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { ContractNo = response.PosTrackIds.Last().ToString(), StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch, ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}" });
                            else
                                dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = $"{DateTime.Now.ToLongPersianDateTime()} - {actionStateDescription}{Environment.NewLine}{response.Description}" });
                        }
                        else
                        {
                            dataContext.Terminals.Where(x => x.Id == terminalInfo.Id).Update(x => new Terminal { StatusId = (byte)Enums.TerminalStatus.NeedToReform, ErrorComment = response.Description });
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

        public async Task<InqueryAcceptorResult> UpdateTerminalInfo(string contractNo)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
                    return new InqueryAcceptorResult { IsSuccess = false };

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

            return new InqueryAcceptorResult { IsSuccess = false };
        }

        // trackingCode = contractNo
        public async Task<(bool, byte, string)> UpdateStatus(long trackingCode, byte status_Id)
        {
            var statusDescription = string.Empty;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            var result = await _client.CheckPosStatusAsync(trackingCode);

            switch (result)
            {
                case 0:
                    statusDescription = "درخواست با این شماره یافت نشد";
                    break;
                case 1:
                    status_Id = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    statusDescription = "درخواست ثبت شده است";
                    break;
                case 2:
                    status_Id = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    statusDescription = "درخواست مشاهده شده و در دست پیگیری است";
                    break;
                case 3:
                    status_Id = (byte)Enums.TerminalStatus.Allocated;
                    statusDescription = "پایانه تخصیص یافته است";
                    break;
                case 4:
                    status_Id = (byte)Enums.TerminalStatus.Installed;
                    statusDescription = "پایانه نصب و راه اندازی شده است";
                    break;
                case 5:
                    statusDescription = "پایانه تحت تعمیر می باشد";
                    break;
                case 6:
                    status_Id = (byte)Enums.TerminalStatus.Revoked;
                    statusDescription = "پایانه جمع آوری شده است";
                    break;
                case 7:
                    status_Id = (byte)Enums.TerminalStatus.Revoked;
                    statusDescription = "درخواست لغو شده است";
                    break;
                case 10:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    var shaparakStatus = _client.CheckPosShaparakStatus(trackingCode);
                    if (shaparakStatus.ChannelState == 1)
                    {
                        var shaparakStatusDescription = shaparakStatus.Data.FirstOrDefault();
                        statusDescription = shaparakStatusDescription?.Error;
                    }
                    break;

                // خطای شاپرکی - در این حالت بعد از عدد 8 کد خطای شاپرکی آورده می شود مثلاً 839 یعنی خطای 39 از سوی شاپرک

                case 85:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 5 در سامانه جامع شاپرک: فرمت فیلدها را ویرایش نمایید به ازای مقدار یک فیلد، مقداری یافت نشد";
                    break;
                case 86:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 6 در سامانه جامع شاپرک: فرمت فیلدها را ویرایش نمایید فرمت فیلد مذکور درست نمی باشد";
                    break;
                case 811:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 11 در سامانه جامع شاپرک: اطلاعات قبلاً به شاپرک ارسال شده است رکورد تکراری";
                    break;
                case 812:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "اطلاعات کد ملی و یا تاریخ تولد را ویرایش نمایید کد ملی و تاریخ تولد با هم مطابقت ندارد";
                    break;
                case 813:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 13 در سامانه جامع شاپرک: خطایی نامشخص سمت شاپرک رخ داده است خطای شاپرک";
                    break;
                case 817:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 17 در سامانه جامع شاپرک: نوع گروه را ویرایش نمایید نوع گروه اطلاعات خطا دارد";
                    break;
                case 829:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد29 در سامانه جامع شاپرک: محدودیت سنی لازم برای پذیرنده رعایت نشده است. اطلاعات پذیرنده را ویرایش نمایید";
                    break;
                case 830:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 30 در سامانه جامع شاپرک: کد ملی برای فردی است که فوت شده است اطلاعات پذیرنده را ویرایش نمایید پذیرنده در قید حیات نیست";
                    break;
                case 838:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 38 در سامانه جامع شاپرک: اطلاعات پذیرنده را مجدداً ارسال نمایید. استعلام کد پستی ناموفق انجام شده است";
                    break;
                case 839:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 39 در سامانه جامع شاپرک: کد پستی نادرست. از کد پستی محل اشتغال پذیرنده استعلام بگیرید";
                    break;
                case 843:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 43 در سامانه جامع شاپرک: اطلاعات فیلد مذکور را بررسی نمایید و از صحت اطلاعات اطمینان حاصل نمایید طول رشته فیلد مذکور کمتر / بیشتر از حد مجاز";
                    break;
                case 848:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 48 در سامانه جامع شاپرک: شماره شبا پذیرنده را بررسی نمایید شماره شبا مشکل دارد";
                    break;
                case 855:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 55 در سامانه جامع شاپرک: پذیرنده قدیمی می باشد بدون پایانه می باشد در صورت امکان پذیرنده جدید ثبت نمایید دیتای کانورت قبلی در هنگام درج دیتای جدید، یافت نمی شود";
                    break;
                case 858:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 58 در سامانه جامع شاپرک: اطلاعات پذیرنده را بررسی نمایید و از صحت اطلاعات اطمینان حاصل نمایید رنج فیلد مذکور نادرست است";
                    break;
                case 859:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 59 در سامانه جامع شاپرک: کد صنف یا کد زیر صنف و کد پستی را بررسی نمایید. در صورت امکان پذیرنده جدید ثبت نمایید اطلاعات صنف جدید یا کد پستی با اطلاعات قبلی مطابق نیست";
                    break;
                case 862:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 62 در سامانه جامع شاپرک: کد پستی را بررسی نمایید استان محل اشتغال پذیرنده با کد پستی مطابقت ندارد کد پستی با استان مطابقت ندارد";
                    break;
                case 863:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 63 در سامانه جامع شاپرک: کد پستی را بررسی نمایید شهر محل اشتغال پذیرنده با کد پستی مطابقت ندارد کد پستی با شهر مطابقت ندارد";
                    break;
                case 868:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 68 در سامانه جامع شاپرک: پایانه جدید برای پذیرنده ثبت نمایید پایانه توسط نظارت غیرفعال شده است";
                    break;
                case 89999:
                    status_Id = (byte)Enums.TerminalStatus.NeedToReform;
                    statusDescription = "خطای کد 9999 در سامانه جامع شاپرک: مدتی بعد مجدداً استعلام نمایید";
                    break;

                // کد خطاهای ارسال به شاپرک

                case 9110:
                    statusDescription = "کد 9110: در انتظار ارسال به شاپرک";
                    status_Id = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
                    break;
                case 9120:
                    statusDescription = "کد 9120: در انتظار تایید شاپرک";
                    status_Id = (byte)Enums.TerminalStatus.SendToShaparak;
                    break;
                case 9130:
                    statusDescription = "کد 9130: در انتظار تخصیص سریال";
                    status_Id = (byte)Enums.TerminalStatus.ReadyForAllocation;
                    break;
                case 9121:
                    statusDescription = "کد 9121: در انتظار ارسال مجدد به شاپرک";
                    status_Id = (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                    break;
            }

            return (true, status_Id, $"{DateTime.Now.ToLongPersianDateTime()} - {statusDescription}");
        }

        /// <summary>
        /// درخواست جمع آوری
        /// </summary>
        public async Task<SendRevokeRequestResponseModel> SendRevokeRequest(long revokeRequestId, string contractNo)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
                var result = await _client.CancelPosRequestAsync(Convert.ToInt64(contractNo));

                switch (result)
                {
                    case 1:
                        return new SendRevokeRequestResponseModel { IsSuccess = true, StatusId = (byte)Enums.RequestStatus.SentToPsp, Result = "کد 1: درخواست با موفقیت لغو گردید." };
                    case 2:
                        return new SendRevokeRequestResponseModel { IsSuccess = true, StatusId = (byte)Enums.RequestStatus.SentToPsp, Result = "کد 2: درخواست جمع آوری برای پایانه ثبت گردید." };
                    case 500:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 500: درخواست یافت نشد." };
                    case 501:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 501: درخواست یافت نشد." };
                    case 502:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 502: امکان لغو درخواست وجود ندارد." };
                    case 504:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 504: پایانه متناظر یا درخواست یافت نشد." };
                    case 505:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 505: پایانه فعال نمی باشد." };
                    case 506:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 506: درخواست فسخ می بایست از طریق نمایندگی ثبت شود." };
                    case 507:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد 507: درخواست فسخ قبلاً ثبت شده است." };
                    case 200:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = "کد 503: خطا در اجرای عملیات لغو." };
                    default:
                        return new SendRevokeRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = "خطا. کد ناشناخته از سمت وب سرویس" };
                }
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
        public async Task<SendChangeAccountRequestResponseModel> SendChangeAccountRequest(long changeAccountRequestId, string ownerFullName, string shebaNo, long branchId, string nationalCode, string terminalNo, byte[] fileData)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
                            return new SendChangeAccountRequestResponseModel { IsSuccess = true, StatusId = (byte)Enums.RequestStatus.SentToPsp, Result = "کد 1: درخواست با موفقیت ثبت شد.", RequestId = response.RequestId };
                        case -1:
                            return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد -1: پایانه با کد ملی ارسال شده یافت نشد." };
                        case -2:
                            return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد -2: درخواست تغییر شبا در سیستم موجود می باشد." };
                        case -3:
                            return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "کد -3: در اجرای فرمان مشکل وجود دارد. با تیم پشتیبانی پارسیان تماس حاصل فرمایید." };
                        default:
                            return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.Rejected, Result = "خطای ناشناخته در ارسال درخواست تغییر حساب به پارسیان" };
                    }
                }

                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0" };
            }
            catch (Exception exception)
            {
                exception.AddLogData("ChangeAccountRequestId", changeAccountRequestId).LogNoContext();

                return new SendChangeAccountRequestResponseModel { IsSuccess = false, StatusId = (byte)Enums.RequestStatus.WebServiceError, Result = $"{DateTime.Now.ToLongPersianDateTime()} - خطای وب سرویس پارسیان - ChannelState = 0" };
            }
        }

        public async Task<(string, string)> CheckChangeAccountRequest(long requestId)
        {
            var response = await _client.CheckChangeIbanStatusAsync(new ChangeIbanStatusRequest { RequestId = requestId });

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
                BAN_BRANCH_CODE = (short)terminalInfo.BranchId, // کد مکنا شعبه
                BAN_ZONE_CODE = (short)terminalInfo.ParentBranchId, // کد مکنا سرپرستی
                CITY_CODE = (int)terminalInfo.CityId, // کد شهر شاپرکی محل نصب
                CoCa_Address = terminalInfo.Address, // آدرس محل نصب
                CoCa_AddressCode = terminalInfo.Address, // آدرس شاپرکی محل نصب - نیازی به فرمت شاپرکی نیست
                CoCa_City_Code = (int)terminalInfo.CityId, // کد شهر شاپرکی
                CoCa_Fax = "", // نمابر - اختیاری
                CoCa_Pos_Box = terminalInfo.PostCode, // کد پستی 10 رقمی محل کار
                CoCa_Tel = terminalInfo.TelCode + terminalInfo.Tel, // تلفن - اختیاری
                COCU_BIRTH_DATE = terminalInfo.IsLegalPersonality ? int.Parse(terminalInfo.CompanyRegistrationDate.ToPersianDate().Replace("/", string.Empty)) : int.Parse(terminalInfo.Birthdate.ToPersianDate().Replace("/", string.Empty)), // تاریخ تولد / ثبت شرکت
                COCU_ConcatNAME = terminalInfo.IsLegalPersonality ? terminalInfo.Title : terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh() + " " + terminalInfo.LastName.ApplyPersianYeKe().RemoveHamzeh(), // ترکیب نام و نام خانوادگی برای حقیقی  نام شرکت برای حقوقی
                COCU_Economic_National_Code = terminalInfo.IsLegalPersonality ? terminalInfo.LegalNationalCode.Trim() : terminalInfo.NationalCode.Trim(), // حقیقی: کد ملی 10 رقمی  حقوقی: شماره ثبت شرکت 11 رقمی  اتباع خارجی: کد فراگیر
                COCU_FAMILY = terminalInfo.IsLegalPersonality ? terminalInfo.Title : terminalInfo.LastName, // نام خانوادگی - در صورت حقوقی بودن نام شرکت
                COCU_FATHER_NAME = terminalInfo.IsLegalPersonality ? terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh() : terminalInfo.FatherName.ApplyPersianYeKe().RemoveHamzeh(), // نام پدر - در صورت حقوقی بودن نام مدیرعامل شرکت
                COCU_ID_CARD_NO = terminalInfo.IsLegalPersonality ? terminalInfo.CompanyRegistrationNumber : terminalInfo.IdentityNumber, // شماره شناسنامه - برای حقوقی شماره ثبت شرکت و برای اتباع خارجی شماره ثبت شرکت
                COCU_ISSUE_CODE = int.Parse(terminalInfo.BirthCertificateIssueDate.ToPersianDate().Replace("/", string.Empty)), // ایرانی تاریخ صدور شناسنامه برای اتباع خارجی تاریخ انقضاء پاسپورت
                COCU_Mobile = terminalInfo.Mobile, // تلفن همراه 11 رقمی با صفر اول
                COCU_NAME = terminalInfo.IsLegalPersonality ? terminalInfo.Title : terminalInfo.FirstName.ApplyPersianYeKe().RemoveHamzeh(), // نام - در صورت حقوقی بودن نام شرکت
                COCU_SEX_CODE = terminalInfo.IsLegalPersonality ? (short)1 : (short)2, // یک: مرد  دو: زن  در صورت حقیقی بودن یک ارسال شود
                COMC_Bank_Acc = terminalInfo.AccountNo.Replace("-", string.Empty).Substring(5, 13), // شماره حساب - 13 رقم - 13 رقم از سمت راست ببریم. مهم نیست. شماره شبا مهم است
                Comc_Bank_IBAN = terminalInfo.ShebaNo, // شماره شبا - 26 کاراکتر به همراه آی آر
                CoMC_City_Part_Code = Convert.ToInt16(terminalInfo.RegionalMunicipalityId), // منطقه شهرداری - اگر نداشتیم 0 بزنیم
                COMC_CUS_WORK_POSTCODE = terminalInfo.PostCode,
                COMC_STOR_NAME = terminalInfo.Title.Substring(0, terminalInfo.Title.Length > 25 ? 25 : terminalInfo.Title.Length), // نام فروشگاه حداکثر 25 کاراکتر - اگر 25 کاراکتر بیشتر بود ببریم
                COMC_STOR_NAMEL = terminalInfo.EnglishTitle.Substring(0, terminalInfo.EnglishTitle.Length > 25 ? 25 : terminalInfo.EnglishTitle.Length), // نام انگلیسی فروشگاه حداکثر 25 کاراکتر - اگر 25 کاراکتر بیشتر بود ببریم
                COMC_STTLMNT_CODE = 1, // نوع تسویه - همیشه یک ارسال شود
                COUNTRY_CODE = 98, // عدد 98 برای ایرانیان و کد کشور برای اتباع خارجی
                CustomerType = terminalInfo.IsLegalPersonality ? (short)2 : (short)1, // نوع پذیرنده  1: حقیقی  2: حقوقی
                OrganizationId = "4749585234116348", // کد سازمان درخواست دهنده - این کد میبایست از طرف تجارت الکترونیک اعلام گردد
                RequestCode = (int)terminalInfo.Id, // کد یکتای درخواست - یک کد یکتا به ازای هر درخواست که سازمان درخواست کننده آن را ایجاد می نماید
                ShaparakTermGroup = terminalInfo.GuildId.ToString().PadLeft(8, '0'), // کد صنف شاپرکی
                STATE_CODE = (short)terminalInfo.StateId, // کد استان - مطابق شاپرک
                TermCount = 1, // تعداد پایانه درخواستی
                TermModel = terminalInfo.DeviceTypeId == (long)Enums.DeviceType.GPRS || terminalInfo.DeviceTypeId == (long)Enums.DeviceType.WIFI || terminalInfo.DeviceTypeId == (long)Enums.DeviceType.MPOS ? 34 : 37, // مدل پایانه  37 برای ثابت و 34 برای سیار
                CoCa_Tel2 = "",
                COCU_FAMILY_ENG = terminalInfo.IsLegalPersonality ? terminalInfo.EnglishTitle : terminalInfo.EnglishLastName, // نام خانوادگی انگیلیسی - در صورت حقوقی بودن نام شرکت
                COCU_NAME_ENG = terminalInfo.IsLegalPersonality ? terminalInfo.EnglishTitle : terminalInfo.EnglishFirstName, // نام انگیلیسی - در صورت حقوقی بودن نام شرکت
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
    }
}