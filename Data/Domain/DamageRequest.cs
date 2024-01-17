using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("DamageRequest")]
    public class DamageRequest
    {
        public long Id { get; set; }
        public int DamageValue { get; set; }
        public long OrganizationUnitId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public long UserId { get; set; }
        public long? ReviewerUserId { get; set; }
        public byte DamageRequestStatusId { get; set; }
        public bool LastReplySeen { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
        public virtual User User { get; set; }
        public virtual User ReviewerUser { get; set; }
        public virtual DamageRequestStatus DamageRequestStatus { get; set; }
        public virtual OrganizationUnit OrganizationUnit { get; set; }

        public virtual ICollection<DamageRequestReply> Replies { get; set; } = new HashSet<DamageRequestReply>();
        public virtual ICollection<DamageRequestDocument> DamageRequestDocuments { get; set; } = new HashSet<DamageRequestDocument>();
       
        public string FullName { get; set; }
        public string GUID { get; set; }
        public string ExtraDataTopicValue { get; set; }
        public int? ExtraDataTopic { get; set; }
        public string SerialNumber { get; set; }
        public string FileNameFinalFile { get; set; }
        public byte[] FinalFile { get; set; }
        public long TerminalId { get; set; }
    }
}