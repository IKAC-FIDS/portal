using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class UserRoleController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public UserRoleController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> Manage(long userId, CancellationToken cancellationToken)
        {
            var userRoles = await _dataContext.Users
                .Where(x => !x.IsDeleted && x.Id == userId)
                .SelectMany(x => x.Roles.Select(y => y.RoleId))
                .ToListAsync(cancellationToken);

            var viewModel = await _dataContext.Roles.Where(role=>role.Id !=11 )
                .Select(x => new UserRoleManageViewModel
                {
                    RoleId = x.Id,
                    RoleName = x.PersianName,
                    IsSelected = userRoles.Contains(x.Id)
                })
                .ToListAsync(cancellationToken);

            ViewBag.CUserId = userId;
          //  ViewBag.UserId = CurrentUserId;
            ViewBag.UserList = (await _dataContext.Users.Where(d=>d.Roles.Any(v=>v.RoleId == 5))
                    .Select(x => new { UserId = x.Id, x.FullName })
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.UserId, x => x.FullName);
            return View("_Manage", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Toggle(ToggleUserRoleViewModel viewModel, CancellationToken cancellationToken)
        {
            var result = true;
            var userRoles = (await _dataContext.Users.Include(x => x.Roles)
                .FirstAsync(x => x.Id == viewModel.UserId && !x.IsDeleted, cancellationToken)).Roles;

            var roles = userRoles.Where(x => x.RoleId == viewModel.RoleId).ToList();
            if (roles.Any())
            {
                roles.ForEach(x => userRoles.Remove(x));
                result = false;
            }
            else
            {
                userRoles.Add(new UserRole { RoleId = viewModel.RoleId, UserId = viewModel.UserId });
            }
            var ds = _dataContext.SaveChanges();
            await _dataContext.SaveChangesAsync(cancellationToken);

        
            return JsonSuccessResult(result);
        }
    }
}