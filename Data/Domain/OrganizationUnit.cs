using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("OrganizationUnit")]
    public class OrganizationUnit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public long? ParentId { get; set; }
        public long? CityId { get; set; }
        public bool DisableNewTerminalRequest { get; set; }
        public bool DisableWirelessTerminalRequest { get; set; }

        public virtual OrganizationUnit Parent { get; set; }
        public virtual City City { get; set; }
        public  virtual ICollection<CardRequest> CardRequests { get; set; }
        public virtual ICollection<ChangeAccountRequest> ChangeAccountRequests { get; set; } = new HashSet<ChangeAccountRequest>();
        public virtual ICollection<OrganizationUnit> Children { get; set; } = new HashSet<OrganizationUnit>();
        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
         
        public virtual ICollection<CustomerStatusResult> CustomerStatusResults { get; set; } = new HashSet<CustomerStatusResult>();

        public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
        public  virtual  ICollection<PspBranchRate> PspBranchRate { get; set; }
        public virtual ICollection<DamageRequest> DamageRequest { get; set; }
    }
}