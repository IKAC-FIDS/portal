using EntityFramework.Extensions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dapper;
using iTextSharp.text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Portal.IService;
using RestSharp;
using Serilog;
using StackExchange.Exceptional;
using Stimulsoft.Base.Json;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.IranKishServiceRefrence;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Merchant.Web.UI.ViewModels.newirankish;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using TES.Merchant.Web.UI.WebTasks;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TerminalController : BaseController
    {
        private readonly AppDataContext _dataContext;
        private string m_exePath = string.Empty;

        public IBasicService _basicService;
        private ICustomerService _CustomerService;

        public TerminalController(AppDataContext dataContext, IBasicService basicService,
            ICustomerService customerService)
        {
            _dataContext = dataContext;
            _basicService = basicService;
            _CustomerService = customerService;
        }

        private static string RemoveLeadingZeros(string str)
        {
            // Regex to remove leading
            // zeros from a string
            var regex = "^0+(?!$)";

            // Replaces the matched
            // value with given string
            str = Regex.Replace(str, regex, "");

            return str;
        }

        [HttpGet]
        [AllowAnonymous]
        public string TestIrankish()
        {
           
            
               var irankishService = new IranKishService();
               var isUp = irankishService.IsUp();
               var  m_exePath = Server.MapPath("~/logs/myapp.txt");
               Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .WriteTo.File(m_exePath, rollingInterval: RollingInterval.Minute)
                   .CreateLogger();
               Log.Information( JsonConvert.SerializeObject(isUp));

               return isUp.FirstOrDefault()?.ActivityCode;



        }
        [HttpGet]
        [AllowAnonymous]
        public bool ReadTransaction(string fileName, string date) //14000701
        {
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
                    readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));
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
        public async Task<ActionResult> CustomerReport(CancellationToken cancellationToken, bool? IsSpecial = null,
            bool? IsGood = null)

        {
            ViewBag.IsSpecial = IsSpecial;
            ViewBag.IsGood = IsGood;
            var t = new PersianCalendar();

            var date = t.GetYear(DateTime.Now).ToString("0000") + "/" + t.GetMonth(DateTime.Now).ToString("00") + "/" +
                       t.GetDayOfMonth(DateTime.Now).ToString("00");

            if (t.GetMonth(DateTime.Now) == 1)
            {
                ViewBag.LastMonth = 12;
            }
            else if (t.GetMonth(DateTime.Now) == 2)
            {
                ViewBag.LastMonth = 1;
            }
            else
            {
                ViewBag.LastMonth = t.GetMonth(DateTime.Now) - 1;
            }


            if (IsSpecial.HasValue && IsSpecial.Value)
            {
                ViewBag.Title = "مشتریان ویژه";
            }
            else if (IsGood.HasValue && IsGood.Value)
            {
                ViewBag.Title = "مشتریان زیان ده";
            }
            else
            {
                ViewBag.Title = "     اطلاعات ماهیانه مشتریان";
            }


            return View();
        }


        [HttpPost]
        public async Task<ActionResult> CalculateResult(int year, int month, int wirelessRent, int nonWirelessRent)
        {
            try
            {
                m_exePath = Server.MapPath("~/logs/uploadResult.txt");
                System.IO.File.AppendAllText(m_exePath, "start\n");

                var client =
                    new RestClient(
                        $"http://localhost:5072/Result/GenerateBaseTable?month={month}&year={year}&wirelessRent={wirelessRent}" +
                        $"&nonWirelessRent={nonWirelessRent}");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                System.IO.File.AppendAllText(m_exePath, $"response + {JsonConvert.SerializeObject(response)}\n");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = JsonConvert.DeserializeObject<CalculateResultDto>(response.Content);


                    var jjjjj = _dataContext.CalculateResults
                        .Where(b => b.IsGoodYear == year && b.IsGoodMonth == month)
                        .ToList();

                    _dataContext.CalculateResults.RemoveRange(jjjjj);
                    _dataContext.SaveChanges();

                    var aaaa = _dataContext.CustomerStatusResults
                        .Where(a => a.IsGoodYear == year && a.IsGoodMonth == month)
                        .ToList();
                    _dataContext.CustomerStatusResults.RemoveRange(aaaa);
                    _dataContext.SaveChanges();


                    var s = 0;
                    var terminals = _dataContext.Terminals.ToList();

                    Parallel.ForEach(terminals, terminal =>
                    {
                        terminal.IsGood = null;
                        terminal.IsGoodMonth = null;
                        terminal.IsGoodYear = null;
                        terminal.IsGoodValue = null;
                        terminal.LowTransaction = null;
                    });

                    Parallel.ForEach(result.TerminalData, VARIABLE =>
                    {
                        try
                        {
                            s += 1;
                            var terminal = terminals.FirstOrDefault(b => b.TerminalNo == VARIABLE.TerminalNo);
                            terminal.IsGood = VARIABLE.IsGood;
                            terminal.IsGoodValue = VARIABLE.IsGoodValue;
                            terminal.IsGoodMonth = month;
                            terminal.IsGoodYear = year;
                            terminal.IsActive = VARIABLE.IsActive;
                            terminal.TransactionCount = VARIABLE.TransactionCount;
                            terminal.TransactionValue = VARIABLE.TransactionValue;
                            terminal.LowTransaction = VARIABLE.LowTransaction;
                            Console.WriteLine($"==========> {s} - ");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            VARIABLE.IsBad = true;
                        }
                    });
                    System.IO.File.AppendAllText(m_exePath, "\nbefore add range");

                    _dataContext.CalculateResults.AddRange(result.TerminalData.Select(b => new CalculateResult()
                    {
                        IsBad = b.IsBad,
                        IsGood = b.IsGood,
                        TerminalNo = b.TerminalNo,
                        IsGoodValue = b.IsGoodValue,
                        IsGoodMonth = month,
                        IsInNetwork = b.IsInNetwork,
                        IsGoodYear = year,
                        BranchId = b.BranchId != null ? long.Parse(b.BranchId) : 0,
                        AccountNumber = b.AccountNo,
                        IsActive = b.IsActive,
                        p_hazineh_soodePardakty = b.p_hazineh_soodePardakty,
                        p_hazineh_rent = b.p_hazineh_rent,
                        p_hazineh_karmozdShapark = b.p_hazineh_karmozdShapark,
                        p_hazineh_hashiyeSood = b.p_hazineh_hashiyeSood,
                        p_daramad_Vadie = b.p_daramad_Vadie,
                        p_daramad_Moadel = b.p_daramad_Moadel,
                        p_daramd_Tashilat = b.p_daramd_Tashilat,
                        TransactionValue = b.TransactionValue,
                        TransactionCount = b.TransactionCount,
                        PspTitle = b.PspTitle,
                        PspId = b.PspId,
                        LowTransaction = b.LowTransaction,
                    }).ToList());
                    _dataContext.CustomerStatusResults.AddRange(result.CustomerData.Select(b =>
                        new CustomerStatusResult()
                        {
                            IsGood = b.IsGood,
                            CustomerId = b.CustomerId,
                            IsGoodMonth = month,
                            IsGoodYear = year,
                            Daramad = b.Daramd,
                            Hazineh = b.Hazineh,
                            TransactionCount = b.TransactionCount,
                            TransactionValue = b.TransactionValue,
                            Avg = b.Avg,
                            Special = b.Avg >= 1000000000,
                            IsGoodValue = b.IsGoodValue,
                            BranchId = !string.IsNullOrEmpty(b.BranchId) ? int.Parse(b.BranchId) : 1,
                        }).ToList());
                    System.IO.File.AppendAllText(m_exePath, "\nbefor save chagnge");

                    _dataContext.SaveChanges();
                    System.IO.File.AppendAllText(m_exePath, "\nafter save chagnge");
                }

                Log.Information("OK");
                return new JsonResult();
            }
            catch (Exception ex)
            {
                var t = ex.Message;
                System.IO.File.AppendAllText(m_exePath, "\n");
                System.IO.File.AppendAllText(m_exePath, t);
                System.IO.File.AppendAllText(m_exePath, "\n");
                System.IO.File.AppendAllText(m_exePath, ex.InnerException?.Message);
                if (ex.InnerException != null)
                    System.IO.File.AppendAllText(m_exePath, ex.InnerException?.InnerException?.Message);

                return new JsonResult();
            }
        }


        [HttpGet]
        public ActionResult LowTransaction(bool? TwoMonthInActive, bool? ThreeMonthInActive)
        {
            var qqq = (_dataContext.TerminalStatus.Where(b => b.Id != 16 && b.Id != 9)
                .Select(x => new { x.Id, x.Title })
                .ToList());
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            if (TwoMonthInActive.HasValue && TwoMonthInActive.Value)
            {
                ViewBag.Title = "مشتریان زیان ده در دو ماه متوالی";
            }

            if (ThreeMonthInActive.HasValue && ThreeMonthInActive.Value)
            {
                ViewBag.Title = "مشتریان زیان ده در سه ماه متوالی";
            }

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.MarketerList = (_dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (_dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (_dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (_dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToList())
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (_dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
                .FirstOrDefault();

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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = "",
                NeedToReformTerminalCount = query.Count(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
            };
            vieModel.ThreeMonthInActive = ThreeMonthInActive.HasValue ? ThreeMonthInActive.Value : false;
            vieModel.TwoMonthInActive = TwoMonthInActive.HasValue ? TwoMonthInActive.Value : false;
            return View(vieModel);
        }

        [HttpGet]
        public ActionResult OutOfNetworkTerminals()
        {
            var qqq = (_dataContext.TerminalStatus.Where(b => b.Id != 16 && b.Id != 9)
                .Select(x => new { x.Id, x.Title })
                .ToList());
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.MarketerList = (_dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (_dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (_dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (_dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToList())
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (_dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
                .FirstOrDefault();

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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = "",
                NeedToReformTerminalCount = query.Count(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
            };

            return View(vieModel);
        }

        [HttpGet]
        public ActionResult BiResultByParameters(bool? IsInNetwork)
        {
            var qqq = (_dataContext.TerminalStatus.Where(b => b.Id != 16 && b.Id != 9)
                .Select(x => new { x.Id, x.Title })
                .ToList());
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.MarketerList = (_dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (_dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (_dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (_dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToList())
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (_dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
                .FirstOrDefault();

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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = "",
                NeedToReformTerminalCount = query.Count(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
            };

            return View(vieModel);
        }

        [HttpPost]
        public async Task<ActionResult> BiResultByParameters(BiByParametersSearch searchParameters,
            string orderByColumn,
            bool retriveTotalPageCount, int page)
        {
            var direction = "ASC";
            if (string.IsNullOrEmpty(orderByColumn))
            {
                orderByColumn = "TerminalNo";
            }
            else
            {
                direction = orderByColumn.Split(' ')[1];
                orderByColumn = orderByColumn.Split(' ')[0];
            }


            Func<dynamic, dynamic> orderingFunction = i =>
                orderByColumn;

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

            var totalRowsCount = rows.Count;
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

                totalRowsCount = rows.Count;
            }

            if (direction == "ASC")
                return JsonSuccessResult(new
                {
                    rows = rows.OrderBy(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            else
            {
                return JsonSuccessResult(new
                {
                    rows = rows.OrderByDescending(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            }
        }


        [HttpPost]
        public async Task<ActionResult> OutOfNetworkTerminals(BiByParametersSearch searchParameters,
            string orderByColumn,
            bool retriveTotalPageCount, int page)
        {
            var direction = "ASC";
            if (string.IsNullOrEmpty(orderByColumn))
            {
                orderByColumn = "TerminalNo";
            }
            else
            {
                direction = orderByColumn.Split(' ')[1];
                orderByColumn = orderByColumn.Split(' ')[0];
            }


            Func<dynamic, dynamic> orderingFunction = i =>
                orderByColumn;

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

            var rowss = s.ToList();


            var rows = new List<ResultParameters>();
            foreach (var row in rowss)
            {
                try
                {
                    if (!searchParameters.p_daramad_Moadel)
                    {
                        row.p_daramad_Moadel = 0;
                    }

                    if (!searchParameters.p_daramad_Vadie)
                    {
                        row.p_daramad_Vadie = 0;
                    }

                    if (!searchParameters.p_daramd_Tashilat)
                    {
                        row.p_daramd_Tashilat = 0;
                    }

                    var Daramad = row.p_daramad_Moadel + row.p_daramd_Tashilat + row.p_daramad_Vadie;

                    if (!searchParameters.p_hazineh_karmozdShapark)
                    {
                        row.p_hazineh_karmozdShapark = 0;
                    }

                    if (!searchParameters.p_hazineh_hashiyeSood)
                    {
                        row.p_hazineh_hashiyeSood = 0;
                    }

                    if (!searchParameters.p_hazineh_rent)
                    {
                        row.p_hazineh_rent = 0;
                    }

                    if (!searchParameters.p_hazineh_soodePardakty)
                    {
                        row.p_hazineh_soodePardakty = 0;
                    }

                    var Hazine = row.p_hazineh_soodePardakty + row.p_hazineh_rent +
                                 row.p_hazineh_hashiyeSood + row.p_hazineh_karmozdShapark;
                    row.IsGoodValue = Daramad - Hazine;
                    row.IsGood = row.IsGoodValue > 0;

                    var a = new ResultParameters
                    {
                        TerminalNo = row.TerminalNo,
                        Daramad = Math.Round(Daramad, 2),
                        Hazine = Math.Round(Hazine, 2),
                        IsGood = row.IsGood,
                        Title = row.Title,
                        TransactionValue = row.TransactionValue,
                        TransactionCount = row.TransactionCount,
                        AccountNumber = row.AccountNumber,
                        PspTitle = row.PspTitle,
                        PspId = row.PspId,
                        IsGoodValue = row.IsGoodValue.HasValue ? Math.Round(row.IsGoodValue.Value, 2) : 0
                    };
                    rows.Add(a);
                }


                catch (Exception ex)
                {
                    var a = new ResultParameters
                    {
                        TerminalNo = row.TerminalNo,
                        Daramad = 0,
                        Hazine = 0,
                        IsGood = false,
                        IsGoodValue = 0
                    };
                    rows.Add(a);
                }
            }

            var totalRowsCount = rows.Count;
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

                totalRowsCount = rows.Count;
            }

            if (direction == "ASC")
                return JsonSuccessResult(new
                {
                    rows = rows.OrderBy(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            else
            {
                return JsonSuccessResult(new
                {
                    rows = rows.OrderByDescending(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult> LowTransaction(BiByParametersSearch searchParameters,
            string orderByColumn,
            bool retriveTotalPageCount, int page)
        {
            var direction = "ASC";
            if (string.IsNullOrEmpty(orderByColumn))
            {
                orderByColumn = "TerminalNo";
            }
            else
            {
                direction = orderByColumn.Split(' ')[1];
                orderByColumn = orderByColumn.Split(' ')[0];
            }

            var prop = typeof(CalculateResult).GetProperty("orderByColumn");

            Func<dynamic, dynamic> orderingFunction = i =>
                orderByColumn;

            var rowss = new List<ResultParameters>();
            var t = new PersianCalendar();

            var date = t.GetYear(DateTime.Now).ToString("0000") + "/" + t.GetMonth(DateTime.Now).ToString("00") + "/" +
                       t.GetDayOfMonth(DateTime.Now).ToString("00");

            var y1 = 1400;
            var y2 = 1400;
            var y3 = 1400;
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

                var oooo = _dataContext.CalculateResults.Where(a => a.IsGood.HasValue && !a.IsGood.Value
                        &&
                        string.IsNullOrEmpty(searchParameters.TerminalNo)
                            ? true
                            : a.TerminalNo == searchParameters.TerminalNo
                    )
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
                            rowss.Add(new ResultParameters()
                            {
                                IsGood = false,
                                IsGoodValue = Math.Round((Double)daramad - Hazine, 2),
                                Daramad = Math.Round((Double)daramad, 2),
                                Hazine = Math.Round((Double)Hazine, 2),
                                TerminalNo = VARIABLE.Key,
                                CustomerId = VARIABLE.FirstOrDefault().CustomerId
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


                var oooo = _dataContext.CalculateResults.Where(a => a.IsGood.HasValue && !a.IsGood.Value
                        &&
                        string.IsNullOrEmpty(searchParameters.TerminalNo)
                            ? true
                            : a.TerminalNo == searchParameters.TerminalNo
                    )
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
                                rowss.Add(new ResultParameters()
                                {
                                    IsGood = false,
                                    IsGoodValue = Math.Round((Double)daramad - Hazine, 2),
                                    Daramad = Math.Round((Double)daramad, 2),
                                    Hazine = Math.Round((Double)Hazine, 2),
                                    TerminalNo = VARIABLE.Key,
                                    CustomerId = VARIABLE.FirstOrDefault().CustomerId
                                });
                            }
                }
            }


            var totalRowsCount = rowss.Count;
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

                totalRowsCount = rowss.Count;
            }

            if (direction == "ASC")
                return JsonSuccessResult(new
                {
                    rows = rowss.OrderBy(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            else
            {
                return JsonSuccessResult(new
                {
                    rows = rowss.OrderByDescending(orderByColumn).Skip(page - 1).Take(30).ToList(),
                    totalRowsCount = totalRowsCount
                });
            }
        }

        [HttpGet]
        public ActionResult Inquiry()
        {
            ViewBag.PspList = _basicService.GetPspList().Select(x => new { x.Id, x.Title })
                .ToSelectList(x => x.Id, x => x.Title);

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> UploadInstalledFile(int year, int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/ZeroShapark/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        }

        [HttpPost]
        public async Task<ActionResult> UploadAvgFile(int year, int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/ave/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        }


        [HttpPost]
        public async Task<ActionResult> UploadMinFile(int year, int month)
        {
            var client = new RestClient($"http://192.168.10.102:8008/min/Upload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            return new JsonResult();
        }

        [HttpPost]
        public async Task<ActionResult> UploadWageFile(int year, int month)
        {
          //  var client = new RestClient($"http://localhost:5072/wage/TestUpload?month={month}&year={year}");
            var client = new RestClient($"http://192.168.10.102:8008/wage/TestUpload?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
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
            return JsonSuccessResult(new { rows, totalRowsCount.Count });
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetCustomerData(UploadTerminalValidationDataViewModel viewModel)
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
                    IsGoodValue = Math.Round((Double)x.Daramad, 2) - Math.Round((Double)x.Hazineh, 2),
                    CustomerId = x.CustomerId,
                    Daramad = Math.Round((Double)x.Daramad, 2),
                    Hazineh = Math.Round((Double)x.Hazineh, 2),
                    Avg = x.Avg,
                    Special = x.Special,
                })
                .ToList();

            var tsfsafdsa =
                string.IsNullOrEmpty(viewModel.orderClause) ? "Month" : viewModel.orderClause.Split(' ')[0];

            var ascc = string.IsNullOrEmpty(viewModel.orderClause) ? "DESC" : viewModel.orderClause.Split(' ')[1];
            var prop = TypeDescriptor.GetProperties(typeof(CustomerStatusResultsViewModel))
                .Find(tsfsafdsa, true);


            if (ascc == "DESC")
            {
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
                    .OrderByDescending(x => prop.GetValue(x))
                    .Skip((viewModel.page.Value - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new { rows, totalRowsCount = data.Count });
            }
            else
            {
                var rows = data.Select(x => new CustomerStatusResultsViewModel
                    {
                        Id = x.Id,
                        Month = x.Month,
                        Year = x.Year,
                        IsGood = x.IsGood,
                        IsGoodValue = x.IsGoodValue,
                        CustomerId = x.CustomerId,
                        Daramad = x.Daramad,
                        Hazineh = x.Hazineh,
                        Avg = x.Avg,
                        Special = x.Special
                    })
                    .OrderBy(x => prop.GetValue(x))
                    .Skip((viewModel.page.Value - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new { rows, totalRowsCount = data.Count });
            }
        }


        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetUploadTerminalValidationData(UploadTerminalValidationDataViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var client = new RestClient("http://192.168.10.102:8008/Result/GetUploadReport?year=" + viewModel.Year);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = client.Execute(request);

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
            return JsonSuccessResult(new { rows, data.Count });
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
                            readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
                            readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
                AddTerminalToMongo.Add(new TerminalMongo() { TerminalNo = terminalNo, PhoneNumber = "09123437620" });
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
                        readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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


            var t = _dataContext.Database.Connection.ExecuteAsync(
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
                                        readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
                                        readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
                                        readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
                                        readTransactionWageDto.WageValue = double.Parse(RemoveLeadingZeros(data[12]));

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
            // var qqq = (await _dataContext.TerminalStatus.Where(b => b.Id != 16 && b.Id != 9)
            //     .Select(x => new {x.Id, x.Title})
            //     .ToListAsync(cancellationToken));
            // var p = new
            // {
            //     Id = (byte) 30,
            //     Title = "فاقد سند مسدودی"
            // };
            // qqq.Add(p);
            // p = new
            // {
            //     Id = (byte) 31,
            //     Title = "   اماده تخصیص ( تاخیر در نصب )  "
            // };
            // qqq.Add(p);
            //
            // ViewBag.StatusList = qqq
            //     .ToSelectList(x => x.Id, x => x.Title,
            //         selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());
            //
            // ViewBag.MarketerList = (await _dataContext.Marketers
            //         .Select(x => new {x.Id, x.Title})
            //         .ToListAsync(cancellationToken))
            //     .ToSelectList(x => x.Id, x => x.Title);
            //
            // ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
            //         .Where(x => x.IsActive)
            //         .Select(x => new {x.Id, x.Title})
            //         .ToListAsync(cancellationToken))
            //     .ToSelectList(x => x.Id, x => x.Title);
            //
            // ViewBag.StateList = (await _dataContext.States
            //         .Select(x => new {x.Id, x.Title})
            //         .ToListAsync(cancellationToken))
            //     .ToSelectList(x => x.Id, x => x.Title);
            //
            // ViewBag.BranchList = (await _dataContext.OrganizationUnits
            //         .Select(x => new {x.Id, x.Title}).ToListAsync(cancellationToken))
            //     .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            //
            // ViewBag.ParentGuildList = (await _dataContext.Guilds
            //         .Where(x => !x.ParentId.HasValue && x.IsActive)
            //         .OrderByDescending(x => x.IsActive)
            //         .Select(x => new {x.Id, x.Title})
            //         .ToListAsync(cancellationToken))
            //     .ToSelectList(x => x.Id, x => x.Title);
            //
            // var query = _dataContext.Terminals.Where(x => x.StatusId != (byte) Enums.TerminalStatus.Deleted)
            //     .AsQueryable();
            //
            // if (User.IsBranchUser())
            // {
            //     query = query.Where(x => x.BranchId == CurrentUserBranchId);
            // }
            //
            // if (User.IsSupervisionUser())
            // {
            //     query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            // }
            //
            // if (User.IsTehranBranchManagementUser())
            // {
            //     query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            // }
            //
            // if (User.IsCountyBranchManagementUser())
            // {
            //     query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            // }
            //
            // var lastTransaction = await _dataContext.TransactionSums
            //     .OrderByDescending(x => x.PersianLocalYear)
            //     .ThenByDescending(x => x.PersianLocalYearMonth)
            //     .Select(x => new {x.PersianLocalMonth, x.PersianLocalYear})
            //     .FirstOrDefaultAsync(cancellationToken);
            //
            // var transactionYears = Enumerable.Range(1395, lastTransaction.PersianLocalYear - 1394);
            // var wageYears = Enumerable.Range(1395, DateTime.Now.ToPersianYear() - 1394);
            //
            // var dateRanges = new List<(string, string)>();
            // foreach (var transactionYear in transactionYears)
            // {
            //     dateRanges.AddRange(Enumerable
            //         .Range(1,
            //             lastTransaction.PersianLocalYear == transactionYear ? lastTransaction.PersianLocalMonth : 12)
            //         .Select(x => ($"{x.ToString().GetMonthName()} {transactionYear}", $"{transactionYear}/{x:00}/01")));
            // }
            //
            // ViewBag.TransactionDateList = dateRanges
            //     .OrderByDescending(x => x.Item2)
            //     .ToSelectList(x => x.Item2, x => x.Item1,
            //         selectedValue: new[]
            //             {$"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01"});
            //
            // dateRanges = new List<(string, string)>();
            // foreach (var wageYear in wageYears)
            // {
            //     dateRanges.AddRange(Enumerable
            //         .Range(1,
            //             12)
            //         .Select(x => ($"{x.ToString().GetMonthName()} {wageYear}", $"{wageYear}/{x:00}/01")));
            // }
            //
            // var now = $"{DateTime.Now.ToPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01";
            // var wagetemp = dateRanges
            //     .OrderByDescending(x => x.Item2)
            //     .ToSelectList(x => x.Item2, x => x.Item1,
            //         selectedValue: new[]
            //             {now});
            //
            // ViewBag.wageDateList = wagetemp;
            //
            // var vieModel = new TerminalManageViewModel
            // {
            //     FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
            //     ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
            //
            //     ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
            //     FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
            //
            //
            //     CommaSeparatedStatuses = commaSeparatedStatuses,
            //     NeedToReformTerminalCount = await query.CountAsync(
            //         x => x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
            //              x.StatusId == (long) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            // };
            //
            // return View(vieModel);

            var qqq = (await _dataContext.TerminalStatus
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken));
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };

            return View(vieModel);
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> TestManage(string commaSeparatedStatuses, CancellationToken cancellationToken)
        {
            var qqq = (await _dataContext.TerminalStatus
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken));
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };

            return View(vieModel);
        }


        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> TerminalWage(string commaSeparatedStatuses, bool? InActive, bool? LowTransaction
            , bool? TwoMonthInActive, bool? ThreeMonthInActive, CancellationToken cancellationToken)
        {
            ViewBag.Title = "مدیریت پذیرندگان";

            if (!string.IsNullOrEmpty(commaSeparatedStatuses))
            {
                ViewBag.Title = "پایانه های تاخیر در نصب     ";
            }

            if (InActive.HasValue && InActive.Value)
            {
                ViewBag.Title = "پایانه های غیر فعال";
            }

            if (LowTransaction.HasValue && LowTransaction.Value)
            {
                ViewBag.Title = " پایانه زیان ده      ";
            }

            var qqq = (await _dataContext.TerminalStatus
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken));
            var p = new
            {
                Id = (byte)30,
                Title = "فاقد سند مسدودی"
            };
            qqq.Add(p);
            p = new
            {
                Id = (byte)31,
                Title = "   اماده تخصیص ( تاخیر در نصب )  "
            };
            qqq.Add(p);

            ViewBag.StatusList = qqq
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title }).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted)
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
                query = query.Where(x => x.Branch.CityId == (long)Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long)Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new { x.PersianLocalMonth, x.PersianLocalYear })
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
                        { $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01" });

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
                        { now });

            ViewBag.wageDateList = wagetemp;

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",

                ToWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",
                FromWageTransactionDate = $"{DateTime.Now.GetPersianYear()}/{DateTime.Now.GetPersianMonth():00}/01",


                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };

            vieModel.InActive = InActive.HasValue ? InActive.Value : false;
            vieModel.LowTransaction = LowTransaction.HasValue ? LowTransaction.Value : false;
            vieModel.TwoMonthInActive = TwoMonthInActive.HasValue ? TwoMonthInActive.Value : false;
            vieModel.ThreeMonthInActive = ThreeMonthInActive.HasValue ? ThreeMonthInActive.Value : false;

            return View(vieModel);
        }
        [AllowAnonymous]
        [AjaxOnly]
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
                await _dataContext.GetTerminalData(searchParams, orderByColumn, retriveTotalPageCount, page - 1, 300);

            return JsonSuccessResult(new { rows, totalRowsCount });
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetCustomData(TerminalSearchParameters searchParams, string orderByColumn,
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
                await _dataContext.GetTerminalData(searchParams, orderByColumn, retriveTotalPageCount, page - 1, 300);
            return JsonSuccessResult(new { rows, totalRowsCount });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Test()
        {
            var pspList = _dataContext.Psps.Select(v => v.Id).ToList();
            var pspqueue = _dataContext.Terminals
                .Where(a => a.StatusId == (byte)Enums.TerminalStatus.NotReturnedFromSwitch)
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
                Rate = (float)((float)(a.Count) / ((float)(orderedpspqueue.IndexOf(a) + 3) * (float)pspcount)),
                indexOf = orderedpspqueue.IndexOf(a),
                Count = (a.Count)
            }).ToList();
            return Json(pashmak);
        }


        [HttpGet]
        public RequestInqueryResult TerminalInquiryTest()
        {
            using (var parsianservice = new ParsianService())
            {
                var t = parsianservice.RequestInQuery(new RequestInqueryInput()
                {
                    RequestData =
                        new RequestInqueryRequestData() { TopiarId = "9813608" }
                }, 503853);
                return t;
            }
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetRequestStatus(int topiarId, int TerminalId, int? PspId,
            CancellationToken cancellationToken)
        {
            var viewModel = new TerminalDetailsViewModel();

            if (PspId == 2) // irankish
            {
                using (var irankishService = new NewIranKishService())
                {
                    var terminalInfo = _dataContext.Terminals.FirstOrDefault(a => a.Id == TerminalId);

                    try
                    {                        
                        var result = irankishService.Inquery(terminalInfo.Id.ToString());

                              if (result.status && result.data.status ==   7)
                            {
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        ErrorComment = result.data.documnentStatus + " " + result.data.description,
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte()
                                    });  
                            }
                            else if (result.status && result.data != null && result.data.status == 2)
                            {
                                // var status = Enums.TerminalStatus.ReadyForAllocation;
                                // if (!result.data.terminal.FirstOrDefault().mountDate.HasValue && result.MountDate.HasValue)
                                // {
                                //     status = Enums.TerminalStatus.Installed;
                                // }
                                //
                                // if (terminalInfo.RevokeDate.HasValue)
                                // {
                                //     status = Enums.TerminalStatus.Revoked;
                                // }
                                //
                                // await dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                //     .UpdateAsync(x => new Terminal
                                //     {
                                //         StatusId = status.ToByte(),
                                //         TerminalNo = result.Terminal,
                                //         MerchantNo = result.Acceptor,
                                //         RevokeDate = result.DisMountDate,
                                //         InstallationDate = result.MountDate,
                                //         
                                //     });
                                // //todo ==> send files =====>
                                //
                                //
                                //
                                //
                                // //todo ==> send files ======>
                                // AddTerminalToMongo.Add(new TerminalMongo()
                                // {
                                //     TerminalNo =  result.Terminal,PhoneNumber = terminalInfo.Mobile,
                                //     Address = terminalInfo.Address,
                                //     Description = terminalInfo.Description,  
                                //     Id = terminalInfo.Id,
                                //     Tel = terminalInfo.Tel,
                                //     Title = terminalInfo.Title,
                                //     AccountNo = terminalInfo.AccountNo,
                                //     BatchDate = terminalInfo.BatchDate.HasValue ? terminalInfo.BatchDate.ToString() : "",
                                //     BlockPrice = terminalInfo.BlockPrice,
                                //     BranchId = terminalInfo.BranchId,
                                //     CityId = terminalInfo.CityId,
                                //     ContractDate = terminalInfo.ContractDate.HasValue ? terminalInfo.ContractDate.ToString() : "",
                                //     ContractNo = terminalInfo.ContractNo,
                                //    GuildId = terminalInfo.GuildId,
                                //    MarketerId = terminalInfo.MarketerId,
                                //    PspId = terminalInfo.PspId,
                                //    DeviceTypeId = terminalInfo.DeviceTypeId,
                                //   
                                //     
                                // });
                            }
                            else if (result.data.status == 1)
                            {
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        TerminalNo = result.data.terminal.FirstOrDefault().termianlNo,
                                        MerchantNo = result.data.accountNo,
                                    });
                                
                             
                            }
                            else if (result.data.status == 4)
                            {
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        TerminalNo = result.data.terminal.FirstOrDefault().termianlNo,
                                        MerchantNo = result.data.accountNo,
                                        StatusId = Enums.TerminalStatus.SendToShaparak.ToByte()
                                    });
                                
                              
                            }
                            else if (result.data.status == 3 || result.data.status == 5)
                            {
                                ///// OK ===============================================================
                                var errorComment = result.data.status == 3
                                    ? "عدم تایید تعریف پذیرنده در شاپرک"
                                    : "رد درخواست در کارت اعتباری ایران کیش";
                            
                                if (!string.IsNullOrEmpty(result.data.description))
                                {
                                    errorComment = errorComment + " " + result.data.description;
                                }
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte(),
                                        ErrorComment = errorComment
                                    });
                            }
                            else if (result.data.status == -1)
                            {
                                var errorComment = result.data.description;
                                if (!string.IsNullOrEmpty(result.data.description))
                                {
                                    errorComment = errorComment + " " + result.data.description;
                                }
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NeedToReform.ToByte(),
                                        ErrorComment = errorComment
                                    });
                            }                      
                            else if (result.data.status == 6)
                            {
                                await _dataContext.Terminals.Where(x => x.Id == terminalInfo.Id)
                                    .UpdateAsync(x => new Terminal
                                    {
                                        StatusId = Enums.TerminalStatus.NotReturnedFromSwitch.ToByte(),
                                        ErrorComment = ""
                                    });
                            }

                  //      viewModel.StateTitle = result.قثس.documnentStatus + " " + result.data.accountStatusDescription ;

                  viewModel.StateTitle = result.data.status + " " + result.data.description;
                        return PartialView("_TerminalStatus", viewModel);
                    }
                    catch (Exception exception)
                    {
                        viewModel.ErrorComment = exception.Message;
                        return PartialView("_TerminalStatus", viewModel);
                        exception.AddLogData("TerminalId", terminalInfo.Id).LogNoContext();
                    }
                }
            }
            else // parsian
            {
                using (var parsianService = new ParsianService())
                {
                    if (topiarId != 0)
                    {
                        var res = parsianService
                            .UpdateStatusForRequestedTerminal(topiarId.ToString(), (int)TerminalId)
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
                        if (res.StepCode == 7 && res.InstallStatusId == 2)
                        {
                            ter.TerminalNo = res.TerminalNo;
                        }

                        _dataContext.SaveChanges();
                        return PartialView("_TerminalStatus", viewModel);
                    }
                    else
                    {
                        var terminalNo = _dataContext.Terminals.FirstOrDefault(b => b.Id == TerminalId).TerminalNo;
                        var res = parsianService
                                .UpdateStatusForRegisteredTerminal(terminalNo, (int)TerminalId)
                            ;
                        viewModel.StateTitle = res.Status;
                        viewModel.ErrorComment = res.Error;

                        return PartialView("_TerminalStatus", viewModel);
                    }
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
            var ppp = _dataContext.Terminals.FirstOrDefault(x => x.Id == terminalId);

            var nationalCode = _dataContext.MerchantProfiles.Where(b => b.Id == ppp.MerchantProfileId)
                .Select(z => z.NationalCode).FirstOrDefault();
            var ids = _dataContext.MerchantProfiles.Where(b => b.NationalCode == nationalCode).Select(z => z.Id)
                .ToList();
            var tt = _dataContext.Terminals.Where(b => ids.Contains(b.MerchantProfileId) &&
                                                       b.StatusId != (byte)Enums.TerminalStatus.Deleted
                                                       && b.StatusId != (byte)Enums.TerminalStatus.Revoked);
            var s = tt.ToList();
            var ParsianTerminals = tt.Where(b => b.PspId == 3).Count();
            var FanavaTerminals = tt.Where(b => b.PspId == 1).Count();
            var IrankishTerminals = tt.Where(b => b.PspId == 2).Count();

            var viewModel = await _dataContext.Terminals
                .Where(x => x.Id == terminalId)
                .Select(x => new TerminalDetailsViewModel
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    ParsianTerminals = ParsianTerminals,
                    FanavaTerminals = FanavaTerminals,
                    IrankishTerminals = IrankishTerminals,
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

                    StateTitle = x.City.State.Title,
                    MarketerTitle = x.Marketer.Title,
                    LastUpdateTime = x.LastUpdateTime,
                    Mobile = x.MerchantProfile.Mobile,
                    BlockDocumentStatusId = x.BlockDocumentStatusId,
                    HomeTel = x.MerchantProfile.HomeTel,
                    DeviceTypeTitle = x.DeviceType.Title,
                    DeviceTypeId = x.DeviceTypeId,
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
                    TopiarId = x.TopiarId,
                    StepCode = x.StepCode,
                    NewParsian = x.NewParsian,
                    CustomerCategoryId = x.CustomerCategoryId.Value,
                    StepCodeTitle = x.StepCodeTitle,
                    CustomerCategory = x.CustomerCategory.Name,
                    InstallStatus = x.InstallStatus,
                    InstallStatusId = x.InstallStatusId,
                    PreferredPspTitle = x.PreferredPsp.Title,
                    TaxPayerCode = x.TaxPayerCode,
                    OrgaNizationId = x.BranchId,
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
                })
                .FirstAsync(cancellationToken);

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
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.PspId });

            ViewBag.CustomerCategory = (await _dataContext.CustomerCategory
                    .Select(x => new { x.Id, Title = x.Name })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.PspId });


            //todo
            //
            // var pspList = _dataContext.Psps.ToList();
            // var ruleList = _dataContext.RuleType.Where(a => a.IsActive
            // ).Include(a => a.RuleOrders).Include(a => a.RuleDefinitions).ToList();
            // var candidatePsp = new List<(int, double)>();
            // foreach (var psp in pspList)
            // {
            //     double pspRate = 0;
            //     foreach (var rule in ruleList)
            //     {
            //         if (!rule.RuleDefinitions.Any(a => (a.DeviceTypeId == 1000 && a.PspId == psp.Id)
            //                                            || (a.DeviceTypeId == viewModel.DeviceTypeId &&
            //                                                a.PspId == psp.Id)))
            //         {
            //             continue;
            //         }
            //
            //         var index = 1;
            //
            //         switch (rule.Id)
            //         {
            //             case 1: //Queue
            //
            //                 var queue = _dataContext.Terminals
            //                     .Where(a => a.PspId.HasValue)
            //                     .GroupBy(a => a.PspId).Select(a => new
            //                     {
            //                         PspId = a.Key,
            //                         Count = a.Count(
            //                             b => b.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
            //                     }).ToList();
            //
            //
            //                 index = queue.OrderByDescending(a => a.Count).ToList()
            //                     .IndexOf(queue.FirstOrDefault(a => a.PspId == psp.Id)) + 1;
            //
            //
            //                 var pspruleWieght =
            //                     _dataContext.RulePspWeight.FirstOrDefault(a =>
            //                         a.RuleTypeId == rule.Id && a.PspId == psp.Id);
            //                 pspRate = pspRate + (index * (pspruleWieght?.Weight ?? (100 / pspList.Count)));
            //
            //
            //                 break;
            //             case 2: //Branch 
            //
            //                 var branchRate = _dataContext.PspBranchRate
            //                     .Where(a => a.PspId == psp.Id && a.OrganizationUnitId == viewModel.OrgaNizationId)
            //                     .Select(a => new
            //                     {
            //                         a.PspId,
            //                         a.Rate
            //                     }).ToList();
            //
            //                 index = branchRate.OrderBy(a => a.Rate).ToList()
            //                     .IndexOf(branchRate.FirstOrDefault(a => a.PspId == psp.Id)) + 1;
            //                 var pspruleWieght2 =
            //                     _dataContext.RulePspWeight.FirstOrDefault(a =>
            //                         a.RuleTypeId == rule.Id && a.PspId == psp.Id);
            //                 pspRate = pspRate + (index * (pspruleWieght2?.Weight ?? 0.33));
            //
            //
            //                 break;
            //         }
            //     }
            //
            //     if (pspRate != 0)
            //         pspRate /= ruleList.Count;
            //     candidatePsp.Add((psp.Id, pspRate));
            // }
            //
            // if (!candidatePsp.Any() || !viewModel.CustomerCategoryId.HasValue || viewModel.CustomerCategoryId.Value == 0
            // ) return PartialView("_Details", viewModel);
            // {
            //     var orderlist = candidatePsp.OrderByDescending(a => a.Item2).ToList();
            //     var asd = _dataContext.CustomerCategory.FirstOrDefault(a =>
            //         (a.Id == viewModel.CustomerCategoryId));
            //     foreach (var rated in orderlist)
            //     {
            //         if ((asd.From <= orderlist.IndexOf(rated) + 1) && (asd.To >= orderlist.IndexOf(rated) + 1))
            //         {
            //             viewModel.PspId = (byte?) rated.Item1;
            //         }
            //     }
            // }


            return PartialView("_Details", viewModel);
        }

        #region PardakhNovin

        [HttpGet]
        public string PardakhtNovinBankBranch()
        {
            using (var parsianservice = new PardakhtNovinService())
            {
                var request = new GetGuildsRequest();
                request.PageIndex = 0;
                request.PageSize = 60;
                return JsonConvert.SerializeObject(parsianservice.GetGuilds(request));
            }
        }

        [HttpGet]
        public string GetPosModelList()
        {
            using (var parsianservice = new PardakhtNovinService())
            {
                var request = new GetBankRequest();
                request.PageIndex = 0;
                request.PageSize = 120;
                return JsonConvert.SerializeObject(parsianservice.GetPosModelList(request));
            }
        }

        [HttpGet]
        public string GetPosTypeList()
        {
            using (var parsianservice = new PardakhtNovinService())
            {
                var request = new GetBankRequest();
                request.PageIndex = 0;
                request.PageSize = 60;
                return JsonConvert.SerializeObject(parsianservice.GetPosTypeList(request));
            }
        }

        [HttpGet]
        public string PardakhtNovinBank()
        {
            using (var parsianservice = new PardakhtNovinService())
            {
                var request = new GetBankRequest();
                request.PageIndex = 0;
                request.PageSize = 60;
                return JsonConvert.SerializeObject(parsianservice.GetBank(request));
            }
        }

        [HttpGet]
        public string UploadPardakhNovin()
        {
            using (var parsianservice = new PardakhtNovinService())
            {
                var request = new AddFileRequest();
                request.FileName = "TestImage.jpg";
                request.BinaryDataBase64 =
                    "/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMraHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjMtYzAxMSA2Ni4xNDU2NjEsIDIwMTIvMDIvMDYtMTQ6NTY6MjcgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIgeG1sbnM6eG1wTU09Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9tbS8iIHhtbG5zOnN0UmVmPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvc1R5cGUvUmVzb3VyY2VSZWYjIiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzYgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkMwMTIwN0NEMDg3NjExRTRBRTlDOTkzQjM4RTdBMDY4IiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkMwMTIwN0NFMDg3NjExRTRBRTlDOTkzQjM4RTdBMDY4Ij4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6QzAxMjA3Q0IwODc2MTFFNEFFOUM5OTNCMzhFN0EwNjgiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6QzAxMjA3Q0MwODc2MTFFNEFFOUM5OTNCMzhFN0EwNjgiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgICAgICAgICAgIDAwMDAwMDAwMDAQEBAQEBAQIBAQICAgECAgMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwMDAwP/wAARCAD2AdYDAREAAhEBAxEB/8QAkwABAAMBAQEBAQEBAAAAAAAAAAgJCgcGBQQDAgEBAQAAAAAAAAAAAAAAAAAAAAAQAAAFBAABBQkLCAgEBgMAAAABAgMEBQYHCBEhEhNYCZbWFzeXGDh42DEiFNQVlVd3t7gZI7RVdbUWdrZBMtLTlNU2VqfXaClRYUKSMyVDJEQRAQAAAAAAAAAAAAAAAAAAAAD/2gAMAwEAAhEDEQA/AOUai6i6oXLqhrFcdx6xa81+4a/rzhat12u1vC2N6rWa1Warje2p1Uq1Wqk62n51SqdSnPrekSHlrdedWpa1GozMBIbzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAeZXpv1S9ZvIPizvVAPMr036pes3kHxZ3qgHmV6b9UvWbyD4s71QDzK9N+qXrN5B8Wd6oB5lem/VL1m8g+LO9UA8yvTfql6zeQfFneqAjzmnUXVClZI1Fg0vWLXmmwbl2GuWiXHDgYWxvDi1+jMaobO3GxSa3Hj202zVaYzcNAgT0R3ycaTMhMPknpWW1JCQ2lfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAH6oMN6oTYkCOSTkTpUeGwS1c1BvSXUMtEpXLzUmtZcT/oIBPb8NHaD9F2X3XxfiwB+GjtB+i7L7r4vxYA/DR2g/Rdl918X4sAfho7Qfouy+6+L8WAPw0doP0XZfdfF+LAINSqBUIlxyLWdJn5Vi1t6gOpQ7xj/KDM5VOWSXjSXFn4Sk+CuBe95eACcv4aO0H6Lsvuvi/FgD8NHaD9F2X3XxfiwB+GjtB+i7L7r4vxYA/DR2g/Rdl918X4sA4/mvULMmALVp945Dh29HotSuCLbMVdJrrNUkHVJlOqtUYSuO2y2pDBxKM+Zr48CURF/SAi+A9Da1p3Pe9bh23Z9Aq9zV6oKNMOkUSBJqU54k8DccKPFbcWhhlPvnHFcG20EalGSSMwE7LN7MvZK5osaZWisixGn0ocXEuW4X5dVaaWov8A+S2KZX4pP9GfO6NyQ0Zf1VGlXEiD19X7KrOcSMb1IvXGFXeQS1LiO1C5aY8vhzeYiM45bUiM4tXE+PSLZSXD3T4gIcZb1qzXg8yeyJYtTpdJW50TFxwVR61bTy1LShpCq1SnZcOG++pZdGzJUw+vl4I5DAcKASKwdq5lbYaFcM/HEShyY9sSqfDqp1estUpaHqm1KeikwlxpzpkmiIvnGXDgfD/xAd2/DR2g/Rdl918X4sAfho7Qfouy+6+L8WAPw0doP0XZfdfF+LAH4aO0H6Lsvuvi/FgHEM36qZa17pNErWRotBjQrhqL9Lpp0mtNVV1cqNG+Fu9K20y30TaWv6TPlM+ACNwAAAAAAAAAAAAAAAAAAAACM2ePGnpX6zN1fc320ANK/Q31L9WbA/2WWqAkyAAAAAAAAAAAAAAAAAAAAAAP3UycqmVKn1JCEurp86JOQ0szSlxUR9t9KFGXKSVm3wMy5eAC1j8WTIP0SWb8+Vv+7APxZMg/RJZvz5W/7sBcNTr1lTcTQcirhR0TZeO4t6qpyXHPgqZT9tIrqoSXT/K/B0uq6MlH77m8vugKefxZMg/RJZvz5W/7sA/FkyD9Elm/Plb/ALsBWfEq67gyPGrzrKIzlbvZmruR21GtDC6lXUzFsoWoiUtDSnuaRmRGZEA1S5wyFMxRiW/MjU+nRqtNtGhPVeNTZjrrEWY408y2TL7zBG62gyd48U8vIAqK/FkyD9Elm/Plb/uwD8WTIP0SWb8+Vv8AuwD8WTIP0SWb8+Vv+7AR52W3eunZWxaTYtbsagWzEpN2wLsbn0qo1GZIekQaPXqOiItqWhLaWXG68tZqL3xKbIvcMwEJGGHpLzMaMy7IkSHW2GGGG1uvPvOrJtpllpslLcdcWokpSkjMzPgQDTjrpguwdTcOuVOt/JcG4WrfVcWUb3loaN7nxYp1CfAbndH0yKBQiSpuOwjgThoN00m64ozCAeTu1ZuhdWnQ8Q4/t+JRGXXWIdbvv5SqVTnskbiEzk0aj1KkRaUtZGlSGnJEwk8PfcePNSHOKH2p+eoU5tyu2pjSu041J6eIxS6/R5hoLjxKLPbuKYywtXEuJuR3y5OQiAWna87J4z22suuQ00ViJVIkYoF746uMoVYa+AVFC2kvs9KymNX7encFNG4thtSXCNDrSOLZrCkvd/XOLr3lkmbbZdbx/fEWRcFoNuKW78lKZfQzW7a+EOrU7IKjSXm1tKVxUUSSylalrJajD8esO39x6w027qbQrOol0Iu6dSZ0h2rT58NUNVJYmsNoZKGlROJeKaZqNXKRpLgAlL+LJkH6JLN+fK3/AHYCXune6Nz7M3pdNr1yyqDbEe37XTX2ZVKqFQmPSHlVaDTvg7iJiUoQ0SJZq4ly8SIB+zcjci5dZLlsyhUKzKHc7Vz0OoVZ9+rVCfDciuQ56IaWWUw0KStC0q5xmrl4gIbfiyZB+iSzfnyt/wB2AjHs5uTcmzdAtig12zKHbDdsVeXV48ik1CfMXKXLhfA1svImISlCEp4KI0nx4gIagAAAAAAAAAAAAAAAAAAAAIzZ48aelfrM3V9zfbQA0r9DfUv1ZsD/AGWWqAkyAAAAAAAAAAAAAAAAAAAAAAAAAANW9C9GGjfUNTvs+ZAZSAAB9+1P9U23+v6P+0YwDTzuR6L+av4Ll/nUQBlnAAAAAe2xpVKbQ8j2BWqybZUikXtatUqpuoStoqbT67AlzjcQsjStv4K0riRkZGQDTftlY1yZO1zyhZ1mE5JuGr0KFKpUaKsumqh0at0qvv0mMrnoQt2tQqY5FQRqJKzeIj5DAZYpcSXT5UiDPiyIU2I85Glw5bLkaVFkMrNt5iRHeSh1l5paTSpKiJSTLgZAPzgJIarZ5LXTLUO/5VMnVqjOUStUGu0inSW4sudCqEdL0QmnHz+Dmcasw4rxksjLmIPh77gA7buLuPbGztu2nRaTYFWtiXalemVNirVOrwJ6pEGoU74LMhFGiwGlx1uyGWF8SfUkya5SM+BpCAQAAtR7KHxu5K+rhH8zUYB9XtZPGDiT+Da5+22wFTIAAAAAAAAAAAAAAAAAAAAAAAIzZ48aelfrM3V9zfbQA0r9DfUv1ZsD/ZZaoCTIAAAAAAAAAAAAAAAAAAAAAAAAAA1b0L0YaN9Q1O+z5kBlIAAH37U/1Tbf6/o/7RjANZ+V8ewsr45u7HNRqMqkwrupDtIk1GE009KhtuuNOG8w0/8AkVrI2uHBXJygK3fwnMd/SzenzLQ/7QB+E5jv6Wb0+ZaH/aAPwnMd/SzenzLQ/wC0Arg3B16o2tWS6HYtDuGqXLEq1i0y7HJ9WixIkhqROr9zUdcRDcMzaUw21QULJR++NThl7hEAikAud087Qm3aVbdDxZneY/S10KJHpVt5GcTJnQ5lOj81iBTLrbZafmQ5cFnmtNzyJxl1pJfCOiUhTrwWMXNibXXYmlN12tWtYGSIU5lCWLrpDsR6oOsKaSaG493W3Kj1dtBNKSZJRKLm8h8CMiAQ9vrsscL1zpn7Gu687DluEsm40pUO7qGwZ842zRCmlTK0vmmrgrnVI+ckiIuB8VGEDstdnBnzHMSVWLabpWUqLFT0jhWl8KbuVtlKTNbjlsTW0yJRkouBNwXprpkfHmkXHgEAn2Hozz0aSy7HkR3XGH2H21tPMPNLNt1l5pwkrbdbWk0qSoiMjLgYD+QAAtR7KHxu5K+rhH8zUYB9XtZPGDiT+Da5+22wFTIAAAAAAAAAAAAAAAAAAAAAAAIzZ48aelfrM3V9zfbQA0r9DfUv1ZsD/ZZaoCTIAAAAAAAAAAAAAAAAAAAAAAAAAA1b0L0YaN9Q1O+z5kBlIAAH37U/1Tbf6/o/7RjANS20VzV6zdfcrXRa9Uk0W4KLasmbSqrDNKZUGUiRGQl9hS0rQSySsy5SP3QGeTzy9ofpqvL/ABEL4kAeeXtD9NV5f4iF8SAPPL2h+mq8v8RC+JAOP5AyZfuVazGuHIl0VO7K1DpjNGi1GqraXIZpceVNnMQkG000noW5dRfcIuHHnOHygPCgP7uRpLTMeQ7HfajyycOK+404hmSTLhtPHHdUkkPE06XNVzTPmq5D5QH2bcuy6rOnlVLRuW4LWqZEkiqNuVmo0OeRJPnJIplMkxZBEkz4l77kMBL+we0M2dsc2mpl3QL8pzSmz+AXxR41RcNKTPpCOs006TcDinUmXK7LcJJpIyIuKucFpGsfaBWTnavQrCumguY/v6okpFHZKf8AKluXHIaaW85Fp9QXHiSadUltNqU3GkIUlZFzUPLcMkGHDe0z1xt47YLYS16cxTK/T6pTaXkJMZBtMVym1V1qmUquyGm0m18rwam4xFW5wSqQzITz1GbKCMKSgABaj2UPjdyV9XCP5mowD6vayeMHEn8G1z9ttgKmQAAAAAAAAAAAAAAAAAAAAAAARmzx409K/WZur7m+2gBpX6G+pfqzYH+yy1QEmQAAAAAAAAAAAAAAAAAAAAAAAAABq3oXow0b6hqd9nzIDKQAAPv2p/qm2/1/R/2jGAaedyPRfzV/Bcv86iAMs4AAAACa/Z83hTLS2gspurmyiJdkSsWe08+2y4hmp1WMmTRCSbqFKbelVqnx47am+CyW8RceaauIWV9pthqq3/im3shW/Ccn1LFU6qyqtGjtqXIO0a+xCTWJyUIPnPFSJlIiurLmn0cdTznEiSrnBn9AAHY9erauO7c4YpotqNSV1pd+WxPYfipUa6bHpNXi1WfWXFIMlNx6NBhuSXFEfFKGj4cvAgGgHtAK9TqHqnktuebana7+7VBpjC1JScioyrnpEpJN84lc5yJDhPSeBFx4MHw4e6QZmwABaj2UPjdyV9XCP5mowD6vayeMHEn8G1z9ttgKmQAAAAAAAAAAAAAAAAAAAAAAARmzx409K/WZur7m+2gBpX6G+pfqzYH+yy1QEmQAAAAAAAAAAAAAAAAAAAAAAAAABq3oXow0b6hqd9nzIDKQAAPv2p/qm2/1/R/2jGAa2Ml4/o2VLDujHlwyanDot2Utyk1GVRnoseqMx3XG3FLhPzoVRiNvkpouBuMOJ4f0AIDfhU69f7yzN3Q2R/y7APwqdev95Zm7obI/5dgH4VOvX+8szd0Nkf8ALsBxPY3s8sLYhwpf+SLaufKE6uWrTIk2nxa5WrTk0p52RV6dT1pmsQLKpkxxsmZijIkPtnziI+JlxIwp1iypMGVGmwn3osyHIZlRJUdxTT8aTHcS8w+y6gyW28y6glJURkZGRGQDQ9qbvPY+ZKBS7PyTV6XamVYsdqnyW6o8zAot7G00TZVOjypBtw2qlNIuL9PUpK+kMzYJbZ81sPq5X7OjXzJVQl1ykxKzjWtzXXZMldlyIjVDkyXlc5br1uVGJNgRUcTMybgnCRx5TI+XiHCInZM2MiVz52X7skwudxOPEtyjwpXM6RJ834Y9NntEroiNPHoOHOMlcOBc0wmDjvB2uWoFu1S6IfyZbKUxSYrWQr4q0aRXJEYzb4QEVN9uKxGbmPNIP4HAYYTJeJP5NayRwCmbdzbbzi7mgW9aJTIeLLPlyH6MmUlceTc9ZcbXFcuedCWlK4jTcVa2YLLhdK0y44tfMW8ppsIJgAC1HsofG7kr6uEfzNRgH1e1k8YOJP4Nrn7bbAVMgAAAAAAAAAAAAAAAAAAAAAAAjNnjxp6V+szdX3N9tADSv0N9S/VmwP8AZZaoCTIAAAAAAAAAAAAAAAAAAAAAAAAAA6u3nnOTVLRRGszZXborcBNKbpDeRLvRS26WiOURFNRT01goiYCYhdETJI6Mm/e8OHIA5QAAP6NOusOtPsOuMvsuIdZeaWpt1p1tRLbdacQZLbcbWRGlRGRkZcSAdi843YX6eMzeVC9/88APON2F+njM3lQvf/PADzjdhfp4zN5UL3/zwA843YX6eMzeVC9/88AfJrmb803PSptBuXL+ULhodRbS1UKNXL/uyrUqe0h1D6GptOn1aRDlNoeaSskrQoiUkj90iAcvAAHZ7R2KztYkVuBaeW79pFNZQTbFLRclRlUqMhKeYRRaXOelU+N73gXFttJ8CL/wLgHR5G8W1kplbDuZK8lC+bzlR6ZbMN4uapKy5kiJQ2JDfE08vNUXEuJHxIzIBH67r9vi/wCf8qXxd9y3fUCUs0SrkrdRrLrBOcOc3GOfIfKKyRJIibbJKEpSREREREQeSAAAB6q076vewpkmoWNeV1WZPmxvgcydadw1e3ZkuGTqHyiyZNHmQ3n43TNpXzFqNPOSR8OJEA/3dl/X1f0iJLvq9btvSXT2VxoEq7LjrFxyIUd1ZOusRHqxMmORmXHC5ykoNKTVymXEB5IAAAAAAAAAAAAAAAAAAAAAAAEZs8eNPSv1mbq+5vtoAaV+hvqX6s2B/sstUBJkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEZs8eNPSv1mbq+5vtoAaV+hvqX6s2B/sstUBJkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEZs8eNPSv1mbq+5vtoAaV+hvqX6s2B/sstUBJkAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEZs8eNPSv1mbq+5vtoAaV+hvqX6s2B/sstUBJkAAAAAAAAAAAAAAAAAAAFgmm+m9sbN2xeVdrt5V62H7Yr0GksMUmDT5bUpqXT/AIYbrpzOC0OIWXDk5DIBwDaHDNLwHmGt40o9an1+DSqbQZrdTqTEeNLdXV6VGqDiFtReLKUsrfNKeHKZFygPga94xp+Zcx2RjOqVOZRoF1zajEkVOntMPTIhQ6JU6o2tlqQRsrNTsFKTJX/pUf8ASAtm/Ccx39LN6fMtD/tAKhM141l4eytfeNZjzkk7Tr8mBDmPIJt6fR30tz6FUHmySlLb1QosuO8tKeKUqcMkmZcDMOXAADuGueG5OesvWtjVqVJp0KrLnS63VorTbztJotLgvzZsxLbpk0bizaQy1zvem88gj90BafUuymx5Ap0+cnK15uHChSpZNnR6GknDjsOPEg1EZmklGjhx4HwAUgAAD9MKHLqMyJT4Ed2XOnyWIcOKwg3H5MuU6hiPHZbTxUt155ZJSRcpmfABdnSeyes1yl01ys5UuqPWF0+GuqsQqRRnITNSVHbVOaiOOLU4uK3KNRNmozM0EXHlAVD5ax3VMTZKvXHFY5y5tpV6bS0yFJ5nw+ASikUiqIRyGlqrUl9iSgjIjJDpcSI+QBzsAAAEnsC6i5l2FP4faNGj0m0W5C48q97medp1vk8yskvx6f0TEmoVqW1wUSkxWXG21lzXXGuJGAsws7soMdxIrKr+ydeVdn8ErebtOFRrZgJXxNSmEnVot0yn2i5E8/iypREZklBmRJDok3sutcJUdbUaq5Ppzx8DRKjXLRXnEGXHgRtzrWlsKQo/dLmkrh7hl7oCLeVuyruqkQ5VUxBfcW7DYbU6m17ritUOrv8AM/8AwwK5Fddo8yS7x5EyGoDZcOVzlAVXXNbFw2ZXanbF10aoW/cFHkrh1Ok1SM5FmRH0cvNcacIuc24gyU2tPFDiFEtBqSZGYfCAWY6maJ2lsVipzINavu47cmouisUAqfS6dTJUU2abHpryJHSyzJ3pXTnGRl7hc0gEm/wnMd/SzenzLQ/7QB+E5jv6Wb0+ZaH/AGgD8JzHf0s3p8y0P+0AfhOY7+lm9PmWh/2gFGYAAAAAAAAAAAAAAAAAAjNnjxp6V+szdX3N9tADSv0N9S/VmwP9llqgJMgAAAAAAAAAAAAAAAAAAAvM7Jzxd5Z/jSi/sNQCD3aPelZeP6gsr+WaeA8Rot6V+Hf1xXP5QuEBpLuq86dadTsKnVEyQd+XiqzIDylc1tmoqtG7LqjEsz5OMorWUwgv/U48ki5TIBTJ2quMjpV9WFliFHUUS7KM/alccQaejRWrbc+FUx94jIl9PUaPUFNJ4GaeZA9xJ/1gqbAAF0vZU4uRDpuRs01RgmjlqZsW3JT6SaQ3Ah9BW7qkocWrmrYfk/AG+eRESFRnE84/fEkLU4lyU+8sbRrvpKlKpV1WOzclMUsuC1U+uUFNThKUXAuCjjSk8f8AzAZBwABOrs88SeEzYai1mfGS/buMYyr3qXStpWy7VorqY1qxOKucSXyrbyJieKT4ogrLkPgYC5G79hYdubb4xwa7MQiDc+Prmk1FBGXRIumqzYc200yXOB8yQ1TbSntNo5OcdTQZ8eKAFf3apYf+AV+y820uMZRq8wVkXYttKuYmr05p+dbk50ySouln0pMiOozUkiTBaIiMzMwFQoAAmTpTrSexWTlN15qQnHNltxaxeb7SnmTqRvurTSbXjymVIcjyK26w4pxaVJW3EYeNKkudHxC6/Y/ZPHmpFiUSnxaNEmV6XAVTbBx9SOhpkRuDS2m46Zcw2GlN0e3abzm2yNLZuPL/ACbST5ri2wo7yLvBsvkabJffyVWLQp7y+LFFx++9aMOG0TinEstTaY6ivSElzuBqkTHlrT70zMuQByql7B53ostudTMy5QjSGzSZH+/VzPNOElRKJuRGkVJ2NKZNREZodQtB8OUgFkGqvaOXW7ctEx/nx+JWqTWpUSkUvITMSNTqrSJ0pxEaEVzR4TcenVClOuKQhcpDTL7HE3HTeI1GgJJ9ozr1TMiYqnZYotObTfeMYR1CZLYRzXqxYzK1u1uBL5qeD3yGhxVQZWs/yLTT6E//AC8gZ7QGh3swvRpf+se6vzCgAK+dpNpNg7N2Dyta9r5Wuii2/Rboeh0qlQ3oqYsGKmLFWlllK4q1EglLM+Uz90BwLzy9ofpqvL/EQviQB55e0P01Xl/iIXxIA88vaH6ary/xEL4kAjKAAAAAAAAAAAAAAAAAAIzZ48aelfrM3V9zfbQA0r9DfUv1ZsD/AGWWqAkyAAAAAAAAAAAAAAAAAAAC8zsnPF3ln+NKL+w1AIPdo96Vl4/qCyv5Zp4DxGi3pX4d/XFc/lC4QFt/aR3TU7GxRia86MvmVa1NhbHuGnGalJScuj2vfs9htw08psurYJKy5SUgzIyMjAey24tWmbEajVa4LYb+UVt27RMuWYtLKXn1ogQCqz7bTfI4UyXa86ZHJKTJZOucOB8qTDNUA/222484hppC3XXVpbbbbSpbjji1ElCEISRqWtajIiIi4mYDQZlZbGo+g0ezoyk0+6qnasOx0mw42h569r9bkzbxlMyDUbq3IMeRU3mFp5y0JjtknmpSRpCUeE/RaxH9QNhfZ3SQGU0AAaIOzfxYzjjX5d9VhtmFVsnT37olSpJoYVEtOkoegW63JeWom0xlNNyqglZqIianFzuBkfAKa8wZ2rF47J3BnGhS1okwL6g1mzHXOkJMek2jKiR7TJTJk2baXKfSmFvt8E85xxZnyqMzDQJlS26Ftdq9UmaGlqQi/rIg3ZZy1K5y4VyMRma3Q2FumTC2XmaoyUOTxJJpQp1Ck+6kBl0fYejPPRpLLseRHdcYfYfbW08w80s23WXmnCStt1taTSpKiIyMuBgP5ANJ/Z643j2DrTalSXHbbrOQpE+96s8SFE44xOkLhW+2bjhJcW0i34Udwk8CQlx5fN48TWoKLdnsrzsz5wv29H5a5NMOsy6JazZuJcZiWpRJD0GiNRyQRNoTJjoOU4SeJKfkOK4majMw4EAAACwWsdo9m+fjul48g0u0IjLFoxrUrtyVGBKrlwXAlulJpM2e98KlM0aM5Umecp1BxHT56jMlgK+gGh3swvRpf+se6vzCgAKfNy/ShzV/GUj8yhAIygAAAAAAAAAAAAAAAAAAAAACM2ePGnpX6zN1fc320ANK/Q31L9WbA/2WWqAkyAAAAAAAAAAAAAAAAAAAC8zsnPF3ln+NKL+w1AIPdo96Vl4/qCyv5Zp4DxGi3pX4d/XFc/lC4QFpXarej1Zv1zW9/JGRAH3uzWyQ1fuvTtkVNxuZUMa1qdbj8aRzXzetmudNWKKuQ2slEqMapMyGhCiMuiiEXucgCkPP2N3MRZmyLjxTa24tu3LNbpHPJwlOW9P5lUt14+lM1Gp6hzo6lcqi5xnwUr3TDrGj2LfCtsfYtPlRfhNCtOQu/bhI0rW0UK2FsyKc08lKTQpmbcTsKOtKzSlTbqi5f6phKLtUcpfLWQrMxNAlc+FZNGXcdeZbUXN/eK5iSUCPIT7vTU+gRW3mz9zmVA/d/oC1jCfotYj+oGwvs7pIDKaA6HiXHtRyvkuycdUvnplXdcNPpK320844MBx3patU1J5q+LVKpTT0lfvVe8aPkP3AGgnd3IlPwTq9UrctlaKTNuSn07FdnRI7i23YNLep5xakuN0aTdbRT7Vgvtod4oJt5xr3xKNJKDNmAvp7LnLZ3Ni648T1KSldSxzVflKiNrWrpHLVuh+RLU02lalG4VNuBEo1qTwSlEtpPAvdUFeG/uIPBTsLcMyBGJi28joO/KL0TRNx2JdTkPN3JT0Gng0TkevNPPEhJJ6OPJaLh7hmEJQGraxUlbusFoFSvyHyJgigqgGfvjQuBj+KuOtZlzTWvntEpR+6o+J+6YDKSAAJv6+aL39sRYK8gW3eNoUKnIrtRoHwGtprRzTkU1mE86//APoU6Sx0LhTkkn33O4pPiQDuX4UOXfpKxx/7Lm/yYA/Chy79JWOP/Zc3+TAKxK9SXqBXKzQpDrT8ii1ao0l95nndC89TZj0N11rnpSvonFsmaeJEfA+UgGgfswvRpf8ArHur8woADt95YV1IuC6K1Wr4tjFky7ajMVIrsms1eCxVHpxoQlS5rLlUZW28baU8SNKT4cAHmfN80d/2hhn58p3+cgHm+aO/7Qwz8+U7/OQEZtxcO6rWrrjkWvY3tvGUC9IH7o/I0u3qrCk1hr4VfdsQqj8EYaqchxfSUmQ+lzgg+DSlHyEXEgoyAAAAAAAAAAAAAAAAAAEZs8eNPSv1mbq+5vtoAaV+hvqX6s2B/sstUBJkAAAAAAAAAAAAAAAAAAAF5nZOeLvLP8aUX9hqAQe7R70rLx/UFlfyzTwHiNFvSvw7+uK5/KFwgLSu1W9Hqzfrmt7+SMiAIEdmxk47H2EYtSXINqjZQosy23UK49Cmu09K61b0lfAyMnlLiyIbfIoudN5SL+skOy9qvjL5NvHH+WYLHNjXNSZNn11xB+9TV6AtU+kPvEZf/NPpU91ojIzLmQC4kR8qg7F2YGPYVnYov/NlwEmAm5p8inwqjJ5xMxrPstl1+qVBCiLgUd+suyEOnwM+NPLh/wCYU9ZhyHNyvlG+siz+eTt2XHUKnHaWXBUSmG58Ho0Dhz3OBU6kMMMF75XI37pgNNOE/RaxH9QNhfZ3SQGU0BbX2VuJk1a7r2zLUo6lRbThps+2XFl+SVXa4ymVXJTSubxKTTKGlpk/fcOjqR8SM+BkFnGaMa635ilUmn5ldtysyrSXPTTadNyHVrZcpbtVTCVNN+BQ7oonSPvtw2eCn0LWhJe9MiUfEOHeZ/oD/t6zfLLef/MIB0vFWGtSsNXM5dGLTtW27im01+hOyWMp16rlMp02TEkOQXKdWryqkCQTkqEytPFk1pWhJpMjAcn7SXER5BwQq86bFS/cGKKgdxIUlCDkOWtUCZgXVGbWrgaGmEIjT3PfcrcAyIjUZEAztANSWo10U/IuruJpPSFKbYsaJZVVbNajdKTajLlozW5BmSXEuvopnScT5VIcSojMlEZhmZvy0Knj+9rssisNuN1O07hq9Al9I2po3XKXOeiFJQlXusS22idaURmlba0qSZpMjMPJgJd4R3WzDgKy1WJY0Sy3qIuszq4a69RahPnfDag1EZfST8atQG+gJENHNT0fEj48p8eQOv8A4oeyX6Pxj3L1jvoAXS62ZEr+WcH49yJdCKe3X7opcyZUkUqO7Ep6XWKzUoCCix3pEt1pHQREcSNxR87ifH+gBl0yR4xL9/jS6f25OAX09mF6NL/1j3V+YUABT5uX6UOav4ykfmUIBGUAAAAAAAAAAAAAAAAAAAAAARmzx409K/WZur7m+2gBpX6G+pfqzYH+yy1QEmQAAAAAAAAAAAAAAAAAAAXmdk54u8s/xpRf2GoBB7tHvSsvH9QWV/LNPAeI0W9K/Dv64rn8oXCAtK7Vb0erN+ua3v5IyIAoete4qlaFy29dlGd6Cr2zW6VX6W9xWno6hR5zFQhrM21IXzSkR08eBkfABo+2TtOPtLqK/VLPiHUqlVbctzJ9kxWujckHU4sVuouU1sn+hMp8ijzJkHmmba0vO80y4kaDDlO1tQh6x6Q0fFFGfZ+VK5RaLith5olI+GHOhPTr7q/QoUhRN1SMxNJZ/wBVLs5HEj48DDP0A1ZYT9FrEf1A2F9ndJAZTiI1GSUkalKMiSkiMzMzPgRERcpmZgNQ+vll0rWfV+hM3Ik6eu2bPqd/386oyU+3VZMJ646+0rnGhC3KWyRQmyLmkpEZHumZmYZor7u+qZAvS674rS1rql2XBVrgmkt5T/Qu1Sa9L+CtOLJJ/B4aHSaaSSUpQ2hKUpIiIiDygD/pGaTJSTNKkmRpURmRkZHxIyMuUjIwGqPXm/6bsLrraNfrjbNUO5rUk2tfMF0zUiTVobD9uXSxJbU446hmquNOPoSpRqOPIQZmfHiYZqMy42qOIMo3xjepk8btq16ZAiSH+b0k+juGUuhVQ+YlCP8A7SjSGJHAkp4dJw4EZcCCxXsy9iYNp12q4Ku2opiUq8agmsWJKlOcyPGutTCI1RoKnXF8xkrhix2VxU+9QcphSC5zkhJGHcd99K65kqpLzNiKmIqF2lCaj3taMUm2plyNQGibh1+ikfMTKrsaGhLD8c1c+Uy00bXF5BoeCjefAnUuZJp1ThS6dUIbqmJcGfGehzIr6D4LZkxZCG32HUH7qVJIyAfkAdAx1ivIeWa41buOrRrN1VRxaEupp0YzhQEuEo0v1aqPmzTKRFPmH+VlPNNmfIR8TIgGoPXDHNbxJhDHeO7kfgSa7bNFdjVR2luuvwCly6lOqS2Yz77Mdx5EcppNms0JJSkmZFwMgGXDJHjEv3+NLp/bk4BfT2YXo0v/AFj3V+YUABT5uX6UOav4ykfmUIBGUAAAAAAAAAAAAAAAAAAAAAARmzx409K/WZur7m+2gBpX6G+pfqzYH+yy1QEmQAAAAAAAAAAAAAAAAAAAXJ9mRlDGlg2Jk6HfWRLFsuXPu6kyYEW7LtoFuSJsdujG05IiMVioQ3ZLDbvvVLQSkkrkM+ICHW/V02zeWy11160LjoV1UORRLRaj1m26vT65Sn3Y1vQWJLTNRpkiVDdcjvINC0pWZoURkfAyAeP0zr9CtfZrFNeuatUi3aFTqrWXKhWq7UodIpMFt21q7HacmVGoPR4cVDkh5CEmtaSNaiSXKZEAsg7SvLGLL7wValIsfJeP7yq0bLNCqUil2peVuXFUWKczZ99xXZ70KkVKZJahNSZjLanVJJCVuoSZ8VJIwpBAXs9nnstj2n4NVYWSMiWfZ1VsO4J8KipvK66NQF1K2q0tVZhLgLrk2EcsoFSkTGFIbNwo7SWSM0pWhJBD7tIs4UHKmVLbtmzLhpFzWhYFu8WqvQKpErNGn3Fc5x59Wch1GnPSIExEWnRYLCjQtfRvtuoMyUSkkFc4DS5iDPGDqZrfi+hVLMuKafW6fhGyaTPo87IloRKrCqsSw6ZDlUyXT36w3LjVCNLbU04ytCXEOJNJkSi4AKLtULdsy4s82F4Qrmte1LMoFUTdddqN216l2/S5TVvGmoQaQmXVpEWLIeq1VbYZUySyWqOp1Rf1TAWw7/bM47kYDnWZjbI9lXhW79rVOoVTas67KDcMmm21GNdXqsmWmjz5q4rE12AxD9+SScRJWRHyGAoTAAABbX2ZuwlqWIxkLGWQ7vty0aFKXFva2qldVdp1v0xNT4RqLcFORPq78WGuXOjJgutMk4lZpjvKJKi5xpDzHaWeCi8LksjKeN8i48u+pzoLto3dTbSvK26/UUHTjdn2/WZECj1GZK6Fcd6TFdfUkkI6GOgz4qSQCr5txxlxDrS1tOtLS4242pSHG3EKJSFoWkyUhaFERkZHxIwFuGt/aZT7cp9Os7PlPqNxQYTTUODkKitok3A3HaQltkrnpjzzKa0tpJcFzGFokqSXFbT7pqcUFgbt56W7HR2JVUq+GL6lmwlTCbnKi067YrHRJWaUR6+3TLqgtIQZEsiShJGXA+UuBB/FnVnTGjKOqqx5jRptvmKU9VKy5Mp6Scdb5hrYqtakU/mrcNKS4o4Hx5vuHwMP93PtdqZgyiqpVOvKyUMQzWUW0MVRKdWV/CEo4KjohWqj5Epj5pZJJnKejIIyJJqI+BAKqtje0XyJleNPtPGsSTjOyJaHosyU1LS7elehup5i2plTjc1mhRH2+RbEM1OGRmlUhxCjQArhAXs9nTl7E9ka+PUW9MoY7tCsnf1yzCpN0XrbVAqZw34VEQxKKBVanElHGeU0skL5nNUaT4HyGAk9Wa3odcdUm1y4avqNXq1Unjk1GsVmfhuqVSoSDSlJvzahOdflynjSkiNTi1K4EXKA+Z/29P8Aoz/4IgH/AG9P+jP/AIIgH/b0/wCjP/giAz/7C/up4ccqfuL+737nfvrXP3b/AHT+Tf3Z+SPha/gfyH8j/wD1fyb0XDovg/5Lm/1eQBxsAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAGlfob6l+rNgf7LLVASZAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGbPHjT0r9Zm6vub7aAI86i5pyRStUNYqXB1F2GuWDTdecLQIdx0S5dUGKNX4sPG9tR49bpLFx7O0C4WaZVWWyfjonwIUxLTiSfYZd5zaQkN4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0APDxlPqV7M91Wm/taAHh4yn1K9me6rTf2tADw8ZT6lezPdVpv7WgB4eMp9SvZnuq039rQA8PGU+pXsz3Vab+1oAeHjKfUr2Z7qtN/a0AR5zTmnJEzJGosiRqLsNSnqVsNcs+DBn3Lqg5KuWU5qhs7S10SiLpeztSgsVNiDUnqitdRep8M4dPfSl9UtUWLJD/2Q==";
                return JsonConvert.SerializeObject(parsianservice.AddFile(request,1));
            }
        }

        #endregion

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Details(AllocatePspViewModel viewModel, CancellationToken cancellationToken)
        {
            var terminal = await _dataContext.Terminals.FirstOrDefaultAsync(
                x => x.Id == viewModel.Id && x.StatusId == (byte)Enums.TerminalStatus.New, cancellationToken);

            if (terminal == null)
            {
                return JsonWarningMessage(
                    "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' است امکان تایید یا عدم تایید دارند");
            }

            terminal.Email = viewModel.Email;
            terminal.WebUrl = viewModel.WebUrl;
            terminal.IsVirtualStore = viewModel.IsVirtualStore;
            if (viewModel.StatusId == (byte)Enums.TerminalStatus.NotReturnedFromSwitch)
            {
                if (terminal.StatusId != (byte)Enums.TerminalStatus.New &&
                    terminal.StatusId != (byte)Enums.TerminalStatus.NeedToReform)
                {
                    return JsonWarningMessage(
                        "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' یا 'نیازمند اصلاح' است امکان تایید دارند");
                }


                terminal.PspId = viewModel.PspId;
                var result = _dataContext.SaveChangesAsync(cancellationToken).Result;

                return AddAcceptor(terminal.Id, terminal.PspId, viewModel.IsVirtualStore)
                    ? JsonSuccessMessage()
                    : JsonSuccessMessage(MessageType.Danger,
                        "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
            }

            if (viewModel.StatusId == (byte)Enums.TerminalStatus.NeedToReform)
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
                                                          (x.StatusId == (byte)Enums.TerminalStatus.New ||
                                                           x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                                                           x.StatusId == (byte)Enums.TerminalStatus
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

            if (viewModel == null || viewModel.MarketerId == (long)Enums.Marketer.BankOrBranch &&
                !User.IsBranchUser() && !User.IsAcceptorsExpertUser())
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new { x.Id, x.Title })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.MarketerId });

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => new { x.Id, x.Title })
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}", selectedValue: new[] { viewModel.BranchId });

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
                    .Select(x => new { x.Id, x.Title })
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.ActivityTypeId });

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
                    ForEntityTypeId = (long)Enums.EntityType.MerchantProfile
                })
                .ToListAsync(cancellationToken);

            var previouslyUploadedTerminalDocuments = await _dataContext.TerminalDocuments
                .Where(x => x.TerminalId == viewModel.TerminalId)
                .Select(x => new UploadedDocumentViewModel
                {
                    DocumentId = x.Id, DocumentTypeTitle = x.DocumentType.Title,
                    ForEntityTypeId = (long)Enums.EntityType.Terminal
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
                                 x.StatusId != (byte)Enums.TerminalStatus.Deleted &&
                                 (x.StatusId == (byte)Enums.TerminalStatus.New ||
                                  x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                                  x.StatusId == (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch),
                    cancellationToken);

            var branchLimitations = await _dataContext.CheckBranchLimitations(CurrentUserBranchId);

            var selectedDeviceTypeInfo = await _dataContext.DeviceTypes.Where(x => x.Id == viewModel.DeviceTypeId)
                .Select(x => new { x.BlockPrice, x.IsWireless }).FirstAsync(cancellationToken);

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                if (!await _dataContext.TerminalDocuments.AnyAsync(
                        x => x.TerminalId == viewModel.TerminalId &&
                             x.DocumentTypeId == (byte)Enums.DocumentType.SanadMasdoodi, cancellationToken) &&
                    viewModel.PostedFiles.Any(x =>
                        x.DocumentTypeId == (byte)Enums.DocumentType.SanadMasdoodi &&
                        !x.PostedFile.IsValidFormat(".pdf")))
                {
                    return JsonWarningMessage(
                        "لطفاً فایل سند مسدودی را انتخاب نمایید. توجه نمایید که این فایل بایستی با فرمت pdf ارسال شود.");
                }

                if (await _dataContext.Terminals.AnyAsync(
                        x => x.StatusId != (byte)Enums.TerminalStatus.Deleted && x.Id != viewModel.TerminalId &&
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
            terminal.MarketerId = User.IsBranchUser() ? (int)Enums.Marketer.BankOrBranch : viewModel.MarketerId;

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                terminal.BlockDocumentDate = viewModel.BlockDocumentDate;
                terminal.BlockDocumentNumber = viewModel.BlockDocumentNumber;
                terminal.BlockPrice = selectedDeviceTypeInfo.BlockPrice;
                terminal.BlockDocumentStatusId = (byte)Enums.BlockDocumentStatus.WaitingForReview;
                terminal.BlockAccountNumber =
                    $"{accountNumber.Split('-')[0]}-{viewModel.BlockAccountType}-{accountNumber.Split('-')[2]}-{viewModel.BlockAccountRow}";
            }
            else
            {
                terminal.BlockPrice = 0;
                terminal.BlockDocumentDate = null;
                terminal.BlockAccountNumber = null;
                terminal.BlockDocumentNumber = null;
                terminal.BlockDocumentStatusId = (byte)Enums.BlockDocumentStatus.NotRegistered;
            }

            var terminalDocumentTypesToRemove = viewModel.PostedFiles
                .Where(x => x.ForEntityTypeId == (int)Enums.EntityType.Terminal && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.TerminalDocuments.RemoveRange(_dataContext.TerminalDocuments.Where(x =>
                terminalDocumentTypesToRemove.Contains(x.DocumentTypeId) && x.TerminalId == terminal.Id));

            foreach (var item in viewModel.PostedFiles.Where(x =>
                         x.ForEntityTypeId == (int)Enums.EntityType.Terminal && x.PostedFile.IsValidFile()))
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
                .Where(x => x.ForEntityTypeId == (int)Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.MerchantProfileDocuments.RemoveRange(_dataContext.MerchantProfileDocuments.Where(x =>
                merchantProfileDocumentTypesToRemove.Contains(x.DocumentTypeId) &&
                x.MerchantProfileId == terminal.MerchantProfileId));

            #endregion

            foreach (var item in viewModel.PostedFiles.Where(x =>
                         x.ForEntityTypeId == (int)Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile()))
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

            //todo PN Edit PardakhNovin
            if (terminal.PspId == 4)
            {
                using (var pardakhtNovinsercie = new PardakhtNovinService())
                {
                    pardakhtNovinsercie.EditAcceptor(terminal.Id);
                }
                //todo PN Edit PardakhNovin
            }

            if (terminal.PspId == 2)
            {
                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte)Enums.PspCompany.IranKish)
                {
                    using (var irankishService = new NewIranKishService())
                    {
                        var result = irankishService.EditAcceptor(terminal.Id);
                        return result
                            ? JsonSuccessMessage()
                            : JsonSuccessMessage(MessageType.Danger,
                                "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                    }
                }
            }
            if (terminal.PspId == 4)
            {
                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte)Enums.PspCompany.PardakhNovin)
                {
                    using (var irankishService = new PardakhtNovinService())
                    {
                        var result = irankishService.EditAcceptor(terminal.Id);
                        return result.Status == PardakthNovinStatus.Successed && result.SavedID != 0 
                            ? JsonSuccessMessage()
                            : JsonSuccessMessage(MessageType.Danger,
                                "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                    }
                }
            }
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

                    attach.AddRange(_dataContext.TerminalDocuments.Where(a => a.TerminalId == terminal.Id).ToList()
                        .Select(b => new
                            UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String(b.FileData)
                            }).ToList());
                    attach.AddRange(_dataContext.MerchantProfileDocuments
                        .Where(a => a.MerchantProfileId == terminal.MerchantProfileId)
                        .ToList().Select(
                            b => new
                                UploadAttachmentRequestData
                                {
                                    ContentType = b.ContentType,
                                    FileName = b.FileName,
                                    Base64 = Convert.ToBase64String(b.FileData)
                                }).ToList());


                    parsianService.NewAddAcceptor(terminal.Id, attach);
                    var res = parsianService
                        .UpdateStatusForRequestedTerminal(terminal.TopiarId.Value.ToString(), (int)terminal.Id).Result;
                    terminal.InstallStatus = res.InstallStatus;
                    terminal.InstallStatusId = res.InstallStatusId;
                    terminal.InstallationDate = res.InstallationDate;
                    terminal.StepCode = res.StepCode;
                    terminal.StepCodeTitle = res.StepCodeTitle;
                    terminal.ErrorComment = res.Error;
                    terminal.StatusId = (byte)Enums.TerminalStatus.NotReturnedFromSwitch;
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
                if (terminal.StatusId == (byte)Enums.TerminalStatus.New ||
                    terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                    terminal.StatusId == (byte)Enums.TerminalStatus.NotReturnedFromSwitch)
                {
                    terminal.StatusId = (byte)Enums.TerminalStatus.Deleted;
                    await _dataContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    return JsonWarningMessage(
                        "تنها وضعیت های 'ورود بازاریابی'، 'برنگشته از سوئیچ' و 'نیازمند اصلاح' قابلیت حذف دارند.");
                }

                return JsonSuccessMessage();
            }

            query = query.Where(x => x.StatusId == (byte)Enums.TerminalStatus.New);

            terminal = await query.FirstAsync(cancellationToken);
            terminal.StatusId = (byte)Enums.TerminalStatus.Deleted;
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
                    .Where(x => x.Id == terminalId && (x.StatusId == (byte)Enums.TerminalStatus.NeedToReform ||
                                                       x.StatusId == (byte)Enums.TerminalStatus
                                                           .UnsuccessfulReturnedFromSwitch))
                    .Select(x => new
                    {
                        x.Id, x.StatusId, x.PspId, x.ContractNo, x.MerchantProfileId, x.TopiarId,
                        PersianCharRefId = x.MerchantProfile.PersianCharRefId,
                        MerchantProfieNatoionalCode = x.MerchantProfile.NationalCode,
                        IdentityNumber = x.MerchantProfile.IdentityNumber,
                        BirthCrtfctSeriesNumber = x.MerchantProfile.BirthCrtfctSeriesNumber
                    })
                    .FirstOrDefault();

                if (terminal == null)
                    return JsonWarningMessage("پایانه مورد نظر یافت نشد.");

                if (!terminal.PspId.HasValue)
                    return JsonWarningMessage("برای پذیرنده انتخاب نشده است و امکان ثبت درخواست وجود ندارد.");

                bool result;
                // if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                //     terminal.PspId == (byte) Enums.PspCompany.IranKish)
                // {
                //     using (var irankishService = new IranKishService())
                //         result = irankishService.EditAcceptor(terminalId);
                //
                //     return result
                //         ? JsonSuccessMessage()
                //         : JsonSuccessMessage(MessageType.Danger,
                //             "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                // }
                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte)Enums.PspCompany.PardakhNovin)
                {
                    using (var irankishService = new PardakhtNovinService())
                    {
                         var rresult = irankishService.AddAcceptor(terminal.Id);
                        return rresult.Status == PardakthNovinStatus.Successed && rresult.SavedID != 0 
                            ? JsonSuccessMessage()
                            : JsonSuccessMessage(MessageType.Danger,
                                "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                    }
                }
                
                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte)Enums.PspCompany.Fanava)
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform && !terminal.TopiarId.HasValue &&
                    terminal.PspId == (byte)Enums.PspCompany.Parsian)
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

                if (terminal.StatusId == (byte)Enums.TerminalStatus.NeedToReform && terminal.TopiarId.HasValue &&
                    terminal.PspId == (byte)Enums.PspCompany.Parsian)
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
                        requestInqueryInput.RequestCode = (int)terminal.Id;
                        requestInqueryInput.RequestData.ChangeInfoTypeRefId = 31286; //required
                        requestInqueryInput.RequestData.PersonTypeRefId = 31220; //required
                        requestInqueryInput.RequestData.NationalCode = terminal.MerchantProfieNatoionalCode;


                        requestInqueryInput.RequestData.BirthCertificateNumber =
                            terminal.IdentityNumber == "0"
                                ? terminal.MerchantProfieNatoionalCode
                                : terminal.IdentityNumber; //شماره شناسنامه


                        using (var parsianService2 = new ParsianService())
                        {
                            requestInqueryInput.RequestData.PersianCharRefId =
                                parsianService2.GetPersianCharRefId(terminal.PersianCharRefId); //بخش حرفی سری شناسنامه
                        }

                        requestInqueryInput.RequestData.BirthCertificateSeriesNumber =
                            terminal.BirthCrtfctSeriesNumber; // عددی سری شناسنامه


                        var resultt = parsianService.RequestChangeInfo(requestInqueryInput);
                        return resultt.IsSuccess
                            ? JsonSuccessMessage()
                            : JsonSuccessMessage(MessageType.Danger,
                                "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                    }
                }

                if (terminal.PspId == (byte)Enums.PspCompany.Fanava &&
                    terminal.StatusId == (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
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
                requestInqueryInput.RequestData.TopiarId = "7564587";
                var result = parsianService.RequestInQuery(requestInqueryInput, 500665);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Faranegin()
        {
            var mp = _dataContext.MerchantProfiles
                .Include(t => t.Terminals).Where(b => b.Id == 4186334).ToList();

            foreach (var merchantProfile in mp)
            {
                if (!merchantProfile.Terminals.Any())
                    continue;
                var shebaNumber = merchantProfile.Terminals.FirstOrDefault().ShebaNo;
                if (string.IsNullOrEmpty(shebaNumber))
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
                    merchantProfile.BirthCrtfctSeriesNumber = !string.IsNullOrEmpty(response.certificateSeries)
                        ? response.certificateSeries.Split('-')[1]
                        : null;
                    merchantProfile.PersianCharRefId = !string.IsNullOrEmpty(response.certificateSeries)
                        ? response.certificateSeries.Split('-')[0]
                        : null;
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
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public ActionResult GetRequestChangeInfoData(UploadTerminalValidationDataViewModel viewModel)
        {
            var query = _dataContext.ParsianRequestForInfo.Where(a => a.StatusId != 1
                                                                      && (string.IsNullOrEmpty(viewModel
                                                                              .NationalCode) ||
                                                                          a.NationalCode == viewModel.NationalCode)
            ).AsQueryable();


            var data = query.ToList().Select(x => new ParsianRequestForInfoViewModel
                {
                    Id = x.Id,
                    StatusId = x.StatusId,
                    TopiarId = x.TopiarId,
                    Error = x.Error,
                    Date = x.Create.ToPersianDateTime(),
                    NationalCode = x.NationalCode
                })
                .ToList();

            var tsfsafdsa =
                string.IsNullOrEmpty(viewModel.orderClause) ? "NationalCode" : viewModel.orderClause.Split(' ')[0];

            var ascc = string.IsNullOrEmpty(viewModel.orderClause) ? "DESC" : viewModel.orderClause.Split(' ')[1];
            var prop = TypeDescriptor.GetProperties(typeof(ParsianRequestForInfoViewModel))
                .Find(tsfsafdsa, true);


            if (ascc == "DESC")
            {
                var rows = data.Select(x => new ParsianRequestForInfoViewModel
                    {
                        Id = x.Id,
                        TopiarId = x.TopiarId,
                        StatusId = x.StatusId, Error = x.Error,
                        NationalCode = x.NationalCode, Date = x.Date,
                    })
                    .OrderByDescending(x => prop.GetValue(x))
                    .Skip((viewModel.page.Value - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new { rows, totalRowsCount = data.Count });
            }
            else
            {
                var rows = data.Select(x => new ParsianRequestForInfoViewModel
                    {
                        Id = x.Id,
                        TopiarId = x.TopiarId,
                        StatusId = x.StatusId, Error = x.Error,
                        NationalCode = x.NationalCode, Date = x.Date,
                    })
                    .OrderBy(x => prop.GetValue(x))
                    .Skip((viewModel.page.Value - 1) * 20)
                    .Take(20)
                    .ToList();
                return JsonSuccessResult(new { rows, totalRowsCount = data.Count });
            }
        }


        [HttpPost]
        public ActionResult RequestChangeInfo2(string customerNumber)
        {
            using (var parsianService = new ParsianService())
            {
                var merchantProfile = _dataContext.MerchantProfiles.Where(a => a.Id == 4144806)
                    .FirstOrDefault();

              
                //taxcode


                //
                //================ taxcode ================

                var at = DateTime.UtcNow - new DateTime(1970, 1, 1);
                var secondsSinceEpoch = (int)at.TotalSeconds;
                //     
                //     var RequestChangeShopPost = new RequestChangeShopPost
                //     {
                //         RequestData = new RequestChangeShopPostData(),
                //         RequestCode = - secondsSinceEpoch
                //     };
                //     RequestChangeShopPost.RequestData.ChangeInfoTypeRefId =  31399;
                //     RequestChangeShopPost.RequestData.TaxPayerCode = "1605615125"    ; 
                //     RequestChangeShopPost.RequestData.AcceptorCode = "4381288442";     
                // var result1 = parsianService.RequestChangeShopPost(RequestChangeShopPost);
                //   return Json(result1, JsonRequestBehavior.AllowGet);

                // ==========================
              
                
                if (string.IsNullOrEmpty(merchantProfile.CustomerNumber))
                {
                    return JsonErrorMessage("شماره مشتری معتبر نمی باشد");
                }

                if (!TosanService.TryGetCustomerInfo(merchantProfile.CustomerNumber,
                        merchantProfile.CustomerNumber, out var response, out var errorMessage))
                {
                    return JsonErrorMessage(errorMessage);
                }

                at = DateTime.UtcNow - new DateTime(1970, 1, 1);
                secondsSinceEpoch = (int)at.TotalSeconds;

                var requestInqueryInput = new RequestChangeInfoInput2
                {
                    RequestData = new RequestChangeInfoInputData2(),
                    RequestCode = -secondsSinceEpoch
                };
                requestInqueryInput.RequestData.ChangeInfoTypeRefId = 31286;
                requestInqueryInput.RequestData.PersonTypeRefId = 31220;
                requestInqueryInput.RequestData.NationalCode = merchantProfile.NationalCode;
                requestInqueryInput.RequestData.CellPhoneNumber = merchantProfile.Mobile;
                requestInqueryInput.RequestData.BirthDate =
                    merchantProfile.BirthCertificateIssueDate.ToString("yyyy-MM-dd");
                requestInqueryInput.RequestData.BirthCertificateNumber = merchantProfile.IdentityNumber == "0"
                    ? merchantProfile.NationalCode
                    : merchantProfile.IdentityNumber; //شماهر شناسنامه
                requestInqueryInput.RequestData.BirthCertificateSeriesNumber =
                    !string.IsNullOrEmpty(response.certificateSeries) ? response.certificateSeries.Split('-')[1] : null;

                requestInqueryInput.RequestData.PersianCharRefId = GetPersianCharRefId(
                    !string.IsNullOrEmpty(response.certificateSeries) ? response.certificateSeries.Split('-')[0] : "");

                if (!string.IsNullOrEmpty(response.certificateSeries))
                    requestInqueryInput.RequestData.PersianCharRefId =
                        parsianService.GetPersianCharRefId(response.certificateSeries.Split('-')[0]);
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
                .Select(x => new { x.Id, x.Title })
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
                    terminalIdList.Contains(x.Id) && x.StatusId == (byte)Enums.TerminalStatus.New);
                validTerminals.Update(x => new Terminal { PspId = pspId });

                if (validTerminals.Any())
                {
                    var validTerminalsIdList = validTerminals.Select(x => new
                    {
                        x.Id,
                        x.MerchantProfileId
                    }).ToList();
                    switch (pspId)
                    {
                        case (byte)Enums.PspCompany.Fanava:
                        {
                            using (var fanavaService = new FanavaService())
                                fanavaService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }
                        case (byte)Enums.PspCompany.PardakhNovin:
                        {
                            using (var pardakhtNovinService = new PardakhtNovinService())
                                pardakhtNovinService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }
                        case (byte)Enums.PspCompany.IranKish:
                        {
                            using (var irankishService = new NewIranKishService())
                                irankishService.AddAcceptorList(validTerminalsIdList.Select(b => b.Id).ToList());
                            break;
                        }

                        case (byte)Enums.PspCompany.Parsian:
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

        [HttpGet]
        public ActionResult GetCustomerByCode()
        {
            using (var irankishService = new PardakhtNovinService())
            {
                var k = irankishService.GetCustomerByCode(new GetCustomerByCodeRequest()
                {
                    Parameters = new GetCustomerByCodeRequestParameters()
                    {
                        CustomerCode = "2130561187"
                    }
                });
                return Json(k);
            }

            return null;
        }

        [HttpGet]
        public ActionResult IrankishInquiry(string terminalId)
        {
            using (var irankishService = new NewIranKishService())
            {  
                  
                        var result = irankishService.Inquery(terminalId);
                        return  Json( result);
                  
                
            }
        }
 private byte[] CreatePDF2()
        {
            iTextSharp.text.  Document doc = new iTextSharp.text. Document(PageSize.LETTER, 50, 50, 50, 50);

            using (MemoryStream output = new MemoryStream())
            {
                iTextSharp.text.pdf. PdfWriter wri =   iTextSharp.text.pdf.PdfWriter.GetInstance(doc, output);
                doc.Open();

                var header = new Paragraph(" ") {Alignment = Element.ALIGN_CENTER};
                var  paragraph = new Paragraph("   ");
                var phrase = new Phrase(" ");
                var chunk = new Chunk(" ");

                
                doc.Add(header);
                doc.Add(paragraph);
                doc.Add(phrase);
                doc.Add(chunk);

                doc.Close();
                return output.ToArray();
            }

        }
        [HttpGet]
        public ActionResult TestIranKishUpload(long terminalId,string trackingCode)
        {
            var t = _dataContext.Terminals.Include(b => b.TerminalDocuments).Include(a => a.MerchantProfile)
                .FirstOrDefault(b => b.Id == terminalId);

            
 
            
               byte[] malekiyat_10 = CreatePDF2(); 
                        byte[] taahod_15 =CreatePDF2(); 
                        byte[] hoviyati_11 =CreatePDF2(); 
                        byte[] sherkati_13 =CreatePDF2(); 
                    
                        var outputDoc_11 =   PdfReader.Open(new MemoryStream(malekiyat_10), PdfDocumentOpenMode.Import);
                        var outputDoc_11_file = false;
                        var outputDoc_15     =   PdfReader.Open(new MemoryStream(taahod_15), PdfDocumentOpenMode.Import);
                        var outputDoc_15_file = false;

                        var outputDoc_10 =   PdfReader.Open(new MemoryStream(hoviyati_11), PdfDocumentOpenMode.Import);
                        var outputDoc_10_file = false;

                        var outputDoc_13 =   PdfReader.Open(new MemoryStream(sherkati_13), PdfDocumentOpenMode.Import);
                        var outputDoc_13_file = false;

                    
                        
                      
                        foreach (var terminalDocument in t.TerminalDocuments)
                        {
                            var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                            switch (am)
                            {
                                case  "11" :
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_11.AddPage(pp);
                                            outputDoc_11_file = true;
                                        }
                                    }
                                    break;
                               
                                case "10":
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_10.AddPage(pp);
                                            outputDoc_10_file = true;

                                        }
                                    }
                                    break;
                                case "13":
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_13.AddPage(pp);
                                            outputDoc_13_file = true;
                                        }
                                    }
                                    break;
                            }; 
                        
                        }
                     
                        foreach (var terminalDocument in t.MerchantProfile.MerchantProfileDocuments)
                        {
                           
                             var am = getIrankishDocType(terminalDocument.DocumentTypeId);
                            switch (am)
                            {
                                case  "11" :
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_11.AddPage(pp);
                                            outputDoc_11_file = true;

                                        }
                                    }
                                    break;
                              
                                case "10":
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_10.AddPage(pp);
                                            outputDoc_10_file = true;

                                        }
                                    }
                                    break;
                                case "13":
                                    using (PdfDocument pdfDocument = PdfReader.Open(new MemoryStream(terminalDocument.FileData),
                                               PdfDocumentOpenMode.Import))
                                    { 
                                        //A PDF document must be opened with PdfDocumentOpenMode.Import to import pages from it.
                                        foreach (var pp in pdfDocument.Pages)
                                        {
                                            outputDoc_13.AddPage(pp);
                                            outputDoc_13_file = true;

                                        }
                                    }
                                    break;
                            }; 
                        }
                      
                     
                        string filename = $@"D:\\zer\\outputDoc_11_{terminalId}.pdf";
                        if (outputDoc_11.Pages.Count != 1)
                        {
                            outputDoc_11.Pages.RemoveAt(0);
                            outputDoc_11.Save(filename);
                        }

                        filename =$@"D:\\zer\\outputDoc_13_{terminalId}.pdf";
                          if (outputDoc_13.Pages.Count != 1)
                          {
                              outputDoc_13.Pages.RemoveAt(0);
                              outputDoc_13.Save(filename);
                          }

                          filename =$@"D:\\zer\\outputDoc_10_{terminalId}.pdf";
                        if (outputDoc_10.Pages.Count != 1)
                        {
                            outputDoc_10.Pages.RemoveAt(0);
                            outputDoc_10.Save(filename);
                        }

                      

                        if (outputDoc_13_file  )
                        {
                            using(MemoryStream stream = new MemoryStream()) 
                            { 
                                outputDoc_13.Save(stream, true); 
                                sherkati_13 = stream.ToArray(); 
                        
                            }
                        
                            var a13 = new AddDocumentRequest
                            {
                                 TrackingCode = terminalId.ToString(),
                                DocumentType = "13",
                                BankId = 6830,
                                File = Convert.ToBase64String(sherkati_13, 0,    sherkati_13.Length)
                            };
                            if ( t.MerchantProfile.IsLegalPersonality)
                            {
                                
                                using (var irankishService = new NewIranKishService())
                                {
                                    var k =    irankishService.AddDocument(a13,(int)terminalId);
                                    var m = k.Result;
                                }
                            }
                         
                        }
                        //11
                        if (outputDoc_11_file)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                outputDoc_11.Save(stream, true);
                                hoviyati_11 = stream.ToArray();

                            }

                            var a11 = new AddDocumentRequest
                            {
                                TrackingCode =terminalId.ToString(),
                                DocumentType = "11",
                                BankId = 6830,
                                File = Convert.ToBase64String(hoviyati_11, 0, hoviyati_11.Length)
                            };
                          
                            
                                
                                using (var irankishService = new NewIranKishService())
                                {
                                    var k =    irankishService.AddDocument(a11,(int)terminalId);
                                    var m = k.Result;
                                }
                             
                        
                        }

                        //10
                        if (outputDoc_10_file)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                outputDoc_10.Save(stream, true);
                                malekiyat_10 = stream.ToArray();

                            }

                            var a10 = new AddDocumentRequest
                            {
                                TrackingCode = terminalId.ToString(),
                                DocumentType = "10",
                                BankId = 6830,
                                File = Convert.ToBase64String(malekiyat_10, 0, malekiyat_10.Length)
                            };
                          
                            if (! t.MerchantProfile.IsLegalPersonality)
                            {
                                
                                using (var irankishService = new NewIranKishService())
                                {
                                    var k =    irankishService.AddDocument(a10,(int)terminalId);
                                    var m = k.Result;
                                }
                            }
                        }
                        //
                        // //15
                        // if (outputDoc_15_file)
                        // {
                        //     using (MemoryStream stream = new MemoryStream())
                        //     {
                        //         outputDoc_15.Save(stream, true);
                        //         taahod_15 = stream.ToArray();
                        //
                        //     }
                        //
                        //     var a15 = new AddDocumentRequest
                        //     {
                        //       //  TrackingCode = result.data.documentTrackingCode,
                        //         DocumentType = "15",
                        //         BankId = 6830,
                        //         File = Convert.ToBase64String(taahod_15, 0, taahod_15.Length)
                        //     };
                        //     acceptorEntity.Documents.Add( new irankishDocument()
                        //     {
                        //         File = a15.File,
                        //         DocumentType = 15, 
                        //     });
                        //    
                        // }
                        //


            return new JsonResult();
        }

        private string getIrankishDocType(long terminalDocumentDocumentType)
        {
            switch (terminalDocumentDocumentType)
            {
                case  0 :
                    return "10";
                case  1 :
                    return "55";
                case  9 :
                    return  "55";
                case  10 :
                    return "11";
                case  11 :
                    return  "11";
              
                case  13 :
                    return  "11";
                case  14 :
                    return "11";
                case  15 :
                    return "13";
                case  16 :
                    return "13";
                case  17 :
                    return "13";
            }

            return "55";
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

        private bool AddAcceptor(long terminalId, byte? pspId, bool? IsVirtualStore)
        {
            var result = false;

            var terminal = _dataContext.Terminals.FirstOrDefault(b => b.Id == terminalId);
            switch (pspId)
            {
                case (byte)Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);
                    break;
                }

                case (byte)Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = irankishService.AddAcceptor(terminalId);
                    break;
                }

                case (byte)Enums.PspCompany.PardakhNovin:

                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
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
                        var k = pardakhtNovinService.AddAcceptor(terminalId);
                        result = k.Data != null && !string.IsNullOrEmpty(k.Data.FollowupCode);
                    }

                    break;
                }
                case (byte)Enums.PspCompany.Parsian:
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