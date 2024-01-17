using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("StorageLog")]
    public class StorageLog
    {
        public  int Id { get; set; }
        public int Value { get; set; }
        public bool Add { get; set; }
        public string Date { get; set; }
        public string User { get; set; }
        public  int StorageId { get; set; }
        public string UserId { get; set; }
    }
}