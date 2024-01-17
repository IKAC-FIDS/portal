using System;
using ImageProcessor;
using ImageProcessor.Imaging;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using TES.Data;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    [CustomAuthorize(DefaultRoles.AcceptorsExpertUser, DefaultRoles.BranchUser, DefaultRoles.ITUser)]
    public class MerchantProfileDocumentController : Controller
    {
        private readonly AppDataContext _dataContext;

        public MerchantProfileDocumentController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetDocument(int id, int? width, int? height, CancellationToken cancellationToken)
        {
            var data = await _dataContext.MerchantProfileDocuments
                .Where(x => x.Id == id)
                .Select(x => new { x.FileName, x.FileData ,x.MerchantProfileId })
                .FirstOrDefaultAsync(cancellationToken);
         
            
            if (data.FileData == null || data.FileData.Length == 0)
            {
                return new EmptyResult();
            }

            if (!width.HasValue && !height.HasValue)
            {
                return File(data.FileData, MediaTypeNames.Image.Jpeg, data.FileName);
            }

            using (var inStream = new MemoryStream(data.FileData))
            using (var outStream = new MemoryStream())
            using (var imageFactory = new ImageFactory())
            {
                imageFactory.Load(inStream)
                    .Resize(new ResizeLayer(new Size(width.GetValueOrDefault(0), height.GetValueOrDefault(0)), ResizeMode.Crop))
                    .Save(outStream);

                return File(outStream.ToArray(), MediaTypeNames.Image.Jpeg, data.FileName);
            }
        }
    }
}