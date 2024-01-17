using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace TES.Data.Domain
{
    [Table("psp.TransactionSum")]
    public class TransactionSum
    {
        public int Id { get; set; }

        [Required] [StringLength(50)] public string TerminalNo { get; set; }

        public int PersianLocalYear { get; set; }
        public int PersianLocalMonth { get; set; }
        public long SumPrice { get; set; }
        public int TotalCount { get; set; }
        public int PersianLocalYearMonth { get; set; }
        public long? BuyTransactionAmount { get; set; }
        public int? BuyTransactionCount { get; set; }
        public long? BillTransactionAmount { get; set; }
        public int? BillTransactionCount { get; set; }
        public long? ChargeTransactionAmount { get; set; }
        public int? ChargeTransactionCount { get; set; }
        public int? BalanceCount { get; set; }
    }
}