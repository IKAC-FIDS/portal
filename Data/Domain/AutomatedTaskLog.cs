using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("log.AutomatedTaskLog")]
    public class AutomatedTaskLog
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string TaskName { get; set; }

        public DateTime ExecutionTime { get; set; }

        [Required]
        [StringLength(200)]
        public string ActivityTitle { get; set; }
        
        public string Message { get; set; }
    }
}