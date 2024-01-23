using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class GetRequestListResponse : BaseResponse
    {
        public  int TotalRows { get; set; }
        public List<GetRequestListResponseData> Data { get; set; }
    }

    public class UpdateRequestByFollowUpCodeResponse : BaseResponse
    {
        public  int SavedID { get; set; }

    }
    public class   AddPosReplacementResponse : BaseResponse
    {
    public  int SavedID { get; set; }
    }
    public class BindSerialToSwitchResponse : BaseResponse
    {
        public  int SavedID { get; set; }
    }

    public class AddInstallationRollbackResponse : BaseResponse
    {
        public  int SavedID { get; set; }

    }

    public class AddEditRequestResponse : BaseResponse
    {
        public  int SavedID { get; set; }

    }
    public class AddAccountChangeResponse : BaseResponse
    {
        public  int SavedID { get; set; }

    }
    public class GetPosReplacementDetailResponse : BaseResponse
    {
        public GetPosReplacementDetailResponseData  Data { get; set; }

    }

    public class GetAccountChangeDetailResponseData
    {
        public  int AccountChangeID { get; set; }
                public  string Request { get; set; }
                public  string TerminalID { get; set; }
                public  string MerchantID { get; set; }
                public  string NewBank { get; set; }
                public  string NewBranch { get; set; }
                public  string NewBranchCode { get; set; }
                public  string PreAccountNumber { get; set; }
                public  string NewAccountNumber { get; set; }
                public  string PreShabaCode { get; set; }
                public  string NewShabaCode { get; set; }
                public  string LetterImage { get; set; }
                public  string SecondLetterImage { get; set; }
                public  string WorkFlowValue { get; set; }
                public  string WorkFlowCaption { get; set; }
                public  string FlowMessage { get; set; } 

    }
    public class GetAccountChangeDetailResponse : BaseResponse
    {
        public GetAccountChangeDetailResponseData  Data { get; set; }

    }

    public class GetInstallationRollbackResponse : BaseResponse
    {
        public  GetInstallationRollbackResponseData  Data { get; set; }
 
    }

    public class GetInstallationRollbackResponseData
    {
        public  string BankFollowUpCode { get; set; }
        public  string Request { get; set; }
        public  string CustomerName { get; set; }
        public  string CustomerCode { get; set; }
        public  string StateName { get; set; }
        public  string CityName { get; set; }
        public  string WorkTitle { get; set; }

        
        public  string TerminalID { get; set; }
        public  string MerchantID { get; set; }
        public  string PortType { get; set; }
        public  string PosType { get; set; }

        public  string PosModel { get; set; }
        public  string RollbackReason { get; set; }
        public  string PosSerialNumber { get; set; }

        public string FlowMessage { get; set; }
        public string WorkFlowCaption { get; set; }
        public string WorkFlowValue { get; set; }
        public  string SwitchRollBackDate { get; set; }
        public  string RollBackDate { get; set; }
    }
    public class GetEditRequestResponse : BaseResponse
    {
        public  GetEditRequestResponseData  Data { get; set; }

    }

    public class GetEditRequestResponseData
    {
        public  string Request { get; set; }
        public  string MerchantID {get;set;}
        public  string TerminalID{ get; set; }
        public  string ChangeTypeID { get; set; }
        public  string NewCityShaparakCode { get; set; }
        
        
        public  string NewPostalCode { get; set; }
        public  string NewShaparakAddressText  { get; set; }
        public  string NewPhoneNumber { get; set; }
        
        public  string NewMobile { get; set; }
        public  string NewWorkTitle { get; set; }
        public  string NewWorkTitleEng { get; set; }
        public  string NewMobileGPRS { get; set; }
        public  int NewOwneringTypeID { get; set; }
        public  int NewWorkTypeID { get; set; }
        public  string NewRentContractNo { get; set; }
        public  DateTime NewRentEndingDate { get; set; }
        public  string NewCityZone { get; set; }
        public  string NewGuildSupplementaryCode  { get; set; }
        public  string NewActivityLicenseNumberReferenceName  { get; set; }
        
        public int EditRequestID { get; set; }
        public string FlowMessage { get; set; }
        
        public string WorkFlowCaption { get; set; }
        public string WorkFlowValue { get; set; }

        public  string NewActivityLicenseNumber  { get; set; }
        public  string NewShaparakAddressEng   { get; set; }
        
        
        public  DateTime NewBusinessLicenseRegisterDate   { get; set; }
        public  DateTime  NewBusinessLicenseEndDate { get; set; }
    }

    public class GetPosReplacementDetailResponseData
    {
        public  int PosReplacementID { get; set; }
        public  string Request { get; set; }
        public  string TerminalID { get; set; }
        public  string MerchantID { get; set; }
        public  string NewPosType { get; set; }
        public  string NewPosModel { get; set; }
        public  string SerialNumber { get; set; }
        public  string NewSerialNumber { get; set; }
        public  string ReplacementReason { get; set; }
        public  string PosStatus { get; set; }
        public  string WorkFlowValue { get; set; }
        public  string WorkFlowCaption { get; set; }
        public  string FlowMessage { get; set; }

    }

    public class UpdateDocumentResponse : BaseResponse
    {
        public  int SaveID { get; set; }      
    }
    public class GetRequestListResponseData
    {
        public  string AccountNumber { get; set; }
        public  string AccountShabaCode { get; set; } 
        public  string ActivityLicenseNumberReferenceName { get; set; } 
        public  string Branch { get; set; } 
        public  string BranchCode { get; set; } 
        public  string City { get; set; } 
        public  string CustomerName { get; set; } 
        public  string CustomerType { get; set; } 
        public  string Description { get; set; } 
        public  string FaxNumber { get; set; } 
        public  string FollowupCode { get; set; } 
        public  string Guild { get; set; } 
        public  bool ISForeignNationals { get; set; } 
        public  string MainCustomer { get; set; } 
        public  string WorkTitle { get; set; } 
        public  string WorkTitleEng { get; set; } 
        public  int WorkType { get; set; } 
        public  string TerminalID { get; set; } 
        public  string StateName { get; set; } 
        public  string StateCode { get; set; } 
        public  string ShaparakAddressEng { get; set; } 
        public  string ShaparakAddressText { get; set; } 
        public  string ShaparakAddress { get; set; } 
        public  DateTime RentEndingDate { get; set; } 
        public  string Region { get; set; } 
        public  string RegionCode { get; set; } 
        public  string PosType { get; set; } 
        public  string PostalCode { get; set; } 
        public  string PortType { get; set; } 
        public  int OwneringType { get; set; } 
        public  string PhoneNumber { get; set; } 
        public  string MiddleCity { get; set; } 
        public  string MerchantID { get; set; } 
        public  string WorkFlowCaption { get; set; } 
        public  string WorkFlowValue { get; set; } 
        public  DateTime     InstallationDate { get; set; } 
        public  DateTime AgencyReciveDate { get; set; } 
        public  DateTime ShaparakDefinitionDate { get; set; } 
        public  string FlowMessage { get; set; } 
        public  Document RequestMerchantDocument { get; set; } 

    }
    
    public class GetRequestDetailByFollowupCodeResponse : BaseResponse
    {
        public GetRequestDetailByFollowupCodeData Data { get; set; }
    }

    public class GetRequestDetailByFollowupCodeData
    {
        public string BankFollowupCode { get; set; }
        public  string AccountNumber { get; set; }
        public string AccountShabaCode { get; set; }
        public  string ActivityLicenseNumberReferenceName { get; set; }
        public  string Branch { get; set; }
        public  string BrachCode { get; set; }
        public  string CustomerType { get; set; }
        public  string CustomerName { get; set; }
        public  string FollowupCode { get; set; }
        public string Guild { get; set; }
        public  string ISForeignNationals { get; set; }
        public  string MainCustomer { get; set; }
        public  string WorkTitle { get; set; }
        public  string WorkTitleEng { get; set; }
        public  string WorkType { get; set; }
        public  string TerminalID { get; set; }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public string ShaparakAddressEng { get; set; }
        public string ShaparakAddressText { get; set; }
        public  DateTime? RentEndingDate { get; set; }
        public  string Region  { get; set; }
        public  string RegionCode  { get; set; }
        public  string PortType { get; set; }
        public  string PostalCode { get; set; }
        public  string PosType { get; set; }
        public  string OwneringType   { get; set; } 
        public  string PhoneNumber   { get; set; } 
        public  string MiddleCity   { get; set; } 
        public  string MerchantID   { get; set; } 
        public  string WorkFlowCaption   { get; set; } 
        public  string WorkFlowValue   { get; set; } 
        public  DateTime? InstallationDate   { get; set; } 
        public  DateTime? AgencyReciveDate   { get; set; } 
        public  DateTime? ShaparakDefinitionDate   { get; set; } 
        public  string FlowMessage   { get; set; } 
        public  string IsMultiAccount   { get; set; } 
        public  string IsMultiAccountOwner   { get; set; } 
        public  int? MainSharingPercent   { get; set; } 
        public  int RequestID   { get; set; } 
        public  DateTime? InstallationRollBackDate   { get; set; } 

        
         
    }
}