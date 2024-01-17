using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    
        [Table("dbo.ParsianRequest2")]
    public class ParsianRequest2
    {
        public long Id { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Input { get; set; }
        public string Result { get; set; }
        public int? TerminalId { get; set; }
        public  DateTime? Create { get; set; } =DateTime.Now;
    }
    [Table("dbo.ParsianRequest")]
    public class ParsianRequest
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Input { get; set; }
        public string Result { get; set; }
        public int? TerminalId { get; set; }
        public  DateTime? Create { get; set; } =DateTime.Now;
    }

    [Table("psp.PardakhtNovinRequest")]

    public class PardakhtNovinRequest
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Input { get; set; }
        public string Result { get; set; }
        public long? TerminalId { get; set; }
        public  long TrackId { get; set; }
        public  DateTime? Create { get; set; } =DateTime.Now;
      
    }

    [Table("dbo.IrankishRequest")]
    public class IrankishRequest
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Input { get; set; }
        public string Result { get; set; }
        public int? TerminalId { get; set; }
        public  DateTime? Create { get; set; } =DateTime.Now;
        public string psptrackingCode { get; set; }
        public string documentTrackingCode { get; set; }
        public string indicator { get; set; }
    }

    
}