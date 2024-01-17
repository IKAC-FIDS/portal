using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.NewTerminalWageReport")]

    public class NewTerminalWageReport
    {
        public long Id { get; set; }

        public string TerminalNo { get; set; }
        private double msubValue = 0;

        public double Value { get; set; }

        [NotMapped]
        public double subValue
        {
            get
            {



                if (Wage < 50000)
                    return 500;
                else if (Wage > 250000)
                    return 2500;
                else
                {
                    return Wage / 100;
                }


            }
            set { msubValue = value; }
        }

        [NotMapped] public double Wage { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool? IsSystemTerminal { get; set; }
        public string Psp { get; set; }
        public string AccountNo { get; set; }
    }
}