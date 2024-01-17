using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize]
    public class RegionalMunicipalityController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public RegionalMunicipalityController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetRegionalMunicipalities(byte stateId, CancellationToken cancellationToken)
        {
            var result = (await _dataContext.RegionalMunicipalities
                .Where(x => x.StateId == stateId)
                .Select(x => new { x.Id, x.Title })
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            return JsonSuccessResult(result);
        }
    }
}