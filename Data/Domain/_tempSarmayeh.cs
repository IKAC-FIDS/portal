using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp._tempSarmayeh")]
     
    public class _tempSarmayeh
    {
        public string TerminalNo { get; set; }
        public  string NationalCode { get; set; }
        public string Sheba { get; set; }
        public  bool? IsLegal { get; set; }
        public  string SabtCode { get; set; }
        public  string SabdDate { get; set; }
        public  string IdentityCode { get; set; }
        public  string Serial { get; set; }
        public  string SeriNumber { get; set; }
        public  string SeriLetter { get; set; }
        public bool InComplete { get; set; }
    }
    [Table("dbo._damage")]
     
    public class _damage
    {
        public string title { get; set; }
        public  string body { get; set; }
        public string amount { get; set; }
        public string branch { get; set; }
        public string terminalId { get; set; }
        
    }

    
    
}