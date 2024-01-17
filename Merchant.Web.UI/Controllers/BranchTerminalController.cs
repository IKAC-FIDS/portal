using EntityFramework.Extensions;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.DataModel;
using TES.Data.Domain;
using TES.Data.SearchParameter;
using TES.Merchant.Web.UI.Service;
using TES.Merchant.Web.UI.Service.Models.Parsian.NewModels;
using TES.Merchant.Web.UI.ViewModels;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;
using Enums = TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.Controllers
{
    public class BranchTerminalController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public BranchTerminalController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }


        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Manage(string commaSeparatedStatuses, CancellationToken cancellationToken)
        {
            ViewBag.StatusList = (await _dataContext.TerminalStatus
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title,
                    selectedValue: commaSeparatedStatuses?.GetCommaSeparatedValues()?.ToArray());

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.DeviceTypeList = (await _dataContext.DeviceTypes
                    .Where(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = (await _dataContext.States
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Select(x => new {x.Id, x.Title}).ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}");

            ViewBag.ParentGuildList = (await _dataContext.Guilds
                    .Where(x => !x.ParentId.HasValue && x.IsActive)
                    .OrderByDescending(x => x.IsActive)
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            var query = _dataContext.Terminals.Where(x => x.StatusId != (byte) Enums.TerminalStatus.Deleted)
                .AsQueryable();

            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            }

            if (User.IsTehranBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId == (long) Enums.City.Tehran);
            }

            if (User.IsCountyBranchManagementUser())
            {
                query = query.Where(x => x.Branch.CityId != (long) Enums.City.Tehran);
            }

            var lastTransaction = await _dataContext.TransactionSums
                .OrderByDescending(x => x.PersianLocalYear)
                .ThenByDescending(x => x.PersianLocalYearMonth)
                .Select(x => new {x.PersianLocalMonth, x.PersianLocalYear})
                .FirstOrDefaultAsync(cancellationToken);

            var transactionYears = Enumerable.Range(1395, lastTransaction.PersianLocalYear - 1394);

            var dateRanges = new List<(string, string)>();
            foreach (var transactionYear in transactionYears)
            {
                dateRanges.AddRange(Enumerable
                    .Range(1,
                        lastTransaction.PersianLocalYear == transactionYear ? lastTransaction.PersianLocalMonth : 12)
                    .Select(x => ($"{x.ToString().GetMonthName()} {transactionYear}", $"{transactionYear}/{x:00}/01")));
            }

            ViewBag.TransactionDateList = dateRanges
                .OrderByDescending(x => x.Item2)
                .ToSelectList(x => x.Item2, x => x.Item1,
                    selectedValue: new[]
                        {$"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01"});

            var vieModel = new TerminalManageViewModel
            {
                FromTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                ToTransactionDate = $"{lastTransaction.PersianLocalYear}/{lastTransaction.PersianLocalMonth:00}/01",
                CommaSeparatedStatuses = commaSeparatedStatuses,
                NeedToReformTerminalCount = await query.CountAsync(
                    x => x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                         x.StatusId == (long) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch, cancellationToken)
            };
            //var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
           //                                           && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                               || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage = message.Count(d =>
           //      d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
           //      && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                    || User.IsMessageManagerUser()));
           //  
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));
            return View(vieModel);
        }

        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> GetData(TerminalSearchParameters searchParams, string orderByColumn,
            bool retriveTotalPageCount, int page)
        {
            searchParams.IsBranchUser = User.IsBranchUser();
            searchParams.IsSupervisionUser = User.IsSupervisionUser();
            searchParams.IsTehranBranchManagment = User.IsTehranBranchManagementUser();
            searchParams.IsCountyBranchManagment = User.IsCountyBranchManagementUser();
            searchParams.CurrentUserBranchId = CurrentUserBranchId;

        var query = _dataContext.BranchTerminal
             .AsQueryable(); 

            var totalRowsCount =   query.Count( );

            var ro =   query
          
                .Select(x => new
                {
                    x.Id, 
                    x.TerminalNo  ,
            x.BranchCode  ,
            x.BranchTitle ,
            x.RevokeDate ,
            x.InstallationDate  
         
                })
                .OrderByDescending(x => x.Id)
              
                
                .ToList();

            var rows = ro.Select(x => new TerminalData 
                {
                    TerminalId = x.Id,
                    TerminalNo = x.TerminalNo,
                    BranchCode = x.BranchCode,
                    JalaliRevokeDate = x.RevokeDate.HasValue ? x.RevokeDate.ToPersianDate() : "",
                    BranchTitle = x.BranchTitle,
                    JalaliInstallationDate = x.InstallationDate.ToPersianDate(),
                }) 
                .OrderByDescending(x => x.TerminalId)
             
                .ToList();
            return JsonSuccessResult(new { rows, totalRowsCount });
        }


        [HttpGet]
        [AllowAnonymous]
        public ActionResult Test()
        {
            var pspList = _dataContext.Psps.Select(v => v.Id).ToList();

            var pspqueue = _dataContext.Terminals
                .Where(a => a.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                .GroupBy(a => a.PspId).Select(a => new
                {
                    PspId = a.Key,
                    Count = a.Count()
                }).ToList();

            var pspcount = _dataContext.Psps.Count();
            var orderedpspqueue = pspqueue.OrderByDescending(a => a.Count).ToList();

            var pashmak = orderedpspqueue.Select(a => new
            {
                PspId = a.PspId,
                Rate = (float) ((float) (a.Count) / ((float) (orderedpspqueue.IndexOf(a) + 3) * (float) pspcount)),
                indexOf = orderedpspqueue.IndexOf(a),
                Count = (a.Count)
            }).ToList();


            return Json(pashmak);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize]
        public async Task<ActionResult> Details(long terminalId, CancellationToken cancellationToken)
        {
            var ppp = _dataContext.Terminals
                .Where(x => x.Id == terminalId).FirstOrDefault();
            var viewModel = await _dataContext.Terminals
                .Where(x => x.Id == terminalId)
                .Select(x => new TerminalDetailsViewModel
                {
                    Id = x.Id,
                    Tel = x.Tel,
                    PspId = x.PspId,
                    Title = x.Title,
                    UserId = x.UserId,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    ShebaNo = x.ShebaNo,
                    TelCode = x.TelCode,
                    PostCode = x.PostCode,
                    BranchId = x.BranchId,
                    StatusId = x.StatusId,
                    PspTitle = x.Psp.Title,
                    AccountNo = x.AccountNo,
                    BatchDate = x.BatchDate,
                    CityTitle = x.City.Title,
                    MarketerId = x.MarketerId,
                    ContractNo = x.ContractNo,
                    TerminalNo = x.TerminalNo,
                    MerchantNo = x.MerchantNo,
                    RevokeDate = x.RevokeDate,
                    SubmitTime = x.SubmitTime,
                    StatusTitle = x.Status.Title,
                    BranchTitle = x.Branch.Title,
                    ErrorComment = x.ErrorComment,
                    ContractDate = x.ContractDate,
                    DeviceTypeId = x.DeviceTypeId,
                    StateTitle = x.City.State.Title,
                    MarketerTitle = x.Marketer.Title,
                    LastUpdateTime = x.LastUpdateTime,
                    Mobile = x.MerchantProfile.Mobile,
                    HomeTel = x.MerchantProfile.HomeTel,
                    DeviceTypeTitle = x.DeviceType.Title,
                    InstallationDate = x.InstallationDate,
                    IsMale = x.MerchantProfile.IsMale,
                    LastName = x.MerchantProfile.LastName,
                    SubmitterUserFullName = x.User.FullName,
                    MerchantProfileId = x.MerchantProfileId,
                    Birthdate = x.MerchantProfile.Birthdate,
                    FirstName = x.MerchantProfile.FirstName,
                    ActivityTypeTitle = x.ActivityType.Title,
                    FatherName = x.MerchantProfile.FatherName,
                    HomeAddress = x.MerchantProfile.HomeAddress,
                    GenderTitle = x.MerchantProfile.IsMale ? "مرد" : "زن",
                    HomePostCode = x.MerchantProfile.HomePostCode,
                    NationalCode = x.MerchantProfile.NationalCode,
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    IdentityNumber = x.MerchantProfile.IdentityNumber,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                    EnglishLastName = x.MerchantProfile.EnglishLastName,
                    EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                    NationalityTitle = x.MerchantProfile.Nationality.Title,
                    SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                    LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                    IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                    BlockAccountNumber = x.BlockAccountNumber,
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    BlockPrice = x.BlockPrice,
                    TopiarId = x.TopiarId,
                    StepCode = x.StepCode,
                    NewParsian = x.NewParsian,
                    CustomerCategoryId = x.CustomerCategoryId.Value,
                    StepCodeTitle = x.StepCodeTitle,
                    CustomerCategory = x.CustomerCategory.Name,
                    InstallStatus = x.InstallStatus,
                    InstallStatusId = x.InstallStatusId,
                    PreferredPspTitle = x.PreferredPsp.Title,
                    TaxPayerCode = x.TaxPayerCode,
                    OrgaNizationId = x.BranchId,
                    CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                    LegalPersonalityTitle = x.MerchantProfile.IsLegalPersonality ? "حقوقی" : "حقیقی",
                    CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                    RegionalMunicipalityTitle =
                        x.RegionalMunicipalityId.HasValue ? x.RegionalMunicipality.Title : string.Empty,
                    GuildTitle = x.Guild.ParentId.HasValue
                        ? x.Guild.Parent.Title + " / " + x.Guild.Title
                        : x.Guild.Title,
                    TerminalDocuments = x.TerminalDocuments.Select(y => new DocumentViewModel
                    {
                        Id = y.Id,
                        FileName = y.FileName,
                        DocumentTypeTitle = y.DocumentType.Title
                    }),
                    MerchantProfileDocuments = x.MerchantProfile.MerchantProfileDocuments.Select(y =>
                        new DocumentViewModel
                        {
                            Id = y.Id,
                            FileName = y.FileName,
                            DocumentTypeTitle = y.DocumentType.Title
                        })
                })
                .FirstAsync(cancellationToken);

            ViewBag.PspList = (await _dataContext.Psps
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.PspId});

            ViewBag.CustomerCategory = (await _dataContext.CustomerCategory
                    .Select(x => new {x.Id, Title = x.Name})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.PspId});


            //todo

            var pspList = _dataContext.Psps.ToList();
            var ruleList = _dataContext.RuleType.Where(a => a.IsActive
            
       
            ).Include(a => a.RuleOrders).Include(a=>a.RuleDefinitions).ToList();
            var candidatePsp = new List<(int, double)>();
            foreach (var psp in pspList)
            {
                double pspRate = 0;
                foreach (var rule in ruleList)
                {
                    if (!rule.RuleDefinitions.Any(a => (a.DeviceTypeId == 1000 && a.PspId == psp.Id)
                                                      || (a.DeviceTypeId == viewModel.DeviceTypeId &&
                                                          a.PspId == psp.Id)))
                    {
                        
                        continue;
                        
                    }
                 
                    var index = 1;

                    switch (rule.Id)
                    {
                        case 1: //Queue

                            var queue = _dataContext.Terminals
                                .Where(a => a.PspId.HasValue)
                                .GroupBy(a => a.PspId).Select(a => new
                                {
                                    PspId = a.Key,
                                    Count = a.Count(
                                        b => b.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                                }).ToList();


                            index = queue.OrderByDescending(a => a.Count).ToList()
                                .IndexOf(queue.FirstOrDefault(a => a.PspId == psp.Id)) + 1;


                            var pspruleWieght =
                                _dataContext.RulePspWeight.FirstOrDefault(a =>
                                    a.RuleTypeId == rule.Id && a.PspId == psp.Id);
                            pspRate = pspRate + (index * (pspruleWieght?.Weight ?? (100 / pspList.Count)));


                            break;
                        case 2: //Branch 

                            var branchRate = _dataContext.PspBranchRate
                                .Where(a => a.PspId == psp.Id && a.OrganizationUnitId == viewModel.OrgaNizationId)
                                .Select(a => new
                                {
                                     a.PspId,
                                      a.Rate
                                }).ToList();

                            index = branchRate.OrderBy(a => a.Rate).ToList()
                                .IndexOf(branchRate.FirstOrDefault(a => a.PspId == psp.Id)) + 1;
                            var pspruleWieght2 =
                                _dataContext.RulePspWeight.FirstOrDefault(a =>
                                    a.RuleTypeId == rule.Id && a.PspId == psp.Id);
                            pspRate = pspRate + (index * (pspruleWieght2?.Weight ?? 0.33));


                            break;
                    }
                } 
                if(pspRate !=0)
                    pspRate /= ruleList.Count  ;
                candidatePsp.Add((psp.Id, pspRate));
            }

            if (!candidatePsp.Any() || !viewModel.CustomerCategoryId.HasValue || viewModel.CustomerCategoryId.Value == 0) return PartialView("_Details", viewModel);
            {
                var orderlist = candidatePsp.OrderByDescending(a => a.Item2).ToList();
                var asd = _dataContext.CustomerCategory.FirstOrDefault(a =>
                    (a.Id == viewModel.CustomerCategoryId));
                foreach (var rated in orderlist)
                {
                    if ((asd.From <= orderlist.IndexOf(rated) + 1) && (asd.To >= orderlist.IndexOf(rated) + 1))
                    {
                                 
                                viewModel.PspId = (byte?) rated.Item1;
                    }
                }
              
              
                }


            return PartialView("_Details", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> Details(AllocatePspViewModel viewModel, CancellationToken cancellationToken)
        {
            var terminal = await _dataContext.Terminals.FirstOrDefaultAsync(
                x => x.Id == viewModel.Id && x.StatusId == (byte) Enums.TerminalStatus.New, cancellationToken);

            if (terminal == null)
            {
                return JsonWarningMessage(
                    "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' است امکان تایید یا عدم تایید دارند");
            }

            if (viewModel.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
            {
                if (terminal.StatusId != (byte) Enums.TerminalStatus.New &&
                    terminal.StatusId != (byte) Enums.TerminalStatus.NeedToReform)
                {
                    return JsonWarningMessage(
                        "تنها پایانه هایی که وضعیت آن ها 'ورود بازاریابی' یا 'نیازمند اصلاح' است امکان تایید دارند");
                }

                terminal.PspId = viewModel.PspId;
                await _dataContext.SaveChangesAsync(cancellationToken);

                return AddAcceptor(terminal.Id, terminal.PspId)
                    ? JsonSuccessMessage()
                    : JsonSuccessMessage(MessageType.Danger,
                        "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
            }

            if (viewModel.StatusId == (byte) Enums.TerminalStatus.NeedToReform)
            {
                terminal.ErrorComment = viewModel.ErrorComment;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Edit(long terminalId, CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals.Where(x => x.Id == terminalId &&
                                                          (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                                           x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                                           x.StatusId == (byte) Enums.TerminalStatus
                                                               .UnsuccessfulReturnedFromSwitch));

            if (User.IsBranchUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId);
            }

            if (User.IsSupervisionUser())
            {
                query = query.Where(x => x.BranchId == CurrentUserBranchId || x.Branch.ParentId == CurrentUserBranchId);
            }

            var viewModel = await query
                .Select(x => new MerchantDataEntryViewModel
                {
                    Tel = x.Tel,
                    Title = x.Title,
                    CityId = x.CityId,
                    TerminalId = x.Id,
                    TelCode = x.TelCode,
                    GuildId = x.GuildId,
                    Address = x.Address,
                    StatusId = x.StatusId,
                    BranchId = x.BranchId,
                    PostCode = x.PostCode,
                    AccountNo = x.AccountNo,
                    StateId = x.City.StateId,
                    MarketerId = x.MarketerId,
                    DeviceTypeId = x.DeviceTypeId,
                    EnglishTitle = x.EnglishTitle,
                    ParentGuildId = x.Guild.ParentId,
                    Mobile = x.MerchantProfile.Mobile,
                    ActivityTypeId = x.ActivityTypeId,
                    HomeTel = x.MerchantProfile.HomeTel,
                    LastName = x.MerchantProfile.LastName,
                    IsMale = x.MerchantProfile.IsMale,
                    MerchantProfileId = x.MerchantProfileId,
                    FirstName = x.MerchantProfile.FirstName,
                    Birthdate = x.MerchantProfile.Birthdate,
                    FatherName = x.MerchantProfile.FatherName,
                    HomeAddress = x.MerchantProfile.HomeAddress,
                    HomePostCode = x.MerchantProfile.HomePostCode,
                    NationalCode = x.MerchantProfile.NationalCode,
                    NationalityId = x.MerchantProfile.NationalityId,
                    ShaparakAddressFormat = x.ShaparakAddressFormat,
                    RegionalMunicipalityId = x.RegionalMunicipalityId,
                    IdentityNumber = x.MerchantProfile.IdentityNumber,
                    EnglishLastName = x.MerchantProfile.EnglishLastName,
                    EnglishFirstName = x.MerchantProfile.EnglishFirstName,
                    SignatoryPosition = x.MerchantProfile.SignatoryPosition,
                    EnglishFatherName = x.MerchantProfile.EnglishFatherName,
                    IsLegalPersonality = x.MerchantProfile.IsLegalPersonality,
                    CompanyRegistrationDate = x.MerchantProfile.CompanyRegistrationDate,
                    CompanyRegistrationNumber = x.MerchantProfile.CompanyRegistrationNumber,
                    BirthCertificateIssueDate = x.MerchantProfile.BirthCertificateIssueDate,
                    LegalNationalCode = x.MerchantProfile.LegalNationalCode,
                    AccountBranchCode = x.AccountNo.Substring(0, 4),
                    AccountCustomerNumber = x.AccountNo.Substring(9, 8),
                    BlockAccountType = x.BlockAccountNumber.Substring(0, 3),
                    BlockAccountRow = x.BlockAccountNumber.Substring(18, 3),
                    BlockDocumentDate = x.BlockDocumentDate,
                    BlockDocumentNumber = x.BlockDocumentNumber,
                    TaxPayerCode = x.TaxPayerCode
                })
                .FirstOrDefaultAsync(x => x.TerminalId == terminalId, cancellationToken);

            if (viewModel == null || viewModel.MarketerId == (long) Enums.Marketer.BankOrBranch &&
                !User.IsBranchUser() && !User.IsAcceptorsExpertUser())
            {
                return RedirectToAction("NotFound", "Error");
            }

            ViewBag.MarketerList = (await _dataContext.Marketers
                    .Select(x => new {x.Id, x.Title})
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.MarketerId});

            ViewBag.BranchList = (await _dataContext.OrganizationUnits
                    .Where(x => x.ParentId.HasValue)
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => $"{x.Id} - {x.Title}", selectedValue: new[] {viewModel.BranchId});

            ViewBag.GuildList = await _dataContext.Guilds
                .Where(x => !x.ParentId.HasValue)
                .OrderByDescending(x => x.IsActive)
                .Select(x => new GuildViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    ChildGuilds = x.Children.Select(y => new GuildViewModel.ChildGuildViewModel
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            ViewBag.ActivityTypeList = (await _dataContext.ActivityTypes.Where(b=>b.Id !=3)
                    .Select(x => new {x.Id, x.Title})
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title, selectedValue: new[] {viewModel.ActivityTypeId});

            ViewBag.NationalityList = (await _dataContext.Nationalities
                    .Select(x => new NationalityViewModel
                    {
                        Id = x.Id,
                        Title = x.Title
                    })
                    .OrderBy(x => x.Title)
                    .ToListAsync(cancellationToken))
                .ToSelectList(x => x.Id, x => x.Title);

            ViewBag.StateList = await _dataContext.States
                .Select(x => new StateViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Cities = x.Cities.Select(y => new CityViewModel
                    {
                        Id = y.Id,
                        Title = y.Title
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            ViewBag.DeviceTypeList = await _dataContext.DeviceTypes
                .Where(x => x.IsActive)
                .Select(x => new DeviceTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    BlockPrice = x.BlockPrice
                })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken);

            ViewBag.AddressComponentList = await _dataContext.AddressComponents
                .Select(x => new AddressComponentViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    PrefixTypeCode = x.PrefixTypeCode,
                    PriorityNumber = x.PriorityNumber
                })
                .OrderBy(x => x.Title)
                .ToListAsync(cancellationToken);

            ViewBag.DocumentTypeList = await _dataContext.DocumentTypes
                .Where(x => !x.IsForLegalPersonality.HasValue ||
                            x.IsForLegalPersonality == viewModel.IsLegalPersonality)
                .Select(x => new DocumentTypeViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsRequired = x.IsRequired,
                    ForEntityTypeId = x.ForEntityTypeId,
                    IsForLegalPersonality = x.IsForLegalPersonality
                })
                .OrderByDescending(x => x.IsRequired)
                .ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);

            var previouslyUploadedMerchantProfileDocuments = await _dataContext.MerchantProfileDocuments
                .Where(x => x.MerchantProfileId == viewModel.MerchantProfileId)
                .Select(x => new UploadedDocumentViewModel
                {
                    DocumentId = x.Id, DocumentTypeTitle = x.DocumentType.Title,
                    ForEntityTypeId = (long) Enums.EntityType.MerchantProfile
                })
                .ToListAsync(cancellationToken);

            var previouslyUploadedTerminalDocuments = await _dataContext.TerminalDocuments
                .Where(x => x.TerminalId == viewModel.TerminalId)
                .Select(x => new UploadedDocumentViewModel
                {
                    DocumentId = x.Id, DocumentTypeTitle = x.DocumentType.Title,
                    ForEntityTypeId = (long) Enums.EntityType.Terminal
                })
                .ToListAsync(cancellationToken);

            viewModel.PreviouslyUploadedDocuments =
                previouslyUploadedMerchantProfileDocuments.Concat(previouslyUploadedTerminalDocuments);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser)]
        public async Task<ActionResult> Edit(MerchantDataEntryViewModel viewModel, CancellationToken cancellationToken)
        {
            if (!viewModel.PostCode.IsValidPostCode())
            {
                return JsonErrorMessage("کد پستی محل پذیرنده صحیح نمی باشد");
            }

            if (string.IsNullOrEmpty(viewModel.ShaparakAddressFormat) ||
                viewModel.ShaparakAddressFormat.Split('،').Length < 2)
            {
                return JsonWarningMessage(
                    "ثبت آدرس پذیرنده الزامی می‌باشد. توجه نمایید که آدرس بایستی حداقل دارای دو بخش باشد.");
            }

            if (viewModel.BirthCertificateIssueDate >= DateTime.Today)
            {
                return JsonWarningMessage("تاریخ صدور شناسنامه بایستی کوچکتر از تاریخ امروز باشد.");
            }

            var terminal = await _dataContext.Terminals
                .Include(x => x.DeviceType)
                .FirstAsync(x => x.Id == viewModel.TerminalId &&
                                 x.StatusId != (byte) Enums.TerminalStatus.Deleted &&
                                 (x.StatusId == (byte) Enums.TerminalStatus.New ||
                                  x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                  x.StatusId == (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch),
                    cancellationToken);

            var branchLimitations = await _dataContext.CheckBranchLimitations(CurrentUserBranchId);

            var selectedDeviceTypeInfo = await _dataContext.DeviceTypes.Where(x => x.Id == viewModel.DeviceTypeId)
                .Select(x => new {x.BlockPrice, x.IsWireless}).FirstAsync(cancellationToken);

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                if (!await _dataContext.TerminalDocuments.AnyAsync(
                        x => x.TerminalId == viewModel.TerminalId &&
                             x.DocumentTypeId == (byte) Enums.DocumentType.SanadMasdoodi, cancellationToken) &&
                    viewModel.PostedFiles.Any(x =>
                        x.DocumentTypeId == (byte) Enums.DocumentType.SanadMasdoodi &&
                        !x.PostedFile.IsValidFormat(".pdf")))
                {
                    return JsonWarningMessage(
                        "لطفاً فایل سند مسدودی را انتخاب نمایید. توجه نمایید که این فایل بایستی با فرمت pdf ارسال شود.");
                }

                if (await _dataContext.Terminals.AnyAsync(
                    x =>  x.StatusId != (byte) Enums.TerminalStatus.Deleted && x.Id != viewModel.TerminalId && 
                         x.BlockDocumentNumber == viewModel.BlockDocumentNumber, cancellationToken))
                {
                    return JsonWarningMessage("شماره سند مسدودی وارد شده تکراری است.");
                }
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && !x.PostedFile.IsValidFormat(".pdf")))
            {
                return JsonWarningMessage("تنها فرمت قابل قبول برای مدارک pdf می باشد.");
            }

            if (viewModel.PostedFiles.Any(x => x.PostedFile.IsValidFile() && x.PostedFile.ContentLength >= 1070016))
            {
                return JsonWarningMessage("حجم هر کدام از مدارک ارسال شده نباید بیشتر از 1 مگابایت باشد.");
            }

            if (string.IsNullOrEmpty(viewModel.Address) || viewModel.Address.Length > 100)
            {
                return JsonWarningMessage("آدرس پذیرنده نباید کمتر از یک کاراکتر و بیشتر از 100 کاراکتر باشد");
            }

            if (!terminal.DeviceType.IsWireless && selectedDeviceTypeInfo.IsWireless && branchLimitations.Item2)
            {
                return JsonWarningMessage("امکان تغییر نوع دستگاه از ثابت به سیار برای شعبه شما غیرفعال می باشد");
            }

            AccountNumberExtensions.TryGenerateAccountNumberFromSheba(terminal.ShebaNo, out var accountNumber);

            terminal.Tel = viewModel.Tel;
            terminal.Title = viewModel.Title;
            terminal.CityId = viewModel.CityId;
            terminal.GuildId = viewModel.GuildId;
            terminal.Address = viewModel.Address;
            terminal.TelCode = viewModel.TelCode;
            terminal.PostCode = viewModel.PostCode;
            terminal.EnglishTitle = viewModel.EnglishTitle;
            terminal.DeviceTypeId = viewModel.DeviceTypeId;
            terminal.ActivityTypeId = viewModel.ActivityTypeId;
            terminal.ShaparakAddressFormat = viewModel.ShaparakAddressFormat;
            terminal.RegionalMunicipalityId = viewModel.RegionalMunicipalityId;
            terminal.TaxPayerCode = viewModel.TaxPayerCode;
            terminal.MarketerId = User.IsBranchUser() ? (int) Enums.Marketer.BankOrBranch : viewModel.MarketerId;

            if (selectedDeviceTypeInfo.BlockPrice > 0)
            {
                terminal.BlockDocumentDate = viewModel.BlockDocumentDate;
                terminal.BlockDocumentNumber = viewModel.BlockDocumentNumber;
                terminal.BlockPrice = selectedDeviceTypeInfo.BlockPrice;
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.WaitingForReview;
                terminal.BlockAccountNumber =
                    $"{accountNumber.Split('-')[0]}-{viewModel.BlockAccountType}-{accountNumber.Split('-')[2]}-{viewModel.BlockAccountRow}";
            }
            else
            {
                terminal.BlockPrice = 0;
                terminal.BlockDocumentDate = null;
                terminal.BlockAccountNumber = null;
                terminal.BlockDocumentNumber = null;
                terminal.BlockDocumentStatusId = (byte) Enums.BlockDocumentStatus.NotRegistered;
            }

            var terminalDocumentTypesToRemove = viewModel.PostedFiles
                .Where(x => x.ForEntityTypeId == (int) Enums.EntityType.Terminal && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.TerminalDocuments.RemoveRange(_dataContext.TerminalDocuments.Where(x =>
                terminalDocumentTypesToRemove.Contains(x.DocumentTypeId) && x.TerminalId == terminal.Id));

            foreach (var item in viewModel.PostedFiles.Where(x =>
                x.ForEntityTypeId == (int) Enums.EntityType.Terminal && x.PostedFile.IsValidFile()))
            {
                terminal.TerminalDocuments.Add(new TerminalDocument
                {
                    DocumentTypeId = item.DocumentTypeId,
                    FileData = item.PostedFile.ToByteArray(),
                    FileName = item.PostedFile.FileName
                });
            }

            var merchantProfile =
                await _dataContext.MerchantProfiles.FirstAsync(x => x.Id == viewModel.MerchantProfileId,
                    cancellationToken);
            merchantProfile.NationalityId = viewModel.NationalityId;
            merchantProfile.SignatoryPosition = viewModel.SignatoryPosition;
            merchantProfile.BirthCertificateIssueDate = viewModel.BirthCertificateIssueDate;

            var merchantProfileDocumentTypesToRemove = viewModel.PostedFiles
                .Where(x => x.ForEntityTypeId == (int) Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile())
                .Select(x => x.DocumentTypeId).ToList();
            _dataContext.MerchantProfileDocuments.RemoveRange(_dataContext.MerchantProfileDocuments.Where(x =>
                merchantProfileDocumentTypesToRemove.Contains(x.DocumentTypeId) &&
                x.MerchantProfileId == terminal.MerchantProfileId));

            foreach (var item in viewModel.PostedFiles.Where(x =>
                x.ForEntityTypeId == (int) Enums.EntityType.MerchantProfile && x.PostedFile.IsValidFile()))
            {
                merchantProfile.MerchantProfileDocuments.Add(new MerchantProfileDocument
                {
                    DocumentTypeId = item.DocumentTypeId,
                    FileData = item.PostedFile.ToByteArray(),
                    FileName = item.PostedFile.FileName
                });
            }

            await _dataContext.SaveChangesAsync(cancellationToken);
            var canSendTopPsp = _dataContext.DocumentTypes.Where(b => b.SendToPsp.HasValue && b.SendToPsp.Value).ToList();


            //todo for parsian
            using (var parsianService = new ParsianService())
            {
                if (terminal.NewParsian.HasValue && terminal.NewParsian.Value)
                {
                 
                    var attach =   viewModel.PostedFiles.Where(b=>
                        canSendTopPsp.Select(a=>a.Id).Contains( 
                            b.DocumentTypeId )  ).Select(b => new UploadAttachmentRequestData
                    {
                        ContentType = b.PostedFile.ContentType,
                        FileName = b.PostedFile.FileName,
                        Base64 = Convert.ToBase64String( b.PostedFile.ToByteArray())
                    }).ToList();
                    parsianService.NewAddAcceptor(terminal.Id,attach);
                    var res = parsianService
                        .UpdateStatusForRequestedTerminal(terminal.TopiarId.Value.ToString(), (int) terminal.Id).Result;
                    terminal.InstallStatus = res.InstallStatus;
                    terminal.InstallStatusId = res.InstallStatusId;
                    terminal.InstallationDate = res.InstallationDate;
                    terminal.StepCode = res.StepCode;
                    terminal.StepCodeTitle = res.StepCodeTitle;
                    terminal.ErrorComment = res.Error;
                    terminal.StatusId = (byte) Enums.TerminalStatus.NotReturnedFromSwitch;
                    var t = _dataContext.SaveChanges();
                }
            }

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(long terminalId, CancellationToken cancellationToken)
        {
            var query = _dataContext.Terminals.Where(x => x.Id == terminalId);
            Terminal terminal;

            if (User.IsAdmin())
            {
                terminal = await query.FirstOrDefaultAsync(cancellationToken);
                if (terminal.StatusId == (byte) Enums.TerminalStatus.New ||
                    terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                    terminal.StatusId == (byte) Enums.TerminalStatus.NotReturnedFromSwitch)
                {
                    terminal.StatusId = (byte) Enums.TerminalStatus.Deleted;
                    await _dataContext.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    return JsonWarningMessage(
                        "تنها وضعیت های 'ورود بازاریابی'، 'برنگشته از سوئیچ' و 'نیازمند اصلاح' قابلیت حذف دارند.");
                }

                return JsonSuccessMessage();
            }

            query = query.Where(x => x.StatusId == (byte) Enums.TerminalStatus.New);

            terminal = await query.FirstAsync(cancellationToken);
            terminal.StatusId = (byte) Enums.TerminalStatus.Deleted;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        private static readonly object ConfirmLock = new object();

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public ActionResult Confirm(long terminalId)
        {
            lock (ConfirmLock)
            {
                var terminal = _dataContext.Terminals
                    .Where(x => x.Id == terminalId && (x.StatusId == (byte) Enums.TerminalStatus.NeedToReform ||
                                                       x.StatusId == (byte) Enums.TerminalStatus
                                                           .UnsuccessfulReturnedFromSwitch))
                    .Select(x => new {x.Id, x.StatusId, x.PspId, x.ContractNo,x.MerchantProfileId})
                    .FirstOrDefault();

                if (terminal == null)
                    return JsonWarningMessage("پایانه مورد نظر یافت نشد.");

                if (!terminal.PspId.HasValue)
                    return JsonWarningMessage("برای پذیرنده انتخاب نشده است و امکان ثبت درخواست وجود ندارد.");

                bool result;
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.IranKish)
                {
                    using (var irankishService = new NewIranKishService())
                        result = irankishService.EditAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.Fanava)
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                //todo PN 
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.PardakhNovin)
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                       var rresult = pardakhtNovinService.AddAcceptor(terminalId);
                       result = rresult.Status == PardakthNovinStatus.Successed;
                    }

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }
                
                if (terminal.StatusId == (byte) Enums.TerminalStatus.NeedToReform &&
                    terminal.PspId == (byte) Enums.PspCompany.Parsian)
                {
                    using (var parsianService = new ParsianService())
                    {
                        var files = _dataContext.MerchantProfileDocuments
                            .Where(b => b.MerchantProfileId == terminal.MerchantProfileId && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()       .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String( b.FileData)
                            })
                            .ToList();
                        
                        files.AddRange(   _dataContext.TerminalDocuments
                            .Where(b => b.Id == terminal.Id && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList()  .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String( b.FileData)
                            })
                            .ToList());



                        
                        result = parsianService.NewAddAcceptor(terminalId,files);
                    }

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                    //return JsonWarningMessage("پذیرنده های ارسال شده به پارسیان قابلیت ویرایش از طریق وب سرویس را ندارند.");
                }

                if (terminal.PspId == (byte) Enums.PspCompany.Fanava &&
                    terminal.StatusId == (byte) Enums.TerminalStatus.UnsuccessfulReturnedFromSwitch)
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.EditAcceptor(terminalId);

                    return result
                        ? JsonSuccessMessage()
                        : JsonSuccessMessage(MessageType.Danger,
                            "خطایی در ارسال درخواست ثبت پذیرنده به وجود آمد. شما می توانید از طریق پنجره 'مشاهده اطلاعات کامل' خطای رخ داده را مشاهده نمایید.");
                }

                return JsonErrorMessage();
            }
        }


        [HttpGet]
        public ActionResult NewSendRevokeRequest(string TerminalNo)
        {
            using (var parsianService = new ParsianService())
            {
                var q = parsianService.NewSendRevokeRequest(1, TerminalNo, 2, 1);

                return Json(q, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult LoginTest()
        {
            using (var parsianService = new ParsianService())
            {
                var q = parsianService.Maraz();

                return Json(q, JsonRequestBehavior.AllowGet);
            }
        }

     
        [HttpGet]
        public ActionResult TerminalQueryTest(string terminalnumber)
        {
            using (var parsianService = new ParsianService())
            {
                var sd = parsianService.UpdateStatusForRegisteredTerminal("92034489", 434484);
                return Json(sd, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult QueryTest(string topidid)
        {
            using (var parsianService = new ParsianService())
            {
                var sd = parsianService.UpdateStatusForRequestedTerminal("3641964", int.Parse("434484"));
                return Json(sd, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public async Task<ActionResult> GroupConfirm()
        {
            var pspList = await _dataContext.Psps
                .Select(x => new {x.Id, x.Title})
                .ToListAsync();

            ViewBag.PspList = pspList.ToSelectList(x => x.Id, x => x.Title);

            return View("_GroupConfirm");
        }

        private static readonly object GroupConfirmLock = new object();

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.AcceptorsExpertUser)]
        public ActionResult GroupConfirm(List<long> terminalIdList, byte pspId)
        {
            lock (GroupConfirmLock)
            {
                var validTerminals = _dataContext.Terminals.Where(x =>
                    terminalIdList.Contains(x.Id) && x.StatusId == (byte) Enums.TerminalStatus.New);
                validTerminals.Update(x => new Terminal {PspId = pspId});

                if (validTerminals.Any())
                {
                    var validTerminalsIdList = validTerminals.Select(x => new 
                    {
                        x.Id,
                        x.MerchantProfileId
                    }).ToList();
                    switch (pspId)
                    {
                        case (byte) Enums.PspCompany.Fanava:
                        {
                            using (var fanavaService = new FanavaService())
                                fanavaService.AddAcceptorList(validTerminalsIdList.Select(b=>b.Id).ToList());

                            break;
                        }

                        //todo PN AddAcceptorList
                        case (byte) Enums.PspCompany.PardakhNovin:
                        {
                            using (var pardakhtNovinService = new PardakhtNovinService())
                             //   pardakhtNovinService.AddAcceptorList(validTerminalsIdList.Select(b=>b.Id).ToList());

                            break;
                        }
                        
                        case (byte) Enums.PspCompany.IranKish:
                        {
                            using (var irankishService = new NewIranKishService())
                                irankishService.AddAcceptorList(validTerminalsIdList.Select(b=>b.Id).ToList());

                            break;
                        }

                        case (byte) Enums.PspCompany.Parsian:
                        {
                            using (var parsianService = new ParsianService())
                            {
                                
                                foreach (var terminal in validTerminalsIdList)
                                {
                                    var files = _dataContext.MerchantProfileDocuments
                                        .Where(b => b.MerchantProfileId == terminal.MerchantProfileId && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                                        .ToList()    .Select(b => new UploadAttachmentRequestData
                                        {
                                            ContentType = b.ContentType,
                                            FileName = b.FileName,
                                            Base64 = Convert.ToBase64String( b.FileData)
                                        })
                                        .ToList();
                                                            
                                   files.AddRange(   _dataContext.TerminalDocuments
                                        .Where(b => b.Id == terminal.Id && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                                        .ToList()    .Select(b => new UploadAttachmentRequestData
                                        {
                                            ContentType = b.ContentType,
                                            FileName = b.FileName,
                                            Base64 = Convert.ToBase64String( b.FileData)
                                        })
                                        .ToList());
                                    
                                    parsianService.NewAddAcceptor(terminal.Id,files);
                                }
                            }

                            break;
                        }
                    }
                }

                return JsonSuccessMessage();
            }
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public ActionResult UpdateBatchDate()
        {
           //  var message = _dataContext.Messages.ToList();
           //  ViewBag.OpenMessage = message.Count(d => d.StatusId == (int) Common.Enumerations.MessageStatus.Open
           //                                           && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                               || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage = message.Count(d =>
           //      d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
           //      && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                    || User.IsMessageManagerUser()));
           //  ViewBag.InProgressMessage = message.Count(d =>
           //      d.StatusId == (int) Common.Enumerations.MessageStatus.UnderReview
           //      && (d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                    || User.IsMessageManagerUser()));
           //  
           //  var cardmessage = _dataContext.CardRequest.ToList();
           // ViewBag.ReadyForDeliverCardRequst =cardmessage.Count(d => d.StatusId ==   (int)Common.Enumerations.CardRequestStatus.ReadyForDeliver   
           //                                                        && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId
           //                                                            || User.IsCardRequestManager())); 
           //  ViewBag.InProgressCardRequstMessage =cardmessage.Count(d => d.StatusId ==  (int)Common.Enumerations.MessageStatus.UnderReview  
           //                                                              && ( d.UserId == CurrentUserId || d.ReviewerUserId == CurrentUserId                                                               
           //                                                                  || User.IsCardRequestManager()));
            return View();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> UpdateBatchDate(HttpPostedFileBase file, CancellationToken cancellationToken)
        {
            if (!file.IsValidFormat(".xlsx"))
            {
                return JsonWarningMessage("تنها فایل با پسوند .xlsx مجاز می باشد.");
            }

            var updateCommandList = new List<string>();

            using (var package = new ExcelPackage(file.InputStream))
            {
                var workSheet = package.Workbook.Worksheets.First();

                var errorMessageList = new List<string>();

                for (var rowNumber = 2; rowNumber <= workSheet.Dimension.End.Row; rowNumber++)
                {
                    try
                    {
                        var row = workSheet.Cells[rowNumber, 1, rowNumber, workSheet.Dimension.End.Column];

                        var terminalNo = row[rowNumber, 1].Text;
                        var batchDate = row[rowNumber, 2].Text.ToNullableMiladiDate();
                        updateCommandList.Add(
                            $"UPDATE psp.Terminal SET BatchDate = '{batchDate}' WHERE TerminalNo = '{terminalNo}'");
                    }
                    catch
                    {
                        errorMessageList.Add($"خطای کنترل نشده در سطر {rowNumber}");
                    }
                }

                if (errorMessageList.Any())
                {
                    return JsonWarningMessage("لطفاً خطاهای اعلام شده را بررسی نموده و مجدداً فایل را بارگذاری نمایید.",
                        data: errorMessageList);
                }
            }

            await _dataContext.Database.ExecuteSqlCommandAsync(string.Join(Environment.NewLine, updateCommandList),
                cancellationToken);

            return JsonSuccessMessage("فرآیند وارد نمودن اطلاعات پایانه ها از طریق فایل با موفقیت انجام شد.");
        }

        //[HttpPost]
        //[AjaxOnly]
        //[CustomAuthorize(DefaultRoles.Administrator)]
        //public async Task<ActionResult> ToggleVip(long terminalId)
        //{
        //    var terminal = await _dataContext.Terminals.FirstAsync(x => x.Id == terminalId);

        //    var currentBranchInstalledTerminalCount = await _dataContext.Terminals.CountAsync(x => x.BranchId == CurrentUserBranchId && x.StatusId == (byte)Enums.TerminalStatus.Installed);
        //    var currentBranchVipTerminalCount = await _dataContext.Terminals.CountAsync(x => x.BranchId == CurrentUserBranchId && x.IsVip);
        //    var maxVipTerminal = currentBranchInstalledTerminalCount * 0.1 > 10 ? 10 : currentBranchInstalledTerminalCount * 0.1;

        //    if (!terminal.IsVip && currentBranchVipTerminalCount + 1 > maxVipTerminal)
        //    {
        //        return JsonWarningMessage("شما به حداکثر میزان دستگاه های ویژه رسیده اید و امکان افزودن دستگاه بیشتر به عنوان دستگاه ویژه وجود ندارد");
        //    }

        //    terminal.IsVip = !terminal.IsVip;
        //    await _dataContext.SaveChangesAsync();

        //    return JsonSuccessMessage(terminal.IsVip ? "دستگاه به لیست دستگاه های ویژه افزوده شد" : "دستگاه از لیست دستگاه های ویژه خارج شد");
        //}

        private bool AddAcceptor(long terminalId, byte? pspId)
        {
            var result = false;
            var terminal = _dataContext.Terminals.FirstOrDefault(b => b.Id == terminalId);

            switch (pspId)
            {
                case (byte) Enums.PspCompany.Fanava:
                {
                    using (var fanavaService = new FanavaService())
                        result = fanavaService.AddAcceptor(terminalId);
                    break;
                }
                //todo PN
                case (byte) Enums.PspCompany.PardakhNovin:
                {
                    using (var pardakhtNovinService = new PardakhtNovinService())
                    {
                       var rresult = pardakhtNovinService.AddAcceptor(terminalId);
                       result = rresult.Status == PardakthNovinStatus.Successed;
                    }
                    break;
                }

                case (byte) Enums.PspCompany.IranKish:
                {
                    using (var irankishService = new NewIranKishService())
                        result = irankishService.AddAcceptor(terminalId);
                    break;
                }

                case (byte) Enums.PspCompany.Parsian:
                {
                    using (var parsianService = new ParsianService())
                    {
                        var files = _dataContext.MerchantProfileDocuments
                            .Where(b => b.MerchantProfileId == terminal.MerchantProfileId && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .ToList() .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String( b.FileData)
                            })
                            .ToList();
                                                            
                        files.AddRange(   _dataContext.TerminalDocuments
                            .Where(b => b.Id == terminal.Id && (b.DocumentType.SendToPsp.HasValue && b.DocumentType.SendToPsp.Value))
                            .Select(b => new UploadAttachmentRequestData
                            {
                                ContentType = b.ContentType,
                                FileName = b.FileName,
                                Base64 = Convert.ToBase64String( b.FileData)
                            })
                            .ToList());
                        
                        result = parsianService.NewAddAcceptor(terminalId,files);
                    }
                    break;
                }
            }

            return result;
        }
    }
}