using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class OrganizationUnitController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public OrganizationUnitController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult Manage()
        {
     
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GetData(GetOrganizationUnitDataViewModel viewModel, CancellationToken cancellationToken)
        {
            var query = _dataContext.OrganizationUnits.AsQueryable();

            if (!string.IsNullOrEmpty(viewModel.OrganizationUnitTitle))
            {
                query = query.Where(x => x.Title.Contains(viewModel.OrganizationUnitTitle));
            }

            if (viewModel.OrganizationUnitId.HasValue)
            {
                query = query.Where(x => x.Id == viewModel.OrganizationUnitId);
            }

            var result = await query
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.CityId,
                    CityTitle = x.City.Title,
                    StateTitle = x.City.State.Title,
                    UserCount = x.Users.Count,
                    x.DisableNewTerminalRequest,
                    x.DisableWirelessTerminalRequest
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.OrganizationUnits.Where(x => x.Id == id)
                .Select(x => new OrganizationUnitViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    DisableNewTerminalRequest = x.DisableNewTerminalRequest,
                    DisableWirelessTerminalRequest = x.DisableWirelessTerminalRequest
                })
                .FirstAsync(cancellationToken);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(OrganizationUnitViewModel viewModel, CancellationToken cancellationToken)
        {
            var organizationUnit = await _dataContext.OrganizationUnits.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            organizationUnit.Title = viewModel.Title;
            organizationUnit.DisableNewTerminalRequest = viewModel.DisableNewTerminalRequest;
            organizationUnit.DisableWirelessTerminalRequest = viewModel.DisableWirelessTerminalRequest;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult GroupChangePermissions()
        {
            return View("_GroupChangePermissions");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GroupChangePermissions(OrganizationUnitGroupChangePermissionsViewModel viewModel, CancellationToken cancellationToken)
        {
            var organizationUnits = await _dataContext.OrganizationUnits.Where(x => viewModel.OrganizationUnitIdList.Contains(x.Id)).ToListAsync(cancellationToken);

            organizationUnits.ForEach(x =>
            {
                x.DisableNewTerminalRequest = viewModel.DisableNewTerminalRequest;
                x.DisableWirelessTerminalRequest = viewModel.DisableWirelessTerminalRequest;
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
    }
}