using System.Web;
using System.Web.Mvc;

namespace TES.Merchant.Web.UI.Functions
{
    public static class HtmlExtensions
    {
        public static HtmlString ConvertToJson(this HtmlHelper htmlHelper, object model, bool escapeHtml = false)
        {
            return new HtmlString(string.Empty);
        }
    }
}