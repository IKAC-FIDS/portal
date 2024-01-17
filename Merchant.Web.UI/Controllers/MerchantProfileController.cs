using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Common.Enumerations;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Drawing;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;

namespace TES.Merchant.Web.UI.Controllers
{
    public class MerchantProfileController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public MerchantProfileController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public ApplicationUserManager UserManager =>
            HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();


        [AllowAnonymous]
        [HttpGet]
        public object GetPsp(string NationalCode)
        {
            var merchatn = _dataContext.MerchantProfiles.FirstOrDefault(b => b.NationalCode == NationalCode);

            if (merchatn != null)
            {
                return Json(merchatn.Terminals.Select(b => b.Psp).Distinct().ToList()
                        .Select(a => new
                        {
                            a.Id,
                            a.Title
                        }),
                    JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }


        public KhafanResultDto GetTransactionMonth(List<TransactionSum> transactionSums, DateTime? terminalRevokeDate,
            DateTime input)
        {
            var result = new KhafanResultDto();


            try
            {
                if (!transactionSums.Any())
                {
                    result.NoTransactionMount = 0;
                    result.LowTransactionMount = 0;
                    return result;
                }

                if (!terminalRevokeDate.HasValue)
                    terminalRevokeDate = DateTime.Now;


                var totalMount = 0;
                long sum = 0;
                while (input <= terminalRevokeDate.Value)
                {
                    input = input.AddMonths(1);
                    var persiandate = input.GetPersianMonth();
                    var persianYear = input.GetPersianYear();


                    totalMount = totalMount + 1;

                    if (!transactionSums.Any(b => b.PersianLocalMonth == persiandate
                                                  && b.PersianLocalYear == persianYear))
                    {
                        result.NoTransactionMount = result.NoTransactionMount + 1;
                    }
                    else
                    {
                        var transaction = transactionSums.FirstOrDefault(b => b.PersianLocalMonth == persiandate
                                                                              && b.PersianLocalYear == persianYear);

                        sum = sum + transaction.SumPrice;

                        if (result.LowestTransaction == 0 && transaction.SumPrice != 0)

                        {
                            result.LowestTransaction = transaction.SumPrice;
                        }
                        else if (transaction.SumPrice != 0)
                        {
                            if (transaction.SumPrice < result.LowestTransaction)
                                result.LowestTransaction = transaction.SumPrice;
                        }


                        if (transaction.SumPrice > result.HighestTransaction)
                            result.HighestTransaction = transaction.SumPrice;


                        if (transaction.SumPrice <= 999)
                        {
                            result.NoTransactionMount = result.NoTransactionMount + 1;
                        }
                        else if (transaction.SumPrice >= 1000 && transaction.SumPrice < 2000000 &&
                                 transaction.TotalCount < 60)
                        {
                            result.LowTransactionMount = result.LowTransactionMount + 1;
                        }
                        else
                        {
                            result.HighTransactionMount = result.HighTransactionMount + 1;
                        }
                    }
                }

                if (totalMount != 0)
                    result.Average = sum / totalMount;
                else
                {
                    result.Average = sum;
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }

         
        }


        public int GetNoTransactionMount(List<TransactionSum> transactionSums, DateTime? terminalRevokeDate,
            DateTime input)
        {
            var result = 0;

            if (!transactionSums.Any())

                return 0;

            if (!terminalRevokeDate.HasValue)
                terminalRevokeDate = DateTime.Now;
            while (input <= terminalRevokeDate.Value)
            {
                input = input.AddMonths(1);
                var persiandate = input.GetPersianMonth();
                var persianYear = input.GetPersianYear();

                if (!transactionSums.Any(b => b.PersianLocalMonth == persiandate
                                              && b.PersianLocalYear == persianYear))
                {
                    result = result + 1;
                }
            }

            return result;
        }

        [AllowAnonymous]
        [HttpGet]
        public object GetKhafanReport()
        {
            var terminals = _dataContext.Terminals
                // .Where(b => b.InstallationDate.HasValue
                //                                               && b.TerminalNo == "21415701"
                //)
                .Where(b => b.InstallationDate.HasValue)
                .ToList();

            var result = new List<KhafanResultDto>();
            var badlist = new List<string>();
            foreach (var terminal in terminals)
            {
                try
                {
                    var transaction = _dataContext.TransactionSums
                        .Where(b => b.TerminalNo == terminal.TerminalNo)
                        .ToList();

                    if (!transaction.Any())
                        continue;
                    var ssss = GetTransactionMonth(transaction, terminal.RevokeDate, terminal.InstallationDate.Value);
                    if (ssss.Average == 0)
                        continue;

                    var tr = new KhafanResultDto();
                    tr.TerminalNo = terminal.TerminalNo;

                    tr.MerchantTitle = terminal.MerchantProfile.FirstName + " " + terminal.MerchantProfile.LastName;
                    tr.Senf = terminal?.Guild?.Title == "کسب و کارهای مرتبط با این گروه صنفی"
                        ? terminal?.Guild?.Parent?.Title
                        : terminal?.Guild?.Title;
                    tr.MerchantNationalCode = terminal.MerchantProfile.NationalCode;
                    tr.NoTransactionMount =
                        ssss.NoTransactionMount;
                    tr.LowTransactionMount =
                        ssss.LowTransactionMount;
                    tr.Status = terminal.Status.Title;
                    tr.HighTransactionMount = ssss.HighTransactionMount;
                    tr.HighestTransaction = ssss.HighestTransaction;
                    tr.LowestTransaction = ssss.LowestTransaction;

                    tr.Average = ssss.Average;
                    tr.TerminalTitle = terminal.Title;
                    result.Add(tr);
                }
                catch
                {
                    badlist.Add(terminal.TerminalNo);
                }
            }


            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("پایانه ها");

                worksheet.Row(1).Height = 50;
                var headerRowStyle = worksheet.Row(1).Style;
                headerRowStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRowStyle.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#0B303D"));
                headerRowStyle.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                headerRowStyle.Font.Bold = true;
                headerRowStyle.Font.Size = 12;
                headerRowStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                headerRowStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var cellsStyle = worksheet.Cells.Style;
                cellsStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                cellsStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Column(1).Width = 26;
                worksheet.Column(2).Width = 16;
                worksheet.Column(3).Width = 44;
                worksheet.Column(4).Width = 18;
                worksheet.Column(5).Width = 18;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 18;
                worksheet.Column(8).Width = 18;
                worksheet.Column(9).Width = 18;
                worksheet.Column(10).Width = 18;
                worksheet.Column(11).Width = 18;


                worksheet.Cells[1, 1].Value = "MerchantTitle";
                worksheet.Cells[1, 2].Value = "Senf";

                worksheet.Cells[1, 3].Value = "MerchantNationalCode";
                worksheet.Cells[1, 4].Value = "TerminalNo";
                worksheet.Cells[1, 5].Value = "NoTransactionMonth";
                worksheet.Cells[1, 6].Value = "LowTransactionMonth";
                worksheet.Cells[1, 7].Value = "HighTransactionMonth";
                worksheet.Cells[1, 8].Value = "LowestTransactionMonth";
                worksheet.Cells[1, 9].Value = "HighestTransactionMonth";
                worksheet.Cells[1, 10].Value = "Average";
                worksheet.Cells[1, 11].Value = "Status";

                var rowNumber = 2;

                foreach (var item in result)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.MerchantTitle;
                    worksheet.Cells[rowNumber, 2].Value = item.Senf;

                    worksheet.Cells[rowNumber, 3].Value = item.MerchantNationalCode;
                    worksheet.Cells[rowNumber, 4].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 5].Value = item.NoTransactionMount;
                    worksheet.Cells[rowNumber, 6].Value = item.LowTransactionMount;
                    worksheet.Cells[rowNumber, 7].Value = item.HighTransactionMount;
                    worksheet.Cells[rowNumber, 8].Value = item.LowestTransaction;
                    worksheet.Cells[rowNumber, 9].Value = item.HighestTransaction;
                    worksheet.Cells[rowNumber, 10].Value = item.Average;
                    worksheet.Cells[rowNumber, 11].Value = item.Status;

                    rowNumber++;
                }


                var stream = new MemoryStream();
                package.SaveAs(stream);
                var content = stream.ToArray();

                
                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));
                
                
                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "khafan.xlsx");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public object getBranchList()
        {
            var merchatn = _dataContext.OrganizationUnits.ToList();

            return Json(merchatn.ToList()
                    .Select(a => new
                    {
                        a.Id,
                        UserName = a.Users.FirstOrDefault()?.UserName,
                        Title = a.Title
                    }),
                JsonRequestBehavior.AllowGet);


            return Json(null, JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        [HttpPost]
        public async Task<object> BranchLogin(BranchLoginDto input)
        {
            var user = UserManager.FindByName(input.UserName);


            if (await UserManager.CheckPasswordAsync(user, input.Password))
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser
            ,
            DefaultRoles.BranchManagment
            , DefaultRoles.ITUser)]
        public async Task<ActionResult> Create()
        {
            //return RedirectToAction("Disable", "Error");

            var branchLimitations = await _dataContext.CheckBranchLimitations(CurrentUserBranchId);

            if (branchLimitations.Item1 && !User.IsBranchManagementUser())
            {
                AddWarningMessage(
                    @"عطف به اطلاعیه شماره 27249/99 مورخ 31/03/1399 اداره سازمان و برنامه ریزی و پیرو بند 7‏-4 «دستورالعمل پایانه فروش (POS)» و تبصره ذیل آن (ابلاغی طی بخشنامه شماره 71191‏‏/96 مورخ 20‏‏/07‏‏/96)، در خصوص جمع آوری پایانه های فروش 'فاقد تراکنش' و 'کم تراکنش' شعبه شما سقف مجاز  'حداکثر 10 درصد از پذیرندگان پایانه های فروش هر شعبه تا سقف 10 پذیرنده' را رعایت ننموده و دسترسی ثبت درخواست جدید پایانه فروش برای آن شعبه غیرفعال شده است.
            لذا جهت فعال شدن دسترسی ثبت درخواست جدید پایانه فروش نسبت به ثبت' درخواست جمع‌آوری برای پایانه‌های فاقد تراکنش یا کم تراکنش در پورتال مدیریت پذیرندگان' و یا اطلاع رسانی به پذیرندگان ذیربط جهت فعال شدن مجدد پایانه ها' اقدام نمایید.
            درصورتیکه تعداد پایانه های فروش فاقد تراکنش و کم تراکنش آن شعبه در بررسی های دوره ای برابر یا کمتر از 10 پایانه شود ، به صورت سیستمی دسترسی ثبت درخواست برای آن شعبه فعال خواهد شد.");

                return RedirectToAction("Disable", "Error");
            }

            var marketerList = await _dataContext.Marketers
                .Select(x => new {x.Id, x.Title})
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.MarketerList = marketerList.ToSelectList(x => x.Id, x => x.Title);

            var branchList = await _dataContext.OrganizationUnits
                .Where(x => x.ParentId.HasValue)
                .Select(x => new {x.Id, x.Title})
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.BranchList = branchList.ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            var pspList = await _dataContext.Psps
                .Select(x => new {x.Id, x.Title})
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.PspList = pspList.ToSelectList(x => x.Id, x => x.Title);


            ViewBag.CustomerCategory = _dataContext.CustomerCategory
                .Select(x => new {Id = (byte) (long) x.Id, Title = x.Name})
                .ToList()
                .ToSelectList(x => x.Id, x => x.Title);


            var activityTypeList = await _dataContext.ActivityTypes.Where(a=>a.Id != 3)
                .Select(x => new {x.Id, x.Title})
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.ActivityTypeList = new SelectList(activityTypeList, "Id", "Title", 1);   

            ViewBag.StateList = await _dataContext.States
                .Select(x => new StateViewModel
                {
                    Id = x.Id,
                    Title = "استان " + x.Title,
                    Code = x.Code,
                    Cities = x.Cities.Where(y => y.Id != 1).Select(y => new CityViewModel
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                })
                .ToListAsync();
            
           
            ViewBag.GuildList = await _dataContext.Guilds
                .Where(x => !x.ParentId.HasValue && x.IsActive)
                .Select(x => new GuildViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ChildGuilds = x.Children.Select(y => new GuildViewModel.ChildGuildViewModel
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                })
                .ToListAsync();

            var nationalityList = await _dataContext.Nationalities
                .Select(x => new NationalityViewModel
                {
                    Id = x.Id,
                    Title = x.Title
                })
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.NationalityList = nationalityList.ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = await _dataContext.DeviceTypes
                .Where(x => x.IsActive && (!branchLimitations.Item2 || !x.IsWireless))
                .Select(x => new DeviceTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    BlockPrice = x.BlockPrice
                })
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.AddressComponentList = await _dataContext.AddressComponents
                .Select(x => new AddressComponentViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    PrefixTypeCode = x.PrefixTypeCode,
                    PriorityNumber = x.PriorityNumber
                })
                .ToListAsync();

            ViewBag.DocumentTypeList = await _dataContext.DocumentTypes
                .Select(x => new DocumentTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsRequired = x.IsRequired,
                    ForEntityTypeId = x.ForEntityTypeId,
                    IsForLegalPersonality = x.IsForLegalPersonality
                })
                .OrderByDescending(x => x.IsRequired)
                .ThenBy(x => x.Title)
                .ToListAsync();

            ViewBag.DisableWirelessTerminalRequest =
                !User.IsBranchManagementUser() ? (dynamic) branchLimitations.Item2 : false;
           
            return View();
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser
            ,
            DefaultRoles.BranchManagment
            , DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(MerchantDataEntryViewModel viewModel,
            CancellationToken cancellationToken)
        {
            //return JsonWarningMessage("با توجه به عدم سرویس دهی شرکت های psp طرف قرارداد  از پانزدهم اسفند ماه 1398 لغایت پانزدهم فروردین ماه 1399، بدلیل انبار گردانی و سرویس دهی ویژه ایام پایانی سال و ایام نوروز امکان ارائه سرویس جهت ثبت و نصب دستگاه پایانه فروش جدید وجود ندارد.");

            var branchLimitations = await _dataContext.CheckBranchLimitations(CurrentUserBranchId);

            if (branchLimitations.Item1 && !User.IsBranchManagementUser())
            {
                return JsonWarningMessage("امکان ثبت درخواست پایانه جدید برای شعبه شما بسته شده است");
            }

            var validationResult = ValidateMerchantDataEntryViewModel(viewModel);
            if (validationResult != null)
            {
                return validationResult;
            }

            AccountNumberExtensions.GenerateAccountNumber(viewModel.AccountBranchCode, viewModel.AccountType,
                viewModel.AccountCustomerNumber, viewModel.AccountRow, out var accountNumberWithDash,
                out var accountNumberWithoutDash);

            if (!AccountNumberExtensions.TryGenerateShebaNumber(accountNumberWithoutDash, out var shebaNo))
            {
                return JsonWarningMessage(
                    "شماره حساب وارد شده صحیح نمی باشد. لطفاً شماره حساب وارد شده را چک نموده و مجدداً تلاش نمایید.");
            }

            var now = DateTime.Now;
            var terminal = new Terminal
            {
                SubmitTime = now,
                ShebaNo = shebaNo,
                Tel = viewModel.Tel,
                LastUpdateTime = now,
                UserId = CurrentUserId,
                Title = viewModel.Title,
                CityId = viewModel.CityId,
                GuildId = viewModel.GuildId,
                Address = viewModel.Address,
                TelCode = viewModel.TelCode,
                PostCode = viewModel.PostCode,
                EnglishAddress = "sarmayeh bank",
                AccountNo = accountNumberWithDash,
                EnglishTitle = viewModel.EnglishTitle,
                DeviceTypeId = viewModel.DeviceTypeId,
                TaxPayerCode = viewModel.TaxPayerCode,
                StatusId = (byte) Enums.TerminalStatus.New,
                ActivityTypeId = viewModel.ActivityTypeId,
                PreferredPspId = viewModel.PreferredPspId,
                CustomerCategoryId = viewModel.CustomerCategoryId,
                ShaparakAddressFormat = viewModel.ShaparakAddressFormat,
                RegionalMunicipalityId = viewModel.RegionalMunicipalityId,
                MarketerId = (User.IsBranchUser() || User.IsBranchManagementUser())
                    ? (int) Enums.Marketer.BankOrBranch
                    : viewModel.MarketerId
            };

            var deviceTypeBlockPrice = await _dataContext.DeviceTypes.Where(x => x.Id == viewModel.DeviceTypeId)
                .Select(x => x.BlockPrice)
                .FirstAsync(cancellationToken);

            if (deviceTypeBlockPrice > 0)
            {
                // شماره مشتری و کد شعبه حساب مسدودی باید عین شماره حساب اصلی باشد
                AccountNumberExtensions.GenerateAccountNumber(viewModel.AccountBranchCode, viewModel.BlockAccountType,
                    viewModel.AccountCustomerNumber, viewModel.BlockAccountRow, out var blockAccountNumberWithDash,
                    out _);

                terminal.BlockPrice = deviceTypeBlockPrice;
                terminal.BlockAccountNumber = blockAccountNumberWithDash;
                terminal.BlockDocumentDate = viewModel.BlockDocumentDate;
                terminal.BlockDocumentNumber = viewModel.BlockDocumentNumber;
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.WaitingForReview;
            }
            else
            {
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.NotRegistered;
            }

            if (User.IsInRole(DefaultRoles.BranchUser.ToString()))
            {
                if (!CurrentUserBranchId.HasValue)
                {
                    return JsonWarningMessage("شعبه کاربر یافت نشد.");
                }

                terminal.BranchId = CurrentUserBranchId.Value;
            }
            else
            {
                terminal.BranchId = viewModel.BranchId;
            }

            foreach (var item in viewModel.PostedFiles.Where(x =>
                x.ForEntityTypeId == (int) EntityType.Terminal && x.PostedFile.IsValidFile())) // پی دی اف بود قبلاً
            {
                terminal.TerminalDocuments.Add(new TerminalDocument
                {
                    FileName = item.PostedFile.FileName,
                    DocumentTypeId = item.DocumentTypeId,
                    FileData = item.PostedFile.ToByteArray(),
                    ContentType = item.PostedFile.ContentType
                });
            }

            var merchantProfile = viewModel.IsLegalPersonality
                ? await _dataContext.MerchantProfiles.FirstOrDefaultAsync(
                    x => x.IsLegalPersonality == viewModel.IsLegalPersonality &&
                         x.LegalNationalCode.Equals(viewModel.LegalNationalCode), cancellationToken)
                : await _dataContext.MerchantProfiles.FirstOrDefaultAsync(
                    x => x.IsLegalPersonality == viewModel.IsLegalPersonality &&
                         x.NationalCode.Equals(viewModel.NationalCode), cancellationToken);

            if (merchantProfile == null)
            {
                merchantProfile = new MerchantProfile
                {
                    SubmitTime = now,
                    LastUpdateTime = now,
                    UserId = CurrentUserId,
                    Mobile = viewModel.Mobile,
                    IsMale = viewModel.IsMale,
                    HomeTel = viewModel.HomeTel,
                    LastName = viewModel.LastName,
                    FirstName = viewModel.FirstName,
                    Birthdate = viewModel.Birthdate,
                    FatherName = viewModel.FatherName,
                    HomeAddress = viewModel.HomeAddress,
                    HomePostCode = viewModel.HomePostCode,
                    NationalCode = viewModel.NationalCode,
                    NationalityId = viewModel.NationalityId,
                    CustomerNumber = viewModel.CustomerNumber,
                    IdentityNumber = viewModel.IdentityNumber,
                    EnglishLastName = viewModel.EnglishLastName,
                    EnglishFirstName = viewModel.EnglishFirstName,
                    EnglishFatherName = viewModel.EnglishFatherName,
                    LegalNationalCode = viewModel.LegalNationalCode,
                    SignatoryPosition = viewModel.SignatoryPosition,
                    IsLegalPersonality = viewModel.IsLegalPersonality,
                    CompanyRegistrationDate = viewModel.CompanyRegistrationDate,
                    CompanyRegistrationNumber = viewModel.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = viewModel.BirthCertificateIssueDate,
                    BirthCrtfctSerial = viewModel.BirthCrtfctSerial,
                    BirthCrtfctSeriesNumber = viewModel.BirthCrtfctSeriesNumber,
                    PersianCharRefId = viewModel.PersianCharRefId
                    
                };

                foreach (var item in viewModel.PostedFiles.Where(x =>
                    x.ForEntityTypeId == (int) EntityType.MerchantProfile && x.PostedFile.IsValidFile())
                ) // عکس بود قبلاً
                {
                    merchantProfile.MerchantProfileDocuments.Add(new MerchantProfileDocument
                    {
                        DocumentTypeId = item.DocumentTypeId,
                        FileData = item.PostedFile.ToByteArray(),
                        FileName = item.PostedFile.FileName,
                        ContentType = item.PostedFile.ContentType
                    });
                }

                merchantProfile.Terminals.Add(terminal);
                _dataContext.MerchantProfiles.Add(merchantProfile);
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                merchantProfile.LastUpdateTime = now;
                merchantProfile.UserId = CurrentUserId;
                merchantProfile.IsMale = viewModel.IsMale;
                merchantProfile.Mobile = viewModel.Mobile;
                merchantProfile.HomeTel = viewModel.HomeTel;
                merchantProfile.LastName = viewModel.LastName;
                merchantProfile.FirstName = viewModel.FirstName;
                merchantProfile.Birthdate = viewModel.Birthdate;
                merchantProfile.FatherName = viewModel.FatherName;
                merchantProfile.HomeAddress = viewModel.HomeAddress;
                merchantProfile.HomePostCode = viewModel.HomePostCode;
                merchantProfile.NationalityId = viewModel.NationalityId;
                merchantProfile.IdentityNumber = viewModel.IdentityNumber;
                merchantProfile.CustomerNumber = viewModel.CustomerNumber;
                merchantProfile.EnglishLastName = viewModel.EnglishLastName;
                merchantProfile.EnglishFirstName = viewModel.EnglishFirstName;
                merchantProfile.LegalNationalCode = viewModel.LegalNationalCode;
                merchantProfile.SignatoryPosition = viewModel.SignatoryPosition;
                merchantProfile.EnglishFatherName = viewModel.EnglishFatherName;
                merchantProfile.CompanyRegistrationDate = viewModel.CompanyRegistrationDate;
                merchantProfile.BirthCertificateIssueDate = viewModel.BirthCertificateIssueDate;
                merchantProfile.CompanyRegistrationNumber = viewModel.CompanyRegistrationNumber;
             
                merchantProfile.BirthCrtfctSerial = viewModel.BirthCrtfctSerial;
                merchantProfile.BirthCrtfctSeriesNumber =viewModel.BirthCrtfctSeriesNumber;
                merchantProfile.PersianCharRefId = viewModel.PersianCharRefId;
                
                
                merchantProfile.Terminals.Add(terminal);
                foreach (var item in viewModel.PostedFiles.Where(x =>
                             x.ForEntityTypeId == (int) EntityType.MerchantProfile && x.PostedFile.IsValidFile())
                        ) // عکس بود قبلاً
                {
                    merchantProfile.MerchantProfileDocuments.Add(new MerchantProfileDocument
                    {
                        DocumentTypeId = item.DocumentTypeId,
                        FileData = item.PostedFile.ToByteArray(),
                        FileName = item.PostedFile.FileName,
                        ContentType = item.PostedFile.ContentType
                    });
                }
                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            return JsonSuccessResult(terminal.Id);
        }

        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> GetCustomersOfAccountNumber(string accountNumber,
            CancellationToken cancellationToken)
        {
            var customerNumber = accountNumber.Split('-')[2].PadLeft(8, '0');
            var customerNumberBlackListInfo = await _dataContext.CustomerNumberBlackLists
                .Where(x => x.CustomerNumber == customerNumber)
                .Select(x => new {x.Description, x.SubmitTime})
                .FirstOrDefaultAsync(cancellationToken);

            if (customerNumberBlackListInfo != null)
            {
                return JsonErrorMessage(string.IsNullOrEmpty(customerNumberBlackListInfo.Description)
                    ? "شماره مشتری وارد شده در لیست سیاه قرار دارد و امکان ثبت درخواست نصب پایانه وجود ندارد"
                    : $"شماره مشتری وارد شده در لیست سیاه مشتریان بوده و امکان ثبت درخواست نصب پایانه فروش جدید برای این مشتری وجود ندارد. اضافه شدن مشتری مذکور به لیست سیاه مشتریان ناشی از ثبت درخواست جمع آوری پایانه قبلی این مشتری توسط شعبه به دلیل '{customerNumberBlackListInfo.Description}' در تاریخ {customerNumberBlackListInfo.SubmitTime.ToPersianDate()} بوده است.");
            }

            if (!TosanService.TryGetCustomersOfAccountNumber(accountNumber, out var customers, out var errorMessage))
            {
                return JsonErrorMessage(errorMessage);
            }

            return JsonSuccessResult(customers);
        }

        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> GetExistedMerchantProfile(GetExistedMerchantProfileViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!TosanService.TryGetCustomerInfo(viewModel.PrimaryAccountCustomerNumber,
                viewModel.SelectedAccountCustomerNumber, out var response, out var errorMessage))
            {
                return JsonErrorMessage(errorMessage);
            }

            var incompleteCustomerInfoMessage = TosanService.GetIncompleteCustomerInfoMessage(response);
            if (!string.IsNullOrEmpty(incompleteCustomerInfoMessage))
            {
                return JsonErrorMessage(incompleteCustomerInfoMessage, new {incompleteCustomerInfo = true});
            }

            // اطلاعاتی که وب سرویس بر میگردونه و ما کاری نداریم تو دیتابیس چی ثبت شده همیشه آخرین اطلاعات رو استعلام میگیریم و نشون میدیم
            var inquiredData = new
            {
                response.Mobile,
                response.IsMale,
                response.LastName,
                response.FirstName,
                response.FatherName,
                response.NationalCode,
                response.IdentityNumber,
                response.LegalNationalCode,
                response.IsLegalPersonality,
                response.CompanyRegistrationNumber,
                EnglishLastName = response.LatinLastName,
                EnglishFirstName = response.LatinFirstName,
                HomeTel = response.HomeAddress.PhoneNumber,
                EnglishFatherName = response.FatherLatinName,
                Birthdate = response.Birthdate.ToPersianDate(),
                HomePostCode = response.HomeAddress.PostalCode,
                HomeAddress = response.HomeAddress.PostalAddress,
                CertificateSerial =       response.certificateSerial,
                CertificateNumberSeries = !string.IsNullOrEmpty(response.certificateSeries) ? response.certificateSeries.Split('-')[1]   : null,
        
                CertificateCharSeries = !string.IsNullOrEmpty(response.certificateSeries)  ?response.certificateSeries.Split('-')[0] : null,
                CompanyRegistrationDate = response.CompanyRegistrationDate.ToPersianDate()
            };

            // اطلاعاتی که وب سرویس نمیدهد و فقط ممکنه از قبل ثبت شده باشه توی دیتابیس ما
            var merchantProfileInfo = await _dataContext.MerchantProfiles
                .Where(x => response.IsLegalPersonality
                    ? x.LegalNationalCode.Equals(response.LegalNationalCode)
                    : x.NationalCode.Equals(response.NationalCode))
                .Select(x => new
                {
                    x.NationalityId,
                    x.SignatoryPosition,
                    x.BirthCertificateIssueDate,
                })
                .FirstOrDefaultAsync(cancellationToken);

            var merchantProfileData = merchantProfileInfo == null
                ? null
                : new
                {
                    merchantProfileInfo.NationalityId,
                    merchantProfileInfo.SignatoryPosition,
                    BirthCertificateIssueDate = merchantProfileInfo.BirthCertificateIssueDate.ToPersianDate()
                };

            return JsonSuccessResult(new
            {
                merchantProfileData,
                inquiredData
            });
        }

        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser,
            DefaultRoles.BranchManagment)]
        public ActionResult VerifyAccountNumber(string accountNumber)
        {
            if (User.IsBranchUser() || User.IsBranchManagementUser())
            {
                var splittedAccountNumber = accountNumber.Split('-');
                accountNumber =
                    $"{User.Identity.GetBranchId()}-{splittedAccountNumber[1]}-{splittedAccountNumber[2]}-{splittedAccountNumber[3]}";
            }

            var checkAccountNumberResult =
                TosanService.TryGetAccountOwnerFullName(accountNumber, out var ownerFullName, out var errorMessage);

            if (!checkAccountNumberResult)
            {
                return JsonWarningMessage(errorMessage);
            }

            AccountNumberExtensions.TryGenerateShebaNumber(accountNumber.Replace("-", ""), out var shebaNumber);

            return JsonSuccessResult(new {OwnerFullName = ownerFullName, ShebaNumber = shebaNumber});
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> UpdateMerchantProfileFromWebService(
            UpdateMerchantProfileFromWebServiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var merchantProfile =
                await _dataContext.MerchantProfiles.FirstAsync(x => x.Id == viewModel.MerchantProfileId,
                    cancellationToken);
            var shebaNumber = await _dataContext.Terminals.Where(x => x.Id == viewModel.TerminalId)
                .Select(x => x.ShebaNo).FirstAsync(cancellationToken);
            AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNumber, out var accountNumber);
            var primaryCustomerNumber = accountNumber.Split('-')[2];

            if (!TosanService.TryGetCustomerInfo(primaryCustomerNumber,
                merchantProfile.CustomerNumber ?? primaryCustomerNumber, out var response, out var errorMessage))
            {
                return JsonErrorMessage(errorMessage);
            }

            var incompleteCustomerInfoMessage = TosanService.GetIncompleteCustomerInfoMessage(response);
            if (!string.IsNullOrEmpty(incompleteCustomerInfoMessage))
            {
                return JsonErrorMessage(incompleteCustomerInfoMessage, new {incompleteCustomerInfo = true});
            }

            merchantProfile.Mobile = response.Mobile;
            merchantProfile.IsMale = response.IsMale;
            merchantProfile.LastName = response.LastName;
            merchantProfile.Birthdate = response.Birthdate;
            merchantProfile.FirstName = response.FirstName;
            merchantProfile.FatherName = response.FatherName;
            merchantProfile.NationalCode = response.NationalCode;
            merchantProfile.IdentityNumber = response.IdentityNumber;
            merchantProfile.EnglishLastName = response.LatinLastName;
            merchantProfile.EnglishFirstName = response.LatinFirstName;
            merchantProfile.HomeTel = response.HomeAddress.PhoneNumber;
            merchantProfile.EnglishFatherName = response.FatherLatinName;
            merchantProfile.HomePostCode = response.HomeAddress.PostalCode;
            merchantProfile.LegalNationalCode = response.LegalNationalCode;
            merchantProfile.HomeAddress = response.HomeAddress.PostalAddress;
            merchantProfile.IsLegalPersonality = response.IsLegalPersonality;
            merchantProfile.CompanyRegistrationDate = response.CompanyRegistrationDate;
            merchantProfile.CompanyRegistrationNumber = response.CompanyRegistrationNumber;
            merchantProfile.BirthCrtfctSerial = response.certificateSerial;
            merchantProfile.BirthCrtfctSeriesNumber = !string.IsNullOrEmpty(response.certificateSeries) ? response.certificateSeries.Split('-')[1] : null;
            merchantProfile.PersianCharRefId =  !string.IsNullOrEmpty(response.certificateSeries) 
                ? response.certificateSeries.Split('-')[0] : null;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult(new
            {
                merchantProfile.Mobile,
                merchantProfile.IsMale,
                merchantProfile.HomeTel,
                merchantProfile.LastName,
                merchantProfile.FirstName,
                merchantProfile.FatherName,
                merchantProfile.HomeAddress,
                merchantProfile.HomePostCode,
                merchantProfile.NationalCode,
                merchantProfile.IdentityNumber,
                merchantProfile.EnglishLastName,
                merchantProfile.EnglishFirstName,
                merchantProfile.LegalNationalCode,
                merchantProfile.EnglishFatherName,
                merchantProfile.IsLegalPersonality,
                merchantProfile.CompanyRegistrationNumber,
                Birthdate = merchantProfile.Birthdate.ToPersianDate(),
                CompanyRegistrationDate = merchantProfile.CompanyRegistrationDate.ToPersianDate()
            });
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Import(CancellationToken cancellationToken)
        {
            var viewModel = new MerchantProfileImportViewModel
            {
                PspList = await _dataContext.Psps.Select(x => x.Title).ToListAsync(cancellationToken),
                MarketerList = await _dataContext.Marketers.Select(x => x.Title).ToListAsync(cancellationToken),
                DeviceTypeList = await _dataContext.DeviceTypes.Select(x => x.Title).ToListAsync(cancellationToken),
                NationalityList = await _dataContext.Nationalities.Select(x => x.Title).ToListAsync(cancellationToken),
                ActivityTypeList = await _dataContext.ActivityTypes.Select(x => x.Title).ToListAsync(cancellationToken),
                TerminalStatusList =
                    await _dataContext.TerminalStatus.Select(x => x.Title).ToListAsync(cancellationToken),
                BlockDocumentStatusList = await _dataContext.BlockDocumentStatuses.Select(x => x.Title)
                    .ToListAsync(cancellationToken)
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetData(UpdateJobDetailsViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var totalRowsCount = _dataContext.UpdateJob.ToList();


            var path = Server.MapPath("~/Job/Result.txt");


            var t = System.IO.File.ReadAllLines(path);
            var ro = totalRowsCount
                .Select(x => new UpdateJobViewModel
                {
                    Id = x.Id,
                    RowNumber = x.RowNumber,
                    ProcessedRow = string.IsNullOrEmpty(x.EndDateTime) ? t.Count() : x.RowNumber.Value,
                    ErrorMessage = x.ErrorMessage,
                    Start = x.StartDateTime,
                    End = x.EndDateTime,
                    Error = x.HasError
                })
                .OrderByDescending(x => x.Id)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .ToList();

            var rows = ro.Select(x => new
                {
                    x.Id,
                    x.RowNumber,
                    x.ProcessedRow,
                    x.Start,
                    x.End,
                    x.Error,
                    x.ErrorMessage
                })
                .OrderByDescending(x => x.Id)
                .ToList();
            return JsonSuccessResult(new {rows, totalRowsCount.Count});
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> BatchImport(CancellationToken cancellationToken)
        {
            var viewModel = new MerchantProfileImportViewModel
            {
                PspList = await _dataContext.Psps.Select(x => x.Title).ToListAsync(cancellationToken),
                MarketerList = await _dataContext.Marketers.Select(x => x.Title).ToListAsync(cancellationToken),
                DeviceTypeList = await _dataContext.DeviceTypes.Select(x => x.Title).ToListAsync(cancellationToken),
                NationalityList = await _dataContext.Nationalities.Select(x => x.Title).ToListAsync(cancellationToken),
                ActivityTypeList = await _dataContext.ActivityTypes.Select(x => x.Title).ToListAsync(cancellationToken),
                TerminalStatusList =
                    await _dataContext.TerminalStatus.Select(x => x.Title).ToListAsync(cancellationToken),
                BlockDocumentStatusList = await _dataContext.BlockDocumentStatuses.Select(x => x.Title)
                    .ToListAsync(cancellationToken)
            };
           


            ViewBag.UpdateJobList = (await _dataContext.UpdateJob
                    .Select(x => new
                    {
                        x.Id, Title = x.StartDateTime +
                                      (!string.IsNullOrEmpty(x.EndDateTime) ? (" - " + x.EndDateTime) : "")
                                      + (!string.IsNullOrEmpty(x.ErrorMessage) ? (" - " + x.ErrorMessage) : "") +
                                      (" ( " + x.RowNumber + " / " + x.Details.Count + " ) ")
                    })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            return View(viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Import(HttpPostedFileBase file, CancellationToken cancellationToken)
        {
             if (!file.IsValidFormat(".xlsx"))
            {
                return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
            }

            var merchantProfileDataTable = new DataTable();
            merchantProfileDataTable.Columns.Add(new DataColumn("Id", typeof(long)));
            merchantProfileDataTable.Columns.Add(new DataColumn("FirstName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("LastName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("EnglishFirstName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("EnglishLastName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("NationalCode", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("IsMale", typeof(bool)));
            merchantProfileDataTable.Columns.Add(new DataColumn("HomeTel", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("Mobile", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("HomeAddress", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("HomePostCode", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("IsLegalPersonality", typeof(bool)));
            merchantProfileDataTable.Columns.Add(new DataColumn("NationalityId", typeof(long)));
            merchantProfileDataTable.Columns.Add(new DataColumn("FatherName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("EnglishFatherName", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("IdentityNumber", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("Birthdate", typeof(DateTime)));
            merchantProfileDataTable.Columns.Add(new DataColumn("BirthCertificateIssueDate", typeof(DateTime)));
            merchantProfileDataTable.Columns.Add(new DataColumn("CompanyRegistrationNumber", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("CompanyRegistrationDate", typeof(DateTime)));
            merchantProfileDataTable.Columns.Add(new DataColumn("LegalNationalCode", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("SignatoryPosition", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("CustomerNumber", typeof(string)));
            merchantProfileDataTable.Columns.Add(new DataColumn("UserId", typeof(long)));
            merchantProfileDataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            merchantProfileDataTable.Columns.Add(new DataColumn("LastUpdateTime", typeof(DateTime)));

            #region

            var terminalDataTable = new DataTable();
            terminalDataTable.Columns.Add(new DataColumn("PspId", typeof(byte)));
            terminalDataTable.Columns.Add(new DataColumn("Title", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("EnglishTitle", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("DeviceTypeId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("BranchId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("AccountNo", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("ShebaNo", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("StatusId", typeof(byte)));
            terminalDataTable.Columns.Add(new DataColumn("CityId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("TelCode", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("Tel", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("RegionalMunicipalityId", typeof(byte)));
            terminalDataTable.Columns.Add(new DataColumn("GuildId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("ActivityTypeId", typeof(byte)));
            terminalDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("Address", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("EnglishAddress", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("ShaparakAddressFormat", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("PostCode", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("MarketerId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("ContractNo", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("ContractDate", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("MerchantNo", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("InstallationDate", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("RevokeDate", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("BatchDate", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("LastUpdateTime", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("BlockDocumentDate", typeof(DateTime)));
            terminalDataTable.Columns.Add(new DataColumn("BlockDocumentNumber", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("BlockDocumentStatusId", typeof(byte)));
            terminalDataTable.Columns.Add(new DataColumn("BlockAccountNumber", typeof(string)));
            terminalDataTable.Columns.Add(new DataColumn("BlockPrice", typeof(int)));
            terminalDataTable.Columns.Add(new DataColumn("UserId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("MerchantProfileId", typeof(long)));
            terminalDataTable.Columns.Add(new DataColumn("TaxPayerCode", typeof(string)));

            #endregion

            var terminalNoList = new List<string>();
            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;
                var merchatProfileIdList =
                    await _dataContext.GetIdListFromSequence(totalNumberOfRowsWithoutHeader, cancellationToken);

                var errorMessageList = new List<string>();

                var rowNumber = 2;
                for (rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    var thisItemErrorMessageList = new List<string>();

                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var pspId = GetPspIdFromText(row[rowNumber, 22].Text);
                        var activityTypeId = GetActivityTypeIdFromText(row[rowNumber, 34].Text);
                        var marketerId = GetMarketerIdFromText(row[rowNumber, 40].Text);
                        var deviceTypeId = GetDeviceTypeIdFromText(row[rowNumber, 25].Text);
                        var nationalityId = GetNationalityIdFromText(row[rowNumber, 12].Text);
                        var isLegalPersonality = GetIsLegalPersonalityFromText(row[rowNumber, 11].Text);
                        var isMale = GetIsMaleFromText(row[rowNumber, 6].Text);
                        var statusId = GetStatusIdFromText(row[rowNumber, 28].Text);
                        var blockDocumentStatusId = GetBlockDocumentStatusIdFromText(row[rowNumber, 51].Text);
                        var isValidCityId = long.TryParse(row[rowNumber, 29].Text, out var cityId);
                        var isValidBranchId = long.TryParse(row[rowNumber, 26].Text, out var branchId);
                        var isValidGuildId = long.TryParse(row[rowNumber, 33].Text, out var guildId);

                        if (!blockDocumentStatusId.HasValue)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 51 : وضعیت سند مسدودی وارد شده صحیح نمی باشد");

                        if (!isValidGuildId)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 33 : کد صنف وارد شده بایستی عدد باشد");

                        if (!isValidBranchId)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 26 : کد شعبه وارد شده بایستی عدد باشد");

                        if (!isValidCityId)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 29 : کد شهر وارد شده بایستی عدد باشد");

                        if (!statusId.HasValue)
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 28 : وضعیت وارد شده صحیح نمی باشد");

                        if (!isMale.HasValue)
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 6 : جنسیت وارد شده صحیح نمی باشد");

                        if (!isLegalPersonality.HasValue)
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 11 : شخصیت وارد شده صحیح نمی باشد");

                        if (!nationalityId.HasValue)
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 12 : ملیت وارد شده صحیح نمی باشد");

                        if (!deviceTypeId.HasValue)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 25 : نوع دستگاه وارد شده صحیح نمی باشد");

                        if (!marketerId.HasValue)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 40 : بازاریاب وارد شده صحیح نمی باشد");

                        if (!activityTypeId.HasValue)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 34 : نوع فعالیت وارد شده صحیح نمی باشد");

                        if (!pspId.HasValue)
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 22 : PSP وارد شده صحیح نمی باشد");

                        if (!AccountNumberExtensions.TryGenerateAccountNumberFromSheba(row[rowNumber, 27].Text,
                            out var accountNumber))
                            errorMessageList.Add($"سطر {rowNumber} - ستون 28 : شماره شبا وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 6].Text))
                            errorMessageList.Add($"سطر {rowNumber} - ستون 6 : جنسیت وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 11].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 11 : شخصیت وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 12].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 12 : ملیت وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 22].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 22 : شرکت PSP وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 25].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 25 : نوع دستگاه وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 1].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 1 : نام را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 2].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 2 : نام خانوادگی را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 3].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 3 : نام انگلیسی را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 4].Text))
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 4 : نام خانوادگی انگلیسی را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 5].Text) ||
                            row[rowNumber, 5].Text.Trim().Length != 10 || !row[rowNumber, 5].Text.IsItNumber())
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 5 : کد ملی 10 رقمی را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 8].Text) || !row[rowNumber, 8].Text.IsItNumber())
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 8 : شماره موبایل وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 15].Text) || !row[rowNumber, 8].Text.IsItNumber())
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 15 : شماره شناسنامه وارد شده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 23].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 23 : نام پذیرنده را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 24].Text))
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 24 : نام انگلیسی پذیرنده را وارد نمایید");

                        if (string.IsNullOrEmpty(row[rowNumber, 33].Text) || !row[rowNumber, 33].Text.IsItNumber())
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 33 : کد صنف پذیرنده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 29].Text) || !row[rowNumber, 29].Text.IsItNumber())
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 29 : کد شهر پذیرنده اشتباه است");

                        if (string.IsNullOrEmpty(row[rowNumber, 30].Text) || row[rowNumber, 30].Text.Length != 3)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 30 : پیش شماره تلفن پذیرنده خالی است");

                        if (string.IsNullOrEmpty(row[rowNumber, 31].Text))
                            thisItemErrorMessageList.Add($"سطر {rowNumber} - ستون 31 : شماره تلفن پذیرنده خالی است");

                        if (string.IsNullOrEmpty(row[rowNumber, 39].Text) || row[rowNumber, 39].Text.Length != 10 ||
                            !row[rowNumber, 39].Text.IsItNumber())
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 39 : کد پستی پذیرنده نامعتبر است. (خالی، طول غیر از 10 کاراکتر یا مقدار غیر عددی)");

                        if (!string.IsNullOrEmpty(row[rowNumber, 53].Text) &&
                            !long.TryParse(row[rowNumber, 53].Text, out _))
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 52 : مبلغ مسدودی وارد شده صحیح نمی باشد");

                        if (!string.IsNullOrEmpty(row[rowNumber, 32].Text) &&
                            !long.TryParse(row[rowNumber, 32].Text, out _))
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 32 : منطقه شهرداری وارد شده صحیح نمی باشد");

                        if (string.IsNullOrEmpty(row[rowNumber, 54].Text) && !row[rowNumber, 54].Text.IsItNumber() &&
                            row[rowNumber, 54].Text.Length != 10)
                            thisItemErrorMessageList.Add(
                                $"سطر {rowNumber} - ستون 54 : کد رهگیری ثبت نام مالیاتی وارد شده صحیح نمی باشد");

                        if (thisItemErrorMessageList.Any())
                        {
                            errorMessageList.AddRange(thisItemErrorMessageList);
                            continue;
                        }

                        var merchantProfileDataRow = merchantProfileDataTable.NewRow();
                        merchantProfileDataRow["Id"] = merchatProfileIdList[rowNumber - 2];
                        merchantProfileDataRow["FirstName"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        merchantProfileDataRow["LastName"] = row[rowNumber, 2].Text.ApplyPersianYeKe();
                        merchantProfileDataRow["EnglishFirstName"] = row[rowNumber, 3].Text;
                        merchantProfileDataRow["EnglishLastName"] = row[rowNumber, 4].Text;
                        merchantProfileDataRow["NationalCode"] = row[rowNumber, 5].Text;
                        merchantProfileDataRow["IsMale"] = isMale;
                        merchantProfileDataRow["HomeTel"] = row[rowNumber, 7].Text;
                        merchantProfileDataRow["Mobile"] = row[rowNumber, 8].Text;
                        merchantProfileDataRow["HomeAddress"] = row[rowNumber, 9].Text;
                        merchantProfileDataRow["HomePostCode"] = row[rowNumber, 10].Text;
                        merchantProfileDataRow["IsLegalPersonality"] = isLegalPersonality;
                        merchantProfileDataRow["NationalityId"] = nationalityId;
                        merchantProfileDataRow["FatherName"] = row[rowNumber, 13].Text.ApplyPersianYeKe();
                        merchantProfileDataRow["EnglishFatherName"] = row[rowNumber, 14].Text;
                        merchantProfileDataRow["IdentityNumber"] = row[rowNumber, 15].Text;
                        merchantProfileDataRow["Birthdate"] = row[rowNumber, 16].Text.ToMiladiDate();
                        merchantProfileDataRow["BirthCertificateIssueDate"] = row[rowNumber, 17].Text.ToMiladiDate();
                        merchantProfileDataRow["CompanyRegistrationNumber"] = row[rowNumber, 18].Text;
                        merchantProfileDataRow["CompanyRegistrationDate"] =
                            row[rowNumber, 19].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        merchantProfileDataRow["LegalNationalCode"] = row[rowNumber, 20].Text;
                        merchantProfileDataRow["SignatoryPosition"] = row[rowNumber, 21].Text;
                        merchantProfileDataRow["CustomerNumber"] = accountNumber.Split('-')[2];
                        merchantProfileDataRow["SubmitTime"] = row[rowNumber, 44].Text.ToMiladiDate();
                        merchantProfileDataRow["LastUpdateTime"] = row[rowNumber, 48].Text.ToMiladiDate();
                        merchantProfileDataRow["UserId"] = 215;

                        merchantProfileDataTable.Rows.Add(merchantProfileDataRow);

                        var terminalDataRow = terminalDataTable.NewRow();
                        terminalDataRow["PspId"] = pspId;
                        terminalDataRow["Title"] = row[rowNumber, 23].Text;
                        terminalDataRow["EnglishTitle"] = row[rowNumber, 24].Text;
                        terminalDataRow["DeviceTypeId"] = deviceTypeId;
                        terminalDataRow["BranchId"] = branchId;
                        terminalDataRow["AccountNo"] = accountNumber;
                        terminalDataRow["ShebaNo"] = row[rowNumber, 27].Text;
                        terminalDataRow["StatusId"] = statusId;
                        terminalDataRow["CityId"] = cityId;
                        terminalDataRow["TelCode"] = row[rowNumber, 30].Text;
                        terminalDataRow["Tel"] = row[rowNumber, 31].Text;
                        terminalDataRow["RegionalMunicipalityId"] = string.IsNullOrEmpty(row[rowNumber, 32].Text)
                            ? (object) DBNull.Value
                            : Convert.ToByte(row[rowNumber, 32].Text);
                        terminalDataRow["GuildId"] = guildId;
                        terminalDataRow["ActivityTypeId"] = activityTypeId;
                        terminalDataRow["TerminalNo"] = row[rowNumber, 35].Text;
                        terminalDataRow["Address"] = row[rowNumber, 36].Text;
                        terminalDataRow["EnglishAddress"] = row[rowNumber, 37].Text;
                        terminalDataRow["ShaparakAddressFormat"] = row[rowNumber, 38].Text;
                        terminalDataRow["PostCode"] = row[rowNumber, 39].Text;
                        terminalDataRow["MarketerId"] = marketerId;
                        terminalDataRow["ContractNo"] = row[rowNumber, 41].Text;
                        terminalDataRow["ContractDate"] = row[rowNumber, 42].Text.ToMiladiDate().ToShortDateString();
                        terminalDataRow["MerchantNo"] = row[rowNumber, 43].Text;
                        terminalDataRow["SubmitTime"] = row[rowNumber, 44].Text.ToMiladiDate();
                        terminalDataRow["InstallationDate"] =
                            row[rowNumber, 45].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        terminalDataRow["RevokeDate"] =
                            row[rowNumber, 46].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        terminalDataRow["BatchDate"] =
                            row[rowNumber, 47].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        terminalDataRow["LastUpdateTime"] = row[rowNumber, 48].Text.ToMiladiDate();
                        terminalDataRow["BlockDocumentDate"] =
                            row[rowNumber, 49].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        terminalDataRow["BlockDocumentNumber"] = row[rowNumber, 50].Text;
                        terminalDataRow["BlockDocumentStatusId"] = blockDocumentStatusId;
                        terminalDataRow["BlockAccountNumber"] = row[rowNumber, 52].Text;
                        terminalDataRow["BlockPrice"] = string.IsNullOrEmpty(row[rowNumber, 53].Text)
                            ? (object) DBNull.Value
                            : Convert.ToInt64(row[rowNumber, 53].Text);
                        terminalDataRow["UserId"] = 215;
                        terminalDataRow["MerchantProfileId"] = merchatProfileIdList[rowNumber - 2];
                        terminalDataRow["TaxPayerCode"] = row[rowNumber, 54].Text;

                        terminalDataTable.Rows.Add(terminalDataRow);

                        terminalNoList.Add(row[rowNumber, 35].Text);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }

                if (errorMessageList.Any())
                {
                    return JsonWarningMessage("لطفاً خطاهای اعلام شده را بررسی نموده و مجدداً فایل را بارگذاری نمایید.",
                        data: errorMessageList);
                }
            }

          
            
            using (                                var sqlConnection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
            {
                await sqlConnection.OpenAsync(cancellationToken);

                using (var transaction = sqlConnection.BeginTransaction())
                {
                    using (var sqlBulkCopy =
                        new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction))
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Id", "Id"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FirstName", "FirstName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LastName", "LastName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnglishFirstName",
                            "EnglishFirstName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnglishLastName",
                            "EnglishLastName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NationalCode", "NationalCode"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsMale", "IsMale"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("HomeTel", "HomeTel"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Mobile", "Mobile"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("HomeAddress", "HomeAddress"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("HomePostCode", "HomePostCode"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsLegalPersonality",
                            "IsLegalPersonality"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NationalityId", "NationalityId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FatherName", "FatherName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnglishFatherName",
                            "EnglishFatherName"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IdentityNumber",
                            "IdentityNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Birthdate", "Birthdate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BirthCertificateIssueDate",
                            "BirthCertificateIssueDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CompanyRegistrationNumber",
                            "CompanyRegistrationNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CompanyRegistrationDate",
                            "CompanyRegistrationDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LegalNationalCode",
                            "LegalNationalCode"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LastUpdateTime",
                            "LastUpdateTime"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CustomerNumber",
                            "CustomerNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("UserId", "UserId"));

                        sqlBulkCopy.BatchSize = 10000;
                        sqlBulkCopy.BulkCopyTimeout = 10000;
                        sqlBulkCopy.DestinationTableName =
                            $"[{_dataContext.Database.Connection.Database}].[psp].[MerchantProfile]";
   
                        try
                        {
                            await sqlBulkCopy.WriteToServerAsync(merchantProfileDataTable, cancellationToken);
                        }
                        catch(Exception ex)
                        {
                            transaction.Rollback();
                        }

                        sqlBulkCopy.DestinationTableName =
                            $"[{_dataContext.Database.Connection.Database}].[psp].[Terminal]";

                        sqlBulkCopy.ColumnMappings.Clear();
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PspId", "PspId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Title", "Title"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnglishTitle", "EnglishTitle"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("DeviceTypeId", "DeviceTypeId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchId", "BranchId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ShebaNo", "ShebaNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("AccountNo", "AccountNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StatusId", "StatusId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CityId", "CityId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TelCode", "TelCode"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Tel", "Tel"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("RegionalMunicipalityId",
                            "RegionalMunicipalityId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("GuildId", "GuildId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ActivityTypeId",
                            "ActivityTypeId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Address", "Address"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EnglishAddress",
                            "EnglishAddress"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ShaparakAddressFormat",
                            "ShaparakAddressFormat"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PostCode", "PostCode"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("MarketerId", "MarketerId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ContractNo", "ContractNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ContractDate", "ContractDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("MerchantNo", "MerchantNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDate",
                            "InstallationDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("RevokeDate", "RevokeDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BatchDate", "BatchDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BlockDocumentDate",
                            "BlockDocumentDate"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BlockDocumentNumber",
                            "BlockDocumentNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BlockDocumentStatusId",
                            "BlockDocumentStatusId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BlockAccountNumber",
                            "BlockAccountNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BlockPrice", "BlockPrice"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("UserId", "UserId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("MerchantProfileId",
                            "MerchantProfileId"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TaxPayerCode", "TaxPayerCode"));

                        try
                        {
                            await sqlBulkCopy.WriteToServerAsync(terminalDataTable, cancellationToken);
                        }
                        catch(Exception ex)
                        {
                            transaction.Rollback();
                        }

                        transaction.Commit();
                    }
                }
            }


            if (terminalNoList.Any())
            {
                for (var i = 0; i < terminalNoList.Count; i++)
                {
                    var terminalNo = terminalNoList[i];
                    var CalculateResult = _dataContext.CalculateResults.Where(a => a.TerminalNo == terminalNo )
                        .OrderByDescending(a=>a.IsGoodYear).ThenByDescending(a=>a.IsGoodMonth)
                        .FirstOrDefault();
                    if (CalculateResult != null && CalculateResult.IsInNetwork.HasValue && !CalculateResult.IsInNetwork.Value  )
                    {
                        var terminal = _dataContext.Terminals.FirstOrDefault(a => a.TerminalNo == terminalNo);
                        if (terminal!= null)
                        {
                            terminal.IsGood = CalculateResult.IsGood;
                            terminal.IsGoodValue = CalculateResult.IsGoodValue;
                            terminal.IsGoodMonth = CalculateResult.IsGoodMonth;
                            terminal.LowTransaction = CalculateResult.LowTransaction;
                            terminal.TransactionCount = CalculateResult.TransactionCount;
                            terminal.TransactionValue = CalculateResult.TransactionValue;
                            CalculateResult.IsInNetwork = true;
                        }
                    }
                
                }

                _dataContext.SaveChanges();
            }
            
            return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات پایانه ها از طریق فایل با موفقیت انجام شد.");
        }

        
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> BatchImport(string ab, CancellationToken cancellationToken)
        {
            UpdateJob updateJob = new UpdateJob();
            updateJob.StartDateTime = DateTime.Now.ToPersianDateTime();
            _dataContext.UpdateJob.Add(updateJob);
            _dataContext.SaveChanges();
            try
            {
                #region

                var terminalDataTable = new DataTable();

                terminalDataTable.Columns.Add(new DataColumn("StatusId", typeof(byte)));
                terminalDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
                terminalDataTable.Columns.Add(new DataColumn("InstallationDate", typeof(DateTime)));
                terminalDataTable.Columns.Add(new DataColumn("RevokeDate", typeof(DateTime)));
                terminalDataTable.Columns.Add(new DataColumn("BatchDate", typeof(DateTime)));
                terminalDataTable.Columns.Add(new DataColumn("MerchantProfileId", typeof(long)));

                #endregion

                var badData = _dataContext.TerminalNotes.Include(b => b.Terminal)
                    .Where(a => a.Body.Contains("حذف")
                       ).Select(a => a.TerminalId)
                    .ToList();


                var path2 = Server.MapPath("~/Job/UpdateTerminal.xlsx");
                var path = Server.MapPath("~/Job/Result.txt");

                var stream = System.IO.File.OpenRead(path2);
                System.IO.File.WriteAllText(path, "");

                using (var package = new ExcelPackage(stream))
                {
                    var workSheet = package.Workbook.Worksheets.First();

                    var tx = _dataContext.Terminals.AsQueryable();
                    var errorMessageList = new List<string>();


                    var rowNumber = 2;

                    updateJob.RowNumber = workSheet.Dimension.End.Row;
                    _dataContext.SaveChanges();


                    for (rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                    {
                        // var updateJobDetails = new UpdateJobDetails();
                        var thisItemErrorMessageList = new List<string>();
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        try
                        {
                            var statusId = GetStatusIdFromText(row[rowNumber, 19].Text);
                            if (!statusId.HasValue)
                            {
                                thisItemErrorMessageList.Add(
                                    $"سطر {rowNumber} - ستون 19 : وضعیت وارد شده صحیح نمی باشد");

                                //   updateJobDetails.TerminalNumber =  row[rowNumber, 1].Text;
                                //  updateJobDetails.UpdateJob = updateJob;
                                //  updateJobDetails.ErrorMessage = "Status Is Not Valid";
                                //   updateJob.HasError = true;
                                //   _dataContext.UpdateJobDetails.Add(updateJobDetails);
                                //  _dataContext.SaveChanges();
                                var isOk = false;


                                do
                                {
                                    try
                                    {
                                        System.IO.File.AppendAllText(path,
                                            $"{rowNumber} - Terimnal Number : {row[rowNumber, 1].Text} - Faield : Status Not Found");

                                        isOk = true;
                                    }
                                    catch
                                    {
                                    }
                                } while (!isOk);

                                continue;
                            }


                            var tn = row[rowNumber, 1].Text;
                            var termianl = tx.FirstOrDefault(a => a.TerminalNo == tn);

                            if (termianl != null)
                            {
                                if (badData.Contains(termianl.Id))

                                {
                                    termianl.StatusId = (byte) Enums.TerminalStatus.Deleted;
                                }
                                else
                                {
                                    termianl.StatusId = statusId.HasValue ? statusId.Value : termianl.StatusId;
                                }

                                termianl.InstallationDate = row[rowNumber, 26].Text.ToNullableMiladiDate() ??
                                                           null;
                                termianl.RevokeDate = row[rowNumber, 27].Text.ToNullableMiladiDate() ??
                                                      null;
                                termianl.BatchDate =
                                    row[rowNumber, 25].Text.ToNullableMiladiDate() ?? termianl.BatchDate;

                                //  updateJobDetails.TerminalNumber = tn;
                                //  updateJobDetails.UpdateJob = updateJob;
                                // _dataContext.UpdateJobDetails.Add(updateJobDetails);
                                // _dataContext.SaveChanges();


                                var isOk = false;


                                do
                                {
                                    try
                                    {
                                        System.IO.File.AppendAllText(path,
                                            $"{rowNumber} - Terimnal Number : {row[rowNumber, 1].Text} - Successed" +
                                            Environment.NewLine);
                                        isOk = true;
                                    }
                                    catch
                                    {
                                    }
                                } while (!isOk);
                            }
                            else
                            {
                                // updateJobDetails.TerminalNumber =  row[rowNumber, 1].Text;
                                // updateJobDetails.UpdateJob = updateJob;
                                // updateJobDetails.ErrorMessage =" پایانه یافت نشد";
                                //updateJob.HasError = true;
                                // _dataContext.UpdateJobDetails.Add(updateJobDetails);
                                //_dataContext.SaveChanges();


                                var isOk = false;

                                do
                                {
                                    try
                                    {
                                        System.IO.File.AppendAllText(path,
                                            $"{rowNumber} - Terimnal Number : {row[rowNumber, 1].Text} - Faield : Terminal Not Found" +
                                            Environment.NewLine);
                                        isOk = true;
                                    }
                                    catch
                                    {
                                    }
                                } while (!isOk);
                            }
                        }
                        catch (Exception ex)
                        {
                            //  updateJobDetails.TerminalNumber =  row[rowNumber, 1].Text;
                            // updateJobDetails.UpdateJob = updateJob;
                            //  updateJobDetails.ErrorMessage = ex.Message;
                            //  updateJob.HasError = true;
                            //   _dataContext.UpdateJobDetails.Add(updateJobDetails);
                            //  _dataContext.SaveChanges();


                            var isOk = false;

                            do
                            {
                                try
                                {
                                    System.IO.File.AppendAllText(path,
                                        $"{rowNumber} - Terimnal Number : {row[rowNumber, 1].Text} - Faield : {ex.Message}" +
                                        Environment.NewLine);
                                    isOk = true;
                                }
                                catch
                                {
                                }
                            } while (!isOk);

                            errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                        }
                    }
                }

                try
                {
                    _dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    var u = _dataContext.UpdateJob.FirstOrDefault(a => a.Id == updateJob.Id);
                    u.HasError = true;
                    u.EndDateTime = DateTime.Now.ToPersianDateTime();
                    u.ErrorMessage = ex.Message;
                    _dataContext.SaveChanges();
                    return JsonErrorMessage(ex.Message);
                }

                var u2 = _dataContext.UpdateJob.FirstOrDefault(a => a.Id == updateJob.Id);
                u2.HasError = false;
                u2.EndDateTime = DateTime.Now.ToPersianDateTime();
                _dataContext.SaveChanges();

                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات پایانه ها از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                var u = _dataContext.UpdateJob.FirstOrDefault(a => a.Id == updateJob.Id);
                u.HasError = true;
                u.EndDateTime = DateTime.Now.ToPersianDateTime();
                u.ErrorMessage = ex.Message;
                _dataContext.SaveChanges();
                return JsonErrorMessage(ex.Message);
            }
        }

        private static byte? GetBlockDocumentStatusIdFromText(string input)
        {
            switch (input)
            {
                case "ثبت شده":
                    return 1;
                case "ثبت نشده":
                    return 2;
                case "در انتظار پایش دوره ای":
                    return 3;
                case "در انتظار بررسی":
                    return 4;
                default:
                    return null;
            }
        }

        private static byte? GetActivityTypeIdFromText(string input)
        {
            switch (input)
            {
                case "فیزیکی":
                    return 1;
                case "مجازی":
                    return 2;
                case "فیزیکی - مجازی":
                    return 3;
                default:
                    return null;
            }
        }

        private static long? GetMarketerIdFromText(string input)
        {
            switch (input)
            {
                case "شرکت تجارت الکترونیک سرمایه":
                    return 1;
                case "نماینده شرکت psp":
                    return 2;
                case "بانک/شعبه":
                    return 3;
                case "شرکت 1":
                    return 4;
                case "شرکت 2":
                    return 5;
                case "شرکت 3":
                    return 6;
                default:
                    return null;
            }
        }

        private static byte? GetPspIdFromText(string input)
        {
            switch (input)
            {
                case "فن آوا":
                    return 1;
                case "ایران کیش":
                    return 2;
                case "پارسیان":
                    return 3;
                case "پرداخت نوین":
                    return 4;
                default:
                    return null;
            }
        }

        private static long? GetDeviceTypeIdFromText(string input)
        {
            switch (input)
            {
                case "Dialup":
                    return 1;
                case "LAN POS":
                    return 2;
                case "GPRS":
                    return 3;
                case "PDA":
                    return 6;
                case "MPOS":
                    return 7;
                case "Wifi":
                    return 8;
                case "PCPOS":
                    return 9;
                case "BlueTooth":
                    return 13;
                case "IPG":
                    return 14;
                case "Cacheless ATM":
                    return 16;
                case "DailLANGPRSWifi":
                    return 19;
                case "PinPad":
                    return 20;
                case "Base":
                    return 21;
                default:
                    return null;
            }
        }

        private static bool? GetIsLegalPersonalityFromText(string input)
        {
            input = input.Trim().ApplyPersianYeKe();

            switch (input)
            {
                case "حقیقی":
                    return false;
                case "حقوقی":
                    return true;
                default:
                    return null;
            }
        }

        private static bool? GetIsMaleFromText(string input)
        {
            input = input.Trim().ApplyPersianYeKe();

            switch (input)
            {
                case "مرد":
                    return true;
                case "زن":
                    return false;
                default:
                    return null;
            }
        }

        private static long? GetNationalityIdFromText(string input)
        {
            input = input.Trim().ApplyPersianYeKe();

            switch (input)
            {
                case "ایرانی":
                    return (long) Enums.Nationality.Persian;
                case "افغانی":
                    return (long) Enums.Nationality.NonePersian;
                default:
                    return null;
            }
        }

        private static byte? GetStatusIdFromText(string input)
        {
            input = input.Trim().ApplyPersianYeKe();

            switch (input)
            {
            
                    
                case "ورود بازاریابی":
                    return (byte) Enums.TerminalStatus.New;
                case "برنگشته از سوئیچ":
                    return (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                case "نیازمند اصلاح":
                case "عدم تاييد تعريف ترمينال در  شاپرک":
                case "عدم تایید تعریف ترمینال در  شاپرک":
                    return (byte) Enums.TerminalStatus.NeedToReform;
                case "آماده تخصیص":
                case "کد باز":
                    return (byte) Enums.TerminalStatus.ReadyForAllocation;
                case "تخصیص داده شده":
                case "تخصيص يافته":
                case "تخصیص یافته":
                    return (byte) Enums.TerminalStatus.Allocated;
                case "تست شده":
                    return (byte) Enums.TerminalStatus.Test;
                case "نصب شده":
                case "نصب":
                    case "تایید ناظر آرشیو":
                case "راه اندازي شده":
                case "راه اندازی شده":
                case "تکمیل مدارک":
                    return (byte) Enums.TerminalStatus.Installed;
                case "جمع آوری شده":
                case "غير فعال":
                case "غیر فعال":
                case "ارسال فايل غير فعال سازي به خدمات":
                case "ارسال فایل غیر فعال سازی به خدمات":
                case "آماده فسخ":
                case "ابطال شده":
                case "عدم تاييد غير فعال سازي در شاپرک":
                case "عدم تایید غیر فعال سازی در شاپرک":
                case "عدم تاييدتعريف ترمينال در خدمات":
                case "عدم تاییدتعریف ترمینال در خدمات":
                    return (byte) Enums.TerminalStatus.Revoked;
                case "ارسال شده به شاپرک":
                    return (byte) Enums.TerminalStatus.SendToShaparak;
                case "لغو نصب":
                case "حذف شده":
                case "ارسال فايل تعريف ترمينال به شاپرک":
                case "ارسال فایل تعریف ترمینال به شاپرک":
                    return (byte) Enums.TerminalStatus.Deleted;
                case "دریافت شده از سویچ ناموفق":
                    return (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                case "در انتظار جمع آوری و غیر فعال سازی":
                case "تاييد بررسي درخواست ابطال":
                case "تایید بررسی درخواست ابطال":
                case "در انتظار ابطال":
                case "درخواست ابطال فایلی":
                case "درخواست ابطال فرمی":
                case "درخواست غير فعال سازي":
                case "در انتظار جمع آوری":
                case "ارسال فايل غير فعال سازي به شاپرک":
                case "درخواست غیر فعال سازی":
                    return (byte) Enums.TerminalStatus.WaitingForRevoke;
                case "تحت تعمير":
                case "تحت تعمیر":
                    return (byte) Enums.TerminalStatus.Repairing;
                default:
                    return null;
            }
        }

        private JsonAppResult ValidateMerchantDataEntryViewModel(MerchantDataEntryViewModel viewModel)
        {
            var customerNumber = viewModel.CustomerNumber.PadLeft(8, '0');

            var customerNumberBlackListInfo = _dataContext.CustomerNumberBlackLists
                .Where(x => x.CustomerNumber == customerNumber)
                .Select(x => new {x.Description, x.SubmitTime})
                .FirstOrDefault();

            if (customerNumberBlackListInfo != null)
            {
                return JsonErrorMessage(string.IsNullOrEmpty(customerNumberBlackListInfo.Description)
                    ? "شماره مشتری وارد شده در لیست سیاه قرار دارد و امکان ثبت درخواست نصب پایانه وجود ندارد"
                    : $"شماره مشتری وارد شده در لیست سیاه مشتریان بوده و امکان ثبت درخواست نصب پایانه فروش جدید برای این مشتری وجود ندارد. اضافه شدن مشتری مذکور به لیست سیاه مشتریان ناشی از ثبت درخواست جمع آوری پایانه قبلی این مشتری توسط شعبه به دلیل '{customerNumberBlackListInfo.Description}' در تاریخ {customerNumberBlackListInfo.SubmitTime.ToPersianDate()} بوده است.");
            }

            if (!viewModel.PostCode.IsValidPostCode())
            {
                return JsonErrorMessage("کد پستی محل پذیرنده صحیح نمی باشد");
            }

            if (viewModel.BlockPrice > 0)
            {
                if (string.IsNullOrEmpty(viewModel.BlockAccountBranchCode) ||
                    string.IsNullOrEmpty(viewModel.BlockAccountCustomerNumber) ||
                    string.IsNullOrEmpty(viewModel.BlockAccountRow) ||
                    string.IsNullOrEmpty(viewModel.BlockAccountType) ||
                    string.IsNullOrEmpty(viewModel.BlockDocumentNumber) ||
                    !viewModel.BlockDocumentDate.HasValue)
                {
                    return JsonWarningMessage("پر کردن اطلاعات مسدودی اجباری است");
                }

                if (_dataContext.Terminals.Any(x =>
                    x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                    x.BlockDocumentNumber == viewModel.BlockDocumentNumber))
                {
                    return JsonWarningMessage("شماره سند مسدودی وارد شده تکراری است.");
                }

                AccountNumberExtensions.GenerateAccountNumber(viewModel.AccountBranchCode, viewModel.BlockAccountType,
                    viewModel.AccountCustomerNumber, viewModel.BlockAccountRow, out var blockAccountNumberWithDash,
                    out _);
                if (!TosanService.TryGetAccountOwnerFullName(blockAccountNumberWithDash, out _, out _))
                {
                    return JsonErrorMessage(
                        "خطا در استعلام شماره حساب مسدودی. لطفاً از صحت شماره حساب مسدودی مطمئن شده و مجدداً تلاش نمایید.");
                }
            }

            if (string.IsNullOrEmpty(viewModel.Address) || viewModel.Address.Length > 100)
            {
                return JsonWarningMessage("آدرس پذیرنده نباید کمتر از یک کاراکتر و بیشتر از 100 کاراکتر باشد");
            }

            if (DateTimeExtensions.CalculateAge(viewModel.Birthdate) > 100 ||
                DateTimeExtensions.CalculateAge(viewModel.Birthdate) < 18)
            {
                return JsonWarningMessage("سن پذیرنده بایستی بزرگتر از 18 سال و کوچکتر از 100 سال باشد.");
            }

            if (viewModel.BirthCertificateIssueDate >= DateTime.Today)
            {
                return JsonWarningMessage("تاریخ صدور شناسنامه بایستی کوچکتر از تاریخ امروز باشد.");
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && !x.PostedFile.IsValidFormat(".pdf")))
            {
                return JsonWarningMessage("تنها فرمت قابل قبول برای مدارک pdf می باشد.");
            }

            if (string.IsNullOrEmpty(viewModel.ShaparakAddressFormat) ||
                viewModel.ShaparakAddressFormat.Split('،').Length < 2)
            {
                return JsonWarningMessage(
                    "ثبت آدرس پذیرنده الزامی می‌باشد. توجه نمایید که آدرس بایستی حداقل دارای دو بخش باشد.");
            }

            if (viewModel.BlockPrice > 0 && viewModel.PostedFiles.Any(x =>
                x.DocumentTypeId == (long) Enums.DocumentType.SanadMasdoodi && !x.PostedFile.IsValidFormat(".pdf")))
            {
                return JsonWarningMessage("لطفاً فایل سند مسدودی را انتخاب نمایید.");
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && x.PostedFile.ContentLength > 3670016))
            {
                return JsonWarningMessage("حجم هر کدام از مدارک ارسال شده نباید بیشتر از 3.5 مگابایت باشد.");
            }

            if (viewModel.PostedFiles.Where(x => x.DocumentTypeId != (long) Enums.DocumentType.SanadMasdoodi).Any(x =>
                (x.IsForLegalPersonality == viewModel.IsLegalPersonality || !x.IsForLegalPersonality.HasValue) &&
                x.IsRequired && (!x.PostedFile.IsValidFile() || !x.PostedFile.IsValidFormat(".pdf"))))
            {
                return JsonWarningMessage("فایل های الزامی مشخص شده را وارد نمایید.");
            }

            // if (viewModel.Title.Length > 24)
            // {
            //     return JsonWarningMessage("نام فروشگاه نبایستی بیشتر از 24 کاراکتر باشد.");
            // }

            if (string.IsNullOrEmpty(viewModel.PostCode) || viewModel.PostCode.Length != 10 ||
                !viewModel.PostCode.IsItNumber())
            {
                return JsonWarningMessage("لطفاً کد پستی محل پذیرنده را به صورت صحیح (10 رقمی) وارد نمایید.");
            }

            viewModel.AccountBranchCode = viewModel.AccountBranchCode.PadLeft(4, '0');
            viewModel.AccountType = viewModel.AccountType.PadLeft(3, '0');
            viewModel.AccountCustomerNumber = viewModel.AccountCustomerNumber.PadLeft(8, '0');
            viewModel.AccountRow = viewModel.AccountRow.PadLeft(3, '0');

            var errorList = ModelState.ToErrorList();
            return errorList.Any() ? JsonWarningMessage(errorList) : null;
        }
    }
}