using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("log.UpdateJob")]
    public class UpdateJob
    {
        public int Id { get; set; }
        public  string StartDateTime { get; set; }
        public  string EndDateTime { get; set; }
        public bool? HasError { get; set; }
        public string ErrorMessage { get; set; }
        public  List<UpdateJobDetails> Details { get; set; }
        public int? RowNumber { get; set; }
    }
}