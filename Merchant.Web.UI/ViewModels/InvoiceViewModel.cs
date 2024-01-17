using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public byte InvoiceTypeId { get; set; }
        public int WirlessBankMarketerPrice { get; set; }
        public int WithWireMarketerPrice { get; set; }
        public int WirlessTesMarketerPrice { get; set; }
        public int WithWireTesMarketerPrice { get; set; }
        public int TransactionAmountForReward { get; set; }
        public decimal CoefficientReward { get; set; }
        public int MaxRewardPricePerDevice { get; set; }
        public byte MinWirlessTesMarketerCount { get; set; }
        public byte MinWithWireTesMarketerCount { get; set; }
        public byte CoefficientFine { get; set; }
        public int NotPMFinePricePerDevice { get; set; }
        public byte MinNotPMFinePercentage { get; set; }
        public int NotEMFinePricePerWorkDay { get; set; }
        public byte NotEMDelayAllowedWorkDay { get; set; }
        public int NotInstalledFinePricePerWorkDay { get; set; }
        public byte NotInstalledDelayAllowedWorkDay { get; set; }
        public int NotGetTerminalNoFinePricePerWorkDay { get; set; }
        public byte GetTerminalNoDelayAllowedWorkDay { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsConfirmed { get; set; }
        public int WirelessNotRevokePrice { get; set; }
        public int NotWirelessNotRevokePrice { get; set; }
    }
}