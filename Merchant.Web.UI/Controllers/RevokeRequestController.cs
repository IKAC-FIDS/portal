using EntityFramework.Extensions;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Persia;
using Stimulsoft.Report;
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
    public class RevokeRequestController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public RevokeRequestController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Manage(CancellationToken cancellationToken)
        {
            ViewBag.StatusList = (await _dataContext.RequestStatus
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                .Where(x => x.ParentId.HasValue)
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
            var message = _dataContext.Messages.ToList();
            ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
                                                    && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                        || User.IsMessageManagerUser()));
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator, DefaultRoles.ITUser, DefaultRoles.BranchUser)]
        public ActionResult GetRevokeRequestData(RequestSearchParameters viewModel)
        {
            viewModel.IsBranchUser = User.IsBranchUser();
            viewModel.CurrentUserBranchId = CurrentUserBranchId;
            viewModel.IsSupervisionUser = User.IsSupervisionUser();
            viewModel.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            viewModel.IsCountyBranchManagment = User.IsCountyBranchManagementUser();

            var data = _dataContext.GetRevokeRequestData(viewModel, viewModel.RetriveTotalPageCount, viewModel.Page - 1, 300, out var totalRowsCount);

            return JsonSuccessResult(new { rows = data, totalRowsCount });
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(string terminalNo, CancellationToken cancellationToken)
        {
            var data = await _dataContext.RevokeRequests
                .Where(x => x.TerminalNo == terminalNo)
                .OrderByDescending(x => x.SubmitTime)
                .Select(x => new
                {
                    x.Id,
                    x.StatusId,
                    x.TerminalNo,
                    x.SubmitTime,
                    x.DeliveryDescription,
                    StatusTitle = x.Status.Title,
                    UserFullName = x.User.FullName,
                    ReasonTitle = x.Reason.Title + " - " + x.SecondReason.Title,
                })
                .ToListAsync(cancellationToken);

            var result = data
                .Select(x => new
                {
                    x.Id,
                    x.StatusId,
                    x.TerminalNo,
                    x.DeliveryDescription,
                    x.ReasonTitle,
                    x.StatusTitle,
                    x.UserFullName,
                    SubmitTime = x.SubmitTime.ToPersianDateTime()
                })
                .ToList();

            return JsonSuccessResult(result);
        }

               [HttpGet]
                [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
                public async Task<ActionResult> Edit(long id, CancellationToken cancellationToken)
                {

                    var data = _dataContext.RevokeRequests.FirstOrDefault(a => a.Id == id);

                    var viewModel = new RevokeRequestViewModel();
                    viewModel.ReasonId = data.ReasonId;
                    viewModel.SecondReasonId = data.SecondReasonId;
                    viewModel.DeliveryDescription = data.DeliveryDescription;
                    viewModel.Id = data.Id;
                    var reasonList = await _dataContext.RevokeReasons
                        .Select(x => new { x.Id, x.Title, x.Level })
                        .Where(arg => arg.Id !=3)
                        .OrderBy(x => x.Id)
                        .ToListAsync(cancellationToken);

          
                    ViewBag.ReasonList = reasonList.Where(x => x.Level == 1).ToSelectList(x => x.Id, x => x.Title);
           
                    ViewBag.SecondReasonList = reasonList.Where(x => x.Level == 2).ToSelectList(x => x.Id, x => x.Title);

                    
                    return View("_Edit", viewModel);
                }
        
                [HttpPost]
                [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
                public async Task<ActionResult> Edit(RevokeRequestViewModel viewModel, CancellationToken cancellationToken)
                {
                    var data = _dataContext.RevokeRequests.FirstOrDefault(a => a.Id == viewModel.Id);
                    data.ReasonId = viewModel.ReasonId;
                    data.SecondReasonId = viewModel.SecondReasonId;
                    data.DeliveryDescription = viewModel.DeliveryDescription;
                    
                    _dataContext.SaveChanges();
                    return JsonSuccessMessage();
                }


        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(string terminalNo, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(terminalNo.Trim()))
            {
                return RedirectToAction("CustomError", "Error", new { message = "پذیرنده مورد نظر فاقد شماره پایانه می‌باشد و امکان ثبت درخواست وجود ندارد." });
            }

            var query = _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed);

            
            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (!query.Any())
            {
                return RedirectToAction("NotFound", "Error");
            }
            
            

            ViewBag.TerminalNo = terminalNo;

            var reasonList = await _dataContext.RevokeReasons
                .Select(x => new { x.Id, x.Title, x.Level })
                .Where(arg => arg.Id !=3)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

          
                ViewBag.ReasonList = reasonList.Where(x => x.Level == 1).ToSelectList(x => x.Id, x => x.Title);
           
            ViewBag.SecondReasonList = reasonList.Where(x => x.Level == 2).ToSelectList(x => x.Id, x => x.Title);

            return PartialView("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(RevokeRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (User.IsBranchUser() && !_dataContext.Terminals.Any(x => x.TerminalNo == viewModel.TerminalNo && x.BranchId == CurrentUserBranchId))
            {
                return JsonErrorMessage("شما امکان ثبت درخواست جمع آوری برای این پایانه را ندارید.");
            }

            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == viewModel.TerminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed)
                .Select(x => new { x.TerminalNo, x.PspId, x.ContractNo })
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(terminalInfo?.TerminalNo))
            {
                return JsonErrorMessage("امکان ثبت درخواست جمع آوری برای پایانه هایی که وضعیت آن ها 'ورود بازاریابی'، 'نیازمند اصلاح'، 'جمع آوری شده' است نمی باشد.");
            }
            var dt = DateTime.Now;
         
            var m = dt.ToPersianMonth();
            var y = dt.ToPersianYear();
            //todo ===> bishtar az 'n' ta darkhast dar mah
            PersianCalendar pc = new PersianCalendar();
            DateTime thisDate = DateTime.Now;
           var start =  $"{pc.GetYear(thisDate)}/{pc.GetMonth(thisDate)}/01";
           var end =  $"{pc.GetYear(thisDate)}/{pc.GetMonth(thisDate)}/{(pc.GetMonth(thisDate) > 6 ? 30 : 31)}";
           var list = _dataContext.RevokeRequests.Where(e=>e.StatusId != (byte)Enums.RequestStatus.Rejected ).ToList();
           var t = list.Count(b => b.JalaliYearSubmitTIme == pc.GetYear(thisDate) && 
                                   b.JalaliMonthSubmitTIme == pc.GetMonth(thisDate));

           if (t >= 1000)
           {
               return JsonErrorMessage("امکان ثبت بیشتر از 1000 درخواست جمع اوری در ماه فراهم نمی باشد");

           }
            //============>
            var revokeRequest = new RevokeRequest
            {
                UserId = CurrentUserId,
                SubmitTime = DateTime.Now,
                ReasonId = viewModel.ReasonId,
                TerminalNo = viewModel.TerminalNo,
                SecondReasonId = viewModel.SecondReasonId,
                DeliveryDescription = viewModel.DeliveryDescription,
                StatusId = (byte)Enums.RequestStatus.Registered
            };

            _dataContext.RevokeRequests.Add(revokeRequest);
            await _dataContext.SaveChangesAsync(cancellationToken);

         
            var nowYear = m == 1 ?  (y  - 1) : (y);
            var noMount = m == 1 ?  12 :(m - 1);

            var naroPsp = _dataContext.TransactionSums.Where(b => b.TerminalNo == viewModel.TerminalNo &&
                                                                  b.PersianLocalMonth == noMount && b.PersianLocalYear == nowYear).FirstOrDefault();
            if (naroPsp != null)
            {
                if (naroPsp.TotalCount > 200)
                {
                    var data =  _dataContext.RevokeRequests.FirstOrDefault(b => b.Id == revokeRequest.Id);
                    if (data != null) 
                        data.StatusId = (byte) Enums.RequestStatus.NeedToEdit;
                    return JsonSuccessMessage();
                }
            }
            return await SendRevokeRequest(revokeRequest.Id, revokeRequest.TerminalNo, revokeRequest.ReasonId, cancellationToken);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangeStatus(RevokeRequestChangeStatusViewModel viewModel, CancellationToken cancellationToken)
        {
            var revokeRequest = await _dataContext.RevokeRequests.FirstAsync(x => x.Id == viewModel.RevokeRequestId, cancellationToken);

            if (viewModel.Approve)
            {
                var terminal = await _dataContext.Terminals
                    .Where(x => x.TerminalNo == revokeRequest.TerminalNo)
                    .FirstOrDefaultAsync(cancellationToken);

                if (terminal != null)
                {
                    terminal.LastUpdateTime = DateTime.Now;
                    terminal.StatusId = Enums.TerminalStatus.Revoked.ToByte();
                }
            }

            revokeRequest.StatusId = viewModel.Approve ? (byte)Enums.RequestStatus.Done : (byte)Enums.RequestStatus.Rejected;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long revokeRequestId, CancellationToken cancellationToken)
        {
            var query = _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId);

            if (User.IsAcceptorsExpertUser())
            {
                query = query.Where(x => x.StatusId == (byte)Enums.RequestStatus.NeedToReform || x.StatusId == (byte)Enums.RequestStatus.Registered || x.StatusId == (byte)Enums.RequestStatus.WebServiceError);
            }

            var revokeRequest = await query.FirstOrDefaultAsync(cancellationToken);

            if (revokeRequest == null)
            {
                return JsonErrorMessage("درخواست مورد نظر یافت نشد. کارشناس پذیرندگان تنها درخواست هایی با وضعیت ثبت شده، نیازمند اصلاح و خطای وب سرویس را می تواند حذف کند.");
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
            var revokeRequest = await _dataContext.RevokeRequests.FirstAsync(x => x.Id == revokeRequestId, cancellationToken);

            if (revokeRequest.StatusId != (byte)Enums.RequestStatus.WebServiceError)
            {
                return JsonErrorMessage("تنها درخواست های جمع آوری ای که وضعیتشان 'خطای وب سرویس' است امکان ارسال مجدد دارند.");
            }

            return await SendRevokeRequest(revokeRequest.Id, revokeRequest.TerminalNo, revokeRequest.ReasonId, cancellationToken);
        }


        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> ReSend(long revokeRequestId, CancellationToken cancellationToken)
        {
            var revokeRequest = await _dataContext.RevokeRequests.FirstAsync(x => x.Id == revokeRequestId, cancellationToken);

           // if (revokeRequest.StatusId != (byte)Enums.RequestStatus.WebServiceError)
           if (revokeRequest.StatusId == (byte)Enums.RequestStatus.Done)

            {
                return JsonErrorMessage("درخواست های انجام شده امکان ارسال مجدد ندارند.");
            }

            return await ReSendRevokeRequest(revokeRequest.Id, revokeRequest.TerminalNo, revokeRequest.ReasonId, cancellationToken);
        }

        
          private async Task<ActionResult> ReSendRevokeRequest(long revokeRequestId, string terminalNo, byte reasonId, CancellationToken cancellationToken)
        {
            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo  && x.StatusId != (byte)Enums.TerminalStatus.Revoked)
                .Select(x => new { x.PspId, x.ContractNo, x.TerminalNo ,x.Id , x.FollowupCode})
                .FirstOrDefaultAsync(cancellationToken);

            var reasonTitle = await _dataContext.RevokeReasons
                .Where(x => x.Id == reasonId)
                .Select(x =>new { x.Title,x.Id})
                .FirstAsync(cancellationToken);

            var result = new SendRevokeRequestResponseModel();

            switch (terminalInfo.PspId)
            {
                case (byte)Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = await fanavaService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo, reasonTitle.Title);
                    break;
                }

                case (byte)Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = await irankishService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo, reasonTitle.Title,terminalInfo.Id);
                    break;
                }
                case (byte)Enums.PspCompany.PardakhNovin:
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
                case (byte)Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                        result = await parsianService.NewSendRevokeRequest(reasonId, terminalInfo.TerminalNo
                            ,(int)terminalInfo.Id,(int)revokeRequestId );
                    break;
                }
            }

            await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x => new RevokeRequest { StatusId = result.StatusId, Result = result.Result });

            return result.IsSuccess ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
        }
          
        private async Task<ActionResult> SendRevokeRequest(long revokeRequestId, string terminalNo, byte reasonId, CancellationToken cancellationToken)
        {
            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed)
                .Select(x => new { x.PspId, x.ContractNo, x.TerminalNo ,x.Id, x.FollowupCode})
                .FirstOrDefaultAsync(cancellationToken);

            var reasonTitle = await _dataContext.RevokeReasons
                .Where(x => x.Id == reasonId)
                .Select(x => new {x.Title,x.Id})
                .FirstAsync(cancellationToken);

            var result = new SendRevokeRequestResponseModel();

            switch (terminalInfo.PspId)
            {
                case (byte)Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = await fanavaService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo, reasonTitle.Title);
                    break;
                }

                case (byte)Enums.PspCompany.IranKish:
                {
                    //todo => to newirankish
                    using (var irankishService = new NewIranKishService())
                        result = await irankishService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo, reasonTitle.Title,terminalInfo.Id);
                    
                    // using (var irankishService = new IranKishService())
                    //     result = await irankishService.SendRevokeRequest(revokeRequestId, terminalInfo.TerminalNo, reasonTitle.Title);

                    break;
                }
                case (byte)Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                        var  result3 =   pardakhtNovinService.SendRevokeRequest
                            (revokeRequestId, terminalInfo.TerminalNo , terminalInfo.FollowupCode.ToString(), reasonTitle.Title,reasonTitle.Id,terminalInfo.Id).Result;
                        await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x =>
                            new RevokeRequest { 
                                PardakhtNovinSaveId = result3.SavedID,
                                StatusId =
                                result3.Status == PardakthNovinStatus.Successed && 
                                result3.SavedID != 0 ? (byte)2 : (byte)7 , Result = result3.StatusTitle });
                        //todo
                    
                       var m =  _dataContext.Terminals.Where(x => x.TerminalNo == terminalInfo.TerminalNo).UpdateAsync(x =>
                            new Terminal { 
                                RevokreRequestSavedId = result3.SavedID }).Result;
                        return result3.Status == PardakthNovinStatus.Successed && 
                               result3.SavedID != 0
                            ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
                    }
                    break;
                }

                case (byte)Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                        result = await parsianService.NewSendRevokeRequest(reasonId, terminalInfo.TerminalNo
                            ,(int)terminalInfo.Id,(int)revokeRequestId );
                    break;
                }
            }

            await _dataContext.RevokeRequests.Where(x => x.Id == revokeRequestId).UpdateAsync(x => new RevokeRequest { StatusId = result.StatusId, Result = result.Result });

            return result.IsSuccess ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
        }
    }
}