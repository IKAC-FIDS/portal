using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Merchant.Web.UI.Service;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class ServiceController : BaseController
    {
        
        private readonly AppDataContext _dataContext;

        public ServiceController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public ActionResult Status()
        {
      return View();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> GetStatus()
        {
            bool fanavaIsUp, irankishIsUp, parsianIsUp, tosanIsUp;

            using (var fanavaService = new FanavaService())
            using (var irankishService = new NewIranKishService())
            using (var parsianService = new ParsianService())
            {
                fanavaIsUp = await fanavaService.IsUp();
                irankishIsUp =   irankishService.IsUp().Length > 0;
                parsianIsUp = parsianService.IsUp();
                tosanIsUp = TosanService.IsUp();
            }

            return JsonSuccessResult(new { fanavaIsUp, irankishIsUp, parsianIsUp, tosanIsUp });
        }
    }
}