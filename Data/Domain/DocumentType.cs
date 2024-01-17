using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.DocumentType")]
    public class DocumentType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public bool IsRequired { get; set; }
        public int ForEntityTypeId { get; set; }
        public bool? IsForLegalPersonality { get; set; }

        public bool? SendToPsp { get; set; }
        private ICollection<MerchantProfileDocument> _merchantProfileDocuments;
        public virtual ICollection<MerchantProfileDocument> MerchantProfileDocuments
        {
            get => _merchantProfileDocuments ?? (_merchantProfileDocuments = new HashSet<MerchantProfileDocument>());
            set => _merchantProfileDocuments = value;
        }

        private ICollection<TerminalDocument> _terminalDocuments;
        public virtual ICollection<TerminalDocument> TerminalDocuments
        {
            get => _terminalDocuments ?? (_terminalDocuments = new HashSet<TerminalDocument>());
            set => _terminalDocuments = value;
        }
    }
}