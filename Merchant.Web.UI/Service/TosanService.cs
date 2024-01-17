using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; 
using System.Security.Cryptography.X509Certificates; 
using System.Web;
using TES.Common.Extensions;
using TES.Merchant.Web.UI.Service.Models;

namespace TES.Merchant.Web.UI.Service
{
    public class TosanService
    {
        public static bool IsUp()
        {
            try
            {
                var path = HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

                string sessionId;
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    sessionId = service.login(new[] { new AccountNumberVerificationService.contextEntry() }, new AccountNumberVerificationService.userInfoRequestBean { username = "tes-portal", password = "14186621" });
                }

                return !string.IsNullOrEmpty(sessionId);
            }
            catch
            {
                return false;
            }
        }

        public static bool TryGetAccountOwnerFullName(string accountNumber, out string ownerFullName, out string errorMessage)
        {
            // Set LoadUserProfile = true to work on server!

            ownerFullName = null;
            errorMessage = null;
            var splittedAccountNumber = accountNumber.Split('-');
            var newAccountNumber = $"{splittedAccountNumber[0].TrimStart('0')}-{splittedAccountNumber[1].TrimStart('0')}-{splittedAccountNumber[2].TrimStart('0')}-{splittedAccountNumber[3].TrimStart('0')}";

            var path = HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            try
            {
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    var sessionId = service.login(new[] { new AccountNumberVerificationService.contextEntry() }, new AccountNumberVerificationService.userInfoRequestBean { username = "tes-portal", password = "14186621" });
                    ownerFullName = service.getDepositOwnerName(new[] { new AccountNumberVerificationService.contextEntry { key = "SESSIONID", value = sessionId } }, new AccountNumberVerificationService.depositOwnerRequestBean { depositNumber = newAccountNumber });
                    if (string.IsNullOrEmpty(ownerFullName))
                    {
                        errorMessage = "شماره حساب وارد شده صحیح نمی باشد";
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "خطا در استعلام نام صاحب حساب. لطفاً از صحت شماره حساب وارد شده مطمئن شده و مجدداً تلاش نمایید.";

                ex.AddLogData("CheckAccountNumber", accountNumber).AddLogData("Path", path).LogNoContext();

                return false;
            }
        }

        public static bool TryGetCustomersOfAccountNumber(string accountNumber, out IEnumerable<TosanCustomerOfAccountNumber> customers, out string errorMessage)
        {
            customers = null;
            errorMessage = null;
            var path = HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            try
            {
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    var sessionId = service.login(new[] { new AccountNumberVerificationService.contextEntry() }, new AccountNumberVerificationService.userInfoRequestBean { username = "tes-portal", password = "14186621" });

                    var splittedAccountNumber = accountNumber.Split('-');
                    var optimizedAccountNumber = $"{splittedAccountNumber[0].TrimStart('0')}-{splittedAccountNumber[1].TrimStart('0')}-{splittedAccountNumber[2].TrimStart('0')}-{splittedAccountNumber[3].TrimStart('0')}";

                
                    var getDepositCustomerResponse = service.getDepositCustomer(
                        new[] { new AccountNumberVerificationService.contextEntry { key = "SESSIONID", value = sessionId } },
                        new AccountNumberVerificationService.depositCustomerRequestBean { depositNumber = optimizedAccountNumber, lengthSpecified = true, offsetSpecified = true, offset = 0, length = 100 });
                    customers = getDepositCustomerResponse.depositCustomers.Select(x => new TosanCustomerOfAccountNumber { CustomerNumber = x.cif, Name = x.name });

                    if (!getDepositCustomerResponse.depositCustomers.Any())
                    {
                        errorMessage = "با شماره حساب وارد شده هیچ اطلاعاتی از وب سرویس فرا‌ نگین به دست نیامد.";
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = "خطا در دریافت اطلاعات مشتری ها. لطفاً صحت شماره حساب وارد شده را چک کرده و مجدداً تلاش نمایید."
                    + "2 " + ex.Message + " " + ex.StackTrace;
                ex.AddLogData("GetCustomerInfo", accountNumber).AddLogData("Path", path).LogNoContext();

                return false;
            }
        }

        public static bool TryGetCustomerInfo(string primaryAccountCustomerNumber, string selectedAccountCustomerNumber, out TosanGetCustomerInfoResponse customerInfo, out string errorMessage)
        {
            customerInfo = null;
            errorMessage = null;
            var path = HttpContext.Current.Server.MapPath("~/App_Data/Yaghout-Client.p12");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            ServicePointManager.ServerCertificateValidationCallback += (se, cert, chain, sslerror) => true;

            try
            {
                using (var service = new AccountNumberVerificationService.soap_sarmayehInterbank())
                {
                    service.ClientCertificates.Add(new X509Certificate2(path, "1012125416"));
                    var sessionId = service.login(new[] { new AccountNumberVerificationService.contextEntry() }, new AccountNumberVerificationService.userInfoRequestBean { username = "tes-portal", password = "14186621" });

                    primaryAccountCustomerNumber = primaryAccountCustomerNumber.TrimStart('0');
                    selectedAccountCustomerNumber = selectedAccountCustomerNumber.TrimStart('0');

                    var selectedAccountResponse = service.getCustomerDetailInfo(
                        new[] { new AccountNumberVerificationService.contextEntry { key = "SESSIONID", value = sessionId } },
                        new AccountNumberVerificationService.customerDetailRequestBean { cif = selectedAccountCustomerNumber });

                    var homeAddress = selectedAccountResponse.addresses?.LastOrDefault(x => x.addressType == AccountNumberVerificationService.addressType.HOME);
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
                        certificateSerial = selectedAccountResponse.certificateSerial,
                        certificateSeries = selectedAccountResponse.certificateSeries,

                        BirthDateFieldSpecified = selectedAccountResponse.birthDateSpecified,
                        GenderFieldSpecified = selectedAccountResponse.genderSpecified,
                        PersonalityTypeFieldSpecified = selectedAccountResponse.personalityTypeSpecified,
                        IsMale = selectedAccountResponse.gender == AccountNumberVerificationService.genderWS.MALE,
                        IsLegalPersonality = selectedAccountResponse.personalityType == AccountNumberVerificationService.personalityType.LEGAL,
                        PrimaryAccountBirthDate = selectedAccountResponse.personalityType == AccountNumberVerificationService.personalityType.ACTUAL ?
                            selectedAccountResponse.birthDate : (DateTime?)null,
                        HomeAddress = new TosanGetCustomerInfoResponse.Address
                        {
                            PhoneNumber = homeAddress?.phoneNumber,
                            PostalAddress = homeAddress?.postalAddress,
                            PostalCode = homeAddress?.postalCode
                        }
                    };
  //                  if (primaryAccountCustomerNumber == "2135576")
  //                  {
   //                     customerInfo.PrimaryAccountBirthDate = null;
   //                     customerInfo.IsLegalPersonality = true;
  //                      customerInfo.LegalNationalCode = "10100154602";
   //                     customerInfo.CompanyRegistrationNumber ="1627";
   //                   
  //                  }
 //                   else
                    if (primaryAccountCustomerNumber != selectedAccountCustomerNumber)
                    {
                        var primaryAccountResponse = service.getCustomerDetailInfo(
                            new[] { new AccountNumberVerificationService.contextEntry { key = "SESSIONID", value = sessionId } },
                            new AccountNumberVerificationService.customerDetailRequestBean { cif =
                                primaryAccountCustomerNumber == "16270000" ? selectedAccountCustomerNumber.TrimStart('0') : 
                                primaryAccountCustomerNumber.TrimStart('0') });

                        customerInfo.PrimaryAccountBirthDate = primaryAccountResponse.personalityType == AccountNumberVerificationService.personalityType.ACTUAL ?
                            primaryAccountResponse.birthDate : (DateTime?)null;
                        if (primaryAccountResponse.personalityType == AccountNumberVerificationService.personalityType.LEGAL)
                        {
                            customerInfo.IsLegalPersonality = true;
                            customerInfo.LegalNationalCode = primaryAccountResponse.code;
                            customerInfo.CompanyRegistrationNumber = primaryAccountResponse.ssn;
                            customerInfo.CompanyRegistrationDate = primaryAccountResponse.birthDate;
                        }
                    }

                    if ( primaryAccountCustomerNumber != "16270000" && !customerInfo.IsLegalPersonality && customerInfo.PrimaryAccountBirthDate.CalculateAge() < 18)
                    {
                        errorMessage = "صاحب سپرده به سن قانونی نرسیده است.";
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage =
                    "خطا در دریافت اطلاعات مشتری. لطفاً صحت اطلاعات وارد شده را چک کرده و مجدداً تلاش نمایید."
                    + " " + ex.Message + " " + ex.StackTrace
                    ;
                ex.AddLogData("GetCustomerInfo", primaryAccountCustomerNumber).AddLogData("Path", path).LogNoContext();

                return false;
            }
        }

        public static string GetIncompleteCustomerInfoMessage(TosanGetCustomerInfoResponse customerInfo)
        {
            var persianIncompleteFieldNames = new List<string>();

            if (customerInfo == null)
                return null;
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

            return persianIncompleteFieldNames.Any() ?
                $"{(persianIncompleteFieldNames.Count == 1 ? "فیلد" : "فیلدهای")} {string.Join("، ", persianIncompleteFieldNames)} از اطلاعات این مشتری در سیستم فرانگین ناقص می باشد. لطفا پس از بروزرسانی، مجدداً اقدام نمایید" :
                null;
        }
    }
}