using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("log.UpdateWageTask")]
    public class UpdateWageTask
    {
        public int Id { get; set; }
        public  string StartDateTime { get; set; }
        public  string EndDateTime { get; set; }
        public bool? HasError { get; set; }
        public string ErrorMessage { get; set; }
        public int? RowNumber { get; set; }
        public string Date { get; set; }
        public string StackTrace { get; set; }
    }
}