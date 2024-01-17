using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardRequestStatus")]
    public class CardRequestStatus
    {
        public byte Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<CardRequest> Messages { get; set; }
    }
}