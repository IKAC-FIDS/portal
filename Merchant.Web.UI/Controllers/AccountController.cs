using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        public ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        private readonly AppDataContext _dataContext;

        public AccountController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                Redirect("~");
            }

            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [UserActivityLog(IgnoreActionParameters = "Password")]
        public async Task<ActionResult> Login(LoginViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var user = await _dataContext.Users.FirstOrDefaultAsync(x => !x.IsDeleted  && x.UserName.Equals(viewModel.Email, StringComparison.OrdinalIgnoreCase), cancellationToken);

            if (user == null)
            {
                AddDangerMessage("لطفاً نام کاربری و رمز عبور خود را با دقت وارد نمایید.");

                return View(viewModel);
            }

            if (user.LockoutEndDateUtc > DateTime.UtcNow)
            {
                return View("Lockout");
            }

            if (await UserManager.CheckPasswordAsync(user, viewModel.Password))
            {
                await UserManager.ResetAccessFailedCountAsync(user.Id);
                await SignInManager.SignInAsync(user, viewModel.RememberMe, false);

                if (User.IsJustCardRequester())
                {
                    return Redirect("\\CardRequest");
                }
                return Redirect(Url.IsLocalUrl(viewModel.ReturnUrl) ? viewModel.ReturnUrl : "~");
            }

            AddDangerMessage("لطفاً نام کاربری و رمز عبور خود را با دقت وارد نمایید.");
            await UserManager.AccessFailedAsync(user.Id);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<ActionResult> ChangePassword(CancellationToken cancellationToken)
        {
            var passwordExpirationDate = await _dataContext.Users.Where(x => x.Id == CurrentUserId && !x.IsDeleted).Select(x => x.PasswordExpirationDate).FirstAsync(cancellationToken);
            ViewBag.PasswordExpired = passwordExpirationDate < DateTime.Now;
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
           //                                          && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                              || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.UnderReview 
           //                                                && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                    || User.IsMessageManagerUser()));
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserActivityLog(IgnoreActionParameters = "CurrentPassword,NewPassword,ConfirmNewPassword")]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.CurrentPassword == viewModel.NewPassword)
            {
                AddDangerMessage("رمز عبور جدید نباید با رمز عبور قبلی شما یکسان باشد. لطفاً رمز عبور دیگری انتخاب نمایید.");
                // var message = _dataContext.Messages.ToList();
                // ViewBag.OpenMessage =message.Count(d => d.StatusId ==(int)Common.Enumerations.MessageStatus.Open   
                //                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                             || User.IsMessageManagerUser()));
                // ViewBag.InProgressMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.UnderReview 
                //                                               && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                                   || User.IsMessageManagerUser()));
                // var cardmessage = _dataContext.CardRequest.ToList();
                // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
                //                                                       && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                                           || User.IsCardRequestManager())); 
                // ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
                //                                                             && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
                //                                                                 || User.IsCardRequestManager()));
                return View();
            }

            if (viewModel.NewPassword.Equals("123456"))
            {
                AddDangerMessage("امکان انتخاب این رمز عبور وجود ندارد.");
                // var message = _dataContext.Messages.ToList();
                // ViewBag.OpenMessage =message.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
                //                                         && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                             || User.IsMessageManagerUser()));
                // ViewBag.InProgressMessage =message.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview 
                //                                               && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                                   || User.IsMessageManagerUser()));
                //
                // var cardmessage = _dataContext.CardRequest.ToList();
                // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
                //                                                       && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                //                                                           || User.IsCardRequestManager())); 
                // ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.CardRequestStatus.UnderReview  
                //                                                             && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
                //                                                                 || User.IsCardRequestManager()));
                return View();
            }

            var result = await UserManager.ChangePasswordAsync(CurrentUserId, viewModel.CurrentPassword, viewModel.NewPassword);

            if (result.Succeeded)
            {
                var user = await _dataContext.Users.FirstAsync(x => x.Id == CurrentUserId && !x.IsDeleted, cancellationToken);
                user.PasswordExpirationDate = DateTime.Now.AddMonths(3);
                await _dataContext.SaveChangesAsync(cancellationToken);

                AddSuccessMessage("رمز عبور شما با موفقیت تغییر پیدا کرد.");
                await SignInManager.SignInAsync(user, false, false);

                return Redirect("~");
            }

            AddDangerMessage("متاسفانه خطایی رخ داد.");
            var amessage = _dataContext.Messages.ToList();
            ViewBag.OpenMessage =amessage.Count(d => d.StatusId ==   (int)Common.Enumerations.MessageStatus.Open   
                                                    && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
                                                        || User.IsMessageManagerUser()));
            return View();
        }
      
        public async Task<ActionResult> LogOff()
        {
            var user = await UserManager.FindByNameAsync(User.Identity.Name);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            await UserManager.UpdateSecurityStampAsync(user.Id);

            return Redirect("~");
        }
    }
}