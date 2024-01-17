using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.ViewModels;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class PropertyDamageRequestController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public PropertyDamageRequestController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> UploadFile(DamageRequestUploadViewModel viewModel, CancellationToken cancellationToken)
        {
            if (
                viewModel.PostedFiles == null
            )
            {
                return JsonErrorMessage("   بارگذاری فایل ضروری می باشد  ");
            }
            if (
                viewModel.PostedFiles == null || 
                ( viewModel.PostedFiles != null
                  && !viewModel.PostedFiles.IsValidFile() 
                    &&  !viewModel.PostedFiles.IsValidFormat(".jpg,.jpeg,.pdf,.docx,.png")
                                                     ||
                  viewModel.PostedFiles.ContentLength > 1 * 3024 * 3024))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            var damageRequest = _dataContext.DamageRequest.FirstOrDefault(d => d.Id == viewModel.Id);
            if (damageRequest != null)
            {
                damageRequest.DamageRequestStatusId =(byte) viewModel.Status   ;
                damageRequest.FinalFile = viewModel.PostedFiles.ToByteArray();
                damageRequest.FileNameFinalFile = viewModel.PostedFiles.FileName;
                damageRequest.LastChangeStatusDate = DateTime.Now;
            }

            _dataContext.SaveChanges();

            return JsonSuccessResult();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator, DefaultRoles.ITUser,
            DefaultRoles.BranchUser,DefaultRoles.BranchManagment)]
        public async Task<ActionResult> Manage(CancellationToken cancellationToken)
        {
            ViewBag.StatusList = (await _dataContext.RequestStatus
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Id)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Id)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            var message = _dataContext.Messages.ToList();
            ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
                                                     && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                         || User.IsMessageManagerUser()));
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator, DefaultRoles.ITUser,
            DefaultRoles.BranchUser)]
        public ActionResult GetRevokeRequestData(RequestSearchParameters viewModel)
        {
            viewModel.IsBranchUser = User.IsBranchUser();
            viewModel.CurrentUserBranchId = CurrentUserBranchId;
            viewModel.IsSupervisionUser = User.IsSupervisionUser();
            viewModel.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            viewModel.IsCountyBranchManagment = User.IsCountyBranchManagementUser();

            var data = _dataContext.GetRevokeRequestData(viewModel, viewModel.RetriveTotalPageCount, viewModel.Page - 1,
                300, out var totalRowsCount);

            return JsonSuccessResult(new {rows = data, totalRowsCount});
        }
        [HttpGet]
        [AjaxOnly]
        public ActionResult UploadFinalFile(int Id,int statusId)
        {
         
            var tupleList = new List<(int, string)>
            {
               
                (3, " پرداخت شده از سوی بانک  "),
                (6, "  پرداخت شده از سوی مشتری    "),
                (7, "  عدم نیاز به پرداخت    "),
              
            };


            ViewBag.closeStatus = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2
                    , selectedValue : new[] {1});
            
            var CardRequestViewModel = new CardRequestViewModel();
            CardRequestViewModel.Id =  Id;
           
            return View("_UploadFinalFile",CardRequestViewModel);

        }
        
        [HttpGet]
        public async Task<ActionResult> GetDocument(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.DamageRequestDocument.Where(x => x.Id == id);

           

            var data = await query.Select(x => new { x.FileName, x.FileData }).FirstOrDefaultAsync(cancellationToken);

            if (data == null || data.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            return File(data.FileData, "application/octet-stream", data.FileName);
        }
        
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator, DefaultRoles.BranchManagment,
            DefaultRoles.BranchUser)]
        public async Task<ActionResult> GetData(TicketIndexViewModel viewModel, string orderByColumn,
            string orderByDirection, CancellationToken cancellationToken)
        {
            var query = _dataContext.DamageRequest.Include(b => b.User.OrganizationUnit).AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.searchGuid))
            {
                query = query.Where(x => x.GUID.Contains(viewModel.searchGuid.ToString()));
            }
            if (viewModel.BranchId.HasValue)
            {
                query = query.Where(x => x.OrganizationUnitId == viewModel.BranchId);
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
                query = query.Where(x => x.DamageRequestStatusId == viewModel.StatusId);
            }

            // فقط تیکت هایی که هیچ کسی در حال بررسیش نیست
            if (viewModel.JustNotReviewingMessages)
            {
                query = query.Where(x => !x.ReviewerUserId.HasValue);
            }


            if ( User.IsBranchUser() )
            {
                 
                    query = query.Where(x => x.OrganizationUnitId   ==  CurrentUserBranchId);
                
            }


            var totalRowsCount = await query.CountAsync(cancellationToken);

            var OpenMessage = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.Open).Count();
            var InProgressMessage = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.UnderReview)
                .Count();
            var ClosedByBranch = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.PayFromBranch
            
            ).Count();
            
            var ClosedByCustomer = query.Where(b => 
                                          b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.PayFromCustomer
            ).Count();
            
            var Rejected = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.Delay).Count();

            var EndOfProcess = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.EndProcess).Count();

            var NoNeedToPay = query.Where(b => b.DamageRequestStatusId == (byte) Enums.DamageRequestStatus.NoNeedForPayment).Count();

            
            var data = query.Include(b=>b.OrganizationUnit).Include(b=>b.Replies).ToList();

            var ro = data
              
                .Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.DamageRequestStatusId,
                    x.GUID,
