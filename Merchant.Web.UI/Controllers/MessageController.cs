using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Dapper;
using OfficeOpenXml;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using TES.Web.Core.Helper;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize]
    public class MessageController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public MessageController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
           
            return View();
        }


        [HttpGet]
        public ActionResult Message()
        {
          
            return View();
        }
        
            
       
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetData(TicketIndexViewModel viewModel, string orderByColumn,
            string orderByDirection, CancellationToken cancellationToken)
        {
            var query = _dataContext.Messages.Include(b => b.User.OrganizationUnit).AsQueryable();

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

            if (viewModel.BranchId.HasValue)
            {
                query = query.Where(x => x.User.OrganizationUnit.Id == viewModel.BranchId);
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


            if (!User.IsAdmin() && !User.IsMessageManagerUser() && !User.IsItUser() && !User.IsBranchManagementUser())
            {
                if (User.IsTehranBranchManagementUser())
                {
                    query = query.Where(x => x.User.OrganizationUnit.CityId == (long) Enums.City.Tehran);
                }
                else if (User.IsCountyBranchManagementUser())
                {
                    query = query.Where(x => x.User.OrganizationUnit.CityId != (long) Enums.City.Tehran);
                }
                else if (User.IsSupervisionUser())
                {
                    query = query.Where(x =>
                        x.User.OrganizationUnit.Id == CurrentUserBranchId ||
                        x.User.OrganizationUnit.ParentId == CurrentUserBranchId);
                }
                else
                {
                    query = query.Where(x => x.UserId == CurrentUserId || x.ReviewerUserId == CurrentUserId);
                }
            }


            var totalRowsCount = await query.CountAsync(cancellationToken);

            var OpenMessage = query.Where(b => b.StatusId == (byte) Enums.MessageStatus.Open).Count();
            var InProgressMessage = query.Where(b => b.StatusId == (byte) Enums.MessageStatus.UnderReview).Count();
            var Closed = query.Where(b => b.StatusId == (byte) Enums.MessageStatus.Close).Count();
            var Rejected = query.Where(b => b.StatusId == (byte) Enums.MessageStatus.Reject).Count();


            var secondsubjects = _dataContext.MessageSubjects.Where(b => b.ParentId.HasValue).ToList();

            var ro = query
                .Include(x => x.Replies)
                .Select(x => new
                {
                    x.Id,
                    SubjectId = x.MessageSubjectId,
                    x.StatusId,
                    x.GUID,

                    x.Body,
                    Subject = x.MessageSubject.Title,
                   
                    EnteredFullName = x.FullName,
                    EnteredPhone = x.Phone,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.MessageStatus.Title,
                    ReviewerFullName = x.ReviewerUser.FullName,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    CreationDate = x.CreationDate,
                    x.MessageSecondSubjectId,
                    LastReplyCreationDate = x.Replies.OrderByDescending(y => y.CreationDate).Select(y =>
                        y.CreationDate).FirstOrDefault()
                })
                .OrderByDescending(x => x.Id)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .ToList();

            if (string.IsNullOrEmpty(orderByColumn))
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.SubjectId,
                        x.StatusId,
                        x.Subject,
                        x.GUID,
                        x.Body,
                        SecondSubject = x.MessageSecondSubjectId.HasValue ? secondsubjects.FirstOrDefault(b=>b.Id == x.MessageSecondSubjectId)
                            ?.Title :  "",
                        x.EnteredFullName,
                        x.EnteredPhone,
                        x.UserFullName,
                        x.StatusTitle,
                        x.ReviewerFullName,
                        CreationDate = x.CreationDate.ToPersianDate(),
                        LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                            ? x.LastReplyCreationDate.ToPersianDate()
                            : "",
                        lastReplyCreationDateMiladi = x.LastReplyCreationDate
                    })
                    .ToList();
                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, Closed, Rejected});
            }

            if (orderByDirection.Contains("DESC"))
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.SubjectId,
                        x.StatusId,
                        x.GUID,
                        x.Body,
                        SecondSubject = x.MessageSecondSubjectId.HasValue ? secondsubjects.FirstOrDefault(b=>b.Id == x.MessageSecondSubjectId)
                            ?.Title :  "",
                        x.Subject,
                        x.EnteredFullName,
                        x.EnteredPhone,
                        x.UserFullName,
                        x.StatusTitle,
                        x.ReviewerFullName,
                        CreationDate = x.CreationDate.ToPersianDate(),
                        LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                            ? x.LastReplyCreationDate.ToPersianDate()
                            : "",
                        lastReplyCreationDateMiladi = x.LastReplyCreationDate
                    })
                    .OrderByDescending(orderByColumn)
                    .ToList();
                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, Closed, Rejected});
            }
            else
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.SubjectId,
                        x.StatusId,
                        x.GUID,
                        x.Body,
                        SecondSubject = x.MessageSecondSubjectId.HasValue ? secondsubjects.FirstOrDefault(b=>b.Id == x.MessageSecondSubjectId)
                            ?.Title :  "",
                        x.EnteredFullName,
                        x.EnteredPhone,
                        x.UserFullName,
                        x.StatusTitle,
                        x.ReviewerFullName,
                        CreationDate = x.CreationDate.ToPersianDate(),
                        LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                            ? x.LastReplyCreationDate.ToPersianDate()
                            : "",
                        lastReplyCreationDateMiladi = x.LastReplyCreationDate
                    })
                    .OrderBy(orderByColumn)
                    .ToList();

                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, Closed, Rejected});
            }
        }

        [HttpGet]
        public async Task<ActionResult> Details(long id, CancellationToken cancellationToken)
        {
          
            ViewBag.UserId = CurrentUserId;
            var query = _dataContext.Messages.Where(x => x.Id == id);


            if (!User.IsAdmin() && !User.IsMessageManagerUser() && !User.IsItUser() && !User.IsBranchManagementUser())
            {
                if (User.IsTehranBranchManagementUser())
                {
                    query = query.Where(x => x.User.OrganizationUnit.CityId == (long) Enums.City.Tehran);
                }
                else if (User.IsCountyBranchManagementUser())
                {
                    query = query.Where(x =>
                        !x.User.OrganizationUnit.CityId.HasValue &&
                        x.User.OrganizationUnit.CityId != (long) Enums.City.Tehran);
                }
                else if (User.IsSupervisionUser())
                {
                    query = query.Where(x =>
                        x.User.OrganizationUnit.Id == CurrentUserBranchId ||
                        x.User.OrganizationUnit.ParentId == CurrentUserBranchId);
                }
                else
                {
                    query = User.IsAcceptorsExpertUser()
                        ? query.Where(x => !x.ReviewerUserId.HasValue || x.ReviewerUserId == CurrentUserId)
                        : query.Where(x => x.UserId == CurrentUserId);
                }
            }

            var subjects = _dataContext.MessageSubjects.ToList();

            var messageData = await query
                .Select(x => new
                {
                    Message = x,
                    x.Id,
                    x.Body,
                    x.UserId,
                    SubjectId = x.MessageSubjectId,
                    x.StatusId,
                    Subject = x.MessageSubject.Title,
                    x.CreationDate,
                    x.ReviewerUserId,
                    x.MessageSecondSubjectId,
                    x.LastChangeStatusDate,
                    x.Phone,
                    UserFullName = x.User.FullName,
                    Status = x.MessageStatus.Title,
                    x.ExtraDataTopic,
                    x.ExtraDataTopicValue,
                    x.GUID,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    Documents = x.MessageDocuments.Select(y => new {y.Id, y.FileName})
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (messageData == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var replies = _dataContext.MessageReplies
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


            var tupleList = new List<(int, string)>
            {
                (1, " کد ملی پذیرنده "),
                (2, "شماره پایانه"),
                (3, " شماره پیگیری  "),
                (4, "  شماره مشتری ")
            };


            var viewModel = new MessageDetailsViewModel
            {
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
                Phone = messageData.Phone,
                Subject = messageData.Subject,
                StatusId = messageData.StatusId,
                SecondSubject = subjects.Where(b=>b.Id == messageData.MessageSecondSubjectId).FirstOrDefault()?.Title,
                Status = messageData.Status,
                ExtraDataTopicValue = messageData.ExtraDataTopicValue,
                ExtraDataTopic = messageData.ExtraDataTopic.HasValue
                    ? tupleList.FirstOrDefault(b => b.Item1 == messageData.ExtraDataTopic).Item2
                    : "",
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
        
            
        public JsonResult LoadSubject(int terminalId)
        {
            var pspid = _dataContext.Terminals.FirstOrDefault(b => b.Id == terminalId).PspId;
            var subcategoriesList = _dataContext.MessageSubjects.Where(b => ((b.PspId == 4  && b.PspId != 5)
                                                                             ||  b.PspId == pspid ) && !b.ParentId.HasValue).ToList();

            var subjectListt = _dataContext.MessageSubjects.ToList();
            ViewBag.SubjectList = subjectListt
                .ToSelectList(x => x.Id, x => x.Title);

            var subcategoriesData = subcategoriesList.Select(m => new SelectListItem()
            {
                Text = m.Title.ToString(),
                Value = m.Id.ToString(),
            });
            return Json(subcategoriesData, JsonRequestBehavior.AllowGet);
            
            
        }
        public JsonResult LoadSecondSubject(int categoryId)
        {
            var subcategoriesList = _dataContext.MessageSubjects.Where(b => b.ParentId == categoryId).ToList();

            var subjectListt = _dataContext.MessageSubjects.ToList();
            ViewBag.SubjectList = subjectListt
                .ToSelectList(x => x.Id, x => x.Title);

            var subcategoriesData = subcategoriesList.Select(m => new SelectListItem()
            {
                Text = m.Title.ToString(),
                Value = m.Id.ToString(),
            });
            return Json(subcategoriesData, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult Create()
        {
            // var subjectListt = new List<string>();
            //
            // subjectListt.Add("نارضایتی از شرکت فن آوا کارت");
            //   subjectListt.Add(" نارضایتی از شرکت ایران کیش"   );
            //   subjectListt.Add("نارضایتی از شرکت تجارت الکترونیک پارسیان "   );
            //   subjectListt.Add(" پیگیری نصب"   );
            //   subjectListt.Add(" پیگیری جمع آوری"   );
            //   subjectListt.Add(" پیگیری مغایرت حساب"   );
            //   subjectListt.Add(" پیگیری تغییر حساب "   );
            //   subjectListt.Add("سایر  "   );


            var subjectListt = _dataContext.MessageSubjects.Where(b => b.ParentId == null).ToList();
            ViewBag.SubjectList = subjectListt
                .ToSelectList(x => x.Id, x => x.Title);


            var tupleList = new List<(int, string)>
            {
                (1, " کد ملی پذیرنده "),
                (2, "شماره پایانه"),
                (3, " شماره پیگیری  "),
                (4, "  شماره مشتری ")
            };


            ViewBag.TypeList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2);


            return View("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(MessageViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.PostedFiles.Any(x => x != null) &&
                viewModel.PostedFiles.Any(x =>
                    !x.IsValidFile() || !x.IsValidFormat(".jpg,.jpeg,.pdf,.docx,.png,.xls,.xlsx") ||
                    x.ContentLength > 1 * 1024 * 1024))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var message = new Message
            {
                Body = viewModel.Body,
                Phone = viewModel.Phone,
                FullName = viewModel.FullName,
                UserId = CurrentUserId,
                //  Subject = viewModel.Subject,
                MessageSubjectId = viewModel.MessageSubjectId,
                MessageSecondSubjectId = viewModel.MessageSecondSubjectId,
                CreationDate = DateTime.Now,
                ExtraDataTopicValue = viewModel.ExtraDataTopicValue,
                ExtraDataTopic = 3,

                StatusId = Enums.MessageStatus.Open.ToByte()
            };

            if (viewModel.PostedFiles.Any())
            {
                foreach (var item in viewModel.PostedFiles.Where(x => x != null && x.IsValidFile()))
                {
                    message.MessageDocuments.Add(new MessageDocument
                        {FileData = item.ToByteArray(), FileName = item.FileName});
                }
            }

            _dataContext.Messages.Add(message);

            await _dataContext.SaveChangesAsync(cancellationToken);


            message.GUID = "TES-" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.GetPersianMonthInt() +
                           message.Id;
            await _dataContext.SaveChangesAsync(cancellationToken);

            AddInformationMessage($"تیکت شما با شماره پیگیری {message.GUID} در سیستم ثبت شد");

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        // [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser,DefaultRoles.BranchUser)]
        public async Task<ActionResult> ChangeStatus(MessageChangeStatusViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var query = _dataContext.Messages.Where(x => x.Id == viewModel.MessageId);

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

            // اگر وضعیت در حالت بسته قرار داشت فقط مدیر می تواند آن را باز کند
            if ((message.StatusId == Enums.MessageStatus.Close.ToByte() && !User.IsAdmin()) && !canReject)
            {
                return JsonErrorMessage("تیکت در حالت بسته قرار دارد و امکان تغییر وضعیت آن برای شما میسر نیست");
            }

            // اگر وضعیت انتخاب شده "در حال بررسی" بود کاربر بررسی کننده تیکت هم تنظیم میشود
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

            message.StatusId = viewModel.StatusId;
            message.LastChangeStatusDate = DateTime.Now;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Download(TicketIndexViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.Messages.AsQueryable();

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
                    x.Body,
                    x.GUID,
                    SubjectId = x.MessageSubjectId,
                    Subject = x.MessageSubject.Title,
                    SeconSubject = x.MessageSecondSubject.Title,
                    x.CreationDate,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.MessageStatus.Title,
                    ReviewerFullName = x.ReviewerUser.FullName,
                    x.FullName,x.ExtraDataTopicValue,
                   LastComment =  x.Replies.OrderByDescending(y => y.CreationDate)
                        .Select(y =>   y.Body).FirstOrDefault(),
                    LastReplyCreationDate = x.Replies.OrderByDescending(y => y.CreationDate)
                        .Select(y => (DateTime?) y.CreationDate).FirstOrDefault()
                })
                .ToListAsync(cancellationToken);


            var terminals = _dataContext.Terminals.ToList();
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
                worksheet.Column(9).Width = 16;

                worksheet.Column(8).Width = 16;
                
                worksheet.Column(10).Width = 200;
                worksheet.Column(13).Width = 200;

                worksheet.Cells[1, 1].Value = "وضعیت تیکت";
                worksheet.Cells[1, 2].Value = "کد پیگیری";
                worksheet.Cells[1, 3].Value = "موضوع";
                worksheet.Cells[1, 4].Value = "موضوع دوم";
                worksheet.Cells[1, 5].Value = " نام کاربری  ";
                worksheet.Cells[1, 6].Value = " نام و نام خانوادگی ارجاع دهنده  ";
                worksheet.Cells[1, 7].Value = "  ارجاع گیرنده    ";
                worksheet.Cells[1, 8].Value = "تاریخ ثبت  ";
                worksheet.Cells[1, 9].Value = "تاریخ آخرین اقدام";
                worksheet.Cells[1, 10].Value = "متن پیام";

                worksheet.Cells[1, 11].Value = " شماره پیگیری  ";
                worksheet.Cells[1, 12].Value = "  PSP    ";
                worksheet.Cells[1, 13].Value = "  آخرین پیام    ";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    try
                    {
                        worksheet.Cells[rowNumber, 2].Value = item.GUID;
                        worksheet.Cells[rowNumber, 1].Value = item.StatusTitle;
                        worksheet.Cells[rowNumber, 3].Value = item.Subject;
                        worksheet.Cells[rowNumber, 4].Value = item.SeconSubject;
                        worksheet.Cells[rowNumber, 5].Value = item.UserFullName;
                        worksheet.Cells[rowNumber, 6].Value = item.FullName;
                        worksheet.Cells[rowNumber, 7].Value = item.ReviewerFullName;
                        worksheet.Cells[rowNumber, 8].Value = item.CreationDate.ToPersianDateTime();
                        worksheet.Cells[rowNumber, 9].Value = item.LastReplyCreationDate?.ToPersianDateTime();
                        worksheet.Cells[rowNumber, 10].Value = item.Body;


                        worksheet.Cells[rowNumber, 11].Value = item.ExtraDataTopicValue;
                        worksheet.Cells[rowNumber, 12].Value = item.ExtraDataTopicValue != null
                            ? terminals
                                .FirstOrDefault(b => b.Id == long.Parse(item.ExtraDataTopicValue))?.Psp?.Title
                            : "";
                        worksheet.Cells[rowNumber, 13].Value = item.LastComment;


                    }
                    catch
                    {
                        continue;
                    }
                    rowNumber++;
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    return File(memoryStream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tickets.xlsx");
                }
            }
        }
    }
}