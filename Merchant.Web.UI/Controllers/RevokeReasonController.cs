using System.Collections.Generic;
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
    [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
    public class RevokeReasonController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public RevokeReasonController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Manage()
        {
          
            
            
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetData(CancellationToken cancellationToken)
        {
            var result = await _dataContext.RevokeReasons
                .OrderBy(x => x.Level)
                .ThenBy(x => x.Title)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Level ,
                    x.ParentId,
                    Parent = x.Parent.Title
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult Create()
        {
            var parentList =   _dataContext.RevokeReasons.Where(a=>a.ParentId == null && a.Level != 2)
                .Select(x => new {x.Id, x.Title})
                .ToList();

            ViewBag.ParentList = parentList.ToSelectList(x => x.Id, x => x.Title);

            return View("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(RevokeReasonViewModel viewModel, CancellationToken cancellationToken)
        {
            var rr = new RevokeReason {Title = viewModel.Title, Order = 5, Level = viewModel.Level , ParentId = viewModel.ParentId};
            _dataContext.RevokeReasons.Add(rr);
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> Edit(byte id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.RevokeReasons.Where(x => x.Id == id)
                .Select(x => new RevokeReasonViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Level = x.Level ,
                    ParentId = x.ParentId
                })
                .FirstAsync(cancellationToken);
            var parentList =   _dataContext.RevokeReasons.Where(a=>a.ParentId == null && a.Level != 2)
                .Select(x => new {x.Id, x.Title})
                .ToList();

            ViewBag.ParentList = parentList.ToSelectList(x => x.Id, x => x.Title);

            return View("_Edit", viewModel);
        }
        public JsonResult LoadSecond(int id)
        {
            var pspid = _dataContext.RevokeReasons.Where(b => b.ParentId == id && b.Id != 3).OrderBy(a=>a.Order).ToList();
           
             
            var subcategoriesData = pspid.Select(m => new SelectListItem()
            {
                Text = m.Title.ToString(),
                Value = m.Id.ToString(),
            });
            return Json(subcategoriesData, JsonRequestBehavior.AllowGet);
            
            
        }
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Edit(RevokeReasonViewModel viewModel, CancellationToken cancellationToken)
        {
            var revokeReason = await _dataContext.RevokeReasons.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            revokeReason.Title = viewModel.Title;
            revokeReason.Level = viewModel.Level;
            revokeReason.ParentId = viewModel.ParentId;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Delete(byte id, CancellationToken cancellationToken)
        {
            if (await _dataContext.RevokeRequests.AnyAsync(x => x.ReasonId == id, cancellationToken))
            {
                return JsonErrorMessage("به علت وجود حداقل یک مورد درخواست جمع آوری با این دلیل، امکان حذف وجود ندارد.");
            }

            var revokeReason = await _dataContext.RevokeReasons.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.RevokeReasons.Remove(revokeReason);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
    }
}