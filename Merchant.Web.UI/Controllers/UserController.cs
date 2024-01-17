using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Data;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class UserController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public UserController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Manage()
        {
            var branchList = await _dataContext.OrganizationUnits
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Title)
                .ToListAsync();

            ViewBag.BranchList = branchList.ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            var roleList = await _dataContext.Roles
                .Select(x => new { x.Id, x.PersianName })
                .OrderBy(x => x.Id)
                .ToListAsync();

            ViewBag.RoleList = roleList.ToSelectList(x => x.Id, x => x.PersianName);
               return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> GetData(UserSearchParameters viewModel)
        {
            var query = _dataContext.Users.Where(x => !x.IsDeleted).AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.UserName))
            {
                query = query.Where(x => x.UserName.Equals(viewModel.UserName));
            }

            if (!string.IsNullOrEmpty(viewModel.Email))
            {
                query = query.Where(x => x.Email.Contains(viewModel.Email));
            }

            if (!string.IsNullOrEmpty(viewModel.FullName))
            {
                query = query.Where(x => x.FullName.Contains(viewModel.FullName));
            }

            if (!string.IsNullOrEmpty(viewModel.PhoneNumber))
            {
                query = query.Where(x => x.PhoneNumber.Contains(viewModel.PhoneNumber));
            }

            if (viewModel.BranchId.HasValue)
            {
                query = query.Where(x => x.OrganizationUnitId == viewModel.BranchId);
            }

            if (viewModel.RoleIdList.Any())
            {
                query = query.Where(x => x.Roles.Any(y => viewModel.RoleIdList.Contains(y.RoleId)));
            }

            if (viewModel.Locked.HasValue)
            {
                query = viewModel.Locked.Value ? query.Where(x => x.LockoutEndDateUtc.HasValue && x.LockoutEndDateUtc >= DateTime.UtcNow) : query.Where(x => !x.LockoutEndDateUtc.HasValue || x.LockoutEndDateUtc < DateTime.UtcNow);
            }

            var totalRowsCount = 0;

            if (viewModel.RetriveTotalPageCount)
            {
                totalRowsCount = await query.CountAsync();
            }

            var rows = await query
                .OrderByDescending(x => x.Id)
                .Skip((viewModel.Page - 1) * 20)
                .Take(20)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.UserName,
                    x.PhoneNumber,
                    IsLocked = x.LockoutEndDateUtc.HasValue && x.LockoutEndDateUtc > DateTime.UtcNow,
                    BranchTitle = x.OrganizationUnitId.HasValue ? x.OrganizationUnit.Title : string.Empty
                })
                .ToListAsync();

            return JsonSuccessResult(new { rows, totalRowsCount });
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Create()
        {
            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Title)
                .ToListAsync())
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            return PartialView("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(UserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (await _dataContext.Users.AnyAsync(x => x.UserName == viewModel.UserName, cancellationToken))
            {
                return JsonErrorMessage("این نام کاربری از قبل ثبت شده است.");
            }

            var user = new User
            {
                EmailConfirmed = true,
                FullName = viewModel.FullName,
                UserName = viewModel.UserName,
                PhoneNumber = viewModel.PhoneNumber,
                PasswordExpirationDate = DateTime.Now,
                OrganizationUnitId = viewModel.BranchId
            };

            var result = await UserManager.CreateAsync(user, viewModel.NewPassword);
            if (!result.Succeeded)
            {
                return JsonErrorMessage();
            }

            UserManager.AddToRole(user.Id, DefaultRoles.BranchUser.ToString());
            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long userId, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstAsync(x => x.Id == userId, cancellationToken);
            user.IsDeleted = true;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangeOrganizationUnit(long userId, CancellationToken cancellationToken)
        {
            var userOrganizationUnitId = await _dataContext.Users
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => x.OrganizationUnitId)
                .FirstAsync(cancellationToken);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}", selectedValue: new[] { userOrganizationUnitId });

            var viewModel = new ChangeOrganizationUnitViewModel
            {
                BranchId = userOrganizationUnitId,
                UserId = userId
            };

            return View("_ChangeOrganizationUnit", viewModel);
        }

        
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangePhoneNumber(long userId, CancellationToken cancellationToken)
        {
            var userOrganizationUnitId = await _dataContext.Users
                .Where(x => x.Id == userId && !x.IsDeleted)
                .Select(x => x.PhoneNumber)
                .FirstAsync(cancellationToken);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new { x.Id, x.Title })
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}", selectedValue: new[] { userOrganizationUnitId });

            var viewModel = new ChangePhoneNumberViewModel
            {
                PhoneNumber = userOrganizationUnitId,
                UserId = userId
            };

            return View("_ChangePhoneNumber", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangeOrganizationUnit(ChangeOrganizationUnitViewModel viewModel, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstAsync(x => !x.IsDeleted && x.Id == viewModel.UserId, cancellationToken);
            user.OrganizationUnitId = viewModel.BranchId;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> ChangePhoneNumber(ChangePhoneNumberViewModel viewModel, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstAsync(x => !x.IsDeleted && x.Id == viewModel.UserId, cancellationToken);
            user.PhoneNumber = viewModel.PhoneNumber;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> NewPassword(long userId, CancellationToken cancellationToken)
        {
            var token = await UserManager.GeneratePasswordResetTokenAsync(userId);
            var newPassword = new Random().Next(1000000, 100000000).ToString();
            var result = await UserManager.ResetPasswordAsync(userId, token, newPassword);

            if (result.Succeeded)
            {
                ViewBag.NewPassword = newPassword;
                var user = await _dataContext.Users.FirstAsync(x => !x.IsDeleted && x.Id == userId, cancellationToken);
                user.PasswordExpirationDate = DateTime.Now;
                await _dataContext.SaveChangesAsync(cancellationToken);
                await UserManager.UpdateSecurityStampAsync(userId);

                return View("_NewPassword");
            }
           
            return View("_Error");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Lock(long userId, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            user.LockoutEndDateUtc = DateTime.UtcNow.AddYears(10);
            user.SecurityStamp = Guid.NewGuid().ToString("D");
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Unlock(long userId, CancellationToken cancellationToken)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
            user.LockoutEndDateUtc = null;
            user.SecurityStamp = Guid.NewGuid().ToString("D");
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}