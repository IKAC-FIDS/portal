using EntityFramework.Extensions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Microsoft.Ajax.Utilities;
using OfficeOpenXml.FormulaParsing.Utilities;
using RestSharp;
using Serilog;
using Stimulsoft.Base.Json;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using TES.Merchant.Web.UI.WebTasks;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class RemovedTerminalController : BaseController
    {
        private readonly AppDataContext _dataContext;
        private string m_exePath = string.Empty;

        public RemovedTerminalController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public static string removeLeadingZeros(string str)
        {
            // Regex to remove leading
            // zeros from a string
            string regex = "^0+(?!$)";

            // Replaces the matched
            // value with given string
            str = Regex.Replace(str, regex, "");

            return str;
        }

        [HttpGet]
        [AllowAnonymous]
        public bool ReadTransaction(string fileName, string date) //14000701
        {
            //  var Dirooz = DateTime.Now.Date.AddDays(-1);
            // var PersianDirooz = Dirooz.Date.ToPersianYear().ToString() + 
            //                     (Dirooz.Date.ToPersianMonth() > 9
            //                         ? Dirooz.Date.ToPersianMonth().ToString()
            //                         : "0"
            //                     + Dirooz.Date.ToPersianMonth().ToString()) +  
            //                     Dirooz.Date.ToPersianDayOfWeek();

            var path2 = Server.MapPath("~/Job/" + fileName + ".txt");
            var linesRead = System.IO.File.ReadLines(path2);

            var readResult = new List<WageTransaction>();
            var firsrtRow = linesRead.FirstOrDefault();
            if (string.IsNullOrEmpty(firsrtRow))
                return false;

            var d = firsrtRow.Split('|');
            var y = int.Parse(d[4].Substring(0, 4));
            var m = int.Parse(d[4].Substring(5, 2));
            var terminalWageReports = _dataContext.TerminalWageReport.Where(b => b.Year == y
                && b.Month == m).ToList();
            foreach (var lineRead in linesRead)
            {
                try
                {
                    var readTransactionWageDto = new WageTransaction();
                    var data = lineRead.Split('|');
                    var rrn = data[7];
                    //CHECK DB 
                    if (data[4] != date)
                    {
                        continue;
                        //
                        // var wt =    _dataContext.WageTransaction.FirstOrDefault(b => b.RRN == rrn);
                        // if (wt != null)
                        // {
                        //     readTransactionWageDto = wt;
                        //     readResult.Add(wt);
                        // }
                    }

                    var exist = readResult.FirstOrDefault(b => b.RRN == rrn);
                    if (exist != null)
                        readTransactionWageDto = exist;

                    readTransactionWageDto.RowNumber = data[0];
                    readTransactionWageDto.TerminalNo = data[3];
                    readTransactionWageDto.Date = data[4];
                    readTransactionWageDto.Time = data[5];
                    readTransactionWageDto.RRN = data[7];
                    readTransactionWageDto.Sheba = data[23];
                    readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));
                    if (exist == null)
                        readResult.Add(readTransactionWageDto);
                    var terminalWageReport = terminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                        readTransactionWageDto.TerminalNo);
                    if (terminalWageReport != null)
                    {
                        terminalWageReport.Wage = readTransactionWageDto.WageValue;
                        terminalWageReport.Value += terminalWageReport.subValue;
                    }
                    else
                    {
                        terminalWageReport = new TerminalWageReport
                        {
                            Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                            Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                            Wage = readTransactionWageDto.WageValue,
                            TerminalNo = readTransactionWageDto.TerminalNo
                        };
                        terminalWageReport.Value = terminalWageReport.subValue;
                        terminalWageReports.Add(terminalWageReport);
                    }
                }
                catch (Exception ex)
                {
                    var readTransactionWageDto = new WageTransaction();
                    var data = lineRead.Split('|');
                    readTransactionWageDto.TerminalNo = data[4];
                    readTransactionWageDto.Error = ex.Message;
                    readTransactionWageDto.HasError = true;
                }
            }

            _dataContext.WageTransaction.AddRange(readResult);
            var res = _dataContext.SaveChanges() > 0;


            _dataContext.TerminalWageReport.AddRange(terminalWageReports);
            res = _dataContext.SaveChanges() > 0;


            return res;
        }
        [HttpGet]
        public async Task<ActionResult> ImportBiFiles(CancellationToken cancellationToken)

        {
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> CustomerReport(CancellationToken cancellationToken)

        {
            return View();
        }
        
                   
        [HttpPost]  
        public  async Task<ActionResult> CalculateResult(int year,int month)
        {
            try
            {
               
                m_exePath =Server.MapPath( "~/logs/uploadResult.txt");

                System.IO.File.AppendAllText(m_exePath, "start\n");
                
                 
                 
                var client =
                    new RestClient($"https://localhost:7072/Result/GenerateBaseTable?month={month}&year={year}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                IRestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<CalculateResultDto>(response.Content);


                    var s = 0;
                    var terminals = _dataContext.Terminals.ToList();
                    Parallel.ForEach(result.TerminalData, VARIABLE =>
                    {

                        try
                        {
                            s = s + 1;
                          
                            var terminal = terminals.FirstOrDefault(b => b.TerminalNo == VARIABLE.TerminalNo);
                            terminal.IsGood = VARIABLE.IsGood;
                            terminal.IsGoodValue = VARIABLE.IsGoodValue;
                            terminal.IsGoodMonth = month;
                            terminal.IsGoodYear = year;
                            terminal.LowTransaction = VARIABLE.LowTransaction;
                            Console.WriteLine($"==========> {s} - ");

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            VARIABLE.IsBad = true;
                           
                        }
                    });
                    System.IO.File.AppendAllText(m_exePath,  "\nbefore add range");

                    _dataContext.CalculateResults.AddRange(result.TerminalData.Select(b => new CalculateResult()
                    {
                        IsBad = b.IsBad,
                        IsGood = b.IsGood,
                        TerminalNo = b.TerminalNo,
                        IsGoodValue = b.IsGoodValue,
                        IsGoodMonth = month,
                        IsGoodYear = year,
                        LowTransaction = b.LowTransaction
                    }));
                    _dataContext.CustomerStatusResults.AddRange(result.CustomerData.Select(b =>
                        new CustomerStatusResult()
                        {

                            IsGood = b.IsGood,
                            CustomerId = b.CustomerId,
                            IsGoodMonth = month,
                            IsGoodYear = year,
                            Daramad = b.Daramd,
                            Hazineh = b.Hazineh,
                            Avg = b.Avg,
                            Special = b.Avg >= 1000000000,
                            IsGoodValue = b.IsGoodValue,
                            BranchId = !string.IsNullOrEmpty(b.BranchId)? int.Parse(  b.BranchId): 1,
                        }));
                    System.IO.File.AppendAllText(m_exePath,  "\nbefor save chagnge");

                    _dataContext.SaveChanges();
                    System.IO.File.AppendAllText(m_exePath,  "\nafter save chagnge");

                }
                Log.Information( "OK");
                return new JsonResult();
            }
            catch (Exception ex)
            {
                var t = ex.Message;
                System.IO.File.AppendAllText(m_exePath,"\n");
                System.IO.File.AppendAllText(m_exePath,t);
                System.IO.File.AppendAllText(m_exePath,"\n");
                System.IO.File.AppendAllText(m_exePath,ex.InnerException?.Message);
                if(ex.InnerException != null)
                    System.IO.File.AppendAllText(m_exePath,ex.InnerException?.InnerException?.Message);

                return new JsonResult();
                
            }
        } 
        [HttpPost]  
        public  async Task<ActionResult> UploadInstalledFile(int year,int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/ZeroShapark/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        } 
        [HttpPost]  
        public  async Task<ActionResult> UploadAvgFile(int year,int month)
        { 
            var client = new RestClient($"http://192.168.10.102:8008/ave/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        } 

        
        [HttpPost]  
        public  async Task<ActionResult> UploadMinFile(int year,int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/min/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        } 
        [HttpPost]  
        public  async Task<ActionResult> UploadWageFile(int year,int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/wage/TestUpload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        } 

        [HttpGet]
        public async Task<ActionResult> ImportWage(CancellationToken cancellationToken)

        {
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetWageUpdateData(UpdateJobDetailsViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var totalRowsCount = _dataContext.UpdateWageTask.ToList();


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
                    Error = x.HasError,
                    Date = x.Date,
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
                    x.ErrorMessage,
                    x.Date
                })
                .OrderByDescending(x => x.Id)
                .ToList();
            return JsonSuccessResult(new {rows, totalRowsCount.Count});
        }
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetCustomerData(UploadTerminalValidationDataViewModel viewModel)
        {
            
           
            var query =   _dataContext.CustomerStatusResults.AsQueryable();
         
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
                query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            }
 
             
            var data =query
                    
                .Where(b=>b.IsGoodYear == viewModel.Year && b.IsGoodMonth == viewModel.Month
                && (string.IsNullOrEmpty(viewModel.CustomerId ) ? true : viewModel.CustomerId == b.CustomerId))
                .Select(x => new CustomerStatusResultsViewModel
                {
                    Id = x.Id,
                    Month = x.IsGoodMonth,
                    Year = x.IsGoodYear,
                    IsGood = x.IsGood,
                    IsGoodValue = x.IsGoodValue,
                    CustomerId = x.CustomerId,
                    Daramad = x.Daramad,
                    Hazineh = x.Hazineh,
                    Avg = x.Avg,
                    Special = x.Special,
                   // Fullname =x.Fullname,
                  //  NationalCode = x.NationalCode
                })
                 
               
                .ToList();
      
            var tsfsafdsa = 
                string.IsNullOrEmpty(viewModel.orderClause )  ? "Month" : viewModel.orderClause.Split(' ')[0];
            
            var ascc =    string.IsNullOrEmpty(viewModel.orderClause )  ? "DESC" : viewModel.orderClause.Split(' ')[1];
            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(CustomerStatusResultsViewModel)).Find( tsfsafdsa,true) ;


            if (ascc == "DESC")
            {
                
                var   rows = data.Select(x =>  new    CustomerStatusResultsViewModel
                    {
                        Id = x.Id    ,
                        Month = x.Month,
                        Year = x.Year,
                        IsGood = x.IsGood,
                        IsGoodValue = x.IsGoodValue,
                        CustomerId = x.CustomerId,
                        Daramad = Math.Round((Double) x.Daramad, 2),
                        Hazineh = Math.Round((double) x.Hazineh, 2) ,
                        Avg = x.Avg,
                        Special = x.Special
                    })
                
                    .OrderByDescending(x=>  prop.GetValue(x))
                    .Skip(( viewModel.page.Value  - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new {rows, totalRowsCount = data.Count});

            }
            else
            {
                var   rows = data.Select(x =>  new    CustomerStatusResultsViewModel
                    {
                        Id = x.Id    ,
                        Month = x.Month,
                        Year = x.Year,
                        IsGood = x.IsGood,
                        IsGoodValue = x.IsGoodValue,
                        CustomerId = x.CustomerId,
                        Daramad =  Math.Round((Double) x.Daramad, 2) ,
                        Hazineh = Math.Round((Double) x.Hazineh, 2),
                        Avg = x.Avg,
                        Special = x.Special
                    })
                
                    .OrderBy(x=>  prop.GetValue(x))
                    .Skip(( viewModel.page.Value  - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new {rows, totalRowsCount = data.Count});

            }
            
         
        }
 
        
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetUploadTerminalValidationData(UploadTerminalValidationDataViewModel viewModel,  CancellationToken cancellationToken)
        {
            
            var client = new RestClient("http://192.168.10.102:8008/Result/GetUploadReport?year="+viewModel.Year);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            var data = JsonConvert.DeserializeObject<List<UploadTerminalValidation>>(response.Content);

            // var totalRowsCount = _dataContext.UpdateWageTask.ToList();
            //
            //
            // var path = Server.MapPath("~/Job/Result.txt");
            //
            //
            // var t = System.IO.File.ReadAllLines(path);
            // var ro = totalRowsCount
            //     .Select(x => new UpdateJobViewModel
            //     {
            //         Id = x.Id,
            //         RowNumber = x.RowNumber,
            //         ProcessedRow = string.IsNullOrEmpty(x.EndDateTime) ? t.Count() : x.RowNumber.Value,
            //         ErrorMessage = x.ErrorMessage,
            //         Start = x.StartDateTime,
            //         End = x.EndDateTime,
            //         Error = x.HasError,
            //         Date = x.Date,
            //     })
            //     .OrderByDescending(x => x.Id)
            //     
            //    
            //     .ToList();
            //
             var rows = data.Select(x => new
                 {
                     x._id,
                    x.Month,
                    x.Year,
                    x.Avg,
                    x.Min,
                    x.Wage,
                    x.Result,
                    x.Installed
                 })
                  .OrderByDescending(x => x.Month)
                 .ToList();
          return JsonSuccessResult(new {rows, data.Count});
        }
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> ImportWage(HttpPostedFileBase file
            , string date
            , CancellationToken cancellationToken)
        {
            if (_dataContext.UpdateWageTask.Any(b => b.Date == date))
                return JsonWarningMessage("قبلا بارگذاری شده است");

            var updateJob = new UpdateWageTask
            {
                StartDateTime = DateTime.Now.ToPersianDateTime(),
                Date = date
            };
            _dataContext.UpdateWageTask.Add(updateJob);
            var t = _dataContext.SaveChangesAsync(cancellationToken).Result;

            try
            {
                var fileName = file.FileName;
                var path2 = Server.MapPath("~/Job/" + fileName);
                var linesRead = System.IO.File.ReadLines(path2);
                var readResult = new List<WageTransaction>();
                var lineReads = linesRead.ToList();
                var firstRow = lineReads.LastOrDefault();
                var allTerminal = _dataContext.Terminals.Include(a => a.TerminalNo).ToList();
                double value = 0;
                double otherValue = 0;
                var terminalCount = 0;
                var otherTerminalCount = 0;
                if (string.IsNullOrEmpty(firstRow))
                    return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                var d = firstRow.Split('|');
                var y = int.Parse(d[4].Substring(0, 4));
                var m = int.Parse(d[4].Substring(5, 2));
                var OldterminalWageReports = _dataContext.TerminalWageReport.Where(b => b.Year == y
                    && b.Month == m).ToList();
                var terminalWageReports = new List<TerminalWageReport>();
                foreach (var lineRead in lineReads)
                {
                    try
                    {
                        var readTransactionWageDto = new WageTransaction();
                        var data = lineRead.Split('|');
                        var rrn = data[7];
                        //CHECK DB 
                        if (data[4] != date)
                        {
                            continue;
                        }

                        var tr = allTerminal.FirstOrDefault(b => b.TerminalNo == data[3]);
                        if (tr == null)
                        {
                            readTransactionWageDto.RowNumber = data[0];
                            readTransactionWageDto.TerminalNo = data[3];
                            readTransactionWageDto.Date = data[4];
                            readTransactionWageDto.Time = data[5];
                            readTransactionWageDto.RRN = data[7];
                            readTransactionWageDto.Sheba = data[23];
                            readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                            readResult.Add(readTransactionWageDto);
                            var terminalWageReport = OldterminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                                readTransactionWageDto.TerminalNo);
                            var terminalWageReport2 = terminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                                readTransactionWageDto.TerminalNo);
                            if (terminalWageReport != null)
                            {
                                terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                terminalWageReport.Value += terminalWageReport.subValue;
                                terminalWageReport.IsSystemTerminal = false;

                                otherValue = terminalWageReport.subValue + otherValue;
                            }
                            else if (terminalWageReport2 != null)
                            {
                                terminalWageReport = terminalWageReport2;
                                terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                terminalWageReport.Value += terminalWageReport.subValue;
                                terminalWageReport.IsSystemTerminal = false;
                                otherValue = terminalWageReport.subValue + otherValue;
                            }
                            else
                            {
                                otherTerminalCount = otherTerminalCount + 1;
                                terminalWageReport = new TerminalWageReport
                                {
                                    Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                    Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                    Wage = readTransactionWageDto.WageValue,
                                    IsSystemTerminal = false,
                                    TerminalNo = readTransactionWageDto.TerminalNo
                                };
                                terminalWageReport.Value = terminalWageReport.subValue;
                                otherValue = terminalWageReport.Value + otherValue;
                                terminalWageReports.Add(terminalWageReport);
                            }
                        }
                        else
                        {
                            readTransactionWageDto.RowNumber = data[0];
                            readTransactionWageDto.TerminalNo = data[3];
                            readTransactionWageDto.Date = data[4];
                            readTransactionWageDto.Time = data[5];
                            readTransactionWageDto.RRN = data[7];
                            readTransactionWageDto.Sheba = data[23];
                            readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                            readResult.Add(readTransactionWageDto);
                            var terminalWageReport = OldterminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                                readTransactionWageDto.TerminalNo);
                            var terminalWageReport2 = terminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                                readTransactionWageDto.TerminalNo);
                            if (terminalWageReport != null)
                            {
                                terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                terminalWageReport.Value += terminalWageReport.subValue;
                                terminalWageReport.IsSystemTerminal = true;
                                terminalWageReport.Psp = tr.Psp.Title;
                                terminalWageReport.AccountNo = tr.AccountNo;

                                value = terminalWageReport.subValue + value;
                            }
                            else if (terminalWageReport2 != null)
                            {
                                terminalWageReport = terminalWageReport2;
                                terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                terminalWageReport.Value += terminalWageReport.subValue;
                                terminalWageReport.IsSystemTerminal = true;
                                terminalWageReport.Psp = tr.Psp.Title;
                                terminalWageReport.AccountNo = tr.AccountNo;
                                value = terminalWageReport.subValue + value;
                            }
                            else
                            {
                                terminalCount = terminalCount + 1;
                                terminalWageReport = new TerminalWageReport
                                {
                                    Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                    Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                    Wage = readTransactionWageDto.WageValue,
                                    IsSystemTerminal = true,
                                    Psp = tr.Psp.Title,
                                    AccountNo = tr.AccountNo,
                                    TerminalNo = readTransactionWageDto.TerminalNo
                                };
                                terminalWageReport.Value = terminalWageReport.subValue;
                                value = terminalWageReport.Value + value;
                                terminalWageReports.Add(terminalWageReport);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var readTransactionWageDto = new WageTransaction();
                        var data = lineRead.Split('|');
                        readTransactionWageDto.TerminalNo = data[4];
                        readTransactionWageDto.Error = ex.Message;
                        readTransactionWageDto.HasError = true;
                    }
                }

                _dataContext.WageTransaction.AddRange(readResult);
                var res = _dataContext.SaveChangesAsync(cancellationToken).Result > 0;
                _dataContext.TerminalWageReport.AddRange(terminalWageReports);
                res = _dataContext.SaveChangesAsync(cancellationToken).Result > 0;
                var u2 = _dataContext.UpdateWageTask.FirstOrDefault(a => a.Id == updateJob.Id);
                u2.HasError = false;
                u2.EndDateTime = DateTime.Now.ToPersianDateTime();
                u2.RowNumber = readResult.Count;
                t = _dataContext.SaveChangesAsync(cancellationToken).Result;

                var TotalWageReport = _dataContext.TotalWageReport.FirstOrDefault(b => b.Month ==
                    m && b.Year == y);
                if (TotalWageReport == null)
                {
                    TotalWageReport = new TotalWageReport();
                    TotalWageReport.TerminalCount = terminalCount;
                    TotalWageReport.Month = m;
                    TotalWageReport.Year = y;
                    TotalWageReport.Value = value;
                    TotalWageReport.OtherValue = otherValue;
                    TotalWageReport.OtherTerminalCount = otherTerminalCount;
                    _dataContext.TotalWageReport.Add(TotalWageReport);
                    t = _dataContext.SaveChangesAsync(cancellationToken).Result;
                }
                else
                {
                    TotalWageReport.TerminalCount = TotalWageReport.TerminalCount + terminalCount;
                    TotalWageReport.Month = m;
                    TotalWageReport.Year = y;
                    TotalWageReport.Value += value;
                    TotalWageReport.OtherValue += otherValue;
                    TotalWageReport.OtherTerminalCount += otherTerminalCount;
                    t = _dataContext.SaveChangesAsync(cancellationToken).Result;
                }

                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات    کارمزد ها از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                var u = _dataContext.UpdateWageTask.FirstOrDefault(a => a.Id == updateJob.Id);
                u.HasError = true;
                u.EndDateTime = DateTime.Now.ToPersianDateTime();
                u.ErrorMessage = ex.Message;
                u.StackTrace = ex.StackTrace;
                t = _dataContext.SaveChangesAsync(cancellationToken).Result;
                return JsonErrorMessage(" بروز خطا ", ex.StackTrace);
            }
        }


        [HttpGet]
        public async Task<ActionResult> WageTest()
        {
            try
            {
                var readResult = new List<WageTransaction>();
                var lineReads = _dataContext.WageTransaction.Where(b => b.TerminalNo == "05332175").ToList();


                var allTerminal = _dataContext.Terminals.Include(a => a.Psp).ToList();
                double value = 0;
                double otherValue = 0;
                var terminalCount = 0;
                var otherTerminalCount = 0;

                var terminalWageReports = new List<TerminalWageReport>();
                foreach (var lineRead in lineReads)
                {
                    try
                    {
                        var data = lineRead;


                        var terminalWageReport = terminalWageReports.FirstOrDefault(b => b.TerminalNo ==
                            data.TerminalNo);
                        if (terminalWageReport != null)
                        {
                            terminalWageReport.Wage = data.WageValue;
                            terminalWageReport.Value += terminalWageReport.subValue;
                            terminalWageReport.IsSystemTerminal = true;


                            value = terminalWageReport.subValue + value;
                        }

                        else
                        {
                            terminalCount = terminalCount + 1;
                            terminalWageReport = new TerminalWageReport
                            {
                                Wage = data.WageValue,
                                IsSystemTerminal = true,

                                TerminalNo = data.TerminalNo
                            };
                            terminalWageReport.Value = terminalWageReport.subValue;
                            value = terminalWageReport.Value + value;
                            terminalWageReports.Add(terminalWageReport);
                        }
                    }
                    catch (Exception ex)
                    {
                        var readTransactionWageDto = new WageTransaction();
                    }
                }


                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات    کارمزد ها از طریق فایل با موفقیت انجام شد.");
            }
            catch (Exception ex)
            {
                return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات    کارمزد ها از طریق فایل با موفقیت انجام شد.");
            }
        }


        public async Task<ActionResult> DownloadMonthData(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var twr = _dataContext.TerminalWageReport.Where(x => x.Year == year && x.Month == month).ToList();


            var data = new List<WageReportResult>();

            foreach (var x in twr)
            {
                var d = new WageReportResult
                {
                    TerminalNo = x.TerminalNo,
                    Owner = (x.IsSystemTerminal.HasValue && x.IsSystemTerminal.Value),
                    Value = x.Value,
                    Psp = x.Psp,
                    AccountNo = x.AccountNo,
                };
                data.Add(d);
            }


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
                worksheet.Cells[1, 2].Value = "سیسیتمی";
                worksheet.Cells[1, 3].Value = " پذیرنده";
                worksheet.Cells[1, 4].Value = " شماره حساب";
                worksheet.Cells[1, 5].Value = " کارمزد";


                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.Owner ? "بله" : "خیر";
                    worksheet.Cells[rowNumber, 3].Value = item.Psp;
                    worksheet.Cells[rowNumber, 4].Value = item.AccountNo;
                    worksheet.Cells[rowNumber, 5].Value = item.Value;
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/ReportExportFiles");

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"TerminalWage-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();

                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));

                return JsonSuccessResult(fileKey);
            }
        }

        [HttpGet]
        public string TestAddToMongo(string terminalNo)
        {
            try
            {
                AddTerminalToMongo.Add(new TerminalMongo() {TerminalNo = terminalNo,PhoneNumber = "09126231715"});
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        
        
        
        [HttpGet]
        public async Task<ActionResult> NewBatchImportWage(int month, int year)
        {
            var dinfo = new DirectoryInfo(Server.MapPath("~/Job/"));
            var Files = dinfo.GetFiles($"{year}{month}*.txt");
            var readResult = new List<WageTransaction>();
            var allTerminal = _dataContext.Terminals;
            var terminalWageReports = new List<NewTerminalWageReport>();
            
            Console.WriteLine($"start step 1 : {DateTime.Now}");
            foreach (var file in Files)
            {
                var linesReads = System.IO.File.ReadLines(file.FullName);
                foreach (var lineRead in linesReads)
                {
                    try
                    {
                        var readTransactionWageDto = new WageTransaction();
                        var data = lineRead.Split('|');


                        readTransactionWageDto.RowNumber = data[0];
                        readTransactionWageDto.TerminalNo = data[3];
                        readTransactionWageDto.Date = data[4];
                        readTransactionWageDto.Time = data[5];
                        readTransactionWageDto.RRN = data[7];
                        readTransactionWageDto.Sheba = data[23];
                        readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                        readResult.Add(readTransactionWageDto);
                    }
                    catch (Exception ex)
                    {
                        var data = lineRead.Split('|');

                        var s = ex.Message;
                    }
                }
            }
            Console.WriteLine($"End step 1 : {DateTime.Now}");
            var terminals = readResult.GroupBy(b => b.TerminalNo);
            Console.WriteLine($"Start step 2 : {DateTime.Now}");
            foreach (var terminal in terminals)
            {
                var terminalWageReport = new NewTerminalWageReport
                {
                    TerminalNo = terminal.Key
                };
                var calculatedWage = terminal.Select(b => b.subValue).Sum();
                var wageValue = terminal.Select(b => b.WageValue).Sum();
                terminalWageReport.Value = calculatedWage;
                terminalWageReport.Wage = wageValue;
                terminalWageReport.Month = month;
                terminalWageReport.Year = year;                
                terminalWageReports.Add(terminalWageReport);
            }
            Console.WriteLine($"End step 2: {DateTime.Now}");
            Console.WriteLine($"Start step 3 : {DateTime.Now}");
            _dataContext.NewTerminalWageReport.AddRange(terminalWageReports);
            _dataContext.SaveChanges();
            
            
            var t =  _dataContext.Database.Connection.ExecuteAsync(
                "update psp.TerminalWageReport set IsSystemTerminal = 1 where TerminalNo in (select TerminalNo from psp.Terminal)"
                ).Result;
            Console.WriteLine($"End step 3: {DateTime.Now}");
            return new JsonResult();
        }

        [HttpGet]
        public async Task<ActionResult> BatchImportWage()
        {
            var days = Enumerable.Range(1, 30);
            var baddays = new List<int>();

            foreach (var VARIABLE in days)
            {
                var date = $"1400/09/{VARIABLE:00}";
                var isExist = false;
                try
                {
                    //0701

                    if (_dataContext.UpdateWageTask.Any(b => b.Date == date))
                    {
                        // isExist = true;
                        // var s  = _dataContext.UpdateWageTask.FirstOrDefault(b => b.Date == date);
                        // _dataContext.UpdateWageTask.Remove(s);
                        // _dataContext.SaveChanges();
                        continue;
                    }

                    var updateJob = new UpdateWageTask
                    {
                        StartDateTime = DateTime.Now.ToPersianDateTime(),
                        Date = date
                    };
                    _dataContext.UpdateWageTask.Add(updateJob);
                    var t = _dataContext.SaveChanges();

                    try
                    {
                        var fileName = $"140009{VARIABLE:00}.txt";
                        var path2 = Server.MapPath("~/Job/" + fileName);
                        var linesRead = System.IO.File.ReadLines(path2);
                        var readResult = new List<WageTransaction>();
                        var lineReads = linesRead.ToList();
                        var firstRow = lineReads.LastOrDefault();
                        var allTerminal = _dataContext.Terminals.Include(b => b.Psp);

                        //pu 
                        double value = 0;
                        double otherValue = 0;
                        var terminalCount = 0;
                        var otherTerminalCount = 0;
                        //////////////


                        double otherPmValue = 0;
                        double pmValue = 0;
                        var PmTerminalCount = 0;
                        var otherPmTerminalCount = 0;


                        if (string.IsNullOrEmpty(firstRow))
                            return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
                        var d = firstRow.Split('|');
                        var y = int.Parse(d[4].Substring(0, 4));
                        var m = int.Parse(d[4].Substring(5, 2));
                        var OldterminalWageReports = _dataContext.TerminalWageReport.Where(b => b.Year == y
                            && b.Month == m);
                        var terminalWageReports = new List<TerminalWageReport>();

                        foreach (var lineRead in lineReads)
                        {
                            try
                            {
                                var readTransactionWageDto = new WageTransaction();
                                var data = lineRead.Split('|');
                                var rrn = data[7];


                                if (data[9] != "PU")
                                {
                                    var temp = data[3];
                                    var tr = allTerminal.Any(b => b.TerminalNo == temp);
                                    if (!tr)
                                    {
                                        readTransactionWageDto.RowNumber = data[0];
                                        readTransactionWageDto.TerminalNo = data[3];
                                        readTransactionWageDto.Date = data[4];
                                        readTransactionWageDto.Time = data[5];
                                        readTransactionWageDto.RRN = data[7];
                                        readTransactionWageDto.Sheba = data[23];
                                        readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                                        readResult.Add(readTransactionWageDto);
                                        var terminalWageReport = OldterminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        var terminalWageReport2 = terminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        if (terminalWageReport != null)
                                        {
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            otherPmValue = terminalWageReport.subValue + otherPmValue;
                                        }
                                        else if (terminalWageReport2 != null)
                                        {
                                            terminalWageReport = terminalWageReport2;
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            otherPmValue = terminalWageReport.subValue + otherPmValue;
                                        }
                                        else
                                        {
                                            otherPmTerminalCount = otherPmTerminalCount + 1;
                                            terminalWageReport = new TerminalWageReport
                                            {
                                                Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                                Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                                Wage = readTransactionWageDto.WageValue,
                                                TerminalNo = readTransactionWageDto.TerminalNo
                                            };
                                            terminalWageReport.Value = terminalWageReport.subValue;
                                            otherPmValue = terminalWageReport.Value + otherPmValue;
                                            terminalWageReports.Add(terminalWageReport);
                                        }
                                    }
                                    else
                                    {
                                        readTransactionWageDto.RowNumber = data[0];
                                        readTransactionWageDto.TerminalNo = data[3];
                                        readTransactionWageDto.Date = data[4];
                                        readTransactionWageDto.Time = data[5];
                                        readTransactionWageDto.RRN = data[7];
                                        readTransactionWageDto.Sheba = data[23];
                                        readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                                        readResult.Add(readTransactionWageDto);
                                        var terminalWageReport = OldterminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        var terminalWageReport2 = terminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        if (terminalWageReport != null)
                                        {
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            terminalWageReport.IsSystemTerminal = true;


                                            pmValue = terminalWageReport.subValue + pmValue;
                                        }
                                        else if (terminalWageReport2 != null)
                                        {
                                            terminalWageReport = terminalWageReport2;
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            pmValue = terminalWageReport.subValue + pmValue;
                                        }
                                        else
                                        {
                                            PmTerminalCount += 1;
                                            terminalWageReport = new TerminalWageReport
                                            {
                                                Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                                Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                                Wage = readTransactionWageDto.WageValue,
                                                TerminalNo = readTransactionWageDto.TerminalNo,
                                            };
                                            terminalWageReport.Value = terminalWageReport.subValue;
                                            pmValue = terminalWageReport.Value + pmValue;
                                            terminalWageReports.Add(terminalWageReport);
                                        }
                                    }
                                }
                                else
                                {
                                    var temp = data[3];
                                    var tr = allTerminal.Any(b => b.TerminalNo == temp);
                                    if (!tr)
                                    {
                                        readTransactionWageDto.RowNumber = data[0];
                                        readTransactionWageDto.TerminalNo = data[3];
                                        readTransactionWageDto.Date = data[4];
                                        readTransactionWageDto.Time = data[5];
                                        readTransactionWageDto.RRN = data[7];
                                        readTransactionWageDto.Sheba = data[23];
                                        readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                                        readResult.Add(readTransactionWageDto);
                                        var terminalWageReport = OldterminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        var terminalWageReport2 = terminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        if (terminalWageReport != null)
                                        {
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            otherValue = terminalWageReport.subValue + otherValue;
                                        }
                                        else if (terminalWageReport2 != null)
                                        {
                                            terminalWageReport = terminalWageReport2;
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            otherValue = terminalWageReport.subValue + otherValue;
                                        }
                                        else
                                        {
                                            otherTerminalCount = otherTerminalCount + 1;
                                            terminalWageReport = new TerminalWageReport
                                            {
                                                Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                                Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                                Wage = readTransactionWageDto.WageValue,
                                                TerminalNo = readTransactionWageDto.TerminalNo
                                            };
                                            terminalWageReport.Value = terminalWageReport.subValue;
                                            otherValue = terminalWageReport.Value + otherValue;
                                            terminalWageReports.Add(terminalWageReport);
                                        }
                                    }
                                    else
                                    {
                                        readTransactionWageDto.RowNumber = data[0];
                                        readTransactionWageDto.TerminalNo = data[3];
                                        readTransactionWageDto.Date = data[4];
                                        readTransactionWageDto.Time = data[5];
                                        readTransactionWageDto.RRN = data[7];
                                        readTransactionWageDto.Sheba = data[23];
                                        readTransactionWageDto.WageValue = double.Parse(removeLeadingZeros(data[12]));

                                        readResult.Add(readTransactionWageDto);
                                        var terminalWageReport = OldterminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        var terminalWageReport2 = terminalWageReports.FirstOrDefault(b =>
                                            b.TerminalNo ==
                                            readTransactionWageDto.TerminalNo);
                                        if (terminalWageReport != null)
                                        {
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            terminalWageReport.IsSystemTerminal = true;
                                            //  terminalWageReport.Psp = tr.Psp.Title;
                                            //  terminalWageReport.AccountNo = tr.AccountNo;

                                            value = terminalWageReport.subValue + value;
                                        }
                                        else if (terminalWageReport2 != null)
                                        {
                                            terminalWageReport = terminalWageReport2;
                                            terminalWageReport.Wage = readTransactionWageDto.WageValue;
                                            terminalWageReport.Value += terminalWageReport.subValue;
                                            value = terminalWageReport.subValue + value;
                                            //  terminalWageReport.Psp = tr.Psp.Title;
                                            //    terminalWageReport.AccountNo = tr.AccountNo;
                                        }
                                        else
                                        {
                                            terminalCount = terminalCount + 1;
                                            terminalWageReport = new TerminalWageReport
                                            {
                                                Year = int.Parse(readTransactionWageDto.Date.Substring(0, 4)),
                                                Month = int.Parse(readTransactionWageDto.Date.Substring(5, 2)),
                                                Wage = readTransactionWageDto.WageValue,
                                                TerminalNo = readTransactionWageDto.TerminalNo,
                                                //    Psp = tr.Psp.Title,
                                                //   AccountNo = tr.AccountNo 
                                            };
                                            terminalWageReport.Value = terminalWageReport.subValue;
                                            value = terminalWageReport.Value + value;
                                            terminalWageReports.Add(terminalWageReport);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var readTransactionWageDto = new WageTransaction();
                                var data = lineRead.Split('|');
                                readTransactionWageDto.TerminalNo = data[4];
                                readTransactionWageDto.Error = ex.Message;
                                readTransactionWageDto.HasError = true;
                            }
                        }

                        _dataContext.WageTransaction.AddRange(readResult);
                        var res = _dataContext.SaveChanges() > 0;
                        _dataContext.TerminalWageReport.AddRange(terminalWageReports);
                        res = _dataContext.SaveChanges() > 0;
                        var u2 = _dataContext.UpdateWageTask.FirstOrDefault(a => a.Id == updateJob.Id);
                        u2.HasError = false;
                        u2.EndDateTime = DateTime.Now.ToPersianDateTime();
                        u2.RowNumber = readResult.Count;
                        t = _dataContext.SaveChanges();

                        var TotalWageReport = _dataContext.TotalWageReport.FirstOrDefault(b => b.Month ==
                            m && b.Year == y);
                        if (TotalWageReport == null)
                        {
                            TotalWageReport = new TotalWageReport();

                            TotalWageReport.Month = m;
                            TotalWageReport.Year = y;
                            TotalWageReport.Value = value;
                            TotalWageReport.OtherValue = otherValue;
                            TotalWageReport.TerminalCount = terminalCount;
                            TotalWageReport.OtherTerminalCount = otherTerminalCount;


                            TotalWageReport.PmValue = pmValue;
                            TotalWageReport.OtherPmValue = otherPmValue;
                            TotalWageReport.PmTerminalCount = PmTerminalCount;
                            TotalWageReport.OtherPmTerminalCount = otherPmTerminalCount;
                            _dataContext.TotalWageReport.Add(TotalWageReport);
                            t = _dataContext.SaveChanges();
                        }
                        else
                        {
                            TotalWageReport.Month = m;
                            TotalWageReport.Year = y;
                            TotalWageReport.Value += value;
                            TotalWageReport.OtherValue += otherValue;
                            TotalWageReport.OtherTerminalCount += otherTerminalCount;
                            TotalWageReport.TerminalCount = TotalWageReport.TerminalCount + terminalCount;


                            TotalWageReport.PmValue += pmValue;
                            TotalWageReport.OtherPmValue += otherPmValue;
                            TotalWageReport.OtherPmTerminalCount += otherPmTerminalCount;
                            TotalWageReport.PmTerminalCount = TotalWageReport.PmTerminalCount + PmTerminalCount;


                            t = _dataContext.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        var u = _dataContext.UpdateWageTask.FirstOrDefault(a => a.Id == updateJob.Id);
                        u.HasError = true;
                        u.EndDateTime = DateTime.Now.ToPersianDateTime();
                        u.ErrorMessage = ex.Message;
                        u.StackTrace = ex.StackTrace;
                        t = _dataContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    baddays.Add(VARIABLE);
                    var u = _dataContext.UpdateWageTask.FirstOrDefault(a => a.Date == date);
                    u.HasError = true;
                    u.EndDateTime = DateTime.Now.ToPersianDateTime();
                    u.ErrorMessage = ex.Message;
                    u.StackTrace = ex.StackTrace;
                    var t = _dataContext.SaveChanges();
                }
            }


            return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات    کارمزد ها از طریق فایل با موفقیت انجام شد.");
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Manage(string commaSeparatedStatuses, CancellationToken cancellationToken)
        {
            var qqq = (await _dataContext.TerminalStatus.Where(b=>b.Id == 16 || b.Id == 9)
                .Select(x => new {x.Id, x.Title})
                .ToListAsync(cancellationToken));
            var p = new
            {
                Id = (byte) 30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            // p = new
            // {
            //     Id = (byte) 31,
            //     Title = "   اماده تخصیص ( تاخیر در نصب )  "
            // };
            // qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new {x.Id, x.Title}).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte) Enums.TerminalStatus.Deleted)
                .AsQueryable();

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
                query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new {x.PersianLocalMonth, x.PersianLocalYear})
                .FirstOrDefaultAsync(cancellationToken);

            var transactionYears = Enumerable.Range(1395, lastTransaction.PersianLocalYear - 1394);
            var wageYears = Enumerable.Range(1395, DateTime.Now.ToPersianYear() - 1394);

            var dateRanges = new List<(string, string)>();
            foreach (var transactionYear in transactionYears)
            {
                dateRanges.AddRange(Enumerable
                    .Range(1,
                        lastTransaction.PersianLocalYear == transactionYear ? lastTransaction.PersianLocalMonth : 12)
                    .Select(x => ($"{x.ToString().GetMonthName()} {transactionYear}", $"{transactionYear}/{x:00}/01")));
            }

            ViewBag.TransactionDateList = dateRanges
                .OrderByDescending(x => x.Item2)
                .ToSelectList(x => x.Item2, x => x.Item1,
                    selectedValue: new[]
                        {$"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01"});

            dateRanges = new List<(string, string)>();
            foreach (var wageYear in wageYears)
            {
                dateRanges.AddRange(Enumerable
                    .Range(1,
                        12)
                    .Select(x => ($"{x.ToString().GetMonthName()} {wageYear}", $"{wageYear}/{x:00}/01")));
            }

            var now = $"{DateTime.Now.ToPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01";
            var wagetemp = dateRanges
                .OrderByDescending(x => x.Item2)
                .ToSelectList(x => x.Item2, x => x.Item1,
                    selectedValue: new[]
                        {now});

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };
            
            return View(vieModel);
        }
         [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> TestManage(string commaSeparatedStatuses, CancellationToken cancellationToken)
        {
            var qqq = (await _dataContext.TerminalStatus
                .Select(x => new {x.Id, x.Title})
                .ToListAsync(cancellationToken));
            var p = new
            {
                Id = (byte) 30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte) 31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new {x.Id, x.Title}).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte) Enums.TerminalStatus.Deleted)
                .AsQueryable();

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
                query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new {x.PersianLocalMonth, x.PersianLocalYear})
                .FirstOrDefaultAsync(cancellationToken);

            var transactionYears = Enumerable.Range(1395, lastTransaction.PersianLocalYear - 1394);
            var wageYears = Enumerable.Range(1395, DateTime.Now.ToPersianYear() - 1394);

            var dateRanges = new List<(string, string)>();
            foreach (var transactionYear in transactionYears)
            {
                dateRanges.AddRange(Enumerable
                    .Range(1,
                        lastTransaction.PersianLocalYear == transactionYear ? lastTransaction.PersianLocalMonth : 12)
                    .Select(x => ($"{x.ToString().GetMonthName()} {transactionYear}", $"{transactionYear}/{x:00}/01")));
            }

            ViewBag.TransactionDateList = dateRanges
                .OrderByDescending(x => x.Item2)
                .ToSelectList(x => x.Item2, x => x.Item1,
                    selectedValue: new[]
                        {$"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01"});

            dateRanges = new List<(string, string)>();
            foreach (var wageYear in wageYears)
            {
                dateRanges.AddRange(Enumerable
                    .Range(1,
                        12)
                    .Select(x => ($"{x.ToString().GetMonthName()} {wageYear}", $"{wageYear}/{x:00}/01")));
            }

            var now = $"{DateTime.Now.ToPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01";
            var wagetemp = dateRanges
                .OrderByDescending(x => x.Item2)
                .ToSelectList(x => x.Item2, x => x.Item1,
                    selectedValue: new[]
                        {now});

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };
            
            return View(vieModel);
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(TerminalSearchParameters searchParams, string orderByColumn,
            bool retriveTotalPageCount, int page)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Hello, world!");
            searchParams.IsBranchUser = User.IsBranchUser();
            searchParams.IsSupervisionUser = User.IsSupervisionUser();
            searchParams.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            searchParams.IsCountyBranchManagment = User.IsCountyBranchManagementUser();
            searchParams.CurrentUserBranchId = CurrentUserBranchId;

            var (rows, totalRowsCount) =
                await _dataContext.GetRemovedTerminalData(searchParams, orderByColumn, retriveTotalPageCount, page - 1, 300);

            return JsonSuccessResult(new {rows, totalRowsCount});
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Test()
        {
            var pspList = _dataContext.Psps.Select(v => v.Id).ToList();

            var pspqueue = _dataContext.Terminals
                .Where(a => a.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                .GroupBy(a => a.PspId).Select(a => new
                {
                    PspId = a.Key,
                    Count = a.Count()
                }).ToList();

            var pspcount = _dataContext.Psps.Count();
            var orderedpspqueue = pspqueue.OrderByDescending(a => a.Count).ToList();

            var pashmak = orderedpspqueue.Select(a => new
            {
                PspId = a.PspId,
                Rate = (float) ((float) (a.Count) / ((float) (orderedpspqueue.IndexOf(a) + 3) * (float) pspcount)),
                indexOf = orderedpspqueue.IndexOf(a),
                Count = (a.Count)
            }).ToList();


            return Json(pashmak);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetRequestStatus(int topiarId, int TerminalId,
            CancellationToken cancellationToken)
        {
            var viewModel = new TerminalDetailsViewModel();

            using (var parsianService = new ParsianService())
            {
                if (topiarId != 0)
                {
                    var res = parsianService
                        .UpdateStatusForRequestedTerminal(topiarId.ToString(), (int) TerminalId)
                        .Result;
                    viewModel.StateTitle = res.StatusTitle;
                    viewModel.ErrorComment = res.Error;
                    viewModel.StepCodeTitle = res.StepCodeTitle;
                    viewModel.InstallStatus = res.InstallStatus;

                    var ter = _dataContext.Terminals.FirstOrDefault(b => b.Id == TerminalId);
                    ter.StatusId = res.StatusId;
                    ter.ErrorComment = res.Error;
                    ter.InstallStatus = viewModel.InstallStatus;
                    ter.InstallStatusId = viewModel.InstallStatusId;
                    _dataContext.SaveChanges();
                    return PartialView("_TerminalStatus", viewModel);
                }
                else
                {
                    var terminalNo = _dataContext.Terminals.FirstOrDefault(b => b.Id == TerminalId).TerminalNo;
                    var res = parsianService
                            .UpdateStatusForRegisteredTerminal(terminalNo, (int) TerminalId)
                        ;
                    viewModel.StateTitle = res.Status;
                    viewModel.ErrorComment = res.Error;

                    return PartialView("_TerminalStatus", viewModel);
                }
            }
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> Details(long terminalId
            , string from, string to
            , CancellationToken cancellationToken)
        {
            var ppp = _dataContext.RemovedTerminals.Include(b=>b.RemovedMerchantProfile).FirstOrDefault(x => x.Id == terminalId);
            var ppp2 = _dataContext.RemovedTerminals.Include(b=>b.RemovedMerchantProfile).Where(x => x.Id == terminalId).ToList();

            var nationalCode = _dataContext.RemovedMerchantProfile.Where(b => b.Id == ppp.RemovedMerchantProfileId)
                .Select(z => z.NationalCode).FirstOrDefault();
            var ids = _dataContext.RemovedMerchantProfile.Where(b => b.NationalCode == nationalCode).Select(z => z.Id)
                .ToList();
            var tt = _dataContext.RemovedTerminals.Where(b => ids.Contains(b.RemovedMerchantProfileId) &&
                                                              b.StatusId != (byte) Enums.TerminalStatus.Deleted
                                                              && b.StatusId != (byte) Enums.TerminalStatus.Revoked);
            var s = tt.ToList();
            var ParsianTerminals = tt.Where(b => b.PspId == 3).Count();
            var FanavaTerminals = tt.Where(b => b.PspId == 1).Count();
            var IrankishTerminals = tt.Where(b => b.PspId == 2).Count();

            var viewModel =  new TerminalDetailsViewModel ()
                {
                    Id = ppp.Id,
                    Tel = ppp.Tel,
                    PspId = ppp.PspId,
                    Title = ppp.Title,
                    ParsianTerminals = ParsianTerminals,
                    FanavaTerminals = FanavaTerminals,
                    IrankishTerminals = IrankishTerminals,
                   UserId = ppp.UserId,
                                     CityId = ppp.CityId,
                                     TerminalId = ppp.Id,
                                     GuildId =ppp.GuildId,
                                     Address =ppp.Address,
                                     ShebaNo =ppp.ShebaNo,
                                     TelCode =ppp.TelCode,
                                     PostCode =ppp.PostCode,
                                     BranchId =ppp.BranchId,
                                     StatusId =ppp.StatusId,
                                     PspTitle =ppp.Psp?.Title,
                                     AccountNo =ppp.AccountNo,
                                     BatchDate =ppp.BatchDate,
                                     CityTitle =ppp.City.Title,
                                     MarketerId =ppp.MarketerId,
                                     ContractNo =ppp.ContractNo,
                                     TerminalNo =ppp.TerminalNo,
                                     MerchantNo =ppp.MerchantNo,
                                     RevokeDate =ppp.RevokeDate,
                                     SubmitTime =ppp.SubmitTime,
                                     StatusTitle = ppp.Status.Title,
                                     BranchTitle = ppp.Branch.Title,
                                     ErrorComment =ppp.ErrorComment ?? "",
                                     ContractDate =ppp.ContractDate,
                                     DeviceTypeId =ppp.DeviceTypeId,
                                     StateTitle = ppp.City.State.Title,
                                     MarketerTitle = ppp.Marketer.Title,
                                     LastUpdateTime = ppp.LastUpdateTime,
                                     Mobile = ppp.RemovedMerchantProfile.Mobile,
                                     BlockDocumentStatusId = ppp.BlockDocumentStatusId, 
                   HomeTel = ppp.RemovedMerchantProfile.HomeTel,
                   
                   
                    DeviceTypeTitle =ppp.DeviceType.Title,
                    InstallationDate = ppp.InstallationDate,
                    IsMale =ppp.RemovedMerchantProfile.IsMale,
                    LastName =ppp.RemovedMerchantProfile.LastName,
                    SubmitterUserFullName = ppp.User.FullName,
                    MerchantProfileId = ppp.RemovedMerchantProfileId,
                    Birthdate = ppp.RemovedMerchantProfile.Birthdate,
                    FirstName = ppp.RemovedMerchantProfile.FirstName,
                    ActivityTypeTitle = ppp.ActivityType.Title,
                    
                    
                    FatherName =ppp.RemovedMerchantProfile.FatherName,
                    HomeAddress = ppp.RemovedMerchantProfile.HomeAddress,
                    GenderTitle = ppp.RemovedMerchantProfile.IsMale ? "مرد" : "زن",
                    HomePostCode = ppp.RemovedMerchantProfile.HomePostCode,
                    NationalCode =ppp.RemovedMerchantProfile.NationalCode,
                    ShaparakAddressFormat =  ppp.ShaparakAddressFormat,
                    IdentityNumber = ppp.RemovedMerchantProfile.IdentityNumber,
                    RegionalMunicipalityId = ppp.RegionalMunicipalityId,
                    EnglishLastName = ppp.RemovedMerchantProfile.EnglishLastName,
                    EnglishFirstName = ppp.RemovedMerchantProfile.EnglishFirstName,
                    NationalityTitle = ppp.RemovedMerchantProfile.Nationality.Title,
                    SignatoryPosition = ppp.RemovedMerchantProfile.SignatoryPosition != null ? ppp.RemovedMerchantProfile.SignatoryPosition : "",
                    LegalNationalCode =ppp.RemovedMerchantProfile.LegalNationalCode,
                    IsLegalPersonality = ppp.RemovedMerchantProfile.IsLegalPersonality,
                    BlockAccountNumber = ppp.BlockAccountNumber,
                    BlockDocumentDate = ppp.BlockDocumentDate,
                    BlockDocumentNumber = ppp.BlockDocumentNumber,
                    BlockPrice = ppp.BlockPrice,
                    TopiarId =ppp.TopiarId,
                    StepCode =ppp.StepCode,
                    NewParsian = ppp.NewParsian,
                    CustomerCategoryId =ppp.CustomerCategoryId != null ?   ppp.CustomerCategoryId.Value : 0, 
                    StepCodeTitle = ppp.StepCodeTitle ??"",
                    CustomerCategory = ppp.CustomerCategory != null ? ppp.CustomerCategory.Name : "",
                    InstallStatus = ppp.InstallStatus??"",
                    InstallStatusId = ppp.InstallStatusId,
                    PreferredPspTitle = ppp.PreferredPsp?.Title,
                    TaxPayerCode = ppp.TaxPayerCode,
                    OrgaNizationId = ppp.BranchId,
                    CompanyRegistrationDate = ppp.RemovedMerchantProfile.CompanyRegistrationDate,
                    LegalPersonalityTitle = ppp.RemovedMerchantProfile.IsLegalPersonality ? "حقوقی" : "حقیقی",
                    CompanyRegistrationNumber = ppp.RemovedMerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = ppp.RemovedMerchantProfile.BirthCertificateIssueDate,
                    RegionalMunicipalityTitle = ppp.RegionalMunicipalityId != null ? 
                       (  ppp.RegionalMunicipalityId.HasValue ? ppp.RegionalMunicipality?.Title : string.Empty ) : "",
                           GuildTitle = ppp.Guild.ParentId.HasValue
                               ? ppp.Guild.Parent.Title + " / " + ppp.Guild.Title
                               : ppp.Guild.Title ,
                    TerminalDocuments = ppp.TerminalDocuments.Select(y => new DocumentViewModel
                    {
                        Id = y.Id,
                        FileName = y.FileName,
                        DocumentTypeTitle = y.DocumentType.Title
                   }),
                     MerchantProfileDocuments = ppp.RemovedMerchantProfile.MerchantProfileDocuments.Select(y =>
                         new DocumentViewModel
                       {
                           Id = y.Id,
                           FileName = y.FileName,
                             DocumentTypeTitle = y.DocumentType.Title
                       })
                }
                ;

            if (from != null)
            {
                var fromMonth = int.Parse(from.Substring(5, 2));
                var fromYear = int.Parse(from.Substring(0, 4));

                var toMonth = int.Parse(to.Substring(5, 2));
                var toYear = int.Parse(to.Substring(0, 4));

                if (!string.IsNullOrEmpty(viewModel.TerminalNo))
                    viewModel.Wage = _dataContext.TerminalWageReport.FirstOrDefault(b =>
                        b.TerminalNo == viewModel.TerminalNo &&
                        b.Month >= fromMonth && b.Year >= fromYear &&
                        b.Month <= toMonth && b.Year <= toYear
                    )?.Value;
            }

            ViewBag.PspList = (await _dataContext.Psps
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.PspId});

            ViewBag.CustomerCategory = (await _dataContext.CustomerCategory
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.PspId});


            

            return PartialView("_Details", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Details(AllocatePspViewModel viewModel, CancellationToken cancellationToken)
        {
            var terminal = await _dataContext.RemovedTerminals.FirstOrDefaultAsync(
                x => x.Id == viewModel.Id && x.StatusId == (byte) Enums.TerminalStatus.New, cancellationToken);

            if (terminal == null)
            {
                return JsonWarningMessage(
                    "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' است امکان تایید یا عدم تایید دارند");
            }

            if (viewModel.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
            {
                if (terminal.StatusId != (byte) Enums.TerminalStatus.New &&
                    terminal.StatusId != (byte) Enums.TerminalStatus.NeedToReform)
                {
                    return JsonWarningMessage(
                        "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' یا 'نیازمند اصلاح' است امکان تایید دارند");
                }

                terminal.PspId = viewModel.PspId;
                await _dataContext.SaveChangesAsync(cancellationToken);

                return AddAcceptor(terminal.Id, terminal.PspId)
                    ? JsonSuccessMessage()
                    : JsonSuccessMessage(MessageType.Danger,
                        "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
            }

            if (viewModel.StatusId == (byte) Enums.TerminalStatus.NeedToReform)
            {
                terminal.ErrorComment = viewModel.ErrorComment;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Edit(long terminalId, CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals.Where(x => x.Id == terminalId &&
                                                          (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                                           x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                                           x.StatusId == (byte) Enums.TerminalStatus
                                                               .UnsuccessfulReturnedFromSwitch));

            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            }

            var viewModel = await query
                .Select(x => new MerchantDataEntryViewModel
                {
                    Tel = x.Tel,
                    Title = x.Title,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    TelCode = x.TelCode,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    StatusId = x.StatusId,
                    BranchId = x.BranchId,
                    PostCode = x.PostCode,
                    AccountNo = x.AccountNo,
                    StateId = x.City.StateId,
                    MarketerId = x.MarketerId,
                    DeviceTypeId = x.DeviceTypeId,
                    EnglishTitle = x.EnglishTitle,
                    ParentGuildId = x.Guild.ParentId,
                    Mobile = x.MerchantProfile.Mobile,
                    ActivityTypeId = x.ActivityTypeId,
                    HomeTel = x.MerchantProfile.HomeTel,
                    LastName = x.MerchantProfile.LastName,
                    IsMale = x.MerchantProfile.IsMale,
                    MerchantProfileId = x.MerchantProfileId,
                    FirstName = x.MerchantProfile.FirstName,
                    Birthdate = x.MerchantProfile.Birthdate,
                    FatherName = x.MerchantProfile.FatherName,
                    HomeAddress = x.MerchantProfile.HomeAddress,
                    HomePostCode = x.MerchantProfile.HomePostCode,
                    NationalCode = x.MerchantProfile.NationalCode,
                    NationalityId = x.MerchantProfile.NationalityId,
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                    IdentityNumber = x.MerchantProfile.IdentityNumber,
                    EnglishLastName = x.MerchantProfile.EnglishLastName,
                    EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                    SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                    EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                    IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                    CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                    CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                    LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                    AccountBranchCode = x.AccountNo.Substring(0, 4),
                    AccountCustomerNumber = x.AccountNo.Substring(9, 8),
                    BlockAccountType = x.BlockAccountNumber.Substring(0, 3),
                    BlockAccountRow = x.BlockAccountNumber.Substring(18, 3),
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    TaxPayerCode = x.TaxPayerCode
                })
                .FirstOrDefaultAsync(x => x.TerminalId == terminalId, cancellationToken);

            if (viewModel == null || viewModel.MarketerId == (long) Enums.Marketer.BankOrBranch &&
                !User.IsBranchUser() && !User.IsAcceptorsExpertUser())
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.MarketerId});

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}", selectedValue: new[] {viewModel.BranchId});

            ViewBag.GuildList = await _dataContext.Guilds
                .Where(x => !x.ParentId.HasValue)
                .OrderByDescending(x => x.IsActive)
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
                .ToListAsync(cancellationToken);

            ViewBag.ActivityTypeList = (await _dataContext.ActivityTypes.Where(b => b.Id != 3)
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.ActivityTypeId});

            ViewBag.NationalityList = (await _dataContext.Nationalities
                    .Select(x => new NationalityViewModel
                    {
                        Id = x.Id,
                        Title = x.Title
                    })
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = await _dataContext.States
                .Select(x => new StateViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Cities = x.Cities.Select(y => new CityViewModel
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            ViewBag.DeviceTypeList = await _dataContext.DeviceTypes
                .Where(x => x.IsActive)
                .Select(x => new DeviceTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    BlockPrice = x.BlockPrice
                })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken);

            ViewBag.AddressComponentList = await _dataContext.AddressComponents
                .Select(x => new AddressComponentViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    PrefixTypeCode = x.PrefixTypeCode,
                    PriorityNumber = x.PriorityNumber
                })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken);

            ViewBag.DocumentTypeList = await _dataContext.DocumentTypes
                .Where(x => !x.IsForLegalPersonality.HasValue ||
                            x.IsForLegalPersonality == viewModel.IsLegalPersonality)
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
                .ToListAsync(cancellationToken);

            var previouslyUploadedMerchantProfileDocuments = await _dataContext.MerchantProfileDocuments
                .Where(x => x.MerchantProfileId == viewModel.MerchantProfileId)
                .Select(x => new UploadedDocumentViewModel
                {
                    DocumentId = x.Id, DocumentTypeTitle = x.DocumentType.Title,
                    ForEntityTypeId = (long) Enums.EntityType.MerchantProfile
                })
                .ToListAsync(cancellationToken);

            var previouslyUploadedTerminalDocuments = await _dataContext.TerminalDocuments
                .Where(x => x.TerminalId == viewModel.TerminalId)
                .Select(x => new UploadedDocumentViewModel
                {
                    DocumentId = x.Id, DocumentTypeTitle = x.DocumentType.Title,
                    ForEntityTypeId = (long) Enums.EntityType.Terminal
                })
                .ToListAsync(cancellationToken);

            viewModel.PreviouslyUploadedDocuments =
                previouslyUploadedMerchantProfileDocuments.Concat(previouslyUploadedTerminalDocuments);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Edit(MerchantDataEntryViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!viewModel.PostCode.IsValidPostCode())
            {
                return JsonErrorMessage("کد پستی محل پذیرنده صحیح نمی باشد");
            }

            if (string.IsNullOrEmpty(viewModel.ShaparakAddressFormat) ||
                viewModel.ShaparakAddressFormat.Split('،').Length < 2)
            {
                return JsonWarningMessage(
                    "ثبت آدرس پذیرنده الزامی می‌باشد. توجه نمایید که آدرس بایستی حداقل دارای دو بخش باشد.");
            }

            if (viewModel.BirthCertificateIssueDate >= DateTime.Today)
            {
                return JsonWarningMessage("تاریخ صدور شناسنامه بایستی کوچکتر از تاریخ امروز باشد.");
            }

            var terminal = await _dataContext.Terminals
                .Include(x => x.DeviceType)
                .FirstAsync(x => x.Id == viewModel.TerminalId &&
                                 x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                                 (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                  x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                  x.StatusId == (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch),
                    cancellationToken);

            var branchLimitations = await _dataContext.CheckBranchLimitations(CurrentUserBranchId);

            var selectedDeviceTypeInfo = await _dataContext.DeviceTypes.Where(x => x.Id == viewModel.DeviceTypeId)
                .Select(x => new {x.BlockPrice, x.IsWireless}).FirstAsync(cancellationToken);

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                if (!await _dataContext.TerminalDocuments.AnyAsync(
                        x => x.TerminalId == viewModel.TerminalId &&
                             x.DocumentTypeId == (byte) Enums.DocumentType.SanadMasdoodi, cancellationToken) &&
                    viewModel.PostedFiles.Any(x =>
                        x.DocumentTypeId == (byte) Enums.DocumentType.SanadMasdoodi &&
                        !x.PostedFile.IsValidFormat(".pdf")))
                {
                    return JsonWarningMessage(
                        "لطفاً فایل سند مسدودی را انتخاب نمایید. توجه نمایید که این فایل بایستی با فرمت pdf ارسال شود.");
                }

                if (await _dataContext.Terminals.AnyAsync(
                        x => x.StatusId != (byte) Enums.TerminalStatus.Deleted && x.Id != viewModel.TerminalId &&
                             x.BlockDocumentNumber == viewModel.BlockDocumentNumber, cancellationToken))
                {
                    return JsonWarningMessage("شماره سند مسدودی وارد شده تکراری است.");
                }
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && !x.PostedFile.IsValidFormat(".pdf")))
            {
                return JsonWarningMessage("تنها فرمت قابل قبول برای مدارک pdf می باشد.");
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && x.PostedFile.ContentLength > 1070016))
            {
                return JsonWarningMessage("حجم هر کدام از مدارک ارسال شده نباید بیشتر از 1 مگابایت باشد.");
            }

            if (string.IsNullOrEmpty(viewModel.Address) || viewModel.Address.Length > 100)
            {
                return JsonWarningMessage("آدرس پذیرنده نباید کمتر از یک کاراکتر و بیشتر از 100 کاراکتر باشد");
            }

            if (!terminal.DeviceType.IsWireless && selectedDeviceTypeInfo.IsWireless && branchLimitations.Item2)
            {
                return JsonWarningMessage("امکان تغییر نوع دستگاه از ثابت به سیار برای شعبه شما غیرفعال می باشد");
            }

            AccountNumberExtensions.TryGenerateAccountNumberFromSheba(terminal.ShebaNo, out var accountNumber);

            terminal.Tel = viewModel.Tel;
            terminal.Title = viewModel.Title;
            terminal.CityId = viewModel.CityId;
            terminal.GuildId = viewModel.GuildId;
            terminal.Address = viewModel.Address;
            terminal.TelCode = viewModel.TelCode;
            terminal.PostCode = viewModel.PostCode;
            terminal.EnglishTitle = viewModel.EnglishTitle;
            terminal.DeviceTypeId = viewModel.DeviceTypeId;
            terminal.ActivityTypeId = viewModel.ActivityTypeId;
            terminal.ShaparakAddressFormat = viewModel.ShaparakAddressFormat;
            terminal.RegionalMunicipalityId = viewModel.RegionalMunicipalityId;
            terminal.TaxPayerCode = viewModel.TaxPayerCode;
            terminal.MarketerId = User.IsBranchUser() ? (int) Enums.Marketer.BankOrBranch : viewModel.MarketerId;

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                terminal.BlockDocumentDate = viewModel.BlockDocumentDate;
                terminal.BlockDocumentNumber = viewModel.BlockDocumentNumber;
                terminal.BlockPrice = selectedDeviceTypeInfo.BlockPrice;
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.WaitingForReview;
                terminal.BlockAccountNumber =
                    $"{accountNumber.Split('-')[0]}-{viewModel.BlockAccountType}-{accountNumber.Split('-')[2]}-{viewModel.BlockAccountRow}";
            }
            else
            {
                terminal.BlockPrice = 0;
                terminal.BlockDocumentDate = null;
                terminal.BlockAccountNumber = null;
                terminal.BlockDocumentNumber = null;
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.NotRegistered;
            }

            var terminalDocumentTypesToRemove = viewModel.PostedFiles
                .Where(x => x.ForEntityTypeId == (int) Enums.EntityType.Terminal && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.TerminalDocuments.RemoveRange(_dataContext.TerminalDocuments.Where(x =>
                terminalDocumentTypesToRemove.Contains(x.DocumentTypeId) && x.TerminalId == terminal.Id));

            foreach (var item in viewModel.PostedFiles.Where(x =>
                         x.ForEntityTypeId == (int) Enums.EntityType.Terminal && x.PostedFile.IsValidFile()))
            {
                terminal.TerminalDocuments.Add(new TerminalDocument
                {
                    DocumentTypeId = item.DocumentTypeId,
                    FileData = item.PostedFile.ToByteArray(),
                    FileName = item.PostedFile.FileName,
                    ContentType = item.PostedFile.ContentType
                });
            }

            #region MyRegion

            var merchantProfile =
                await _dataContext.MerchantProfiles.FirstAsync(x => x.Id == viewModel.MerchantProfileId,
                    cancellationToken);
            merchantProfile.NationalityId = viewModel.NationalityId;
            merchantProfile.SignatoryPosition = viewModel.SignatoryPosition;
            merchantProfile.BirthCertificateIssueDate = viewModel.BirthCertificateIssueDate;

            var merchantProfileDocumentTypesToRemove = viewModel.PostedFiles
                .Where(x => x.ForEntityTypeId == (int) Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.MerchantProfileDocuments.RemoveRange(_dataContext.MerchantProfileDocuments.Where(x =>
                merchantProfileDocumentTypesToRemove.Contains(x.DocumentTypeId) &&
                x.MerchantProfileId == terminal.MerchantProfileId));

            #endregion

            foreach (var item in viewModel.PostedFiles.Where(x =>
                         x.ForEntityTypeId == (int) Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile()))
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

            var canSendTopPsp = _dataContext.DocumentTypes.Where(b => b.SendToPsp.HasValue && b.SendToPsp.Value)
                .ToList();

            //todo for parsian
            using (var parsianService = new ParsianService())
            {
                if (terminal.NewParsian.HasValue && terminal.NewParsian.Value)
                {
                    var attach = viewModel.PostedFiles.Where(b => b.PostedFile != null).Where(b =>
                        canSendTopPsp.Select(a => a.Id).Contains(
                            b.DocumentTypeId)).Select(b => new UploadAttachmentRequestData
                    {
                        ContentType = b.PostedFile.ContentType,
                        FileName = b.PostedFile.FileName,
                        Base64 = Convert.ToBase64String(b.PostedFile.ToByteArray())
                    }).ToList();


                    parsianService.NewAddAcceptor(terminal.Id, attach);
                    var res = parsianService
                        .UpdateStatusForRequestedTerminal(terminal.TopiarId.Value.ToString(), (int) terminal.Id).Result;
                    terminal.InstallStatus = res.InstallStatus;
                    terminal.InstallStatusId = res.InstallStatusId;
                    terminal.InstallationDate = res.InstallationDate;
                    terminal.StepCode = res.StepCode;
                    terminal.StepCodeTitle = res.StepCodeTitle;
                    terminal.ErrorComment = res.Error;
                    terminal.StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                    var t = _dataContext.SaveChanges();
                }
            }

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long terminalId, CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals.Where(x => x.Id == terminalId);
            Terminal terminal;

            if (User.IsAdmin())
            {
                terminal = await query.FirstOrDefaultAsync(cancellationToken);
                if (terminal.StatusId == (byte) Enums.TerminalStatus.New ||
                    terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                    terminal.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                {
                    terminal.StatusId = (byte) Enums.TerminalStatus.Deleted;
                    await _dataContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    return JsonWarningMessage(
                        "تنها وضعیت های 'ورود بازاریابی'، 'برنگشته از سوئیچ' و 'نیازمند اصلاح' قابلیت حذف دارند.");
                }

                return JsonSuccessMessage();
            }

            query = query.Where(x => x.StatusId == (byte) Enums.TerminalStatus.New);

            terminal = await query.FirstAsync(cancellationToken);
            terminal.StatusId = (byte) Enums.TerminalStatus.Deleted;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        private static readonly object ConfirmLock = new object();

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public ActionResult Confirm(long terminalId)
        {
            lock (ConfirmLock)
            {
                var terminal = _dataContext.Terminals
                    .Where(x => x.Id == terminalId && (x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                                       x.StatusId == (byte) Enums.TerminalStatus
                                                           .UnsuccessfulReturnedFromSwitch))
                    .Select(x => new
                    {
                        x.Id, x.StatusId, x.PspId, x.ContractNo, x.MerchantProfileId , x.TopiarId 
                        ,PersianCharRefId = x.MerchantProfile.PersianCharRefId
                        ,MerchantProfieNatoionalCode = x.MerchantProfile.NationalCode,
                        IdentityNumber = x.MerchantProfile.IdentityNumber , BirthCrtfctSeriesNumber = x.MerchantProfile.BirthCrtfctSeriesNumber
                    })
                    .FirstOrDefault();

                if (terminal == null)
                    return JsonWarningMessage("پایانه مورد نظر یافت نشد.");

                if (!terminal.PspId.HasValue)
                    return JsonWarningMessage("برای پذیرنده انتخاب نشده است و امکان ثبت درخواست وجود ندارد.");

                bool result;
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.IranKish)
                {
                    using (var irankishService = new NewIranKishService())
                        result = irankishService.EditAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.PardakhNovin)
                {
                    var result2 = new UpdateRequestByFollowUpCodeResponse();
                    using (var pardakhtNovinService = new PardakhtNovinService())
                         result2 = pardakhtNovinService.EditAcceptor(terminalId);

                    return result2.Status == PardakthNovinStatus.Successed && result2.SavedID != 0
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.Fanava)
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&  !terminal.TopiarId.HasValue &&
                    terminal.PspId == (byte) Enums.PspCompany.Parsian)
                {
                    using (var parsianService = new ParsianService())
                    {
                        var files = _dataContext.MerchantProfileDocuments
                            .Where(b => b.MerchantProfileId == terminal.MerchantProfileId &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()
                            .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList();

                        files.AddRange(_dataContext.TerminalDocuments
                            .Where(b => b.Id == terminal.Id &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList().Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList());


                        result = parsianService.NewAddAcceptor(terminalId, files);
                    }

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                   
                }  
                
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform && terminal.TopiarId.HasValue &&
                    terminal.PspId == (byte) Enums.PspCompany.Parsian)
                {
                    using (var parsianService = new ParsianService())
                    {
                        var files = _dataContext.MerchantProfileDocuments
                            .Where(b => b.MerchantProfileId == terminal.MerchantProfileId &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()
                            .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList();

                        files.AddRange(_dataContext.TerminalDocuments
                            .Where(b => b.Id == terminal.Id &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList().Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList());


                        var requestInqueryInput = new RequestChangeInfoInput2();
                        requestInqueryInput.RequestData = new RequestChangeInfoInputData2();
                        requestInqueryInput.RequestCode = (int) terminal.Id;
                        requestInqueryInput.RequestData.ChangeInfoTypeRefId = 31286;//required
                        requestInqueryInput.RequestData.PersonTypeRefId = 31220;//required
                        requestInqueryInput.RequestData.NationalCode = terminal.MerchantProfieNatoionalCode;
                
               
                
                        requestInqueryInput.RequestData.BirthCertificateNumber = 
                            terminal.IdentityNumber == "0" ? terminal.MerchantProfieNatoionalCode : terminal.IdentityNumber ;//شماره شناسنامه
              
                
                       
                        using (var parsianService2 = new ParsianService())
                        {
                            requestInqueryInput.RequestData.PersianCharRefId =
                                parsianService2.GetPersianCharRefId(terminal.PersianCharRefId);//بخش حرفی سری شناسنامه
                        }

                        requestInqueryInput.RequestData.BirthCertificateSeriesNumber = terminal.BirthCrtfctSeriesNumber;// عددی سری شناسنامه
                     
                        
                    var    resultt = parsianService.RequestChangeInfo(requestInqueryInput);
                    return resultt.IsSuccess
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");

                    }

                
                }

                if (terminal.PspId == (byte) Enums.PspCompany.Fanava &&
                    terminal.StatusId == (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.EditAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                return JsonErrorMessage();
            }
        }


        [HttpGet]
        public ActionResult NewSendRevokeRequest(string TerminalNo)
        {
            using (var parsianService = new ParsianService())
            {
                var q = parsianService.NewSendRevokeRequest(1, TerminalNo, 2, 1);

                return Json(q, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult LoginTest()
        {
            using (var parsianService = new ParsianService())
            {
                var q = parsianService.Maraz();

                return Json(q, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateBadData()
        {
            using (var parsianService = new ParsianService())
            {
                parsianService.UpdateBadData();

                return Json("OK", JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult TestRevokeQuery(string terminalnumber)
        {
            using (var parsianService = new ParsianService())
            {
              
                var requestInqueryInput = new RequestInqueryInput();
                requestInqueryInput.RequestData = new RequestInqueryRequestData();
                requestInqueryInput.RequestData.TopiarId = "6954253";
                var result = parsianService.RequestInQuery(requestInqueryInput, 499871);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Faranegin()
        {
            var mp = _dataContext.MerchantProfiles.Include(t=>t.Terminals).Where(b => string.IsNullOrEmpty(b.PersianCharRefId)).ToList();

            foreach (var merchantProfile in mp)
            {
                if(!merchantProfile.Terminals.Any())
                    continue;
                var shebaNumber = merchantProfile.Terminals.FirstOrDefault().ShebaNo;
                if(string.IsNullOrEmpty(shebaNumber))
                    continue;
                
                AccountNumberExtensions.TryGenerateAccountNumberFromSheba(shebaNumber, out var accountNumber);
                var primaryCustomerNumber = accountNumber.Split('-')[2];

                if (!TosanService.TryGetCustomerInfo(primaryCustomerNumber,
                        merchantProfile.CustomerNumber ?? primaryCustomerNumber, out var response,
                        out var errorMessage))
                {
                    continue;
                }

                var incompleteCustomerInfoMessage = TosanService.GetIncompleteCustomerInfoMessage(response);
                if (!string.IsNullOrEmpty(incompleteCustomerInfoMessage))
                {
                    continue;
                }


                
                merchantProfile.BirthCrtfctSerial = response.certificateSerial;
                if (response.certificateSeries != null)
                {
                    merchantProfile.BirthCrtfctSeriesNumber = !string.IsNullOrEmpty(  response.certificateSeries  )  ?
                    response.certificateSeries.Split('-')[1] :null;
                    merchantProfile.PersianCharRefId  = !string.IsNullOrEmpty(  response.certificateSeries  ) ?
                        response.certificateSeries.Split('-')[0] : null;
                }

                _dataContext.SaveChanges();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        
        public int GetPersianCharRefId(string terminalInfoPersianCharRefId)
        {
            switch (terminalInfoPersianCharRefId)
            {
                case "الف":
                    return 1152071;
                case "ب":
                    return 1152072;
                case "ل":
                    return 1152073;
                case "د":
                    return 1152074;
                case "ر":
                    return 1152075;
                case "1":
                    return 1152076;
                case "2":
                    return 1152077;
                case "3":
                    return 1152078;
                case "4":
                    return 1152079;
                case "9":
                    return 1152080;
                case "10":
                    return 1152081;
                case "11":
                    return 1152082;
                case "":
                    return 0;
                default:
                    return 1152071;
                
            }
        }


        [HttpGet]
        public ActionResult RequestChangeInfo()
        {
            using (var parsianService = new ParsianService())
            {
                if (!TosanService.TryGetCustomerInfo("02085006",
                        "02085006", out var response, out var errorMessage))
                {
                    return JsonErrorMessage(errorMessage);
                }

                var requestInqueryInput = new RequestChangeInfoInput2();
                requestInqueryInput.RequestData = new RequestChangeInfoInputData2();
                requestInqueryInput.RequestCode =  36;
                requestInqueryInput.RequestData.ChangeInfoTypeRefId = 31286;
                requestInqueryInput.RequestData.PersonTypeRefId = 31220;
                requestInqueryInput.RequestData.NationalCode = "0084471565";
                requestInqueryInput.RequestData.BirthCertificateNumber = "0084471565"; //شماهر شناسنامه
                requestInqueryInput.RequestData.BirthCertificateSeriesNumber = ! string .IsNullOrEmpty(response.certificateSeries)?
                    response.certificateSeries.Split('-')[1] : null;
                
                requestInqueryInput.RequestData.PersianCharRefId = GetPersianCharRefId( ! string .IsNullOrEmpty(response.certificateSeries)?
                    response.certificateSeries.Split('-')[0] : "");
                
                if(string .IsNullOrEmpty(response.certificateSeries))
                  requestInqueryInput.RequestData.PersianCharRefId =    parsianService.GetPersianCharRefId( response.certificateSeries.Split('-')[0] );
                var result = parsianService.RequestChangeInfo(requestInqueryInput);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult QueryTest(string topidid)
        {
            using (var parsianService = new ParsianService())
            {
                var sd = parsianService.UpdateStatusForRequestedTerminal("3921297", int.Parse("495181"));
                return Json(sd, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> GroupConfirm()
        {
            var pspList = await _dataContext.Psps
                .Select(x => new {x.Id, x.Title})
                .ToListAsync();

            ViewBag.PspList = pspList.ToSelectList(x => x.Id, x => x.Title);

            return View("_GroupConfirm");
        }

        private static readonly object GroupConfirmLock = new object();

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public ActionResult GroupConfirm(List<long> terminalIdList, byte pspId)
        {
            lock (GroupConfirmLock)
            {
                var validTerminals = _dataContext.Terminals.Where(x =>
                    terminalIdList.Contains(x.Id) && x.StatusId == (byte) Enums.TerminalStatus.New);
                validTerminals.Update(x => new Terminal {PspId = pspId});

                if (validTerminals.Any())
                {
                    var validTerminalsIdList = validTerminals.Select(x => new
                    {
                        x.Id,
                        x.MerchantProfileId
                    }).ToList();
                    switch (pspId)
                    {
                        case (byte) Enums.PspCompany.Fanava:
                        {
                            using (var fanavaService = new FanavaService())
                                fanavaService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }

                        case (byte) Enums.PspCompany.PardakhNovin:
                        {
                            using (var pardakhtNovinService = new PardakhtNovinService())
                                pardakhtNovinService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }

                        case (byte) Enums.PspCompany.IranKish:
                        {
                            using (var irankishService = new NewIranKishService())
                                irankishService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }

                        case (byte) Enums.PspCompany.Parsian:
                        {
                            using (var parsianService = new ParsianService())
                            {
                                foreach (var terminal in validTerminalsIdList)
                                {
                                    var files = _dataContext.MerchantProfileDocuments
                                        .Where(b => b.MerchantProfileId == terminal.MerchantProfileId &&
                                                    (b.DocumentType.SendToPsp.HasValue &&
                                                     b.DocumentType.SendToPsp.Value))
                                        .ToList().Select(b => new UploadAttachmentRequestData
                                        {
                                            ContentType = b.ContentType,
                                            FileName = b.FileName,
                                            Base64 = Convert.ToBase64String(b.FileData)
                                        })
                                        .ToList();

                                    files.AddRange(_dataContext.TerminalDocuments
                                        .Where(b => b.Id == terminal.Id && (b.DocumentType.SendToPsp.HasValue &&
                                                                            b.DocumentType.SendToPsp.Value))
                                        .ToList().Select(b => new UploadAttachmentRequestData
                                        {
                                            ContentType = b.ContentType,
                                            FileName = b.FileName,
                                            Base64 = Convert.ToBase64String(b.FileData)
                                        })
                                        .ToList());


                                    parsianService.NewAddAcceptor(terminal.Id, files);
                                }
                            }

                            break;
                        }
                    }
                }

                return JsonSuccessMessage();
            }
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public ActionResult UpdateBatchDate()
        {
            
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> UpdateBatchDate(HttpPostedFileBase file, CancellationToken cancellationToken)
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
                        var batchDate = row[rowNumber, 2].Text.ToNullableMiladiDate();
                        updateCommandList.Add(
                            $"UPDATE psp.Terminal SET BatchDate = '{batchDate}' WHERE TerminalNo = '{terminalNo}'");
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

            return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات پایانه ها از طریق فایل با موفقیت انجام شد.");
        }

        //[HttpPost]
        //[AjaxOnly]
        //[CustomAuthorize(DefaultRoles.Administrator)]
        //public async Task<ActionResult> ToggleVip(long terminalId)
        //{
        //    var terminal = await _dataContext.Terminals.FirstAsync(x => x.Id == terminalId);

        //    var currentBranchInstalledTerminalCount = await _dataContext.Terminals.CountAsync(x => x.BranchId == CurrentUserBranchId && x.StatusId == (byte)Enums.TerminalStatus.Installed);
        //    var currentBranchVipTerminalCount = await _dataContext.Terminals.CountAsync(x => x.BranchId == CurrentUserBranchId && x.IsVip);
        //    var maxVipTerminal = currentBranchInstalledTerminalCount * 0.1 > 10 ? 10 : currentBranchInstalledTerminalCount * 0.1;

        //    if (!terminal.IsVip && currentBranchVipTerminalCount + 1 > maxVipTerminal)
        //    {
        //        return JsonWarningMessage("شما به حداکثر میزان دستگاه های ویژه رسیده اید و امکان افزودن دستگاه بیشتر به عنوان دستگاه ویژه وجود ندارد");
        //    }

        //    terminal.IsVip = !terminal.IsVip;
        //    await _dataContext.SaveChangesAsync();

        //    return JsonSuccessMessage(terminal.IsVip ? "دستگاه به لیست دستگاه های ویژه افزوده شد" : "دستگاه از لیست دستگاه های ویژه خارج شد");
        //}

        private bool AddAcceptor(long terminalId, byte? pspId)
        {
            var result = false;

            var terminal = _dataContext.Terminals.FirstOrDefault(b => b.Id == terminalId);
            switch (pspId)
            {
                case (byte) Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);
                    break;
                }
                case (byte) Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                        var rresult = pardakhtNovinService.AddAcceptor(terminalId);
                        if (rresult.Status == PardakthNovinStatus.Successed)
                            result = true;
                        else
                        {
                            result = false;
                        }
                    }
                    break;
                }

                case (byte) Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = irankishService.AddAcceptor(terminalId);
                    break;
                }

                case (byte) Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                    {
                        var files = new List<UploadAttachmentRequestData>();
                        files = _dataContext.MerchantProfileDocuments
                            .Where(b => b.MerchantProfileId == terminal.MerchantProfileId &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()
                            .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList();

                        var ttt = new List<UploadAttachmentRequestData>();


                        ttt = _dataContext.TerminalDocuments
                            .Where(b => b.Id == terminal.Id &&
                                        (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()
                            .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            })
                            .ToList();
                        files.AddRange(ttt);


                        result = parsianService.NewAddAcceptor(terminalId, files);
                    }

                    break;
                }
            }

            return result;
        }
    }
}