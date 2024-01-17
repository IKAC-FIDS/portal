using System;
using System.Data.Entity;
using System.Web.Mvc;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TES.Common.Extensions;
using TES.Data;
using TES.Data.Domain;
using TES.Merchant.Web.UI.ViewModels;
using TES.Security;
using TES.Web.Core;
using TES.Web.Core.Extensions;

namespace TES.Merchant.Web.UI.Controllers
{
    public class TicketSubjectsController : BaseController
    {
        
        private readonly AppDataContext _dataContext;

        public TicketSubjectsController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        [HttpGet]
        [AjaxOnly]
        public async Task<ActionResult> GetSecondData(int subjectId, string orderByColumn,
            string orderByDirection, CancellationToken cancellationToken)
        {
            var query =_dataContext.MessageSubjects.Where(b=>b.ParentId == subjectId).AsQueryable();
          

            
           
            var ro = query
               
                .Select(x => new
                {
                    x.Id,
                  x.Title,
                    
                })
                .OrderByDescending(x => x.Id)
                
                .ToList();

            if (string.IsNullOrEmpty(orderByColumn))
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.Title,
                       
                    })
                    .ToList();
                return JsonSuccessResult(new {rows });
            }

            if (orderByDirection.Contains("DESC"))
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.Title,
                       
                    })
                    .OrderByDescending(orderByColumn)
                    .ToList();
                return JsonSuccessResult(new {rows });
            }
            else
            {
                var rows = ro.Select(x => new
                    {
                        x.Id,
                        x.Title,
                        
                    })
                    .OrderBy(orderByColumn)
                    .ToList();

                return JsonSuccessResult(new {rows });
            }
        }


        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> GetData(CancellationToken cancellationToken)
        {
            var data =   _dataContext.MessageSubjects.Where(b=>!b.ParentId.HasValue)
               
                .Select(x => new
                {
                    x.Id,
                    x.PspId,
                    x.Title,
                
                })
                .ToList();
            
            

            var psp = _dataContext.Psps.ToList();
            var result = data
                .Select(x => new
                {
                    x.Id,
                   Psp =x.PspId == 4 ? "همه" : (x.PspId == 5 ? "هیچکدام" :   psp.Where(b=>b.Id == x.PspId).FirstOrDefault()?.Title),
                    x.Title,
                    
                     
                })
                .ToList();

            return JsonSuccessResult(result);
        }
        
        [HttpGet]
        public ActionResult Manage()
        {
            var MessageSubjects = _dataContext.MessageSubjects.ToList();

            var s = MessageSubjects
                .Select(x => new
                {
                    x.Id, Title = x.Title

                });
            ViewBag.MessageSubjects = (s )
                      
                .ToSelectList(x => x.Id, x => x.Title);
            return View();
        }

        [HttpGet]
        [AjaxOnly]

        public ActionResult Create()
        {
            var ssss = _dataContext.Psps
                .Select(x => new {x.Id, Title = x.Title})
                .ToList();
            ssss.Add( new { Id = (byte) 4 , Title =  "همه"});
            ssss.Add( new { Id = (byte) 5 , Title =  "هیچکدام"});
            ViewBag.PspList = (ssss)
                .ToSelectList(x => x.Id, x => x.Title);
            return View("_Create");
        }


        [HttpGet]
        [AjaxOnly]

        public ActionResult CreateSecondSubject(int parentId)
        {

            var a = new SecondSubjectListViewModel();
            a.ParentId = parentId;
          
          return  View("_CreateSecond",a);

        }
        [HttpPost]
        [AjaxOnly]
     
        public async Task<ActionResult> CreateSecond(SecondSubjectListViewModel viewModel, CancellationToken cancellationToken)
        {
            _dataContext.MessageSubjects.Add(new MessageSubject
            {
                ParentId = viewModel.ParentId,
                Title = viewModel.Title, 
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
     
        public async Task<ActionResult> Create(MessageSubjectViewModel viewModel, CancellationToken cancellationToken)
        {
            _dataContext.MessageSubjects.Add(new MessageSubject
            {
                PspId = viewModel.PspId,
                Title = viewModel.Title, 
            });

            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
        
            
        [HttpGet]
        public async Task<ActionResult> SecondSubject(int id )
        {
            SecondSubjectListViewModel a = new SecondSubjectListViewModel();
            
            var viewModel =   _dataContext.MessageSubjects.Where(b => b.ParentId == id)
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,

                    Title = x.Title,
                }).ToList();
            a.ParentId = id;
            a.SecondSubject = viewModel;
            a.ParentTitle = _dataContext.MessageSubjects.FirstOrDefault(b => b.Id == id).Title;
            return View("_secondSubject", a);
        }

        [HttpGet]
        [AjaxOnly]
    
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.MessageSubjects
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                  
                    Title = x.Title, 
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            var ssss = _dataContext.Psps
                .Select(x => new {x.Id, Title = x.Title})
                .ToList();
            ssss.Add( new { Id = (byte) 4 , Title =  "همه"});
            ssss.Add( new { Id = (byte) 5 , Title =  "هیچکدام"});
            ViewBag.PspList = (ssss)
                .ToSelectList(x => x.Id, x => x.Title);
            
            return View("_Edit", viewModel);
        }
        
        [HttpGet]
        [AjaxOnly]
    
        public async Task<ActionResult> EditSecond(int id, CancellationToken cancellationToken)
        {
            var viewModel = await _dataContext.MessageSubjects
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                  
                    Title = x.Title, 
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_EditSecond", viewModel);
        }
        [HttpPost]
        [AjaxOnly] 
        public async Task<ActionResult> EditSecond(MessageSubjectViewModel viewModel, CancellationToken cancellationToken)
        {
            var news = await _dataContext.MessageSubjects.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
         
            news.Title = viewModel.Title; 
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly] 
        public async Task<ActionResult> Edit(MessageSubjectViewModel viewModel, CancellationToken cancellationToken)
        {
            var news = await _dataContext.MessageSubjects.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
         
            news.Title = viewModel.Title;
            news.PspId = viewModel.PspId;
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly] 
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var news = await _dataContext.MessageSubjects.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.MessageSubjects.Remove(news);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}