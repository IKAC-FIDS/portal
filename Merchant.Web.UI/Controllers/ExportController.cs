using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Portal.IService;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.DataModel;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using Enums = TES.Common.Enumerations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RestSharp;
using PdfSharp.Drawing.BarCodes;
using Newtonsoft.Json;
using TES.Merchant.Web.UI.Functions;
using System.Transactions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class ExportController : BaseController
    {
        private readonly AppDataContext _dataContext;
        private ICustomerService _CustomerService;

        public ExportController(AppDataContext dataContext, ICustomerService customerService)
        {
            _dataContext = dataContext;
            _CustomerService = customerService;
        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
            DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
            DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportCustomerReport(UploadTerminalValidationDataViewModel viewModel)
        {
            var query = _dataContext.CustomerStatusResults.AsQueryable();
            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            }

            if (User.IsTehranBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var d = _CustomerService.GetCustomerData(User, viewModel.CustomerId, viewModel.Month, viewModel.Year,
                CurrentUserBranchId, viewModel.Special, viewModel.LowTransaction);
            var data = d.Select(x => new CustomerStatusResultsViewModel
            {
                Id = x.Id,
                Month = x.IsGoodMonth,
                Year = x.IsGoodYear,
                IsGood = x.IsGood,
                IsGoodValue = x.IsGoodValue % 1 > 0.5 ? Math.Ceiling(x.IsGoodValue) : Math.Floor(x.IsGoodValue),
                CustomerId = x.CustomerId,
                Daramad = x.Daramad % 1 > 0.5 ? Math.Ceiling(x.Daramad) : Math.Floor(x.Daramad),
                Hazineh = x.Hazineh % 1 > 0.5 ? Math.Ceiling(x.Hazineh) : Math.Floor(x.Hazineh),

                Avg = x.Avg % 1 > 0.5 ? Math.Ceiling(x.Avg) : Math.Floor(x.Avg),
                Special = x.Special,
            })
                .ToList();



            var rows = data.Select(x => new CustomerStatusResultsViewModel
            {
                Id = x.Id,
                Month = x.Month,
                Year = x.Year,
                IsGood = x.IsGood,
                IsGoodValue = x.IsGoodValue,
                CustomerId = x.CustomerId,
                Daramad = Math.Round((Double)x.Daramad, 2),
                Hazineh = Math.Round((Double)x.Hazineh, 2),
                Avg = x.Avg,
                Special = x.Special
            })


                .ToList();
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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره  مشتری";
                worksheet.Cells[1, 2].Value = "ویژه ";
                worksheet.Cells[1, 3].Value = "  معدل ";
                worksheet.Cells[1, 4].Value = "درآمد  ";
                worksheet.Cells[1, 5].Value = "هزینه";
                worksheet.Cells[1, 6].Value = "سودده / زیان ده";
                worksheet.Cells[1, 7].Value = "سود  / زیان  ";


                var rowNumber = 2;

                foreach (var item in rows)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.CustomerId;
                    worksheet.Cells[rowNumber, 2].Value = !item.Special.HasValue ? "" : (item.Special.Value ? "بله" : "خیر");
                    worksheet.Cells[rowNumber, 3].Value = item.AvgEx;
                    worksheet.Cells[rowNumber, 4].Value = item.DaramadEx;
                    worksheet.Cells[rowNumber, 5].Value = item.HazinehEx;
                    worksheet.Cells[rowNumber, 6].Value = !item.IsGood.HasValue ? "" : (item.IsGood.Value ? "سودده" : "زیان ده");
                    worksheet.Cells[rowNumber, 7].Value = item.IsGoodValue;
                    rowNumber++;

                }
                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();
                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));
                return JsonSuccessResult(fileKey);
            }



        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
              DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
              DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportBiResultByParametersData(BiByParametersSearch searchParameters)
        {



            var s = from a in _dataContext.CalculateResults
                    join
                        o in _dataContext.OrganizationUnits on a.BranchId equals o.Id
                    where a.IsGoodYear == searchParameters.Year
                            && a.IsInNetwork.HasValue && a.IsInNetwork.Value
                            && a.IsGoodMonth == searchParameters.Month &&
                            (string.IsNullOrEmpty(searchParameters.TerminalNo) ||
                             a.TerminalNo == searchParameters.TerminalNo)
                    select new ResultParameters
                    {
                        Id = a.Id,
                        IsGoodYear = a.IsGoodYear,
                        TerminalNo = a.TerminalNo,
                        Title = o.Title,
                        IsGood = a.IsGood,
                        AccountNumber = a.AccountNumber,
                        IsGoodValue = a.IsGoodValue,
                        p_daramad_Vadie = a.p_daramad_Vadie,
                        p_daramd_Tashilat = a.p_daramd_Tashilat,
                        p_hazineh_rent = a.p_hazineh_rent,
                        p_hazineh_hashiyeSood = a.p_hazineh_hashiyeSood,
                        p_hazineh_karmozdShapark = a.p_hazineh_karmozdShapark,
                        p_hazineh_soodePardakty = a.p_hazineh_soodePardakty,
                        IsGoodMonth = a.IsGoodMonth,
                        p_daramad_Moadel = a.p_daramad_Moadel,

                    };
            var rows = new List<ResultParameters>();
            var rowss = s.ToList();
            foreach (var VARIABLE in rowss)
            {
                try
                {
                    if (!searchParameters.p_daramad_Moadel)
                    {
                        VARIABLE.p_daramad_Moadel = 0;
                    }

                    if (!searchParameters.p_daramad_Vadie)
                    {
                        VARIABLE.p_daramad_Vadie = 0;
                    }

                    if (!searchParameters.p_daramd_Tashilat)
                    {
                        VARIABLE.p_daramd_Tashilat = 0;
                    }

                    var Daramad = VARIABLE.p_daramad_Moadel + VARIABLE.p_daramd_Tashilat + VARIABLE.p_daramad_Vadie;

                    if (!searchParameters.p_hazineh_karmozdShapark)
                    {
                        VARIABLE.p_hazineh_karmozdShapark = 0;
                    }

                    if (!searchParameters.p_hazineh_hashiyeSood)
                    {
                        VARIABLE.p_hazineh_hashiyeSood = 0;
                    }

                    if (!searchParameters.p_hazineh_rent)
                    {
                        VARIABLE.p_hazineh_rent = 0;
                    }

                    if (!searchParameters.p_hazineh_soodePardakty)
                    {
                        VARIABLE.p_hazineh_soodePardakty = 0;
                    }

                    var Hazine = VARIABLE.p_hazineh_soodePardakty + VARIABLE.p_hazineh_rent +
                                 VARIABLE.p_hazineh_hashiyeSood + VARIABLE.p_hazineh_karmozdShapark;
                    VARIABLE.IsGoodValue = Daramad - Hazine;
                    VARIABLE.IsGood = VARIABLE.IsGoodValue > 0;

                    var a = new ResultParameters
                    {
                        TerminalNo = VARIABLE.TerminalNo,
                        Daramad = Math.Round(Daramad, 2),
                        Hazine = Math.Round(Hazine, 2),
                        IsGood = VARIABLE.IsGood,
                        Title = VARIABLE.Title,
                        AccountNumber = VARIABLE.AccountNumber,
                        IsGoodValue = VARIABLE.IsGoodValue.HasValue ? Math.Round(VARIABLE.IsGoodValue.Value, 2) : 0
                    };
                    rows.Add(a);
                }


                catch (Exception ex)
                {
                    var a = new ResultParameters
                    {
                        TerminalNo = VARIABLE.TerminalNo,
                        Daramad = 0,
                        Hazine = 0,
                        IsGood = false,
                        IsGoodValue = 0
                    };
                    rows.Add(a);
                }
            }


            if (searchParameters.TransactionStatusList != null)
            {
                var a = searchParameters.TransactionStatusList.FirstOrDefault();
                if (a == Enums.TransactionStatus.HighTransaction)
                {
                    rows = rows.Where(b =>
                        b.IsGood.Value == true).ToList();

                }
                else
                {

                    rows = rows.Where(b =>
                        b.IsGood.Value == false).ToList();
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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 26;
                worksheet.Column(7).Width = 26;



                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = " شماره حساب  ";
                worksheet.Cells[1, 3].Value = "  شعبه ";


                worksheet.Cells[1, 4].Value = "درآمد  ";
                worksheet.Cells[1, 5].Value = "هزینه";
                worksheet.Cells[1, 6].Value = "سود / زیان";
                worksheet.Cells[1, 7].Value = "وضعیت ( ماه گذشته) ";




                var rowNumber = 2;

                foreach (var item in rows)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;

                    worksheet.Cells[rowNumber, 2].Value = item.AccountNumber;
                    worksheet.Cells[rowNumber, 3].Value = item.Title;

                    worksheet.Cells[rowNumber, 4].Value = item.DaramadEx;

                    worksheet.Cells[rowNumber, 5].Value = item.HazineEx;
                    worksheet.Cells[rowNumber, 6].Value = item.IsGoodValueEx;
                    worksheet.Cells[rowNumber, 7].Value = !item.IsGood.HasValue ? "" : (item.IsGood.Value ? "سودده" : "زیان ده");

                    rowNumber++;

                }


                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }



        }


        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
        DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
        DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportBiResultByParametersDataOutOfNetwork(BiByParametersSearch searchParameters)
        {



            var s = from a in _dataContext.CalculateResults
                    join
                        o in _dataContext.OrganizationUnits on a.BranchId equals o.Id
                    where a.IsGoodYear == searchParameters.Year
                            && a.IsInNetwork.HasValue && !a.IsInNetwork.Value
                            && a.IsGoodMonth == searchParameters.Month &&
                            (string.IsNullOrEmpty(searchParameters.TerminalNo) ||
                             a.TerminalNo == searchParameters.TerminalNo)
                    select new ResultParameters
                    {
                        Id = a.Id,
                        IsGoodYear = a.IsGoodYear,
                        TerminalNo = a.TerminalNo,
                        Title = o.Title,
                        IsGood = a.IsGood,
                        AccountNumber = a.AccountNumber,
                        IsGoodValue = a.IsGoodValue,
                        p_daramad_Vadie = a.p_daramad_Vadie,
                        p_daramd_Tashilat = a.p_daramd_Tashilat,
                        p_hazineh_rent = a.p_hazineh_rent,
                        p_hazineh_hashiyeSood = a.p_hazineh_hashiyeSood,
                        p_hazineh_karmozdShapark = a.p_hazineh_karmozdShapark,
                        p_hazineh_soodePardakty = a.p_hazineh_soodePardakty,
                        IsGoodMonth = a.IsGoodMonth,
                        p_daramad_Moadel = a.p_daramad_Moadel,
                        TransactionCount = a.TransactionCount,
                        TransactionValue = a.TransactionValue,
                        PspTitle = a.PspTitle,
                        PspId = a.PspId
                    };
            var rows = new List<ResultParameters>();
            var rowss = s.ToList();
            foreach (var VARIABLE in rowss)
            {
                try
                {
                    if (!searchParameters.p_daramad_Moadel)
                    {
                        VARIABLE.p_daramad_Moadel = 0;
                    }

                    if (!searchParameters.p_daramad_Vadie)
                    {
                        VARIABLE.p_daramad_Vadie = 0;
                    }

                    if (!searchParameters.p_daramd_Tashilat)
                    {
                        VARIABLE.p_daramd_Tashilat = 0;
                    }

                    var Daramad = VARIABLE.p_daramad_Moadel + VARIABLE.p_daramd_Tashilat + VARIABLE.p_daramad_Vadie;

                    if (!searchParameters.p_hazineh_karmozdShapark)
                    {
                        VARIABLE.p_hazineh_karmozdShapark = 0;
                    }

                    if (!searchParameters.p_hazineh_hashiyeSood)
                    {
                        VARIABLE.p_hazineh_hashiyeSood = 0;
                    }

                    if (!searchParameters.p_hazineh_rent)
                    {
                        VARIABLE.p_hazineh_rent = 0;
                    }

                    if (!searchParameters.p_hazineh_soodePardakty)
                    {
                        VARIABLE.p_hazineh_soodePardakty = 0;
                    }

                    var Hazine = VARIABLE.p_hazineh_soodePardakty + VARIABLE.p_hazineh_rent +
                                 VARIABLE.p_hazineh_hashiyeSood + VARIABLE.p_hazineh_karmozdShapark;
                    VARIABLE.IsGoodValue = Daramad - Hazine;
                    VARIABLE.IsGood = VARIABLE.IsGoodValue > 0;

                    var a = new ResultParameters
                    {
                        TerminalNo = VARIABLE.TerminalNo,
                        Daramad = Math.Round(Daramad, 2),
                        Hazine = Math.Round(Hazine, 2),
                        IsGood = VARIABLE.IsGood,
                        AccountNumber = VARIABLE.AccountNumber,
                        Title = VARIABLE.Title,
                        TransactionCount = VARIABLE.TransactionCount,
                        TransactionValue = VARIABLE.TransactionValueEx,
                        PspTitle = VARIABLE.PspTitle,
                        PspId = VARIABLE.PspId,
                        IsGoodValue = VARIABLE.IsGoodValueEx,

                    };
                    rows.Add(a);
                }


                catch (Exception ex)
                {
                    var a = new ResultParameters
                    {
                        TerminalNo = VARIABLE.TerminalNo,
                        Daramad = 0,
                        Hazine = 0,
                        IsGood = false,
                        IsGoodValue = 0
                    };
                    rows.Add(a);
                }
            }


            if (searchParameters.TransactionStatusList != null)
            {
                var a = searchParameters.TransactionStatusList.FirstOrDefault();
                if (a == Enums.TransactionStatus.HighTransaction)
                {
                    rows = rows.Where(b =>
                        b.IsGood.Value == true).ToList();

                }
                else
                {

                    rows = rows.Where(b =>
                        b.IsGood.Value == false).ToList();
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
                worksheet.Column(5).Width = 26;
                worksheet.Column(6).Width = 26;
                worksheet.Column(7).Width = 26;



                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = " Psp  ";
                worksheet.Cells[1, 3].Value = " شماره حساب  ";
                worksheet.Cells[1, 4].Value = "  شعبه ";
                worksheet.Cells[1, 5].Value = "  جمع تراکنش ";
                worksheet.Cells[1, 6].Value = "  تعداد تراکنش ";
                worksheet.Cells[1, 7].Value = "درآمد  ";
                worksheet.Cells[1, 8].Value = "هزینه";
                worksheet.Cells[1, 9].Value = "سود / زیان";
                worksheet.Cells[1, 10].Value = "وضعیت ( ماه گذشته) ";




                var rowNumber = 2;

                foreach (var item in rows)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.PspTitleEx;
                    worksheet.Cells[rowNumber, 3].Value = item.AccountNumber;
                    worksheet.Cells[rowNumber, 4].Value = item.Title;
                    worksheet.Cells[rowNumber, 5].Value = item.TransactionValueEx;
                    worksheet.Cells[rowNumber, 6].Value = item.TransactionCount;
                    worksheet.Cells[rowNumber, 7].Value = item.DaramadEx;
                    worksheet.Cells[rowNumber, 8].Value = item.HazineEx;
                    worksheet.Cells[rowNumber, 9].Value = item.IsGoodValue;
                    worksheet.Cells[rowNumber, 10].Value = !item.IsGood.HasValue ? "" : (item.IsGood.Value ? "سودده" : "زیان ده");

                    rowNumber++;

                }

                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }



        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
            DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
            DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportLowTransactionData(BiByParametersSearch searchParameters)
        {

            var rowss = new List<ResultParameters>();

            PersianCalendar t = new PersianCalendar();


            var y1 = 1400;
            var y2 = 1400;
            int y3 = 1400;
            var m1 = 1400;
            var m2 = 1400;
            var m3 = 1400;


            if (searchParameters.TwoMonthInActive.Value)
            {
                if (t.GetMonth(DateTime.Now) == 1)
                {
                    y1 = t.GetYear(DateTime.Now) - 1;
                    y2 = t.GetYear(DateTime.Now) - 1;
                    m1 = 11;
                    m2 = 12;
                }
                else if (t.GetMonth(DateTime.Now) == 2)
                {
                    y1 = t.GetYear(DateTime.Now);
                    y2 = t.GetYear(DateTime.Now) - 1;
                    m1 = 12;
                    m2 = 1;
                }
                else
                {
                    y1 = t.GetYear(DateTime.Now);
                    y2 = t.GetYear(DateTime.Now);
                    m1 = t.GetMonth(DateTime.Now) - 2;
                    m2 = t.GetMonth(DateTime.Now) - 1;
                }

                var oooo = _dataContext.CalculateResults.Where(a => a.IsGood.HasValue && !a.IsGood.Value &&
                                                                    string.IsNullOrEmpty(searchParameters.TerminalNo) ? true :

                        a.TerminalNo == searchParameters.TerminalNo)
                    .GroupBy(a => a.TerminalNo).ToList();
                foreach (var VARIABLE in oooo)
                {
                    if (VARIABLE.Any(b => b.IsGoodYear == y1 && b.IsGoodMonth == m1))
                        if (VARIABLE.Any(b => b.IsGoodYear == y2 && b.IsGoodMonth == m2))
                        {
                            var daramad = VARIABLE.Sum(a => a.p_daramad_Moadel) + VARIABLE.Sum(b => b.p_daramad_Vadie) +
                                          VARIABLE.Sum(j => j.p_daramd_Tashilat);
                            var Hazine = VARIABLE.Sum(e => e.p_hazineh_soodePardakty) +
                                         VARIABLE.Sum(e => e.p_hazineh_rent) +
                                         VARIABLE.Sum(e => e.p_hazineh_hashiyeSood) +
                                         VARIABLE.Sum(e => e.p_hazineh_karmozdShapark);
                            var d = daramad - Hazine;
                            rowss.Add(new ResultParameters()
                            {
                                IsGood = false,
                                IsGoodValue = d % 1 > 0.5 ? Math.Ceiling(d) : Math.Floor(d),
                                Daramad = daramad % 1 > 0.5 ? Math.Ceiling(daramad) : Math.Floor(daramad),
                                Hazine = Hazine % 1 > 0.5 ? Math.Ceiling(Hazine) : Math.Floor(Hazine),
                                TerminalNo = VARIABLE.Key
                            });
                        }
                }
            }
            else if (searchParameters.ThreeMonthInActive.Value)
            {
                ViewBag.Title = "مشتریان زیان ده در سه ماه متوالی";
                if (t.GetMonth(DateTime.Now) == 1)
                {
                    y1 = t.GetYear(DateTime.Now) - 1;
                    y2 = t.GetYear(DateTime.Now) - 1;
                    m1 = 11;
                    m2 = 12;
                    m3 = 10;
                }
                else if (t.GetMonth(DateTime.Now) == 2)
                {
                    y1 = t.GetYear(DateTime.Now);
                    y2 = t.GetYear(DateTime.Now) - 1;
                    m1 = 12;
                    m2 = 1;
                    m3 = 11;
                }
                else
                {
                    y1 = t.GetYear(DateTime.Now);
                    y2 = t.GetYear(DateTime.Now);
                    y3 = y2;
                    m1 = t.GetMonth(DateTime.Now) - 2;
                    m2 = t.GetMonth(DateTime.Now) - 1;
                    m3 = t.GetMonth(DateTime.Now) - 3;
                }


                var oooo = _dataContext.CalculateResults.Where(a => a.IsGood.HasValue && !a.IsGood.Value &&
                                                                    string.IsNullOrEmpty(searchParameters.TerminalNo) ? true :

                        a.TerminalNo == searchParameters.TerminalNo)
                    .GroupBy(a => a.TerminalNo).ToList();
                foreach (var VARIABLE in oooo)
                {
                    if (VARIABLE.Any(b => b.IsGoodYear == y1 && b.IsGoodMonth == m1))
                        if (VARIABLE.Any(b => b.IsGoodYear == y2 && b.IsGoodMonth == m2))
                            if (VARIABLE.Any(b => b.IsGoodYear == y3 && b.IsGoodMonth == m3))
                            {
                                var daramad = VARIABLE.Sum(a => a.p_daramad_Moadel) +
                                              VARIABLE.Sum(b => b.p_daramad_Vadie) +
                                              VARIABLE.Sum(j => j.p_daramd_Tashilat);
                                var Hazine = VARIABLE.Sum(e => e.p_hazineh_soodePardakty) +
                                             VARIABLE.Sum(e => e.p_hazineh_rent) +
                                             VARIABLE.Sum(e => e.p_hazineh_hashiyeSood) +
                                             VARIABLE.Sum(e => e.p_hazineh_karmozdShapark);
                                var d = daramad - Hazine;
                                rowss.Add(new ResultParameters()
                                {
                                    IsGood = false,
                                    IsGoodValue = d % 1 > 0.5 ? Math.Ceiling(d) : Math.Floor(d),
                                    Daramad = daramad % 1 > 0.5 ? Math.Ceiling(daramad) : Math.Floor(daramad),
                                    Hazine = Hazine % 1 > 0.5 ? Math.Ceiling(Hazine) : Math.Floor(Hazine),
                                    TerminalNo = VARIABLE.Key
                                });
                            }
                }
            }



            if (searchParameters.TransactionStatusList != null)
            {
                var a = searchParameters.TransactionStatusList.FirstOrDefault();
                if (a == Enums.TransactionStatus.HighTransaction)
                {
                    rowss = rowss.Where(b =>
                        b.IsGood.Value == true).ToList();
                }
                else
                {
                    rowss = rowss.Where(b =>
                        b.IsGood.Value == false).ToList();
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
                worksheet.Column(5).Width = 26;



                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = "درآمد  ";
                worksheet.Cells[1, 3].Value = "هزینه";
                worksheet.Cells[1, 4].Value = "سود / زیان";
                worksheet.Cells[1, 5].Value = "وضعیت ( ماه گذشته) ";




                var rowNumber = 2;

                foreach (var item in rowss)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.Daramad;
                    worksheet.Cells[rowNumber, 3].Value = item.Hazine;
                    worksheet.Cells[rowNumber, 4].Value = item.IsGoodValue;
                    worksheet.Cells[rowNumber, 5].Value = !item.IsGood.HasValue ? "" : (item.IsGood.Value ? "سودده" : "زیان ده");

                    rowNumber++;

                }

                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }



        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser
            , DefaultRoles.Auditor
            , DefaultRoles.SupervisionUser, DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement, DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportTerminalData(TerminalSearchParameters searchParams)
        {
            searchParams.IsBranchUser = User.IsBranchUser();
            searchParams.IsSupervisionUser = User.IsSupervisionUser();
            searchParams.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            searchParams.IsCountyBranchManagment = User.IsCountyBranchManagementUser();
            searchParams.CurrentUserBranchId = CurrentUserBranchId;


            var (rows, totalRowsCount) =
                await _dataContext.GetTerminalData(searchParams, "", true, 1 - 1, 300);

            var data = await _dataContext.GetTerminalExportData(searchParams);

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
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
                worksheet.Column(21).Width = 20;
                worksheet.Column(22).Width = 26;
                worksheet.Column(23).Width = 26;
                worksheet.Column(24).Width = 17;
                worksheet.Column(25).Width = 75;
                worksheet.Column(26).Width = 13;
                worksheet.Column(27).Width = 16;
                worksheet.Column(28).Width = 16;
                worksheet.Column(29).Width = 16;
                worksheet.Column(30).Width = 65;
                worksheet.Column(31).Width = 45;
                worksheet.Column(32).Width = 10;
                worksheet.Column(33).Width = 14;
                worksheet.Column(34).Width = 17;
                worksheet.Column(35).Width = 16;
                worksheet.Column(36).Width = 20;
                worksheet.Column(37).Width = 26;
                worksheet.Column(38).Width = 16;
                worksheet.Column(39).Width = 21;
                worksheet.Column(40).Width = 26;
                worksheet.Column(41).Width = 33;
                worksheet.Column(42).Width = 22;
                worksheet.Column(43).Width = 22;
                worksheet.Column(44).Width = 22;
                worksheet.Column(45).Width = 16;
                worksheet.Column(46).Width = 16;
                worksheet.Column(47).Width = 26;
                worksheet.Column(48).Width = 20;
                worksheet.Column(49).Width = 26;
                worksheet.Column(50).Width = 26;
                worksheet.Column(51).Width = 26;
                worksheet.Column(52).Width = 26;
                worksheet.Column(53).Width = 26;
                worksheet.Column(54).Width = 26;
                worksheet.Column(55).Width = 26;
                worksheet.Column(56).Width = 26;
                worksheet.Column(57).Width = 26;
                //worksheet.Column(55).Width = 10;

                worksheet.Cells[1, 1].Value = "شماره پیگیری";
                worksheet.Cells[1, 2].Value = "کدملی";
                worksheet.Cells[1, 3].Value = "نام فروشگاه";
                worksheet.Cells[1, 4].Value = "نام";
                worksheet.Cells[1, 5].Value = "نام خانوادگی";
                worksheet.Cells[1, 6].Value = "نام (انگلیسی)";
                worksheet.Cells[1, 7].Value = "نام خانوادگی (انگلیسی)";
                worksheet.Cells[1, 8].Value = "شماره پایانه";
                worksheet.Cells[1, 9].Value = "شماره پذیرنده";
                worksheet.Cells[1, 10].Value = "وضعیت";


                worksheet.Cells[1, 11].Value = "شرکت psp";
                worksheet.Cells[1, 12].Value = "کدشعبه";
                worksheet.Cells[1, 13].Value = "نام شعبه";
                worksheet.Cells[1, 14].Value = "شماره حساب";
                worksheet.Cells[1, 15].Value = "شماره شبا";
                worksheet.Cells[1, 16].Value = "تاریخ درخواست";
                worksheet.Cells[1, 17].Value = "تاریخ کدباز یا بچ";
                worksheet.Cells[1, 18].Value = "تاریخ نصب";
                worksheet.Cells[1, 19].Value = "تاریخ جمع آوری";
                worksheet.Cells[1, 20].Value = "نوع دستگاه درخواستی";
                worksheet.Cells[1, 21].Value = "متن آخرین خطای دریافتی از PSP";
                worksheet.Cells[1, 22].Value = "شهر";
                worksheet.Cells[1, 23].Value = "استان";
                worksheet.Cells[1, 24].Value = "سرپرستی";
                worksheet.Cells[1, 25].Value = "آدرس";
                worksheet.Cells[1, 26].Value = "پیش شماره";
                worksheet.Cells[1, 27].Value = "تلفن";
                worksheet.Cells[1, 28].Value = "شماره موبایل";
                worksheet.Cells[1, 29].Value = "کدپستی";
                worksheet.Cells[1, 30].Value = "صنف";
                worksheet.Cells[1, 31].Value = "صنف تکمیلی";
                worksheet.Cells[1, 32].Value = "شخصیت";
                worksheet.Cells[1, 33].Value = "تاریخ تولد";
                worksheet.Cells[1, 34].Value = "شماره شناسنامه";
                worksheet.Cells[1, 35].Value = "نام پدر";
                worksheet.Cells[1, 36].Value = "نام پدر (انگلیسی)";
                worksheet.Cells[1, 37].Value = "شناسه ملی شرکت";
                worksheet.Cells[1, 38].Value = "تاریخ ثبت شرکت";
                worksheet.Cells[1, 39].Value = "شماره ثبت شرکت";
                worksheet.Cells[1, 40].Value = "کاربر ثبت کننده اطلاعات";
                worksheet.Cells[1, 41].Value = "بازاریابی توسط";
                worksheet.Cells[1, 42].Value = "مجموع مبلغ کل تراکنش ها";
                worksheet.Cells[1, 43].Value = "تعداد تراکنش";
                worksheet.Cells[1, 44].Value = "تاریخ آخرین تراکنش";
                worksheet.Cells[1, 45].Value = "جنسیت";
                worksheet.Cells[1, 46].Value = "در قید حیات";
                worksheet.Cells[1, 47].Value = "شماره قرارداد";
                worksheet.Cells[1, 48].Value = "تاریخ سند مسدودی";
                worksheet.Cells[1, 49].Value = "شماره سند مسدودی";
                worksheet.Cells[1, 50].Value = "شماره حساب مسدودی";
                worksheet.Cells[1, 51].Value = "مبلغ مسدودی";
                worksheet.Cells[1, 52].Value = "از تاریخ تراکنش";
                worksheet.Cells[1, 53].Value = "تا تاریخ تراکنش";

                worksheet.Cells[1, 54].Value = "کد رهگیری ثبت نام مالیاتی";

                worksheet.Cells[1, 55].Value = "آدرس";

                worksheet.Cells[1, 56].Value = "کد پستی";


                worksheet.Cells[1, 57].Value = "وضعیت سود ده / زیان ده";
                worksheet.Cells[1, 58].Value = "وضعیت شاپرکی";
                worksheet.Cells[1, 59].Value = "وضعیت فعالیت";
                worksheet.Cells[1, 60].Value = "تاخیر در نصب";
                worksheet.Cells[1, 61].Value = "سود   / زیان";


                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalId;
                    worksheet.Cells[rowNumber, 2].Value = item.NationalCode;
                    worksheet.Cells[rowNumber, 3].Value = item.TerminalTitle;
                    worksheet.Cells[rowNumber, 4].Value = item.FirstName;
                    worksheet.Cells[rowNumber, 5].Value = item.LastName;
                    worksheet.Cells[rowNumber, 6].Value = item.EnglishFirstName;
                    worksheet.Cells[rowNumber, 7].Value = item.EnglishLastName;
                    worksheet.Cells[rowNumber, 8].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 9].Value = item.MerchantNo;
                    worksheet.Cells[rowNumber, 10].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 11].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 12].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 13].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 14].Value = item.AccountNo;
                    worksheet.Cells[rowNumber, 15].Value = item.ShebaNo;
                    worksheet.Cells[rowNumber, 16].Value = item.SubmitTime.ToPersianDateTime();
                    worksheet.Cells[rowNumber, 17].Value = item.BatchDate.ToPersianDateTime();
                    worksheet.Cells[rowNumber, 18].Value = item.InstallationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 19].Value = item.RevokeDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 20].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 21].Value = item.ErrorComment;
                    worksheet.Cells[rowNumber, 22].Value = item.CityTitle;
                    worksheet.Cells[rowNumber, 23].Value = item.StateTitle;
                    worksheet.Cells[rowNumber, 24].Value = item.ParentBranchTitle;
                    worksheet.Cells[rowNumber, 25].Value = item.Address;
                    worksheet.Cells[rowNumber, 26].Value = item.TelCode;
                    worksheet.Cells[rowNumber, 27].Value = item.Tel;
                    worksheet.Cells[rowNumber, 28].Value = item.Mobile;
                    worksheet.Cells[rowNumber, 29].Value = item.PostCode;
                    worksheet.Cells[rowNumber, 30].Value = item.GuildTitle;
                    worksheet.Cells[rowNumber, 31].Value = item.ParentGuildTitle;
                    worksheet.Cells[rowNumber, 32].Value = item.LegalPersonalityTitle;
                    worksheet.Cells[rowNumber, 33].Value = item.Birthdate.ToPersianDate();
                    worksheet.Cells[rowNumber, 34].Value = item.IdentityNumber;
                    worksheet.Cells[rowNumber, 35].Value = item.FatherName;
                    worksheet.Cells[rowNumber, 36].Value = item.EnglishFatherName;
                    worksheet.Cells[rowNumber, 37].Value = item.LegalNationalCode;
                    worksheet.Cells[rowNumber, 38].Value = item.CompanyRegistrationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 39].Value = item.CompanyRegistrationNumber;
                    worksheet.Cells[rowNumber, 40].Value = item.SubmitterUserFullName;
                    worksheet.Cells[rowNumber, 41].Value = item.MarketerTitle;
                    worksheet.Cells[rowNumber, 42].Value = item.SumOfTransactions;
                    worksheet.Cells[rowNumber, 43].Value = item.TransactionCount;
                    worksheet.Cells[rowNumber, 44].Value = item.LastTransactionDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 45].Value = item.IsMale ? "مرد" : "زن";
                    worksheet.Cells[rowNumber, 46].Value = "در قید حیات";
                    worksheet.Cells[rowNumber, 47].Value = item.ContractNo;
                    worksheet.Cells[rowNumber, 48].Value = item.BlockDocumentDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 49].Value = item.BlockDocumentNumber;
                    worksheet.Cells[rowNumber, 50].Value = item.BlockAccountNumber;
                    worksheet.Cells[rowNumber, 51].Value = item.BlockPrice;
                    worksheet.Cells[rowNumber, 52].Value = searchParams.FromTransactionDate?.ToPersianDate();
                    worksheet.Cells[rowNumber, 53].Value = searchParams.ToTransactionDate?.ToPersianDate();

                    worksheet.Cells[rowNumber, 54].Value = item.TaxPayerCode;
                    worksheet.Cells[rowNumber, 55].Value = item.HomeAddress;
                    worksheet.Cells[rowNumber, 56].Value = item.HomePostCode;
                    worksheet.Cells[rowNumber, 57].Value = item.IsGood.HasValue ? (item.IsGood.Value ? "سودده" : "زیان ده") : "-";
                    worksheet.Cells[rowNumber, 58].Value = item.LowTransaction.HasValue ? (item.LowTransaction.Value ? "کم تراکنش" : " پر تراکنش  ") : "-";
                    worksheet.Cells[rowNumber, 59].Value = item.IsActive.HasValue ? ((item.IsActive.Value ? "فعال" : "غیر فعال")) : "-";
                    worksheet.Cells[rowNumber, 60].Value = item.InstallDelayDays;
                    worksheet.Cells[rowNumber, 61].Value = item.IsGoodValue;
                    rowNumber++;

                }

                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }



        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
         DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
         DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportSheabAccountData(BiByParametersSearch searchParameters)
        {
            int month = searchParameters.Month;
            int year = searchParameters.Year;

            var client = new RestClient($"http://localhost:5072/GetShebaAndAccountReport?year={year}&month={month}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/ShebaAndAccount?year={year}&month={month}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();

            result.Data = response.Content;


            var data = JsonConvert.DeserializeObject<List<ShebaAccountModel>>(result.Data.ToString());

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("شبا و حساب");

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

                //worksheet.Column(55).Width = 10;

                worksheet.Cells[1, 1].Value = "شماره شبا";
                worksheet.Cells[1, 2].Value = "شماره حساب";



                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Sheba;
                    worksheet.Cells[rowNumber, 2].Value = item.Account;

                    rowNumber++;

                }

                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"ShebaAccount-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser,
        DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement,
        DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportComplementaryReportData(BiByParametersSearch searchParameters)
        {
            int month = searchParameters.Month;
            int year = searchParameters.Year;

            var client = new RestClient($"http://localhost:5072/GetResultComplementaryReport?year={year}&month={month}");
            // var client = new RestClient($"http://192.168.10.102:8008/GetResultComplementaryReport?year={year}&month={month}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();

            result.Data = response.Content;


            var data = JsonConvert.DeserializeObject<List<ComplemetaryReportField>>(result.Data.ToString());

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                #region Complementary Report 
                /*
                var worksheet = workbook.Worksheets.Add("گزارش جامع");

                worksheet.Row(1).Height = 50;
                var headerRowStyle = worksheet.Row(1).Style;
                headerRowStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRowStyle.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#022349"));
                headerRowStyle.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                headerRowStyle.Font.Bold = true;
                headerRowStyle.Font.Size = 12;
                headerRowStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                headerRowStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var cellsStyle = worksheet.Cells.Style;
                cellsStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                cellsStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                worksheet.Column(1).Width = 16;
                worksheet.Column(2).Width = 16;
                worksheet.Column(3).Width = 16;
                worksheet.Column(4).Width = 16;
                worksheet.Column(5).Width = 16;
                worksheet.Column(6).Width = 16;
                worksheet.Column(7).Width = 16;
                worksheet.Column(8).Width = 16;
                worksheet.Column(9).Width = 16;
                worksheet.Column(10).Width = 16;
                worksheet.Column(11).Width = 16;
                worksheet.Column(12).Width = 16;
                worksheet.Column(13).Width = 16;
                worksheet.Column(14).Width = 16;
                worksheet.Column(15).Width = 16;
                worksheet.Column(16).Width = 16;
                worksheet.Column(17).Width = 16;
                worksheet.Column(18).Width = 16;
                worksheet.Column(19).Width = 16;
                worksheet.Column(20).Width = 16;
                worksheet.Column(21).Width = 16;
                worksheet.Column(22).Width = 16;
                worksheet.Column(23).Width = 16;
                worksheet.Column(24).Width = 16;
                worksheet.Column(25).Width = 16;
                worksheet.Column(26).Width = 16;
                worksheet.Column(27).Width = 16;
                worksheet.Column(28).Width = 16;
                worksheet.Column(29).Width = 16;
                worksheet.Column(30).Width = 16;
                worksheet.Column(31).Width = 16;
                worksheet.Column(32).Width = 16;
                worksheet.Column(33).Width = 16;
                worksheet.Column(34).Width = 16;
                worksheet.Column(35).Width = 16;
                worksheet.Column(36).Width = 16;
                worksheet.Column(37).Width = 16;
                worksheet.Column(38).Width = 16;
                worksheet.Column(39).Width = 16;
                worksheet.Column(40).Width = 16;
                worksheet.Column(41).Width = 16;
                worksheet.Column(42).Width = 16;
                worksheet.Column(43).Width = 16;
                worksheet.Column(44).Width = 16;
                worksheet.Column(45).Width = 16;
                worksheet.Column(46).Width = 16;
                worksheet.Column(47).Width = 16;
                worksheet.Column(48).Width = 16;
                worksheet.Column(49).Width = 16;
                worksheet.Column(50).Width = 16;
                worksheet.Column(51).Width = 16;
                worksheet.Column(52).Width = 16;
                worksheet.Column(53).Width = 16;
                worksheet.Column(54).Width = 16;
                worksheet.Column(55).Width = 16;
                worksheet.Column(56).Width = 16;
                worksheet.Column(57).Width = 16;
                worksheet.Column(58).Width = 16;
                worksheet.Column(59).Width = 16;
                worksheet.Column(60).Width = 16;
                worksheet.Column(61).Width = 16;
                worksheet.Column(62).Width = 16;
                worksheet.Column(63).Width = 16;


                //worksheet.Column(55).Width = 10;
                worksheet.Cells[1, 1].Value = "ردیف";
                worksheet.Cells[1, 2].Value = "شماره ترمينال";
                worksheet.Cells[1, 3].Value = "سريال دستگاه";
                worksheet.Cells[1, 4].Value = "مالک دستگاه";
                worksheet.Cells[1, 5].Value = "psp";
                worksheet.Cells[1, 6].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 7].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 8].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 9].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 10].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 11].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 12].Value = "تعداد تراکنش قبض " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 13].Value = "جمع مبلغ تراکنش قبض " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 14].Value = "تعداد تراکنش شارژ " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 15].Value = "جمع مبلغ تراکنش شارژ " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 16].Value = "تعداد مانده گيري " + GetMonthValue.GetMonth(month) + " 1402";
                worksheet.Cells[1, 17].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 18].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 19].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 20].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 21].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 22].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 1) + " 1402";
                worksheet.Cells[1, 23].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 24].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 25].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 26].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 27].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 28].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 2) + " 1402";
                worksheet.Cells[1, 29].Value = "تعداد تراکنش خريد " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 30].Value = "مبلغ تراکنش خريد " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 31].Value = "وضعيت تعداد تراکنش " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 32].Value = "وضعيت مبلغ تراکنش " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 33].Value = "وضعيت فعاليت دستورالعمل " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 34].Value = "وضعيت فعاليت شاپرکي " + GetMonthValue.GetMonth(month - 3) + " 1402";
                worksheet.Cells[1, 35].Value = "شماره پذيرنده";
                worksheet.Cells[1, 36].Value = "شماره قرارداد";
                worksheet.Cells[1, 37].Value = "فروشگاه";
                worksheet.Cells[1, 38].Value = "شماره حساب";
                worksheet.Cells[1, 39].Value = "شبا";
                worksheet.Cells[1, 40].Value = "شماره مشتري";
                worksheet.Cells[1, 41].Value = "کد شعبه";
                worksheet.Cells[1, 42].Value = "نام شعبه";
                worksheet.Cells[1, 43].Value = "منطقه شعبه";
                worksheet.Cells[1, 44].Value = "استان بانک مرکزي";
                worksheet.Cells[1, 45].Value = "استان";
                worksheet.Cells[1, 46].Value = "شهر";
                worksheet.Cells[1, 47].Value = "نوع دستگاه";
                worksheet.Cells[1, 48].Value = "مدل دستگاه";
                worksheet.Cells[1, 49].Value = "بازاريابي توسط";
                worksheet.Cells[1, 50].Value = "کد ملي";
                worksheet.Cells[1, 51].Value = "نام";
                worksheet.Cells[1, 52].Value = "نام خانوادگي";
                worksheet.Cells[1, 53].Value = "مدير فروشکاه";
                worksheet.Cells[1, 54].Value = "وضعيت";
                worksheet.Cells[1, 55].Value = "صنف";
                worksheet.Cells[1, 56].Value = "آدرس فروشگاه";
                worksheet.Cells[1, 57].Value = "تلفن فروشگاه";
                worksheet.Cells[1, 58].Value = "موبايل";
                worksheet.Cells[1, 59].Value = "تاريخ درخواست";
                worksheet.Cells[1, 60].Value = "کد باز";
                worksheet.Cells[1, 61].Value = "تاريخ نصب";
                worksheet.Cells[1, 62].Value = "ماه نصب";
                worksheet.Cells[1, 63].Value = "تاريخ ابطال";
                worksheet.Cells[1, 64].Value = "ماه ابطال";

                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = rowNumber - 1;
                    worksheet.Cells[rowNumber, 2].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 3].Value = item.DeviceSerial;
                    worksheet.Cells[rowNumber, 4].Value = item.DeviceOwner;
                    worksheet.Cells[rowNumber, 5].Value = item.PSP;
                    worksheet.Cells[rowNumber, 6].Value = item.CountTransactionBuyCurrentMonth;
                    worksheet.Cells[rowNumber, 7].Value = item.AmountTransactionBuyCurrentMonth;
                    worksheet.Cells[rowNumber, 8].Value = item.StatusCountTransactionCurrentMonth;
                    worksheet.Cells[rowNumber, 9].Value = item.StatusAmountTransactionCurrentMonth;
                    worksheet.Cells[rowNumber, 10].Value = item.StatusActivityInstructionsCurrentMonth;
                    worksheet.Cells[rowNumber, 11].Value = item.StatusActivityShaparakCurrentMonth;
                    worksheet.Cells[rowNumber, 12].Value = item.CountTransactionBillCurrentMonth;
                    worksheet.Cells[rowNumber, 13].Value = item.SumAmountTransactionBillCurrentMonth;
                    worksheet.Cells[rowNumber, 14].Value = item.CountChargeTransactionCurrentMonth;
                    worksheet.Cells[rowNumber, 15].Value = item.SumAmountChargeTransactionCurrentMonth;
                    worksheet.Cells[rowNumber, 16].Value = item.CountBalanceCurrentMonth;
                    worksheet.Cells[rowNumber, 17].Value = item.CountTransactionBuyMonth_1;
                    worksheet.Cells[rowNumber, 18].Value = item.AmountTransactionBuyMonth_1;
                    worksheet.Cells[rowNumber, 19].Value = item.StatusCountTransactionMonth_1;
                    worksheet.Cells[rowNumber, 20].Value = item.StatusAmountTransactionMonth_1;
                    worksheet.Cells[rowNumber, 21].Value = item.StatusActivityInstructionsMonth_1;
                    worksheet.Cells[rowNumber, 22].Value = item.StatusActivityShaparakMonth_1;
                    worksheet.Cells[rowNumber, 23].Value = item.CountTransactionBuyMonth_2;
                    worksheet.Cells[rowNumber, 24].Value = item.AmountTransactionBuyMonth_2;
                    worksheet.Cells[rowNumber, 25].Value = item.StatusCountTransactionMonth_2;
                    worksheet.Cells[rowNumber, 26].Value = item.StatusAmountTransactionMonth_2;
                    worksheet.Cells[rowNumber, 27].Value = item.StatusActivityInstructionsMonth_2;
                    worksheet.Cells[rowNumber, 28].Value = item.StatusActivityShaparakMonth_2;
                    worksheet.Cells[rowNumber, 29].Value = item.CountTransactionBuyMonth_3;
                    worksheet.Cells[rowNumber, 30].Value = item.AmountTransactionBuyMonth_3;
                    worksheet.Cells[rowNumber, 31].Value = item.StatusCountTransactionMonth_3;
                    worksheet.Cells[rowNumber, 32].Value = item.StatusAmountTransactionMonth_3;
                    worksheet.Cells[rowNumber, 33].Value = item.StatusActivityInstructionsMonth_3;
                    worksheet.Cells[rowNumber, 34].Value = item.StatusActivityShaparakMonth_3;
                    worksheet.Cells[rowNumber, 35].Value = item.MerchandNumber;
                    worksheet.Cells[rowNumber, 36].Value = item.ContractNumber;
                    worksheet.Cells[rowNumber, 37].Value = item.Store;
                    worksheet.Cells[rowNumber, 38].Value = item.AccountNumber;
                    worksheet.Cells[rowNumber, 39].Value = item.Sheba;
                    worksheet.Cells[rowNumber, 40].Value = item.CustomerNumber;
                    worksheet.Cells[rowNumber, 41].Value = item.BranchCode;
                    worksheet.Cells[rowNumber, 42].Value = item.BranchName;
                    worksheet.Cells[rowNumber, 43].Value = item.BranchArea;
                    worksheet.Cells[rowNumber, 44].Value = item.CentralBankProvince;
                    worksheet.Cells[rowNumber, 45].Value = item.State;
                    worksheet.Cells[rowNumber, 46].Value = item.City;
                    worksheet.Cells[rowNumber, 47].Value = item.DeviceType;
                    worksheet.Cells[rowNumber, 48].Value = item.DeviceModel;
                    worksheet.Cells[rowNumber, 49].Value = item.MarketingBy;
                    worksheet.Cells[rowNumber, 50].Value = item.NationalCode;
                    worksheet.Cells[rowNumber, 51].Value = item.Name;
                    worksheet.Cells[rowNumber, 52].Value = item.Family;
                    worksheet.Cells[rowNumber, 53].Value = item.StoreManager;
                    worksheet.Cells[rowNumber, 54].Value = item.Status;
                    worksheet.Cells[rowNumber, 55].Value = item.business;
                    worksheet.Cells[rowNumber, 56].Value = item.StoreAddress;
                    worksheet.Cells[rowNumber, 57].Value = item.StoreTel;
                    worksheet.Cells[rowNumber, 58].Value = item.Mobile;
                    worksheet.Cells[rowNumber, 59].Value = item.RequestDate;
                    worksheet.Cells[rowNumber, 60].Value = item.OpenCode;
                    worksheet.Cells[rowNumber, 61].Value = item.InstallDate;
                    worksheet.Cells[rowNumber, 62].Value = item.InstallMonth;
                    worksheet.Cells[rowNumber, 63].Value = item.CancellationDate;
                    worksheet.Cells[rowNumber, 64].Value = item.CancellationMonth;
                    rowNumber++;

                }
                */
                #endregion Complementary Report 

                #region branchRanking

                var branchRanking = workbook.Worksheets.Add("رتبه بندی شعب");

                branchRanking.Row(1).Height = 50;
                var headerbranchRankingRowStyle = branchRanking.Row(1).Style;
                headerbranchRankingRowStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerbranchRankingRowStyle.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#022349"));
                headerbranchRankingRowStyle.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                headerbranchRankingRowStyle.Font.Bold = true;
                headerbranchRankingRowStyle.Font.Size = 12;
                headerbranchRankingRowStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                headerbranchRankingRowStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                var branchRankingcellsStyle = branchRanking.Cells.Style;
                branchRankingcellsStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                branchRankingcellsStyle.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                branchRanking.Column(1).Width = 16;
                branchRanking.Column(2).Width = 16;
                branchRanking.Column(3).Width = 16;
                branchRanking.Column(4).Width = 16;
                branchRanking.Column(5).Width = 16;
                branchRanking.Column(6).Width = 16;
                branchRanking.Column(7).Width = 16;
                branchRanking.Column(8).Width = 16;
                branchRanking.Column(9).Width = 16;
                branchRanking.Column(10).Width = 16;
                branchRanking.Column(11).Width = 16;
                branchRanking.Column(12).Width = 16;
                branchRanking.Column(13).Width = 16;
                branchRanking.Column(14).Width = 16;
                branchRanking.Column(15).Width = 16;
                branchRanking.Column(16).Width = 16;
                branchRanking.Column(17).Width = 16;
                branchRanking.Column(18).Width = 16;
                branchRanking.Column(19).Width = 16;
                branchRanking.Column(20).Width = 16;
                branchRanking.Column(21).Width = 16;
                branchRanking.Column(22).Width = 16;
                branchRanking.Column(23).Width = 16;
                branchRanking.Column(24).Width = 16;
                branchRanking.Column(25).Width = 16;
                branchRanking.Column(26).Width = 16;
                branchRanking.Column(27).Width = 16;
               


                //branchRanking.Column(55).Width = 10;
                branchRanking.Cells[1, 1].Value = "کد شعبه";
                branchRanking.Cells[1, 2].Value = "نام شعبه";
                branchRanking.Cells[1, 3].Value = "تعداد دستگاههای کارتخوان";
                branchRanking.Cells[1, 4].Value = "تعداد دستگاه منصوبه ثابت ";
                branchRanking.Cells[1, 5].Value = "تعداد دستگاه منصوبه MPOS";
                branchRanking.Cells[1, 6].Value = "تعداد دستگاه منصوبه سیار";
                branchRanking.Cells[1, 7].Value = "دستگاه ثابت (درصد)";
                branchRanking.Cells[1, 8].Value = "دستگاه سیار(درصد)";
                branchRanking.Cells[1, 9].Value = "پر تراکنش";
                branchRanking.Cells[1, 10].Value ="نسبت پر تراکنش شعبه(درصد)";
                branchRanking.Cells[1, 11].Value ="کم تراکنش";
                branchRanking.Cells[1, 12].Value = "نسبت کم تراکنش شعبه(درصد)";
                branchRanking.Cells[1, 13].Value = " فاقد تراکنش";
                branchRanking.Cells[1, 14].Value = "نسبت فاقد تراکنش شعبه(درصد)";
                branchRanking.Cells[1, 15].Value = "مجموع فاقد تراکنش و کم تراکنش (درصد)";
                branchRanking.Cells[1, 16].Value = "جمع مبلغ تراکنش ثابت";
                branchRanking.Cells[1, 17].Value = "جمع تعداد تراکنش ثابت";
                branchRanking.Cells[1, 18].Value = "جمع مبلغ تراکنش MPOS";
                branchRanking.Cells[1, 19].Value = "جمع تعداد تراکنش MPOS";
                branchRanking.Cells[1, 20].Value = "جمع مبلغ تراکنش سیار";
                branchRanking.Cells[1, 21].Value = "جمع تعداد تراکنش سیار";
                branchRanking.Cells[1, 22].Value = "جمع مبلغ تراکنش";
                branchRanking.Cells[1, 23].Value = "جمع تعداد تراکنش";
                branchRanking.Cells[1, 24].Value = "تعداد تراکنش بر پوز";
                branchRanking.Cells[1, 25].Value = "مبلغ تراکنش بر پوز";
                branchRanking.Cells[1, 26].Value = "متوسط مبلغ تراکنش بر تعداد تراکنش";
                branchRanking.Cells[1, 27].Value = "رتبه شعب براساس بالاترین متوسط مبلغ تراکنش بر پوز ";



                var   rowNumber = 2;
                var branchRanckingData = data;
                //var branchList = branchRanckingData.GroupBy(t => t.BranchCode).Select(t=>t.Key).ToList();
                var branchList = branchRanckingData.GroupBy(t => t.BranchCode).ToList();

                //var branchList= branchnames.Select(t=>t.Key).ToList();
                foreach (var item in branchList)
                {
                    var branchname= item.FirstOrDefault().BranchName;
                    branchRanking.Cells[rowNumber, 1].Value = item.Key;
                    branchRanking.Cells[rowNumber, 2].Value = branchname;
                    //تعداد دستگاههای کارتخوان
                    var cardReaderDeviceNum = data.Where(w => w.BranchName == branchname ).Count();
                    branchRanking.Cells[rowNumber, 3].Value = cardReaderDeviceNum;

                    //تعداد دستگاه منصوبه ثابت 
                    var StaticInstallDeviceNum = data.Where(w => w.BranchName == branchname).Where(w=>w.DeviceType == "Dialup" ||
                        w.DeviceType == "LAN POS" || w.DeviceType == "ثابت فروشگاهی" || w.DeviceType == "HDLC/LAN" || w.DeviceType == "Typical").Count();
                    branchRanking.Cells[rowNumber, 4].Value = StaticInstallDeviceNum;

                    //تعداد دستگاه منصوبه MPOS
                    var MPOSInstallDeviceNum = data.Where(w => w.BranchName == branchname && w.DeviceModel == "BlueTooth POS").Count();
                    branchRanking.Cells[rowNumber, 5].Value = MPOSInstallDeviceNum;

                    //تعداد دستگاه منصوبه سیار
                    var movingInstallDeviceNum = data.Where(w => w.BranchName == branchname).Where(w=> w.DeviceModel == "Gprs" && w.DeviceModel == "سیار فروشگاهی"
                    && w.DeviceModel == "AndroidPos" && w.DeviceModel == "wifi/GPRS" && w.DeviceModel == "Combo").Count();
                    branchRanking.Cells[rowNumber, 6].Value = movingInstallDeviceNum;

                    //دستگاه ثابت (درصد)`
                    branchRanking.Cells[rowNumber, 7].Value = (Convert.ToUInt16(StaticInstallDeviceNum) / Convert.ToUInt16(cardReaderDeviceNum)) * 100;

                    //دستگاه سیار(درصد)
                    branchRanking.Cells[rowNumber, 8].Value = (Convert.ToUInt16(StaticInstallDeviceNum) + Convert.ToUInt16(MPOSInstallDeviceNum)) / Convert.ToUInt16(cardReaderDeviceNum) * 100; ;

                    //پر تراکنش
                    var highTransaction = data.Where(w => w.BranchName == branchname && w.StatusActivityInstructionsCurrentMonth == "پر تراکنش").Count();

                    branchRanking.Cells[rowNumber, 9].Value = highTransaction;

                    //نسبت پر تراکنش شعبه(درصد)
                    branchRanking.Cells[rowNumber, 10].Value = (Convert.ToUInt16(highTransaction) / Convert.ToUInt16(cardReaderDeviceNum)) * 100;

                    //کم تراکنش
                    var lowTransaction = data.Where(w => w.BranchName == branchname && w.StatusActivityInstructionsCurrentMonth == "کم تراکنش").Count();
                    branchRanking.Cells[rowNumber, 11].Value = lowTransaction;


                    //نسبت کم تراکنش شعبه(درصد)
                    branchRanking.Cells[rowNumber, 12].Value = (Convert.ToUInt16(lowTransaction) / Convert.ToUInt16(cardReaderDeviceNum)) * 100;

                    // فاقد تراکنش
                    var noTransaction = data.Where(w => w.BranchName == branchname && w.StatusActivityInstructionsCurrentMonth == "فاقد تراکنش").Count();
                    branchRanking.Cells[rowNumber, 13].Value = noTransaction;

                    //نسبت فاقد تراکنش شعبه(درصد)
                    branchRanking.Cells[rowNumber, 14].Value = (Convert.ToUInt16(noTransaction) / Convert.ToUInt16(cardReaderDeviceNum)) * 100;

                    //مجموع فاقد تراکنش و کم تراکنش (درصد)
                    branchRanking.Cells[rowNumber, 15].Value =((Convert.ToUInt16(noTransaction)+ Convert.ToUInt16(lowTransaction)) / Convert.ToUInt16(cardReaderDeviceNum)) * 100;

                    //جمع مبلغ تراکنش ثابت                    
                    branchRanking.Cells[rowNumber, 16].Value = data.Where(w =>  w.BranchName == branchname).Where(w=> w.DeviceModel == "Dialup" &&
                        w.DeviceModel == "LAN POS" && w.DeviceModel == "ثابت فروشگاهی" && w.DeviceModel == "HDLC/LAN" & w.DeviceModel == "Typical").Sum(s=>Convert.ToDouble(s.AmountTransactionBuyCurrentMonth));

                    //جمع تعداد تراکنش ثابت
                    branchRanking.Cells[rowNumber, 17].Value = data.Where(w => w.BranchName == branchname).Where(w=> w.DeviceModel == "Dialup" &&
                        w.DeviceModel == "LAN POS" && w.DeviceModel == "ثابت فروشگاهی" && w.DeviceModel == "HDLC/LAN" & w.DeviceModel == "Typical").Sum(s => Convert.ToDouble(s.CountTransactionBuyCurrentMonth));

                    //جمع مبلغ تراکنش MPOS
                    branchRanking.Cells[rowNumber, 18].Value = data.Where(w => w.BranchName == branchname).Where(w=> w.DeviceModel == "BlueTooth POS").Sum(s => Convert.ToDouble(s.AmountTransactionBuyCurrentMonth));

                    //جمع تعداد تراکنش MPOS
                    branchRanking.Cells[rowNumber, 19].Value = data.Where(w => w.BranchName == branchname && w.DeviceModel == "BlueTooth POS").Sum(s => Convert.ToDouble(s.CountTransactionBuyCurrentMonth));

                    //جمع مبلغ تراکنش سیار
                    branchRanking.Cells[rowNumber, 20].Value = data.Where(w => w.BranchName == branchname).Where(w=> w.DeviceModel == "Gprs" && w.DeviceModel == "سیار فروشگاهی"
                    && w.DeviceModel == "AndroidPos" && w.DeviceModel == "wifi/GPRS" && w.DeviceModel == "Combo").Sum(s => Convert.ToDouble(s.AmountTransactionBuyCurrentMonth));

                    //جمع تعداد تراکنش سیار
                    branchRanking.Cells[rowNumber, 21].Value = data.Where(w => w.BranchName == branchname).Where(w=> w.DeviceModel == "Gprs" && w.DeviceModel == "سیار فروشگاهی"
                    && w.DeviceModel == "AndroidPos" && w.DeviceModel == "wifi/GPRS" && w.DeviceModel == "Combo").Sum(s => Convert.ToDouble(s.CountTransactionBuyCurrentMonth));

                    //جمع مبلغ تراکنش
                    var sumAmountOfTransaction = data.Where(w => w.BranchName == branchname ).Sum(s => Convert.ToDouble(s.AmountTransactionBuyCurrentMonth));
                    branchRanking.Cells[rowNumber, 22].Value = sumAmountOfTransaction;

                    //جمع تعداد تراکنش
                    var sumNumberOfTransaction = data.Where(w => w.BranchName == branchname).Sum(s => Convert.ToDouble(s.CountTransactionBuyCurrentMonth)); ;
                    branchRanking.Cells[rowNumber, 23].Value = sumNumberOfTransaction;

                    //تعداد تراکنش بر پوز
                    var countTransactionOfPos = Convert.ToInt32(sumNumberOfTransaction) / Convert.ToUInt16(cardReaderDeviceNum);
                    branchRanking.Cells[rowNumber, 24].Value = countTransactionOfPos;

                    //مبلغ تراکنش بر پوز
                    var amountTransactinOfPos = Convert.ToInt32(sumAmountOfTransaction) / Convert.ToUInt16(cardReaderDeviceNum);
                    branchRanking.Cells[rowNumber, 25].Value = amountTransactinOfPos;


                    // متوسط مبلغ تراکنش بر تعداد تراکنش
                    branchRanking.Cells[rowNumber, 26].Value = Convert.ToInt32(amountTransactinOfPos) / Convert.ToUInt16(countTransactionOfPos);

                    //رتبه شعب براساس بالاترین متوسط مبلغ تراکنش بر پوز 
                    branchRanking.Cells[rowNumber, 27].Value =0;


                }




                #endregion




                var dirPath = Server.MapPath("~/App_Data/TerminalExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"ComplementaryReport-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser, DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement, DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> ExportTransactionData(string terminalNo)
        {
            var data = await _dataContext.TransactionSums
                .Where(x => x.TerminalNo == terminalNo).Select(x => new
                {
                    x.SumPrice,
                    x.TotalCount,
                    x.BalanceCount,
                    x.PersianLocalYear,
                    x.PersianLocalMonth,
                    x.BuyTransactionCount,
                    x.BuyTransactionAmount,
                    x.BillTransactionCount,
                    x.BillTransactionAmount,
                    x.ChargeTransactionCount,
                    x.ChargeTransactionAmount
                })
                .ToListAsync();

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ داده ای جهت تهیه خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("تراکنش ها");

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

                worksheet.Column(1).Width = 30;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 30;
                worksheet.Column(4).Width = 30;
                worksheet.Column(5).Width = 30;
                worksheet.Column(6).Width = 30;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 30;
                worksheet.Column(9).Width = 30;
                worksheet.Column(10).Width = 30;
                worksheet.Column(11).Width = 30;

                worksheet.Cells[1, 1].Value = "مجموع مبلغ تراکنش ها";
                worksheet.Cells[1, 2].Value = "مجموع تعداد تراکنش ها";
                worksheet.Cells[1, 3].Value = "ماه";
                worksheet.Cells[1, 4].Value = "سال";
                worksheet.Cells[1, 5].Value = "مبلغ تراکنش های خرید";
                worksheet.Cells[1, 6].Value = "تعداد تراکنش های خرید";
                worksheet.Cells[1, 7].Value = "مبلغ تراکنش های قبض";
                worksheet.Cells[1, 8].Value = "تعداد تراکنش های قبض";
                worksheet.Cells[1, 9].Value = "مبلغ تراکنش های شارژ";
                worksheet.Cells[1, 10].Value = "تعداد تراکنش های شارژ";
                worksheet.Cells[1, 11].Value = "تعداد مانده گیری";
                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.SumPrice;
                    worksheet.Cells[rowNumber, 2].Value = item.TotalCount;
                    worksheet.Cells[rowNumber, 3].Value = item.PersianLocalMonth;
                    worksheet.Cells[rowNumber, 4].Value = item.PersianLocalYear;
                    worksheet.Cells[rowNumber, 5].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionCount;
                    worksheet.Cells[rowNumber, 7].Value = item.BillTransactionAmount;
                    worksheet.Cells[rowNumber, 8].Value = item.BillTransactionCount;
                    worksheet.Cells[rowNumber, 9].Value = item.ChargeTransactionAmount;
                    worksheet.Cells[rowNumber, 10].Value = item.ChargeTransactionCount;
                    worksheet.Cells[rowNumber, 11].Value = item.BalanceCount;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/TransactionExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Transactions-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser, DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement, DefaultRoles.CountyBranchManagement)]
        public ActionResult ExportChangeAccountRequestData(RequestSearchParameters searchParams)
        {
            searchParams.IsBranchUser = User.IsBranchUser();
            searchParams.IsSupervisionUser = User.IsSupervisionUser();
            searchParams.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            searchParams.IsCountyBranchManagment = User.IsCountyBranchManagementUser();
            searchParams.CurrentUserBranchId = CurrentUserBranchId;

            var data = _dataContext.GetChangeAccountRequestData(searchParams, false, null, null, out _);

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("درخواست های تغییر حساب");

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

                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 10;
                worksheet.Column(9).Width = 24;
                worksheet.Column(10).Width = 26;
                worksheet.Column(11).Width = 26;
                worksheet.Column(12).Width = 30;
                worksheet.Column(13).Width = 200;

                worksheet.Cells[1, 1].Value = "شماره پیگیری درخواست";
                worksheet.Cells[1, 2].Value = "شماره پیگیری پایانه";
                worksheet.Cells[1, 3].Value = "شماره پایانه";
                worksheet.Cells[1, 4].Value = "نوع دستگاه";
                worksheet.Cells[1, 5].Value = "شماره قرارداد";
                worksheet.Cells[1, 6].Value = "وضعیت درخواست";
                worksheet.Cells[1, 7].Value = "شعبه";
                worksheet.Cells[1, 8].Value = "PSP";
                worksheet.Cells[1, 9].Value = "تاریخ ثبت درخواست";
                worksheet.Cells[1, 10].Value = "شماره حساب درخواست شده";
                worksheet.Cells[1, 11].Value = "شماره حساب قدیمی";
                worksheet.Cells[1, 12].Value = "کاربر ثبت کننده درخواست";
                worksheet.Cells[1, 13].Value = "نتیجه";

                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.ChangeAccountRequestId;
                    worksheet.Cells[rowNumber, 2].Value = item.TerminalId;
                    worksheet.Cells[rowNumber, 3].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 4].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.ContractNo;
                    worksheet.Cells[rowNumber, 6].Value = item.RequestStatus;
                    worksheet.Cells[rowNumber, 7].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 8].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 9].Value = item.SubmitTime.ToPersianDateTime();
                    worksheet.Cells[rowNumber, 10].Value = item.RequestedAccountNo;
                    worksheet.Cells[rowNumber, 11].Value = item.OldAccountNo;
                    worksheet.Cells[rowNumber, 12].Value = item.SubmitterUserFullName;
                    worksheet.Cells[rowNumber, 13].Value = item.Result;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/RequestExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"ChangeAccountRequests-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.SupervisionUser, DefaultRoles.ITUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement, DefaultRoles.CountyBranchManagement)]
        public ActionResult ExportRevokeRequestData(RequestSearchParameters searchParams)
        {
            searchParams.IsBranchUser = User.IsBranchUser();
            searchParams.IsSupervisionUser = User.IsSupervisionUser();
            searchParams.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            searchParams.IsCountyBranchManagment = User.IsCountyBranchManagementUser();
            searchParams.CurrentUserBranchId = CurrentUserBranchId;

            var data = _dataContext.GetRevokeRequestData(searchParams, false, null, null, out _);

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("درخواست های جمع آوری");

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

                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 15;
                worksheet.Column(4).Width = 15;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 10;
                worksheet.Column(9).Width = 30;
                worksheet.Column(10).Width = 30;
                worksheet.Column(11).Width = 24;
                worksheet.Column(12).Width = 30;
                worksheet.Column(13).Width = 200;

                worksheet.Cells[1, 1].Value = "شماره پیگیری درخواست";
                worksheet.Cells[1, 2].Value = "شماره پیگیری پایانه";
                worksheet.Cells[1, 3].Value = "شماره پایانه";
                worksheet.Cells[1, 4].Value = "نوع دستگاه";
                worksheet.Cells[1, 5].Value = "شماره قرارداد";
                worksheet.Cells[1, 6].Value = "وضعیت درخواست";
                worksheet.Cells[1, 7].Value = "شعبه";
                worksheet.Cells[1, 8].Value = "PSP";
                worksheet.Cells[1, 9].Value = "علت";
                worksheet.Cells[1, 10].Value = "علت دوم";
                worksheet.Cells[1, 11].Value = "تاریخ ثبت درخواست";
                worksheet.Cells[1, 12].Value = "کاربر ثبت کننده درخواست";
                worksheet.Cells[1, 13].Value = "نتیجه";

                var rowNumber = 2;

                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.RevokeRequestId;
                    worksheet.Cells[rowNumber, 2].Value = item.TerminalId;
                    worksheet.Cells[rowNumber, 3].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 4].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.ContractNo;
                    worksheet.Cells[rowNumber, 6].Value = item.RequestStatus;
                    worksheet.Cells[rowNumber, 7].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 8].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 9].Value = item.ReasonTitle;
                    worksheet.Cells[rowNumber, 10].Value = item.SecondReasonTitle;
                    worksheet.Cells[rowNumber, 11].Value = item.SubmitTime.ToPersianDateTime();
                    worksheet.Cells[rowNumber, 12].Value = item.SubmitterUserFullName;
                    worksheet.Cells[rowNumber, 13].Value = item.Result;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/RequestExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"RevokeRequests-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser, DefaultRoles.SupervisionUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> ExportInstallationDelayData(DateTime fromDate, DateTime toDate, int? delay, byte? pspId, bool? justInstalledTerminals)
        {
            var data = await _dataContext.GetInstallationDelayData(fromDate, toDate, delay, pspId, CurrentUserBranchId, justInstalledTerminals, User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser());

            if (!data.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("تاخیر در نصب");
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
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 15;
                worksheet.Column(7).Width = 15;
                worksheet.Column(8).Width = 40;
                worksheet.Column(9).Width = 26;
                worksheet.Column(10).Width = 17;
                worksheet.Column(11).Width = 17;

                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = "شرکت PSP";
                worksheet.Cells[1, 3].Value = "تاریخ نصب";
                worksheet.Cells[1, 4].Value = "تاریخ بچ";
                worksheet.Cells[1, 5].Value = "میزان تاخیر";
                worksheet.Cells[1, 6].Value = "میزان تاخیر بدون تعطیلات";
                worksheet.Cells[1, 7].Value = "کد شعبه";
                worksheet.Cells[1, 8].Value = "نام شعبه";
                worksheet.Cells[1, 9].Value = "وضعیت";
                worksheet.Cells[1, 10].Value = "سیار / ثابت";
                worksheet.Cells[1, 11].Value = "نوع دستگاه";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.PersianInstallationDate;
                    worksheet.Cells[rowNumber, 4].Value = item.PersianBatchDate;
                    worksheet.Cells[rowNumber, 5].Value = item.DelayCount;
                    worksheet.Cells[rowNumber, 6].Value = item.DelayCountWithoutHoliday;
                    worksheet.Cells[rowNumber, 7].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 8].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 9].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 10].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 11].Value = item.DeviceTypeTitle;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"InstallationDelay-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ExportTerminalPm(int year, int month, byte? pspId)
        {
            var data = await _dataContext.GetTerminalPmData(pspId, year, month, false, User.Identity.GetBranchId(), User.IsBranchUser(), User.IsSupervisionUser(), User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser(), null, null);

            if (!data.Item1.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("گزارش PM");
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
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 20;

                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = "PSP";
                worksheet.Cells[1, 3].Value = "وضعیت";
                worksheet.Cells[1, 4].Value = "مدل دستگاه";
                worksheet.Cells[1, 5].Value = "سیار / ثابت";
                worksheet.Cells[1, 6].Value = "وضعیت PM";


                var rowNumber = 2;
                foreach (var item in data.Item1)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 4].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 6].Value = item.PmTime.HasValue ? "PM شده" : "PM نشده";

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Pm-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ExportTerminalEm(int year, int month, byte? pspId)
        {
            var data = await _dataContext.GetTerminalEmData(pspId, year, month, false, User.Identity.GetBranchId(), User.IsBranchUser(), User.IsSupervisionUser(), User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser(), null, null);

            if (!data.Item1.Any())
            {
                return JsonInfoMessage("هیچ موردی جهت دریافت خروجی یافت نشد.");
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("گزارش EM");
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
                worksheet.Column(2).Width = 12;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 20;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 25;
                worksheet.Column(7).Width = 25;

                worksheet.Cells[1, 1].Value = "شماره پایانه";
                worksheet.Cells[1, 2].Value = "PSP";
                worksheet.Cells[1, 3].Value = "وضعیت";
                worksheet.Cells[1, 4].Value = "مدل دستگاه";
                worksheet.Cells[1, 5].Value = "سیار / ثابت";
                worksheet.Cells[1, 6].Value = "تاریخ EM درخواستی";
                worksheet.Cells[1, 7].Value = "تاریخ EM";
                worksheet.Cells[1, 8].Value = "تفاوت تاریخ (تعداد روز)";
                worksheet.Cells[1, 9].Value = "تعداد روز تعطیلات";

                var rowNumber = 2;
                foreach (var item in data.Item1)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 4].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.IsWireless ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 6].Value = item.PersianRequestEmTime;
                    worksheet.Cells[rowNumber, 7].Value = item.PersianEmTime;
                    worksheet.Cells[rowNumber, 8].Value = item.Difference;
                    worksheet.Cells[rowNumber, 9].Value = item.HolidayCount;

                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Em-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize]
        public ActionResult DownloadReportOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/ReportExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        [CustomAuthorize]
        public ActionResult DownloadTerminalOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/TerminalExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        [CustomAuthorize]
        public ActionResult DownloadBlockDocumentOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/BlockDocumentExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        [CustomAuthorize]
        public ActionResult DownloadTransactionOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/TransactionExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        [CustomAuthorize]
        public ActionResult DownloadRequestOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/RequestExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");

        [CustomAuthorize]
        public ActionResult DownloadShebaAccountOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/TerminalExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");


        [CustomAuthorize]
        public ActionResult DownloadComplementaryReportOutputFile(string fileKey) => File(Server.MapPath($"~/App_Data/TerminalExportFiles/{fileKey}.xlsx"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileKey + ".xlsx");


    }
}