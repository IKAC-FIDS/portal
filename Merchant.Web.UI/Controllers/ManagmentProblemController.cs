using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TES.Data;
using TES.Web.Core;

namespace TES.Merchant.Web.UI.Controllers
{
    public class ManagmentProblemController : Controller
    {
        private readonly AppDataContext _dataContext;
        public ManagmentProblemController(AppDataContext dataContext)
        {
            this._dataContext = dataContext;    
        }
        // GET: ManagmentProblem
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]        
        [CustomAuthorize]
     //   public Task<ActionResult> DeleteTerminal()            
        public ActionResult DeleteTerminal()            
        {
            return View();
        }


    }
}