using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.StatusSituation")]
    public class StatusSituation
    {
        public long Id { get; set; }
        public long BaSimMansoobe { get; set; }
        public long BiSimMansoobe { get; set; }
        public long KamFaalBaSim { get; set; }
        public long KamFaalBiSim { get; set; }
        public long NimeFaalBaSim { get; set; }
        public long NimeFaalBiSim { get; set; }
        public long FaalBaSim { get; set; }
        public long FaalBiSim { get; set; }
        public long PadashBazaryabi { get; set; }
        public long BaSimPmNashode { get; set; }
        public long BiSimPmNashode { get; set; }
        public long BaSimEmNashode { get; set; }
        public long BiSimEmNashode { get; set; }
        public long TakhirDarNasbBaSim { get; set; }
        public long TakhirDarNasbBiSim { get; set; }
        public int PersianMonth { get; set; }
        public int PersianYear { get; set; }
    }
}