using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("Nationality")]
    public class Nationality
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public virtual ICollection<MerchantProfile> MerchantProfiles { get; set; } = new HashSet<MerchantProfile>();
    }
}