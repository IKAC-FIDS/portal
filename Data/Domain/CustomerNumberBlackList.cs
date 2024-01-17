using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.CustomerNumberBlackList")]
    public class CustomerNumberBlackList
    {
        public int Id { get; set; }
        public string CustomerNumber { get; set; }
        public DateTime SubmitTime { get; set; }
        public long SubmitterUserId { get; set; }
        public string Description { get; set; }

        public virtual User SubmitterUser { get; set; }
    }
}
