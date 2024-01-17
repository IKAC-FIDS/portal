using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class GuildController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public GuildController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public ActionResult Index()
        {
  ;
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> GetData(long? parentGuildId, long? guildId, string title, CancellationToken cancellationToken)
        {
            var query = _dataContext.Guilds.AsQueryable();

            if (parentGuildId.HasValue)
            {
                query = query.Where(x => x.ParentId == parentGuildId);
            }

            if (guildId.HasValue)
            {
                query = query.Where(x => x.Id == guildId);
            }

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(x => (x.Title.Contains(title) || x.Parent.Title.Contains(title)) && x.IsActive);
            }

            var result = await query
                .Where(x => x.ParentId.HasValue)
                .OrderBy(x => x.IsActive)
                .GroupBy(x => new { x.ParentId, x.Parent.Title })
                .Select(x => new
                {
                    x.Key.ParentId,
                    ParentTitle = x.Key.Title,
                    ChildGuilds = x.Select(y => new 
                    {
                        y.Id,
                        y.Title
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetChildGuilds(long parentId, CancellationToken cancellationToken)
        {
            var result = (await _dataContext.Guilds
                .Where(x => x.ParentId == parentId && x.IsActive)
                .OrderByDescending(x => x.IsActive)
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            return JsonSuccessResult(result);
        }
    }
}