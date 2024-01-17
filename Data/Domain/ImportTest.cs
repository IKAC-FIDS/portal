using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.ImportTest")]
    public class ImportTest
    {
        
        [Key] public string TerminalNo { get; set; }
    }
}