x.DamageValue,
                    x.FileNameFinalFile,
                    x.Body,

                    EnteredFullName = x.FullName,

                    OrganizationId = x.OrganizationUnitId,
                    x.OrganizationUnit.Title,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.DamageRequestStatus.Title,
                    ReviewerFullName = x.ReviewerUser?.FullName,
                    ReviewerUserFullName = x.ReviewerUser?.FullName,
                    CreationDate = x.CreationDate,

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
                        x.Subject,
                        x.DamageRequestStatusId,
                        x.GUID,
                        x.Body,
                        x.EnteredFullName,
                        Title =  x.Title + " " + x.OrganizationId,
x.FileNameFinalFile,
x.DamageValue,
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
                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, ClosedByBranch,ClosedByCustomer
                    ,EndOfProcess , NoNeedToPay
                    , Rejected});
            }

            if (orderByDirection.Contains("DESC"))
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.Subject,
                        x.DamageRequestStatusId,
                        x.GUID,
                        x.Body,
                        x.EnteredFullName,
                        x.FileNameFinalFile,
                        Title =  x.Title + " " + x.OrganizationId,
                        x.UserFullName,
                        x.StatusTitle,
                        x.DamageValue,
                        x.ReviewerFullName,
                        CreationDate = x.CreationDate.ToPersianDate(),
                        LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                            ? x.LastReplyCreationDate.ToPersianDate()
                            : "",
                        lastReplyCreationDateMiladi = x.LastReplyCreationDate
                    })
                    .OrderByDescending(orderByColumn)
                    .ToList();
                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, ClosedByBranch,ClosedByCustomer
                    ,EndOfProcess , NoNeedToPay
                    , Rejected});
            }
            else
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.Subject,
                        x.DamageRequestStatusId,
                        x.GUID,
                        x.Body,
                        x.EnteredFullName,
