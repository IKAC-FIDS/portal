using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RuleOrder")]

    public class RuleOrder
    {
        [Key]
        public  int Id { get; set; }
        public  virtual  RuleType RuleType { get; set; }
        public  int RuleTypeId { get; set; }
        public  bool Order { get; set; }
    }
}