using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardTemplate")]
    public class CardTemplate
    {
        
        public  int Id { get; set; }
        public  string ImageName { get; set; }
        public  string Code { get; set; }
        
    }
}