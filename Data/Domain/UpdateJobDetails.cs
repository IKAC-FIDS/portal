using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("log.UpdateJobDetails")]
    public class UpdateJobDetails
    {
        public  int Id { get; set; }
        public  string TerminalNumber { get; set; }
        public  bool? HasError { get; set; }
        public  string ErrorMessage { get; set; }
        public virtual UpdateJob UpdateJob { get; set; }
        public  int? UpdateJobId { get; set; }
    }
}