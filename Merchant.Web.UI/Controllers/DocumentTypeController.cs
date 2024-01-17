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

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class DocumentTypeController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public DocumentTypeController(AppDataContext dataContext)
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
            var result = await _dataContext.DocumentTypes
                .OrderByDescending(x => x.Title)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.IsRequired,
                    x.ForEntityTypeId,
                    x.IsForLegalPersonality
                })
                .ToListAsync(cancellationToken);

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult Create() => View("_Create");
        
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(DocumentTypeViewModel viewModel, CancellationToken cancellationToken)
        {
            _dataContext.DocumentTypes.Add(new DocumentType
            {
                Title = viewModel.Title,
                IsRequired = viewModel.IsRequired,
                ForEntityTypeId = viewModel.ForEntityTypeId,
                IsForLegalPersonality = viewModel.IsForLegalPersonality
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.DocumentTypes
                .Select(x => new DocumentTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsRequired = x.IsRequired,
                    IsForLegalPersonality = x.IsForLegalPersonality
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Edit(DocumentTypeViewModel viewModel, CancellationToken cancellationToken)
        {
            var documentType = await _dataContext.DocumentTypes.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
            documentType.Title = viewModel.Title;
            documentType.IsForLegalPersonality = viewModel.IsForLegalPersonality;
            documentType.IsRequired = viewModel.IsRequired;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (await _dataContext.TerminalDocuments.AnyAsync(x => x.DocumentTypeId == id, cancellationToken) || await _dataContext.MerchantProfileDocuments.AnyAsync(x => x.DocumentTypeId == id, cancellationToken))
            {
                return JsonWarningMessage("به علت وجود مدرک ثبت شده با این نوع امکان حذف وجود ندارد.");
            }

            var documentType = await _dataContext.DocumentTypes.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.DocumentTypes.Remove(documentType);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}