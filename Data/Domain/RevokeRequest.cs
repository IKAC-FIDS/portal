using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace TES.Data.Domain
{
    [Table("psp.RevokeRequest")]
    public class RevokeRequest
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public byte ReasonId { get; set; }
        public byte? SecondReasonId { get; set; }

        [StringLength(500)]
        public string DeliveryDescription { get; set; }

        public byte StatusId { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime SubmitTime { get; set; }

        [NotMapped]
        public string JalaliSubmitTime
        {
            get
            {   PersianCalendar pc = new PersianCalendar();
                DateTime thisDate = DateTime.Now;
                return $"{pc.GetYear(thisDate)}/{pc.GetMonth(thisDate)}/{pc.GetDayOfMonth(thisDate)}";
            }
        }
        public int JalaliYearSubmitTIme
        {
            get
            {   PersianCalendar pc = new PersianCalendar();
             
                return pc.GetYear(SubmitTime);
            }
        }
        public int JalaliMonthSubmitTIme
        {
            get
            {   PersianCalendar pc = new PersianCalendar(); 
                return pc.GetMonth(SubmitTime);
            }
        }

        public long UserId { get; set; }

        [StringLength(3000)]
        public string Result { get; set; }

        public virtual User User { get; set; }
        public virtual RequestStatus Status { get; set; }
        public virtual RevokeReason Reason { get; set; }
        public virtual RevokeReason SecondReason { get; set; }
        public int? PardakhtNovinSaveId { get; set; }
    }
}