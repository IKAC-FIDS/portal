using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.NotFoundTerminal")]
    public class NotFoundTerminal
    { 
        [Key] public int Id { get; set; }

        public string TerminalNo { get; set; }
    }
}