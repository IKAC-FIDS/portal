using System;
using System.Collections.Generic;
using System.Web;
using TES.Common.Enumerations;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MessageViewModel
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Subject { get; set; }
        public  string FullName { get; set; }
        public string Phone { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public long SenderId { get; set; }
        public  int? ExtraDataTopic { get; set; }
        public  string ExtraDataTopicValue { get; set; }
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public int MessageSubjectId { get; set; }
        public int? MessageSecondSubjectId { get; set; }
    }

    public class DamageRequestViewModel
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Subject { get; set; }
        public  string FullName { get; set; }
        public string Phone { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public long SenderId { get; set; }
        public  int? ExtraDataTopic { get; set; }
        public  string ExtraDataTopicValue { get; set; }
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public int DamageValue { get; set; }
        public string SerialNumber { get; set; }
        public int OrganizationUnitId { get; set; }
        public long TerminalId { get; set; }
    }
    public class Template
    {
        public  int Id { get; set; }
        public  string Code { get; set; }
        public string ImageName { get; set; }
        public string ByteArray { get; set; }
        public int Available { get; set; }
        public string Design { get; set; }
        public int Total { get; set; }
    }

    public class HavelReport
    {
        public  int Id { get; set; }
    }
    public class CardRequestViewModel
    {
        public CardRequestViewModel()
        {
            TemplateList = new List<Template>();
        }
        public  int Count { get; set; }
        public  string Idd { get; set; }
        public  int Id { get; set; }
        public  int CardServiceTypeId { get; set; }
        public  List<Template> TemplateList { get; set; }
        
        public List<HttpPostedFileBase> PostedFiles { get; set; }
        public string TemplateId { get; set; }
        public string Price { get; set; }
        public int BranchId { get; set; }
        public int Type { get; set; }
        public int PrintType { get; set; }
        public int Priority { get; set; }
        public int DeliveryType { get; set; }
        public bool UsePacket { get; set; }
        public int ServiceType { get; set; }
        public string Body { get; set; }
        public bool HasPacket { get; set; }
        public DamageRequestStatus Status { get; set; }
    }
    
    public class DamageRequestUploadViewModel
    {
        public DamageRequestUploadViewModel()
        {
            TemplateList = new List<Template>();
        }
    
        public  int Id { get; set; } 
        public  List<Template> TemplateList { get; set; }
        
        public HttpPostedFileBase PostedFiles { get; set; }
        public DamageRequestStatus Status { get; set; }
    }
}