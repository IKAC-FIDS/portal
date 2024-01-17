using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.DeviceType")]
    public class DeviceType
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsWireless { get; set; }

        public int BlockPrice { get; set; }

        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
    }
}