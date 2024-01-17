using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.WageTransaction")]
    public class WageTransaction
    {
        public long Id { get; set; }
        public string RRN { get; set; }
        public string RowNumber { get; set; }
        public bool? HasError { get; set; }
        public string TerminalNo { get; set; }
        public double WageValue { get; set; }
        public string Error { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Sheba { get; set; }

        private double msubValue = 0;

        public double subValue
        {
            get
            {



                if (WageValue < 50000)
                    return 500;
                else if (WageValue > 250000)
                    return 2500;
                else
                {
                    return WageValue / 100;
                }


            }
            set { msubValue = value; }
        }
    }
}