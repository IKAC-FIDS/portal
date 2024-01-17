using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.ChangeAccountRequest")]
    public class ChangeAccountRequest
    {
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNo { get; set; }

        public long BranchId { get; set; }

        [Required]
        [StringLength(50)]
        public string ShebaNo { get; set; }

        public DateTime SubmitTime { get; set; }

        public long UserId { get; set; }

        public byte StatusId { get; set; }

        public string TerminalNo { get; set; }

        /// <summary>
        /// فقط برای نگه داشتن شماره پیگیری درخواست های تغییر حساب پارسیان کاربرد دارد
        /// </summary>
        public long? RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string CurrentAccountNo { get; set; }

        public byte[] FileData { get; set; }

        [StringLength(3000)]
        public string Result { get; set; }

        public virtual OrganizationUnit Branch { get; set; }

        public virtual User User { get; set; }

        public virtual RequestStatus Status { get; set; }
        public string Error { get; set; }
        public string TopiarId { get; set; }
        public string  PardakhtNovinTrackId { get; set; }
    }
}