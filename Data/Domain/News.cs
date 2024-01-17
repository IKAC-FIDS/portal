using System;
using System.Collections.Generic;

namespace TES.Data.Domain
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime PublishDate { get; set; }
        public bool IsMain { get; set; }
        public virtual ICollection<NewsDocument> NewsDocuments { get; set; } = new HashSet<NewsDocument>();

    }
}