using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using EntityFramework.Extensions;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using TES.Common;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;
using Stimulsoft.Report;
using Stimulsoft.Report.Mvc;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize]
    public class CardRequestController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public CardRequestController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var message = _dataContext.Messages.ToList();
            //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.Open   
            //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.UnderReview   
            //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            //  var cardmessage = _dataContext.CardRequest.ToList();
            // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
            //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                            || User.IsCardRequestManager())); 
            //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
            //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
            //                                                                  || User.IsCardRequestManager()));
            //  ViewBag.OpenCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.Open  
            //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
            //                                                                  || User.IsCardRequestManager()));
            ViewBag.UserId = CurrentUserId;
            return View();
        }


        [HttpGet]
        public ActionResult CardRequest()
        {
            var message = _dataContext.Messages.ToList();
            //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.Open   
            //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                              || User.IsMessageManagerUser()));
            //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
            //                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                    || User.IsMessageManagerUser()));
            //  
            //  var cardmessage = _dataContext.CardRequest.ToList();
            // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
            //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                            || User.IsCardRequestManager())); 
            //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
            //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
            //                                                                  || User.IsCardRequestManager()));
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetData(TicketIndexViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.CardRequest
                .Include(b => b.CardServiceType)
                .Include(b => b.CardType).AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.searchGuid))
            {
                query = query.Where(x => x.GUID.Contains(viewModel.searchGuid.ToString()));
            }

            if (viewModel.FromCreationDate.HasValue)
            {
                query = query.Where(x => x.CreationDate >= viewModel.FromCreationDate);
            }

            if (viewModel.ToCreationDate.HasValue)
            {
                query = query.Where(x => x.CreationDate <= viewModel.ToCreationDate);
            }

            if (viewModel.StatusId.HasValue)
            {
                query = query.Where(x => x.StatusId == viewModel.StatusId);
            }

            // فقط تیکت هایی که هیچ کسی در حال بررسیش نیست
            if (viewModel.JustNotReviewingMessages)
            {
                query = query.Where(x => !x.ReviewerUserId.HasValue);
            }


            // اگر مدیر سیستم بود یا مدیر تیکت ها همه رو ببینه 
            if (!User.IsAdmin() && !User.IsCardRequestManager() && !User.IsCardRequester() &&
                !User.IsJustCardRequester())
            {
                query = query.Where(x => x.UserId == CurrentUserId || x.ReviewerUserId == CurrentUserId);
            }

            var totalRowsCount = await query.CountAsync(cancellationToken);

            var ro = query
                .Include(x => x.Replies)
                .Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.StatusId,
                    x.GUID,
                    x.FileNameFinalFile,
                    x.Count,
                    Priority = x.Priority == 1 ? "عادی" : "فوری",
                    PrintType = x.PrintType == 1 ? "ساده" : "برجسته",
                    DeliveryType = x.DeliveryType == 1 ? "مراجعه از شعبه" : "ارسال به ستاد",
                    x.TemplateId,
                    Type = x.CardType.Type,
                    BranchId = x.OrganizationUnitId,
                    x.EndDate,
                    x.Body,
                    x.Price,
                    EnteredFullName = x.FullName,
                    EnteredPhone = x.Phone,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.CardRequestStatus.Title,
                    ReviewerFullName = x.ReviewerUser.FullName,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    CreationDate = x.CreationDate,
                    CardServiceType = x.CardServiceType.Type,
                    LastReplyCreationDate = x.Replies.OrderByDescending(y => y.CreationDate).Select(y =>
                        y.CreationDate
                    ).FirstOrDefault()
                })
                .OrderByDescending(x => x.Id)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .ToList();

            var rows = ro.Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.StatusId,
                    x.GUID,
                    x.Body,
                    x.Count,
                    x.BranchId,
                    x.TemplateId,
                    x.Type,
                    x.Priority,
                    x.EnteredFullName,
                    x.EnteredPhone,
                    x.UserFullName,
                    x.Price,
                    x.StatusTitle,
                    x.DeliveryType,
                    x.FileNameFinalFile,
                    x.PrintType,
                    EndDate = x.EndDate.HasValue ? x.EndDate.Value.ToPersianDate() : "",
                    x.CardServiceType,
                    x.ReviewerFullName,
                    CreationDate = x.CreationDate.ToPersianDate(),
                    LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                        ? x.LastReplyCreationDate.ToPersianDate()
                        : ""
                })
                .OrderByDescending(x => x.Id)
                .ToList();
            return JsonSuccessResult(new {rows, totalRowsCount});
        }

        #region Report

        public ActionResult ActionIEnumerable(int Id)
        {
            var Model = new HavelReport();
            Model.Id = Id;
            return View("ViewIEnumerable", Model);
        }


        public ActionResult ViewerEvent()
        {
            return StiMvcViewer.ViewerEventResult();
        }

        public class ReportDs
        {
            public string HavelehNumber { get; set; }
        }

        public class sss
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
            public string VahedShomaresh { get; set; }
            public int Count { get; set; }
            public string Price { get; set; }
            public long BranchId { get; set; }
            public string TemplateId { get; set; }
            public string RequestDate { get; set; }
            public string DeliverDate { get; set; }
            public string Body { get; set; }
        }

        public ActionResult GetReportIEnumerable(int Id)
        {
            var report = new StiReport();
            report.Load(Server.MapPath("~/Content/Report/Report.mrt"));
            var data = _dataContext.CardRequest.FirstOrDefault(b => b.Id == Id);

            var HavelehNumber = "1";


            var ssss = new List<sss>();
            ssss.Add(new sss()
            {
                Id = 1,
                Name = "کارت هدیه ",
                Status = "نو",
                VahedShomaresh = "عدد",
                Count = data.Count,
                Price = data.Price,
                BranchId = data.OrganizationUnitId,
                TemplateId = data.TemplateId,
                RequestDate = data.CreationDate.ToPersianDate(),
                DeliverDate = DateTime.Now.ToPersianDate(),
                Body = data.Body
            });
            ssss.Add(new sss() {Id = 2});
            // ssss.Add(new sss() {Id = 3});
            // ssss.Add(new sss() {Id = 4});
            // ssss.Add(new sss() {Id = 5});
            // ssss.Add(new sss() {Id = 6});
            // ssss.Add(new sss() {Id = 7});
            // ssss.Add(new sss() {Id =8});
            var obj = new
            {
                HavelehNumber = data.GUID,
                Date = DateTime.Now.ToPersianDate(),
                Data = ssss.Select(a => new
                {
                    Id = a.Id,
                    Name = a.Name,
                    Status = a.Status,
                    VahedShomaresh = a.VahedShomaresh, a.Count, a.Price, a.BranchId,
                    a.TemplateId, a.RequestDate,
                    a.DeliverDate, a.Body
                })
            };
            report.StoreImagesInResources = true;


            report.Compile();
            report.RegBusinessObject("ReportDs", obj);

            return StiMvcViewer.GetReportResult(report);
        }

        private void CheckReference(StiReport report)
        {
            var assemblyName = Assembly.GetExecutingAssembly().ManifestModule.Name;
            var refs = new List<string>(report.ReferencedAssemblies);
            if (!refs.Contains(assemblyName))
            {
                refs.Add(assemblyName);
                report.ReferencedAssemblies = refs.ToArray();
            }
        }

        #endregion

        [HttpGet]
        public async Task<ActionResult> Details(long id, CancellationToken cancellationToken)
        {
            //   var message = _dataContext.CardRequest.ToList();
            // ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.Open   
            //                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                             || User.IsMessageManagerUser()));
            //
            // ViewBag.InProgressMessage =message.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
            //                                               && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                   || User.IsMessageManagerUser()));
            //       var cardmessage = _dataContext.CardRequest.ToList();
            //             ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
            //                                                                   && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
            //                                                                       || User.IsCardRequestManager())); 
            //             ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
            //                                                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
            //                                                                             || User.IsCardRequestManager()));
            // ViewBag.UserId = CurrentUserId;
            var query = _dataContext.CardRequest.Where(x => x.Id == id);

            if (!User.IsAdmin() && !User.IsMessageManagerUser() && !User.IsCardRequester())
            {
                query = User.IsAcceptorsExpertUser()
                    ? query.Where(x => !x.ReviewerUserId.HasValue || x.ReviewerUserId == CurrentUserId)
                    : query.Where(x => x.UserId == CurrentUserId);
            }

            var messageData = await query
                .Select(x => new
                {
                    CardServiceType = x.CardServiceType.Type,
                    x.TemplateId,
                    x.Count,
                    x.Price,
                    Message = x,
                    x.Id,
                    x.Body,
                    x.EndDate,
                    x.HasPacket,
                    x.UserId,
                    x.Subject,
                    x.StatusId,
                    x.CreationDate,
                    x.ReviewerUserId,
                    x.LastChangeStatusDate,
                    UserFullName = x.User.FullName,
                    x.GUID,
                    Status = x.CardRequestStatus.Title,
                    x.PrintType,
                    Type = x.CardType.Type,
                    x.DeliveryType,
                    x.Priority,
                    Branch = x.OrganizationUnit.Id + " - " + x.OrganizationUnit.Title,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    Documents = x.MessageDocuments.Select(y => new {y.Id, y.FileName})
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (messageData == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var replies = _dataContext.CardRequestReplies
                .Where(x => x.MessageId == id)
                .Select(x => new
                {
                    x.Id,
                    x.Body,
                    x.CreationDate,
                    UserFullName = x.User.FullName,
                    Documents = x.MessageReplyDocuments.Select(y => new {y.Id, y.FileName}).ToList()
                })
                .ToList();

            var viewModel = new CardReqeustDetailsViewModel
            {
                CardServiceType = messageData.CardServiceType,
                Count = messageData.Count,
                TemplateId = messageData.TemplateId + ".jpg",
                TemplateCode = messageData.TemplateId,
                ByteArray = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(_dataContext.Storage
                    .FirstOrDefault(b => b.Code ==
                                         messageData.TemplateId).FileData)),
                Price = messageData.Price, Type = messageData.Type,
                HasPacket = messageData.HasPacket ? "بله" : "خیر ",
                DeliveryType = messageData.DeliveryType == 1 ? "مراجعه از شعبه" : "ارسال به ستاد",
                Branch = messageData.Branch,
                EndDatre = messageData.EndDate.ToPersianDate(),
                PrintType = messageData.PrintType == 1 ? "ساده" : "برجسته",
                Priority = messageData.Priority == 1 ? "عادی" : "فوری",
                Replies = replies.Select(x => new MessageReplyViewModel
                {
                    Id = x.Id,
                    Body = x.Body,
                    CreationDate = x.CreationDate,
                    UserFullName = x.UserFullName,
                    Documents = x.Documents.ToDictionary(y => y.Id, y => y.FileName)
                }).ToList(),
                Id = messageData.Id,
                Body = messageData.Body,
                UserId = messageData.UserId,
                GUID = messageData.GUID,
                Subject = messageData.Subject,
                StatusId = messageData.StatusId,
                Status = messageData.Status,
                CreationDate = messageData.CreationDate,
                UserFullName = messageData.UserFullName,
                ReviewerUserId = messageData.ReviewerUserId,
                LastChangeStatusDate = messageData.LastChangeStatusDate,
                ReviewerUserFullName = messageData.ReviewerUserFullName,
                Documents = messageData.Documents.ToDictionary(x => x.Id, x => x.FileName)
            };

            if (messageData.UserId == CurrentUserId)
            {
                messageData.Message.LastReplySeen = true;
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            ViewBag.UserId = CurrentUserId;
            ViewBag.UserList = (await _dataContext.Users.Where(d => d.Roles.Any(v => v.RoleId == 5))
                    .Select(x => new {UserId = x.Id, x.FullName})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.UserId, x => x.FullName);


            return View(viewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult Create()
        {
            var subjectListt = new List<string>();


            subjectListt = new List<string>();
            subjectListt.Add("500000");
            subjectListt.Add("10000000");
            subjectListt.Add("20000000");
            subjectListt.Add("50000000");
            ViewBag.PriceList = subjectListt
                .ToSelectList(x => x, x => x);


            // Array values = Enum.GetValues(typeof(Direction));
            // List<ListItem> items = new List<ListItem>(values.Length);
            //
            // foreach (var i in values)
            // {
            //     items.Add(new ListItem
            //     {
            //         Text = Enum.GetName(typeof(Direction), i),
            //         Value = i.ToString()
            //     });
            // }

            var tupleList = new List<(int, string)>
            {
                (1, "هدیه "),
                (2, "نقدی"),
                (3, "تک کارت")
            };


            ViewBag.TypeList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);


            tupleList = new List<(int, string)>
            {
                (1, "شخصی سازی کارت چند منظوره "),
                (2, "شخصی سازی کارت چند منظوره (پشت و روی کارت)"),
                (3, "شخصی سازی کارت نقدی، خرید، هدیه، اعتباری"),
                (4, "شخصی سازی چاپ لوگو مشکی"),
                (5, "شخصی سازی چاپ لوگو رنگی"),
                //      (6,"پاکت گذاری") 
            };

            ViewBag.ServiceTypeList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);


            tupleList = new List<(int, string)>
            {
                (1, "ساده "),

                (2, "برجسته")
            };


            ViewBag.PrintTypeList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);


            tupleList = new List<(int, string)>
            {
                (1, "عادی "),

                (2, "فوری")
            };

            ViewBag.PriorityList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);

            tupleList = new List<(int, string)>
            {
                (1, "مراجعه از شعبه "),

                (2, "ارسال به ستاد")
            };


            ViewBag.DeliveryType = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);


            var branchList = _dataContext.OrganizationUnits.Where(a => a.Id > 1000).ToList();
            ViewBag.BranchList = branchList
                .ToSelectList(x => x.Id, x => x.Id + " - " + x.Title);

            var CardRequestViewModel = new CardRequestViewModel();

            var packetresource = 0;

            packetresource = _dataContext.Storage.Where(b => !b.IsCard).Min(b => b.Value);


            var sss = _dataContext.Storage.Where(b => b.IsCard).ToList();

            ViewBag.TemplateList = sss.Select(e => new Template

                {
                    Code = e.Code,
                    Id = e.Id,
                    ImageName = e.Title,
                    Design = e.Design,
                    Total = e.Value,
                    Available = e.Value >= packetresource ? packetresource : e.Value,
                    ByteArray = string.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(e.FileData))
                })
                .ToList();
            return View("_Create", CardRequestViewModel);
        }

        [HttpPost]
        public ActionResult SetViewBag(bool value)
        {
            var sss = _dataContext.Storage.Where(b => b.IsCard).ToList();
            var packetresource = 0;
            if (value)
                packetresource = _dataContext.Storage.Where(b => !b.IsCard).Min(b => b.Value);

            else
            {
                packetresource = _dataContext.Storage.Where(b => b.Id == 51).Min(b => b.Value);
            }

            var s = sss.Select(e => new Template

                {
                    Code = e.Code,
                    Id = e.Id,
                    ImageName = e.Title,
                    Available = e.Value >= packetresource ? packetresource : e.Value,
                })
                .ToList();

            return Json(s);
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult UploadFinalFile(int Id)
        {
            var CardRequestViewModel = new CardRequestViewModel();
            CardRequestViewModel.Id = Id;
            return View("_UploadFinalFile", CardRequestViewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> UploadFile(CardRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (
                viewModel.PostedFiles.Any(d => d == null)
            )
            {
                return JsonErrorMessage("   بارگذاری فایل ضروری می باشد  ");
            }

            if (
                viewModel.PostedFiles.Any(d => d == null) ||
                (viewModel.PostedFiles.Any(x => x != null)
                 && viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                   || !x.IsValidFormat(".pdf") || x.ContentLength > 1 * 3024 * 3024)))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var cardrequest = _dataContext.CardRequest.FirstOrDefault(d => d.Id == viewModel.Id);
            cardrequest.FinalFile = viewModel.PostedFiles.FirstOrDefault().ToByteArray();
            cardrequest.FileNameFinalFile = viewModel.PostedFiles.FirstOrDefault().FileName;


            _dataContext.SaveChanges();

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(CardRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (
                viewModel.PostedFiles.Any(d => d == null)
            )
            {
                return JsonErrorMessage("   بارگذاری فایل ضروری می باشد  ");
            }

            if (
                viewModel.PostedFiles.Any(d => d == null) ||
                (viewModel.PostedFiles.Any(x => x != null)
                 && viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                   || !x.IsValidFormat(".mdb") || x.ContentLength > 1 * 3024 * 3024)))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var message = new CardRequest()
            {
                UserId = CurrentUserId,
                Count = viewModel.Count,
                TemplateId = viewModel.TemplateId,
                Price = viewModel.Price,
                HasPacket = viewModel.HasPacket,
                OrganizationUnitId = viewModel.BranchId,
                CardTypeId = viewModel.Type,
                CreationDate = DateTime.Now,
                UsePacket = viewModel.UsePacket,
                Body = viewModel.Body,
                PrintType = viewModel.PrintType,
                Priority = viewModel.Priority,
                DeliveryType = viewModel.DeliveryType,
                RemittanceDate = DateTime.Now,
                CardServiceTypeId = viewModel.CardServiceTypeId,
                StatusId = Enums.MessageStatus.Open.ToByte()
            };

            if (viewModel.PostedFiles.Any())
            {
                foreach (var item in viewModel.PostedFiles.Where(x => x != null && x.IsValidFile()))
                {
                    message.MessageDocuments.Add(new CardRequestDocument()
                        {FileData = item.ToByteArray(), FileName = item.FileName});
                }
            }

            _dataContext.CardRequest.Add(message);

            await _dataContext.SaveChangesAsync(cancellationToken);


            message.GUID = "TES-" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.GetPersianMonthInt() +
                           message.Id;
            await _dataContext.SaveChangesAsync(cancellationToken);

            AddInformationMessage($"تیکت شما با شماره پیگیری {message.GUID} در سیستم ثبت شد");


            //todo update storage

            var iiiiiiiiid = int.Parse(viewModel.Idd);
            var card = _dataContext.Storage.FirstOrDefault(b => b.Id == iiiiiiiiid);

            var storagelog = new StorageLog
            {
                Add = false,
                Date = DateTime.Now.ToPersianDateTime(),
                StorageId = iiiiiiiiid,
                User = User.Identity.GetFullName(),
                UserId = User.Identity.GetUserId(),
                Value = viewModel.Count
            };
            _dataContext.StorageLogs.Add(storagelog);
            card.Value -= viewModel.Count;

            if (viewModel.HasPacket)
            {
                var st = _dataContext.Storage.Where(b => !b.IsCard && (viewModel.HasPacket || b.Id != 48));
                foreach (var VARIABLE in st)
                {
                    storagelog = new StorageLog
                    {
                        Add = false,
                        Date = DateTime.Now.ToPersianDateTime(),
                        StorageId = VARIABLE.Id,
                        User = User.Identity.GetFullName(),
                        UserId = User.Identity.GetUserId(),
                        Value = viewModel.Count
                    };
                    _dataContext.StorageLogs.Add(storagelog);

                    VARIABLE.Value -= viewModel.Count;
                }

                _dataContext.SaveChanges();
            }
            else
            {
                var st = _dataContext.Storage.Where(b => b.Id == 51);
                foreach (var VARIABLE in st)
                {
                    storagelog = new StorageLog
                    {
                        Add = false,
                        Date = DateTime.Now.ToPersianDateTime(),
                        StorageId = VARIABLE.Id,
                        User = User.Identity.GetFullName(),
                        UserId = User.Identity.GetUserId(),
                        Value = viewModel.Count
                    };
                    _dataContext.StorageLogs.Add(storagelog);

                    VARIABLE.Value -= viewModel.Count;
                }

                _dataContext.SaveChanges();
            }

            return JsonSuccessResult();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser,
            DefaultRoles.ITUser)]
        public async Task<ActionResult> GetChangeAccountRequestDocument(long Id, CancellationToken cancellationToken)
        {
            var query = _dataContext.CardRequest.Where(x => x.Id == Id);


            var data = await query.Select(x => new {x.FileNameFinalFile, x.FinalFile})
                .FirstOrDefaultAsync(cancellationToken);

            if (data?.FinalFile == null || data.FinalFile.Length == 0)
            {
                return new EmptyResult();
            }


            var fileContentType = "application/pdf";
            var fileExtension = ".pdf";

            var f = File(data.FinalFile, fileContentType, $"{data.FileNameFinalFile}{fileExtension}");
            return f;
        }

        [HttpPost]
        [AjaxOnly]
        // [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchUser)]
        public async Task<ActionResult> ChangeStatus(MessageChangeStatusViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var query = _dataContext.CardRequest.Where(x => x.Id == viewModel.MessageId);

            // اگر کاربر کارشناس پذیرنده بود فقط مواردی که یا کاربر بررسی کننده ندارن یا اگر دارن مساوی کاربر فعلی هست رو می تونن ببینن
            if (User.IsAcceptorsExpertUser() && !User.IsAdmin())
            {
                query = query.Where(x => !x.ReviewerUserId.HasValue || x.ReviewerUserId == CurrentUserId);
            }

            var message = await query.FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return JsonWarningMessage("تیکت یافت نشد، یا شما اجازه تغییر وضعیت آن را ندارید");
            }

            bool canReject =
                (message.LastChangeStatusDate.HasValue &&
                 message.LastChangeStatusDate.Value.Date.AddDays(5) > DateTime.Now.Date) &&
                message.UserId == CurrentUserId &&
                message.StatusId == (int) TES.Common.Enumerations.MessageStatus.Close;


            // // اگر وضعیت انتخاب شده "در حال بررسی" بود کاربر بررسی کننده تیکت هم تنظیم میشود
            if (viewModel.StatusId == Enums.MessageStatus.UnderReview.ToByte())
            {
                message.ReviewerUserId = viewModel.UserId; // CurrentUserId;
            }

            // اگر وضعیت بسته بود و مدیر سیستم باز رو انتخاب کرده بود کارشناس بررسی کننده خالی می شود تا همه بتونن بردارن تیکت رو
            if (message.StatusId == Enums.MessageStatus.Close.ToByte() &&
                viewModel.StatusId == Enums.MessageStatus.Open.ToByte())
            {
                message.ReviewerUserId = null;
            }

            if (viewModel.StatusId == Enums.CardRequestStatus.Closed.ToByte())
            {
                message.EndDate = DateTime.Now;
            }

            message.StatusId = viewModel.StatusId;
            message.LastChangeStatusDate = DateTime.Now;

            await _dataContext.SaveChangesAsync(cancellationToken);

            //====================================>/
            if (viewModel.StatusId == TES.Common.Enumerations.CardRequestStatus.Cancel.ToByte())
            {
                //roleback =>
             
            var card = _dataContext.Storage.FirstOrDefault(b => b.Code == message.TemplateId);

            var storagelog = new StorageLog
            {
                Add = true,
                Date = DateTime.Now.ToPersianDateTime(),
                StorageId = card.Id,
                User = User.Identity.GetFullName(),
                UserId = User.Identity.GetUserId(),
                Value = message.Count
            };
            _dataContext.StorageLogs.Add(storagelog);
            card.Value += message.Count;

            if (message.HasPacket)
            {
                var st = _dataContext.Storage.Where(b => !b.IsCard && (message.HasPacket || b.Id != 48));
                foreach (var VARIABLE in st)
                {
                    storagelog = new StorageLog
                    {
                        Add = true,
                        Date = DateTime.Now.ToPersianDateTime(),
                        StorageId = VARIABLE.Id,
                        User = User.Identity.GetFullName(),
                        UserId = User.Identity.GetUserId(),
                        Value = message.Count
                    };
                    _dataContext.StorageLogs.Add(storagelog);

                    VARIABLE.Value += message.Count;
                }

                _dataContext.SaveChanges();
            }
            else
            {
                var st = _dataContext.Storage.Where(b => b.Id == 51);
                foreach (var VARIABLE in st)
                {
                    storagelog = new StorageLog
                    {
                        Add = true,
                        Date = DateTime.Now.ToPersianDateTime(),
                        StorageId = VARIABLE.Id,
                        User = User.Identity.GetFullName(),
                        UserId = User.Identity.GetUserId(),
                        Value = message.Count
                    };
                    _dataContext.StorageLogs.Add(storagelog);

                    VARIABLE.Value += message.Count;
                }

                _dataContext.SaveChanges();
            }
            
            }

            return JsonSuccessMessage();
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Download(TicketIndexViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.CardRequest.AsQueryable();

            if (viewModel.Id.HasValue)
            {
                query = query.Where(x => x.Id == viewModel.Id);
            }

            if (viewModel.FromCreationDate.HasValue)
            {
                query = query.Where(x => x.CreationDate >= viewModel.FromCreationDate);
            }

            if (viewModel.ToCreationDate.HasValue)
            {
                query = query.Where(x => x.CreationDate <= viewModel.ToCreationDate);
            }

            if (viewModel.StatusId.HasValue)
            {
                query = query.Where(x => x.StatusId == viewModel.StatusId);
            }

            // فقط تیکت هایی که هیچ کسی در حال بررسیش نیست
            if (viewModel.JustNotReviewingMessages)
            {
                query = query.Where(x => !x.ReviewerUserId.HasValue);
            }

            // اگر مدیر سیستم بود یا مدیر تیکت ها همه رو ببینه 
            if (!User.IsAdmin() && !User.IsMessageManagerUser())
            {
                // اگر کارشناس پذیرنده بود فقط تیکت هایی که بررسی کننده ندارن یا بررسی کنندش خودش هست رو میبینه 
                if (User.IsAcceptorsExpertUser())
                {
                    query = query.Where(x => !x.ReviewerUserId.HasValue || x.ReviewerUserId == CurrentUserId);
                }
                // در غیر این صورت فقط تیکت های خودش رو
                else
                {
                    query = query.Where(x => x.UserId == CurrentUserId);
                }
            }

            var data = await query
                .Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.StatusId,
                    x.GUID,
                    x.FileNameFinalFile,
                    x.Count,
                    Priority = x.Priority == 1 ? "عادی" : "فوری",
                    PrintType = x.PrintType == 1 ? "ساده" : "برجسته",
                    DeliveryType = x.DeliveryType == 1 ? "مراجعه از شعبه" : "ارسال به ستاد",
                    x.TemplateId,
                    Type = x.CardType.Type,
                    BranchId = x.OrganizationUnitId,
                    x.EndDate,
                    x.Body,
                    x.Price,
                    EnteredFullName = x.FullName,
                    EnteredPhone = x.Phone,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.CardRequestStatus.Title,
                    ReviewerFullName = x.ReviewerUser.FullName,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    CreationDate = x.CreationDate,
                    CardServiceType = x.CardServiceType.Type,
                    LastReplyCreationDate = x.Replies.OrderByDescending(y => y.CreationDate).Select(y =>
                        y.CreationDate
                    ).FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

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
                worksheet.Column(3).Width = 0;
                worksheet.Column(4).Width = 25;
                worksheet.Column(5).Width = 25;
                worksheet.Column(6).Width = 26;
                worksheet.Column(7).Width = 16;
                worksheet.Column(8).Width = 18;
                worksheet.Column(9).Width = 18;
                worksheet.Column(10).Width = 18;
                worksheet.Column(11).Width = 18;
                worksheet.Column(12).Width = 18;
                worksheet.Column(13).Width = 18;
                worksheet.Column(14).Width = 18;
                worksheet.Column(15).Width = 18;
                worksheet.Column(16).Width = 18;
                worksheet.Column(17).Width = 18;


                worksheet.Cells[1, 1].Value = "کد پیگیری";
                worksheet.Cells[1, 2].Value = "وضعیت ";
                worksheet.Cells[1, 3].Value = "کاربر درخواست دهنده  ";
                worksheet.Cells[1, 4].Value = "ارجاع شده  ";
                worksheet.Cells[1, 5].Value = "نوع کارت    ";
                worksheet.Cells[1, 6].Value = " نوع درخواست    ";
                worksheet.Cells[1, 7].Value = "تعداد کارت  ";
                worksheet.Cells[1, 8].Value = "  	مبلغ   ";
                worksheet.Cells[1, 9].Value = "  کد شعبه   ";
                worksheet.Cells[1, 10].Value = "  کد طرح کارت   ";
                worksheet.Cells[1, 11].Value = "   	ارجحیت  ";
                worksheet.Cells[1, 12].Value = "  نوع چاپ   ";
                worksheet.Cells[1, 13].Value = "  	نحوه تحویل   ";
                worksheet.Cells[1, 14].Value = "  تاریخ ثبت درخواست   ";
                worksheet.Cells[1, 15].Value = "  تاریخ اعلام خاتمه کار   ";
                worksheet.Cells[1, 16].Value = "   	ملاحظات  ";
                worksheet.Cells[1, 17].Value = "   تاریخ تحویل  ";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.GUID;
                    worksheet.Cells[rowNumber, 2].Value = item.StatusTitle;
                    worksheet.Cells[rowNumber, 3].Value = item.UserFullName;
                    worksheet.Cells[rowNumber, 4].Value = item.ReviewerFullName;
                    worksheet.Cells[rowNumber, 5].Value = item.Type;
                    worksheet.Cells[rowNumber, 6].Value = item.CardServiceType;
                    worksheet.Cells[rowNumber, 7].Value = item.Count;
                    worksheet.Cells[rowNumber, 8].Value = item.Price;
                    worksheet.Cells[rowNumber, 9].Value = item.BranchId;
                    worksheet.Cells[rowNumber, 10].Value = item.TemplateId;
                    worksheet.Cells[rowNumber, 11].Value = item.Priority;
                    worksheet.Cells[rowNumber, 12].Value = item.PrintType;
                    worksheet.Cells[rowNumber, 13].Value = item.DeliveryType;
                    worksheet.Cells[rowNumber, 14].Value = item.CreationDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 15].Value = item.EndDate.ToPersianDate();
                    worksheet.Cells[rowNumber, 16].Value = item.Body;
                    worksheet.Cells[rowNumber, 17].Value = item.EndDate.ToPersianDate();

                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CardRequest.xlsx");
                }
            }
        }
    }
}