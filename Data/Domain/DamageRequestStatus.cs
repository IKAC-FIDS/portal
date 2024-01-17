using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("DamageRequestStatus")]
    public class DamageRequestStatus
    {
        public byte Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<DamageRequest> DamageRequest { get; set; }
    }
}