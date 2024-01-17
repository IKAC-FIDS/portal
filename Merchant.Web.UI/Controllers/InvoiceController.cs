using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.Administrator)]
    public class InvoiceController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public InvoiceController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public ActionResult Manage()
        {
        
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GetData(CancellationToken cancellationToken)
        {
            var data = await _dataContext.Invoices
                .OrderBy(x => x.Id)
                .Select(x => new
                {
                    x.Id,
                    x.ToDate,
                    x.FromDate,
                    x.IsConfirmed,
                    x.CoefficientFine,
                    x.CoefficientReward,
                    x.WithWireMarketerPrice,
                    x.MinNotPMFinePercentage,
                    x.WirlessTesMarketerPrice,
                    x.MaxRewardPricePerDevice,
                    x.NotPMFinePricePerDevice,
                    x.WithWireTesMarketerPrice,
                    x.NotEMDelayAllowedWorkDay,
                    x.NotEMFinePricePerWorkDay,
                    x.WirlessBankMarketerPrice,
                    x.MinWirlessTesMarketerCount,
                    x.TransactionAmountForReward,
                    x.MinWithWireTesMarketerCount,
                    x.NotInstalledDelayAllowedWorkDay,
                    x.NotInstalledFinePricePerWorkDay,
                    x.GetTerminalNoDelayAllowedWorkDay,
                    x.NotGetTerminalNoFinePricePerWorkDay,
                    x.NotWirelessNotRevokePrice,
                    x.WirelessNotRevokePrice,
                    InvoiceTypeTitle = x.InvoiceType.Title
                })
                .ToListAsync(cancellationToken);

            var result = data.Select(x => new
            {
                x.Id,
                x.IsConfirmed,
                //x.CoefficientFine,
                x.InvoiceTypeTitle,
                //x.CoefficientReward,
                x.WithWireMarketerPrice,
                x.MinNotPMFinePercentage,
                x.WirelessNotRevokePrice,
                x.NotWirelessNotRevokePrice,
                //x.WirlessTesMarketerPrice,
                //x.MaxRewardPricePerDevice,
                x.NotPMFinePricePerDevice,
                //x.WithWireTesMarketerPrice,
                x.NotEMDelayAllowedWorkDay,
                x.NotEMFinePricePerWorkDay,
                x.WirlessBankMarketerPrice,
                //x.MinWirlessTesMarketerCount,
                //x.TransactionAmountForReward,
                //x.MinWithWireTesMarketerCount,
                x.NotInstalledDelayAllowedWorkDay,
                x.NotInstalledFinePricePerWorkDay,
                ToDate = x.ToDate.ToPersianDate(),
                x.GetTerminalNoDelayAllowedWorkDay,
                FromDate = x.FromDate.ToPersianDate(),
                x.NotGetTerminalNoFinePricePerWorkDay
            })
            .OrderByDescending(a=>a.FromDate).ToList();

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(CancellationToken cancellationToken)
        {
            ViewBag.InvoiceTypeList = (await _dataContext.InvoiceTypes
                .Select(x => new { x.Id, x.Title })
                .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            return View("_Create");
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Create(InvoiceViewModel viewModel)
        {
            _dataContext.Invoices.Add(new Invoice
            {
                ToDate = viewModel.ToDate,
                FromDate = viewModel.FromDate,
                IsConfirmed = viewModel.IsConfirmed,
                InvoiceTypeId = viewModel.InvoiceTypeId,
                CoefficientFine = viewModel.CoefficientFine,
                CoefficientReward = viewModel.CoefficientReward,
                WithWireMarketerPrice = viewModel.WithWireMarketerPrice,
                MinNotPMFinePercentage = viewModel.MinNotPMFinePercentage,
                NotPMFinePricePerDevice = viewModel.NotPMFinePricePerDevice,
                MaxRewardPricePerDevice = viewModel.MaxRewardPricePerDevice,
                WirlessTesMarketerPrice = viewModel.WirlessTesMarketerPrice,
                WithWireTesMarketerPrice = viewModel.WithWireTesMarketerPrice,
                WirlessBankMarketerPrice = viewModel.WirlessBankMarketerPrice,
                NotEMDelayAllowedWorkDay = viewModel.NotEMDelayAllowedWorkDay,
                NotEMFinePricePerWorkDay = viewModel.NotEMFinePricePerWorkDay,
                TransactionAmountForReward = viewModel.TransactionAmountForReward,
                MinWirlessTesMarketerCount = viewModel.MinWirlessTesMarketerCount,
                MinWithWireTesMarketerCount = viewModel.MinWithWireTesMarketerCount,
                NotInstalledDelayAllowedWorkDay = viewModel.NotInstalledDelayAllowedWorkDay,
                NotInstalledFinePricePerWorkDay = viewModel.NotInstalledFinePricePerWorkDay,
                GetTerminalNoDelayAllowedWorkDay = viewModel.GetTerminalNoDelayAllowedWorkDay,
                NotGetTerminalNoFinePricePerWorkDay = viewModel.NotGetTerminalNoFinePricePerWorkDay,
                WirelessNotRevokePrice = viewModel.WirelessNotRevokePrice,
                NotWirelessNotRevokePrice = viewModel.NotWirelessNotRevokePrice
            });

            await _dataContext.SaveChangesAsync();

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(int id)
        {
            var viewModel = await _dataContext.Invoices.Where(x => x.Id == id)
                .Select(x => new InvoiceViewModel
                {
                    Id = x.Id,
                    ToDate = x.ToDate,
                    FromDate = x.FromDate,
                    IsConfirmed = x.IsConfirmed,
                    InvoiceTypeId = x.InvoiceTypeId,
                    CoefficientFine = x.CoefficientFine,
                    CoefficientReward = x.CoefficientReward,
                    WithWireMarketerPrice = x.WithWireMarketerPrice,
                    MinNotPMFinePercentage = x.MinNotPMFinePercentage,
                    MaxRewardPricePerDevice = x.MaxRewardPricePerDevice,
                    NotPMFinePricePerDevice = x.NotPMFinePricePerDevice,
                    WirlessTesMarketerPrice = x.WirlessTesMarketerPrice,
                    WithWireTesMarketerPrice = x.WithWireTesMarketerPrice,
                    NotEMDelayAllowedWorkDay = x.NotEMDelayAllowedWorkDay,
                    NotEMFinePricePerWorkDay = x.NotEMFinePricePerWorkDay,
                    WirlessBankMarketerPrice = x.WirlessBankMarketerPrice,
                    MinWirlessTesMarketerCount = x.MinWirlessTesMarketerCount,
                    TransactionAmountForReward = x.TransactionAmountForReward,
                    MinWithWireTesMarketerCount = x.MinWithWireTesMarketerCount,
                    NotInstalledDelayAllowedWorkDay = x.NotInstalledDelayAllowedWorkDay,
                    NotInstalledFinePricePerWorkDay = x.NotInstalledFinePricePerWorkDay,
                    GetTerminalNoDelayAllowedWorkDay = x.GetTerminalNoDelayAllowedWorkDay,
                    NotGetTerminalNoFinePricePerWorkDay = x.NotGetTerminalNoFinePricePerWorkDay,
                    WirelessNotRevokePrice = x.WirelessNotRevokePrice,
                    NotWirelessNotRevokePrice = x.NotWirelessNotRevokePrice,
                })
                .FirstAsync();

            var invoiceTypeList = await _dataContext.InvoiceTypes
                .Select(x => new { x.Id, x.Title })
                .ToListAsync();

            ViewBag.InvoiceTypeList = invoiceTypeList.ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] { viewModel.InvoiceTypeId });

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Edit(InvoiceViewModel viewModel, CancellationToken cancellationToken)
        {
            var invoice = await _dataContext.Invoices.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            invoice.ToDate = viewModel.ToDate;
            invoice.FromDate = viewModel.FromDate;
            invoice.IsConfirmed = viewModel.IsConfirmed;
            invoice.InvoiceTypeId = viewModel.InvoiceTypeId;
            invoice.CoefficientFine = viewModel.CoefficientFine;
            invoice.CoefficientReward = viewModel.CoefficientReward;
            invoice.WithWireMarketerPrice = viewModel.WithWireMarketerPrice;
            invoice.MinNotPMFinePercentage = viewModel.MinNotPMFinePercentage;
            invoice.NotPMFinePricePerDevice = viewModel.NotPMFinePricePerDevice;
            invoice.WirlessTesMarketerPrice = viewModel.WirlessTesMarketerPrice;
            invoice.MaxRewardPricePerDevice = viewModel.MaxRewardPricePerDevice;
            invoice.NotEMDelayAllowedWorkDay = viewModel.NotEMDelayAllowedWorkDay;
            invoice.NotEMFinePricePerWorkDay = viewModel.NotEMFinePricePerWorkDay;
            invoice.WirlessBankMarketerPrice = viewModel.WirlessBankMarketerPrice;
            invoice.WithWireTesMarketerPrice = viewModel.WithWireTesMarketerPrice;
            invoice.TransactionAmountForReward = viewModel.TransactionAmountForReward;
            invoice.MinWirlessTesMarketerCount = viewModel.MinWirlessTesMarketerCount;
            invoice.MinWithWireTesMarketerCount = viewModel.MinWithWireTesMarketerCount;
            invoice.NotInstalledDelayAllowedWorkDay = viewModel.NotInstalledDelayAllowedWorkDay;
            invoice.NotInstalledFinePricePerWorkDay = viewModel.NotInstalledFinePricePerWorkDay;
            invoice.GetTerminalNoDelayAllowedWorkDay = viewModel.GetTerminalNoDelayAllowedWorkDay;
            invoice.NotGetTerminalNoFinePricePerWorkDay = viewModel.NotGetTerminalNoFinePricePerWorkDay;

            invoice.WirelessNotRevokePrice = viewModel.WirelessNotRevokePrice;
            invoice.NotWirelessNotRevokePrice = viewModel.NotWirelessNotRevokePrice;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var invoice = await _dataContext.Invoices.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.Invoices.Remove(invoice);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
    }
}