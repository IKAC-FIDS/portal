﻿using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class CardRequestDocumentController : BaseController
    {
        private readonly AppDataContext _dataContext;

        public CardRequestDocumentController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet]
        public async Task<ActionResult> GetDocument(int id, CancellationToken cancellationToken)
        {
            var query = _dataContext.CardRequestDocuments.Where(x => x.Id == id);

            if (!User.IsAdmin() && !User.IsMessageManagerUser())
            {
                if (User.IsAcceptorsExpertUser())
                {
                    query = query.Where(x => x.CardRequest.ReviewerUserId == CurrentUserId);
                }
                else
                {
                    query = query.Where(x => x.CardRequest.UserId == CurrentUserId);
                }
            }

            var data = await query.Select(x => new { x.FileName, x.FileData }).FirstOrDefaultAsync(cancellationToken);

            if (data == null || data.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            return File(data.FileData, "application/octet-stream", data.FileName);
        }
    }
}