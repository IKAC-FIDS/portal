using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RuleType")]

    public class RuleType
    {
        [Key]
        public  int Id { get; set; }
        public string Name { get; set; }
        public  bool IsActive { get; set; }
        public virtual ICollection<RuleOrder> RuleOrders { get; set; }
        public virtual ICollection<RuleDefinition> RuleDefinitions { get; set; }

    }
}