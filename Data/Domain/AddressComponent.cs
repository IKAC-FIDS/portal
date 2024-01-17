using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("AddressComponent")]
    public class AddressComponent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        public byte PrefixTypeCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        public byte PriorityNumber { get; set; }
    }
}