x.FileNameFinalFile,
Title =  x.Title + " " + x.OrganizationId,
                        x.UserFullName,
                        x.StatusTitle,
                        x.ReviewerFullName,
                        x.DamageValue,
                        CreationDate = x.CreationDate.ToPersianDate(),
                        LastReplyCreationDate = x.LastReplyCreationDate != DateTime.MinValue
                            ? x.LastReplyCreationDate.ToPersianDate()
                            : "",
                        lastReplyCreationDateMiladi = x.LastReplyCreationDate
                    })
                    .OrderBy(orderByColumn)
                    .ToList();

                return JsonSuccessResult(new {rows, totalRowsCount, OpenMessage, InProgressMessage, ClosedByBranch,ClosedByCustomer
                    ,
                    EndOfProcess , NoNeedToPay
                    , Rejected});
            }
        }

        [HttpGet]
     
        public async Task<ActionResult> GetChangeAccountRequestDocument(long Id, CancellationToken cancellationToken)
        {
            var query = _dataContext.DamageRequest.Where(x => x.Id == Id);

             
            var data = await query.Select(x => new { x.FileNameFinalFile, x.FinalFile }).FirstOrDefaultAsync(cancellationToken);

            if (data?.FinalFile == null || data.FinalFile.Length == 0)
            {
                return new EmptyResult();
            }

            string fileContentType = MimeMapping.GetMimeMapping(data.FileNameFinalFile);          
            var fileExtension = data.FileNameFinalFile.Split('.')[1];

            var f = File(data.FinalFile, fileContentType, $"{data.FileNameFinalFile}");
            return f;
        }
          [HttpGet]
        public async Task<ActionResult> Details(long id, CancellationToken cancellationToken)
        {
 
            ViewBag.UserId = CurrentUserId;
            var query = _dataContext.DamageRequest.Where(x => x.Id == id);

             
                if (User.IsBranchUser())
                {
                    
                        query =      query.Where(x => x.OrganizationUnitId == CurrentUserBranchId);
                     
                 
                }
                
           

            var messageData = await query
                .Select(x => new
                {
                    Message = x,
                    x.Id,
                    x.Body,
                    x.UserId,
                    x.Subject,
                    x.DamageRequestStatusId,
                    x.CreationDate,
                    x.ReviewerUserId,
                    x.LastChangeStatusDate,
                    x.OrganizationUnitId,
                    UserFullName = x.User.FullName,
                    Status = x.DamageRequestStatus.Title,
                    x.ExtraDataTopic,
                    x.ExtraDataTopicValue,
                    x.SerialNumber,
                    x.DamageValue,
                    x.GUID,
                    ReviewerUserFullName = x.ReviewerUser.FullName,
                    Documents = x.DamageRequestDocuments.Select(y => new { y.Id, y.FileName })
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (messageData == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var replies = _dataContext.DamageRequestReply
                .Where(x => x.DamageRequestId == id)
                .Select(x => new
                {
                    x.Id,
                    x.Body,
                    x.CreationDate,
                    UserFullName = x.User.FullName,
                    Documents = x.MessageReplyDocuments.Select(y => new { y.Id, y.FileName }).ToList()
                })
                .ToList();

            
            var  tupleList = new List<(int, string)>
            {
                (1, "شماره پایانه"),
                (2, " کد پیگیری پایانه  "),


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
                SerialNumber = messageData.SerialNumber,
                DamageValue = messageData.DamageValue,
                OrganizationUnitId = messageData.OrganizationUnitId,
                Subject = messageData.Subject,
                StatusId = messageData.DamageRequestStatusId,
                Status = messageData.Status ,
                ExtraDataTopicValue = messageData.ExtraDataTopicValue ,
                ExtraDataTopic = messageData.ExtraDataTopic.HasValue ? tupleList.FirstOrDefault(b=>b.Item1 ==  messageData.ExtraDataTopic ).Item2 : "",
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
            ViewBag.UserList = (await _dataContext.Users.Where(d=>d.Roles.Any(v=>v.RoleId == 5))
                    .Select(x => new { UserId = x.Id, x.FullName })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.UserId, x => x.FullName);
     
            return View(viewModel);
        }
    [HttpPost]
        [AjaxOnly]
       
        public async Task<ActionResult> ChangeStatus(MessageChangeStatusViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.DamageRequest.Where(x => x.Id == viewModel.MessageId);

          

            var message = await query.FirstOrDefaultAsync(cancellationToken);

            if (message == null)
            {
                return JsonWarningMessage("درخواست یافت نشد، یا شما اجازه تغییر وضعیت آن را ندارید");
            }

             
            
             
            // اگر وضعیت بسته بود و مدیر سیستم باز رو انتخاب کرده بود کارشناس بررسی کننده خالی می شود تا همه بتونن بردارن تیکت رو
            if (
                (
                    message.DamageRequestStatusId == Enums.DamageRequestStatus.PayFromBranch.ToByte() 
                    || message.DamageRequestStatusId == Enums.DamageRequestStatus.PayFromCustomer.ToByte()
                    )
                && viewModel.StatusId == Enums.DamageRequestStatus.Open.ToByte())
            {
                message.ReviewerUserId = null;
            }

            message.DamageRequestStatusId = viewModel.StatusId;
            message.LastChangeStatusDate = DateTime.Now;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(long terminalId, CancellationToken cancellationToken)
        {
            
            
                var isOpenExist = _dataContext.DamageRequest.Any(b => b.TerminalId == terminalId &&
                                                                    (
                                                                        b.DamageRequestStatusId
                                                                        != (byte) Enums.DamageRequestStatus.PayFromBranch
                                                                        ||
                                                                        b.DamageRequestStatusId
                                                                        != (byte) Enums.DamageRequestStatus.PayFromCustomer
                                                                    )
                );
           if(isOpenExist)
               return RedirectToAction("CustomError", "Error",
                   new {message = "درخواست خسارت فعال برای این پایانه وجود دارد"});

            var query = _dataContext.Terminals
                .Where(x => x.Id == terminalId );


            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (!query.Any())
            {
                return RedirectToAction("NotFound", "Error");
            }


        

            var reasonList = await _dataContext.RevokeReasons
                .Select(x => new {x.Id, x.Title, x.Level})
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            if (query.FirstOrDefault().PspId == 3)
            {
                ViewBag.ReasonList = reasonList.Where(x => x.Level == 1).ToSelectList(x => x.Id, x => x.Title);
            }
            else
                ViewBag.ReasonList = reasonList.Where(x => x.Level == 1).ToSelectList(x => x.Id, x => x.Title);

            ViewBag.SecondReasonList = reasonList.Where(x => x.Level == 2).ToSelectList(x => x.Id, x => x.Title);

            var tupleList = new List<(int, string)>
            {
               
                (1, "شماره پایانه"),
                (2, " کد پیگیری  "),
              
            };


            ViewBag.TypeList = tupleList
                .ToSelectList(x => x.Item1, x => x.Item2
              , selectedValue : new[] {1});
            ViewBag.BranchList = _dataContext.OrganizationUnits.Where(b => b.Id >= 1000).ToList()
                .Select(b => new {b.Id,Title = b.Id + " " +  b.Title}).ToList().ToSelectList(x => x.Id, x => x.Title);

            var subjectListt = new List<string>();

            var ViewModel = new DamageRequestViewModel();
            ViewModel.TerminalId = query.FirstOrDefault().Id;
            subjectListt.Add("نارضایتی از شرکت فن آوا کارت");
            subjectListt.Add(" نارضایتی از شرکت ایران کیش");
            subjectListt.Add("نارضایتی از شرکت تجارت الکترونیک پارسیان ");
            subjectListt.Add(" پیگیری نصب");
            subjectListt.Add(" پیگیری جمع آوری");
            subjectListt.Add(" پیگیری مغایرت حساب");
            subjectListt.Add(" پیگیری تغییر حساب ");
            subjectListt.Add("سایر  ");
            ViewBag.SubjectList = subjectListt
                .ToSelectList(x => x, x => x);
            ViewModel.ExtraDataTopic = 2;
            ViewModel.ExtraDataTopicValue = terminalId.ToString();
            ViewModel.OrganizationUnitId = (int)query.FirstOrDefault().BranchId;
            return PartialView("_Create",ViewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(DamageRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.PostedFiles.Any(x => x != null) &&
                viewModel.PostedFiles.Any(x =>
                    !x.IsValidFile() || !x.IsValidFormat(".jpg,.jpeg,.pdf,.docx,.png,.xls,.xlsx") ||
                    x.ContentLength > 1 * 1024 * 1024))
            {
                return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
            }

            if (viewModel.PostedFiles.All(x => x == null))
            {
                return JsonErrorMessage("انتخاب فایل ضروری می باشد");
            }

            var damageRequest = new DamageRequest()
            {
                Body = viewModel.Body, 
                TerminalId = viewModel.TerminalId,
                FullName = viewModel.FullName,
                UserId = CurrentUserId,
                Subject = viewModel.Subject,
                CreationDate = DateTime.Now,
                SerialNumber = viewModel.SerialNumber,
                ExtraDataTopicValue = viewModel.ExtraDataTopicValue,
                ExtraDataTopic = viewModel.ExtraDataTopic,
                DamageValue = viewModel.DamageValue,
                OrganizationUnitId = viewModel.OrganizationUnitId,
                DamageRequestStatusId = Enums.DamageRequestStatus.Open.ToByte()
            };

            if (viewModel.PostedFiles.Any())
            {
                foreach (var item in viewModel.PostedFiles.Where(x => x != null && x.IsValidFile()))
                {
                    damageRequest.DamageRequestDocuments.Add(new DamageRequestDocument
                        {FileData = item.ToByteArray(), FileName = item.FileName});
                }
            }

            _dataContext.DamageRequest.Add(damageRequest);
            await _dataContext.SaveChangesAsync(cancellationToken);
            damageRequest.GUID = "TES-" + DateTime.Now.Year.ToString().Substring(2, 2) +
                                 DateTime.Now.GetPersianMonthInt() + damageRequest.Id;
            await _dataContext.SaveChangesAsync(cancellationToken);
            AddInformationMessage($"درخواست شما با شماره پیگیری {damageRequest.GUID} در سیستم ثبت شد");
            return JsonSuccessResult();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult UploadDamageRequest()
        {
            var k = _dataContext._damage.ToList();
            foreach (var VARIABLE in k)
            {
                var damageRequest = new DamageRequest();

                damageRequest.Body = VARIABLE.body;
                  damageRequest.TerminalId = long.Parse(VARIABLE.terminalId);
                  damageRequest.FullName ="بارگذاری";
                  damageRequest.UserId = CurrentUserId;
                  damageRequest.Subject = VARIABLE.title;
                  damageRequest.CreationDate = DateTime.Now;
                  damageRequest.SerialNumber = "";
                  damageRequest.ExtraDataTopicValue = "";// viewModel.ExtraDataTopicValue,
                  damageRequest.ExtraDataTopic = null;// viewModel.ExtraDataTopic,
                  damageRequest.DamageValue = int.Parse(VARIABLE.amount);
                  damageRequest.OrganizationUnitId = long.Parse(VARIABLE.branch);
                  damageRequest.DamageRequestStatusId = Enums.DamageRequestStatus.Open.ToByte();
                   
                
               
                _dataContext.DamageRequest.Add(damageRequest);
                damageRequest.GUID = "TES-" + DateTime.Now.Year.ToString().Substring(2, 2) +
                                     DateTime.Now.GetPersianMonthInt() + damageRequest.Id;
                _dataContext.SaveChanges();

              
            }
            return null;
        }

       

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long revokeRequestId, CancellationToken cancellationToken)
        {
            var query = _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId);

            if (User.IsAcceptorsExpertUser())
            {
                query = query.Where(x =>
                    x.StatusId == (byte) Enums.RequestStatus.NeedToReform ||
                    x.StatusId == (byte) Enums.RequestStatus.Registered ||
                    x.StatusId == (byte) Enums.RequestStatus.WebServiceError);
            }

            var revokeRequest = await query.FirstOrDefaultAsync(cancellationToken);

            if (revokeRequest == null)
            {
                return JsonErrorMessage(
                    "درخواست مورد نظر یافت نشد. کارشناس پذیرندگان تنها درخواست هایی با وضعیت ثبت شده، نیازمند اصلاح و خطای وب سرویس را می تواند حذف کند.");
            }

            _dataContext.RevokeRequests.Remove(revokeRequest);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Send(long revokeRequestId, CancellationToken cancellationToken)
        {
            var revokeRequest =
                await _dataContext.RevokeRequests.FirstAsync(x => x.Id == revokeRequestId, cancellationToken);

            if (revokeRequest.StatusId != (byte) Enums.RequestStatus.WebServiceError)
            {
                return JsonErrorMessage(
                    "تنها درخواست های جمع آوری ای که وضعیتشان 'خطای وب سرویس' است امکان ارسال مجدد دارند.");
            }

            return await SendRevokeRequest(revokeRequest.Id, revokeRequest.TerminalNo, revokeRequest.ReasonId,
                cancellationToken);
        }
        
       
       [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Download(TicketIndexViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.DamageRequest.Include(b => b.User.OrganizationUnit).AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.searchGuid))
            {
                query = query.Where(x => x.GUID.Contains(viewModel.searchGuid.ToString()));
            }
            if (viewModel.BranchId.HasValue)
            {
                query = query.Where(x => x.OrganizationUnitId == viewModel.BranchId);
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
                query = query.Where(x => x.DamageRequestStatusId == viewModel.StatusId);
            }

            // فقط تیکت هایی که هیچ کسی در حال بررسیش نیست
            if (viewModel.JustNotReviewingMessages)
            {
                query = query.Where(x => !x.ReviewerUserId.HasValue);
            }


            if ( User.IsBranchUser() )
            {
                 
                query = query.Where(x => x.OrganizationUnitId   ==  CurrentUserBranchId);
                
            }



            var data = query.ToList()
                .Select(x => new
                {
                    x.Id,
                    x.Subject,
                    x.DamageRequestStatusId,
                    x.GUID,
                 
                    x.FileNameFinalFile,
                    x.Body,x.DamageValue,
x.TerminalId,
                    EnteredFullName = x.FullName,

                    OrganizationId = x.OrganizationUnitId,
                    x.OrganizationUnit.Title,
                    UserFullName = x.User.FullName,
                    StatusTitle = x.DamageRequestStatus.Title,
                    ReviewerFullName = x.ReviewerUser?.FullName,
                    ReviewerUserFullName = x.ReviewerUser?.FullName,
                    CreationDate = x.CreationDate,

                    LastReplyCreationDate = x.Replies.OrderByDescending(y => y.CreationDate).Select(y =>
                        y.CreationDate).FirstOrDefault()
                }).ToList();
 
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

                worksheet.Cells[1, 1].Value = "وضعیت درخواست";
                worksheet.Cells[1, 2].Value = "شماره پیگیری";
                worksheet.Cells[1, 3].Value = "موضوع";
                worksheet.Cells[1, 4].Value = " مبلغ  ";
                worksheet.Cells[1, 5].Value = " شعبه ارجاع گیرنده ";
                worksheet.Cells[1, 6].Value = " نام و نام خانوادگی ارجاع دهنده  ";
                worksheet.Cells[1, 7].Value =  "تاریخ ثبت  ";
                worksheet.Cells[1, 8].Value = "تاریخ آخرین اقدام";
                worksheet.Cells[1, 9].Value ="متن پیام";
                worksheet.Cells[1, 10].Value = "کد پیگیری درخواست";

                var rowNumber = 2;
                foreach (var item in data)
                {
                    try
                    {
                        worksheet.Cells[rowNumber, 1].Value = item.StatusTitle;
                        worksheet.Cells[rowNumber, 2].Value = item.TerminalId;
                        worksheet.Cells[rowNumber, 3].Value = item.Subject;
                        worksheet.Cells[rowNumber, 4].Value = item.DamageValue;
                        worksheet.Cells[rowNumber, 5].Value = item.Title;
                        worksheet.Cells[rowNumber, 6].Value = item.EnteredFullName;
                        worksheet.Cells[rowNumber, 7].Value =item.CreationDate.ToPersianDateTime();
                        worksheet.Cells[rowNumber, 8].Value = item.LastReplyCreationDate.ToPersianDateTime();
                        worksheet.Cells[rowNumber, 9].Value = item.Body;
                        worksheet.Cells[rowNumber, 10].Value = item.GUID;

 


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
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DamageRequest.xlsx");
                }
            }
        }
        
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> ReSend(long revokeRequestId, CancellationToken cancellationToken)
        {
            var revokeRequest =
                await _dataContext.RevokeRequests.FirstAsync(x => x.Id == revokeRequestId, cancellationToken);

            if (revokeRequest.StatusId != (byte) Enums.RequestStatus.WebServiceError)
            {
                return JsonErrorMessage(
                    "تنها درخواست های جمع آوری ای که وضعیتشان 'خطای وب سرویس' است امکان ارسال مجدد دارند.");
            }

            return await ReSendRevokeRequest(revokeRequest.Id, revokeRequest.TerminalNo, revokeRequest.ReasonId,
                cancellationToken);
        }


        private async Task<ActionResult> ReSendRevokeRequest(long revokeRequestId, string terminalNo, byte reasonId,
            CancellationToken cancellationToken)
        {
            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo && x.StatusId != (byte) Enums.TerminalStatus.Revoked)
                .Select(x => new {x.PspId, x.ContractNo, x.TerminalNo, x.Id, x.FollowupCode})
                .FirstOrDefaultAsync(cancellationToken);

            var reasonTitle = await _dataContext.RevokeReasons
                .Where(x => x.Id == reasonId)
                .Select(x => new {x.Title,x.Id})
                .FirstAsync(cancellationToken);

            var result = new SendRevokeRequestResponseModel();

            switch (terminalInfo.PspId)
            {
                case (byte) Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = await fanavaService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo,
                            reasonTitle.Title);
                    break;
                }
                //TODO PN
                case (byte) Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                        var  result3 =   pardakhtNovinService.SendRevokeRequest
                            (revokeRequestId, terminalInfo.TerminalNo , terminalInfo.FollowupCode.ToString(), reasonTitle.Title,reasonTitle.Id,terminalInfo.Id).Result;
                        await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x =>
                            new RevokeRequest { StatusId =
                                result3.Status == PardakthNovinStatus.Successed && 
                                result3.SavedID != 0 ? (byte)2 : (byte)7 , Result = result3.StatusTitle });
                        return result3.Status == PardakthNovinStatus.Successed && 
                               result3.SavedID != 0
                            ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
                    }
                    break;
                }
                case (byte) Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = await irankishService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo,
                            reasonTitle.Title, terminalInfo.Id);
                    break;
                }

                case (byte) Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                        result = await parsianService.NewSendRevokeRequest(reasonId, terminalInfo.TerminalNo
                            , (int) terminalInfo.Id, (int) revokeRequestId);
                    break;
                }
            }

            await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x => new RevokeRequest
                {StatusId = result.StatusId, Result = result.Result});

            return result.IsSuccess ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
        }

        private async Task<ActionResult> SendRevokeRequest(long revokeRequestId, string terminalNo, byte reasonId,
            CancellationToken cancellationToken)
        {
            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo && x.StatusId == (byte) Enums.TerminalStatus.Installed)
                .Select(x => new {x.PspId, x.ContractNo, x.TerminalNo, x.Id , x.FollowupCode})
                .FirstOrDefaultAsync(cancellationToken);

            var reasonTitle = await _dataContext.RevokeReasons
                .Where(x => x.Id == reasonId)
                .Select(x =>new { x.Title,x.Id})
                .FirstAsync(cancellationToken);

            var result = new SendRevokeRequestResponseModel();

            switch (terminalInfo.PspId)
            {
                case (byte) Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = await fanavaService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo,
                            reasonTitle.Title);
                    break;
                }
                case (byte) Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                        var  result3 =   pardakhtNovinService.SendRevokeRequest
                            (revokeRequestId, terminalInfo.TerminalNo , terminalInfo.FollowupCode.ToString(), reasonTitle.Title,reasonTitle.Id,terminalInfo.Id).Result;
                        await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x =>
                            new RevokeRequest { StatusId =
                                result3.Status == PardakthNovinStatus.Successed && 
                                result3.SavedID != 0 ? (byte)2 : (byte)7 , Result = result3.StatusTitle });
                        return result3.Status == PardakthNovinStatus.Successed && 
                               result3.SavedID != 0
                            ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
                    }
                    break;
                }

                case (byte) Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = await irankishService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo,
                            reasonTitle.Title , terminalInfo.Id);
                    break;
                }

                case (byte) Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                        result = await parsianService.NewSendRevokeRequest(reasonId, terminalInfo.TerminalNo
                            , (int) terminalInfo.Id, (int) revokeRequestId);
                    break;
                }
            }

            await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x => new RevokeRequest
                {StatusId = result.StatusId, Result = result.Result});

            return result.IsSuccess ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
        }
    }
}