using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.CompanyMonthlyTask")]
    public class CompanyMonthlyTask
    {
        public long Id { get; set; }
        public int PersianYear { get; set; }
        public int PersianMonth { get; set; }
        public bool SmsIsSent { get; set; }
        public int PortalDownTime { get; set; }
    }
}