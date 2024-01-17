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
using Enums = TES.Common.Enumerations;


namespace TES.Merchant.Web.UI.Controllers
{
    public class RuleController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public long? CurrentUserBranchId => User.Identity.GetBranchId();
        public string CurrentUserBranch  => User.Identity.GetBranchTitle();

        public RuleController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        #region CustomerCategory

        [HttpGet]
        [AllowAnonymous]
        public ActionResult CustomerCategory()
        {
            return View();
        }

        [HttpGet]
        [AjaxOnly]
        public ActionResult CreateCustomerCategory() => View("CreateCustomerCategory");


        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> EditCustomerCategory(CustomerCategoryViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var revokeReason =
                await _dataContext.CustomerCategory.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);

            revokeReason.Name = viewModel.Name;
            revokeReason.From = viewModel.From;
            revokeReason.To = viewModel.To;

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> EditCustomerCategory(byte id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.CustomerCategory.Where(x => x.Id == id)
                .Select(x => new CustomerCategoryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    From = x.From,
                    To = x.To
                })
                .FirstAsync(cancellationToken);

            return View("EditCustomerCategory", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> DeleteCustomerCategory(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.CustomerCategory.FirstOrDefault(x => x.Id == id);
            _dataContext.CustomerCategory.Remove(query ?? throw new InvalidOperationException());
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> CreateCustomerCategory(CustomerCategoryViewModel viewModel,
            CancellationToken cancellationToken)
        {
            _dataContext.CustomerCategory.Add(new CustomerCategory()
                {Name = viewModel.Name, From = viewModel.From, To = viewModel.To});

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [AjaxOnly]
        [CustomAuthorize]
        public ActionResult GetCustomerCategory()
        {
            var result = _dataContext.CustomerCategory
                .OrderBy(x => x.From)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.From,
                    x.To
                })
                .ToList();

            return JsonSuccessResult(result);
        }

        #endregion

        #region RuleDefinition

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RuleDefinition()
        {
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> EditRuleDefinition(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!viewModel.PostedFile.IsValidFormat(".jpg,.jpeg"))
                return JsonErrorMessage(
                    "فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .jpg یا .jpeg باشد.");

            if (viewModel.PostedFile.IsValidFile() && viewModel.PostedFile.ContentLength > 1 * 1024 * 1024)
                return JsonErrorMessage("حجم فایل ارسالی بایستی کمتر از یک مگابایت باشد.");

            var revokeReason =
                await _dataContext.RuleDefinition.FirstAsync(x => (int) x.Id == (int) viewModel.Id, cancellationToken);

            revokeReason.FileData = viewModel.PostedFile.ToByteArray();
            revokeReason.PspId = (byte) viewModel.PspId;
            revokeReason.Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId);
            revokeReason.DeviceTypeId = viewModel.DeviceTypeId;
            revokeReason.DeviceType = _dataContext.DeviceTypes.FirstOrDefault(ad => ad.Id == viewModel.DeviceTypeId);
            revokeReason.RuleTypeId = viewModel.RuleTypeId;
            revokeReason.Description = viewModel.Description;
            revokeReason.FileData = viewModel.PostedFile.ToByteArray();

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> EditRuleDefinition(byte id, CancellationToken cancellationToken)
        {
            ViewBag.RuleType = (_dataContext.RuleType
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            var ed = _dataContext.DeviceTypes
                .Select(x => new {x.Id, Title = x.Title})
                .ToList();
            var ds = new {Id = (long) 1000, Title = "All Device"};
            ed.Add(ds);
            var qd = ed
                .ToSelectList(x => x.Id, x => x.Title);


            ViewBag.DeviceTypes = qd;


            var viewModel = await _dataContext.RuleDefinition.Where(x => x.Id == id)
                .Select(x => new RuleDefinitionRequestViewModel()
                {
                    Id = x.Id,
                    Description = x.Description,
                    RuleTypeId = x.RuleType.Id,
                    PspId = x.Psp.Id,
                    DeviceTypeId = (int) x.DeviceType.Id
                })
                .FirstAsync(cancellationToken);

            return View("EditRuleDefinition", viewModel);
        }


        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> DeleteRuleDefinition(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.RuleDefinition.FirstOrDefault(x => x.Id == id);
            _dataContext.RuleDefinition.Remove(query ?? throw new InvalidOperationException());
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }


        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> CreateRuleDefinition()
        {
            var viewModel = new ChangeAccountRequestViewModel
            {
            };
            ViewBag.RuleType = (_dataContext.RuleType
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            var ed = _dataContext.DeviceTypes
                .Select(x => new {x.Id, Title = x.Title})
                .ToList();
            var ds = new {Id = (long) 1000, Title = "All Device"};
            ed.Add(ds);
            var qd = ed
                .ToSelectList(x => x.Id, x => x.Title);


            ViewBag.DeviceTypes = qd;


            return PartialView("CreateRuleDefinition", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> CreateRuleDefinition(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (!viewModel.PostedFile.IsValidFormat(".jpg,.jpeg"))
                return JsonErrorMessage(
                    "فایل درخواست تغییر حساب را انتخاب نمایید. توجه نمایید که فایل انتخابی بایستی به صورت .jpg یا .jpeg باشد.");

            if (viewModel.PostedFile.IsValidFile() && viewModel.PostedFile.ContentLength > 1 * 1024 * 1024)
                return JsonErrorMessage("حجم فایل ارسالی بایستی کمتر از یک مگابایت باشد.");


            var changeAccountRequest = new RuleDefinition()
            {
                PspId = (byte) viewModel.PspId,
                Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId),
                DeviceTypeId = viewModel.DeviceTypeId,
                DeviceType = _dataContext.DeviceTypes.FirstOrDefault(ad => ad.Id == viewModel.DeviceTypeId),
                RuleTypeId = viewModel.RuleTypeId,
                Description = viewModel.Description,
                FileData = viewModel.PostedFile.ToByteArray(),
            };

            _dataContext.RuleDefinition.Add(changeAccountRequest);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }


        [AjaxOnly]
        [CustomAuthorize]
        public ActionResult GetRuleDefinition()
        {
            var result = _dataContext.RuleDefinition
                .Select(x => new
                {
                    x.Id,
                    DeviceType = x.DeviceTypeId == 1000 ? "All" : x.DeviceType.Title,
                    x.RuleType.Name,
                    x.Description,
                    PspTitle = x.Psp.Title
                })
                .ToList();

            return JsonSuccessResult(new {rows = result, totalRowsCount = result.Count});
        }

        #endregion

        #region RulePspWeight

        [HttpGet]
        [AllowAnonymous]
        public ActionResult RulePspWeight()
        {
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> CreateRulePspWeight(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var changeAccountRequest = new RulePspWeight()
            {
                PspId = (byte) viewModel.PspId,
                Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId),

                RuleTypeId = viewModel.RuleTypeId,
                Weight = viewModel.Weight,
            };

            _dataContext.RulePspWeight.Add(changeAccountRequest);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> EditRulePspWeight(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            var revokeReason =
                await _dataContext.RulePspWeight.FirstAsync(x => (int) x.Id == (int) viewModel.Id, cancellationToken);


            revokeReason.PspId = (byte) viewModel.PspId;
            revokeReason.Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId);

            revokeReason.RuleTypeId = viewModel.RuleTypeId;
            revokeReason.Weight = viewModel.Weight;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> EditRulePspWeight(byte id, CancellationToken cancellationToken)
        {
            ViewBag.RuleType = (_dataContext.RuleType
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);
 
            var viewModel = await _dataContext.RulePspWeight.Where(x => x.Id == id)
                .Select(x => new RuleDefinitionRequestViewModel()
                {
                    Id = x.Id,
                   
                    RuleTypeId = x.RuleType.Id,
                    PspId = x.Psp.Id,
                   Weight = x.Weight
                })
                .FirstAsync(cancellationToken);

            return View("EditRulePspWeight", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> DeleteRulePspWeight(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.RulePspWeight.FirstOrDefault(x => x.Id == id);
            _dataContext.RulePspWeight.Remove(query ?? throw new InvalidOperationException());
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
        }
        
        
        
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> CreateRulePspWeight()
        {
            var viewModel = new ChangeAccountRequestViewModel
            {
            };
            ViewBag.RuleType = (_dataContext.RuleType
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            return PartialView("CreateRulePspWeight", viewModel);
        }

        [AjaxOnly]
        [CustomAuthorize]
        public ActionResult GetRulePspWeight()
        {
            var result = _dataContext.RulePspWeight
                .Select(x => new
                {
                    x.Id,

                    x.RuleType.Name,
                    x.Weight,
                    PspTitle = x.Psp.Title
                })
                .ToList();

            return JsonSuccessResult(new {rows = result, totalRowsCount = result.Count});
        }

        #endregion

        #region PspWeight 
        [HttpGet]
        [AllowAnonymous]
        public ActionResult PspRating()
        {
            return View();
        }
        #endregion


        #region  pspbranchRate

       
        
        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> CreatePpsRating()
        {
            var viewModel = new ChangeAccountRequestViewModel
            {
                BranchTitle = CurrentUserBranch
            };
            
            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            return PartialView("CreatePpsRating", viewModel);
        }
        
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> EditPspRating(byte id,byte pspId, CancellationToken cancellationToken)
        {

            ViewBag.PspList = (_dataContext.Psps
                    .Select(x => new {x.Id, Title = x.Title})
                    .ToList())
                .ToSelectList(x => x.Id, x => x.Title);

            
            if (id == 0)
            {
                var data = new RuleDefinitionRequestViewModel();
                data.Id = 0;
                data.Description = "";
                data.Rate = 100;
                data.BranchTitle = CurrentUserBranch;
                data.PspId = pspId;
                return View("EditPspRating", data);
            }
         
         


            var viewModel = await _dataContext.PspBranchRate.Where(x => x.Id == id)
                .Select(x => new RuleDefinitionRequestViewModel()
                {
                    Id = x.Id,
                    Description = x.Description,
                    Rate =x.Rate,
                    BranchTitle = CurrentUserBranch,
                    PspId = x.Psp.Id,
                    
                })
                .FirstAsync(cancellationToken);

            return View("EditPspRating", viewModel);
        }

        
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> EditPspRating(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            if (viewModel.Id == 0)
            {
                var changeAccountRequest = new PspBranchRate()
                {
                    PspId = (byte) viewModel.PspId,
                    Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId),
                    Description = viewModel.Description,
                    OrganizationUnit =  _dataContext.OrganizationUnits.FirstOrDefault(d=>d.Id == CurrentUserBranchId.Value),
                    OrganizationUnitId =  CurrentUserBranchId,
                    Rate = viewModel.Weight,
                };

                _dataContext.PspBranchRate.Add(changeAccountRequest);

                await _dataContext.SaveChangesAsync(cancellationToken);

                return JsonSuccessResult();
            }
           
            var revokeReason =
                await _dataContext.PspBranchRate.FirstAsync(x => (int) x.Id == (int) viewModel.Id, cancellationToken);

        
            revokeReason.PspId = (byte) viewModel.PspId;
            revokeReason.Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId);
             
            revokeReason.Description = viewModel.Description;
            revokeReason.Rate = viewModel.Weight;
            revokeReason.OrganizationUnitId = CurrentUserBranchId;
            revokeReason.OrganizationUnit =
                _dataContext.OrganizationUnits.FirstOrDefault(d => d.Id == CurrentUserBranchId);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
        [HttpPost]
        [AjaxOnly]
        public async Task<ActionResult> CreatePpsRating(RuleDefinitionRequestViewModel viewModel,
            CancellationToken cancellationToken)
        {
            
        
            var changeAccountRequest = new PspBranchRate()
            {
                PspId = (byte) viewModel.PspId,
                Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id == viewModel.PspId),
Description = viewModel.Description,
                OrganizationUnit =  _dataContext.OrganizationUnits.FirstOrDefault(d=>d.Id == CurrentUserBranchId.Value),
                OrganizationUnitId =  CurrentUserBranchId,
                Rate = viewModel.Weight,
            };

            _dataContext.PspBranchRate.Add(changeAccountRequest);

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessResult();
        }
        [HttpPost]
        [AjaxOnly]
        public   ActionResult  BranchPspRating(PspRating viewModel 
           )
        {
            var pspId = 0;
            switch (viewModel.Psp)
            {
                case "Pa":
                    pspId = 3;
                    break;
                case "Ik":
                    pspId =2;
                    break;
            default:
                    pspId = 1;
                    break;

            }

            var Exist = _dataContext.PspBranchRate
                .Where(d => d.PspId == pspId && d.OrganizationUnitId == CurrentUserBranchId).FirstOrDefault();

            if (Exist != null)
            {
                Exist.Rate = viewModel.Rate;
                _dataContext.SaveChanges();
                return JsonSuccessResult();

            }
        
            var changeAccountRequest = new PspBranchRate()
            {
                PspId = (byte)pspId,
                Psp = _dataContext.Psps.FirstOrDefault(ad => ad.Id ==pspId),
               // Description = viewModel.Description,
                OrganizationUnit =  _dataContext.OrganizationUnits.FirstOrDefault(d=>d.Id == CurrentUserBranchId.Value),
                OrganizationUnitId =  CurrentUserBranchId,
                Rate = viewModel.Rate,
            };

            _dataContext.PspBranchRate.Add(changeAccountRequest);

              _dataContext.SaveChanges( );

            return JsonSuccessResult();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.BranchUser )]
        public async Task<ActionResult> DeletePspRating(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.PspBranchRate.FirstOrDefault(x => x.Id == id);
            _dataContext.PspBranchRate.Remove(query ?? throw new InvalidOperationException());
            await _dataContext.SaveChangesAsync(cancellationToken);
            return JsonSuccessMessage();
            
        }
        [AjaxOnly]
        [CustomAuthorize]
        public ActionResult GetPspRating()
        {


            if (User.IsBranchUser()) // اگر نقش کاربر شعبه بود فقط اطلاعات خودش را ببیند
            {
                var pspList = _dataContext.Psps.ToList();

                var result = pspList.Select(a => new
                {
                    BranchId = CurrentUserBranchId,
                    Rate = _dataContext.PspBranchRate
                        .FirstOrDefault(d => d.PspId == a.Id && d.OrganizationUnitId == CurrentUserBranchId)?.Rate ?? 100,
                    PspTitle = a.Title,
                    PspId = a.Id,
                    Description = _dataContext.PspBranchRate
                    .FirstOrDefault(d => d.PspId == a.Id && d.OrganizationUnitId == CurrentUserBranchId)?.Description ?? "",
                    BranchName = CurrentUserBranch,
                    Id = _dataContext.PspBranchRate
                        .FirstOrDefault(d => d.PspId == a.Id && d.OrganizationUnitId == CurrentUserBranchId)?.Id ?? 0,

                }).ToList();
                result = result.Where(x => x.BranchId == CurrentUserBranchId).ToList();

                return JsonSuccessResult(new {rows = result, totalRowsCount = result.Count});
                
            }
            
            

       
            
            
            var result2 = _dataContext.PspBranchRate.Include(a=>a.OrganizationUnit).Include(a=>a.Psp)
                .Select(x => new
                {
                    x.Id,
                    BranchName = x.OrganizationUnit.Title,
                    BranchId = x.OrganizationUnit.Id,
                    Rate= x.Rate,
                    x.Description,
                    PspId = x.PspId,
                    PspTitle = x.Psp.Title
                })
                .ToList();

            
           
           

            return JsonSuccessResult(new {rows = result2, totalRowsCount = result2.Count});
        }

        #endregion
        
    }
}