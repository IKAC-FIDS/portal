using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("BranchConnector")]
    public class BranchConnector
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public virtual  OrganizationUnit OrganizationUnit { get; set; }
        public  long OrganizationUnitId { get; set; }
        public  string FirstName { get; set; }
        public  string LastName { get; set; }
        public  string PhoneNumber { get; set; }
    }
    [Table("City")]
    public class City
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public byte StateId { get; set; }

        public virtual State State { get; set; }

        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
        public virtual ICollection<OrganizationUnit> OrganizationUnits { get; set; } = new HashSet<OrganizationUnit>();
    }
}