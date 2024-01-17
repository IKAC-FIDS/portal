using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class BankInvoiceViewModel
    {
        public class IncomeDataItem
        {
            public int BankWirelessCount { get; set; }
            public long BankWirelessPrice { get; set; }
            public int BankWithWireCount { get; set; }
            public long BankWithWirePrice { get; set; }

            public int CompanyActiveWirelessCount { get; set; }
            public long CompanyActiveWirelessPrice { get; set; }
            public int CompanyActiveWithWireCount { get; set; }
            public long CompanyActiveWithWirePrice { get; set; }

            public int CompanyHalfActiveWirelessCount { get; set; }
            public long CompanyHalfActiveWirelessPrice { get; set; }
            public int CompanyHalfActiveWithWireCount { get; set; }
            public long CompanyHalfActiveWithWirePrice { get; set; }

            public int CompanyLowActiveWirelessCount { get; set; }
            public long CompanyLowActiveWirelessPrice { get; set; }
            public int CompanyLowActiveWithWireCount { get; set; }
            public long CompanyLowActiveWithWirePrice { get; set; }

            public int CompanyInactiveWirelessCount { get; set; }
            public long CompanyInactiveWirelessPrice { get; set; }
            public int CompanyInactiveWithWireCount { get; set; }
            public long CompanyInactiveWithWirePrice { get; set; }
        }

        public class RewardDataItem
        {
            public int ActiveWirelessCount { get; set; }
            public long ActiveWirelessRewardPrice { get; set; }
            public int ActiveWithWireCount { get; set; }
            public long ActiveWithWireRewardPrice { get; set; }
        }

        public class InstalDelayFineDataItem
        {
            public int WirelessCount { get; set; }
            public int WithWireCount { get; set; }
            public int WirelessDaysCount { get; set; }
            public int WithWireDaysCount { get; set; }
            public long WirelessFinePrice { get; set; }
            public long WithWireFinePrice { get; set; }
        }

        public class EmFineDataItem
        {
            public int WirelessCount { get; set; }
            public int WithWireCount { get; set; }
            public int WirelessDaysCount { get; set; }
            public int WithWireDaysCount { get; set; }
            public long WirelessFinePrice { get; set; }
            public long WithWireFinePrice { get; set; }
        }

        public class PmFineDataItem
        {
            public int WirelessCount { get; set; }
            public int WithWireCount { get; set; }
            public int WirelessPmCount { get; set; }
            public int WithWirePmCount { get; set; }
            public int WirelessNotPmCount { get; set; }
            public int WithWireNotPmCount { get; set; }
            public long WirelessFinePrice { get; set; }
            public long WithWireFinePrice { get; set; }
            public decimal PmPercent { get; set; }
        }

        public class MarketingFineDataItem
        {
            public int WirelessCount { get; set; }
            public int WithWireCount { get; set; }
            public long WirelessFinePrice { get; set; }
            public long WithWireFinePrice { get; set; }
        }

        public class CompanyMonthlyTaskFineDataItem
        {
            public long TotlaIncome { get; set; }
            public bool SmsIsSent { get; set; }
            public int PortalDownTime { get; set; }
            public long SmsFinePrice { get; set; }
            public long PortalDownTimeFinePrice { get; set; }
        }

        public BankInvoiceViewModel()
        {
            IncomeData = new IncomeDataItem();
            RewardData = new RewardDataItem();
            InstalDelayFineData = new InstalDelayFineDataItem();
            EmFineData = new EmFineDataItem();
            MarketingFineData = new MarketingFineDataItem();
        }

        public IncomeDataItem IncomeData { get; set; }
        public RewardDataItem RewardData { get; set; }
        public InstalDelayFineDataItem InstalDelayFineData { get; set; }
        public EmFineDataItem EmFineData { get; set; }
        public PmFineDataItem PmFineData { get; set; }
        public CompanyMonthlyTaskFineDataItem CompanyMonthlyTaskFineData { get; set; }
        public MarketingFineDataItem MarketingFineData { get; set; }
    }
}