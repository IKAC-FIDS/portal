using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using TES.Common.Extensions;

namespace TES.Data.Domain
{
    [Table("Message")]
    public class Message
    {
        public long Id { get; set; }
      
        public string Body { get; set; }
        public long UserId { get; set; }
        public long? ReviewerUserId { get; set; }
        public byte StatusId { get; set; }
        public bool LastReplySeen { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
       
        public virtual User User { get; set; }
        public virtual MessageSubject MessageSubject { get; set; }
        public virtual MessageSubject MessageSecondSubject { get; set; }

        public virtual User ReviewerUser { get; set; }
        public virtual MessageStatus MessageStatus { get; set; }
        public virtual ICollection<MessageReply> Replies { get; set; } = new HashSet<MessageReply>();
        public virtual ICollection<MessageDocument> MessageDocuments { get; set; } = new HashSet<MessageDocument>();
        public string Phone { get; set; }
        public string FullName { get; set; }
        public string GUID { get; set; }
        public string ExtraDataTopicValue { get; set; }
        public int? ExtraDataTopic { get; set; }
        public long MessageSubjectId { get; set; }
        public string OldSubject { get; set; } = "";
        public long? MessageSecondSubjectId { get; set; }
    }
}

