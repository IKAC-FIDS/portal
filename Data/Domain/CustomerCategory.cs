using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.CustomerCategory")]
    public class CustomerCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        
        public  int From { get; set; }
        public  int To { get; set; }
      
    }
}