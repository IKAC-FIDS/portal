using Dapper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
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
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TempReportDataController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public TempReportDataController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public ActionResult Import()
        {
            
            return View();
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Import(TempReportDataImportViewModel viewModel,
            CancellationToken cancellationToken)
        {
            using (var transaction = _dataContext.Database.BeginTransaction())
            {
                if (viewModel.Report1And2File != null && viewModel.Report1And2File.IsValidFile() &&
                    viewModel.Report1And2File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport1And2Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData1And2(viewModel.Report1And2File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }

                if (viewModel.Report3File != null && viewModel.Report3File.IsValidFile() &&
                    viewModel.Report3File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport3Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData3(viewModel.Report3File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }

                if (viewModel.Report4File != null && viewModel.Report4File.IsValidFile() &&
                    viewModel.Report4File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport4Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData4(viewModel.Report4File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }

                if (viewModel.Report5File != null && viewModel.Report5File.IsValidFile() &&
                    viewModel.Report5File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport5Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData5(viewModel.Report5File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }

                if (viewModel.Report6File != null && viewModel.Report6File.IsValidFile() &&
                    viewModel.Report6File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport6Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData6(viewModel.Report6File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }

                if (viewModel.Report7File != null && viewModel.Report7File.IsValidFile() &&
                    viewModel.Report7File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport7Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData7(viewModel.Report7File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }
                
                if (viewModel.Report8File != null && viewModel.Report8File.IsValidFile() &&
                    viewModel.Report8File.IsValidFormat(".xls,.xlsx"))
                {
                    await _dataContext.Database.Connection.ExecuteAsync(
                        "DELETE FROM TempReport8Data WHERE Year = @year AND Month = @month",
                        new {viewModel.Year, viewModel.Month}, transaction.UnderlyingTransaction);
                    await UploadTempReportData8(viewModel.Report8File, viewModel.Month, viewModel.Year,
                        _dataContext.Database.Connection, transaction, cancellationToken);
                }
            }

           
            return View();
        }

        private async Task UploadTempReportData1And2(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("IsWireless", typeof(bool)));
            //tempReportDataTable.Columns.Add(new DataColumn("StatusId", typeof(byte)));
            // tempReportDataTable.Columns.Add(new DataColumn("Statuses", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("BatchDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("InstallationDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("RevokeDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        tempReportDataRow["IsWireless"] = GetIsWirelessFromText(row[rowNumber, 2].Text);
                        //  tempReportDataRow["StatusId"] = GetStatusIdFromText(row[rowNumber, 3].Text);
                        //  tempReportDataRow["Statuses"] = row[rowNumber, 3].Text;

                        tempReportDataRow["SubmitTime"] = row[rowNumber, 4].Text.ToMiladiDate();
                        tempReportDataRow["BatchDate"] =
                            row[rowNumber, 5].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["InstallationDate"] =
                            row[rowNumber, 6].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["RevokeDate"] =
                            row[rowNumber, 7].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsWireless", "IsWireless"));
                //     sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("StatusId", "StatusId"));
                //    sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Statuses", "Statuses"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BatchDate", "BatchDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDate", "InstallationDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("RevokeDate", "RevokeDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport1And2Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UploadTempReportData3(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("BranchCode", typeof(short)));
            tempReportDataTable.Columns.Add(new DataColumn("BranchTitle", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text;
                        tempReportDataRow["BranchCode"] = row[rowNumber, 2].Text;
                        tempReportDataRow["BranchTitle"] = row[rowNumber, 3].Text;
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchCode", "BranchCode"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BranchTitle", "BranchTitle"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport3Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UploadTempReportData4(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("IsWireless", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("BatchDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("InstallationDelay", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text;
                        tempReportDataRow["IsWireless"] = GetIsWirelessFromText(row[rowNumber, 2].Text);
                        tempReportDataRow["SubmitTime"] = row[rowNumber, 3].Text.ToMiladiDate();
                        tempReportDataRow["BatchDate"] =
                            row[rowNumber, 4].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["InstallationDelay"] = row[rowNumber, 5].Text;
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsWireless", "IsWireless"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BatchDate", "BatchDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDelay", "InstallationDelay"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport4Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UploadTempReportData5(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("IsWireless", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("SubmitTime", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("BatchDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("InstallationDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("InstallationDelay", typeof(short)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        tempReportDataRow["IsWireless"] = GetIsWirelessFromText(row[rowNumber, 2].Text);
                        tempReportDataRow["SubmitTime"] = row[rowNumber, 3].Text.ToMiladiDate();
                        tempReportDataRow["BatchDate"] =
                            row[rowNumber, 4].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["InstallationDate"] =
                            row[rowNumber, 5].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["InstallationDelay"] = row[rowNumber, 6].Text;
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsWireless", "IsWireless"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("SubmitTime", "SubmitTime"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("BatchDate", "BatchDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDate", "InstallationDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDelay", "InstallationDelay"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport5Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UploadTempReportData6(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("Subject", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("FirstOperationDate", typeof(DateTime)));
            tempReportDataTable.Columns.Add(new DataColumn("LastOperationDate", typeof(DateTime)));
            
            tempReportDataTable.Columns.Add(new DataColumn("Type", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("City", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("Sla", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("ValidDay",typeof(string)));

            
            tempReportDataTable.Columns.Add(new DataColumn("InstallationDelay", typeof(short)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        tempReportDataRow["Subject"] = row[rowNumber, 2].Text;
                        tempReportDataRow["FirstOperationDate"] =
                            row[rowNumber, 3].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        tempReportDataRow["LastOperationDate"] =
                            row[rowNumber, 4].Text.ToNullableMiladiDate() ?? (object) DBNull.Value;
                        
                         
                        tempReportDataRow["Type"] = row[rowNumber, 5].Text;
                        tempReportDataRow["City"] = row[rowNumber, 6].Text;
                        tempReportDataRow["Sla"] = row[rowNumber, 7].Text;
                        tempReportDataRow["ValidDay"] = row[rowNumber, 8].Text;
                        
                        tempReportDataRow["InstallationDelay"] = row[rowNumber, 9].Text;
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                        catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Subject", "Subject"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FirstOperationDate",
                    "FirstOperationDate"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("LastOperationDate", "LastOperationDate"));
                
                
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Type", "Type"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("City", "City"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Sla", "Sla"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ValidDay", "ValidDay"));

                
                
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("InstallationDelay", "InstallationDelay"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport6Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
        }

        private async Task UploadTempReportData7(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("IsWireless", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("IsPm", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        tempReportDataRow["IsWireless"] = GetIsWirelessFromText(row[rowNumber, 2].Text);
                        tempReportDataRow["IsPm"] = GetIsPmFromText(row[rowNumber, 3].Text);
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsWireless", "IsWireless"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsPm", "IsPm"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport7Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }
        
             private async Task UploadTempReportData8(HttpPostedFileBase file, byte month, short year,
            IDbConnection sqlConnection, DbContextTransaction transaction, CancellationToken cancellationToken)
        {
            var tempReportDataTable = new DataTable();
            tempReportDataTable.Columns.Add(new DataColumn("TerminalNo", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("CurrentMonth", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("FirstRequest", typeof(string)));
            tempReportDataTable.Columns.Add(new DataColumn("IsWireless", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("IsGoodValue", typeof(double)));
            tempReportDataTable.Columns.Add(new DataColumn("IsGood", typeof(bool)));
            tempReportDataTable.Columns.Add(new DataColumn("Month", typeof(byte)));
            tempReportDataTable.Columns.Add(new DataColumn("Year", typeof(short)));

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();
                var totalNumberOfRowsWithoutHeader = workSheet.Dimension.End.Row - 1;

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var tempReportDataRow = tempReportDataTable.NewRow();
                        tempReportDataRow["TerminalNo"] = row[rowNumber, 1].Text.ApplyPersianYeKe();
                        
                        tempReportDataRow["CurrentMonth"] = row[rowNumber, 2].Text;
                        tempReportDataRow["FirstRequest"] = row[rowNumber,3].Text;

                        tempReportDataRow["IsWireless"] = GetIsWirelessFromText(row[rowNumber, 4].Text);
                        
                        tempReportDataRow["IsGoodValue"] = row[rowNumber,5].Text;
                        tempReportDataRow["IsGood"] = row[rowNumber,6].Text != "0";

                    
                        tempReportDataRow["Month"] = month;
                        tempReportDataRow["Year"] = year;

                        tempReportDataTable.Rows.Add(tempReportDataRow);
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }
            }

            using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection as SqlConnection, SqlBulkCopyOptions.Default,
                transaction.UnderlyingTransaction as SqlTransaction))
            {
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TerminalNo", "TerminalNo"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("CurrentMonth", "CurrentMonth"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("FirstRequest", "FirstRequest"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsGoodValue", "GoodValue"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsGood", "IsGood"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("IsWireless", "IsWireless"));

                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Month", "Month"));
                sqlBulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping("Year", "Year"));

                sqlBulkCopy.BatchSize = 5000;
                sqlBulkCopy.BulkCopyTimeout = 10000;
                sqlBulkCopy.DestinationTableName =
                    $"[{_dataContext.Database.Connection.Database}].[dbo].[TempReport8Data]";

                try
                {
                    await sqlBulkCopy.WriteToServerAsync(tempReportDataTable, cancellationToken);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }
        }


        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData1(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var data = (_dataContext.TempReport1And2Datas
                    .Where(x => x.Year == year && x.Month == month && !x.IsWireless).ToList())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.IsWireless,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDate,
                    x.RevokeDate
                }).ToList();
            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";

                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "تاریخ ابطال";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";

                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.RevokeDate.ToPersianDate();


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report1-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData1(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            var data  = _dataContext.Terminals.Where(b => b.MarketerId == (byte) Enums.Marketer.BankOrBranch
                                                         && b.InstallationDate <= reviewMonthTo &&
                                                         b.InstallationDate.HasValue &&
                                                         !b.DeviceType.IsWireless
                                                         && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom)
            
                )
                .Select(x => new
                {
                    x.TerminalNo,
                    x.DeviceType.IsWireless,
                     StatusTitle = x.Status.Title,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDate,
                    x.RevokeDate
                }).ToList();
           

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";

                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "تاریخ ابطال";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";

                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.RevokeDate.ToPersianDate();


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report1-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData2(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            // var data = _dataContext.Terminals.Where(b => b.MarketerId == (byte) Enums.Marketer.BankOrBranch
            //                                              && b.InstallationDate <= reviewMonthTo
            //                                              && b.InstallationDate.HasValue
            //                                              && b.DeviceType.IsWireless
            //                                              && (!b.RevokeDate.HasValue ||
            //                                                  b.RevokeDate.Value >= reviewMonthFrom)
            //     )
            //     .Select(x => new
            //     {
            //         x.TerminalNo,
            //         x.DeviceType.IsWireless,
            //         // StatusTitle = x.Status?.Title,
            //         x.SubmitTime,
            //         x.BatchDate,
            //         x.InstallationDate,
            //         x.RevokeDate
            //     }).ToList();
            var data = (await _dataContext.TempReport1And2Datas
                    .Where(x => x.Year == year && x.Month == month && x.IsWireless).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.IsWireless,
                    //StatusTitle = x.Status؟.Title,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDate,
                    x.RevokeDate
                });

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";

                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "تاریخ ابطال";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    //worksheet.Cells[rowNumber, 3].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.RevokeDate.ToPersianDate();
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report2-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData2(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            var data = _dataContext.Terminals.Where(b => b.MarketerId == (byte) Enums.Marketer.BankOrBranch
                                                         && b.InstallationDate <= reviewMonthTo
                                                         && b.InstallationDate.HasValue
                                                         && b.DeviceType.IsWireless
                                                         && (!b.RevokeDate.HasValue ||
                                                             b.RevokeDate.Value >= reviewMonthFrom)
                )
                .Select(x => new
                {
                    x.TerminalNo,
                    x.DeviceType.IsWireless,
                     StatusTitle = x.Status.Title,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDate,
                    x.RevokeDate
                }).ToList();
           
            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";

                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "تاریخ ابطال";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    //worksheet.Cells[rowNumber, 3].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.RevokeDate.ToPersianDate();
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report2-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData3(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            // var data =     _dataContext.BranchTerminal.Where(b =>   ( !b.RevokeDate.HasValue ||   b.RevokeDate >= reviewMonthFrom ) )
            //     .Select(x => new
            //     {
            //         x.TerminalNo,
            //         x.BranchCode,
            //         x.BranchTitle
            //     });

            var data =
                (await _dataContext.TempReport3Datas.Where(x => x.Year == year && x.Month == month).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.BranchCode,
                    x.BranchTitle
                });

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;

                worksheet.Cells[1, 1].Value = "شماره ترمینال توسن";
                worksheet.Cells[1, 2].Value = "کد شعبه";
                worksheet.Cells[1, 3].Value = "نام شعبه";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.BranchCode;
                    worksheet.Cells[rowNumber, 3].Value = item.BranchTitle;
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report3-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData3(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            var data =     _dataContext.BranchTerminal.Where(b =>   ( !b.RevokeDate.HasValue ||   b.RevokeDate >= reviewMonthFrom ) )
                .Select(x => new
                {
                    x.TerminalNo,
                    x.BranchCode,
                    x.BranchTitle
                });

            

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;

                worksheet.Cells[1, 1].Value = "شماره ترمینال توسن";
                worksheet.Cells[1, 2].Value = "کد شعبه";
                worksheet.Cells[1, 3].Value = "نام شعبه";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.BranchCode;
                    worksheet.Cells[rowNumber, 3].Value = item.BranchTitle;
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report3-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData4(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var fromDate = date.AbsoluteStart();

            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();

            //
            // var data  =  _dataContext.Terminals
            //     .Where
            //     (b =>
            //         b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
            //         b.InstallationDate <= reviewMonthTo
            //         && b.InstallationDate.HasValue
            //         && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
            //         b.BatchDate >= reviewMonthFrom && b.BatchDate <= reviewMonthTo
            //     )
            //     .ToList()
            //     .Select(x=> new
            //     {
            //       
            //         x.TerminalNo,                   
            //         x.DeviceType.IsWireless,
            //         x.SubmitTime,
            //         x.BatchDate,
            //         InstallationDelay =  x.GetTermialNumberDelay(invoice.GetTerminalNoDelayAllowedWorkDay, holidays),
            //     }).ToList();
            //
            var data =
                (await _dataContext.TempReport4Datas.Where(x => x.Year == year && x.Month == month).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.IsWireless,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDelay
                });


            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "نوع دستگاه";
                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "میزان تاخیر";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDelay;


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report4-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
                public async Task<ActionResult> NewDownloadTempReportData4(int? year, int? month)
                {
                    if (!year.HasValue || !month.HasValue)
                    {
                        return new EmptyResult();
                    }
        
                    var date = $"{year}/{month}/{1}".ToMiladiDate();
                    var fromDate = date.AbsoluteStart();
        
                    var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
                    var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();
        
                    var pc = new PersianCalendar();
                    var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
                    var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);
        
                    var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                        x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
                    var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                        .ToList();
        
                    
                    var data  =  _dataContext.Terminals
                        .Where
                        (b =>
                            b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            b.InstallationDate <= reviewMonthTo
                            && b.InstallationDate.HasValue
                            && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
                            b.BatchDate >= reviewMonthFrom && b.BatchDate <= reviewMonthTo
                        )
                        .ToList()
                        .Select(x=> new
                        {
                          
                            x.TerminalNo,                   
                            x.DeviceType.IsWireless,
                            x.SubmitTime,
                            x.BatchDate,
                            InstallationDelay =  x.GetTermialNumberDelay(invoice.GetTerminalNoDelayAllowedWorkDay, holidays),
                        }).ToList();
                    
           
                    if (!data.Any())
                    {
                        return new EmptyResult();
                    }
        
                    using (var package = new ExcelPackage())
                    {
                        var workbook = package.Workbook;
        
                        var worksheet = workbook.Worksheets.Add("Data");
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
        
                        worksheet.Column(1).Width = 10;
                        worksheet.Column(2).Width = 20;
                        worksheet.Column(3).Width = 24;
                        worksheet.Column(4).Width = 25;
                        worksheet.Column(5).Width = 25;
        
                        worksheet.Cells[1, 1].Value = "شماره ترمینال";
                        worksheet.Cells[1, 2].Value = "نوع دستگاه";
                        worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                        worksheet.Cells[1, 4].Value = "کد باز";
                        worksheet.Cells[1, 5].Value = "میزان تاخیر";
        
        
                        var rowNumber = 2;
                        foreach (var item in data)
                        {
                            worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                            worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                            worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                            worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                            worksheet.Cells[rowNumber, 5].Value = item.InstallationDelay;
        
        
                            rowNumber++;
                        }
        
                        var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");
        
                        if (!Directory.Exists(dirPath))
                        {
                            Directory.CreateDirectory(dirPath);
                        }
        
                        var fileKey = $"Report4-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();
        
                        package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));
        
                        return JsonSuccessResult(fileKey);
                    }
                }
                

                [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData5(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var fromDate = date.AbsoluteStart();
            var monthIsEven = month % 2 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();


            // var data  =   _dataContext.Terminals
            //     .Where(b =>
            //         b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
            //         b.InstallationDate <= reviewMonthTo
            //         && b.InstallationDate.HasValue
            //         && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
            //         b.InstallationDate >= reviewMonthFrom && b.InstallationDate <= reviewMonthTo
            //     )
            //     .ToList()
            //     .Select(x=> new
            //     {
            //         InstallationDelay =  x.GetInstallationDelay(invoice.NotInstalledDelayAllowedWorkDay, holidays),
            //         x.TerminalNo,
            //        x.InstallationDate,
            //         x.DeviceType.IsWireless,
            //         x.SubmitTime,
            //         x.BatchDate,
            //        
            //     }).ToList();
            var data =
                (await _dataContext.TempReport5Datas.Where(x => x.Year == year && x.Month == month).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.IsWireless,
                    x.SubmitTime,
                    x.BatchDate,
                    x.InstallationDate,
                    x.InstallationDelay
                });

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "وضعیت";
                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "میزان تاخیر";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.InstallationDelay;


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report5-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData5(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var fromDate = date.AbsoluteStart();
            var monthIsEven = month % 2 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();


            var data  =   _dataContext.Terminals
                .Where(b =>
                    b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                    b.InstallationDate <= reviewMonthTo
                    && b.InstallationDate.HasValue
                    && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
                    b.InstallationDate >= reviewMonthFrom && b.InstallationDate <= reviewMonthTo
                )
                .ToList()
                .Select(x=> new
                {
                    InstallationDelay =  x.GetInstallationDelay(invoice.NotInstalledDelayAllowedWorkDay, holidays),
                    x.TerminalNo,
                   x.InstallationDate,
                    x.DeviceType.IsWireless,
                    x.SubmitTime,
                    x.BatchDate,
                   
                }).ToList();
        

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "وضعیت";
                worksheet.Cells[1, 3].Value = "تاریخ درخواست";
                worksheet.Cells[1, 4].Value = "کد باز";
                worksheet.Cells[1, 5].Value = "تاریخ نصب";
                worksheet.Cells[1, 6].Value = "میزان تاخیر";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 3].Value = item.SubmitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.BatchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 6].Value = item.InstallationDelay;


                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report5-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData6(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);
            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var monthIsEven = month % 2 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();
            // var data  = _dataContext.TerminalEms.Where(b =>
            //         b.EmTime.HasValue && b.EmTime.Value >= reviewMonthFrom && b.EmTime.Value <= reviewMonthTo)
            //     
            //     .ToList().Select(x => new
            //     {
            //       x.TerminalNo,
            //       Subject = "",//  x.Subject,
            //       FirstOperationDate = new DateTime(),
            //       LastOperationDate = new DateTime(),
            //       InstallationDelay=  x.GetInstallationDelay(invoice.NotEMDelayAllowedWorkDay, holidays),
            //      
            //     }).ToList() ; 

            var data =
                (await _dataContext.TempReport6Datas.Where(x => x.Year == year && x.Month == month).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.Subject,
                    x.FirstOperationDate,
                    x.LastOperationDate,
                    x.Type,
                    x.City,
                    x.Sla,
                    x.ValidDay,
                    x.InstallationDelay
                });


            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Width = 25;
                worksheet.Column(9).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "موضوع";
                worksheet.Cells[1, 3].Value = "تاریخ اولین عملیات";
                worksheet.Cells[1, 4].Value = "تاریخ آخرین عملیات";

                worksheet.Cells[1, 5].Value = "نوع صورت وضعیت بانک";

                    worksheet.Cells[1, 6].Value ="شهر";

                        worksheet.Cells[1,7].Value ="نوع sla";

                            worksheet.Cells[1, 8].Value ="روز مجاز";

                
                
                
                worksheet.Cells[1, 9].Value = "میزان تاخیر";
                


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.Subject;
                    worksheet.Cells[rowNumber, 3].Value = item.FirstOperationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.LastOperationDate.ToPersianDate();
                   
                    worksheet.Cells[rowNumber, 5].Value = item.Type;
                    worksheet.Cells[rowNumber, 6].Value = item.City;
                    worksheet.Cells[rowNumber, 7].Value = item.Sla;
                    worksheet.Cells[rowNumber, 8].Value = item.ValidDay;

                    
                    
                    
                    worksheet.Cells[rowNumber, 9].Value = item.InstallationDelay;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report6-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData6(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);
            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var monthIsEven = month % 2 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == 5);
            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();
            var data  = _dataContext.TerminalEms.Where(b =>
                    b.EmTime.HasValue && b.EmTime.Value >= reviewMonthFrom && b.EmTime.Value <= reviewMonthTo)
                
                .ToList().Select(x => new
                {
                  x.TerminalNo,
                  Subject = "",//  x.Subject,
                  FirstOperationDate = new DateTime(),
                  LastOperationDate = new DateTime(),
                  InstallationDelay=  x.GetInstallationDelay(invoice.NotEMDelayAllowedWorkDay, holidays),
                 
                }).ToList() ; 

     

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "موضوع";
                worksheet.Cells[1, 3].Value = "تاریخ اولین عملیات";
                worksheet.Cells[1, 4].Value = "تاریخ آخرین عملیات";
                worksheet.Cells[1, 5].Value = "میزان تاخیر";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.Subject;
                    worksheet.Cells[rowNumber, 3].Value = item.FirstOperationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 4].Value = item.LastOperationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 5].Value = item.InstallationDelay;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report6-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData7(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue || month % 3 != 0)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);

            var pmdData = _dataContext.TerminalPms
                .Where(b => b.PmTime <= reviewMonthTo && b.PmTime >= reviewMonthFrom).ToList();
            var allTerminal = _dataContext.Terminals.Where(b =>
                b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                b.InstallationDate <= reviewMonthTo
                && b.InstallationDate.HasValue
                && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom)
            ).ToList();

            // var notPm = allTerminal.Where(b =>! pmdData.Contains(b)).ToList();
            // var notPmPercent = Math.Round((decimal) notPm.Count / allTerminal.Count * 100);


            var data =
                (await _dataContext.TempReport7Datas.Where(x => x.Year == year && x.Month == month).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.IsWireless,
                    x.IsPm
                });

            //
            // var data = allTerminal.Select(x => new
            // {
            //     x.TerminalNo,
            //     x.DeviceType.IsWireless,
            //     IsPm = pmdData.Any(b => b.TerminalNo == x.TerminalNo)
            // }).ToList();

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";
                worksheet.Cells[1, 3].Value = "وضعیت";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 3].Value = item.IsPm ? "pm شده" : "pm نشده";
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report7-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        
        /// <summary>
        /// عدم جمع آوری سیار
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData8(int? year, int? month)
        {
            
            var data =
                (await _dataContext.TempReport8Datas.Where(x => x.Year == year && x.Month == month && x.IsWireless).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.CurrentMonth,
                    x.FirstRequest,
                   
                });

         

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "ماه فعلی  ";
                worksheet.Cells[1, 3].Value = "اولین درخواست";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber,2].Value = item.CurrentMonth;
                    worksheet.Cells[rowNumber, 3].Value = item.FirstRequest;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report8-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        
        /// <summary>
        /// عدم جمع آوری ثابت
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
          [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> DownloadTempReportData9(int? year, int? month)
        {
            
            
 
            var data =
                (await _dataContext.TempReport8Datas.Where(x => x.Year == year && x.Month == month && !x.IsWireless).ToListAsync())
                .Select(x => new
                {
                    x.TerminalNo,
                    x.CurrentMonth,
                    x.FirstRequest,
                   
                });
 
            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "ماه فعلی  ";
                worksheet.Cells[1, 3].Value = "اولین درخواست";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber,2].Value = item.CurrentMonth;
                    worksheet.Cells[rowNumber, 3].Value = item.FirstRequest;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report8-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }
        
        

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> NewDownloadTempReportData7(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue || month % 2 != 0)
            {
                return new EmptyResult();
            }

            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);

            var pmdData = _dataContext.TerminalPms
                .Where(b => b.PmTime <= reviewMonthTo && b.PmTime >= reviewMonthFrom).ToList();
            var allTerminal = _dataContext.Terminals.Where(b =>
                b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                b.InstallationDate <= reviewMonthTo
                && b.InstallationDate.HasValue
                && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom)
            ).ToList();

            var notPm = allTerminal.Where(b =>! pmdData.Select(v=>v.TerminalNo).Contains(b.TerminalNo)).ToList();
            var notPmPercent = Math.Round((decimal) notPm.Count / allTerminal.Count * 100);
 
            
            var data = allTerminal.Select(x => new
            {
                x.TerminalNo,
                x.DeviceType.IsWireless,
                IsPm = pmdData.Any(b => b.TerminalNo == x.TerminalNo)
            }).ToList();

            if (!data.Any())
            {
                return new EmptyResult();
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("Data");
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

                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 24;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "مدل دستگاه";
                worksheet.Cells[1, 3].Value = "وضعیت";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 3].Value = item.IsPm ? "pm شده" : "pm نشده";
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Report7-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public ActionResult DownloadReportOutputFile(string fileKey) => File(
            Server.MapPath($"~/App_Data/ReportExportFiles/{fileKey}.xlsx"),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        private static bool GetIsWirelessFromText(string input)
        {
            switch (input)
            {
                case "ثابت":
                    return false;
                case "سيار":
                case "سیار":
                    return true;
                default:
                    return false;
            }
        }

        private static bool GetIsPmFromText(string input)
        {
            switch (input)
            {
                case "pm شده":
                case "بله":
                case "1":
                    return true;
                case "pm نشده":
                case "خیر":
                case "0":
                    return false;
                default:
                    return false;
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
                    return (byte) Enums.TerminalStatus.NeedToReform;
                case "آماده تخصیص":
                    return (byte) Enums.TerminalStatus.ReadyForAllocation;
                case "تخصیص داده شده":
                    return (byte) Enums.TerminalStatus.Allocated;
                case "تست شده":
                    return (byte) Enums.TerminalStatus.Test;
                case "نصب شده":
                    return (byte) Enums.TerminalStatus.Installed;
                case "جمع آوری شده":
                case "ابطال شده":
                    return (byte) Enums.TerminalStatus.Revoked;
                case "ارسال شده به شاپرک":
                    return (byte) Enums.TerminalStatus.SendToShaparak;
                case "حذف شده":
                    return (byte) Enums.TerminalStatus.Deleted;
                case "دریافت شده از سویچ ناموفق":
                    return (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch;
                case "در انتظار جمع آوری و غیر فعال سازی":
                case "در انتظار ابطال":
                    return (byte) Enums.TerminalStatus.WaitingForRevoke;
                default:
                    return (byte) 45;
            }
        }
    }
}