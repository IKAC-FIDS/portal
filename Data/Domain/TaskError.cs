using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("log.TaskError")]
    public class TaskError
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        
        public  string TaskName { get; set; }
        public  string Exception { get; set; }
        public  DateTime Date  { get; set; }
        public string HelpLink { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
    }
}