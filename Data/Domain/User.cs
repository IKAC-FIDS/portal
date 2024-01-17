using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TES.Data.Domain
{
    [Table("User")]
    public class User : IdentityUser<long, UserLogin, UserRole, UserClaim>
    {
        [Required]
        [StringLength(250)]
        public string FullName { get; set; }

        public DateTime PasswordExpirationDate { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual OrganizationUnit OrganizationUnit { get; set; }

        public virtual ICollection<ChangeAccountRequest> ChangeAccountRequests { get; set; } = new HashSet<ChangeAccountRequest>();
        public virtual ICollection<MerchantProfile> MerchantProfiles { get; set; } = new HashSet<MerchantProfile>();
        public virtual ICollection<RevokeRequest> RevokeRequests { get; set; } = new HashSet<RevokeRequest>();
        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
        public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();
        public virtual ICollection<CardRequest> SentCardRequestMessages { get; set; } = new HashSet<CardRequest>();
        public virtual ICollection<CardRequest> ReviewingCardRequestMessages { get; set; } = new HashSet<CardRequest>();

        public virtual ICollection<Message> ReviewingMessages { get; set; } = new HashSet<Message>();
        public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; } = new HashSet<UserActivityLog>();

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User, long> manager, string authenticationType = DefaultAuthenticationTypes.ApplicationCookie)
        {
            return await manager.CreateIdentityAsync(this, authenticationType);
        }
    }
}