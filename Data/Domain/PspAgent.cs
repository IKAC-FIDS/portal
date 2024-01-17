using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.PspAgent")]
    public class PspAgent
    {
        public long Id { get; set; }

        [Required]
        public byte PspId { get; set; }

        [Required]
        [StringLength(500)]
        public string Title { get; set; }

        [StringLength(200)]
        public string CityName { get; set; }

        [Required]
        [StringLength(4000)]
        public string Address { get; set; }

        [Required]
        [StringLength(500)]
        public string Tel { get; set; }

        [Required]
        [StringLength(500)]
        public string EmergencyTel { get; set; }

        public virtual Psp Psp { get; set; }
    }
}