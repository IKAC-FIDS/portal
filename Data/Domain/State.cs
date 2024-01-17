using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("State")]
    public class State
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();
        public virtual ICollection<RegionalMunicipality> RegionalMunicipalities { get; set; } = new HashSet<RegionalMunicipality>();
    }
}