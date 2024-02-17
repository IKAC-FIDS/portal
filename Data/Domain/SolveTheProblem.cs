using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TES.Data.Domain
{
    [Table("psp.SolveTheProblem")]
    public class SolveTheProblem
    {
        [Key] 
        public long SPId { get; set; }
        public string SPDate { get; set; }
        public string PSP { get; set; }
        public string description { get; set; }
        public string RequestBy { get; set; }
    }


}
