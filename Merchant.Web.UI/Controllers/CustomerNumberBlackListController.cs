using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels.CustomerNumberBlackList;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class CustomerNumberBlackListController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public CustomerNumberBlackListController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult Manage()
        {
            // var message = _dataContext.Messages.ToList();
            // ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
            //                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                             || User.IsMessageManagerUser()));
            //
            // ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.UnderReview  
            //                                               && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                   || User.IsMessageManagerUser()));
            //      var cardmessage = _dataContext.CardRequest.ToList();
            //             ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
            //                                                                   && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                                       || User.IsCardRequestManager())); 
            //             ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
            //                                                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
            //                                                                             || User.IsCardRequestManager()));
            return View();
            
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GetData(string customerNumber, bool retriveTotalPageCount, int page, CancellationToken cancellationToken)
        {
            var query = _dataContext.CustomerNumberBlackLists.AsQueryable();

            if (!string.IsNullOrEmpty(customerNumber))
            {
                query = query.Where(x => x.CustomerNumber == customerNumber);
            }

            var rows = await query
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.SubmitTime,
                    x.Description,
                    x.CustomerNumber,
                    SubmitterUserFullName = x.SubmitterUser.FullName
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

            return JsonSuccessResult(new { rows, totalRowsCount });
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult Create() => View("_Create");

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(CreateCustomerNumberBlackListModel model, CancellationToken cancellationToken)
        {
            if (_dataContext.CustomerNumberBlackLists.Any(x => x.CustomerNumber == model.CustomerNumber))
            {
                return JsonWarningMessage("این شماره مشتری از قبل اضافه شده است");
            }

            _dataContext.CustomerNumberBlackLists.Add(new CustomerNumberBlackList
            {
                SubmitTime = DateTime.Now,
                Description = model.Description,
                SubmitterUserId = CurrentUserId,
                CustomerNumber = model.CustomerNumber
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Delete(List<int> idList, CancellationToken cancellationToken)
        {
            _dataContext.CustomerNumberBlackLists.RemoveRange(_dataContext.CustomerNumberBlackLists.Where(x => idList.Contains(x.Id)));
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Export(CancellationToken cancellationToken)
        {
            var data = await _dataContext.CustomerNumberBlackLists
                .Select(x => new
                {
                    x.SubmitTime,
                    x.Description,
                    x.CustomerNumber,
                    SubmitterUserFullName = x.SubmitterUser.FullName
                })
                .ToListAsync(cancellationToken);

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("لیست سیاه مشتریان");
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
                worksheet.Column(2).Width = 26;
                worksheet.Column(3).Width = 36;
                worksheet.Column(4).Width = 100;

                worksheet.Cells[1, 1].Value = "شماره مشتری";
                worksheet.Cells[1, 2].Value = "تاریخ ثبت";
                worksheet.Cells[1, 3].Value = "کاربر ثبت کننده";
                worksheet.Cells[1, 4].Value = "توضیحات";
                

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.CustomerNumber;
                    worksheet.Cells[rowNumber, 2].Value = item.SubmitTime.ToPersianDateTime();
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitterUserFullName;
                    worksheet.Cells[rowNumber, 4].Value = item.Description;

                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CustomerNumberBlackList.xlsx");
                }
            }
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult Import() => View();

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Import(HttpPostedFileBase file, CancellationToken cancellationToken)
        {
            if (!file.IsValidFormat(".xlsx"))
            {
                return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
            }

            var now = DateTime.Now;
            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("CustomerNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Description", typeof(string)));
            dataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            dataTable.Columns.Add(new DataColumn("SubmitterUserId", typeof(long)));

            var currentCustomerNumbers = await _dataContext.CustomerNumberBlackLists.Select(x => x.CustomerNumber).ToListAsync(cancellationToken);
            var errorMessageList = new List<string>();

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var customerNumber = row[rowNumber, 1].Text;

                        if (string.IsNullOrEmpty(customerNumber) || !customerNumber.IsItNumber() || customerNumber.Length != 8)
                        {
                            errorMessageList.Add($"سطر {rowNumber} - شماره مشتری وارد شده صحیح نمی باشد");
                            continue;
                        }

                        if (currentCustomerNumbers.Contains(customerNumber))
                        {
                            continue;
                        }

                        var dataRow = dataTable.NewRow();
                        dataRow["CustomerNumber"] = customerNumber;
                        dataRow["Description"] = row[rowNumber, 2].Text;
                        dataRow["SubmitTime"] = now;
                        dataRow["SubmitterUserId"] = CurrentUserId;

                        dataTable.Rows.Add(dataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            if (errorMessageList.Any())
            {
                return JsonWarningMessage("لطفاً خطاهای اعلام شده را بررسی نموده و مجدداً فایل را بارگذاری نمایید.", data: errorMessageList);
            }

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
            {
                await sqlConnection.OpenAsync(cancellationToken);

                using (var transaction = sqlConnection.BeginTransaction())
                {
                    using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CustomerNumber", "CustomerNumber"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Description", "Description"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitterUserId", "SubmitterUserId"));
                        sqlBulkCopy.BatchSize = 5000;
                        sqlBulkCopy.BulkCopyTimeout = 10000;
                        sqlBulkCopy.DestinationTableName = $"[{_dataContext.Database.Connection.Database}].[psp].[CustomerNumberBlackList]";

                        try
                        {
                            await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                        }
                        catch
                        {
                            transaction.Rollback();
                        }
                    }

                    transaction.Commit();
                }
            }

            return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات لیست سیاه مشتریان با موفقیت انجام شد.");
        }
    }
}