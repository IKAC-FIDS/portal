using System.Collections.Generic;

namespace TES.Data.SearchParameter
{
    public class UserSearchParameters
    {
        public UserSearchParameters()
        {
            RoleIdList = new List<long>();
        }

        /// <summary>
        /// نقش ها
        /// </summary>
        public List<long> RoleIdList { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// نام و نام خانوادگی
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// شماره موبایل
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// کد شعبه
        /// </summary>
        public long? BranchId { get; set; }

        /// <summary>
        /// مسدود؟
        /// </summary>
        public bool? Locked { get; set; }

        public bool RetriveTotalPageCount { get; set; }

        public int Page { get; set; }
    }
}