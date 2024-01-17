using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TES.Data.Domain
{
    public class InvoiceType
    {
        public byte Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}