using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class UpdateCustomerRequest
    {
        public string KeyValue { get; set; }
        public UpdateCustomerRequestData Data { get; set; }
    }

    public class AddPosReplacementRequest
    {
        public AddPosReplacementRequestData Data { get; set; }

    }

    public class AddInstallationRollbackRequest
    {
        public AddInstallationRollbackRequestData Data { get; set; }

    }

    public class AddInstallationRollbackRequestData
    {
        public string Request { get; set; }
        public int RollbackReasonID { get; set; }
        public string Description { get; set; }

        public string TerminalID { get; set; }
        public string BankFollowUpCode  { get; set; }
    }
    public class AddEditRequestRequest
    {
        public AddEditRequestRequestData Data { get; set; }

    }

    public class AddEditRequestRequestData
    {
        public  string Request { get; set; }
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
        
        
        public  string NewActivityLicenseNumber  { get; set; }
        public  string NewShaparakAddressEng   { get; set; }
        
        
        public  DateTime NewBusinessLicenseRegisterDate   { get; set; }
        public  DateTime  NewBusinessLicenseEndDate { get; set; }
        
    }
    public class AddAccountChangeRequest
    {
        public AddAccountChangeRequestData Data { get; set; }

    }

    public class AddAccountChangeRequestData
    {
        public  string BankFollowUpCode { get; set; }
        public  string Request { get; set; }
        public  string TerminalID { get; set; }
        public  int NewBankID { get; set; }
        public  string NewBranchCode { get; set; }
        public  string NewAccountNumber { get; set; }
        public  string NewShabaCode { get; set; }
        public  string LetterImage { get; set; }
        public  string SecondLetterImage { get; set; }
        public  bool AddAccountNumber  { get; set; }
        public  bool IsMultiAccount  { get; set; } 
       
        
       
      
     
      
     

    }

    public class AddPosReplacementRequestData
    {
        public  string Request { get; set; }
        public  string TerminalID { get; set; }
        public  string NewPosType { get; set; }
        public  string NewPosModel { get; set; }
        public  string NewSerialNumber { get; set; }
        public  string ReplacementReasonID { get; set; }
        public  string PosStatusID { get; set; }

    }
    public class BindSerialToSwitchRequest
    {
        public BindSerialToSwitchRequestParameters Parameters { get; set; }
        public BindSerialToSwitchRequesttData Data { get; set; }
    }

    public class GetEditRequestRequest
    {
        public  GetEditRequestRequestParameters Parameters { get; set; }

    }

    public class GetInstallationRollbackRequest
    {
        public  GetInstallationRollbackRequestParameters Parameters { get; set; }

    }

    public class GetInstallationRollbackRequestParameters
    {
        public  string InstallationRollbackID { get; set; }
    }

    public class GetEditRequestRequestParameters
    {
        public  string EditRequestID { get; set; }
    }
    public class GetAccountChangeDetailRequest
    {
      
        public GetAccountChangeDetailRequestParameters Parameters { get; set; }

    }

    public class GetAccountChangeDetailRequestParameters
    {
        public  string AccountChangeID { get; set; }
    }
    public class GetPosReplacementDetailRequest
    {
        public GetPosReplacementDetailRequestParameters Parameters { get; set; }
        
    }

    public class GetPosReplacementDetailRequestParameters
    {
        public  string  PosReplacementID { get; set; }
    }
    public class BindSerialToSwitchRequesttData
    {
        public string ProductSerials { get; set; }
        public  string PosModel { get; set; }
    }

    public class BindSerialToSwitchRequestParameters
    {
        public  string TerminalID { get; set; }
    }
    public class UpdateDocumentRequest
    {  
        public UpdateDocumentRequestParameters Parameters { get; set; }
        public UpdateDocumentRequestData Data { get; set; }
    }

    public class UpdateDocumentRequestData
    {
        public  string DocumentAttachment { get; set; }
    }

    public class UpdateDocumentRequestParameters
    {
        public int RequestID { get; set; }
        public int DocumentTypeID { get; set; }

    }
    public class UpdateRequestByFollowUpCodeRequest
    {
        public List<UpdateRequestByFollowUpCodeRequestaChilds> Childs { get; set; }
        public UpdateRequestByFollowUpCodeRequestParameters Parameters { get; set; }
        public UpdateRequestByFollowUpCodeRequestData Data { get; set; }
    }

    public class UpdateRequestByFollowUpCodeRequestaChilds
    {
       public  string ChildName { get; set; }
    }
    public class UpdateRequestByFollowUpCodeRequestData
    {
        public  string AccountNumber { get; set; }
     
        public  string WorkTitle { get; set; }
        public  string WorkTitleEng { get; set; }
        public  string PhoneNumber { get; set; }
        public  string PostalCode { get; set; }
        public  string ShaparakAddressText { get; set; }
        public  string CityShaparakCode { get; set; }
        public  string GuildSupplementaryCode  { get; set; }
        public  string AccountShabaCode   { get; set; }
        public  string TaxPayerCode { get; set; }
    }
    public class UpdateRequestByFollowUpCodeRequestParameters
    {
        public  string FollowupCode { get; set; }
        public string BankFollowupCode { get; set; }
    }

public class UpdateRequestByFollowUpCodeRequestaDocs
    {
        public string ChildName { get; set; } = "RequestMerchantDocument";
        public List<Document> Data { get; set; }
    }


public class RequestAccounts : UpdateRequestByFollowUpCodeRequestaChilds
{
    public RequestAccounts()
    {
        ChildName = "SubCustomer";
    }
    
    public List<RequestAccountsData> Data { get; set; }
}

public class RequestAccountsData
{
    public int  CustomerID{ get; set; }
    public string AccountNumber { get; set; }
    public string AccountShabaCode{ get; set; }
    public string BranchCode{ get; set; }
    public int BankID { get; set; }
    public int SharingPercent{ get; set; }
}
public class SubCustomerChild : UpdateRequestByFollowUpCodeRequestaChilds
{
    public SubCustomerChild()
    {
        ChildName = "SubCustomer";
    }
    
    public List<SubCustomerData> Data { get; set; }
}

public class SubCustomerData
{
    public int CustomerID { get; set; }
}

}