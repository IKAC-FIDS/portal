using System;

namespace TES.Data.DataModel
{
    public class TerminalData
    {
        public long TerminalId { get; set; }
        public long MerchantProfileId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public string StatusTitle { get; set; }
        public string MerchantNo { get; set; }
        public bool? IsActive { get; set; }
        public double?    TransactionValue{ get; set; }
        public int? TransactionCount { get; set; }
        public int InstallDelayDays { get; set; }
        public string TerminalNo { get; set; }
        public string TerminalTitle { get; set; }
        public string FullName { get; set; }
        public string NationalCode { get; set; }
        public string AccountNo { get; set; }
        public string CityTitle { get; set; }
        public string StateTitle { get; set; }
        public string PspTitle { get; set; }
        public byte? PspId { get; set; }
        public string SubmitterUserFullName { get; set; }
        public long MarketerId { get; set; }
        public long SumOfTransactions { get; set; }
        public int CountOfTransactions { get; set; }
        public string ContractNo { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string JalaliInstallationDate { get; set; }
        public byte? TransactionStatus { get; set; }
        public  int? TopiarId { get; set; }
        public bool? IsGood { get; set; }
        public double? IsGoodValue { get; set; }
        public int? IsGoodYear { get; set; }
        public int? IsGoodMonth { get; set; }
        public long BranchCode { get; set; }
        public string BranchTitle { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string JalaliRevokeDate { get; set; }

        public bool? LowTransaction { get; set; }
        //public bool IsVip { get; set; }
    }
}