using System;
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
    [CustomAuthorize]
    public class TerminalNoteController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public TerminalNoteController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<ActionResult> Index(long terminalId, CancellationToken cancellationToken)
        {
            var viewModel = new TerminalNoteIndexViewModel
            {
                TerminalId = terminalId,
                Notes = await _dataContext.TerminalNotes.Where(x => x.TerminalId == terminalId).Select(x =>
                        new TerminalNoteIndexViewModel.TerminalNoteViewModel
                        {
                            Body = x.Body,
                            SubmitTime = x.SubmitTime,
                            SubmitterUserId = x.SubmitterUserId,
                            SubmitterUserFullName = x.SubmitterUser.FullName,
                            OrganizationUnitId = x.SubmitterUser.OrganizationUnitId,
                            OrganizationUnitTitle = x.SubmitterUser.OrganizationUnit.Title
                        })
                    .ToListAsync(cancellationToken)
            };
            return View(viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> Create(TerminalNoteViewModel viewModel, CancellationToken cancellationToken)
        {
            _dataContext.TerminalNotes.Add(new TerminalNote
            {
                Body = viewModel.Body,
                SubmitTime = DateTime.Now,
                SubmitterUserId = CurrentUserId,
                TerminalId = viewModel.TerminalId,
                
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}