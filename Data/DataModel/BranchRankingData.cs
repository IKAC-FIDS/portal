using System;

namespace TES.Data.DataModel
{
    public class BranchRankingData
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int PersianLocalYearMonth { get; set; }

        public int HighTransactionCount { get; set; }
        public int LowTransactionCount { get; set; }
        public int WithoutTransactionCount { get; set; }

        public int WirelessTerminalCount { get; set; }
        public int WithWireTerminalCount { get; set; }

        public int TotalTerminalCount => WirelessTerminalCount + WithWireTerminalCount;

        public int TotalTransactionCount { get; set; }
        public long TotalTransactionSum { get; set; }

        public int TotalTransactionCountPerPos => Convert.ToInt32(Math.Round(TotalTransactionCount / (decimal) TotalTerminalCount));
        public long TotalTransactionPricePerPos => Convert.ToInt64(Math.Round(TotalTransactionSum / (decimal)TotalTerminalCount));
    }

    public class BranchRankingDataByState
    {
        public byte Id { get; set; }
        public string Title { get; set; }
        public int PersianLocalYearMonth { get; set; }

        public int HighTransactionCount { get; set; }
        public int LowTransactionCount { get; set; }
        public int WithoutTransactionCount { get; set; }

        public int WirelessTerminalCount { get; set; }
        public int WithWireTerminalCount { get; set; }

        public int TotalTerminalCount => WirelessTerminalCount + WithWireTerminalCount;

        public int TotalTransactionCount { get; set; }
        public long TotalTransactionSum { get; set; }

        public int TotalTransactionCountPerPos => Convert.ToInt32(Math.Round(TotalTransactionCount / (decimal)TotalTerminalCount));
        public long TotalTransactionPricePerPos => Convert.ToInt64(Math.Round(TotalTransactionSum / (decimal)TotalTerminalCount));
    }
}
