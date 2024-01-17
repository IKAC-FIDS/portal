using EntityFramework.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class ChangeAccountRequestController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public ChangeAccountRequestController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator,  DefaultRoles.BranchUser, DefaultRoles.SupervisionUser, DefaultRoles.BranchManagment, DefaultRoles.TehranBranchManagement, DefaultRoles.CountyBranchManagement)]
        public async Task<ActionResult> Manage(CancellationToken cancellationToken)
        {
            ViewBag.StatusList = (await _dataContext.RequestStatus
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                .Where(x => x.ParentId.HasValue)
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
           //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                              || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Enums.MessageStatus.UnderReview   
           //                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                    || User.IsMessageManagerUser()));
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
        [CustomAuthorize(DefaultRoles.BranchUser , DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult GetChangeAccountRequestData(RequestSearchParameters viewModel)
        {
            viewModel.IsBranchUser = User.IsBranchUser();
            viewModel.CurrentUserBranchId = CurrentUserBranchId;
            viewModel.IsSupervisionUser = User.IsSupervisionUser();
            viewModel.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            viewModel.IsCountyBranchManagment = User.IsCountyBranchManagementUser();

            var data = _dataContext.GetChangeAccountRequestData(viewModel, viewModel.RetriveTotalPageCount, viewModel.Page - 1, 300, out var totalRowsCount);

            return JsonSuccessResult(new { rows = data, totalRowsCount });
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(string terminalNo, CancellationToken cancellationToken)
        {
            var query = _dataContext.ChangeAccountRequests
                .Where(x => x.TerminalNo == terminalNo)
                .AsQueryable();

            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            }

            var data = await query
                .OrderByDescending(x => x.SubmitTime)
                .Select(x => new
                {
                    x.Id,
                    x.Result,
                    x.StatusId,
                    x.AccountNo,
                    x.SubmitTime,
                    x.CurrentAccountNo,
                    StatusTitle = x.Status.Title,
                    BranchTitle = x.Branch.Title,
                    UserFullName = x.User.FullName,
                    ParentBranchTitle = x.Branch.Parent.Title
                })
                .ToListAsync(cancellationToken);

            var result = data
                .Select(x => new
                {
                    x.Id,
                    x.Result,
                    x.StatusId,
                    x.AccountNo,
                    x.BranchTitle,
                    x.StatusTitle,
                    x.UserFullName,
                    x.CurrentAccountNo,
                    x.ParentBranchTitle,
                    SubmitTime = x.SubmitTime.ToPersianDateTime()
                })
                .ToList();

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(string terminalNo)
        {
            if (string.IsNullOrEmpty(terminalNo.Trim()))
                return RedirectToAction("CustomError", "Error", new { message = "پذیرنده مورد نظر فاقد شماره پایانه می‌باشد و امکان ثبت درخواست وجود ندارد." });

            var previousChangeAccountRequests = _dataContext.ChangeAccountRequests.Where(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.RequestStatus.SentToPsp || x.StatusId == (byte)Enums.RequestStatus.WebServiceError || x.StatusId == (byte)Enums.RequestStatus.NeedToReform);

            if (previousChangeAccountRequests.Any(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.RequestStatus.SentToPsp))
                return RedirectToAction("CustomError", "Error", new { message = "به علت وجود یک درخواست تایید نشده امکان ثبت درخواست جدید وجود ندارد." });

            if (previousChangeAccountRequests.Any(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.RequestStatus.WebServiceError))
                return RedirectToAction("CustomError", "Error", new { message = "به علت وجود یک درخواست با وضعیت خطای وب سرویس، امکان ثبت درخواست جدید وجود ندارد. لطفاً با استفاده از گزینه ارسال مجدد نسبت به ارسال مجدد درخواست اقدام نمایید." });

            if (previousChangeAccountRequests.Any(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.RequestStatus.NeedToReform))
                return RedirectToAction("CustomError", "Error", new { message = "به علت وجود یک درخواست با وضعیت نیازمند اصلاح، امکان ثبت درخواست جدید وجود ندارد. لطفاً درخواست قبلی را ویرایش نمایید." });

            var query = _dataContext.Terminals
                .Where(x => x.TerminalNo == terminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed);

            if (User.IsBranchUser())
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            
          
            // if(CurrentUserId == 400  || CurrentUserId == 403 ||   CurrentUserId == 393)
            //     ViewBag.Disabled = false;
            // else
            // {
            //     ViewBag.Disabled = true;
            // }
            var terminalInfo = await query
                .Select(x => new
                {
                    x.Id,
                    x.PspId,
                    x.AccountNo,
                    BranchTitle = x.Branch.Title
                })
                .FirstOrDefaultAsync();

            if (terminalInfo == null)
            {
                return RedirectToAction("NotFound", "Error");
            }

            var accountBranchCode = terminalInfo.AccountNo.Split('-')[0] == "4402" ? "4102" : terminalInfo.AccountNo.Split('-')[0];

            var viewModel = new ChangeAccountRequestViewModel
            {
                TerminalNo = terminalNo,
                PspId = terminalInfo.PspId,
                CurrentAccountNo = terminalInfo.AccountNo,
                CurrentBranchTitle = terminalInfo.BranchTitle,
                AccountBranchCode = accountBranchCode,
                AccountCustomerNumber = terminalInfo.AccountNo.Split('-')[2]
            };

            return PartialView("_Create", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(ChangeAccountRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(viewModel.TerminalNo) || _dataContext.ChangeAccountRequests.Any(x => x.TerminalNo == viewModel.TerminalNo && (x.StatusId == (byte)Enums.RequestStatus.NeedToReform || x.StatusId == (byte)Enums.RequestStatus.WebServiceError || x.StatusId == (byte)Enums.RequestStatus.SentToPsp || x.StatusId == (byte)Enums.RequestStatus.Registered)))
                return JsonWarningMessage("به علت وجود حداقل یک درخواست با وضعیت ارسال شده به PSP، نیازمند اصلاح یا خطای وب سرویس برای این پایانه امکان ثبت درخواست جدید وجود ندارد.");

            if (User.IsBranchUser() && !_dataContext.Terminals.Any(x => x.TerminalNo == viewModel.TerminalNo && x.BranchId == CurrentUserBranchId))
                return JsonErrorMessage("شما امکان ثبت درخواست تغییر حساب برای این پایانه را ندارید.");

            if (viewModel.PspId == (byte)Enums.PspCompany.IranKish && !viewModel.PostedFile.IsValidFormat(".pdf"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .pdf باشد.");

            if (viewModel.PspId == (byte)Enums.PspCompany.PardakhNovin && !viewModel.PostedFile.IsValidFormat(".pdf"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .pdf باشد.");
            
            if (viewModel.PspId == (byte)Enums.PspCompany.Parsian && !viewModel.PostedFile.IsValidFormat(".jpg,.jpeg"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .jpg یا .jpeg باشد.");

            if (viewModel.PostedFile.IsValidFile() && viewModel.PostedFile.ContentLength > 1 * 1024 * 1024)
                return JsonErrorMessage("حجم فایل ارسالی بایستی کمتر از یک مگابایت باشد.");

          
            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == viewModel.TerminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed)
                .Select(x => new
                {
                    x.PspId,
                    x.ShebaNo,
                    x.AccountNo,
                    x.TerminalNo,
                    x.ContractNo,
                    x.MerchantNo,
                    x.MerchantProfile.LastName,
                    x.MerchantProfile.FirstName,
                    x.MerchantProfile.NationalCode,
                    x.MerchantProfile.LegalNationalCode,
                    x.MerchantProfile.IsLegalPersonality,
                    x.TaxPayerCode,
                    x.Id,
                    AcceptorCode = x.MerchantNo
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (terminalInfo == null || string.IsNullOrEmpty(terminalInfo.AccountNo))
            {
                return JsonErrorMessage("امکان ثبت درخواست برای این پایانه وجود ندارد.");
            }

            var splittedPreviousAccountNumber = terminalInfo.AccountNo.Split('-');
            var newAccountBranchCode = viewModel.AccountBranchCode == "4402" ? "4102" :viewModel.AccountBranchCode;
            AccountNumberExtensions.GenerateAccountNumber(newAccountBranchCode, viewModel.AccountType, splittedPreviousAccountNumber[2], viewModel.AccountRow, out var accountNumberWithDash, out var accountNumberWithoutDash);

            if (!AccountNumberExtensions.TryGenerateShebaNumber(accountNumberWithoutDash, out var shebaNo))
                return JsonWarningMessage("شماره حساب وارد شده صحیح نمی باشد. لطفاً شماره حساب وارد شده را چک نموده و مجدداً تلاش نمایید.");

            if (!terminalInfo.PspId.HasValue)
                return JsonWarningMessage("شرکت Psp برای پذیرنده انتخاب نشده است و امکان ثبت درخواست وجود ندارد.");

            if (terminalInfo.AccountNo == accountNumberWithDash)
                return JsonWarningMessage("شماره حساب درخواستی شما با شماره حساب فعلی پذیرنده یکسان است.");

            var branchCode = Convert.ToInt64(viewModel.AccountBranchCode);
            var branchId = User.IsBranchUser() ? User.Identity.GetBranchId() : await _dataContext.OrganizationUnits.Where(x => x.Id == branchCode).Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

            if (!branchId.HasValue)
            {
                return JsonWarningMessage("کد شعبه یافت نشد.");
            }

            var changeAccountRequest = new ChangeAccountRequest
            {
                ShebaNo = shebaNo,
                UserId = CurrentUserId,
                BranchId = branchId.Value,
                SubmitTime = DateTime.Now,
                TerminalNo = viewModel.TerminalNo,
                AccountNo = accountNumberWithDash,
                CurrentAccountNo = terminalInfo.AccountNo,
                FileData = viewModel.PostedFile.ToByteArray(),
                StatusId = (byte)Enums.RequestStatus.Registered
            };

            _dataContext.ChangeAccountRequests.Add(changeAccountRequest);
            var d = _dataContext.SaveChangesAsync(cancellationToken).Result;

            var s = SendChangeAccountRequest(terminalInfo.PspId.Value,
                changeAccountRequest.Id,
                terminalInfo.AccountNo,
                changeAccountRequest.AccountNo,
                terminalInfo.ContractNo,
                terminalInfo.ShebaNo,
                changeAccountRequest.ShebaNo,
                terminalInfo.FirstName,
                terminalInfo.LastName,
                terminalInfo.TerminalNo,
                terminalInfo.MerchantNo,
                changeAccountRequest.BranchId,
                terminalInfo.NationalCode,
                changeAccountRequest.FileData,
                terminalInfo.IsLegalPersonality,
                terminalInfo.LegalNationalCode,
                terminalInfo.TaxPayerCode,
                terminalInfo.AcceptorCode,
                (int) terminalInfo.Id
            ).Result;
            return  s;
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(long changeAccountRequestId, CancellationToken cancellationToken)
        {
            var changeAccountRequestQuery = _dataContext.ChangeAccountRequests.Where(x => x.Id == changeAccountRequestId && x.StatusId == (byte)Enums.RequestStatus.NeedToReform);

            if (User.IsBranchUser())
                changeAccountRequestQuery = changeAccountRequestQuery.Where(x => x.BranchId == CurrentUserBranchId);

            if (User.IsSupervisionUser())
                changeAccountRequestQuery = changeAccountRequestQuery.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);

            var changeAccountRequest = await changeAccountRequestQuery.FirstOrDefaultAsync(cancellationToken);

            if (changeAccountRequest == null)
                return RedirectToAction("NotFound", "Error");

            var query = _dataContext.Terminals
                .Where(x => x.TerminalNo == changeAccountRequest.TerminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed);

            if (User.IsBranchUser())
                query = query.Where(x => x.BranchId == CurrentUserBranchId);

            var terminalInfo = await query
                .Select(x => new
                {
                    x.Id,
                    x.PspId,
                    x.AccountNo,
                    BranchTitle = x.Branch.Title
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (terminalInfo == null)
                return RedirectToAction("CustomError", "Error", new { message = "امکان ثبت درخواست برای این پذیرنده وجود ندارد." });

            var viewModel = new ChangeAccountRequestViewModel
            {
                PspId = terminalInfo.PspId,
                Id = changeAccountRequest.Id,
                CurrentAccountNo = terminalInfo.AccountNo,
                TerminalNo = changeAccountRequest.TerminalNo,
                CurrentBranchTitle = terminalInfo.BranchTitle,
                AccountBranchCode = terminalInfo.AccountNo.Split('-')[0],
                AccountType = changeAccountRequest.AccountNo.Split('-')[1],
                AccountCustomerNumber = terminalInfo.AccountNo.Split('-')[2],
                AccountRow = changeAccountRequest.AccountNo.Split('-')[3]
            };

            return PartialView("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(ChangeAccountRequestViewModel viewModel, CancellationToken cancellationToken)
        {
            if (User.IsBranchUser() && !_dataContext.Terminals.Any(x => x.TerminalNo == viewModel.TerminalNo && x.BranchId == CurrentUserBranchId))
                return JsonErrorMessage("شما امکان ثبت درخواست تغییر حساب برای این پایانه را ندارید.");

            if (viewModel.PspId == (byte)Enums.PspCompany.IranKish && !viewModel.PostedFile.IsValidFormat(".pdf"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .pdf باشد.");
            if (viewModel.PspId == (byte)Enums.PspCompany.PardakhNovin && !viewModel.PostedFile.IsValidFormat(".pdf"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .pdf باشد.");
            if (viewModel.PspId == (byte)Enums.PspCompany.Parsian && !viewModel.PostedFile.IsValidFormat(".jpg"))
                return JsonErrorMessage("فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .jpg یا .jpeg باشد.");

            if (viewModel.PostedFile.IsValidFile() && viewModel.PostedFile.ContentLength > 1 * 1024 * 1024)
                return JsonErrorMessage("حجم فایل ارسالی بایستی کمتر از یک مگابایت باشد.");

            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.TerminalNo == viewModel.TerminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed)
                .Select(x => new
                {
                    x.AccountNo,
                    x.TerminalNo,
                    x.ShebaNo,
                    x.ContractNo,
                    x.PspId,
                    x.MerchantProfile.FirstName,
                    x.MerchantProfile.LastName,
                    x.MerchantNo,
                    x.MerchantProfile.NationalCode,
                    x.MerchantProfile.IsLegalPersonality,
                    x.MerchantProfile.LegalNationalCode,
                    x.TaxPayerCode,
                    x.Id,
                    AcceptorCode = x.MerchantNo
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(terminalInfo?.AccountNo))
            {
                return JsonErrorMessage("امکان ثبت درخواست برای این پایانه وجود ندارد.");
            }

            var splittedPreviousAccountNumber = terminalInfo.AccountNo.Split('-');

            AccountNumberExtensions.GenerateAccountNumber(splittedPreviousAccountNumber[0], viewModel.AccountType, splittedPreviousAccountNumber[2], viewModel.AccountRow, out var accountNumberWithDash, out var accountNumberWithoutDash);

            if (!AccountNumberExtensions.TryGenerateShebaNumber(accountNumberWithoutDash, out var shebaNo))
            {
                return JsonWarningMessage("شماره حساب وارد شده صحیح نمی باشد. لطفاً شماره حساب وارد شده را چک نموده و مجدداً تلاش نمایید.");
            }

            if (!terminalInfo.PspId.HasValue)
            {
                return JsonWarningMessage("شرکت Psp برای پذیرنده انتخاب نشده است و امکان ثبت درخواست وجود ندارد.");
            }

            var changeAccountRequest = await _dataContext.ChangeAccountRequests.FirstOrDefaultAsync(x => x.Id == viewModel.Id, cancellationToken);

            if (changeAccountRequest.StatusId != (byte)Enums.RequestStatus.NeedToReform)
            {
                return JsonWarningMessage("تنها درخواست های با وضعیت نیازمند اصلاح قابل ویرایش هستند.");
            }

            if (await _dataContext.ChangeAccountRequests.AnyAsync(x => x.Id != changeAccountRequest.Id && x.TerminalNo == changeAccountRequest.TerminalNo && x.ShebaNo == changeAccountRequest.ShebaNo && x.StatusId == (byte)Enums.RequestStatus.SentToPsp, cancellationToken))
            {
                return JsonWarningMessage("یک درخواست تغییر حساب مشابه برای این پایانه ثبت شده است و امکان ثبت مجدد وجود ندارد.");
            }

            changeAccountRequest.ShebaNo = shebaNo;
            changeAccountRequest.UserId = CurrentUserId;
            changeAccountRequest.SubmitTime = DateTime.Now;
            changeAccountRequest.AccountNo = accountNumberWithDash;
            changeAccountRequest.TerminalNo = viewModel.TerminalNo;
            changeAccountRequest.CurrentAccountNo = terminalInfo.AccountNo;
            changeAccountRequest.FileData = viewModel.PostedFile.ToByteArray();

            await _dataContext.SaveChangesAsync(cancellationToken);

            return await SendChangeAccountRequest(terminalInfo.PspId.Value,
                changeAccountRequest.Id,
                terminalInfo.AccountNo,
                changeAccountRequest.AccountNo,
                terminalInfo.ContractNo,
                terminalInfo.ShebaNo,
                changeAccountRequest.ShebaNo,
                terminalInfo.FirstName,
                terminalInfo.LastName,
                terminalInfo.TerminalNo,
                terminalInfo.MerchantNo,
                changeAccountRequest.BranchId,
                terminalInfo.NationalCode,
                changeAccountRequest.FileData,
                terminalInfo.IsLegalPersonality,
                terminalInfo.LegalNationalCode,
                terminalInfo.TaxPayerCode,
                terminalInfo.AcceptorCode,
                (int)terminalInfo.Id
                );
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long changeAccountRequestId, CancellationToken cancellationToken)
        {
            var query = _dataContext.ChangeAccountRequests.Where(x => x.Id == changeAccountRequestId);

            ChangeAccountRequest changeAccountRequest = null;

            if (User.IsAdmin())
            {
                changeAccountRequest = await query.FirstOrDefaultAsync(cancellationToken);
            }
            else if (User.IsAcceptorsExpertUser())
            {
                changeAccountRequest = await query.FirstOrDefaultAsync(x => 
                    x.Id == changeAccountRequestId && 
                    x.StatusId == (byte)Enums.RequestStatus.NeedToReform || 
                    x.StatusId == (byte)Enums.RequestStatus.Registered || 
                    x.StatusId == (byte)Enums.RequestStatus.WebServiceError, cancellationToken);
            }

            if (changeAccountRequest == null)
                return JsonErrorMessage("درخواست مورد نظر یافت نشد. کارشناس پذیرندگان تنها درخواست هایی با وضعیت ثبت شده، نیازمند اصلاح و خطای وب سرویس را می تواند حذف کند.");

            _dataContext.ChangeAccountRequests.Remove(changeAccountRequest);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GetChangeAccountRequestDocument(long changeAccountRequestId, CancellationToken cancellationToken)
        {
            var query = _dataContext.ChangeAccountRequests.Where(x => x.Id == changeAccountRequestId);

            if (User.IsBranchUser())
                query = query.Where(x => x.BranchId == CurrentUserBranchId);

            if (User.IsSupervisionUser())
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);

            var data = await query.Select(x => new { x.TerminalNo, x.FileData }).FirstOrDefaultAsync(cancellationToken);

            if (data?.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            var pspId = await _dataContext.Terminals.Where(x => x.TerminalNo == data.TerminalNo).Select(x => x.PspId).FirstAsync(cancellationToken);

            var fileContentType = pspId == (byte)Enums.PspCompany.Parsian ? "image/jpg" : "application/pdf";
            var fileExtension = pspId == (byte)Enums.PspCompany.Parsian ? ".jpg" : ".pdf";

            var f =  File(data.FileData, fileContentType, $"{data.TerminalNo}{fileExtension}");
            
            return f;
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Send(long changeAccountRequestId, CancellationToken cancellationToken)
        {
            var changeAccountRequest = await _dataContext.ChangeAccountRequests
                .Select(x => new
                {
                    x.Id,
                    x.ShebaNo,
                    x.FileData,
                    x.BranchId,
                    x.StatusId,
                    x.AccountNo,
                    x.TerminalNo
                })
                .FirstAsync(x => x.Id == changeAccountRequestId, cancellationToken);

            var validStatusIdList = 
                new List<byte>
                {
                    (byte)Enums.RequestStatus.WebServiceError, (byte)Enums.RequestStatus.Registered,
                    (byte)Enums.RequestStatus.SentToPsp,
                    (byte)Enums.RequestStatus.NeedToReform
                };
            if (!validStatusIdList.Contains(changeAccountRequest.StatusId))
            {
                return JsonErrorMessage("تنها درخواست های تغییر حسابی که وضعیتشان 'خطای وب سرویس' یا 'ثبت شده' است امکان ارسال مجدد دارند.");
            }

            var terminalInfo = await _dataContext.Terminals
                .Where(x => x.PspId.HasValue && x.TerminalNo == changeAccountRequest.TerminalNo && x.StatusId == (byte)Enums.TerminalStatus.Installed)
                    .Select(x => new
                    {
                        x.AccountNo,
                        x.TerminalNo,
                        x.ContractNo,
                        x.ShebaNo,
                        x.PspId,
                        x.MerchantProfile.FirstName,
                        x.MerchantProfile.LastName,
                        x.MerchantNo,
                        x.MerchantProfile.NationalCode,
                        x.MerchantProfile.IsLegalPersonality,
                        x.MerchantProfile.LegalNationalCode,
                        x.TaxPayerCode,
                        x.Id,
                        AcceptorCode = x.MerchantNo,
                      
                    })
                    .FirstAsync(cancellationToken);

            if (!terminalInfo.PspId.HasValue)
            {
                return JsonErrorMessage("پایانه مورد نظر فاقد PSP می باشد");
            }

            return await SendChangeAccountRequest(terminalInfo.PspId.Value,
                changeAccountRequest.Id,
                terminalInfo.AccountNo,
                changeAccountRequest.AccountNo,
                terminalInfo.ContractNo,
                terminalInfo.ShebaNo,
                changeAccountRequest.ShebaNo,
                terminalInfo.FirstName,
                terminalInfo.LastName,
                terminalInfo.TerminalNo,
                terminalInfo.MerchantNo,
                changeAccountRequest.BranchId,
                terminalInfo.NationalCode,
                changeAccountRequest.FileData,
                terminalInfo.IsLegalPersonality,
                terminalInfo.LegalNationalCode,
                terminalInfo.TaxPayerCode,
                terminalInfo.AcceptorCode,
                (int)terminalInfo.Id);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangeStatus(long changeAccountRequestId, bool approve, CancellationToken cancellationToken)
        {
            var changeAccountRequest = await _dataContext.ChangeAccountRequests
                .Where(x => x.Id == changeAccountRequestId)
                .FirstAsync(cancellationToken);

            var terminal = await _dataContext.Terminals
                .Where(x => x.TerminalNo == changeAccountRequest.TerminalNo && x.PspId == (byte)Enums.PspCompany.Parsian)
                .FirstAsync(cancellationToken);

            if (approve)
            {
                if (terminal != null)
                {
                    terminal.LastUpdateTime = DateTime.Now;
                    terminal.ShebaNo = changeAccountRequest.ShebaNo;
                    terminal.AccountNo = changeAccountRequest.AccountNo;
                }
            }

            changeAccountRequest.StatusId = approve ? (byte)Enums.RequestStatus.Done : (byte)Enums.RequestStatus.Rejected;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        private async Task<ActionResult> SendChangeAccountRequest(byte pspId,
            long changeAccountRequestId,
            string oldAccountNo,
            string newAccountNo,
            string contractNo,
            string oldShebaNo,
            string newShebaNo,
            string firstName,
            string lastName,
            string terminalNo,
            string merchantNo,
            long branchId,
            string nationalCode,
            byte[] fileData,
            bool isLegalPersonality,
            string legalNationalCode,
            string TaxPayerCode,
            string  AcceptorCode,
            int terminalId)
        {
            var result = new SendChangeAccountRequestResponseModel { IsSuccess = false };

            switch (pspId)
            {
                case (byte)Enums.PspCompany.Fanava:
                    {
                        using (var fanavaService = new FanavaService())
                            result = await fanavaService.SendChangeAccountRequest(changeAccountRequestId, terminalNo, contractNo, newAccountNo, oldShebaNo, newShebaNo);
                        break;
                    }

                case (byte)Enums.PspCompany.IranKish:
                    {
                        //todo => new irnakishservice
                        // using (var irankishService = new IranKishService())
                        //     result = await irankishService.SendChangeAccountRequest(
                        //         changeAccountRequestId, oldAccountNo, newAccountNo, newShebaNo, firstName, lastName, merchantNo, branchId, fileData );
                        // break;
                        //
                        using (var irankishService = new NewIranKishService())
                            result = await irankishService.SendChangeAccountRequest(
                                changeAccountRequestId, oldAccountNo, newAccountNo, newShebaNo, firstName, lastName, merchantNo, branchId, fileData,oldShebaNo,
                                AcceptorCode);
                        break;
                    }

                case (byte)Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                        result = await pardakhtNovinService.SendChangeAccountRequest(
                            changeAccountRequestId, oldAccountNo, newAccountNo, newShebaNo, firstName, lastName, merchantNo, branchId, fileData, terminalId);
                    break;
                }
                case (byte) Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                    {
                        result = parsianService.NewSendChangeAccountRequest

                        (changeAccountRequestId, firstName + " " + lastName, newShebaNo, branchId,
                            isLegalPersonality ? legalNationalCode : nationalCode, terminalNo, oldShebaNo, TaxPayerCode,
                            AcceptorCode, fileData, terminalId,isLegalPersonality).Result;

                            if (result.IsSuccess && result.TopiarId == "0")
                        {
                            var logs = _dataContext.ParsianRequests2
                                .Where(b => b.TerminalId == terminalId && b.Method == "RequestChangeAccountInfo")
                                .Select(a =>   a.Result ).ToList()   ;

                            foreach (var VARIABLE in logs)
                            {
                                var log = JsonConvert.DeserializeObject<RequestChangeAccountInfoResult>(VARIABLE);
                                if (log.RequestResult.TopiarId != "0")
                                {
                                    result.TopiarId = log.RequestResult.TopiarId;
                                    break;
                                }
                            }
                           
                            // var res = JsonConvert.DeserializeObject<RequestChangeAccountInfoResult>(apiResponse);
                        }
                    }

                    break;

                // using (var parsianService = new ParsianService())
                        //     result = await parsianService.SendChangeAccountRequest
                        //         
                        //         (changeAccountRequestId, firstName + " " + lastName, newShebaNo, branchId, isLegalPersonality ? legalNationalCode : nationalCode, terminalNo, fileData);
                        // break;
                    }
            }

            var s = _dataContext.ChangeAccountRequests.Where(x => x.Id == changeAccountRequestId).UpdateAsync(x =>
                new ChangeAccountRequest
                {
                    Error = result.Error,
                    TopiarId  = result.TopiarId,
                    Result = result.Result, 
                    StatusId = result.StatusId,
                    RequestId = result.RequestId
                }).Result;

            return result.IsSuccess ? JsonSuccessMessage() : JsonSuccessMessage(MessageType.Danger, result.Result);
        }
    }
}