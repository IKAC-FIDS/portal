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
    public class PspAgentController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public PspAgentController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
       
            return View( );
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetData(byte pspId, CancellationToken cancellationToken)
        {
            var result = await _dataContext.PspAgents
                .Where(x => x.PspId == pspId)
                .OrderBy(x => x.Title)
                .Select(x => new
                {
                    x.Id,
                    x.Tel,
                    x.Title,
                    x.Address,
                    x.CityName,
                    x.EmergencyTel
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(CancellationToken cancellationToken)
        {
            ViewBag.PspList = (await _dataContext.Psps
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            return View("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(PspAgentViewModel viewModel, CancellationToken cancellationToken)
        {
            _dataContext.PspAgents.Add(new PspAgent
            {
                Tel = viewModel.Tel,
                PspId = viewModel.PspId,
                Title = viewModel.Title,
                Address = viewModel.Address,
                CityName = viewModel.CityName,
                EmergencyTel = viewModel.EmergencyTel
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.PspAgents.Where(x => x.Id == id)
                .Select(x => new PspAgentViewModel
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    Address = x.Address,
                    CityName = x.CityName,
                    EmergencyTel = x.EmergencyTel
                })
                .FirstAsync(cancellationToken);

            ViewBag.PspList = (await _dataContext.Psps
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.PspId });

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Edit(PspAgentViewModel viewModel, CancellationToken cancellationToken)
        {
            var pspAgent = await _dataContext.PspAgents.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            pspAgent.Tel = viewModel.Tel;
            pspAgent.PspId = viewModel.PspId;
            pspAgent.Title = viewModel.Title;
            pspAgent.Address = viewModel.Address;
            pspAgent.CityName = viewModel.CityName;
            pspAgent.EmergencyTel = viewModel.EmergencyTel;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            var pspAgent = await _dataContext.PspAgents.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.PspAgents.Remove(pspAgent);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
    }
}