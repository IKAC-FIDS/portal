using Dapper;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.DataModel;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using RestSharp;
using Newtonsoft.Json;

namespace TES.Merchant.Web.UI.Controllers
{
    public class Transaction
    {
        public long BranchId { get; set; } // کد شعبه
        public string TerminalNo { get; set; } // شماره ترمینال 
        public int? HighTransactionCount { get; set; } // تعداد تراکنش های بالا
        public int? LowTransactionCount { get; set; } //تعداد تراکنش های پایین
        public int? WithoutTransactionCount { get; set; } //تعداد بدون تراکنش
        public int? TotalTransactionCount { get; set; } // مجموع تعداد کل تراکنش ها
        public long? SumPriceWirelessTransaction { get; set; } //مجموع مبلغ تراکنش های سیار 
        public long? SumPriceConstTransaction { get; set; } // مجموع مبلغ تراکنش های ثابت
        public long? SumPricePosTransaction { get; set; } //مجموع مبلغ تراکنش های پز

        public long? TotalSumTransaction { get; set; } //مجموع مبلغ کل تراکنش ها
        // public long SumPriceTransaction { get; set; } 
    }

    public class GetReportResult
    {
        public int CountMPOSDevice { get; set; }
        public string PercentHihgTransaction { get; set; }
        public string PercentWithoutTransaction { get; set; }
        public string PercentLowTransaction { get; set; }
        public string PercentTotalLowAndWithoutTransaction { get; set; }
        public string AvgSumPriceTransaction { get; set; }
        public long? SumPriceTransaction { get; set; }
        public long? SumPricePosTransaction { get; set; }
        public long? SumPriceConstTransaction { get; set; }
        public long? SumPriceWirelessTransaction { get; set; }
        public int? CountTotalTransaction { get; set; }
        public int? CountWithoutTransaction { get; set; }
        public int? CountLowTransaction { get; set; }
        public int? CountHihgTransaction { get; set; }
        public string PercentConst { get; set; }
        public string PercentWireless { get; set; }
        public int CountConstDevice { get; set; }
        public int CountWirelessDevice { get; set; }
        public int CountTotalDevice { get; set; }
        public string Title { get; set; }
        public long Id { get; set; }
    }

    public class ReportController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public ReportController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> BankInvoice()
        {
            ViewBag.InvoiceTypeList =
                (await _dataContext.InvoiceTypes.Select(x => new {x.Id, x.Title}).ToListAsync()).ToSelectList(
                    x => x.Id,
                    x => x.Title);
          
            return View();
        }

        public async Task<ActionResult> TerminalWage()
        {
            ViewBag.InvoiceTypeList =
                (await _dataContext.InvoiceTypes.Select(x => new {x.Id, x.Title}).ToListAsync()).ToSelectList(
                    x => x.Id,
                    x => x.Title);
            // var message = _dataContext.Messages.ToList();
            // ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
            //                                          && (d.UserId == CurrentUserId ||
            //                                              d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            // ViewBag.InProgressMessage = message.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsMessageManagerUser()));
            //
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


        [CustomAuthorize]
        public ActionResult BranchRanking()
        {
            // var message = _dataContext.Messages.ToList();
            // ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
            //                                          && (d.UserId == CurrentUserId ||
            //                                              d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            // ViewBag.InProgressMessage = message.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsMessageManagerUser()));
            //
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

        [CustomAuthorize]
        public async Task<ActionResult> GetBranchRankingData(int year, int month, string groupBy,
            CancellationToken cancellationToken)
        {
            if (groupBy.Equals("city"))
            {
                var getBranchRankingDataByCityResult = await _dataContext.GetBranchRankingDataByCity(year, month,
                    CurrentUserBranchId, User.IsSupervisionUser(), User.IsBranchUser(),
                    User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser(), cancellationToken);

                return JsonSuccessResult(
                    getBranchRankingDataByCityResult.OrderByDescending(x => x.TotalTransactionPricePerPos));
            }

            if (groupBy.Equals("orgunit"))
            {
                var getBranchRankingDataByOrganizationUnitResult =
                    await _dataContext.GetBranchRankingDataByOrganizationUnit(year, month, CurrentUserBranchId,
                        User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                        User.IsCountyBranchManagementUser(), cancellationToken);

                return JsonSuccessResult(
                    getBranchRankingDataByOrganizationUnitResult.OrderByDescending(x =>
                        x.TotalTransactionPricePerPos));
            }

            if (groupBy.Equals("state"))
            {
                var getBranchRankingDataByStateResult = await _dataContext.GetBranchRankingDataByState(year, month,
                    CurrentUserBranchId, User.IsSupervisionUser(), User.IsBranchUser(),
                    User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser(), cancellationToken);

                return JsonSuccessResult(
                    getBranchRankingDataByStateResult.OrderByDescending(x => x.TotalTransactionPricePerPos));
            }

            if (groupBy.Equals("branchManagment"))
            {
                var getBranchRankingDataByBranchManagmentResult =
                    await _dataContext.GetBranchRankingDataByBranchManagment(year, month, CurrentUserBranchId,
                        User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                        User.IsCountyBranchManagementUser(), cancellationToken);

                return JsonSuccessResult(
                    getBranchRankingDataByBranchManagmentResult.OrderByDescending(
                        x => x.TotalTransactionPricePerPos));
            }

            return JsonUnsuccessResult();
        }

        [CustomAuthorize]
        public ActionResult TerminalTypes()
        {
           // var message = _dataContext.Messages.ToList();
            // ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
            //                                          && (d.UserId == CurrentUserId ||
            //                                              d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            // ViewBag.InProgressMessage = message.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsMessageManagerUser()));
            //
            // var cardmessage = _dataContext.CardRequest.ToList();
            // ViewBag.ReadyForDeliverCardRequst = cardmessage.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.Open
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsCardRequestManager()));
            // ViewBag.InProgressCardRequstMessage = cardmessage.Count(d =>
            //     d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
            //     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                   || User.IsCardRequestManager()));
            return View();
        }

