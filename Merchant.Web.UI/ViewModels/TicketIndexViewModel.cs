using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class EditResourceDto
    {
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public int Id { get; set; }
        public  string Design { get; set;}
        public  string Title { get; set; }
        public  int Value { get; set; }
        public bool IsCard { get; set; }
        public string Code { get; set; }
    }
    public class CreateResourceDto
    {
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public  string Design { get; set;}
        public  string Title { get; set; }
        public  int Value { get; set; }
        public bool IsCard { get; set; }
        public string Code { get; set; }
    }
    public class TicketIndexViewModel
    {
        public string  searchGuid { get; set; }
        public DateTime? FromCreationDate { get; set; }
        public DateTime? ToCreationDate { get; set; }
        public long? Id { get; set; }
        public byte? StatusId { get; set; }
        public string Subject { get; set; }
        public bool JustNotReviewingMessages { get; set; }
        public int Page { get; set; }
        public int? BranchId { get; set; }
    }

    public class CalculateResultDto
    {
        public List<CustomerResultData> CustomerData { get; set; }
        public  List<TerminalResultData> TerminalData { get; set; }
    }

    public class CustomerResultData
    {
        public  string CustomerId {get; set; }
        public  bool IsGood  { get; set; }
        public double Daramd { get; set; }
        public double Hazineh { get; set; }
        public double IsGoodValue { get; set; }
        public double Avg { get; set; }
        public string BranchId { get; set; }
        public int? TransactionCount { get; set; }
        public double? TransactionValue { get; set; }
    }

    public class TerminalResultData
    {
        public  string TerminalNo { get; set; }
        public  bool? IsGood { get; set; }
        public double? IsGoodValue { get; set; }
        public bool IsBad { get; set; }
        public bool LowTransaction { get; set; }
        public  bool IsActive { get; set; }
        
        
        public double p_hazineh_soodePardakty  { get; set; }
        public int p_hazineh_rent  { get; set; }
        public double p_hazineh_karmozdShapark  { get; set; }
        public double p_hazineh_hashiyeSood  { get; set; }

        public double p_daramad_Vadie  { get; set; }
        public double p_daramad_Moadel  { get; set; }
        public  double p_daramd_Tashilat { get; set; }
        public int? TransactionCount { get; set; }
        public double? TransactionValue { get; set; }
        public bool IsInNetwork { get; set; }
        public string BranchId { get; set; }
        public string AccountNo { get; set; }
        public string PspId { get; set; }
        public string PspTitle { get; set; }
    }
    public class UpdateJobDetailsViewModel
    {
        public  int upId { get; set; }
        public int Page { get; set; }
    }

    public class UploadTerminalValidation
    {
        public  string _id { get; set; }
    
        public bool? Wage { get; set; }
        public bool? Installed { get; set; }
        public bool? Min { get; set; }
        public bool? Avg { get; set; }
    
        public  int Year { get; set; }
        public  int Month { get; set; }
        public  bool? Result { get; set; }
    }
    public class  UploadTerminalValidationDataViewModel
    {
        public  int Month { get; set; }
        public int Year { get; set; }
        public   bool? retriveTotalPageCount   { get; set; } 
        public  int? page{ get; set; }
        public  string CustomerId { get; set; }
        public  string orderClause { get; set; }
        
        public  string NationalCode { get; set; }
        public List<int> LowTransaction { get; set; }
        public List<int> Special { get; set; }
    }
    public class UpdateJobViewModel
    {
        public int Id { get; set; }
        public int? RowNumber { get; set; }
        public int ProcessedRow { get; set; }
        public string ErrorMessage { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool? Error { get; set; }
        public string Date { get; set; }
    }

    public class ParsianRequestForInfoViewModel
    {
        public int Id { get; set; }
        public int? StatusId { get; set; }
        public int? TopiarId { get; set; }
        public string NationalCode { get; set; }
        public string Error { get; set; }
        public string Date { get; set; }
    }
    
    public class CustomerStatusResultsViewModel
    {
        public long Id { get; set; }
        public int? Month { get; set; }
        public int?  Year { get; set; }
        public bool? IsGood { get; set; }
        public double IsGoodValue { get; set; }
        public bool? Special { get; set; }
        public double Avg { get; set; }
        
        public double AvgEx
        {
            get
            {
               
                var result = Math.Abs(Avg % 1) > 0.5
                    ? Math.Ceiling(Math.Abs(Avg))
                    : Math.Floor(Math.Abs(Avg));
                return  result;
            }
        }
        public double Hazineh { get; set; }
        public double HazinehEx
        {
            get
            {
               
                var result = Math.Abs(Hazineh % 1) > 0.5
                    ? Math.Ceiling(Math.Abs(Hazineh))
                    : Math.Floor(Math.Abs(Hazineh));
                return  result;
            }
        }

        public double Daramad { get; set; }
        public double DaramadEx
        {
            get
            {
               
                var result = Math.Abs(Daramad % 1) > 0.5
                    ? Math.Ceiling(Math.Abs(Daramad))
                    : Math.Floor(Math.Abs(Daramad));
                return  result;
            }
        }

        public string CustomerId { get; set; }
        public string Fullname { get; set; }
        public string NationalCode { get; set; }
    }
}