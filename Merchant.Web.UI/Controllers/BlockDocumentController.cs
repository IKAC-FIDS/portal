using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
    public class BlockDocumentController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public BlockDocumentController(AppDataContext dataContext)
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

            ViewBag.TerminalStatusList = (await _dataContext.TerminalStatus
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

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            // var message = _dataContext.Messages.ToList();
            // ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
            //                                          && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            // ViewBag.InProgressMessage = message.Count(d => d.StatusId == (int) Enums.MessageStatus.UnderReview
            //                                                && (d.UserId == CurrentUserId ||
            //                                                    d.ReviewerUserId == CurrentUserId
            //                                                    || User.IsMessageManagerUser()));
            // var cardmessage = _dataContext.CardRequest.ToList();
            // ViewBag.ReadyForDeliverCardRequst = cardmessage.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.CardRequestStatus.ReadyForDeliver
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsCardRequestManager()));
            // ViewBag.InProgressCardRequstMessage = cardmessage.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsCardRequestManager()));
            return View();
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(BlockDocumentSearchViewModel viewModel, bool retriveTotalPageCount,
            int page, CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals
                .Where(x => x.DeviceType.IsWireless && x.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                            x.StatusId != (byte) Enums.TerminalStatus.Revoked);

            if (User.IsTehranBranchManagementUser() || User.IsCountyBranchManagementUser())
            {
                // اگر نقش کاربر اداره امور شعب تهران بود فقط شعبه های تهران را ببیند
                if (User.IsTehranBranchManagementUser())
                    query = query.Where(x => x.CityId == (long) Enums.City.Tehran);

                // اگر نقش کاربر اداره امور شعب شهرستان بود تمامی شعب غیر از تهران را ببیند
                if (User.IsCountyBranchManagementUser())
                    query = query.Where(x => x.CityId != (long) Enums.City.Tehran);
            }
            else
            {
                if (User.IsBranchUser()) // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
                    query = query.Where(x => x.BranchId == CurrentUserBranchId);

                if (User.IsSupervisionUser() && CurrentUserBranchId.HasValue
                ) // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
                {
                    var childOrganizationUnitIdList =
                        await _dataContext.GetChildOrganizationUnits(CurrentUserBranchId.Value);
                    query = query.Where(x => childOrganizationUnitIdList.Contains(x.BranchId));
                }
            }

            if (viewModel.BranchId.HasValue)
            {
                var childOrganizationUnitIdList =
                    await _dataContext.GetChildOrganizationUnits(viewModel.BranchId.Value);
                query = query.Where(x => childOrganizationUnitIdList.Contains(x.BranchId));
            }

            if (viewModel.TerminalStatusIdList != null && viewModel.TerminalStatusIdList.Any())
            {
                query = query.Where(x => viewModel.TerminalStatusIdList.Contains(x.StatusId));
            }

            if (viewModel.BlockDocumentStatusId.HasValue)
            {
                query = query.Where(x => x.BlockDocumentStatusId == viewModel.BlockDocumentStatusId);
            }

            if (!string.IsNullOrEmpty(viewModel.TerminalNo))
            {
                query = query.Where(x => x.TerminalNo == viewModel.TerminalNo);
            }

            if (viewModel.TerminalId.HasValue)
            {
                query = query.Where(x => x.Id == viewModel.TerminalId);
            }

            if (!string.IsNullOrEmpty(viewModel.NationalCode))
            {
                query = query.Where(x => x.MerchantProfile.NationalCode == viewModel.NationalCode);
            }

            if (viewModel.PspId.HasValue)
            {
                query = query.Where(x => x.PspId == viewModel.PspId);
            }

            if (!string.IsNullOrEmpty(viewModel.Title))
            {
                query = query.Where(x => x.Title.Contains(viewModel.Title));
            }

            if (!string.IsNullOrEmpty(viewModel.CustomerNumber))
            {
                query = query.Where(x => x.MerchantProfile.CustomerNumber.Contains(viewModel.CustomerNumber));
            }

            if (viewModel.MarketerId.HasValue)
            {
                query = query.Where(x => x.MarketerId == viewModel.MarketerId);
            }

            if (!string.IsNullOrEmpty(viewModel.FullName))
            {
                query = query.Where(x =>
                    x.MerchantProfile.FirstName.Contains(viewModel.FullName) ||
                    x.MerchantProfile.LastName.Contains(viewModel.TerminalNo));
            }

            if (viewModel.DeviceTypeId.HasValue)
            {
                query = query.Where(x => x.DeviceTypeId == viewModel.DeviceTypeId);
            }

            if (viewModel.FromBlockDocumentDate.HasValue)
            {
                query = query.Where(x => x.BlockDocumentDate >= viewModel.FromBlockDocumentDate);
            }

            if (viewModel.ToBlockDocumentDate.HasValue)
            {
                query = query.Where(x => x.BlockDocumentDate <= viewModel.ToBlockDocumentDate);
            }

            var rows = await query
                .Select(x => new
                {
                    x.Id,
                    x.PspId,
                    x.StatusId,
                    x.AccountNo,
                    x.BlockPrice,
                    x.TerminalNo,
                    x.BlockDocumentDate,
                    x.BlockAccountNumber,
                    x.BlockDocumentNumber,
                    PspTitle = x.Psp.Title,
                    x.BlockDocumentStatusId,
                    DeviceTypeTitle = x.DeviceType.Title,
                    TerminalStatusTitle = x.Status.Title,
                    BlockDocumentStatusTitle = x.BlockDocumentStatus.Title,
                    FullName = x.MerchantProfile.FirstName + " " + x.MerchantProfile.LastName
                })
                .OrderBy(x => x.Id)
                .Skip(page - 1)
                .Take(300)
                .ToListAsync(cancellationToken);

            var totalRowsCount = 0;
            if (retriveTotalPageCount)
            {
                totalRowsCount = await query.CountAsync(cancellationToken);
            }

            return JsonSuccessResult(new {rows, totalRowsCount});
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser,DefaultRoles.BlockDocumentChanger,
            DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchManagment)]
        public async Task<ActionResult> Edit(long terminalId, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.Terminals
                .Where(x => x.Id == terminalId)
                .Select(x => new BlockDocumentEditViewModel
                {
                    BlockAccountNumber = x.BlockAccountNumber,
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    BlockDocumentStatusId = x.BlockDocumentStatusId,
                    BlockPrice = x.BlockPrice,
                    PreferredPspId = x.PreferredPspId,
                    TerminalId = x.Id,
                    TerminalNo = x.TerminalNo,
                    BlockDocumentId = x.TerminalDocuments
                        .Where(y => y.DocumentTypeId == (long) Enums.DocumentType.SanadMasdoodi)
                        .Select(y => (long?) y.Id).FirstOrDefault()
                })
                .FirstAsync(cancellationToken);

            if (!string.IsNullOrEmpty(viewModel.BlockAccountNumber))
            {
                viewModel.AccountBranchCode = viewModel.BlockAccountNumber.Split('-')[0];
                viewModel.AccountType = viewModel.BlockAccountNumber.Split('-')[1];
                viewModel.AccountCustomerNumber = viewModel.BlockAccountNumber.Split('-')[2];
                viewModel.AccountRow = viewModel.BlockAccountNumber.Split('-')[3];
            }


            if (User.IsBranchUser())
            {
                var listAsync = (await _dataContext.BlockDocumentStatuses
                    .Where(b => b.Id != 1)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken));

                ViewBag.BlockDocumentStatusList = listAsync
                    .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.BlockDocumentStatusId});
            }
            else
            {
                var listAsync = (await _dataContext.BlockDocumentStatuses
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken));

                ViewBag.BlockDocumentStatusList = listAsync
                    .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.BlockDocumentStatusId});
            }

            return PartialView("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.BlockDocumentChanger, DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchManagment)]
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

            if (_dataContext.Terminals.Any(b =>
                
                    b.StatusId != (byte)Enums.TerminalStatus.Deleted && b.Id != viewModel.TerminalId &&
                b.BlockDocumentStatusId == (byte) Enums.BlockDocumentStatus.Registered &&
                b.BlockDocumentNumber == viewModel.BlockDocumentNumber))
            {
                return JsonErrorMessage(" شماره مسدودی وارد شده تکراری می باشد          .");
            }

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

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.Administrator,
            DefaultRoles.BranchManagment)]
        public async Task<ActionResult> Export(BlockDocumentSearchViewModel viewModel)
        {
            var query = _dataContext.Terminals
                .Where(x => x.DeviceType.IsWireless && x.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                            x.StatusId != (byte) Enums.TerminalStatus.Revoked);

            if (viewModel.TerminalStatusIdList != null && viewModel.TerminalStatusIdList.Any())
            {
                query = query.Where(x => viewModel.TerminalStatusIdList.Contains(x.StatusId));
            }

            if (viewModel.BlockDocumentStatusId.HasValue)
            {
                query = query.Where(x => x.BlockDocumentStatusId == viewModel.BlockDocumentStatusId);
            }

            if (!string.IsNullOrEmpty(viewModel.TerminalNo))
            {
                query = query.Where(x => x.TerminalNo == viewModel.TerminalNo);
            }

            if (viewModel.TerminalId.HasValue)
            {
                query = query.Where(x => x.Id == viewModel.TerminalId);
            }

            if (!string.IsNullOrEmpty(viewModel.NationalCode))
            {
                query = query.Where(x => x.MerchantProfile.NationalCode == viewModel.NationalCode);
            }

            if (viewModel.PspId.HasValue)
            {
                query = query.Where(x => x.PspId == viewModel.PspId);
            }

            if (!string.IsNullOrEmpty(viewModel.Title))
            {
                query = query.Where(x => x.Title.Contains(viewModel.Title));
            }

            if (viewModel.BranchId.HasValue)
            {
                query = query.Where(x => x.BranchId == viewModel.BranchId);
            }

            if (!string.IsNullOrEmpty(viewModel.CustomerNumber))
            {
                query = query.Where(x => x.MerchantProfile.CustomerNumber.Contains(viewModel.CustomerNumber));
            }

            if (viewModel.MarketerId.HasValue)
            {
                query = query.Where(x => x.MarketerId == viewModel.MarketerId);
            }

            if (!string.IsNullOrEmpty(viewModel.FullName))
            {
                query = query.Where(x =>
                    x.MerchantProfile.FirstName.Contains(viewModel.FullName) ||
                    x.MerchantProfile.LastName.Contains(viewModel.TerminalNo));
            }

            if (viewModel.DeviceTypeId.HasValue)
            {
                query = query.Where(x => x.DeviceTypeId == viewModel.DeviceTypeId);
            }

            if (viewModel.FromBlockDocumentDate.HasValue)
            {
                query = query.Where(x => x.BlockDocumentDate >= viewModel.FromBlockDocumentDate);
            }

            if (viewModel.ToBlockDocumentDate.HasValue)
            {
                query = query.Where(x => x.BlockDocumentDate <= viewModel.ToBlockDocumentDate);
            }

            var data = await query
                .Select(x => new
                {
                    x.Id,
                    x.BlockDocumentStatusId,
                    BlockDocumentStatusTitle = x.BlockDocumentStatus.Title,
                    x.BlockDocumentNumber,
                    x.BlockDocumentDate,
                    x.BlockAccountNumber,
                    x.BlockPrice,
                    x.TerminalNo,
                    x.StatusId,
                    TerminalStatusTitle = x.Status.Title,
                    x.AccountNo,
                    x.PspId,
                    PspTitle = x.Psp.Title,
                    x.MerchantProfile.CustomerNumber,
                    x.Title,
                    x.MerchantProfile.FirstName,
                    x.MerchantProfile.LastName,
                    x.MerchantProfile.NationalCode,
                    x.BranchId,
                    BranchTitle = x.Branch.Title,
                    DeviceTypeTitle = x.DeviceType.Title,
                    MarketerTitle = x.Marketer.Title,
                    x.ContractNo
                })
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("اسناد مسدودی دستگاه های سیار");

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
                worksheet.Column(8).Width = 16;
                worksheet.Column(9).Width = 16;
                worksheet.Column(10).Width = 28;
                worksheet.Column(11).Width = 14;
                worksheet.Column(12).Width = 13;
                worksheet.Column(13).Width = 27;
                worksheet.Column(14).Width = 26;
                worksheet.Column(15).Width = 32;
                worksheet.Column(16).Width = 22;
                worksheet.Column(17).Width = 26;
                worksheet.Column(18).Width = 14;
                worksheet.Column(19).Width = 14;
                worksheet.Column(20).Width = 21;

                worksheet.Cells[1, 1].Value = "شماره پیگیری";
                worksheet.Cells[1, 2].Value = "شماره پایانه";
                worksheet.Cells[1, 3].Value = "وضعیت پایانه";
                worksheet.Cells[1, 4].Value = "تاریخ سند مسدودی";
                worksheet.Cells[1, 5].Value = "شماره سند مسدودی";
                worksheet.Cells[1, 6].Value = "شماره حساب مسدودی";
                worksheet.Cells[1, 7].Value = "مبلغ مسدودی)";
                worksheet.Cells[1, 8].Value = "وضعیت سند مسدودی";
                worksheet.Cells[1, 9].Value = "شماره مشتری";
                worksheet.Cells[1, 10].Value = "نام فروشگاه";
                worksheet.Cells[1, 11].Value = "نام";
                worksheet.Cells[1, 12].Value = "نام خانوادگی";
                worksheet.Cells[1, 13].Value = "کد ملی";
                worksheet.Cells[1, 14].Value = "شرکت PSP";
                worksheet.Cells[1, 15].Value = "کد شعبه";
                worksheet.Cells[1, 16].Value = "نام شعبه";
                worksheet.Cells[1, 17].Value = "شماره حساب";
                worksheet.Cells[1, 18].Value = "نوع دستگاه درخواستی";
                worksheet.Cells[1, 19].Value = "بازاریابی توسط";
                worksheet.Cells[1, 20].Value = "شماره قرارداد";

                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Id;
                    worksheet.Cells[rowNumber, 2].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 3].Value = item.TerminalStatusTitle;
                    worksheet.Cells[rowNumber, 4].Value = item.BlockDocumentDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.BlockDocumentNumber;
                    worksheet.Cells[rowNumber, 6].Value = item.BlockAccountNumber;
                    worksheet.Cells[rowNumber, 7].Value = item.BlockPrice;
                    worksheet.Cells[rowNumber, 8].Value = item.BlockDocumentStatusTitle;
                    worksheet.Cells[rowNumber, 9].Value = item.CustomerNumber;
                    worksheet.Cells[rowNumber, 10].Value = item.Title;
                    worksheet.Cells[rowNumber, 11].Value = item.FirstName;
                    worksheet.Cells[rowNumber, 12].Value = item.LastName;
                    worksheet.Cells[rowNumber, 13].Value = item.NationalCode;
                    worksheet.Cells[rowNumber, 14].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 15].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 16].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 17].Value = item.AccountNo;
                    worksheet.Cells[rowNumber, 18].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 19].Value = item.MarketerTitle;
                    worksheet.Cells[rowNumber, 20].Value = item.ContractNo;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/BlockDocumentExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"BlockDocuments-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
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