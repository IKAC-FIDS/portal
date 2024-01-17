using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardServiceType")]
    public class CardServiceType
    {
        public  int Id { get; set; }
        public  string Type { get; set; }
        public  virtual ICollection<CardRequest> CardRequests { get; set; }
    }
}