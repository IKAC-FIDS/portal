using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("Storage")]
    public class Storage
    {
        public  int Id { get; set; }
      
        public  bool IsCard { get; set; }
        public  int Value { get; set; }  
         
        public  string Design  { get; set;}
        public  string Title { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string Code { get; set; }
        public int? Waste { get; set; }
    }
}