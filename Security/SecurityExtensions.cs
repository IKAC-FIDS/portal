using Microsoft.AspNet.Identity;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace TES.Security
{
    public static class SecurityExtensions
    {
        /// <summary>
        /// نام و نام خانوادگی کاربر فعلی
        /// </summary>
        public static string GetFullName(this IIdentity identity) => (identity as ClaimsIdentity)?.FindFirstValue(ClaimTypes.GivenName);

        /// <summary>
        /// شعبه کاربر فعلی
        /// </summary>
        public static string GetBranchTitle(this IIdentity identity) => (identity as ClaimsIdentity)?.FindFirstValue("BranchTitle");

        /// <summary>
        /// کد شعبه کاربر فعلی
        /// </summary>
        public static long? GetBranchId(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            long? organizationUnitId = null;
            var branchId = (identity as ClaimsIdentity)?.FindFirstValue("BranchId");
            if (!string.IsNullOrEmpty(branchId))
            {
                organizationUnitId = Convert.ToInt64(branchId);
            }
                

            return organizationUnitId;
        }

        /// <summary>
        /// زمان انقضاء رمز عبور کاربر فعلی
        /// </summary>
        public static DateTime GetPasswordExpirationDate(this IIdentity identity)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            var claimsIdentity = identity as ClaimsIdentity;
            var passwordExpirationDate = DateTime.Now;
            var passwordExpirationDateClaim = claimsIdentity?.FindFirstValue("PasswordExpirationDate");
            if (!string.IsNullOrEmpty(passwordExpirationDateClaim))
            {
                passwordExpirationDate = Convert.ToDateTime(passwordExpirationDateClaim);
            }

            return passwordExpirationDate;
        }

        /// <summary>
        /// نقش کاربر مدیر سایت است؟
        /// </summary>
        public static bool IsAdmin(this IPrincipal principal) => principal.IsInRole(DefaultRoles.Administrator.ToString());

        public static bool IsBlockDocumentChanger(this IPrincipal principal) => principal.IsInRole(DefaultRoles.BlockDocumentChanger.ToString());

        
        
        public static bool IsAuditor(this IPrincipal principal) => principal.IsInRole(DefaultRoles.Auditor.ToString());

        /// <summary>
        /// نقش کاربر "کاربر شعبه" است؟
        /// </summary>
        public static bool IsBranchUser(this IPrincipal principal) => principal.IsInRole(DefaultRoles.BranchUser.ToString());

        public static bool IsChangeAccountAdmin(this IPrincipal principal) => principal.IsInRole(DefaultRoles.ChangeAccountAdmin.ToString());

        
        /// <summary>
        /// نقش کاربر "کاربر سرپرتی" است؟
        /// </summary>
        public static bool IsSupervisionUser(this IPrincipal principal) => principal.IsInRole(DefaultRoles.SupervisionUser.ToString());

        /// <summary>
        /// نقش کاربر کارشناس فناوری اطلاعات است؟
        /// </summary>
        public static bool IsItUser(this IPrincipal principal) => principal.IsInRole(DefaultRoles.ITUser.ToString());

        
        /// <summary>
        /// نقش کاربر کارشناس پذیرندگان است؟
        /// </summary>
        public static bool IsAcceptorsExpertUser(this IPrincipal principal) =>  principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.AcceptorsExpertUser.ToString());
      
        public static bool IsCardRequestManager(this IPrincipal principal) =>  principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.CardRequestManager.ToString());
        public static bool IsCardRequester(this IPrincipal principal) =>  principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.CardRequester.ToString());

        public static bool IsJustCardRequester(this IPrincipal principal) =>   principal.Identity.IsAuthenticated && principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.JustCardRequester.ToString());

       
        public static bool IsCardProcessor(this IPrincipal principal) =>   principal.Identity.IsAuthenticated &&principal.IsInRole(DefaultRoles.CardProcessor.ToString());

        
        /// <summary>
        /// نقش کاربر اداره امور شعب است؟
        /// </summary>
        public static bool IsBranchManagementUser(this IPrincipal principal) =>   principal.Identity.IsAuthenticated &&principal.IsInRole(DefaultRoles.BranchManagment.ToString());

        /// <summary>
        /// نقش کاربر اداره امور شعب تهران است؟
        /// </summary>
        public static bool IsTehranBranchManagementUser(this IPrincipal principal) =>  principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.TehranBranchManagement.ToString());

        /// <summary>
        /// نقش کاربر اداره امور شعب شهرستان است؟
        /// </summary>
        public static bool IsCountyBranchManagementUser(this IPrincipal principal) =>  principal.Identity.IsAuthenticated && principal.IsInRole(DefaultRoles.CountyBranchManagement.ToString());

        /// <summary>
        /// نقش کاربر مدیر پیام ها است؟
        /// </summary>
        public static bool IsMessageManagerUser(this IPrincipal principal) =>
            
            principal.Identity.IsAuthenticated &&
            principal.IsInRole(DefaultRoles.TicketManager.ToString());
    }
}