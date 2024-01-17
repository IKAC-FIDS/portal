using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TES.Data.Domain
{

    [Table("dbo.NormalRep")]
    public class NormalRep
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [StringLength(50)]
        public string TerminalNum { get; set; }
        [StringLength(50)]
        public string SerialDevice { get; set; }
        public string OwnerDevice { get; set; }
        [StringLength(2000)]
        public string Address { get; set; }
        public long BranchId { get; set; }
        [StringLength(500)]
        public string AccountNo { get; set; }
        [StringLength(50)]
        public string ShebaNo { get; set; }
        [StringLength(50)]
        public string ContractNo { get; set; }
        [StringLength(2000)]
        public string City1 { get; set; }
        [StringLength(50)]
        public string TelCode { get; set; }
        [StringLength(50)]
        public string Mobile { get; set; }
        [StringLength(2000)]
        public string Ostan { get; set; }
        public long DeviceTypeId { get; set; }
        [StringLength(50)]
        public string CardType1 { get; set; }
        public string Marker { get; set; }
        [StringLength(50)]
        public string NationalCode { get; set; }
        [StringLength(1000)]
        public string StoreManager { get; set; }
        [StringLength(50)]
        public string Statuse { get; set; }
        [StringLength(500)]
        public string Class { get; set; }
        [StringLength(50)]
        public string AcceptorNo { get; set; }
        public string PSP { get; set; }
        [StringLength(50)]
        public string CustomerNo { get; set; }
        public string Market { get; set; }

    }
}
