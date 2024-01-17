using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.PspBranchRate")]
    public class PspBranchRate
    {
        [Key]
        public  int Id { get; set; }
        public  virtual OrganizationUnit  OrganizationUnit { get; set; }
        public  virtual  Psp  Psp { get; set; }
        public byte PspId { get; set; }
        
        public long? OrganizationUnitId { get; set; }
        public  double Rate { get; set; }
        
        public  string Description { get; set; }

    }
}