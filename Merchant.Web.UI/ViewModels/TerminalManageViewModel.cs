namespace TES.Merchant.Web.UI.ViewModels
{
    public class TerminalManageViewModel
    {
        public string CommaSeparatedStatuses { get; set; }
        public int NeedToReformTerminalCount { get; set; }
        public string FromTransactionDate { get; set; }
        public string ToTransactionDate { get; set; }
        public string FromWageTransactionDate { get; set; }
        public string ToWageTransactionDate { get; set; }
        public bool InActive { get; set; }
        public bool LowTransaction { get; set; }
        public bool TwoMonthInActive { get; set; }
        public bool ThreeMonthInActive { get; set; }
    }

    public class BranchConnectorViewModel
    {
        public long Id { get; set; }
        
        public  int OrganizationUnitId { get; set; }
        public  string FirstName { get; set; }
        public  string LastName { get; set; }
        public  string PhoneNumber { get; set; }
    }
}