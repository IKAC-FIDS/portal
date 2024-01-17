namespace TES.Merchant.Web.UI.Service.Models.Parsian
{
    public class WsPosRequest
    {
        public int RequestCode { get; set; }
        public int TermCount { get; set; }
        public string COCU_NAME { get; set; }
        public string COCU_FAMILY { get; set; }
        public string COCU_NAME_ENG { get; set; }
        public string COCU_FAMILY_ENG { get; set; }
        public string COCU_FATHER_NAME { get; set; }
        public string COCU_ID_CARD_NO { get; set; }
        public short COCU_SEX_CODE { get; set; }
        public int COCU_ISSUE_CODE { get; set; }
        public int COCU_BIRTH_DATE { get; set; }
        public string COCU_Economic_National_Code { get; set; }
        public string COCU_Mobile { get; set; }
        public string COCU_ConcatNAME { get; set; }

        public short COUNTRY_CODE { get; set; }
        public short STATE_CODE { get; set; }
        public int CITY_CODE { get; set; }
        public short CoMC_City_Part_Code { get; set; }
        public string COMC_STOR_NAME { get; set; }
        public string COMC_STOR_NAMEL { get; set; }
        public string COMC_CUS_WORK_POSTCODE { get; set; }
        public short ACC_STATEMENT_CODE { get; set; }
        public short ACC_TYPE_CODE { get; set; }
        public short BAN_BANK_CODE { get; set; }
        public short BAN_ZONE_CODE { get; set; }
        public short BAN_BRANCH_CODE { get; set; }
        public string Comc_Bank_IBAN { get; set; }
        public string COMC_Bank_Acc { get; set; }
        public short COMC_STTLMNT_CODE { get; set; }
        public string ShaparakTermGroup { get; set; }
        public int CoCa_City_Code { get; set; }
        public string CoCa_Pos_Box { get; set; }
        public string CoCa_Address { get; set; }
        public string CoCa_Tel { get; set; }
        public string CoCa_Fax { get; set; }
        public string CoCa_Tel2 { get; set; }
        public string CoCa_AddressCode { get; set; }
        public string Sign_First_Name { get; set; }
        public string Sign_Last_Name { get; set; }
        public string Sign_National_Code { get; set; }
        public string Sign_Position { get; set; }
        public int TermModel { get; set; }
        public string OrganizationId { get; set; }
        public int SignBirthDate { get; set; }
        public short CustomerType { get; set; }
    }
}