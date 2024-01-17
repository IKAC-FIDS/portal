using EntityFramework.Extensions;
using System;
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
    public class NewsController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public NewsController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        [CustomAuthorize(DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser)]
        public ActionResult Manage()
        {
        
            return View();
        }

        
        [HttpGet]
        [CustomAuthorize(DefaultRoles.BranchUser, DefaultRoles.Administrator, DefaultRoles.AcceptorsExpertUser, DefaultRoles.ITUser)]
        public async Task<ActionResult> GetChangeAccountRequestDocument(long changeAccountRequestId, CancellationToken cancellationToken)
        {
            var query = _dataContext.NewsDocument.Where(x => x.Id == changeAccountRequestId).FirstOrDefault();

            

           
 

         
            var fileContentType = query.FileName.Contains("mp4") ? "video/mp4" : "application/pdf";

            var f =  File(query.FileData, fileContentType, $"{query.FileName}");
            
            return f;
        }
        
        
        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Index(CancellationToken cancellationToken)
        {
            var ccc =   _dataContext.News
                .OrderByDescending(x => x.PublishDate).ToList();
            
            var viewModel = ccc
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                    Body = x.Body,
                    Title = x.Title,
                    PublishDate = x.PublishDate,
                    AttachFiles = x.NewsDocuments.Where(b=>b.NewsId == x.Id).Select(b=> new AttachFile
                    {
                        Id =b.Id,
                        Title = b.FileName
                    }).ToList()
                })
                .ToList();
            
            
        
            return View(viewModel);
        }

        [HttpGet]
        [CustomAuthorize]
        public async Task<ActionResult> Details(int id, CancellationToken cancellationToken)
        {
            
            var ccc =   _dataContext.News.Where(b=>b.Id == id)
                .OrderByDescending(x => x.PublishDate).ToList();
            
            var viewModel = ccc
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                    Body = x.Body,
                    Title = x.Title,
                    PublishDate = x.PublishDate,
                    AttachFiles = x.NewsDocuments.Where(b=>b.NewsId == x.Id).Select(b=> new AttachFile
                    {
                        Id =b.Id,
                        Title = b.FileName
                    }).ToList()
                })
                .ToList();

            
            
             
        
            return View(viewModel.FirstOrDefault());
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> GetData(CancellationToken cancellationToken)
        {
            var data = await _dataContext.News
                .OrderByDescending(x => x.PublishDate)
                .Select(x => new
                {
                    x.Id,
                    x.Body,
                    x.Title,
                    x.IsMain,
                    x.PublishDate
                })
                .ToListAsync(cancellationToken);

            var result = data
                .Select(x => new
                {
                    x.Id,
                    x.Body,
                    x.Title,
                    x.IsMain,
                    PublishDate = x.PublishDate.ToPersianDateTime()
                })
                .ToList();

            return JsonSuccessResult(result);
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public ActionResult Create() => View("_Create");
        
        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Create(NewsViewModel viewModel, CancellationToken cancellationToken)
        {



            if (viewModel.PostedFiles.Any(b=>b != null))
            {
                if (  
                    viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                   || !x.IsValidFormat(".pdf,.mp4") || x.ContentLength > 1 * 3024 * 3024))
                {
                    return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
                }
            }

            var n = new News
            {
                Body = viewModel.Body,
                Title = viewModel.Title,
                PublishDate = DateTime.Now
            };
            
            _dataContext.News.Add(n);
            
            var s = _dataContext.SaveChangesAsync(cancellationToken).Result;
            foreach (var VARIABLE in viewModel.PostedFiles)
            {
                NewsDocument nd = new NewsDocument();
                nd.NewsId = n.Id;
                nd.FileData = VARIABLE.ToByteArray();
                 nd.FileName = VARIABLE?.FileName;
                 n.NewsDocuments.Add(nd);
            }
            s = _dataContext.SaveChangesAsync(cancellationToken).Result;
            return JsonSuccessMessage();
        }

        [HttpGet]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            
            var viewModel = await _dataContext.News
                .Select(x => new NewsViewModel
                {
                    Id = x.Id,
                    Body = x.Body,
                    Title = x.Title,
                    PublishDate = x.PublishDate
                })
                .FirstAsync(x => x.Id == id, cancellationToken);

            return View("_Edit", viewModel);
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Edit(NewsViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel.PostedFiles.Any(b=>b != null))
            {
                if (     viewModel.PostedFiles.Any(x => !x.IsValidFile()
                                                        || !x.IsValidFormat(".pdf,.mp4") || x.ContentLength > 1 * 3024 * 3024))
                {
                    return JsonErrorMessage("فرمت یا حجم یکی از فایل های وارد شده قابل قبول نمی باشد");
                }
            }

            
            var news = await _dataContext.News.FirstAsync(x => x.Id == viewModel.Id, cancellationToken);
            news.Body = viewModel.Body;
            news.Title = viewModel.Title;
            news.PublishDate = DateTime.Now;
            await _dataContext.SaveChangesAsync(cancellationToken);

            // remove other file =>
            var nds = _dataContext.NewsDocument.Where(b => b.NewsId == news.Id).ToList();
            _dataContext.NewsDocument.RemoveRange(nds);
            var result =  _dataContext.SaveChangesAsync(cancellationToken).Result;

            
            foreach (var variable in viewModel.PostedFiles)
            {
                NewsDocument nd = new NewsDocument();
                nd.NewsId = news.Id;
                nd.FileData = variable.ToByteArray();
                nd.FileName = variable?.FileName;
                news.NewsDocuments.Add(nd);
            }
           var s = _dataContext.SaveChangesAsync(cancellationToken).Result;
            
            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var news = await _dataContext.News.FirstAsync(x => x.Id == id, cancellationToken);
            _dataContext.News.Remove(news);
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }

        [HttpPost]
        [AjaxOnly]
        [CustomAuthorize(DefaultRoles.Administrator)]
        public async Task<ActionResult> ChangeStatus(int id, CancellationToken cancellationToken)
        {
            await _dataContext.News.UpdateAsync(x => new News { IsMain = false });
            await _dataContext.News.Where(x => x.Id == id).UpdateAsync(x => new News { IsMain = true });
            await _dataContext.SaveChangesAsync(cancellationToken);

            return JsonSuccessMessage();
        }
    }
}