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
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class DeviceTypeController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public DeviceTypeController(AppDataContext dataContext)
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
            var result = await _dataContext.DeviceTypes
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Title,
                    x.IsActive,
                    x.IsWireless,
                    x.BlockPrice,
                    TerminalCount = x.Terminals.Count
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.DeviceTypes.Where(x => x.Id == id)
                .Select(x => new DeviceTypeViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Title = x.Title,
                    IsActive = x.IsActive,
                    IsWireless = x.IsWireless,
                    BlockPrice = x.BlockPrice
                })
                .FirstAsync(cancellationToken);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Edit(DeviceTypeViewModel viewModel, CancellationToken cancellationToken)
        {
            var deviceType = await _dataContext.DeviceTypes.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            deviceType.Code = viewModel.Code;
            deviceType.Title = viewModel.Title;
            deviceType.IsActive = viewModel.IsActive;
            deviceType.IsWireless = viewModel.IsWireless;
            deviceType.BlockPrice = viewModel.BlockPrice;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            if (_dataContext.Terminals.Any(x => x.DeviceTypeId == id ))
            {
                return JsonErrorMessage("به علت وجود حداقل یک مورد پذیرنده با این نوع دستگاه امکان حذف وجود ندارد.");
            }

            var deviceType = await _dataContext.DeviceTypes.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.DeviceTypes.Remove(deviceType);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult(deviceType.IsActive);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> ToggleIsActive(long id, CancellationToken cancellationToken)
        {
            var deviceType = await _dataContext.DeviceTypes.FirstAsync(x => x.Id == id, cancellationToken);

            deviceType.IsActive = !deviceType.IsActive;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult(deviceType.IsActive);
        }
    }
}