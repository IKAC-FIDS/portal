using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class GuildViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public IEnumerable<ChildGuildViewModel> ChildGuilds { get; set; }

        public class ChildGuildViewModel
        {
            public long Id { get; set; }
            public string Title { get; set; }
        }
    }
}