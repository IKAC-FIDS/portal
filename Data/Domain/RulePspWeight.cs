using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RulePspWeight")]

    public class RulePspWeight
    {
        [Key]
        public  int Id { get; set; }
        public  virtual  RuleType RuleType { get; set; }
        public  int RuleTypeId { get; set; }
        public  virtual  Psp Psp { get; set; }
        public  byte? PspId { get; set; }
        public  double Weight { get; set; }
    }
}