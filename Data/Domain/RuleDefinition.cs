using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RuleDefinition")]

    public class RuleDefinition
    {
        [Key]
        public  int Id { get; set; }
        public  virtual  RuleType RuleType { get; set; }
        public  int RuleTypeId { get; set; }
        public  virtual  Psp Psp { get; set; }
        public  byte PspId { get; set; }     
        public  virtual  DeviceType  DeviceType { get; set; }

        public  int DeviceTypeId { get; set; } // 1000 means all device
        public byte[] FileData { get; set; }
        public  string Description { get; set; }
    }
}