using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public TransactionController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public ActionResult Index(string terminalNo)
        {
            ViewBag.TerminalNo = terminalNo;

            return PartialView("_Index");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(GetTransactionDataViewModel viewModel,
            CancellationToken cancellationToken)
        {
            // TODO Security :(
            var query = _dataContext.TransactionSums.Where(x => x.TerminalNo == viewModel.TerminalNo);

            var totalRowsCount = 0;

            if (viewModel.RetriveTotalPageCount)
            {
                totalRowsCount = await query.CountAsync(cancellationToken);
            }

            var rows = await query
                .OrderByDescending(x => x.PersianLocalYearMonth)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .Select(x => new
                {
                    x.SumPrice,
                    x.TotalCount,
                    x.PersianLocalYear,
                    x.PersianLocalMonth
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(new {rows, totalRowsCount});
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Import(HttpPostedFileBase file, CancellationToken cancellationToken)
        {
            if (!file.IsValidFormat(".xlsx"))
            {
                AddDangerMessage("تنها فایل با پسوند .xlsx مجاز می‌باشد.");
                return View();
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            dataTable.Columns.Add(new DataColumn("BuyTransactionCount", typeof(int)));
            dataTable.Columns.Add(new DataColumn("BuyTransactionAmount", typeof(long)));
            dataTable.Columns.Add(new DataColumn("BillTransactionCount", typeof(int)));
            dataTable.Columns.Add(new DataColumn("BillTransactionAmount", typeof(long)));
            dataTable.Columns.Add(new DataColumn("ChargeTransactionCount", typeof(int)));
            dataTable.Columns.Add(new DataColumn("ChargeTransactionAmount", typeof(long)));
            dataTable.Columns.Add(new DataColumn("BalanceCount", typeof(int)));
            dataTable.Columns.Add(new DataColumn("PersianLocalMonth", typeof(int)));
            dataTable.Columns.Add(new DataColumn("PersianLocalYear", typeof(int)));

            var persianLocalYearMonths = new List<string>();
            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];
                        var month = Convert.ToInt32(row[rowNumber, 9].Text);
                        var year = Convert.ToInt32(row[rowNumber, 10].Text);

                        var dataRow = dataTable.NewRow();
                        dataRow["TerminalNo"] = row[rowNumber, 1].Text;
                        dataRow["BuyTransactionCount"] = Convert.ToInt32(row[rowNumber, 2].Text);
                        dataRow["BuyTransactionAmount"] = Convert.ToInt64(row[rowNumber, 3].Text);
                        dataRow["BillTransactionCount"] = Convert.ToInt32(row[rowNumber, 4].Text);
                        dataRow["BillTransactionAmount"] = Convert.ToInt64(row[rowNumber, 5].Text);
                        dataRow["ChargeTransactionCount"] = Convert.ToInt32(row[rowNumber, 6].Text);
                        dataRow["ChargeTransactionAmount"] = Convert.ToInt64(row[rowNumber, 7].Text);
                        dataRow["BalanceCount"] = Convert.ToInt64(row[rowNumber, 8].Text);
                        dataRow["PersianLocalMonth"] = Convert.ToInt32(row[rowNumber, 9].Text);
                        dataRow["PersianLocalYear"] = Convert.ToInt32(row[rowNumber, 10].Text);
                        dataTable.Rows.Add(dataRow);
                        persianLocalYearMonths.Add(year + month.ToString("00"));
                    }
                    catch
                    {
                        AddDangerMessage($"خطا در اطلاعات سطر {rowNumber}");
                        return View();
                    }
                }
            }

            using (var sqlConnection =
                   new SqlConnection(ConfigurationManager.ConnectionStrings["AppDataContext"].ConnectionString))
            {
                await sqlConnection.OpenAsync(cancellationToken);

                using (var transaction = sqlConnection.BeginTransaction())
                {
                    var sqlCommand =
                        new SqlCommand(
                            $"DELETE psp.TransactionSum WHERE PersianLocalYearMonth IN ({string.Join(",", persianLocalYearMonths.Distinct())});",
                            sqlConnection, transaction);

                    try
                    {
                        await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
                    }
                    catch
                    {
                        transaction.Rollback();
                    }

                    using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, transaction))
                    {
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BuyTransactionCount",
                            "BuyTransactionCount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BuyTransactionAmount",
                            "BuyTransactionAmount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BillTransactionCount",
                            "BillTransactionCount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BillTransactionAmount",
                            "BillTransactionAmount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ChargeTransactionCount",
                            "ChargeTransactionCount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ChargeTransactionAmount",
                            "ChargeTransactionAmount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BalanceCount", "BalanceCount"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PersianLocalMonth",
                            "PersianLocalMonth"));
                        sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("PersianLocalYear",
                            "PersianLocalYear"));
                        sqlBulkCopy.BatchSize = 5000;
                        sqlBulkCopy.BulkCopyTimeout = 10000;
                        sqlBulkCopy.DestinationTableName =
                            $"[{_dataContext.Database.Connection.Database}].[psp].[TransactionSum]";

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

            AddSuccessMessage("فرآیند وارد نمودن اطلاعات تراکنش های ماهانه با موفقیت انجام شد.");

            #region UpdateBranchPermession

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

            await _dataContext.SaveChangesAsync();

            #endregion

            return RedirectToAction("Manage", "Terminal");
        }
    }
}