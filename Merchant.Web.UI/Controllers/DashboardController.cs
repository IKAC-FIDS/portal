using Persia;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Common.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public long? CurrentUserBranchId => User.Identity.GetBranchId();
        public string CurrentUserBranch  => User.Identity.GetBranchTitle();
        public DashboardController(AppDataContext dataContext)
        {
       
            
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {

            if (User.IsJustCardRequester())
            {
                return Redirect("\\CardRequest");
            }
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.Open   
           //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                              || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.UnderReview   
           //                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                    || User.IsMessageManagerUser()));
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte)Enums.TerminalStatus.Deleted  
                                                        
                                                          ).AsQueryable();

            // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            // اگر نقش کاربر سرپرستی بود اطلاعات خودش و زیر مجموعه هایش را ببیند
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

            var news = await _dataContext.News
                .OrderByDescending(x => x.IsMain)
                .ThenByDescending(x => x.PublishDate)
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                    Body = x.Body,
                    Title = x.Title,
                    IsMain = x.IsMain,
                    PublishDate = x.PublishDate
                })
                .ToListAsync(cancellationToken);

            var viewModel = new DashboardViewModel
            {
                News = news,
                OpenTicketCount = await _dataContext.Messages.Where(x => x.UserId == CurrentUserId && x.StatusId == (byte)Enums.MessageStatus.Open).CountAsync(cancellationToken),
                NeedToReformTerminalCount = await query.CountAsync(x => x.StatusId == (byte)Enums.TerminalStatus.NeedToReform || x.StatusId == (byte)Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken),
                NewCount = await query.CountAsync(x => x.StatusId == (byte)Enums.TerminalStatus.New, cancellationToken),
                WaitingForRevokeCount = await query.CountAsync(x => x.StatusId == (byte)Enums.TerminalStatus.WaitingForRevoke, cancellationToken)
            };

            #region [ نمودار دایره ای به تفکیک وضعیت ]

            viewModel.TerminalsByStatusData = query
                .GroupBy(x => x.Status.Title)
                .Select(x => new
                {
                    Title = x.Key,
                    Count = x.Count()
                })
                .ToList()
                .Select(x => new object[]
                {
                    x.Title,
                    x.Count
                })
                .ToArray();

            #endregion

            #region [ نمودار دایره ای به تفکیک شرکت PSP ]

            if (!User.IsBranchUser())
            {
                viewModel.TerminalsByPspData = query
                    .Where(x => x.StatusId == (byte)Enums.TerminalStatus.Installed)
                    .GroupBy(x => x.Psp.Title)
                    .Select(x => new
                    {
                        Title = x.Key,
                        Count = x.Count()
                    })
                    .ToList()
                    .Select(x => new object[]
                    {
                        x.Title,
                        x.Count
                    })
                    .ToArray();
            }

            #endregion

            #region [ نمودار دایره ای به تفکیک نوع دستگاه ]

            viewModel.TerminalsByDeviceTypeData = query
                .Where(x => x.StatusId == (byte)Enums.TerminalStatus.Installed)
                .GroupBy(x => x.DeviceType.Title)
                .Select(x => new
                {
                    Title = x.Key,
                    Count = x.Count()
                })
                .ToList()
                .Select(x => new object[]
                {
                    x.Title,
                    x.Count
                })
                .ToArray();

            #endregion

            #region [ نمودار میله ای به تفکیک استان ]

            if (!User.IsBranchUser())
            {
                var data = query
                    .Where(x => x.StatusId == (byte)Enums.TerminalStatus.Installed)
                    .GroupBy(x => x.City.State.Title)
                    .Select(x => new
                    {
                        Title = x.Key,
                        Count = x.Count()
                    })
                    .ToList();

                viewModel.TerminalsByStateChart = new ChartViewModel
                {
                    ChartData = data.Select(x => (long)x.Count).ToList(),
                    ChartCategories = data.Select(x => x.Title).ToList()
                };
            }

            #endregion



            #region  PspRating

            var pspBranchRates = _dataContext.PspBranchRate.Where(d => d.OrganizationUnitId == CurrentUserBranchId).ToList();

            if (pspBranchRates.FirstOrDefault(p => p.PspId == 1) != null)
            {
                viewModel.FaRate = pspBranchRates.FirstOrDefault(p => p.PspId == 1).Rate;
            }
            else
            {
                viewModel.FaRate = 100;
            }
            
            if (pspBranchRates.FirstOrDefault(p => p.PspId == 2) != null)
            {
                viewModel.IkRate = pspBranchRates.FirstOrDefault(p => p.PspId == 2).Rate;
            }
            else
            {
                viewModel.IkRate = 100;
            }
            
            
            if (pspBranchRates.FirstOrDefault(p => p.PspId == 3) != null)
            {
                viewModel.PaRate = pspBranchRates.FirstOrDefault(p => p.PspId == 3).Rate;
            }
            else
            {
                viewModel.PaRate = 100;
            }
            #endregion



      
            var t = System.IO.File.Exists(@"c:\test.json");
            if (t)
            {
                var qq = new RatingViewModel();
                using (var file = System.IO.File.OpenText(@"c:\test.json"))
                using (var reader = new JsonTextReader(file))
                {
                    var jsonString =  System.IO.File.ReadAllText(@"c:\test.json");
                    qq = JsonConvert.DeserializeObject<RatingViewModel>(jsonString);
                }
                viewModel.Rating = qq.Rating;
                viewModel.LastUpdate = qq.LastUpdate;
            }

         
            return View(viewModel);
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> TransactionCharts(CancellationToken cancellationToken)
        {
            var localYearMonths = new List<int>();
            var today = Calendar.ConvertToPersian(DateTime.Today);

            var lastTransactionYearMonth = await _dataContext.TransactionSums.OrderByDescending(x => x.PersianLocalYearMonth).Select(x => new { x.PersianLocalYear, x.PersianLocalMonth }).FirstOrDefaultAsync(cancellationToken);
            var currentYear = lastTransactionYearMonth?.PersianLocalYear ?? today.ArrayType[0];
            var currentMonth = lastTransactionYearMonth?.PersianLocalMonth ?? today.ArrayType[1];

            for (var i = 0; i < 12; i++)
            {
                if (currentMonth >= 1)
                {
                    localYearMonths.Add(Convert.ToInt32(currentYear + currentMonth.ToString("00")));
                }
                else
                {
                    currentMonth = 12;
                    currentYear--;
                    localYearMonths.Add(Convert.ToInt32(currentYear + currentMonth.ToString("00")));
                }

                currentMonth--;
            }

            var viewModel = new TransactionChartsViewModel();

            var transactions = (await _dataContext.TransactionSums
                .Where(x => localYearMonths.Contains(x.PersianLocalYearMonth))
                .GroupBy(x => x.PersianLocalYearMonth)
                .Select(x => new { Price = x.Average(y => y.BuyTransactionAmount ?? 0), Count = x.Average(y => (long?)y.BuyTransactionCount ?? 0), x.Key })
                .ToListAsync(cancellationToken))
                .Select(x => new { x.Price, x.Count, YearMonth = x.Key });

            #region [ نمودار خطی میانگین مبلغ تراکنش های خرید یک سال اخیر ]

            var transactionPriceChartData = localYearMonths.OrderBy(x => x).Select(x => new
            {
                Date = x.ToString(),
                AveragePrice = (long?)transactions.FirstOrDefault(y => y.YearMonth == x)?.Price ?? 0
            })
            .ToList();

            viewModel.TransactionsPriceChart = new ChartViewModel
            {
                ChartCategories = transactionPriceChartData.Select(x => x.Date).ToList(),
                ChartData = transactionPriceChartData.Select(x => x.AveragePrice).ToList()
            };

            #endregion

            #region [ نمودار خطی میانگین تعداد تراکنش های خرید یک سال اخیر ]

            var transactionCountChartData = localYearMonths.OrderBy(x => x).Select(x => new
            {
                Date = x.ToString(),
                Count = (long?)transactions.FirstOrDefault(y => y.YearMonth == x)?.Count ?? 0
            })
            .ToList();

            viewModel.TransactionsCountChart = new ChartViewModel
            {
                ChartCategories = transactionCountChartData.Select(y => y.Date).ToList(),
                ChartData = transactionCountChartData.Select(y => y.Count).ToList()
            };

            #endregion

            return PartialView("_TransactionCharts", viewModel);
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> LastSixMonthTransactionStatusChart(CancellationToken cancellationToken)
        {
            var localYearMonths = new List<int>();
            var today = Calendar.ConvertToPersian(DateTime.Today);
            var lastTransactionYearMonth = await _dataContext.TransactionSums.OrderByDescending(x => x.PersianLocalYearMonth).Select(x => new { x.PersianLocalYear, x.PersianLocalMonth }).FirstOrDefaultAsync(cancellationToken);
            var currentYear = lastTransactionYearMonth?.PersianLocalYear ?? today.ArrayType[0];
            var currentMonth = lastTransactionYearMonth?.PersianLocalMonth ?? today.ArrayType[1];

            for (var i = 0; i < 6; i++)
            {
                if (currentMonth >= 1)
                {
                    localYearMonths.Add(Convert.ToInt32(currentYear + currentMonth.ToString("00")));
                }
                else
                {
                    currentMonth = 12;
                    currentYear--;
                    localYearMonths.Add(Convert.ToInt32(currentYear + currentMonth.ToString("00")));
                }

                currentMonth--;
            }

            var transactions = await _dataContext.LastSixMonthTransactionStatus(Convert.ToInt32(localYearMonths.Last()), Convert.ToInt32(localYearMonths.First()), User.Identity.GetBranchId(), User.IsSupervisionUser(), User.IsBranchUser(), User.IsTehranBranchManagementUser(), User.IsCountyBranchManagementUser());

            var viewModel = localYearMonths.OrderBy(x => x).Select(x => new LastSixMonthTransactionStatusChartViewModel
            {
                PersianLocalYearMonth = x.ToString(),
                HighTransactionCount = transactions.Where(y => y.PersianLocalYearMonth == x).Sum(y => y.HighTransactionCount),
                LowTransactionCount = transactions.Where(y => y.PersianLocalYearMonth == x).Sum(y => y.LowTransactionCount),
                WithoutTransactionCount = transactions.Where(y => y.PersianLocalYearMonth == x).Sum(y => y.WithoutTransactionCount)
            })
            .ToList();

            return PartialView("_LastSixMonthTransactionStatusChart", viewModel);
        }
    }
}