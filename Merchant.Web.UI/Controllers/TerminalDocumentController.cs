using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser)]
    public class TerminalDocumentController : Controller
    {
        private readonly AppDataContext _dataContext;

        public TerminalDocumentController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<ActionResult> GetDocument(int id, CancellationToken cancellationToken)
        {
            var data = await _dataContext.TerminalDocuments
                .Where(x => x.Id == id)
                .Select(x => new { x.FileName, x.FileData })
                .FirstOrDefaultAsync(cancellationToken);

            if (data.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            return File(data.FileData, "application/pdf", data.FileName);
        }
    }
}