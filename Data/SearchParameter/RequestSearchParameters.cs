using System;

namespace TES.Data.SearchParameter
{
    public class RequestSearchParameters
    {
        /// <summary>
        /// کد شعبه
        /// </summary>
        public long? BranchId { get; set; }

        /// <summary>
        /// از تاریخ درخواست
        /// </summary>
        public DateTime? FromRequestDate { get; set; }

        /// <summary>
        /// تا تاریخ درخواست
        /// </summary>
        public DateTime? ToRequestDate { get; set; }

        /// <summary>
        /// شماره مشتری
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// شماره پایانه
        /// </summary>
        public string TerminalNo { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// وضعیت درخواست
        /// </summary>
        public byte? RequestStatusId { get; set; }

        /// <summary>
        /// شرکت PSP
        /// </summary>
        public byte? PspId { get; set; }

        /// <summary>
        /// کاربر شعبه؟
        /// </summary>
        public bool IsBranchUser { get; set; }

        /// <summary>
        /// کاربر سرپرستی؟
        /// </summary>
        public bool IsSupervisionUser { get; set; }

        /// <summary>
        /// اداره امور شعب تهران؟
        /// </summary>
        public bool IsTehranBranchManagment { get; set; }

        /// <summary>
        /// اداره امور شعب شهرستان
        /// </summary>
        public bool IsCountyBranchManagment { get; set; }

        /// <summary>
        /// بی سیم؟
        /// </summary>
        public bool? IsWireless { get; set; }

        /// <summary>
        /// کد شبعه کاربر فعلی
        /// </summary>
        public long? CurrentUserBranchId { get; set; }

        public bool RetriveTotalPageCount { get; set; }

        public int Page { get; set; }
    }
}