        [CustomAuthorize]
        public async Task<ActionResult> GetTerminalTypesData(long? branchId, string branchTitle,
            CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals
                .Where(x => x.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                            x.StatusId != (byte) Enums.TerminalStatus.Revoked &&
                            !_dataContext.RevokeRequests.Any(y => y.TerminalNo == x.TerminalNo));

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
                // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
                if (User.IsBranchUser())
                    query = query.Where(x => x.BranchId == CurrentUserBranchId);
                // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
                if (User.IsSupervisionUser() && CurrentUserBranchId.HasValue)
                {
                    var childOrganizationUnitIdList =
                        await _dataContext.GetChildOrganizationUnits(CurrentUserBranchId.Value);
                    query = query.Where(x => childOrganizationUnitIdList.Contains(x.BranchId));
                }
            }

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(branchTitle))
            {
                query = query.Where(x => x.Branch.Title.Contains(branchTitle));
            }

            var data = await query.GroupBy(x => new {x.BranchId, x.Branch.Title})
                .Select(x => new TerminalTypesReportResultModel
                {
                    Id = x.Key.BranchId,
                    Title = x.Key.Title,
                    WirelessCount = x.Count(y => y.DeviceType.IsWireless),
                    WithWireCount = x.Count(y => !y.DeviceType.IsWireless)
                })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);
            return JsonSuccessResult(data);
        }

        [CustomAuthorize]
        public async Task<ActionResult> GetTerminals()
        {
            //در این قسمت ابتدا دسترسی ها بررسی می شود بر اساس دسترسی ویو نمایش داده می شود 
            var query = _dataContext.Terminals
                .Where(x => x.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                            x.StatusId != (byte) Enums.TerminalStatus.Revoked &&
                            !_dataContext.RevokeRequests.Any(y => y.TerminalNo == x.TerminalNo));
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
                // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
                if (User.IsSupervisionUser() && CurrentUserBranchId.HasValue)
                {
                    var childOrganizationUnitIdList =
                        await _dataContext.GetChildOrganizationUnits(CurrentUserBranchId.Value);
                    query = query.Where(x => childOrganizationUnitIdList.Contains(x.BranchId));
                }
            }

            return View();
        }

        // در ابتدا که صفحه لود  می شود چون کد شعبه یا نام شعبه وارد نشده است اطلاعات مربوط به کل شعبه ها نمایش داده می شود 
        [CustomAuthorize]
        public async Task<ActionResult> GetData(long? branchId, string branchTitle,
            CancellationToken cancellationToken)
        {
            var sql = _dataContext.Database.SqlQuery<Transaction>(
                    "GetTransaction" // خروجی اس پی  که به صورت لیست است در یک متغیر ذخیره می کنیم
                )
                .ToList();

            if (User.IsBranchUser()) // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
                sql = sql.Where(x => x.BranchId == CurrentUserBranchId).ToList();
            if (User.IsSupervisionUser() && CurrentUserBranchId.HasValue
            ) // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
            {
                var childOrganizationUnitIdList =
                    await _dataContext.GetChildOrganizationUnits(CurrentUserBranchId.Value);
                sql = sql.Where(x => childOrganizationUnitIdList.Contains(x.BranchId)).ToList();
            }

            if (branchId.HasValue) //اگر فیلتر شعبه مشخص شده باشد 
            {
                var DeviceTypes = _dataContext.DeviceTypes.Select(t => t.Id).ToList();
                var totalDevice = _dataContext.NormalReps.Where(a =>
                    DeviceTypes.Contains(a.DeviceTypeId));
                var WireLessTypes = _dataContext.DeviceTypes.Where(d => d.IsWireless).Select(t => t.Id);
                var wirelessDevice = _dataContext.NormalReps.Where(a =>
                    WireLessTypes.Contains(a.DeviceTypeId));
                var data = _dataContext.OrganizationUnits
                    .Select(s => new
                    {
                        Id = s.Id,
                        Title = s.Title,
                        TotalDeviceCount = totalDevice.Count(nr => nr.BranchId == s.Id),
                        WirelessDevice = wirelessDevice.Count(nr => nr.BranchId == s.Id),
                        ConstDevice = totalDevice.Count(nr => nr.BranchId == s.Id) -
                                      wirelessDevice.Count(nr => nr.BranchId == s.Id),
                        PercentWireless = (totalDevice.Count(nr => nr.BranchId == s.Id) == 0
                            ? 0
                            : (totalDevice.Count(nr => nr.BranchId == s.Id) -
                               wirelessDevice.Count(nr => nr.BranchId == s.Id)) /
                              totalDevice.Count(nr => nr.BranchId == s.Id)) * 100,
                        PercentConst = (totalDevice.Count(nr => nr.BranchId == s.Id) == 0
                            ? 0
                            : totalDevice.Count(nr => nr.BranchId == s.Id) -
                              wirelessDevice.Count(nr => nr.BranchId == s.Id) /
                              (totalDevice.Count(nr => nr.BranchId == s.Id))) * 100,
                    }).ToListAsync(cancellationToken);
                return JsonSuccessResult(data);
            }
            else // همه شعب هنگام لود شدن صفحه نمایش داده می شوند
            {
                //لیست همه دستگاه ها  
                var DeviceTypes = _dataContext.DeviceTypes.Select(t => t.Id).ToList();
                //موجود باشد devicetype   در جدول مرجع یعنی normalrep  بررسی می شود که همه آی دی ها در جدول 
                var totalDevice = _dataContext.NormalReps.Where(a =>
                    DeviceTypes.Contains(a.DeviceTypeId)).ToList();
                //لیست دستگاه های سیار   
                var WireLessTypes = _dataContext.DeviceTypes.Where(d => d.IsWireless).Select(t => t.Id);
                //
                var wirelessDevice = _dataContext.NormalReps.Where(a =>
                    WireLessTypes.Contains(a.DeviceTypeId)).ToList();
                //لیست  دستگاه های پز  
                var MPOSType = _dataContext.DeviceTypes.Where(d => d.IsWireless && d.Code == "MPOS")
                    .Select(t => t.Id);
                var MPOSDevice = _dataContext.NormalReps.Where(a =>
                    MPOSType.Contains(a.DeviceTypeId)).ToList();
                //شماره ترمینال دستگاه های سیار را به دست می آوریم 
                // var TerminalNoList = _dataContext.NormalReps.Where(a => WireLessTypes.Contains(a.DeviceTypeId))
                //.Select(a => a.TerminalNum);
                //var totalWirelessTransaction =
                // _dataContext.NormalReps.Where(a => TerminalNoList.Contains(a.TerminalNum));
                //NormalRep لیست از مقادیر جدول 
                var normalrep = _dataContext.NormalReps.ToList();
                //OrganizationUnit لیست از مقادیر جدول 
                var org = _dataContext.OrganizationUnits.ToList();
                // Transaction s = new Transaction();

                var result = new List<GetReportResult>();
                foreach (var organizationUnit in org)
                {
                    var res = new GetReportResult();

                    var nistesh = sql.FirstOrDefault(b => b.BranchId == organizationUnit.Id);
                    if (nistesh == null)
                        continue;
                    try
                    {
                        res.Id = organizationUnit.Id; //کد شعبه
                        res.Title = organizationUnit.Title; // نام شعبه
                        // تعداد کل دستگاه های کارتخوان
                        res.CountTotalDevice =
                            totalDevice.Count(nr => nr.BranchId == organizationUnit.Id);
                        //تعداد دستگاه های سیار 
                        res.CountWirelessDevice =
                            wirelessDevice.Count(nr => nr.BranchId == organizationUnit.Id);
                        // تعداد دستگاه های ثابت
                        res.CountConstDevice = totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) -
                                               wirelessDevice.Count(nr => nr.BranchId == organizationUnit.Id);
                        res.CountMPOSDevice = MPOSDevice.Count(nr => nr.BranchId == organizationUnit.Id);
                        // درصد تعداد دستگاه های سیار
                        res.PercentWireless = (
                            string.Format("{0:0.00}",
                                (totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) == 0
                                    ? 0
                                    : (double) ((double) wirelessDevice.Count(nr =>
                                                    nr.BranchId == organizationUnit.Id)
                                                / (double) totalDevice.Count(nr =>
                                                    nr.BranchId == organizationUnit.Id))
                                ) * 100));
                        // درصد تعداد دستگاه های ثابت
                        res.PercentConst = string.Format("{0:0.00}",
                            (totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) == 0
                                ? 0
                                : (double) ((double) ((totalDevice.Count(nr => nr.BranchId == organizationUnit.Id)
                                                       - wirelessDevice.Count(nr =>
                                                           nr.BranchId == organizationUnit.Id)))
                                            / (double) totalDevice.Count(nr =>
                                                nr.BranchId == organizationUnit.Id))) *
                            100);

                        //تعداد تراکنش بالا
                        res.CountHihgTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.HighTransactionCount;
                        //تعداد تراکنش پایین
                        res.CountLowTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.LowTransactionCount;
                        //تعداد فاقد تراکنش
                        res.CountWithoutTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.WithoutTransactionCount;
                        //تعدا کل تراکنش ها 
                        res.CountTotalTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.TotalTransactionCount;
                        //جمع مبلغ تراکنش سیار
                        res.SumPriceWirelessTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.SumPriceWirelessTransaction;
                        //جمع مبلغ تراکنش ثابت 
                        res.SumPriceConstTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.SumPriceConstTransaction;
                        //جمع مبلغ تراکنش MPOS 
                        res.SumPricePosTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                            ?.SumPricePosTransaction;
                        res.SumPriceTransaction = sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                                      ?.SumPriceWirelessTransaction +
                                                  sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                                      ?.SumPriceConstTransaction +
                                                  sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                                      ?.SumPricePosTransaction;
                        //درصد تراکنش بالا 
                        res.PercentHihgTransaction = (string.Format("{0:0.00}",
                            (sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?
                                .TotalTransactionCount == 0
                                ? 0
                                : (double) (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      ?.HighTransactionCount
                                  / (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      ?.TotalTransactionCount) * 100));
                        //sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id) : baraye organizationunit.id 1 null hast
                        //درصد تراکنش پایین   
                        res.PercentLowTransaction = (string.Format("{0:0.00}",
                            (sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?.TotalTransactionCount == 0
                                ? 0
                                : (double) (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      .LowTransactionCount
                                  / (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      ?.TotalTransactionCount) * 100));

                        //درصد فاقد تراکنش
                        res.PercentWithoutTransaction = (string.Format("{0:0.00}",
                            (sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?.TotalTransactionCount == 0
                                ? 0
                                : (double) (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      .WithoutTransactionCount
                                  / (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                      ?.TotalTransactionCount) * 100));
                        //درصد مجموع فاقد تراکنش و تراکنش پایین
                        res.PercentTotalLowAndWithoutTransaction =
                            (string.Format("{0:0.00}",
                                sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?.TotalTransactionCount ==
                                0
                                    ? 0
                                    : (double) (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                          .LowTransactionCount /
                                      (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                          ?.TotalTransactionCount * 100 +
                                      (double) (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                          .WithoutTransactionCount /
                                      (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                          ?.TotalTransactionCount * 100));
                        res.AvgSumPriceTransaction = string.Format("{0:0.00}",
                            sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                ?.SumPriceWirelessTransaction +
                            sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?.SumPriceConstTransaction +
                            sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)?.SumPricePosTransaction
                            / (double) sql.FirstOrDefault(a => a.BranchId == organizationUnit.Id)
                                ?.TotalTransactionCount);
                        //   TotalCountTransaction = totalWirelessTransaction.Count(nr => nr.BranchId == organizationUnit.Id)

                        result.Add(res);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                var data = result;
                //var data = org
                // .Select(organizationUnit => new
                // {
                //  Id = organizationUnit.Id, //کد شعبه
                //  Title = organizationUnit.Title, // نام شعبه
                //         // تعداد کل دستگاه های کارتخوان
                //   CountTotalDevice =
                //      totalDevice.Count(nr => nr.BranchId == organizationUnit.Id),
                //تعداد دستگاه های سیار 
                //   CountWirelessDevice =
                //       wirelessDevice.Count(nr => nr.BranchId == organizationUnit.Id),
                // تعداد دستگاه های ثابت
                //  CountConstDevice = totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) -
                //                wirelessDevice.Count(nr => nr.BranchId == organizationUnit.Id),
                //  CountMPOSDevice = MPOSDevice.Count(nr => nr.BranchId == organizationUnit.Id),
                // درصد تعداد دستگاه های سیار
                // PercentWireless = (
                //  string.Format("{0:0.00}", (totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) == 0
                //          ? 0
                //         : (double) ((double) wirelessDevice.Count(nr => nr.BranchId == organizationUnit.Id)
                //                      / (double) totalDevice.Count(nr => nr.BranchId == organizationUnit.Id))
                //      ) * 100)),
                // درصد تعداد دستگاه های ثابت
                //PercentConst = string.Format("{0:0.00}",
                // (totalDevice.Count(nr => nr.BranchId == organizationUnit.Id) == 0
                //    ? 0
                //        : (double) ((double) ((totalDevice.Count(nr => nr.BranchId == organizationUnit.Id)
                //                            - wirelessDevice.Count(nr =>
                //                                nr.BranchId == organizationUnit.Id)))
                //              / (double) totalDevice.Count(nr => nr.BranchId == organizationUnit.Id))) *
                //    100),

                //تعداد تراکنش بالا
                //  CountHihgTransaction = sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.HighTransactionCount,
                //تعداد تراکنش پایین
                //   CountLowTransaction = sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.LowTransactionCount,
                //تعداد فاقد تراکنش
                //   CountWithoutTransaction = sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.WithoutTransactionCount, 
                //تعدا کل تراکنش ها 
                //   CountTotalTransaction = sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount,
                //جمع مبلغ تراکنش سیار
                // SumPriceWirelessTransaction = sql.FirstOrDefault(a =>a.BranchId == organizationUnit.Id)?.SumPriceWirelessTransaction,
                //جمع مبلغ تراکنش ثابت 
                //  SumPriceConstTransaction = sql.FirstOrDefault(a =>a.BranchId == organizationUnit.Id)?.SumPriceConstTransaction,
                //جمع مبلغ تراکنش MPOS 
                // SumPricePosTransaction = sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.SumPricePosTransaction,
                //     SumPriceTransaction = sql.FirstOrDefault(a =>a.BranchId == organizationUnit.Id)?.SumPriceWirelessTransaction+
                //       sql.FirstOrDefault(a =>a.BranchId == organizationUnit.Id)?.SumPriceConstTransaction+
                //    sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.SumPricePosTransaction,
                //درصد تراکنش بالا 
                //    PercentHihgTransaction = (string.Format("{0:0.00}",(sql.FirstOrDefault(a=> a.BranchId == organizationUnit.Id)?
                //                              .TotalTransactionCount   == 0 ? 0
                //           : (double)(double)sql.FirstOrDefault(a=> a.BranchId == organizationUnit.Id)?.HighTransactionCount
                //        / (double)sql.FirstOrDefault(a=> a.BranchId == organizationUnit.Id)?.TotalTransactionCount) * 100)),
                //درصد تراکنش پایین   
                //  PercentLowTransaction = (string.Format("{0:0.00}",(sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount == 0 ? 0
                //    :(double)(double) sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id).LowTransactionCount 
                //  /(double)sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount) * 100)),


                //درصد فاقد تراکنش
                //   PercentWithoutTransaction = (string.Format("{0:0.00}",(sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount == 0 ? 0
                //        : (double)(double) sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id).WithoutTransactionCount /
                //  (double) sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount) * 100)),
                //       //درصد مجموع فاقد تراکنش و تراکنش پایین
                // PercentTotalLowAndWithoutTransaction =
                // (string.Format("{0:0.00}",sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount == 0
                // ? 0
                // : (double)(double)sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id).LowTransactionCount /
                //   (double) sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount * 100 +
                //  (double)(double) sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id).WithoutTransactionCount /
                //     (double)  sql.FirstOrDefault(a=>a.BranchId == organizationUnit.Id)?.TotalTransactionCount * 100)),
                //متوسط مبلغ تراکنش بر تعداد تراکنش
                // AvgSumPriceTransaction = string.Format("{0:0.00}",sql.FirstOrDefault(a => a.BranchId==organizationUnit.Id)?.SumPriceWirelessTransaction+
                //                sql.FirstOrDefault(a =>a.BranchId ==organizationUnit.Id)?.SumPriceConstTransaction+
                //                   sql.FirstOrDefault(a=>a.BranchId==organizationUnit.Id)?.SumPricePosTransaction
                //               /(double)sql.FirstOrDefault(a=> a.BranchId == organizationUnit.Id)?.TotalTransactionCount),
                //    TotalCountTransaction = totalWirelessTransaction.Count(nr => nr.BranchId == organizationUnit.Id)
                //جمع مبلغ تراکنش
                //SumPriceTransaction = sql.FirstOrDefault(a=> a.BranchId == organizationUnit.Id)?.SumPriceTransaction,
                //     }) ;
                return JsonSuccessResult(data);
            }
        }


        public async Task<ActionResult> ExportData() //string params
        {
            var totalDevice = _dataContext.NormalReps.Where(a =>
                _dataContext.DeviceTypes.Select(t => t.Id).Contains(a.DeviceTypeId));
            var wirelessDevice = _dataContext.NormalReps.Where(a =>
                _dataContext.DeviceTypes.Where(d => d.IsWireless).Select(t => t.Id).Contains(a.DeviceTypeId));
            var constDevice = _dataContext.NormalReps.Where(a =>
                _dataContext.DeviceTypes.Where(d => d.IsWireless).Select(t => t.Id).Contains(a.DeviceTypeId));
            // var posDevice = _dataContext.NormalReps.Where(a =>
            //     _dataContext.DeviceTypes.Where(d => d.IsWireless).Select(t => t.Id).Contains(a.DeviceTypeId));
            var data = _dataContext.OrganizationUnits
                .Select(o => new
                {
                    Id = o.Id,
                    Title = o.Title,
                    TotalDeviceCount = totalDevice.Count(nr => nr.BranchId == o.Id),
                    WirelessCount = wirelessDevice.Count(nr => nr.BranchId == o.Id),
                    ConstDevice = constDevice.Count(nr => nr.BranchId == o.Id),
                    //  PosDevice = posDevice.Count(nr => nr.BranchId == o.Id),
                    PercentWireless = (wirelessDevice.Count(nr => nr.BranchId == o.Id) /
                                       totalDevice.Count(nr => nr.BranchId == o.Id)) * 100,
                    PercentConst = (constDevice.Count(nr => nr.BranchId == o.Id) /
                                    totalDevice.Count(nr => nr.BranchId == o.Id)) * 100,
                }).ToList();
            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;
                var worksheet = workbook.Worksheets.Add("گزارش درصد دستگاه ثابت");
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
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 25;
                worksheet.Column(7).Width = 25;
                worksheet.Column(8).Width = 25;
                worksheet.Cells[1, 1].Value = "کد شعبه";
                worksheet.Cells[1, 2].Value = "نام شعبه";
                worksheet.Cells[1, 3].Value = "تعداد دستگاه های کارتخوان";
                worksheet.Cells[1, 6].Value = "تعداد دستگاه های سیار";
                worksheet.Cells[1, 4].Value = "تعداد دستگاه های ثابت";
                worksheet.Cells[1, 5].Value = "تعداد دستگاه های MPOS";
                worksheet.Cells[1, 8].Value = "دستگاه سیار (درصد)";
                worksheet.Cells[1, 7].Value = "دستگاه ثابت (درصد)";
                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Id;
                    worksheet.Cells[rowNumber, 2].Value = item.Title;
                    worksheet.Cells[rowNumber, 3].Value = item.TotalDeviceCount;
                    worksheet.Cells[rowNumber, 4].Value = item.WirelessCount;
                    worksheet.Cells[rowNumber, 5].Value = item.ConstDevice;
                    //  worksheet.Cells[rowNumber, 6].Value = item.PosDevice;
                    worksheet.Cells[rowNumber, 7].Value = item.PercentConst;
                    worksheet.Cells[rowNumber, 8].Value = item.PercentWireless;
                    rowNumber++;
                }

                var dirPath = Server.MapPath("~/App_Data/DataExportFiles");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                var fileKey = $"Terminals-{DateTime.Now.ToPersianDate()}-{Guid.NewGuid()}".ToValidFileName();
                package.SaveAs(new FileInfo(Path.Combine(dirPath, fileKey + ".xlsx")));
                return JsonSuccessResult(fileKey);
            }
        }

        [CustomAuthorize]
        public async Task<ActionResult> DownloadTerminalTypesData(long? branchId, string branchTitle,
            CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals
                .Where(x => x.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                            x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                            x.StatusId != (byte) Enums.TerminalStatus.Revoked &&
                            !_dataContext.RevokeRequests.Any(y => y.TerminalNo == x.TerminalNo));

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

            if (branchId.HasValue)
            {
                query = query.Where(x => x.BranchId == branchId);
            }

            if (!string.IsNullOrEmpty(branchTitle))
            {
                query = query.Where(x => x.Branch.Title.Contains(branchTitle));
            }

            var data = await query.GroupBy(x => new {x.BranchId, x.Branch.Title})
                .Select(x => new TerminalTypesReportResultModel
                {
                    Id = x.Key.BranchId,
                    Title = x.Key.Title,
                    WirelessCount = x.Count(y => y.DeviceType.IsWireless),
                    WithWireCount = x.Count(y => !y.DeviceType.IsWireless)
                })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("گزارش وضعیت درخواست ها");
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
                worksheet.Column(6).Width = 34;
                worksheet.Column(7).Width = 31;
                worksheet.Column(8).Width = 31;

                worksheet.Cells[1, 1].Value = "کد شعبه";
                worksheet.Cells[1, 2].Value = "نام شعبه";
                worksheet.Cells[1, 3].Value = "تعداد دستگاه های کارتخوان";
                worksheet.Cells[1, 4].Value = "تعداد دستگاه ثابت";
                worksheet.Cells[1, 5].Value = "تعداد دستگاه سیار";
                worksheet.Cells[1, 6].Value = "دستگاه ثابت (درصد)";
                worksheet.Cells[1, 7].Value = "دستگاه سیار (درصد)";
                worksheet.Cells[1, 8].Value = "مجاز به ثبت درخواست بیسیم";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Id;
                    worksheet.Cells[rowNumber, 2].Value = item.Title;
                    worksheet.Cells[rowNumber, 3].Value = item.TotalCount;
                    worksheet.Cells[rowNumber, 4].Value = item.WithWireCount;
                    worksheet.Cells[rowNumber, 5].Value = item.WirelessCount;
                    worksheet.Cells[rowNumber, 6].Value = item.WithWirePercentage;
                    worksheet.Cells[rowNumber, 7].Value = item.WirelessPercentage;
                    worksheet.Cells[rowNumber, 8].Value = item.AllowWireless ? "بله" : "خیر";
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TerminalTypes.xlsx");
                }
            }
        }

        [CustomAuthorize]
        public async Task<ActionResult> DownloadBranchRankingData(int year, int month, string groupBy,
            CancellationToken cancellationToken)
        {
            List<BranchRankingData> data;
            string idHeaderText;
            string titleHeaderText;

            if (groupBy.Equals("city"))
            {
                data = await _dataContext.GetBranchRankingDataByCity(year, month, CurrentUserBranchId,
                    User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                    User.IsCountyBranchManagementUser(), cancellationToken);
                idHeaderText = "کد شهر";
                titleHeaderText = "نام شهر";
            }
            else if (groupBy.Equals("state"))
            {
                var tempData = await _dataContext.GetBranchRankingDataByState(year, month, CurrentUserBranchId,
                    User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                    User.IsCountyBranchManagementUser(), cancellationToken);
                data = tempData.Select(x => new BranchRankingData
                {
                    HighTransactionCount = x.HighTransactionCount,
                    Id = x.Id,
                    LowTransactionCount = x.LowTransactionCount,
                    PersianLocalYearMonth = x.PersianLocalYearMonth,
                    Title = x.Title,
                    TotalTransactionCount = x.TotalTransactionCount,
                    TotalTransactionSum = x.TotalTransactionSum,
                    WirelessTerminalCount = x.WirelessTerminalCount,
                    WithoutTransactionCount = x.WithoutTransactionCount,
                    WithWireTerminalCount = x.WithWireTerminalCount
                }).ToList();

                idHeaderText = "کد استان";
                titleHeaderText = "نام استان";
            }
            else if (groupBy.Equals("orgunit"))
            {
                data = await _dataContext.GetBranchRankingDataByOrganizationUnit(year, month, CurrentUserBranchId,
                    User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                    User.IsCountyBranchManagementUser(), cancellationToken);
                idHeaderText = "کد شعبه";
                titleHeaderText = "نام شعبه";
            }
            else if (groupBy.Equals("branchManagment"))
            {
                data = await _dataContext.GetBranchRankingDataByBranchManagment(year, month, CurrentUserBranchId,
                    User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(),
                    User.IsCountyBranchManagementUser(), cancellationToken);
                idHeaderText = "کد منطقه";
                titleHeaderText = "نام منطقه";
            }
            else
            {
                return null;
            }

            using (var package = new ExcelPackage())
            {
                var workbook = package.Workbook;

                var worksheet = workbook.Worksheets.Add("رده بندی بر اساس  مبلغ تراکنش");
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
                worksheet.Column(6).Width = 34;
                worksheet.Column(7).Width = 31;
                worksheet.Column(8).Width = 25;
                worksheet.Column(9).Width = 18;
                worksheet.Column(10).Width = 20;
                worksheet.Column(11).Width = 18;
                worksheet.Column(12).Width = 18;
                worksheet.Column(13).Width = 47;

                worksheet.Cells[1, 1].Value = idHeaderText;
                worksheet.Cells[1, 2].Value = titleHeaderText;
                worksheet.Cells[1, 3].Value = "تعداد دستگاه های کارتخوان";
                worksheet.Cells[1, 4].Value = "تعداد دستگاه منصوبه ثابت";
                worksheet.Cells[1, 5].Value = "تعداد دستگاه منصوبه سیار";
                worksheet.Cells[1, 6].Value = "پُر‌تراکنش";
                worksheet.Cells[1, 7].Value = "کم‌تراکنش";
                worksheet.Cells[1, 8].Value = "فاقد تراکنش";
                worksheet.Cells[1, 9].Value = "جمع مبلغ تراکنش";
                worksheet.Cells[1, 10].Value = "جمع تعداد تراکنش";
                worksheet.Cells[1, 11].Value = "تعداد تراکنش بر دستگاه کارتخوان";
                worksheet.Cells[1, 12].Value = "مبلغ تراکنش بر دستگاه کارتخوان";
                worksheet.Cells[1, 13].Value = "رتبه شعب براساس بالاترین متوسط مبلغ تراکنش بر دستگاه کارتخوان";

                var rowNumber = 2;
                foreach (var item in data.OrderByDescending(x => x.TotalTransactionPricePerPos))
                {
                    worksheet.Cells[rowNumber, 1].Value = item.Id;
                    worksheet.Cells[rowNumber, 2].Value = item.Title;
                    worksheet.Cells[rowNumber, 3].Value = item.TotalTerminalCount;
                    worksheet.Cells[rowNumber, 4].Value = item.WithWireTerminalCount;
                    worksheet.Cells[rowNumber, 5].Value = item.WirelessTerminalCount;
                    worksheet.Cells[rowNumber, 6].Value = item.HighTransactionCount;
                    worksheet.Cells[rowNumber, 7].Value = item.LowTransactionCount;
                    worksheet.Cells[rowNumber, 8].Value = item.WithoutTransactionCount;
                    worksheet.Cells[rowNumber, 9].Value = item.TotalTransactionSum;
                    worksheet.Cells[rowNumber, 10].Value = item.TotalTransactionCount;
                    worksheet.Cells[rowNumber, 11].Value = item.TotalTransactionCountPerPos;
                    worksheet.Cells[rowNumber, 12].Value = item.TotalTransactionPricePerPos;
                    worksheet.Cells[rowNumber, 13].Value = rowNumber - 1;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "BranchRanking.xlsx");
                }
            }
        }

        [CustomAuthorize]
        public ActionResult BranchTransactionStatus(long? branchId, int? fromYear, int? fromMonth, int? toYear,
            int? toMonth)
        {
            if (User.IsBranchUser())
            {
                branchId = CurrentUserBranchId;
            }
            else if (User.IsSupervisionUser())
            {
                ViewBag.BranchList = _dataContext.OrganizationUnits
                    .Where(x => x.ParentId == CurrentUserBranchId)
                    .Select(x => new {x.Id, x.Title})
                    .ToList()
                    .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            }
            else
            {
                ViewBag.BranchList = _dataContext.OrganizationUnits
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => new {x.Id, x.Title})
                    .ToList()
                    .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            }

            if (!branchId.HasValue || !fromYear.HasValue || !fromMonth.HasValue || !toYear.HasValue ||
                !toMonth.HasValue)
            {
              
                return View(new BTSViewModel());
            }

            var sqlCmd = $@"WITH tbl AS (
	                            SELECT ts.PersianLocalYearMonth,
		                               t.BranchId,
		                               SUM(ts.BuyTransactionAmount) AS BranchSumPrice
	                            FROM  psp.[TransactionSum] ts
	                            JOIN  psp.Terminal t ON t.TerminalNo = ts.TerminalNo
	                            WHERE ts.PersianLocalYearMonth >= {fromYear}{fromMonth:00} and ts.PersianLocalYearMonth <= {toYear}{toMonth:00}
	                            GROUP BY t.BranchId, ts.PersianLocalYearMonth
                            )

                            SELECT tbl.PersianLocalYearMonth,
	                               MAX(tbl.BranchSumPrice) AS MaxPrice,
	                               AVG(tbl.BranchSumPrice) AS AveragePrice
	                            FROM tbl
                                GROUP BY tbl.PersianLocalYearMonth;";

            var data = _dataContext.Database.Connection.Query(sqlCmd)
                .Select(x => new BranchTransactionStatusViewModel
                {
                    YearMonth = x.PersianLocalYearMonth,
                    MaxPrice = x.MaxPrice,
                    AveragePrice = x.AveragePrice
                })
                .GroupBy(x => x.YearMonth)
                .ToDictionary(z => z.Key,
                    y => new
                    {
                        MaxPrice = y.Max(z => z.MaxPrice), AveragePrice = (long) y.Average(z => z.AveragePrice)
                    });

            var dataTemplate = DateTimeExtensions
                .GetMonthBetween(fromYear.Value, fromMonth.Value, toYear.Value, toMonth.Value)
                .Select(x => new {Year = x.Item1, Month = x.Item2});

            var result = dataTemplate.OrderBy(x => x.Year).ThenBy(x => x.Month).Select(x =>
            {
                var yearMonth = Convert.ToInt32($"{x.Year}{x.Month:00}");
                var item = data.GetValueOrDefault(yearMonth, new {MaxPrice = 0L, AveragePrice = 0L});

                return new BranchTransactionStatusViewModel
                {
                    YearMonth = yearMonth,
                    MaxPrice = item.MaxPrice,
                    AveragePrice = item.AveragePrice
                };
            });

            string sqlCommand;
            if (branchId == 1 || branchId == 2 || branchId == 3 || branchId == 4 || branchId == 5 || branchId == 6)
            {
                sqlCommand =
                    $@"SELECT bm.Title, PersianLocalYearMonth,SUM(BuyTransactionAmount) AS SumPrice, SUM(BuyTransactionCount) AS TotalCount, AVG(BuyTransactionAmount) AS AveragePrice, AVG(BuyTransactionCount) AS AverageCount FROM psp.TransactionSum
                JOIN psp.Terminal ON Terminal.TerminalNo = TransactionSum.TerminalNo
                JOIN dbo.OrganizationUnit bm ON BranchId = bm.Id
                WHERE bm.ParentId = {branchId} AND PersianLocalYearMonth >= {fromYear}{fromMonth.Value:00} AND PersianLocalYearMonth <= {toYear}{toMonth.Value:00}
                GROUP BY bm.Title,PersianLocalYearMonth
                ORDER BY bm.Title,PersianLocalYearMonth";
            }
            else if (branchId == 11 || branchId == 12)
            {
                sqlCommand =
                    $@"SELECT bm.Title,PersianLocalYearMonth,SUM(BuyTransactionAmount) AS SumPrice, SUM(BuyTransactionCount) AS TotalCount, AVG(BuyTransactionAmount) AS AveragePrice, AVG(BuyTransactionCount) AS AverageCount FROM psp.TransactionSum
                JOIN psp.Terminal ON Terminal.TerminalNo = TransactionSum.TerminalNo
                JOIN dbo.OrganizationUnit b ON BranchId = b.Id
                JOIN dbo.OrganizationUnit bm ON bm.Id = b.ParentId
                WHERE bm.ParentId = {branchId} AND PersianLocalYearMonth >= {fromYear}{fromMonth.Value:00} AND PersianLocalYearMonth <= {toYear}{toMonth.Value:00}
                GROUP BY bm.Title,PersianLocalYearMonth
                ORDER BY bm.Title,PersianLocalYearMonth";
            }
            else if (branchId == 10)
            {
                sqlCommand =
                    $@"SELECT bm.Title,PersianLocalYearMonth,SUM(BuyTransactionAmount) AS SumPrice, SUM(BuyTransactionCount) AS TotalCount, AVG(BuyTransactionAmount) AS AveragePrice, AVG(BuyTransactionCount) AS AverageCount FROM psp.TransactionSum
                JOIN psp.Terminal ON Terminal.TerminalNo = TransactionSum.TerminalNo
                JOIN dbo.OrganizationUnit b ON BranchId = b.Id
                JOIN dbo.OrganizationUnit bm ON bm.Id = b.ParentId
                JOIN dbo.OrganizationUnit t ON t.Id = bm.ParentId
                WHERE t.ParentId = {branchId} AND PersianLocalYearMonth >= {fromYear}{fromMonth.Value:00} AND PersianLocalYearMonth <= {toYear}{toMonth.Value:00}
                GROUP BY bm.Title,PersianLocalYearMonth
                ORDER BY bm.Title,PersianLocalYearMonth";
            }
            else
            {
                sqlCommand =
                    $@"SELECT t.Title,PersianLocalYearMonth,SUM(BuyTransactionAmount) AS SumPrice, SUM(BuyTransactionCount) AS TotalCount, AVG(BuyTransactionAmount) AS AveragePrice, AVG(BuyTransactionCount) AS AverageCount FROM psp.TransactionSum
                JOIN psp.Terminal ON Terminal.TerminalNo = TransactionSum.TerminalNo
                JOIN dbo.OrganizationUnit t ON t.Id = Terminal.BranchId
                WHERE t.Id = {branchId} AND PersianLocalYearMonth >= {fromYear}{fromMonth.Value:00} AND PersianLocalYearMonth <= {toYear}{toMonth.Value:00}
                GROUP BY t.Title,PersianLocalYearMonth
                ORDER BY t.Title,PersianLocalYearMonth";
            }

            var branchNames = new List<string>();
            var newData = new List<BranchTransactionStatusViewModel2>();
            var newData2 = new List<BranchTransactionStatusViewModel3>();
            if (!string.IsNullOrEmpty(sqlCommand))
            {
                var d = _dataContext.Database.Connection.Query(sqlCommand)
                    .Select(x => new
                    {
                        x.Title,
                        x.SumPrice,
                        x.TotalCount,
                        x.AveragePrice,
                        x.AverageCount,
                        x.PersianLocalYearMonth
                    });

                newData2 = d
                    .OrderBy(x => x.Title)
                    .GroupBy(x => x.PersianLocalYearMonth)
                    .Select(z => new BranchTransactionStatusViewModel3
                    {
                        Name = z.Key,
                        SumOfPriceData = z.Select(n => (double?) n.SumPrice ?? 0),
                        TotalCountData = z.Select(n => (double?) n.TotalCount ?? 0),
                        AverageOfPriceData = z.Select(n => (double?) n.AveragePrice ?? 0),
                        AverageCountData = z.Select(n => (double?) n.AverageCount ?? 0)
                    })
                    .ToList();

                branchNames = d.OrderBy(x => x.Title).Select(x => $"{x.Title}").Distinct().ToList();
            }

            var vm = new BTSViewModel
            {
                BTS = result,
                BTS2 = newData,
                BTS3 = newData2,
                BranchNames = branchNames
            };
            var messsage = _dataContext.Messages.ToList();
            ViewBag.OpenMessage = messsage.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
                                                      && (d.UserId == CurrentUserId ||
                                                          d.ReviewerUserId == CurrentUserId
                                                          || User.IsMessageManagerUser()));

            return View(vm);
        }

        [CustomAuthorize]
        public ActionResult ActiveInactiveTerminalReport(long? branchId, int? fromYear, int? fromMonth, int? toYear,
            int? toMonth)
        {
            if (!fromYear.HasValue || !fromMonth.HasValue || !toYear.HasValue || !toMonth.HasValue)
            {
                var prevMonth = DateTime.Now.AddMonths(-1);

                return RedirectToAction("ActiveInactiveTerminalReport", new
                {
                    branchId,
                    fromYear = prevMonth.GetPersianYear(),
                    fromMonth = prevMonth.GetPersianMonth(),
                    toYear = DateTime.Now.GetPersianYear(),
                    toMonth = DateTime.Now.GetPersianMonth()
                });
            }

            var whereClause = string.Empty;
            if (User.IsBranchUser())
            {
                branchId = CurrentUserBranchId;
                if (branchId.HasValue && branchId != CurrentUserBranchId)
                {
                    return RedirectToAction("AccessDenied", "Error");
                }

                whereClause = $" AND b.Id = {branchId}";
            }
            else if (User.IsSupervisionUser())
            {
                branchId = CurrentUserBranchId;
                ViewBag.BranchList = _dataContext.OrganizationUnits
                    .Where(x => x.ParentId == CurrentUserBranchId)
                    .Select(x => new {x.Id, x.Title})
                    .ToList()
                    .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

                whereClause = $" AND b.ParentId = {branchId}";
            }
            else if (User.IsTehranBranchManagementUser())
            {
                whereClause += $" and b.CityId = {(long) Enums.City.Tehran} ";
            }
            else if (User.IsCountyBranchManagementUser())
            {
                whereClause += $" and b.CityId != {(long) Enums.City.Tehran} ";
            }
            else
            {
                ViewBag.BranchList = _dataContext.OrganizationUnits
                    .Select(x => new {x.Id, x.Title})
                    .ToList()
                    .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            }

            if (!branchId.HasValue)
            {
                var messafge = _dataContext.Messages.ToList();
                ViewBag.OpenMessage = messafge.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
                                                          && (d.UserId == CurrentUserId ||
                                                              d.ReviewerUserId == CurrentUserId
                                                              || User.IsMessageManagerUser()));
                return View(Enumerable.Empty<ActiveInactiveTerminalReportViewModel>());
            }

            ViewBag.BranchId = branchId;

            var sqlCmd = $@"
                SELECT
                    tbl.PersianLocalYear,
	                tbl.PersianLocalMonth,
                    dt.IsWireless,
                    SUM(IIF(tbl.BuyTransactionAmount > 20000000 or tbl.BuyTransactionCount > 60, 1, 0)) AS ActiveCount,
                    SUM(IIF(tbl.BuyTransactionAmount <= 20000000 and tbl.BuyTransactionCount <= 60, 1, 0)) AS InactiveCount
                FROM  psp.TransactionSum tbl                 
                JOIN  psp.Terminal tr ON tr.TerminalNo = tbl.TerminalNo
                JOIN  psp.DeviceType dt ON tr.DeviceTypeId = dt.Id
                JOIN  OrganizationUnit b on b.Id = tr.BranchId
               WHERE tr.BranchId IN (select * from  dbo.GetChildOrganizationUnits({branchId}))  AND tbl.PersianLocalYearMonth >= {fromYear * 100 + fromMonth} AND tbl.PersianLocalYearMonth <= {toYear * 100 + toMonth}    {whereClause}      
               GROUP BY IsWireless, tbl.PersianLocalYear, tbl.PersianLocalMonth;";

            var data = _dataContext.Database.Connection.Query(sqlCmd)
                .Select(x => new
                {
                    YearMonth = $"{x.PersianLocalYear}-{x.PersianLocalMonth.ToString().PadLeft(2, '0')}",
                    IsWireless = Convert.ToBoolean(x.IsWireless),
                    x.ActiveCount,
                    x.InactiveCount
                })
                .ToList();

            var withSimData = data.Where(x => !x.IsWireless).GroupBy(x => x.YearMonth).ToDictionary(x => x.Key, y =>
                new
                {
                    ActiveTerminalCount = y.Sum(t => t.ActiveCount),
                    InactiveTerminalCount = y.Sum(t => t.InactiveCount)
                });

            var withoutSimData = data.Where(x => x.IsWireless).GroupBy(x => x.YearMonth).ToDictionary(x => x.Key,
                y =>
                    new
                    {
                        ActiveTerminalCount = y.Sum(t => t.ActiveCount),
                        InactiveTerminalCount = y.Sum(t => t.InactiveCount)
                    });

            var dataTemplate = new List<Tuple<int, int>>();
            for (var year = fromYear.Value; year <= toYear; year++)
            {
                var start = year == fromYear.Value ? fromMonth.Value : 1;
                var end = year == toYear ? toMonth : 12;
                for (var month = start; month <= end; month++)
                {
                    dataTemplate.Add(Tuple.Create(year, month));
                }
            }

            var finalResult = dataTemplate.Select(x =>
            {
                var yearMonth = $"{x.Item1}-{x.Item2.ToString().PadLeft(2, '0')}";
                var withoutSimItem = withoutSimData.GetValueOrDefault(yearMonth,
                    new {ActiveTerminalCount = 0, InactiveTerminalCount = 0});
                var withSimItem = withSimData.GetValueOrDefault(yearMonth,
                    new {ActiveTerminalCount = 0, InactiveTerminalCount = 0});

                return new ActiveInactiveTerminalReportViewModel
                {
                    YearMonth = yearMonth,
                    WithoutSimActiveTerminalCount = withoutSimItem.ActiveTerminalCount,
                    WithoutSimInactiveTerminalCount = withoutSimItem.InactiveTerminalCount,
                    WithSimActiveTerminalCount = withSimItem.ActiveTerminalCount,
                    WithSimInactiveTerminalCount = withSimItem.InactiveTerminalCount
                };
            });
           
            return View(finalResult);
        }

        [CustomAuthorize]
        public ActionResult DownloadActiveInactiveTerminalReport(long? branchId, int? fromYear, int? fromMonth,
            int? toYear, int? toMonth)
        {
            if (!fromYear.HasValue || !fromMonth.HasValue || !toYear.HasValue || !toMonth.HasValue)
            {
                return new EmptyResult();
            }

            if (User.IsBranchUser())
            {
                if (branchId.HasValue)
                {
                    return new EmptyResult();
                }

                branchId = CurrentUserBranchId;
            }

            if (!branchId.HasValue)
            {
                return new EmptyResult();
            }

            var sqlCmd = $@"                
                SELECT 
                    mr.Title AS MarketerTitle,
					mp.FirstName,
					mp.LastName,
					mp.Birthdate,
					mp.NationalCode,
					mp.Mobile,
					mp.FatherName,
					mp.IdentityNumber,
					mp.CompanyRegistrationDate,
					mp.CompanyRegistrationNumber,
					mp.LegalNationalCode,
                    mp.IsMale,
					mp.IsLegalPersonality,
                    tr.Id,
                    tr.TerminalNo,
					tr.SubmitTime,
					tr.InstallationDate,
					tr.RevokeDate,
					tr.MerchantNo,
					dt.Title AS DeviceTypeTitle,
					tr.Title,
					tr.EnglishTitle,
					tr.BranchId,
					br.Title AS BranchTitle,
					tr.AccountNo,
					tr.ShebaNo,
					ts.Title AS StatusTitle,
					tr.BatchDate,
					c.Title AS CityTitle,
					s.Title AS StateTitle,
					tr.TelCode,
					tr.Tel,
					tr.Address,
					tr.PostCode,
					tr.ContractNo,
					tr.ContractDate,
					g.Title AS GuildTitle,
					gp.Title AS ParentGuildTitle,
					psp.Title AS PspTitle,
                    tbl.PersianLocalYear,
	                tbl.PersianLocalMonth,
                    dt.IsWireless,
                    IIF(tbl.BuyTransactionAmount > 20000000 or tbl.BuyTransactionCount > 60, 1, 0) AS IsActive
                FROM psp.TransactionSum tbl                 
                JOIN psp.Terminal tr ON tr.TerminalNo = tbl.TerminalNo          
                JOIN psp.Marketer mr ON mr.Id = tr.MarketerId
				JOIN psp.DeviceType dt ON dt.Id = tr.DeviceTypeId
				JOIN dbo.OrganizationUnit br ON br.Id = tr.BranchId
				JOIN psp.TerminalStatus ts ON ts.Id = tr.StatusId
				JOIN psp.Psp psp ON psp.Id = tr.PspId
				JOIN dbo.City c ON c.Id = tr.CityId
				JOIN dbo.State s ON s.Id = c.StateId
				JOIN psp.Guild g ON g.Id = tr.GuildId
				JOIN psp.MerchantProfile mp ON mp.Id = tr.MerchantProfileId
				LEFT JOIN psp.Guild gp ON gp.Id = g.ParentId
               WHERE tr.BranchId IN (select * from  dbo.GetChildOrganizationUnits({branchId})) AND tbl.PersianLocalYearMonth >= {fromYear * 100 + fromMonth} AND tbl.PersianLocalYearMonth <= {toYear * 100 + toMonth}";

            var data = _dataContext.Database.Connection.Query(sqlCmd)
                .Select(x => new
                {
                    x.Id,
                    x.MarketerTitle,
                    YearMonth = $"{x.PersianLocalYear}{x.PersianLocalMonth.ToString().PadLeft(2, '0')}",
                    x.TerminalNo,
                    x.IsWireless,
                    x.InstallationDate,
                    x.SubmitTime,
                    x.RevokeDate,
                    x.MerchantNo,
                    x.DeviceTypeTitle,
                    x.Title,
                    x.EnglishTitle,
                    x.BranchId,
                    x.BranchTitle,
                    x.AccountNo,
                    x.ShebaNo,
                    x.StatusTitle,
                    x.BatchDate,
                    x.CityTitle,
                    x.StateTitle,
                    x.TelCode,
                    x.Tel,
                    x.Address,
                    x.PostCode,
                    x.ContractNo,
                    x.ContractDate,
                    x.GuildTitle,
                    x.PspTitle,
                    x.NationalCode,
                    x.FirstName,
                    x.LastName,
                    x.FatherName,
                    x.Birthdate,
                    x.Mobile,
                    x.IdentityNumber,
                    x.LegalNationalCode,
                    x.CompanyRegistrationDate,
                    x.ParentGuildTitle,
                    x.IsActive,
                    x.LegalPersonalityTitle,
                    x.CompanyRegistrationNumber,
                    x.GenderTitle
                });

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
                worksheet.Column(7).Width = 16;
                worksheet.Column(8).Width = 44;
                worksheet.Column(9).Width = 18;
                worksheet.Column(10).Width = 26;
                worksheet.Column(11).Width = 16;
                worksheet.Column(12).Width = 16;
                worksheet.Column(13).Width = 28;
                worksheet.Column(14).Width = 14;
                worksheet.Column(15).Width = 13;
                worksheet.Column(16).Width = 27;
                worksheet.Column(17).Width = 26;
                worksheet.Column(18).Width = 32;
                worksheet.Column(19).Width = 22;
                worksheet.Column(20).Width = 26;
                worksheet.Column(21).Width = 14;
                worksheet.Column(22).Width = 14;
                worksheet.Column(23).Width = 21;
                worksheet.Column(24).Width = 20;
                worksheet.Column(25).Width = 26;
                worksheet.Column(26).Width = 26;
                worksheet.Column(27).Width = 26;
                worksheet.Column(28).Width = 17;
                worksheet.Column(29).Width = 75;
                worksheet.Column(30).Width = 13;
                worksheet.Column(31).Width = 16;
                worksheet.Column(32).Width = 16;
                worksheet.Column(33).Width = 16;
                worksheet.Column(34).Width = 65;
                worksheet.Column(35).Width = 45;
                worksheet.Column(36).Width = 10;
                worksheet.Column(37).Width = 14;
                worksheet.Column(38).Width = 17;
                worksheet.Column(39).Width = 16;
                worksheet.Column(40).Width = 26;

                worksheet.Cells[1, 1].Value = "بازاریابی";
                worksheet.Cells[1, 2].Value = "سال / ماه";
                worksheet.Cells[1, 3].Value = "شماره پایانه";
                worksheet.Cells[1, 4].Value = "نوع دستگاه";
                worksheet.Cells[1, 5].Value = "عملکرد پایانه";

                worksheet.Cells[1, 6].Value = "شماره پیگیری";
                worksheet.Cells[1, 7].Value = "کدملی";
                worksheet.Cells[1, 8].Value = "نام فروشگاه";
                worksheet.Cells[1, 9].Value = "نام";
                worksheet.Cells[1, 10].Value = "نام خانوادگی";
                worksheet.Cells[1, 11].Value = "شماره پذیرنده";
                worksheet.Cells[1, 12].Value = "وضعیت";
                worksheet.Cells[1, 13].Value = "شرکت psp";
                worksheet.Cells[1, 14].Value = "کدشعبه";
                worksheet.Cells[1, 15].Value = "نام شعبه";
                worksheet.Cells[1, 16].Value = "شماره حساب";
                worksheet.Cells[1, 17].Value = "شماره شبا";
                worksheet.Cells[1, 18].Value = "تاریخ درخواست";
                worksheet.Cells[1, 19].Value = "تاریخ کدباز یا بچ";
                worksheet.Cells[1, 20].Value = "تاریخ نصب";
                worksheet.Cells[1, 21].Value = "تاریخ جمع آوری";
                worksheet.Cells[1, 22].Value = "نوع دستگاه درخواستی";
                worksheet.Cells[1, 23].Value = "شهر";
                worksheet.Cells[1, 24].Value = "استان";
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
                worksheet.Cells[1, 36].Value = "شناسه ملی شرکت";
                worksheet.Cells[1, 37].Value = "تاریخ ثبت شرکت";
                worksheet.Cells[1, 38].Value = "شماره ثبت شرکت";
                worksheet.Cells[1, 39].Value = "جنسیت";
                worksheet.Cells[1, 40].Value = "شماره قرارداد";

                var rowNumber = 2;
                foreach (var item in data.OrderBy(x => x.MarketerTitle).ThenBy(x => x.YearMonth))
                {
                    DateTime submitTime = item.SubmitTime;
                    DateTime? installationDate = item.InstallationDate;
                    DateTime? revokeDate = item.RevokeDate;
                    DateTime birthDate = item.Birthdate;
                    DateTime? batchDate = item.BatchDate;
                    DateTime? companyRegistrationDate = item.CompanyRegistrationDate;

                    worksheet.Cells[rowNumber, 1].Value = item.MarketerTitle;
                    worksheet.Cells[rowNumber, 2].Value = item.YearMonth;
                    worksheet.Cells[rowNumber, 3].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 4].Value = item.IsWireless == 1 ? "سیار" : "ثابت";
                    worksheet.Cells[rowNumber, 5].Value = item.IsActive == 1 ? "فعال" : "غیر فعال";

                    worksheet.Cells[rowNumber, 6].Value = item.Id;
                    worksheet.Cells[rowNumber, 7].Value = item.NationalCode;
                    worksheet.Cells[rowNumber, 8].Value = item.Title;
                    worksheet.Cells[rowNumber, 9].Value = item.FirstName;
                    worksheet.Cells[rowNumber, 10].Value = item.LastName;
                    worksheet.Cells[rowNumber, 11].Value = item.MerchantNo;
                    worksheet.Cells[rowNumber, 12].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 13].Value = item.PspTitle;
                    worksheet.Cells[rowNumber, 14].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 15].Value = item.BranchTitle;
                    worksheet.Cells[rowNumber, 16].Value = item.AccountNo;
                    worksheet.Cells[rowNumber, 17].Value = item.ShebaNo;
                    worksheet.Cells[rowNumber, 18].Value = submitTime.ToPersianDate();
                    worksheet.Cells[rowNumber, 19].Value = batchDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 20].Value = installationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 21].Value = revokeDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 22].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 23].Value = item.CityTitle;
                    worksheet.Cells[rowNumber, 24].Value = item.StateTitle;
                    worksheet.Cells[rowNumber, 25].Value = item.Address;
                    worksheet.Cells[rowNumber, 26].Value = item.TelCode;
                    worksheet.Cells[rowNumber, 27].Value = item.Tel;
                    worksheet.Cells[rowNumber, 28].Value = item.Mobile;
                    worksheet.Cells[rowNumber, 29].Value = item.PostCode;
                    worksheet.Cells[rowNumber, 30].Value = item.GuildTitle;
                    worksheet.Cells[rowNumber, 31].Value = item.ParentGuildTitle;
                    worksheet.Cells[rowNumber, 32].Value = item.LegalPersonalityTitle;
                    worksheet.Cells[rowNumber, 33].Value = birthDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 34].Value = item.IdentityNumber;
                    worksheet.Cells[rowNumber, 35].Value = item.FatherName;
                    worksheet.Cells[rowNumber, 36].Value = item.LegalNationalCode;
                    worksheet.Cells[rowNumber, 37].Value = companyRegistrationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 38].Value = item.CompanyRegistrationNumber;
                    worksheet.Cells[rowNumber, 39].Value = item.GenderTitle;
                    worksheet.Cells[rowNumber, 40].Value = item.ContractNo;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize]
        public ActionResult InstallationDelay()
        {
             
            return View();
        }

        [CustomAuthorize]
        public async Task<ActionResult> GetInstallationDelayData(DateTime fromDate, DateTime toDate, int? delay,
            byte? pspId, bool? justInstalledTerminals)
        {
            var data = await _dataContext.GetInstallationDelayData(
                fromDate,
                toDate,
                delay,
                pspId,
                CurrentUserBranchId,
                justInstalledTerminals,
                User.IsSupervisionUser(),
                User.IsBranchUser(),
                User.IsTehranBranchManagementUser(),
                User.IsCountyBranchManagementUser());

            return JsonSuccessResult(new {rows = data, totalRowsCount = data.Count});
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> DownloadBankReport1(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var startDate = date.AbsoluteStart().ToPersianDate();
            var endDate = date.AddDays(daysInMonth - 1).AbsoluteEnd().ToPersianDate();

            var sqlCmd = $@"WITH buyTransaction AS (
              SELECT ts.TerminalNo,SUM(ts.BuyTransactionAmount) AS BuyTransactionAmount, SUM(ts.BuyTransactionCount) AS BuyTransactionCount FROM psp.TransactionSum ts WHERE ts.PersianLocalYear = {year} AND ts.PersianLocalMonth = {month}
              GROUP BY ts.TerminalNo
              ),
             tbl AS (SELECT t.TerminalNo,dbo.ToPersianDateTime(t.InstallationDate,NULL) AS InstallationDate,dbo.ToPersianDateTime(t.RevokeDate,NULL) AS RevokeDate
              ,t.StatusId,ts.Title AS StatusTitle,dt.Title AS DeviceTypeTitle,dt.IsWireless,
              t.MarketerId,bt.BuyTransactionAmount,bt.BuyTransactionCount FROM psp.Terminal t
              JOIN psp.TerminalStatus ts ON t.StatusId = ts.Id
              JOIN psp.DeviceType dt ON t.DeviceTypeId = dt.Id
              LEFT JOIN buyTransaction bt ON t.TerminalNo = bt.TerminalNo
            WHERE dbo.ToPersianDateTime(t.InstallationDate,NULL) <= '{endDate}' AND 
              (t.RevokeDate IS NULL OR (dbo.ToPersianDateTime(t.RevokeDate,null) >= '{startDate}' AND  dbo.ToPersianDateTime(t.RevokeDate,null) <= '{endDate}')))


              SELECT * FROM tbl WHERE MarketerId = 3 AND IsWireless = 1";

            var data = (await _dataContext.Database.Connection.QueryAsync(sqlCmd))
                .Select(x => new
                {
                    x.TerminalNo,
                    x.InstallationDate,
                    x.RevokeDate,
                    x.StatusTitle,
                    x.DeviceTypeTitle,
                    x.BuyTransactionAmount,
                    x.BuyTransactionCount
                });

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
                worksheet.Column(7).Width = 16;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "تاریخ نصب";
                worksheet.Cells[1, 3].Value = "تاریخ ابطال";
                worksheet.Cells[1, 4].Value = "وضعیت";
                worksheet.Cells[1, 5].Value = "نوع دستگاه";
                worksheet.Cells[1, 6].Value = "مبلغ تراکنش";
                worksheet.Cells[1, 7].Value = "تعداد تراکنش";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.InstallationDate;
                    worksheet.Cells[rowNumber, 3].Value = item.RevokeDate;
                    worksheet.Cells[rowNumber, 4].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 7].Value = item.BuyTransactionCount;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> DownloadBankReport2(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var startDate = date.AbsoluteStart().ToPersianDate();
            var endDate = date.AddDays(daysInMonth - 1).AbsoluteEnd().ToPersianDate();

            var sqlCmd = $@"WITH buyTransaction AS (
              SELECT ts.TerminalNo,SUM(ts.BuyTransactionAmount) AS BuyTransactionAmount, SUM(ts.BuyTransactionCount) AS BuyTransactionCount FROM psp.TransactionSum ts WHERE ts.PersianLocalYear = {year} AND ts.PersianLocalMonth = {month}
              GROUP BY ts.TerminalNo
              ),
             tbl AS (SELECT t.TerminalNo,dbo.ToPersianDateTime(t.InstallationDate,NULL) AS InstallationDate,dbo.ToPersianDateTime(t.RevokeDate,NULL) AS RevokeDate
              ,t.StatusId,ts.Title AS StatusTitle,dt.Title AS DeviceTypeTitle,dt.IsWireless,
              t.MarketerId,bt.BuyTransactionAmount,bt.BuyTransactionCount FROM psp.Terminal t
              JOIN psp.TerminalStatus ts ON t.StatusId = ts.Id
              JOIN psp.DeviceType dt ON t.DeviceTypeId = dt.Id
              LEFT JOIN buyTransaction bt ON t.TerminalNo = bt.TerminalNo
            WHERE dbo.ToPersianDateTime(t.InstallationDate,NULL) <= '{endDate}' AND 
              (t.RevokeDate IS NULL OR (dbo.ToPersianDateTime(t.RevokeDate,null) >= '{startDate}' AND  dbo.ToPersianDateTime(t.RevokeDate,null) <= '{endDate}')))


              SELECT * FROM tbl WHERE MarketerId = 3 AND IsWireless = 0";

            var data = (await _dataContext.Database.Connection.QueryAsync(sqlCmd))
                .Select(x => new
                {
                    x.TerminalNo,
                    x.InstallationDate,
                    x.RevokeDate,
                    x.StatusTitle,
                    x.DeviceTypeTitle,
                    x.BuyTransactionAmount,
                    x.BuyTransactionCount
                });

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
                worksheet.Column(7).Width = 16;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "تاریخ نصب";
                worksheet.Cells[1, 3].Value = "تاریخ ابطال";
                worksheet.Cells[1, 4].Value = "وضعیت";
                worksheet.Cells[1, 5].Value = "نوع دستگاه";
                worksheet.Cells[1, 6].Value = "مبلغ تراکنش";
                worksheet.Cells[1, 7].Value = "تعداد تراکنش";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.InstallationDate;
                    worksheet.Cells[rowNumber, 3].Value = item.RevokeDate;
                    worksheet.Cells[rowNumber, 4].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 7].Value = item.BuyTransactionCount;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> DownloadBankReport3(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var startDate = date.AbsoluteStart().ToPersianDate();
            var endDate = date.AddDays(daysInMonth - 1).AbsoluteEnd().ToPersianDate();

            var sqlCmd = $@"WITH buyTransaction AS (
              SELECT ts.TerminalNo,SUM(ts.BuyTransactionAmount) AS BuyTransactionAmount, SUM(ts.BuyTransactionCount) AS BuyTransactionCount FROM psp.TransactionSum ts WHERE ts.PersianLocalYear = {year} AND ts.PersianLocalMonth = {month}
              GROUP BY ts.TerminalNo
              ),
             tbl AS (SELECT t.TerminalNo,dbo.ToPersianDateTime(t.InstallationDate,NULL) AS InstallationDate,dbo.ToPersianDateTime(t.RevokeDate,NULL) AS RevokeDate
              ,t.StatusId,ts.Title AS StatusTitle,dt.Title AS DeviceTypeTitle,dt.IsWireless,
              t.MarketerId,bt.BuyTransactionAmount,bt.BuyTransactionCount FROM psp.Terminal t
              JOIN psp.TerminalStatus ts ON t.StatusId = ts.Id
              JOIN psp.DeviceType dt ON t.DeviceTypeId = dt.Id
              LEFT JOIN buyTransaction bt ON t.TerminalNo = bt.TerminalNo
            WHERE dbo.ToPersianDateTime(t.InstallationDate,NULL) <= '{endDate}' AND 
              (t.RevokeDate IS NULL OR (dbo.ToPersianDateTime(t.RevokeDate,null) >= '{startDate}' AND  dbo.ToPersianDateTime(t.RevokeDate,null) <= '{endDate}')))


              SELECT * FROM tbl 
              WHERE MarketerId != 3 AND IsWireless = 0 AND (tbl.BuyTransactionAmount >= 20000000 OR tbl.BuyTransactionCount >= 60)";

            var data = (await _dataContext.Database.Connection.QueryAsync(sqlCmd))
                .Select(x => new
                {
                    x.TerminalNo,
                    x.InstallationDate,
                    x.RevokeDate,
                    x.StatusTitle,
                    x.DeviceTypeTitle,
                    x.BuyTransactionAmount,
                    x.BuyTransactionCount
                });

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
                worksheet.Column(7).Width = 16;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "تاریخ نصب";
                worksheet.Cells[1, 3].Value = "تاریخ ابطال";
                worksheet.Cells[1, 4].Value = "وضعیت";
                worksheet.Cells[1, 5].Value = "نوع دستگاه";
                worksheet.Cells[1, 6].Value = "مبلغ تراکنش";
                worksheet.Cells[1, 7].Value = "تعداد تراکنش";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.InstallationDate;
                    worksheet.Cells[rowNumber, 3].Value = item.RevokeDate;
                    worksheet.Cells[rowNumber, 4].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 7].Value = item.BuyTransactionCount;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> DownloadBankReport4(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var startDate = date.AbsoluteStart().ToPersianDate();
            var endDate = date.AddDays(daysInMonth - 1).AbsoluteEnd().ToPersianDate();

            var sqlCmd = $@"WITH buyTransaction AS (
              SELECT ts.TerminalNo,SUM(ts.BuyTransactionAmount) AS BuyTransactionAmount, SUM(ts.BuyTransactionCount) AS BuyTransactionCount FROM psp.TransactionSum ts WHERE ts.PersianLocalYear = {year} AND ts.PersianLocalMonth = {month}
              GROUP BY ts.TerminalNo
              ),
             tbl AS (SELECT t.TerminalNo,dbo.ToPersianDateTime(t.InstallationDate,NULL) AS InstallationDate,dbo.ToPersianDateTime(t.RevokeDate,NULL) AS RevokeDate
              ,t.StatusId,ts.Title AS StatusTitle,dt.Title AS DeviceTypeTitle,dt.IsWireless,
              t.MarketerId,bt.BuyTransactionAmount,bt.BuyTransactionCount FROM psp.Terminal t
              JOIN psp.TerminalStatus ts ON t.StatusId = ts.Id
              JOIN psp.DeviceType dt ON t.DeviceTypeId = dt.Id
              LEFT JOIN buyTransaction bt ON t.TerminalNo = bt.TerminalNo
            WHERE dbo.ToPersianDateTime(t.InstallationDate,NULL) <= '{endDate}' AND 
              (t.RevokeDate IS NULL OR (dbo.ToPersianDateTime(t.RevokeDate,null) >= '{startDate}' AND  dbo.ToPersianDateTime(t.RevokeDate,null) <= '{endDate}')))


              SELECT * FROM tbl 
              WHERE MarketerId != 3 AND IsWireless = 1 AND (tbl.BuyTransactionAmount >= 20000000 OR tbl.BuyTransactionCount >= 60)";

            var data = (await _dataContext.Database.Connection.QueryAsync(sqlCmd))
                .Select(x => new
                {
                    x.TerminalNo,
                    x.InstallationDate,
                    x.RevokeDate,
                    x.StatusTitle,
                    x.DeviceTypeTitle,
                    x.BuyTransactionAmount,
                    x.BuyTransactionCount
                });

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
                worksheet.Column(7).Width = 16;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "تاریخ نصب";
                worksheet.Cells[1, 3].Value = "تاریخ ابطال";
                worksheet.Cells[1, 4].Value = "وضعیت";
                worksheet.Cells[1, 5].Value = "نوع دستگاه";
                worksheet.Cells[1, 6].Value = "مبلغ تراکنش";
                worksheet.Cells[1, 7].Value = "تعداد تراکنش";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.InstallationDate;
                    worksheet.Cells[rowNumber, 3].Value = item.RevokeDate;
                    worksheet.Cells[rowNumber, 4].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 7].Value = item.BuyTransactionCount;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> DownloadBankReport5(int? year, int? month)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var startDate = date.AbsoluteStart().ToPersianDate();
            var endDate = date.AddDays(daysInMonth - 1).AbsoluteEnd().ToPersianDate();

            var sqlCmd = $@"WITH buyTransaction AS (
              SELECT ts.TerminalNo,SUM(ts.BuyTransactionAmount) AS BuyTransactionAmount, SUM(ts.BuyTransactionCount) AS BuyTransactionCount FROM psp.TransactionSum ts WHERE ts.PersianLocalYear = {year} AND ts.PersianLocalMonth = {month}
              GROUP BY ts.TerminalNo
              ),
             tbl AS (SELECT t.TerminalNo,dbo.ToPersianDateTime(t.InstallationDate,NULL) AS InstallationDate,dbo.ToPersianDateTime(t.RevokeDate,NULL) AS RevokeDate
              ,t.StatusId,ts.Title AS StatusTitle,dt.Title AS DeviceTypeTitle,dt.IsWireless,
              t.MarketerId,bt.BuyTransactionAmount,bt.BuyTransactionCount FROM psp.Terminal t
              JOIN psp.TerminalStatus ts ON t.StatusId = ts.Id
              JOIN psp.DeviceType dt ON t.DeviceTypeId = dt.Id
              LEFT JOIN buyTransaction bt ON t.TerminalNo = bt.TerminalNo
            WHERE dbo.ToPersianDateTime(t.InstallationDate,NULL) <= '{endDate}' AND 
              (t.RevokeDate IS NULL OR (dbo.ToPersianDateTime(t.RevokeDate,null) >= '{startDate}' AND  dbo.ToPersianDateTime(t.RevokeDate,null) <= '{endDate}')))


              SELECT tbl.*,TransactionAmountForReward,i.CoefficientReward,i.MaxRewardPricePerDevice,dbo.InlineMax((BuyTransactionAmount -  TransactionAmountForReward) * i.CoefficientReward,i.MaxRewardPricePerDevice) AS RewardAmount FROM tbl 
                JOIN Invoice i ON '{startDate}' >= dbo.ToPersianDateTime(i.FromDate,NULL) and   ('{endDate}' <= dbo.ToPersianDateTime(i.ToDate,NULL) OR i.ToDate IS NULL)
              WHERE MarketerId != 3 AND IsWireless = 1 AND (tbl.BuyTransactionAmount >= 20000000 OR tbl.BuyTransactionCount >= 60)
                AND BuyTransactionAmount >= i.TransactionAmountForReward";

            var data = (await _dataContext.Database.Connection.QueryAsync(sqlCmd))
                .Select(x => new
                {
                    x.TerminalNo,
                    x.InstallationDate,
                    x.RevokeDate,
                    x.StatusTitle,
                    x.DeviceTypeTitle,
                    x.BuyTransactionAmount,
                    x.BuyTransactionCount,
                    x.TransactionAmountForReward,
                    x.CoefficientReward,
                    x.MaxRewardPricePerDevice,
                    x.RewardAmount
                });

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
                worksheet.Column(7).Width = 26;
                worksheet.Column(8).Width = 26;
                worksheet.Column(9).Width = 26;
                worksheet.Column(10).Width = 26;
                worksheet.Column(11).Width = 26;

                worksheet.Cells[1, 1].Value = "شماره ترمینال";
                worksheet.Cells[1, 2].Value = "تاریخ نصب";
                worksheet.Cells[1, 3].Value = "تاریخ ابطال";
                worksheet.Cells[1, 4].Value = "وضعیت";
                worksheet.Cells[1, 5].Value = "نوع دستگاه";
                worksheet.Cells[1, 6].Value = "مبلغ تراکنش";
                worksheet.Cells[1, 7].Value = "تعداد تراکنش";
                worksheet.Cells[1, 8].Value = "حداقل مبلغ مشمول پاداش";
                worksheet.Cells[1, 9].Value = "ضریب پاداش";
                worksheet.Cells[1, 10].Value = "حداکثر مبلغ";
                worksheet.Cells[1, 11].Value = "مبلغ پاداش";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.TerminalNo;
                    worksheet.Cells[rowNumber, 2].Value = item.InstallationDate;
                    worksheet.Cells[rowNumber, 3].Value = item.RevokeDate;
                    worksheet.Cells[rowNumber, 4].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 5].Value = item.DeviceTypeTitle;
                    worksheet.Cells[rowNumber, 6].Value = item.BuyTransactionAmount;
                    worksheet.Cells[rowNumber, 7].Value = item.BuyTransactionCount;
                    worksheet.Cells[rowNumber, 8].Value = item.TransactionAmountForReward;
                    worksheet.Cells[rowNumber, 9].Value = item.CoefficientReward;
                    worksheet.Cells[rowNumber, 10].Value = item.MaxRewardPricePerDevice;
                    worksheet.Cells[rowNumber, 11].Value = item.RewardAmount;
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ActiveInactiveTerminal.xlsx");
                }
            }
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> GetTerminalWageReportData(int? year)
        {
            var invoice = _dataContext.TotalWageReport.Where(x =>
                x.Year == year).ToList();

            var mlist = Enumerable.Range(1, 12);

            if (invoice == null)
            {
                return JsonWarningMessage("اطلاعاتی یافت نشد");
            }

            var result = new List<object>();
            foreach (var m in mlist)
            {
                var t =
                    invoice.FirstOrDefault(b => b.Month == m);

                var res = new
                {
                    terminalCount = t?.TerminalCount,
                    Value = t != null ? Math.Floor(t.Value) : 0,
                    otherTerminalCount = t?.OtherTerminalCount,
                    otherValue = t != null ? Math.Floor(t.OtherValue) : 0,
                    Month = m.ToString().GetMonthName(),
                    MonthId = m
                };
                result.Add(res);
            }

            return JsonSuccessResult(result);
        }


        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> GetBankInvoiceReportData(int? year, int? month, int invoiceTypeId)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var monthIsEven = month % 3 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == invoiceTypeId);

            if (invoice == null)
            {
                return JsonErrorMessage("تنظیمات صورت وضعیت ثبت نشده است");
            }

            if (!await _dataContext.TempReport1And2Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'اجاره دستگاه های ثابت و سیار منصوبه' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport3Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'اجاره دستگاه های پایانه شعبه ای منصوبه در شعب' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport4Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'جریمه تاخیر در اخذ شماره ترمینال (روز کاری)' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport5Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'جریمه تاخیر در نصب (روز کاری)' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport6Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'جریمه تاخیر در EM (روز کاری)' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport7Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات 'جریمه دستگاه های PM نشده (هر دوره تاخیر)' برای ماه و سال وارد شده بارگذاری نشده است");
            }

            if (!await _dataContext.TempReport8Datas.AnyAsync(x => x.Month == month && x.Year == year))
            {
                return JsonWarningMessage(
                    "اطلاعات جریمه تاخیر در جمع آوری برای ماه و سال وارد شده بارگذاری نشده است");
            }
            
            // اجاره دستگاه های ثابت و سیار منصوبه
            var report1Data = await _dataContext.TempReport1And2Datas.Where(x => x.Month == month && x.Year == year)
                .GroupBy(x => x.IsWireless)
                .Select(x => new {IsWireless = x.Key, Count = x.LongCount()})
                .ToListAsync();

            // اجاره دستگاه های پایانه شعبه ای منصوبه در شعب
            var report3Data = await _dataContext.TempReport3Datas.Where(x => x.Month == month && x.Year == year)
                .LongCountAsync();

            // جریمه تاخیر در اخذ شماره ترمینال - روز کاری
            var report4Data = await _dataContext.TempReport4Datas.Where(x => x.Month == month && x.Year == year)
                .SumAsync(x => x.InstallationDelay);

            // جریمه تاخیر در نصب - روز کاری
            var report5Data = await _dataContext.TempReport5Datas.Where(x => x.Month == month && x.Year == year)
                .SumAsync(x => x.InstallationDelay);

            // جریمه تاخیر در EM - روز کاری
            var report6Data = await _dataContext.TempReport6Datas.Where(x => x.Month == month && x.Year == year)
                .SumAsync(x => x.InstallationDelay);

            var report8Data =   _dataContext.TempReport8Datas.Where(x => x.Month == month && x.Year == year)
                .ToList();
            
            // جریمه دستگاه های PM نشده هر دوره تاخیر
            long calculatedReport7Data = 0;

            if (monthIsEven)
            {
                var report7Data = await _dataContext.TempReport7Datas.Where(x => x.Month == month && x.Year == year)
                    .LongCountAsync();
                var isNotPmReport7Data = await _dataContext.TempReport7Datas
                    .Where(x => x.Month == month && x.Year == year && !x.IsPm).LongCountAsync();
                if (Math.Round((decimal) isNotPmReport7Data / report7Data * 100) > 15)
                {
                    // var percent =report1Data *   12;
                    calculatedReport7Data = (long) ((long) isNotPmReport7Data - (report7Data * 0.15));
                }
                else
                {
                    calculatedReport7Data = 0;
                }

                var result = new List<object>
                {
                    new
                    {
                        Title = "اجاره دستگاه های ثابت منصوبه", UnitPrice = invoice.WithWireMarketerPrice,
                        Count = report1Data.Where(x => !x.IsWireless).Sum(x => x.Count),
                        ReportId = 1,
                        TotalPrice = (report1Data.Where(x => !x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WithWireMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های سیار منصوبه", UnitPrice = invoice.WirlessBankMarketerPrice,
                        Count = report1Data.Where(x => x.IsWireless).Sum(x => x.Count), ReportId =2,
                        TotalPrice = (report1Data.Where(x => x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های پایانه شعبه ای منصوبه در شعب",
                        UnitPrice = invoice.WirlessBankMarketerPrice, Count = report3Data, ReportId = 3,
                        TotalPrice = report3Data * invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "جریمه تاخیر در اخذ شماره ترمینال (روز کاری)",
                        UnitPrice = invoice.NotGetTerminalNoFinePricePerWorkDay, Count = report4Data, ReportId = 4,
                        TotalPrice = (long?) report4Data * invoice.NotGetTerminalNoFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در نصب (روز کاری)",
                        UnitPrice = invoice.NotInstalledFinePricePerWorkDay, ReportId = 5,
                        Count = report5Data,
                        TotalPrice = (long?) report5Data * invoice.NotInstalledFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در EM (روز کاری)", UnitPrice = invoice.NotEMFinePricePerWorkDay, ReportId = 6,
                        Count = report6Data, TotalPrice = (long?) report6Data * invoice.NotEMFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه دستگاه های PM نشده (هر دوره تاخیر)",
                        UnitPrice = invoice.NotPMFinePricePerDevice, ReportId = 7,
                        Count = calculatedReport7Data,
                        TotalPrice = calculatedReport7Data * invoice.NotPMFinePricePerDevice
                    },
                    new
                    {
                        Title = "جریمه تاخیر در  جمع آوری (روز کاری) - دستگاه سیار",UnitPrice = invoice.WirelessNotRevokePrice,
                        Count = report8Data.Where(x =>  x.IsWireless).Count(), ReportId =8,
                        TotalPrice =  report8Data.Where(x =>  x.IsWireless).Count() *
                                     invoice.WirelessNotRevokePrice
                    }, 
                    new
                    {
                        Title = "جریمه تاخیر در  جمع آوری (روز کاری) - دستگاه ثابت",UnitPrice = invoice.NotWirelessNotRevokePrice,
                           Count = report8Data.Where(x => !x.IsWireless).Count(), ReportId = 9,
                          TotalPrice =  report8Data.Where(x =>  !x.IsWireless).Count() *
                                        invoice.NotWirelessNotRevokePrice
                    }
                };
                return JsonSuccessResult(result);
            }
            else
            {
                var result = new List<object>
                {
                    new
                    {
                        Title = "اجاره دستگاه های ثابت منصوبه", UnitPrice = invoice.WithWireMarketerPrice,
                        Count = report1Data.Where(x => !x.IsWireless).Sum(x => x.Count), ReportId =1,
                        TotalPrice = (report1Data.Where(x => !x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WithWireMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های سیار منصوبه", UnitPrice = invoice.WirlessBankMarketerPrice,
                        Count = report1Data.Where(x => x.IsWireless).Sum(x => x.Count), ReportId =2,
                        TotalPrice = (report1Data.Where(x => x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های پایانه شعبه ای منصوبه در شعب",
                        UnitPrice = invoice.WirlessBankMarketerPrice, Count = report3Data, ReportId =3,
                        TotalPrice = report3Data * invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "جریمه تاخیر در اخذ شماره ترمینال (روز کاری)",
                        UnitPrice = invoice.NotGetTerminalNoFinePricePerWorkDay, Count = report4Data, ReportId =4,
                        TotalPrice = (long?) report4Data * invoice.NotGetTerminalNoFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در نصب (روز کاری)",
                        UnitPrice = invoice.NotInstalledFinePricePerWorkDay, ReportId =5,
                        Count = report5Data,
                        TotalPrice = (long?) report5Data * invoice.NotInstalledFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در EM (روز کاری)", UnitPrice = invoice.NotEMFinePricePerWorkDay, ReportId =6,
                        Count = (long) report6Data, TotalPrice = (long) ((long?) report6Data * invoice.NotEMFinePricePerWorkDay)
                    }  , new
                    {
                    Title = "جریمه تاخیر در  جمع آوری (روز کاری) - دستگاه سیار",UnitPrice = invoice.WirelessNotRevokePrice,
                    ReportId =8,
                    Count = (long) report8Data.Where(x =>  x.IsWireless).Count(),
                    TotalPrice =  (long) (report8Data.Where(x =>  x.IsWireless).Count() *
                                          invoice.WirelessNotRevokePrice)
                    }, 
                new
                {
                    Title = "جریمه تاخیر در  جمع آوری (روز کاری) - دستگاه ثابت",UnitPrice = invoice.NotWirelessNotRevokePrice,
                    ReportId =9,
                    Count = (long) report8Data.Where(x => !x.IsWireless).Count(),
                    TotalPrice =  (long) (report8Data.Where(x => !x.IsWireless).Count() *
                                          invoice.NotWirelessNotRevokePrice)
                }
                     
                };

                return JsonSuccessResult(result);
            }
        }

        // GET: NormalRep
        [HttpGet]
        public ActionResult NormalRep()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> NewGetBankInvoiceReportData(int? year, int? month, int invoiceTypeId)
        {
            if (!year.HasValue || !month.HasValue)
            {
                return new EmptyResult();
            }

            var date = $"{year}/{month}/{1}".ToMiladiDate();
            var monthIsEven = month % 2 == 0;
            var daysInMonth = DateTimeExtensions.DaysInMonth(year.Value, month.Value);
            var fromDate = date.AbsoluteStart();
            var toDate = date.AddDays(daysInMonth - 1).AbsoluteEnd();

            var invoice = await _dataContext.Invoices.FirstOrDefaultAsync(x =>
                x.FromDate >= fromDate && x.ToDate <= toDate && x.InvoiceTypeId == invoiceTypeId);

            if (invoice == null)
            {
                return JsonErrorMessage("تنظیمات صورت وضعیت ثبت نشده است");
            }


            //   اجاره دستگاه های ثابت و سیار منصوبه


            var pc = new PersianCalendar();
            var reviewMonthTo = new DateTime(year.Value, month.Value, month <= 6 ? 31 : 30, pc);
            var reviewMonthFrom = new DateTime(year.Value, month.Value, 1, pc);


            #region new report 1

            var report1Data = _dataContext.Terminals.Where(b => b.MarketerId == (byte) Enums.Marketer.BankOrBranch
                                                                && b.InstallationDate <= reviewMonthTo
                                                                && b.InstallationDate.HasValue
                                                                && (!b.RevokeDate.HasValue ||
                                                                    b.RevokeDate.Value >= reviewMonthFrom)
                ).ToList().GroupBy(b => b.DeviceType.IsWireless)
                .Select(b => new {IsWireless = b.Key, Count = b.LongCount()});

            #endregion

            // var report1Data = await _dataContext.TempReport1And2Datas.Where(x => x.Month == month && x.Year == year)
            //     .GroupBy(x => x.IsWireless)
            //     .Select(x => new { IsWireless = x.Key, Count = x.LongCount() })
            //     .ToListAsync();


            // اجاره دستگاه های پایانه شعبه ای منصوبه در شعب
            // var report3Data = await _dataContext.TempReport3Datas.Where(x => x.Month == month && x.Year == year)
            //     .LongCountAsync();

            #region new report 3

            var report3Data = _dataContext.BranchTerminal
                .Where(b => (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom))
                .LongCount();

            #endregion


            //  جریمه تاخیر در اخذ شماره ترمینال - روز کاری
            // var  report4Data = await _dataContext.TempReport4Datas.Where(x => x.Month == month && x.Year == year).SumAsync(x => x.InstallationDelay);

            #region NewReport 4

            var holidays = _dataContext.Holidays.ToList().Select(b => b.Date)
                .ToList();

            var report4Data = _dataContext.Terminals
                .Where
                (b =>
                    b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                    b.InstallationDate <= reviewMonthTo
                    && b.InstallationDate.HasValue
                    && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
                    b.BatchDate >= reviewMonthFrom && b.BatchDate <= reviewMonthTo
                ).ToList()
                .Sum(a => a.GetTermialNumberDelay(invoice.GetTerminalNoDelayAllowedWorkDay, holidays));

            #endregion

            // جریمه تاخیر در نصب - روز کاری
            //   var  report5Data =   await _dataContext.TempReport5Datas.Where(x => x.Month == month && x.Year == year).SumAsync(x => x.InstallationDelay);

            #region new report 5

            var report5Data = _dataContext.Terminals
                .Where(b =>
                    b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                    b.InstallationDate <= reviewMonthTo
                    && b.InstallationDate.HasValue
                    && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom) &&
                    b.InstallationDate >= reviewMonthFrom && b.InstallationDate <= reviewMonthTo
                ).ToList()
                .Sum(a => a.GetInstallationDelay(invoice.NotInstalledDelayAllowedWorkDay, holidays));

            #endregion

            // جریمه تاخیر در EM - روز کاری
            //  var   report6Data =  await _dataContext.TempReport6Datas.Where(x => x.Month == month && x.Year == year).SumAsync(x => x.InstallationDelay);

            #region New Report 6

            var asdf = _dataContext.TerminalEms.Where(b =>
                    b.EmTime.HasValue && b.EmTime.Value >= reviewMonthFrom && b.EmTime.Value <= reviewMonthTo)
                .ToList().Select(a => a.GetInstallationDelay(invoice.NotEMDelayAllowedWorkDay, holidays));

            var report6Data = asdf.Sum(b => b);

            #endregion

            // جریمه دستگاه های PM نشده هر دوره تاخیر
            long calculatedReport7Data = 0;


            if (monthIsEven)
            {
                #region new report 7

                var pmdData = _dataContext.TerminalPms
                    .Where(b => b.PmTime <= reviewMonthTo && b.PmTime >= reviewMonthFrom).Select(a => a.TerminalNo)
                    .ToList();
                var allTerminal = _dataContext.Terminals.Where(b =>
                    b.MarketerId == (byte) Enums.Marketer.BankOrBranch &&
                    b.InstallationDate <= reviewMonthTo
                    && b.InstallationDate.HasValue
                    && (!b.RevokeDate.HasValue || b.RevokeDate >= reviewMonthFrom)
                ).Select(a => a.TerminalNo).ToList();

                var notPm = allTerminal.Where(b => !pmdData.Contains(b)).ToList();
                var notPmPercent = Math.Round((decimal) notPm.Count / allTerminal.Count * 100);
                if (notPmPercent > 15)
                    calculatedReport7Data = notPm.Count;

                #endregion


                var result = new List<object>
                {
                    new
                    {
                        Title = "اجاره دستگاه های ثابت منصوبه", UnitPrice = invoice.WithWireMarketerPrice,
                        Count = report1Data.Where(x => !x.IsWireless).Sum(x => x.Count),
                        TotalPrice = (report1Data.Where(x => !x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WithWireMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های سیار منصوبه", UnitPrice = invoice.WirlessBankMarketerPrice,
                        Count = report1Data.Where(x => x.IsWireless).Sum(x => x.Count),
                        TotalPrice = (report1Data.Where(x => x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های پایانه شعبه ای منصوبه در شعب",
                        UnitPrice = invoice.WirlessBankMarketerPrice, Count = report3Data,
                        TotalPrice = report3Data * invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "جریمه تاخیر در اخذ شماره ترمینال (روز کاری)",
                        UnitPrice = invoice.NotGetTerminalNoFinePricePerWorkDay, Count = report4Data,
                        TotalPrice = (long?) report4Data * invoice.NotGetTerminalNoFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در نصب (روز کاری)",
                        UnitPrice = invoice.NotInstalledFinePricePerWorkDay,
                        Count = report5Data,
                        TotalPrice = (long?) report5Data * invoice.NotInstalledFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در EM (روز کاری)", UnitPrice = invoice.NotEMFinePricePerWorkDay,
                        Count = report6Data, TotalPrice = (long?) report6Data * invoice.NotEMFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه دستگاه های PM نشده (هر دوره تاخیر)",
                        UnitPrice = invoice.NotPMFinePricePerDevice,
                        Count = calculatedReport7Data,
                        TotalPrice = calculatedReport7Data * invoice.NotPMFinePricePerDevice
                    },
                };

                return JsonSuccessResult(result);
            }
            else
            {
                var result = new List<object>
                {
                    new
                    {
                        Title = "اجاره دستگاه های ثابت منصوبه", UnitPrice = invoice.WithWireMarketerPrice,
                        Count = report1Data.Where(x => !x.IsWireless).Sum(x => x.Count),
                        TotalPrice = (report1Data.Where(x => !x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WithWireMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های سیار منصوبه", UnitPrice = invoice.WirlessBankMarketerPrice,
                        Count = report1Data.Where(x => x.IsWireless).Sum(x => x.Count),
                        TotalPrice = (report1Data.Where(x => x.IsWireless).Sum(x => (long?) x.Count) ?? 0) *
                                     invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "اجاره دستگاه های پایانه شعبه ای منصوبه در شعب",
                        UnitPrice = invoice.WirlessBankMarketerPrice, Count = report3Data,
                        TotalPrice = report3Data * invoice.WirlessBankMarketerPrice
                    },
                    new
                    {
                        Title = "جریمه تاخیر در اخذ شماره ترمینال (روز کاری)",
                        UnitPrice = invoice.NotGetTerminalNoFinePricePerWorkDay, Count = report4Data,
                        TotalPrice = (long?) report4Data * invoice.NotGetTerminalNoFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در نصب (روز کاری)",
                        UnitPrice = invoice.NotInstalledFinePricePerWorkDay,
                        Count = report5Data,
                        TotalPrice = (long?) report5Data * invoice.NotInstalledFinePricePerWorkDay
                    },
                    new
                    {
                        Title = "جریمه تاخیر در EM (روز کاری)", UnitPrice = invoice.NotEMFinePricePerWorkDay,
                        Count = report6Data, TotalPrice = (long?) report6Data * invoice.NotEMFinePricePerWorkDay
                    }
                };

                return JsonSuccessResult(result);
            }
        }

    

        [HttpGet]
        public ActionResult GeneralReport()
        {
            return View();
        }

        //[HttpPost]
        //public Task<ActionResult> ReportShebaAccount()
        //{
        //    return View();
        //}


        [HttpGet]
        public ActionResult ReportShebaAccount()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ComplementaryReport()
        {
            return View();
        }

        

        #region Psp Files
             [HttpPost]
        public async Task<ActionResult> UploadFanavaFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadFanavaFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/UploadFanavaFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> UploadIrankishFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadIrankishFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/UploadIrankishFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }
        [HttpPost]
        public async Task<ActionResult> UploadParsianFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadParsianFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/UploadParsianFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }
        [HttpPost]
        public async Task<ActionResult> UploadPardakhtFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadPardakhtFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/UploadPardakhtFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }

        [HttpGet]
        public async Task<ActionResult> GetComplementaryReport(int year, int month)
        {
           

            var client = new RestClient($"http://localhost:5072/GetResultComplementaryReport?year={year}&month={month}");
            // var client = new RestClient($"http://192.168.10.102:8008/GetResultComplementaryReport?year={year}&month={month}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();

            result.Data = response.Content;


            var data = JsonConvert.DeserializeObject<List<ComplemetaryReportField>>(result.Data.ToString());
            return View();
        }


        #endregion

        #region Psp Transaction Files
        [HttpPost]
        public async Task<ActionResult> UploadTransactionFanavaFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadTransactionFanavaFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/UploadTransactionFanavaFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> UploadTransactionIrankishFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadTransactionIrankishFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/UploadTransactionIrankishFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }
        [HttpPost]
        public async Task<ActionResult> UploadTransactionParsianFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadTransactionParsianFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/UploadTransactionParsianFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }
        [HttpPost]
        public async Task<ActionResult> UploadTransactionPardakhtFile(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/UploadTransactionPardakhtFile?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/UploadTransactionPardakhtFile?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }
        #endregion

        #region Sheba And Account
        [HttpPost]
        public async Task<ActionResult> ShebaAndAccount(int year, int month)
        {
            var client = new RestClient($"http://localhost:5072/CalcuteShebaAndAccountReport?month={month}&year={year}");
            // var client = new RestClient($"http://192.168.10.102:8008/ComplementaryReport/CalcuteShebaAndAccountReport?month={month}&year={year}");
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            var response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
            JsonResult result = new JsonResult();
            if (response.Content.Contains("200"))
            {
                result.Data = response.Content;
            }

            Console.WriteLine(response.Content);

            return result;
        }

      
        #endregion



    }
}