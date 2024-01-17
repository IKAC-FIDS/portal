using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class BlockBranchPermission
    {
        public int? PersianLocalMonth { get; set; }
        public long BranchId { get; set; }
        public string TerminalNo { get; set; }
        public long? sumprice { get; set; }

        public string BranchTitle { get; set; }
        public string Title { get; set; }
        public int? totalcount { get; set; }
        public string Status { get; set; }
        public long DeviceTypeId { get; set; }
        public int? PersianLocalYear { get; set; }
        public string PspTitle { get; set; }
        public string DeviceTypeTitle { get; set; }
    }

    public class BranchTerminalCount
    {
        public long BranchId { get; set; }
        public int TerminalCount { get; set; }
    }

    public class BlockBranchPermissionReport
    {
        public string Title { get; set; }
        public long BranchId { get; set; }
        public string Status { get; set; }
        
        public int TotalTerminalCount { get; set; }
        public int TotalLowTransactionTerminalCount { get; set; }
        public float LowtoAllPercent { get; set; }
        public int TotallDeviceCount { get; set; }
        public int WifiDeviceCount { get; set; }
        public float WifitoallDevicePercentage { get; set; }
        public long TotalTerminalTransaction { get; set; }
        public int LowCount { get; set; }
        public int StatusId { get; set; }
        public string RoundLowtoAllPercent { get; set; }
        public int Month { get; set; }
    }

    public class BlockBranchPermissionController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public BlockBranchPermissionController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            ViewBag.BlockDocumentStatusList = (await _dataContext.BlockDocumentStatuses
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.TerminalStatusList = (await _dataContext.BranchPermissionType
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);


            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive && x.IsWireless)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits.Where(d => d.Id >= 1000)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
           //                                           && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                               || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage = message.Count(d => d.StatusId == (int) Enums.MessageStatus.UnderReview
           //                                                 && (d.UserId == CurrentUserId ||
           //                                                     d.ReviewerUserId == CurrentUserId
           //                                                     || User.IsMessageManagerUser()));
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));
            
            return View();
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(BlockDocumentSearchViewModel viewModel, bool retriveTotalPageCount,
            int page, CancellationToken cancellationToken)
        {
            var rowCount = _dataContext.BranchPermission.Select(bp => bp).Count();
            var rows = _dataContext.BranchPermission.Select(bp => bp)
                .OrderBy(x => x.BranchId)
                .Skip(page - 1)
                .Take(300);

            var totalRowsCount = 0;
            if (retriveTotalPageCount)
            {
                totalRowsCount = rowCount;
            }

            return JsonSuccessResult(new {rows, totalRowsCount});
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetDetailData(long branchId   
            )
        {
            var m1 = 8;
            var m2 = 9;

            var y1 = DateTime.Now.ToPersianYear();
            var y2 = y1;

            switch (DateTime.Now.ToPersianMonth())
            {
                case 1:
                    m1 = 11;
                    y1 = y1 - 1;
                    y2 = y2 - 1;
                    m2 = 12;
                    break;
                case 2:
                    m1 = 12;
                    y1 = y1 - 1;
                    m2 = 1;
                    break;
                default:
                    m1 = DateTime.Now.ToPersianMonth() - 2;
                    m2 = DateTime.Now.ToPersianMonth() - 1;
                    break;
            }

            var s = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                    " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                    new SqlParameter("YearTwo", y2),
                    new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m2))
                .Where(d => d.BranchId == branchId)
                .ToList();

            var branchTerminalCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwTerminalBranchCount").ToList();

            var vwWifiTerminalBranchCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwWifiTerminalBranchCount").ToList();
            var fx = s.GroupBy(d => d.BranchId);

            var intList = new List<string>();

            var result = new List<BlockBranchPermissionReport>();
            foreach (var branch in fx)
            {
                var r = new BlockBranchPermissionReport();
                r.Title = branch.FirstOrDefault().Title;

                r.LowCount = 0;
                var terminals = branch.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.LowCount = r.LowCount + 1;
                        intList.Add(terminal.FirstOrDefault().TerminalNo);
                    }
                }

                r.TotalTerminalCount = branchTerminalCount.Where(a => a.BranchId == branch.FirstOrDefault().BranchId)
                    .FirstOrDefault().TerminalCount;


                //  r.TotalLowTransactionTerminalCount = branch.Where(d => d.Status == "Low transaction").Count();
                r.LowtoAllPercent = ((float) r.LowCount / (float) r.TotalTerminalCount) * 100;
                r.WifiDeviceCount =
                    vwWifiTerminalBranchCount.Where(d => d.BranchId == branch.FirstOrDefault().BranchId)
                        .FirstOrDefault().TerminalCount;


                r.WifitoallDevicePercentage = ((float) r.WifiDeviceCount / (float) r.TotalTerminalCount) * 100;

                if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                    r.WifitoallDevicePercentage <= 30)
                {
                    r.Status = "مجاز به ثبت درخواست";
                    r.StatusId = 1;
                }
                else if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                         r.WifitoallDevicePercentage > 30)
                {
                    r.Status = "مجاز به ثبت دستگاه ثابت";
                    r.StatusId = 2;
                }
                else
                {
                    r.Status = "غیر مجاز به ثبت درخواست";
                    r.StatusId = 3;
                }


                result.Add(r);
            }


            var viewModel =   _dataContext.Terminals.Where(d => intList.Contains(d.TerminalNo))
                .Select(x => new TerminalDetailsViewModel
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    UserId = x.UserId,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    ShebaNo = x.ShebaNo,
                    TelCode = x.TelCode,
                    PostCode = x.PostCode,
                    BranchId = x.BranchId,
                    StatusId = x.StatusId,
                    PspTitle = x.Psp.Title,
                    AccountNo = x.AccountNo,
                    BatchDate = x.BatchDate,
                    CityTitle = x.City.Title,
                    MarketerId = x.MarketerId,
                    ContractNo = x.ContractNo,
                    TerminalNo = x.TerminalNo,
                    MerchantNo = x.MerchantNo,
                    RevokeDate = x.RevokeDate,
                    SubmitTime = x.SubmitTime,
                    StatusTitle = x.Status.Title,
                    BranchTitle = x.Branch.Title,
                    ErrorComment = x.ErrorComment,
                    ContractDate = x.ContractDate,
                    DeviceTypeId = x.DeviceTypeId,
                    StateTitle = x.City.State.Title,
                    MarketerTitle = x.Marketer.Title,
                    LastUpdateTime = x.LastUpdateTime,
                    Mobile = x.MerchantProfile.Mobile,
                    HomeTel = x.MerchantProfile.HomeTel,
                    DeviceTypeTitle = x.DeviceType.Title,
                    InstallationDate = x.InstallationDate,
                    IsMale = x.MerchantProfile.IsMale,
                    LastName = x.MerchantProfile.LastName,
                    SubmitterUserFullName = x.User.FullName,
                    MerchantProfileId = x.MerchantProfileId,
                    Birthdate = x.MerchantProfile.Birthdate,
                    FirstName = x.MerchantProfile.FirstName,
                    ActivityTypeTitle = x.ActivityType.Title,
                    FatherName = x.MerchantProfile.FatherName,
                    HomeAddress = x.MerchantProfile.HomeAddress,
                    GenderTitle = x.MerchantProfile.IsMale ? "مرد" : "زن",
                    HomePostCode = x.MerchantProfile.HomePostCode,
                    NationalCode = x.MerchantProfile.NationalCode,
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    IdentityNumber = x.MerchantProfile.IdentityNumber,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                    EnglishLastName = x.MerchantProfile.EnglishLastName,
                    EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                    NationalityTitle = x.MerchantProfile.Nationality.Title,
                    SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                    LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                    IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                    BlockAccountNumber = x.BlockAccountNumber,
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    BlockPrice = x.BlockPrice,
                    PreferredPspTitle = x.PreferredPsp.Title,
                    TaxPayerCode = x.TaxPayerCode,
                    CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                    LegalPersonalityTitle = x.MerchantProfile.IsLegalPersonality ? "حقوقی" : "حقیقی",
                    CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                    RegionalMunicipalityTitle =
                        x.RegionalMunicipalityId.HasValue ? x.RegionalMunicipality.Title : string.Empty,
                    GuildTitle = x.Guild.ParentId.HasValue
                        ? x.Guild.Parent.Title + " / " + x.Guild.Title
                        : x.Guild.Title,
                    TerminalDocuments = x.TerminalDocuments.Select(y => new DocumentViewModel
                    {
                        Id = y.Id,
                        FileName = y.FileName,
                        DocumentTypeTitle = y.DocumentType.Title
                    }),
                    MerchantProfileDocuments = x.MerchantProfile.MerchantProfileDocuments.Select(y =>
                        new DocumentViewModel
                        {
                            Id = y.Id,
                            FileName = y.FileName,
                            DocumentTypeTitle = y.DocumentType.Title
                        })
                }).ToList();


            var rows = viewModel.Select(x => new
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    UserId = x.UserId,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    ShebaNo = x.ShebaNo,
                    TelCode = x.TelCode,
                    PostCode = x.PostCode,
                    BranchId = x.BranchId,
                    StatusId = x.StatusId,
                    PspTitle = x.PspTitle,
                    AccountNo = x.AccountNo,
                    BatchDate = x.BatchDate,
                    CityTitle = x.CityTitle,
                    MarketerId = x.MarketerId,
                    ContractNo = x.ContractNo,
                    TerminalNo = x.TerminalNo,
                    MerchantNo = x.MerchantNo,
                    RevokeDate = x.RevokeDate,
                    SubmitTime = x.SubmitTime,
                    StatusTitle = x.StatusTitle,
                    x.BranchTitle ,
                    ErrorComment = x.ErrorComment,
                    ContractDate = x.ContractDate,
                    DeviceTypeId = x.DeviceTypeId,
                    x.StateTitle  ,
                    x.MarketerTitle  ,
                    x.LastUpdateTime    ,
                    x. Mobile ,
                    x.HomeTel  ,
                    x.DeviceTypeTitle ,
                    InstallationDate = x.InstallationDate,
                    x.IsMale  ,
                    x.LastName    ,
                    x.SubmitterUserFullName ,
                    MerchantProfileId = x.MerchantProfileId,
                  
                    x.ActivityTypeTitle ,
                    
                    x.GenderTitle    ,
                   
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    x.IdentityNumber ,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                 
                    BlockAccountNumber = x.BlockAccountNumber,
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    BlockPrice = x.BlockPrice,
                    x.PreferredPspTitle ,
                    TaxPayerCode = x.TaxPayerCode,
                })
                .OrderBy(x => x.BranchId)
                 
                ;

            var totalRowsCount = 0;
           

            return JsonSuccessResult(new {rows, totalRowsCount});
        }


       
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Edit(BlockDocumentEditViewModel viewModel, CancellationToken cancellationToken)
        {
            var terminal =
                await _dataContext.Terminals.FirstOrDefaultAsync(x => x.Id == viewModel.TerminalId, cancellationToken);

            // اگر وضعیت ثبت شده نبود و وضعیت جدید ثبت شده بود تاریخ تغییر وضعیت به ثبت شده رو نگه میداریم
            if (terminal.BlockDocumentStatusId != (byte) Enums.BlockDocumentStatus.Registered &&
                viewModel.BlockDocumentStatusId == (byte) Enums.BlockDocumentStatus.Registered)
            {
                terminal.BlockDocumentStatusChangedToRegistredDate = DateTime.Now;
            }

            AccountNumberExtensions.GenerateAccountNumber(viewModel.AccountBranchCode, viewModel.AccountType,
                viewModel.AccountCustomerNumber, viewModel.AccountRow, out var accountNumberWithDash, out _);

            terminal.BlockDocumentStatusId = viewModel.BlockDocumentStatusId;
            terminal.BlockAccountNumber = accountNumberWithDash;
            terminal.BlockDocumentDate = viewModel.BlockDocumentDate;
            terminal.BlockDocumentNumber = viewModel.BlockDocumentNumber;
            terminal.BlockPrice = viewModel.BlockPrice;

            if (viewModel.Document != null)
            {
                if (!viewModel.Document.IsValidFile())
                {
                    return JsonErrorMessage("فایل مسدودی وارد شده معتبر نمی باشد.");
                }

                var terminalBlockDocument = await _dataContext.TerminalDocuments
                    .Where(x => x.TerminalId == viewModel.TerminalId &&
                                x.DocumentTypeId == (byte) Enums.DocumentType.SanadMasdoodi)
                    .FirstOrDefaultAsync(cancellationToken);

                if (terminalBlockDocument != null)
                {
                    terminalBlockDocument.FileName = viewModel.Document.FileName;
                    terminalBlockDocument.FileData = viewModel.Document.ToByteArray();
                }
                else
                {
                    _dataContext.TerminalDocuments.Add(new TerminalDocument
                    {
                        TerminalId = viewModel.TerminalId,
                        FileName = viewModel.Document.FileName,
                        FileData = viewModel.Document.ToByteArray(),
                        DocumentTypeId = (byte) Enums.DocumentType.SanadMasdoodi,
                    });
                }
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Import(CancellationToken cancellationToken)
        {
            var viewModel = new ImportBlockDocumentViewModel
            {
                BlockDocumentStatusList = await _dataContext.BlockDocumentStatuses.Select(x => x.Title)
                    .ToListAsync(cancellationToken)
            };

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

            var updateCommandList = new List<string>();

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var terminalNo = row[rowNumber, 1].Text;
                        var blockDocumentDate = row[rowNumber, 2].Text.ToMiladiDate();
                        var blockDocumentNumber = row[rowNumber, 3].Text;
                        var isValidBlockPrice = int.TryParse(row[rowNumber, 5].Text, out var blockPrice);
                        var blockAccountNumber = row[rowNumber, 4].Text;
                        var blockDocumentStatusId = GetBlockDocumentStatusIdFromText(row[rowNumber, 6].Text);

                        if (!blockDocumentStatusId.HasValue)
                        {
                            errorMessageList.Add($"وضعیت سند مسدودی وارد شده در سطر {rowNumber} صحیح نمی باشد.");
                        }

                        if (!isValidBlockPrice)
                        {
                            errorMessageList.Add($"مبلغ مسدودی وارد شده در سطر {rowNumber} صحیح نمی باشد.");
                        }

                        if (Regex.IsMatch(blockAccountNumber, @"/\d{4}-\d{3}-\d{8}-\d{3}/g"))
                        {
                            errorMessageList.Add($"شماره حساب مسدودی وارد شده در سطر {rowNumber} صحیح نمی باشد.");
                        }

                        updateCommandList.Add(
                            $"UPDATE psp.Terminal SET BlockAccountNumber = '{blockAccountNumber}', BlockDocumentDate = '{blockDocumentDate}', BlockDocumentNumber = '{blockDocumentNumber}', BlockPrice = '{blockPrice}', BlockDocumentStatusId = {blockDocumentStatusId} WHERE TerminalNo = '{terminalNo}'");
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

            await _dataContext.Database.ExecuteSqlCommandAsync(string.Join(Environment.NewLine, updateCommandList),
                cancellationToken);

            return JsonSuccessMessage(
                "فرآیند وارد نمودن اطلاعات سند مسدودی پایانه ها از طریق فایل با موفقیت انجام شد.");
        }
        

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.Administrator, DefaultRoles.BranchUser,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> ExportDetails(  int BranchId)
        {

           var m1 = 8;
            var m2 = 9;

            var y1 = DateTime.Now.ToPersianYear();
            var y2 = y1;

            switch (DateTime.Now.ToPersianMonth())
            {
                case 1:
                    m1 = 11;
                    y1 = y1 - 1;
                    y2 = y2 - 1;
                    m2 = 12;
                    break;
                case 2:
                    m1 = 12;
                    y1 = y1 - 1;
                    m2 = 1;
                    break;
                default:
                    m1 = DateTime.Now.ToPersianMonth() - 2;
                    m2 = DateTime.Now.ToPersianMonth() - 1;
                    break;
            }

            var s = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                    " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                    new SqlParameter("YearTwo", y2),
                    new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m2))
                .Where(d => d.BranchId == BranchId)
                .ToList();

            var branchTerminalCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwTerminalBranchCount").ToList();

            var vwWifiTerminalBranchCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwWifiTerminalBranchCount").ToList();
            var fx = s.GroupBy(d => d.BranchId);

            var intList = new List<string>();

            var result = new List<BlockBranchPermissionReport>();
            foreach (var branch in fx)
            {
                var r = new BlockBranchPermissionReport();
                r.Title = branch.FirstOrDefault().Title;

                r.LowCount = 0;
                var terminals = branch.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.LowCount = r.LowCount + 1;
                        intList.Add(terminal.FirstOrDefault().TerminalNo);
                    }
                }

                r.TotalTerminalCount = branchTerminalCount.Where(a => a.BranchId == branch.FirstOrDefault().BranchId)
                    .FirstOrDefault().TerminalCount;


                //  r.TotalLowTransactionTerminalCount = branch.Where(d => d.Status == "Low transaction").Count();
                r.LowtoAllPercent = ((float) r.LowCount / (float) r.TotalTerminalCount) * 100;
                r.WifiDeviceCount =
                    vwWifiTerminalBranchCount.Where(d => d.BranchId == branch.FirstOrDefault().BranchId)
                        .FirstOrDefault().TerminalCount;


                r.WifitoallDevicePercentage = ((float) r.WifiDeviceCount / (float) r.TotalTerminalCount) * 100;

                if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                    r.WifitoallDevicePercentage <= 30)
                {
                    r.Status = "مجاز به ثبت درخواست";
                    r.StatusId = 1;
                }
                else if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                         r.WifitoallDevicePercentage > 30)
                {
                    r.Status = "مجاز به ثبت دستگاه ثابت";
                    r.StatusId = 2;
                }
                else
                {
                    r.Status = "غیر مجاز به ثبت درخواست";
                    r.StatusId = 3;
                }


                result.Add(r);
            }


            var viewModel =   _dataContext.Terminals.Where(d => intList.Contains(d.TerminalNo))
                .Select(x => new TerminalDetailsViewModel
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    UserId = x.UserId,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    ShebaNo = x.ShebaNo,
                    TelCode = x.TelCode,
                    PostCode = x.PostCode,
                    BranchId = x.BranchId,
                    StatusId = x.StatusId,
                    PspTitle = x.Psp.Title,
                    AccountNo = x.AccountNo,
                    BatchDate = x.BatchDate,
                    CityTitle = x.City.Title,
                    MarketerId = x.MarketerId,
                    ContractNo = x.ContractNo,
                    TerminalNo = x.TerminalNo,
                    MerchantNo = x.MerchantNo,
                    RevokeDate = x.RevokeDate,
                    SubmitTime = x.SubmitTime,
                    StatusTitle = x.Status.Title,
                    BranchTitle = x.Branch.Title,
                    ErrorComment = x.ErrorComment,
                    ContractDate = x.ContractDate,
                    DeviceTypeId = x.DeviceTypeId,
                    StateTitle = x.City.State.Title,
                    MarketerTitle = x.Marketer.Title,
                    LastUpdateTime = x.LastUpdateTime,
                    Mobile = x.MerchantProfile.Mobile,
                    HomeTel = x.MerchantProfile.HomeTel,
                    DeviceTypeTitle = x.DeviceType.Title,
                    InstallationDate = x.InstallationDate,
                    IsMale = x.MerchantProfile.IsMale,
                    LastName = x.MerchantProfile.LastName,
                    SubmitterUserFullName = x.User.FullName,
                    MerchantProfileId = x.MerchantProfileId,
                    Birthdate = x.MerchantProfile.Birthdate,
                    FirstName = x.MerchantProfile.FirstName,
                    ActivityTypeTitle = x.ActivityType.Title,
                    FatherName = x.MerchantProfile.FatherName,
                    HomeAddress = x.MerchantProfile.HomeAddress,
                    GenderTitle = x.MerchantProfile.IsMale ? "مرد" : "زن",
                    HomePostCode = x.MerchantProfile.HomePostCode,
                    NationalCode = x.MerchantProfile.NationalCode,
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    IdentityNumber = x.MerchantProfile.IdentityNumber,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                    EnglishLastName = x.MerchantProfile.EnglishLastName,
                    EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                    NationalityTitle = x.MerchantProfile.Nationality.Title,
                    SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                    LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                    IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                    BlockAccountNumber = x.BlockAccountNumber,
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    BlockPrice = x.BlockPrice,
                    PreferredPspTitle = x.PreferredPsp.Title,
                    TaxPayerCode = x.TaxPayerCode,
                    CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                    LegalPersonalityTitle = x.MerchantProfile.IsLegalPersonality ? "حقوقی" : "حقیقی",
                    CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                    RegionalMunicipalityTitle =
                        x.RegionalMunicipalityId.HasValue ? x.RegionalMunicipality.Title : string.Empty,
                    GuildTitle = x.Guild.ParentId.HasValue
                        ? x.Guild.Parent.Title + " / " + x.Guild.Title
                        : x.Guild.Title,
                    TerminalDocuments = x.TerminalDocuments.Select(y => new DocumentViewModel
                    {
                        Id = y.Id,
                        FileName = y.FileName,
                        DocumentTypeTitle = y.DocumentType.Title
                    }),
                    MerchantProfileDocuments = x.MerchantProfile.MerchantProfileDocuments.Select(y =>
                        new DocumentViewModel
                        {
                            Id = y.Id,
                            FileName = y.FileName,
                            DocumentTypeTitle = y.DocumentType.Title
                        })
                }).ToList();


            if (!viewModel.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add(" پایانه های فاقد و کم تراکنش دو ماه متوالی ");

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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 26;


                worksheet.Cells[1, 1].Value = "   مدل  ";
                worksheet.Cells[1, 2].Value = "    شماره پایانه  ";
                worksheet.Cells[1, 3].Value = "  فروشگاه / شرکت  ";
                worksheet.Cells[1, 4].Value = "   PSP  ";
               

                var rowNumber = 2;

                foreach (var item in viewModel)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 2].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 3].Value = item.Title;
                    worksheet.Cells[rowNumber, 4].Value = item.PspTitle;
                  


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/BlockDocumentExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"BlockBranchPermissionDetails-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}"
                    .ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

                [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.Administrator, 
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> ExportByTerminal(     )
        {

           var m1 = 8;
            var m2 = 9;

            var y1 = DateTime.Now.ToPersianYear();
            var y2 = y1;

            switch (DateTime.Now.ToPersianMonth())
            {
                case 1:
                    m1 = 11;
                    y1 = y1 - 1;
                    y2 = y2 - 1;
                    m2 = 12;
                    break;
                case 2:
                    m1 = 12;
                    y1 = y1 - 1;
                    m2 = 1;
                    break;
                default:
                    m1 = DateTime.Now.ToPersianMonth() - 2;
                    m2 = DateTime.Now.ToPersianMonth() - 1;
                    break;
            }

            var s = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                    " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                    new SqlParameter("YearTwo", y2),
                    new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m2))
               
                .ToList();

          
            var intList = new List<string>();

            
  
            var result = new List<TerminalDetailsViewModel>();
         
                var terminals = s.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                  
                    var r = new TerminalDetailsViewModel();
                    
                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.StateTitle = "Low Transaction";
                    }
                    else
                    {
                        r.StateTitle =   terminal.Any(d => d.Status == " Installed two months ago")
                            ? "Installed two months ago"
                            : "High Transaction";
                    }
                  r. Title = terminal.FirstOrDefault().Title ;                  
                  r. BranchId = terminal.FirstOrDefault().BranchId ;                    
                  r. BranchTitle = terminal.FirstOrDefault().BranchTitle ;
                  
                  r. DeviceTypeId =terminal.FirstOrDefault().DeviceTypeId ;
                  r. TerminalNo =  terminal.FirstOrDefault().TerminalNo ;
                  r. DeviceTypeTitle = terminal.FirstOrDefault().DeviceTypeTitle ;
                  r. PspTitle =  terminal.FirstOrDefault().PspTitle;
                    result.Add(r);
                }

                

                
           
            var viewModel =    result 
                .Select(x => new TerminalDetailsViewModel
                {
                  
                    Title = x.Title,
                  
                    BranchId = x.BranchId,
                    
                    BranchTitle = x.BranchTitle,
                    StateTitle =  x.StateTitle,
                    DeviceTypeId = x.DeviceTypeId,
                    TerminalNo =  x.TerminalNo,
                    DeviceTypeTitle = x.DeviceTypeTitle,
                    PspTitle =  x.PspTitle
                     
                }).ToList();


            if (!viewModel.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add(" پایانه های فاقد و کم تراکنش دو ماه متوالی ");

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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 26;

                worksheet.Cells[1, 1].Value = "    شعبه  ";
                worksheet.Cells[1, 2].Value = "   مدل  ";
                worksheet.Cells[1, 3].Value = "    شماره پایانه  ";
                worksheet.Cells[1, 4].Value = "  فروشگاه / شرکت  ";
                worksheet.Cells[1, 5].Value = "   PSP  ";
                worksheet.Cells[1, 6].Value = "   وضعیت  ";
                

                var rowNumber = 2;

                foreach (var item in viewModel)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 2].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber,3].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 4].Value = item.Title;
                    worksheet.Cells[rowNumber, 5].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.StateTitle;



                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/BlockDocumentExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"BlockBranchPermissionByTerminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}"
                    .ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.Administrator,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> Export(BlockDocumentSearchViewModel viewModel)
        {
            var m1 = 8;
            var m2 = 9;

            var y1 = DateTime.Now.ToPersianYear();
            var y2 = y1;

            switch (DateTime.Now.ToPersianMonth())
            {
                case 1:
                    m1 = 11;
                    y1 = y1 - 1;
                    y2 = y2 - 1;
                    m2 = 12;
                    break;
                case 2:
                    m1 = 12;
                    y1 = y1 - 1;
                    m2 = 1;
                    break;
                default:
                    m1 = DateTime.Now.ToPersianMonth() - 2;
                    m2 = DateTime.Now.ToPersianMonth() - 1;
                    break;
            }

            var s = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                new SqlParameter("YearTwo", y2),
                new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m2)).ToList();

            var branchTerminalCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwTerminalBranchCount").ToList();

            var vwWifiTerminalBranchCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwWifiTerminalBranchCount").ToList();
            var fx = s.GroupBy(d => d.BranchId);

            var result = new List<BlockBranchPermissionReport>();
            foreach (var branch in fx)
            {
                var r = new BlockBranchPermissionReport();
                r.Title = branch.FirstOrDefault().Title;

                r.LowCount = 0;
                var terminals = branch.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                    var terInM1 = terminal.FirstOrDefault(d => d.PersianLocalMonth == m1 && d.PersianLocalYear == y1);

                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.LowCount = r.LowCount + 1;
                    }
                }

                r.TotalTerminalCount = branchTerminalCount.Where(a => a.BranchId == branch.FirstOrDefault().BranchId)
                    .FirstOrDefault().TerminalCount;


                //  r.TotalLowTransactionTerminalCount = branch.Where(d => d.Status == "Low transaction").Count();
                r.LowtoAllPercent = ((float) r.LowCount / (float) r.TotalTerminalCount) * 100;
                r.WifiDeviceCount =
                    vwWifiTerminalBranchCount.Where(d => d.BranchId == branch.FirstOrDefault().BranchId)
                        .FirstOrDefault().TerminalCount;

                var branchId = branch.FirstOrDefault()?.BranchId;
                if (branchId != null) r.BranchId = (long) branchId;

                r.WifitoallDevicePercentage = ((float) r.WifiDeviceCount / (float) r.TotalTerminalCount) * 100;

                if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                    r.WifitoallDevicePercentage <= 30)
                {
                    r.Status = "مجاز به ثبت درخواست";
                    r.StatusId = 1;
                }
                else if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                         r.WifitoallDevicePercentage > 30)
                {
                    r.Status = "مجاز به ثبت دستگاه ثابت";
                    r.StatusId = 2;
                }
                else
                {
                    r.Status = "غیر مجاز به ثبت درخواست";
                    r.StatusId = 3;
                }


                result.Add(r);
            }


            if (viewModel.BranchId.HasValue)
            {
                var childOrganizationUnitIdList =
                    await _dataContext.GetChildOrganizationUnits(viewModel.BranchId.Value);
                result = result.Where(x => x.BranchId == viewModel.BranchId).ToList();
            }

            if (viewModel.StatusId.HasValue)
            {
                result = result.Where(x => x.StatusId == viewModel.StatusId).ToList();
            }


            var data = result
                    .Select(r => new
                    {
                        r.Title,
                        r.TotalTerminalCount,
                        r.TotalLowTransactionTerminalCount,
                        LowtoAllPercent = (float) Math.Round(r.LowtoAllPercent * 100f) / 100f,
                        r.WifiDeviceCount,

                        WifitoallDevicePercentage = (float) Math.Round(r.WifitoallDevicePercentage * 100f) / 100f,
                        r.StatusId,
                        r.Status,
                        r.LowCount,
                        r.BranchId
                    })
                    .OrderBy(x => x.BranchId)
                ;

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add(" پایانه های فاقد و کم تراکنش دو ماه متوالی ");

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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 26;


                worksheet.Cells[1, 1].Value = "  کد شعبه";
                worksheet.Cells[1, 2].Value = " نام شعبه  ";
                worksheet.Cells[1, 3].Value = " تعداد دستگاه های کارتخوان  ";
                worksheet.Cells[1, 4].Value = "   فاقد و کم تراکنش دو ماه  ";
                worksheet.Cells[1, 5].Value = "نسبت فاقد تراکنش و کم تراکنش ( دو ماه متوالی )";
                worksheet.Cells[1, 6].Value = "   درصد بیسیم";
                worksheet.Cells[1, 7].Value = "دسترسی";


                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 2].Value = item.Title;
                    worksheet.Cells[rowNumber, 3].Value = item.TotalTerminalCount;
                    worksheet.Cells[rowNumber, 4].Value = item.LowCount;
                    worksheet.Cells[rowNumber, 5].Value =  item.LowtoAllPercent.ToString("F") ;
                    worksheet.Cells[rowNumber, 6].Value =  item.WifitoallDevicePercentage.ToString("F") ;
                    worksheet.Cells[rowNumber, 7].Value = item.Status;


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/BlockDocumentExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"BlockBranchPermission-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}"
                    .ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> Details(long terminalId, CancellationToken cancellationToken)
        {
            TerminalDetailsViewModel viewModel = new TerminalDetailsViewModel();
            viewModel.BranchId = terminalId;
            return PartialView("_Details", viewModel);
        }
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.Administrator, 
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> GetReport(     )
        
     
        {
           
            var m1 = 1;

            var rows = new List<BlockBranchPermissionReport>();
            var y1 =1401;
            var y2 = 1401;


            while (m1 < 8)
            {

                 var branchPermissions = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                new SqlParameter("YearTwo", y2),
                new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m1)).ToList();
            
            var branchTerminalCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwTerminalBranchCount").ToList();

            var vwWifiTerminalBranchCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwWifiTerminalBranchCount").ToList();
            var fx = branchPermissions.GroupBy(d => d.BranchId);

            var result = new List<BlockBranchPermissionReport>();
            foreach (var branch in fx)
            {
                var r = new BlockBranchPermissionReport();
                r.Title = branch.FirstOrDefault().Title;

                r.LowCount = 0;
                var terminals = branch.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                    var terInM1 = terminal.FirstOrDefault(d => d.PersianLocalMonth == m1 && d.PersianLocalYear == y1);

                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.LowCount = r.LowCount + 1;
                    }
                }

                r.TotalTerminalCount = branchTerminalCount.Where(a => a.BranchId == branch.FirstOrDefault().BranchId)
                    .FirstOrDefault().TerminalCount;


                //  r.TotalLowTransactionTerminalCount = branch.Where(d => d.Status == "Low transaction").Count();
                r.LowtoAllPercent = ((float) r.LowCount / (float) r.TotalTerminalCount) * 100;
                r.RoundLowtoAllPercent = r.LowtoAllPercent.ToString("F");
                r.WifiDeviceCount =
                    vwWifiTerminalBranchCount.Where(d => d.BranchId == branch.FirstOrDefault().BranchId)
                        .FirstOrDefault().TerminalCount;

                var branchId = branch.FirstOrDefault()?.BranchId;
                if (branchId != null) r.BranchId = (long) branchId;

                r.WifitoallDevicePercentage = ((float) r.WifiDeviceCount / (float) r.TotalTerminalCount) * 100;

                if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                    r.WifitoallDevicePercentage <= 30)
                {
                    r.Status = "مجاز به ثبت درخواست";
                    r.StatusId = 1;
                }
                else if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                         r.WifitoallDevicePercentage > 30)
                {
                    r.Status = "مجاز به ثبت دستگاه ثابت";
                    r.StatusId = 2;
                }
                else
                {
                    r.Status = "غیر مجاز به ثبت درخواست";
                    r.StatusId = 3;
                }


                r.Month = m1;
                result.Add(r);
            }



            result.Select(r => new BlockBranchPermissionReport
            {
                Title = r.Title,
                TotalTerminalCount = r.TotalTerminalCount,
                TotalLowTransactionTerminalCount = r.TotalLowTransactionTerminalCount,
                
                WifiDeviceCount = r.WifiDeviceCount,
                Month = m1,
               
                StatusId = r.StatusId,
                Status = r.Status,
                LowCount = r.LowCount,
                BranchId = r.BranchId
            }).ToList();

            
            rows.AddRange(result);
                m1 = m1 + 1;
            }
             
           
            if (!rows.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }
            
                using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add(" پایانه های فاقد و کم تراکنش دو ماه متوالی ");

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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 22;
                worksheet.Column(7).Width = 26;

                worksheet.Cells[1, 8].Value = "    ماه";

                worksheet.Cells[1, 1].Value = "  کد شعبه";
                worksheet.Cells[1, 2].Value = " نام شعبه  ";
                worksheet.Cells[1, 3].Value = " تعداد دستگاه های کارتخوان  ";
                worksheet.Cells[1, 4].Value = "   فاقد و کم تراکنش دو ماه  ";
                worksheet.Cells[1, 5].Value = "نسبت فاقد تراکنش و کم تراکنش ( دو ماه متوالی )";
                worksheet.Cells[1, 6].Value = "   درصد بیسیم";
                worksheet.Cells[1, 7].Value = "دسترسی";


                var rowNumber = 2;

                foreach (var item in rows)
                {

                    worksheet.Cells[rowNumber, 1].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 2].Value = item.Title;
                    worksheet.Cells[rowNumber, 3].Value = item.TotalTerminalCount;
                    worksheet.Cells[rowNumber, 4].Value =  item.LowCount;
                    worksheet.Cells[rowNumber, 5].Value =  item.LowtoAllPercent  ;
                    worksheet.Cells[rowNumber, 6].Value =  item.WifitoallDevicePercentage  ;
                    worksheet.Cells[rowNumber, 7].Value =  item.Status;

                    worksheet.Cells[rowNumber, 8].Value = item.Month;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/BlockDocumentExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"BlockBranchPermission-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}"
                    .ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));


                return JsonSuccessResult(fileKey);
            }
           
        }
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> RecalculateBlockBranchPermission()
        {
            TerminalDetailsViewModel viewModel = new TerminalDetailsViewModel();
            var m1 = 8;
            var m2 = 9;

            var y1 = DateTime.Now.ToPersianYear();
            var y2 = y1;

            switch (DateTime.Now.ToPersianMonth())
            {
                case 1:
                    m1 = 11;
                    y1 = y1 - 1;
                    y2 = y2 - 1;
                    m2 = 12;
                    break;
                case 2:
                    m1 = 12;
                    y1 = y1 - 1;
                    m2 = 1;
                    break;
                default:
                    m1 = DateTime.Now.ToPersianMonth() - 2;
                    m2 = DateTime.Now.ToPersianMonth() - 1;
                    break;
            }

             
            var branchPermissions = _dataContext.Database.SqlQuery<BlockBranchPermission>(
                " GetTerminalReport @YearOne, @YearTwo,  @MonthOne,  @MonthTwo  ", new SqlParameter("YearOne", y1),
                new SqlParameter("YearTwo", y2),
                new SqlParameter("MonthOne", m1), new SqlParameter("MonthTwo", m2)).ToList();
            
            var branchTerminalCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwTerminalBranchCount").ToList();

            var vwWifiTerminalBranchCount = _dataContext.Database.SqlQuery<BranchTerminalCount>(
                "select  BranchId,TerminalCount from vwWifiTerminalBranchCount").ToList();
            var fx = branchPermissions.GroupBy(d => d.BranchId);

            var result = new List<BlockBranchPermissionReport>();
            foreach (var branch in fx)
            {
                var r = new BlockBranchPermissionReport();
                r.Title = branch.FirstOrDefault().Title;

                r.LowCount = 0;
                var terminals = branch.GroupBy(d => d.TerminalNo);
                foreach (var terminal in terminals)
                {
                    var terInM1 = terminal.FirstOrDefault(d => d.PersianLocalMonth == m1 && d.PersianLocalYear == y1);

                    if (!terminal.Any(d => d.Status == "High transaction") &&
                        !terminal.Any(d => d.Status == " Installed two months ago"))
                    {
                        r.LowCount = r.LowCount + 1;
                    }
                }

                r.TotalTerminalCount = branchTerminalCount.Where(a => a.BranchId == branch.FirstOrDefault().BranchId)
                    .FirstOrDefault().TerminalCount;


                //  r.TotalLowTransactionTerminalCount = branch.Where(d => d.Status == "Low transaction").Count();
                r.LowtoAllPercent = ((float) r.LowCount / (float) r.TotalTerminalCount) * 100;
                r.RoundLowtoAllPercent = r.LowtoAllPercent.ToString("F");
                r.WifiDeviceCount =
                    vwWifiTerminalBranchCount.Where(d => d.BranchId == branch.FirstOrDefault().BranchId)
                        .FirstOrDefault().TerminalCount;

                var branchId = branch.FirstOrDefault()?.BranchId;
                if (branchId != null) r.BranchId = (long) branchId;

                r.WifitoallDevicePercentage = ((float) r.WifiDeviceCount / (float) r.TotalTerminalCount) * 100;

                if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                    r.WifitoallDevicePercentage <= 30)
                {
                    r.Status = "مجاز به ثبت درخواست";
                    r.StatusId = 1;
                }
                else if (r.LowCount <= 10 && r.LowtoAllPercent <= 10 &&
                         r.WifitoallDevicePercentage > 30)
                {
                    r.Status = "مجاز به ثبت دستگاه ثابت";
                    r.StatusId = 2;
                }
                else
                {
                    r.Status = "غیر مجاز به ثبت درخواست";
                    r.StatusId = 3;
                }


                result.Add(r);
            }

 

            var rows = result.Select(r => new BranchPermission
            {
                Title = r.Title,
                TotalTerminalCount = r.TotalTerminalCount,
                TotalLowTransactionTerminalCount = r.TotalLowTransactionTerminalCount,
                LowtoAllPercent = ((float) Math.Round(r.LowtoAllPercent * 100f) / 100f).ToString() + '%',
                WifiDeviceCount = r.WifiDeviceCount,

                WifitoallDevicePercentage =
                    ((float) Math.Round(r.WifitoallDevicePercentage * 100f) / 100f).ToString() + '%',
                StatusId = r.StatusId,
                Status = r.Status,
                LowCount = r.LowCount,
                BranchId = r.BranchId
            }).ToList();



            _dataContext.BranchPermission.RemoveRange(_dataContext.BranchPermission.ToList());
            _dataContext.BranchPermission.AddRange(rows);

            _dataContext.SaveChangesAsync();
        
            return   Json( null, JsonRequestBehavior.AllowGet);
        }

        private long? GetBlockDocumentStatusIdFromText(string input)
        {
            input = input.Trim().ApplyPersianYeKe();

            switch (input)
            {
                case "ثبت شده":
                    return (long) Enums.BlockDocumentStatus.Registered;
                case "ثبت نشده":
                    return (long) Enums.BlockDocumentStatus.NotRegistered;
                case "در انتظار پایش دوره ای":
                    return (long) Enums.BlockDocumentStatus.WaitingForPeriodicMonitoring;
                case "در انتظار بررسی":
                    return (long) Enums.BlockDocumentStatus.WaitingForReview;
                default:
                    return null;
            }
        }
    }
}