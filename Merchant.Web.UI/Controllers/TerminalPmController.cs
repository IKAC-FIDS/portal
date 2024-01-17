using OfficeOpenXml;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TerminalPmController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public TerminalPmController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        [HttpPost]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(GetTerminalPmDataViewModel viewModle)
        {
            // var (item1, item2) = await _dataContext.GetTerminalPmData(
            //     viewModle.PspId, 
            //     viewModle.Year, 
            //     viewModle.Month, 
            //     viewModle.RetriveTotalPageCount, 
            //     User.Identity.GetBranchId(), 
            //     User.IsBranchUser(), 
            //     User.IsSupervisionUser(), 
            //     User.IsTehranBranchManagementUser(), 
            //     User.IsCountyBranchManagementUser(), 
            //     viewModle.Page - 1, 300);
            //
            // var data = item1
            //     .Select(x => new
            //     {
            //         x.PspId,
            //         x.PspTitle,
            //         x.StatusId,
            //         x.TerminalNo,
            //         x.IsWireless,
            //         x.StatusTitle,
            //         x.DeviceTypeTitle,
            //         PmTime = x.PmTime.ToPersianDate()
            //     });
            var terminals = _dataContext.Terminals.Where(a => a.BranchId == CurrentUserBranchId).Select(b => b.TerminalNo).ToList();
            var report7Data =   _dataContext.TempReport7Datas.Where(x => x.Month ==viewModle.Month   && x.Year == viewModle.Year
                    && terminals.Contains(x.TerminalNo))
                .ToList();
            return JsonSuccessResult(new { rows = report7Data, totalRowsCount = report7Data.Count });
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Import(HttpPostedFileBase file, CancellationToken cancellationToken)
        {
            if (!file.IsValidFormat(".xlsx"))
            {
                AddDangerMessage("تنها فایل با پسوند .xlsx مجاز می‌باشد.");
              
                                                                  
                return View();
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            dataTable.Columns.Add(new DataColumn("PmTime", typeof(DateTime)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var dataRow = dataTable.NewRow();
                        dataRow["TerminalNo"] = row[rowNumber, 1].Text;
                        dataRow["PmTime"] = row[rowNumber, 2].Text.ToMiladiDate();
                        dataTable.Rows.Add(dataRow);
                    }
                    catch
                    {
                        AddDangerMessage($"خطا در اطلاعات سطر {rowNumber}");
                        var message = _dataContext.Messages.ToList();
                        ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
                                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                                    || User.IsMessageManagerUser()));
                        return View();
                    }
                }
            }

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
            {
                await sqlConnection.OpenAsync(cancellationToken);

                using (var transaction = sqlConnection.BeginTransaction())
                {
                    using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PmTime", "PmTime"));
                        sqlBulkCopy.BatchSize = 5000;
                        sqlBulkCopy.BulkCopyTimeout = 10000;
                        sqlBulkCopy.DestinationTableName = $"[{_dataContext.Database.Connection.Database}].[psp].[TerminalPm]";

                        try
                        {
                            await sqlBulkCopy.WriteToServerAsync(dataTable, cancellationToken);
                        }
                        catch
                        {
                            transaction.Rollback();

                            AddDangerMessage("خطا در ورود اطلاعات.");
                            

                            return View();
                        }
                    }

                    transaction.Commit();
                }
            }

            AddSuccessMessage("فرآیند وارد نمودن اطلاعات PM با موفقیت انجام شد.");

            return RedirectToAction("Manage", "Terminal");
        }
    }
}