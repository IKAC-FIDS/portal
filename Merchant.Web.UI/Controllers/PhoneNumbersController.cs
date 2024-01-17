using System.Linq;
using System.Web.Mvc;
using TES.Data;
using TES.Security;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class PhoneNumbersController : BaseController
    {
        
        private readonly AppDataContext _dataContext;

        public PhoneNumbersController(AppDataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        [HttpGet]
        public ActionResult Index()
        {
          
            return View();
            
        }
    }
}