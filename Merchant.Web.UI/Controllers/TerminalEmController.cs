using OfficeOpenXml;
using System;
using System.Configuration;
using System.Data;
using System.Data.Entity;
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
    public class TerminalEmController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public TerminalEmController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize]
        public ActionResult Index()
        {
            var message = _dataContext.Messages.ToList();
            ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
                                                    && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                        || User.IsMessageManagerUser()));
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(GetTerminalEmDataViewModel viewModel)
        {
            // var (item1, item2) = await _dataContext.GetTerminalEmData(viewModel.PspId,
            //     viewModel.Year, 
            //     viewModel.Month, 
            //     viewModel.RetriveTotalPageCount, 
            //     User.Identity.GetBranchId(), 
            //     User.IsBranchUser(), 
            //     User.IsSupervisionUser(), 
            //     User.IsTehranBranchManagementUser(), 
            //     User.IsCountyBranchManagementUser(), 
            //     viewModel.Page - 1, 
            //     300);
            //
            //
            var terminals = _dataContext.Terminals.Where(a => a.BranchId == CurrentUserBranchId).Select(b => b.TerminalNo).ToList();
            var data =
                (  _dataContext.TempReport6Datas.Where(x => x.Year == viewModel.Year && x.Month == 
                    viewModel.Month && terminals.Contains(x.TerminalNo)
                    ).ToList())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.Subject,
                    FirstOperationDate =   x.FirstOperationDate. ToPersianDate(),
                    LastOperationDate=  x.LastOperationDate.ToPersianDate(),
                    x.Type,
                    x.City,
                    x.Sla,
                    x.ValidDay,
                    x.InstallationDelay
                });

            return JsonSuccessResult(new { rows = data, totalRowsCount = data.Count() });
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
            dataTable.Columns.Add(new DataColumn("EmTime", typeof(DateTime)));
            dataTable.Columns.Add(new DataColumn("RequestEmTime", typeof(DateTime)));

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
                        dataRow["RequestEmTime"] = row[rowNumber, 2].Text.ToMiladiDate();
                        dataRow["EmTime"] = string.IsNullOrEmpty(row[rowNumber, 3].Text) ? (object)DBNull.Value : row[rowNumber, 3].Text.ToMiladiDate();
                        dataTable.Rows.Add(dataRow);
                    }
                    catch
                    {
                        AddDangerMessage($"خطا در اطلاعات سطر {rowNumber}");
                   
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
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("EmTime", "EmTime"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("RequestEmTime", "RequestEmTime"));

                        sqlBulkCopy.BatchSize = 5000;
                        sqlBulkCopy.BulkCopyTimeout = 10000;
                        sqlBulkCopy.DestinationTableName = $"[{_dataContext.Database.Connection.Database}].[psp].[TerminalEm]";

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

            AddSuccessMessage("فرآیند وارد نمودن اطلاعات EM با موفقیت انجام شد.");

            return RedirectToAction("Manage", "Terminal");
        }
    }
}