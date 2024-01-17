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
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [Authorize]
    public class BranchConnectorController : BaseController
    {
        public ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        private readonly AppDataContext _dataContext;

        public BranchConnectorController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        [CustomAuthorize(  DefaultRoles.BranchUser)]
        public async Task<ActionResult> Edit(BranchConnectorViewModel viewModel)
        {

           
                var a =  _dataContext.BranchConnector.FirstOrDefault(b => b.OrganizationUnitId == CurrentUserBranchId);
                if (a == null)
                {
                     
                        a = new BranchConnector();
                        a.FirstName = viewModel.FirstName;
                        a.LastName = viewModel.LastName;
                        a.OrganizationUnitId = (int) CurrentUserBranchId;
                        a.PhoneNumber = viewModel.PhoneNumber;
                        _dataContext.BranchConnector.Add(a);
                        _dataContext.SaveChanges();
                   
                }

                else
                {
                    a.FirstName = viewModel.FirstName;
                    a.LastName = viewModel.LastName;
                    a.PhoneNumber = viewModel.PhoneNumber;
                    _dataContext.SaveChanges();
                }
           

            return JsonSuccessResult();
        }
        
        [CustomAuthorize(  DefaultRoles.BranchUser)]
        [HttpGet]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
         

            
            var result = ( _dataContext.BranchConnector
                    .Where(x => x.OrganizationUnitId == CurrentUserBranchId)
                    .Select(x => new { x.Id, x.FirstName,x.LastName,x.PhoneNumber })
                    
                    .FirstOrDefault()) ;

            if (result == null)
            {
                return   View(new BranchConnectorViewModel());
            }
            var vieModel = new BranchConnectorViewModel
            {
                Id = result.Id,
                FirstName = result.FirstName,
                LastName = result.LastName,
                PhoneNumber = result.PhoneNumber
            };

            return View(vieModel);
        }

        [HttpGet]
        [CustomAuthorize(  DefaultRoles.BranchManagment,DefaultRoles.Administrator)]
        public ActionResult Manage()
        {
            return View();
        }
        
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(  DefaultRoles.BranchManagment,DefaultRoles.Administrator)]
        public async Task<ActionResult> GetData( )
        {
            var result =   _dataContext.BranchConnector
             
                .OrderBy(x => x.OrganizationUnitId)
                .Select(x => new
                {
                    x.Id,
                    x.FirstName,
                    x.LastName,
                    OrganizationUnit =   x.OrganizationUnit.Title,
                    x.PhoneNumber, 
                })
                .ToList( );

            return JsonSuccessResult(result);
        }


    }
}