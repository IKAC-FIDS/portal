using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardRequest")]
    public class CardRequest
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public  virtual CardType CardType { get; set; }
        public  virtual OrganizationUnit OrganizationUnit { get; set; }

        public  virtual CardServiceType CardServiceType { get; set; }
        public  int CardTypeId { get; set; }
        public  int CardServiceTypeId { get; set; }
        /// <summary>
        /// کاربر درخواست کننده
        /// </summary>
        public long UserId { get; set; }
        public long? ReviewerUserId { get; set; }
        public byte StatusId { get; set; }
        public bool LastReplySeen { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
       
        public virtual User User { get; set; }
        public virtual User ReviewerUser { get; set; }
        public virtual CardRequestStatus CardRequestStatus { get; set; }
        public virtual ICollection<CardRequestReply> Replies { get; set; } = new HashSet<CardRequestReply>();
        public virtual ICollection<CardRequestDocument> MessageDocuments { get; set; } = new HashSet<CardRequestDocument>();
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string GUID { get; set; }
        
        public int Count { get; set; }
        public  string TemplateId { get; set; }
        public  string Price { get; set; }
        public  long OrganizationUnitId { get; set; }
        
        public  int PrintType { get; set; }
        public  int Priority { get; set; }
        public  int DeliveryType { get; set; }
        public  int ExitRemittance { get; set; }
        public  DateTime RemittanceDate { get; set; }
        public byte[] FileData { get; set; }
        public  byte[] Template { get; set; }
        public bool UsePacket { get; set; }
        public DateTime? EndDate { get; set; }
        public byte[] FinalFile { get; set; }
        public string FileNameFinalFile { get; set; }
        public bool HasPacket { get; set; }
    }
}