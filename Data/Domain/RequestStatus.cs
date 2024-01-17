using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RequestStatus")]
    public class RequestStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        public virtual ICollection<ChangeAccountRequest> ChangeAccountRequests { get; set; } = new HashSet<ChangeAccountRequest>();
        public virtual ICollection<RevokeRequest> RevokeRequests { get; set; } = new HashSet<RevokeRequest>();
    }
}