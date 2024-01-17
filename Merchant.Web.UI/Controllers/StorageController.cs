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
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
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
    public class StorageController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public StorageController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
           
            ViewBag.UserId = CurrentUserId;
            return View();
        }

        public Storage CreateResource(CreateResourceDto input)
        {
            var storageitem = new Storage
            {
                Title = input.Title,
                Design = input.Design,
                Value = input.Value
            };
            _dataContext.Storage.Add(storageitem);
            _dataContext.SaveChanges();
            return (storageitem);
        }

        [HttpGet]
        public ActionResult CardRequest()
        {
             
            return View();
        }
        
            
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetStorageLogData(int id, CancellationToken cancellationToken)
        {
            var sxvxcv = _dataContext.StorageLogs.Where(b => b.StorageId == id).ToList();
            if (!sxvxcv.Any())
            {
                return JsonSuccessResult();
            }
            var viewModel =  
                sxvxcv.Select(x => new StorageLogDto
                {
                    User = x.User,
                    Operation = x.Add ? "اضافه نمودن" : "کم کردن",
                    Date = x.Date,
                    Value = x.Value,
                    StorageId = x.StorageId
                    ,
                }).ToList();


            var rows = viewModel.Select(x => new
                {
                   
                    x.Value,
                    x.User,
                    x.Operation,
                    x.Date
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            return JsonSuccessResult(new {rows, viewModel.Count});
        }
        

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetData(TicketIndexViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.Storage
                .AsQueryable();


            var totalRowsCount = await query.CountAsync(cancellationToken);

            var ro = query
                .Select(x => new
                {
                    x.Id,
                    Card = x.IsCard ? "بله" : "خیر",
                    x.Value,
                    x.Waste,
                    x.Design,
                    x.Title,
                    Total = x.Value + x.Waste
                })
                .OrderByDescending(x => x.Id)
        
             
                .ToList();

            var rows = ro.Select(x => new
                {
                    x.Id,
                    x.Card,
                    x.Value,
                    x.Design,
                    x.Waste,
                    x.Title,
                    x.Total
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
           
            ViewBag.UserId = CurrentUserId;
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
        public async Task<ActionResult> GetDocument(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.Storage.Where(x => x.Id == id);


            var data = await query.Select(x => new {x.FileName, x.FileData}).FirstOrDefaultAsync(cancellationToken);

            if (data == null || data.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            return File(data.FileData, "application/octet-stream", data.FileName);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Edit(EditResourceDto viewModel, CancellationToken cancellationToken)
        {
            var news = await _dataContext.Storage.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
            
            if (  viewModel.IsCard &&
                ( 
                 (viewModel.PostedFiles.Any(x => x != null)
                  && viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                    || !x.IsValidFormat(".jpg") || x.ContentLength > 1 * 3024 * 3024))))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            if ((viewModel.PostedFiles.Any(x => x != null)
                 && viewModel.PostedFiles.Any(x =>  x.IsValidFile())))
            {
                news.FileData = news.IsCard ? viewModel.PostedFiles.FirstOrDefault().ToByteArray() : null;
                news.FileName = news.IsCard ? viewModel.PostedFiles.FirstOrDefault()?.FileName : null;

            }
          
            news.Title = viewModel.Title;
            news.Design = viewModel.Design;
            news.Code = viewModel.Code;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
        
        [HttpPost]
        [AjaxOnly] 
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var news = await _dataContext.Storage.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.Storage.Remove(news);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> AddValue(EditResourceDto viewModel, CancellationToken cancellationToken)
        {
            var storagelog = new StorageLog();
            storagelog.Add = true;
            storagelog.Date = DateTime.Now.ToPersianDateTime();
            storagelog.StorageId = viewModel.Id;
            storagelog.User = User.Identity.GetFullName();
            storagelog.UserId = User.Identity.GetUserId();
            storagelog.Value = viewModel.Value;
            _dataContext.StorageLogs.Add(storagelog);
            var news = await _dataContext.Storage.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
            news.Value += viewModel.Value;
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }
        
        
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> MinusValue(EditResourceDto viewModel, CancellationToken cancellationToken)
        {
            var storagelog = new StorageLog
            {
                Add = false,
                Date = DateTime.Now.ToPersianDateTime(),
                StorageId = viewModel.Id,
                User = User.Identity.GetFullName(),
                UserId = User.Identity.GetUserId(),
                Value = viewModel.Value
            };
            _dataContext.StorageLogs.Add(storagelog);
            var news = await _dataContext.Storage.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
            news.Value -= viewModel.Value;
            news.Waste += viewModel.Value;
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }

        
        [HttpGet]
        public async Task<ActionResult> StorageLogs(int id )
        {
             
            var viewModels =   _dataContext.StorageLogs.Where(b => b.StorageId == id).ToList();
            if (!viewModels.Any())
            {
                return View("_storageLogs",  new StorageLogDto() );
            }
            var storages = _dataContext.Storage.FirstOrDefault(b => b.Id == id);
          var viewModel =      viewModels.Select(x => new StorageLogDto
                {
                    User = x.User,
                    Operation = x.Add ? "اضافه نمودن" : "کم کردن",
                    Date = x.Date,
                    Value = x.Value,
                    StorageId = x.StorageId
                    ,
                    StorageTitle = storages.Title
                }).ToList();
             
            return View("_storageLogs", viewModel.FirstOrDefault());
            
            
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> AddValue(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.Storage
                .Select(x => new EditResourceDto
                {
                    Id = x.Id,
                    Design = x.Design,
                   
                    IsCard = x.IsCard,
                    Title = x.Title,
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_AddValue", viewModel);
        }
        
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> MinusValue(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.Storage
                .Select(x => new EditResourceDto
                {
                    Id = x.Id,
                    Design = x.Design,
                   
                    IsCard = x.IsCard,
                    Title = x.Title,
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_MinusValue", viewModel);
        }
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.Storage
                .Select(x => new EditResourceDto
                {
                    Id = x.Id,
                    Design = x.Design,
                    Value = x.Value,
                    IsCard = x.IsCard,
                    Title = x.Title,
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_Edit", viewModel);
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
            CardRequestViewModel.TemplateList = _dataContext.CardTemplate.Select(e => new Template

                {
                    Code = e.Code,
                    Id = e.Id,
                    ImageName = e.ImageName
                })
                .ToList();
            return View("_Create", CardRequestViewModel);
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

        [HttpGet]
        public ActionResult Manage()
        {
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(CreateResourceDto viewModel, CancellationToken cancellationToken)
        {
            if (
                viewModel.IsCard && viewModel.PostedFiles.Any(d => d == null)
            )
            {
                return JsonErrorMessage("   بارگذاری فایل ضروری می باشد  ");
            }

            if (
                viewModel.IsCard &&
                (viewModel.PostedFiles.Any(d => d == null) ||
                 (viewModel.PostedFiles.Any(x => x != null)
                  && viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                    || !x.IsValidFormat(".jpg") || x.ContentLength > 1 * 3024 * 3024))))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var storageitem = new Storage
            {
                Title = viewModel.Title,
                Design = viewModel.IsCard ? viewModel.Design : "",
                Value = viewModel.Value,
                Code= viewModel.Code,
                IsCard = viewModel.IsCard,
                FileData = viewModel.IsCard ? viewModel.PostedFiles.FirstOrDefault().ToByteArray() : null,
                FileName = viewModel.IsCard ? viewModel.PostedFiles.FirstOrDefault()?.FileName : null,
                Waste = 0
            };
            _dataContext.Storage.Add(storageitem);

            await _dataContext.SaveChangesAsync(cancellationToken);

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