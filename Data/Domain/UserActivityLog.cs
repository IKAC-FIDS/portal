namespace TES.Data.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UserActivityLog", Schema = "log")]
    public class UserActivityLog
    {
        public long? UserId { get; set; }

        [StringLength(300)]
        public string Address { get; set; }

        [StringLength(30)]
        public string Category { get; set; }

        [StringLength(30)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Data { get; set; }

        [StringLength(300)]
        public string UserAgent { get; set; }

        [StringLength(15)]
        public string UserIP { get; set; }

        [Key, Column(Order = 2)]
        public DateTime ActivityTime { get; set; }

        public virtual User User { get; set; }
    }
}