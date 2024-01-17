using System.Collections.Generic;
using System.Web;
using System.Web.Optimization;

namespace TES.Web.Core
{
    public class AsIsBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }

    public static class BundleConfig
    {
        private static void AddBundle(string virtualPath, bool isCss, params string[] files)
        {
            var existing = BundleTable.Bundles.GetBundleFor(virtualPath);
            if (existing != null)
            {
                return;
            }

            var newBundle = isCss ? new Bundle(virtualPath, new CssMinify()) : new Bundle(virtualPath, new JsMinify());
            newBundle.Orderer = new AsIsBundleOrderer();

            foreach (var file in files)
            {
                newBundle.Include(file);
            }

            BundleTable.Bundles.Add(newBundle);
        }

        public static IHtmlString AddScripts(string virtualPath, params string[] files)
        {
            AddBundle(virtualPath, false, files);

            return Scripts.Render(virtualPath);
        }

        public static IHtmlString AddStyles(string virtualPath, params string[] files)
        {
            AddBundle(virtualPath, true, files);

            return Styles.Render(virtualPath);
        }

        public static IHtmlString AddScriptUrl(string virtualPath, params string[] files)
        {
            AddBundle(virtualPath, false, files);

            return Scripts.Url(virtualPath);
        }

        public static IHtmlString AddStyleUrl(string virtualPath, params string[] files)
        {
            AddBundle(virtualPath, true, files);

            return Styles.Url(virtualPath);
        }
    }
}