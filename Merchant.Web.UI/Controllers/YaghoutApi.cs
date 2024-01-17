using System;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using StackExchange.Exceptional;
using TES.Common.Extensions;
using TES.Merchant.Web.UI.AccountNumberVerificationService;
using TES.Merchant.Web.UI.Service.Models;

namespace TES.Merchant.Web.UI.Controllers
{
    public class YaghoutApiController : Controller
    {
        [HttpGet]
        public JsonResult TryGetCustomersOfAccountNumber(string accountNumber)
        {
            GetCustomersOfAccountNumberResult result = new GetCustomersOfAccountNumberResult();
            IEnumerable<TosanCustomerOfAccountNumber> customers = null;
            string errorMessage = null;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            try
            {
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    var sessionId = service.login(new[] {new AccountNumberVerificationService.contextEntry()},
                        new AccountNumberVerificationService.userInfoRequestBean
                            {username = "tes-portal", password = "14186621"});

                    var splittedAccountNumber = accountNumber.Split('-');
                    var optimizedAccountNumber =
                        $"{splittedAccountNumber[0].TrimStart('0')}-{splittedAccountNumber[1].TrimStart('0')}-{splittedAccountNumber[2].TrimStart('0')}-{splittedAccountNumber[3].TrimStart('0')}";

                    var getDepositCustomerResponse = service.getDepositCustomer(
                        new[]
                        {
                            new AccountNumberVerificationService.contextEntry {key = "SESSIONID", value = sessionId}
                        },
                        new AccountNumberVerificationService.depositCustomerRequestBean
                        {
                            depositNumber = optimizedAccountNumber, lengthSpecified = true, offsetSpecified = true,
                            offset = 0, length = 100
                        });

                    customers = getDepositCustomerResponse.depositCustomers.Select(x => new TosanCustomerOfAccountNumber
                        {CustomerNumber = x.cif, Name = x.name});

                    if (!getDepositCustomerResponse.depositCustomers.Any())
                    {
                        errorMessage = "با شماره حساب وارد شده هیچ اطلاعاتی از وب سرویس فرا‌ نگین به دست نیامد.";
                        result.IsSuccess = false;
                        result.Message = errorMessage;
                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    result.IsSuccess = true;

                    result.depositCustomerBeans = getDepositCustomerResponse.depositCustomers.ToList();
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                errorMessage =
                    "خطا در دریافت اطلاعات مشتری ها. لطفاً صحت شماره حساب وارد شده را چک کرده و مجدداً تلاش نمایید.";
                ex.AddLogData("GetCustomerInfo", accountNumber).AddLogData("Path", path).LogNoContext();

                result.IsSuccess = false;
                result.Message = errorMessage;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult TryGetCustomerInfo(TryGetCustomerInfoInput input)
        {
            var primaryAccountCustomerNumber = input.primaryAccountCustomerNumber;
            var selectedAccountCustomerNumber = input.selectedAccountCustomerNumber;
            TosanGetCustomerInfoResponse customerInfo = null;
            string errorMessage = null;
            TryGetCustomerInfo result = new TryGetCustomerInfo();
            var path = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            try
            {
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    var sessionId = service.login(new[] {new AccountNumberVerificationService.contextEntry()},
                        new AccountNumberVerificationService.userInfoRequestBean
                            {username = "tes-portal", password = "14186621"});

                    primaryAccountCustomerNumber = primaryAccountCustomerNumber.TrimStart('0');
                    selectedAccountCustomerNumber = selectedAccountCustomerNumber.TrimStart('0');

                    var selectedAccountResponse = service.getCustomerDetailInfo(
                        new[]
                        {
                            new AccountNumberVerificationService.contextEntry {key = "SESSIONID", value = sessionId}
                        },
                        new AccountNumberVerificationService.customerDetailRequestBean
                            {cif = selectedAccountCustomerNumber});

                    var homeAddress = selectedAccountResponse.addresses?.LastOrDefault(x =>
                        x.addressType == AccountNumberVerificationService.addressType.HOME);
                    customerInfo = new TosanGetCustomerInfoResponse
                    {
                        Birthdate = selectedAccountResponse.birthDate,
                        FatherLatinName = selectedAccountResponse.fatherLatinName,
                        FatherName = selectedAccountResponse.fatherName,
                        FirstName = selectedAccountResponse.firstName,
                        NationalCode = selectedAccountResponse.code,
                        LastName = selectedAccountResponse.lastName,
                        LatinFirstName = selectedAccountResponse.latinFirstName,
                        LatinLastName = selectedAccountResponse.latinLastName,
                        Mobile = selectedAccountResponse.mobile?.NormalizeMobile(),
                        IdentityNumber = selectedAccountResponse.ssn,
                        certificateSeries = selectedAccountResponse.certificateSeries,
                        certificateSerial = selectedAccountResponse.certificateSerial,
                        cif = selectedAccountResponse.cif,
                        birthLocationCode  = selectedAccountResponse.birthLocationCode ,
                        BirthDateFieldSpecified = selectedAccountResponse.birthDateSpecified,
                        GenderFieldSpecified = selectedAccountResponse.genderSpecified,
                        PersonalityTypeFieldSpecified = selectedAccountResponse.personalityTypeSpecified,
                        IsMale = selectedAccountResponse.gender == AccountNumberVerificationService.genderWS.MALE,
                        IsLegalPersonality = selectedAccountResponse.personalityType ==
                                             AccountNumberVerificationService.personalityType.LEGAL,
                        PrimaryAccountBirthDate =
                            selectedAccountResponse.personalityType ==
                            AccountNumberVerificationService.personalityType.ACTUAL
                                ? selectedAccountResponse.birthDate
                                : (DateTime?) null,
                        HomeAddress = new TosanGetCustomerInfoResponse.Address
                        {
                            PhoneNumber = homeAddress?.phoneNumber,
                            PostalAddress = homeAddress?.postalAddress,
                            PostalCode = homeAddress?.postalCode
                        }
                    };

                    if (primaryAccountCustomerNumber != selectedAccountCustomerNumber)
                    {
                        var primaryAccountResponse = service.getCustomerDetailInfo(
                            new[]
                            {
                                new AccountNumberVerificationService.contextEntry {key = "SESSIONID", value = sessionId}
                            },
                            new AccountNumberVerificationService.customerDetailRequestBean
                                {cif = primaryAccountCustomerNumber.TrimStart('0')});

                        customerInfo.PrimaryAccountBirthDate =
                            primaryAccountResponse.personalityType ==
                            AccountNumberVerificationService.personalityType.ACTUAL
                                ? primaryAccountResponse.birthDate
                                : (DateTime?) null;
                        if (primaryAccountResponse.personalityType ==
                            AccountNumberVerificationService.personalityType.LEGAL)
                        {
                            customerInfo.IsLegalPersonality = true;
                            customerInfo.LegalNationalCode = primaryAccountResponse.code;
                            customerInfo.CompanyRegistrationNumber = primaryAccountResponse.ssn;
                            customerInfo.CompanyRegistrationDate = primaryAccountResponse.birthDate;
                        }
                    }

                    if (!customerInfo.IsLegalPersonality && customerInfo.PrimaryAccountBirthDate.CalculateAge() < 18)
                    {
                        errorMessage = "صاحب سپرده به سن قانونی نرسیده است.";
                        result.IsSuccess = false;
                        result.ErrorgMessage = errorMessage;

                        return Json(result);
                    }

                    result.IsSuccess = true;
                    result.Response = customerInfo;

                    return Json(result);
                }
            }
            catch (Exception ex)
            {
                errorMessage =
                    "خطا در دریافت اطلاعات مشتری. لطفاً صحت اطلاعات وارد شده را چک کرده و مجدداً تلاش نمایید.";
                ex.AddLogData("GetCustomerInfo", primaryAccountCustomerNumber).AddLogData("Path", path).LogNoContext();

                result.IsSuccess = false;
                result.ErrorgMessage = errorMessage;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetIncompleteCustomerInfoMessage(TosanGetCustomerInfoResponse customerInfo)
        {
            var persianIncompleteFieldNames = new List<string>();

            if (string.IsNullOrEmpty(customerInfo.FirstName))
                persianIncompleteFieldNames.Add("نام");

            if (string.IsNullOrEmpty(customerInfo.LastName))
                persianIncompleteFieldNames.Add("نام خانوادگی");

            if (string.IsNullOrEmpty(customerInfo.FatherName))
                persianIncompleteFieldNames.Add("نام پدر");

            if (string.IsNullOrEmpty(customerInfo.LatinFirstName))
                persianIncompleteFieldNames.Add("نام انگلیسی");

            if (string.IsNullOrEmpty(customerInfo.LatinLastName))
                persianIncompleteFieldNames.Add("نام خانوادگی انگلیسی");

            if (string.IsNullOrEmpty(customerInfo.FatherLatinName))
                persianIncompleteFieldNames.Add("نام پدر انگلیسی");

            if (string.IsNullOrEmpty(customerInfo.IdentityNumber))
                persianIncompleteFieldNames.Add("شماره شناسنامه");

            if (string.IsNullOrEmpty(customerInfo.NationalCode))
                persianIncompleteFieldNames.Add("کد ملی");

            if (!customerInfo.BirthDateFieldSpecified)
                persianIncompleteFieldNames.Add("تاریخ تولد");

            if (!customerInfo.GenderFieldSpecified)
                persianIncompleteFieldNames.Add("جنسیت");

            if (!customerInfo.PersonalityTypeFieldSpecified)
                persianIncompleteFieldNames.Add("شخصیت");

            if (string.IsNullOrEmpty(customerInfo.Mobile))
                persianIncompleteFieldNames.Add("تلفن همراه");

            if (customerInfo.IsLegalPersonality && string.IsNullOrEmpty(customerInfo.LegalNationalCode))
                persianIncompleteFieldNames.Add("شناسه ملی شرکت");

            if (customerInfo.IsLegalPersonality && !customerInfo.CompanyRegistrationDate.HasValue)
                persianIncompleteFieldNames.Add("تاریخ ثبت شرکت");

            if (customerInfo.IsLegalPersonality && string.IsNullOrEmpty(customerInfo.CompanyRegistrationNumber))
                persianIncompleteFieldNames.Add("شماره ثبت شرکت");

            return Json(persianIncompleteFieldNames.Any()
                ? $"{(persianIncompleteFieldNames.Count == 1 ? "فیلد" : "فیلدهای")} {string.Join("، ", persianIncompleteFieldNames)} از اطلاعات این مشتری در سیستم فرانگین ناقص می باشد. لطفا پس از بروزرسانی، مجدداً اقدام نمایید"
                : null);
        }
    }

    public class GetCustomersOfAccountNumberResult
    {
        public bool IsSuccess { get; set; }
        public List<depositCustomerBean> depositCustomerBeans { get; set; }
        public string Message { get; set; }
    }

    public class TryGetCustomerInfo
    {
        public bool IsSuccess { get; set; }
        public string ErrorgMessage { get; set; }
        public TosanGetCustomerInfoResponse Response { get; set; }
    }
}