using System;
using System.Collections.Generic;
using Enums = TES.Common.Enumerations;

namespace TES.Data.SearchParameter
{
    public class BiByParametersSearch
    {
        public BiByParametersSearch()
        {
            StatusIdList = new List<byte>();
        }

        public bool? ThreeMonthInActive { get; set; }

        public bool? TwoMonthInActive { get; set; }

        public bool? IsInNetwork { get; set; }

        public bool p_hazineh_soodePardakty { get; set; }
        public bool p_hazineh_rent { get; set; }
        public bool p_hazineh_karmozdShapark { get; set; }
        public bool p_hazineh_hashiyeSood { get; set; }

        public bool p_daramad_Vadie { get; set; }
        public bool p_daramad_Moadel { get; set; }
        public bool p_daramd_Tashilat { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }


        /// <summary>
        /// وضعیت
        /// </summary>
        public List<byte> StatusIdList { get; set; }


        /// <summary>
        /// شماره پایانه
        /// </summary>
        public string TerminalNo { get; set; }


        /// <summary>
        /// ویژه؟
        /// </summary>
        //public bool JustVip { get; set; }

        /// <summary>
        /// پر تراکنش، کم تراکنش، فاقد تراکنش، همه؟
        /// </summary>
        public List<Enums.TransactionStatus> TransactionStatusList { get; set; }
    }

    public class TerminalSearchParameters
    {
        public TerminalSearchParameters()
        {
            StatusIdList = new List<byte>();
        }

        /// <summary>
        /// کد پیگیری
        /// </summary>
        public long? TerminalId { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public List<byte> StatusIdList { get; set; }

        /// <summary>
        /// شرکت PSP
        /// </summary>
        public byte? PspId { get; set; }

        /// <summary>
        /// شماره پایانه
        /// </summary>
        public string TerminalNo { get; set; }

        /// <summary>
        /// شماره پذیرنده
        /// </summary>
        public string MerchantNo { get; set; }

        /// <summary>
        /// دارای درخواست تغییر حساب
        /// </summary>
        public bool ChangeAccountRequest { get; set; }

        /// <summary>
        /// دارای درخواست ابطال
        /// </summary>
        public bool RevokeRequest { get; set; }

        /// <summary>
        /// بازاریاب
        /// </summary>
        public long? MarketerId { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public long? DeviceTypeId { get; set; }

        /// <summary>
        /// شخصیت
        /// </summary>
        public bool? IsLegalPersonality { get; set; }

        /// <summary>
        /// عنوان
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// مرد
        /// </summary>
        public bool? IsMale { get; set; }

        /// <summary>
        /// صنف
        /// </summary>
        public long? ParentGuildId { get; set; }

        /// <summary>
        /// استان
        /// </summary>
        public byte? StateId { get; set; }

        /// <summary>
        /// شهر
        /// </summary>
        public long? CityId { get; set; }

        /// <summary>
        /// شعبه
        /// </summary>
        public long? BranchId { get; set; }

        /// <summary>
        /// شماره حساب
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// شماره مشتری
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// نام و نام خانوادگی
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// موبایل
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// از تاریخ ثبت
        /// </summary>
        public DateTime? FromSubmitTime { get; set; }

        /// <summary>
        /// تا تاریخ ثبت
        /// </summary>
        public DateTime? ToSubmitTime { get; set; }

        /// <summary>
        /// از تاریخ نصب
        /// </summary>
        public DateTime? FromInstallationDate { get; set; }

        /// <summary>
        /// تا تاریخ نصب
        /// </summary>
        public DateTime? ToInstallationDate { get; set; }

        /// <summary>
        /// از تاریخ تایید شاپرک
        /// </summary>
        public DateTime? FromBatchDate { get; set; }

        /// <summary>
        /// تا تاریخ تایید شاپرک
        /// </summary>
        public DateTime? ToBatchDate { get; set; }

        /// <summary>
        /// از تاریخ ابطال
        /// </summary>
        public DateTime? FromRevokeDate { get; set; }

        /// <summary>
        /// تا تاریخ ابطال
        /// </summary>
        public DateTime? ToRevokeDate { get; set; }

        /// <summary>
        /// از مبلغ تراکنش
        /// </summary>
        public decimal? FromTransactionPrice { get; set; }

        /// <summary>
        /// تا مبلغ تراکنش
        /// </summary>
        public decimal? ToTransactionPrice { get; set; }

        /// <summary>
        /// از تعداد تراکنش
        /// </summary>
        public int? FromTransactionCount { get; set; }

        /// <summary>
        /// تا تعداد تراکنش
        /// </summary>
        public int? ToTransactionCount { get; set; }

        /// <summary>
        /// بی سیم؟
        /// </summary>
        public bool? IsWireless { get; set; }

        public bool? AllActive { get; set; }

        /// <summary>
        /// از تاریخ تراکنش
        /// </summary>
        public DateTime? FromTransactionDate { get; set; }

        /// <summary>
        /// تا تاریخ تراکنش
        /// </summary>
        public DateTime? ToTransactionDate { get; set; }

        /// <summary>
        /// کاربر شعبه؟
        /// </summary>
        public bool IsBranchUser { get; set; }

        /// <summary>
        /// کاربر سرپرستی؟
        /// </summary>
        public bool IsSupervisionUser { get; set; }

        /// <summary>
        /// کد شعبه کاربر فعلی
        /// </summary>
        public long? CurrentUserBranchId { get; set; }

        /// <summary>
        /// اداره امور شعب تهران؟
        /// </summary>
        public bool IsTehranBranchManagment { get; set; }

        /// <summary>
        /// اداره امور شعب شهرستان؟
        /// </summary>
        public bool IsCountyBranchManagment { get; set; }

        /// <summary>
        /// وضعیت: فعال، غیر فعال، همه
        /// </summary>
        public bool? JustActive { get; set; }

        /// <summary>
        /// ویژه؟
        /// </summary>
        //public bool JustVip { get; set; }

        /// <summary>
        /// پر تراکنش، کم تراکنش، فاقد تراکنش، همه؟
        /// </summary>
        public List<Enums.TransactionStatus> TransactionStatusList { get; set; }

        /// <summary>
        /// سود ده زیان ده
        /// </summary>
        public List<Enums.TransactionStatus> TerminalTransactionStatusList { get; set; }


        /// <summary>
        /// کد رهگیری ثبت نام مالیاتی
        /// </summary>
        public string TaxPayerCode { get; set; }
    }

    public class ResultParameters
    {
        public string TerminalNo { get; set; }
        public double? IsGoodValue { get; set; }
        public double? TransactionValue { get; set; }

        public double TransactionValueEx
        {
            get
            {
                if (!TransactionValue.HasValue)
                    return 0;
                else
                {
                   
                    return Math.Abs(TransactionValue.Value % 1) > 0.5
                        ? Math.Ceiling(Math.Abs(TransactionValue.Value))
                        : Math.Floor(Math.Abs(TransactionValue.Value));
                }
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

        public double Hazine { get; set; }

        public double HazineEx
        {
            get { return Math.Abs(Hazine % 1) > 0.5 ? Math.Ceiling(Hazine) : Math.Floor(Hazine); }
        }

        public double IsGoodValueEx
        {
            get
            {
                if (!IsGoodValue.HasValue)
                    return 0;
                else
                {
                    var negative = IsGoodValue.Value < 0;
                    var result = Math.Abs(IsGoodValue.Value % 1) > 0.5
                        ? Math.Ceiling(Math.Abs(IsGoodValue.Value))
                        : Math.Floor(Math.Abs(IsGoodValue.Value));
                    return negative ? -result : result ;
                }
            }
        }

        public bool? IsGood { get; set; }


        public int IsGoodMonth { get; set; }
        public int? IsGoodYear { get; set; }
        public int Id { get; set; }
        public double p_daramad_Vadie { get; set; }
        public double p_daramad_Moadel { get; set; }
        public double p_hazineh_soodePardakty { get; set; }
        public double p_hazineh_karmozdShapark { get; set; }
        public double p_hazineh_hashiyeSood { get; set; }
        public int p_hazineh_rent { get; set; }
        public double p_daramd_Tashilat { get; set; }
        public string Title { get; set; }
        public string AccountNumber { get; set; }
        public int? TransactionCount { get; set; }

        public object TotalTransaction { get; set; }
        public string PspTitle { get; set; }

        public string PspTitleEx
        {
            get
            {
                switch (PspId)
                {
                    case "581672011":
                        return "آپ";
                        break;
                    case "581672021":
                    case "581672022":
                        return "الکترونیک کارت دماوند";

                        break;
                    case "581672031":
                        return "به پرداخت";
                        break;
                    case "581672041":
                    case "581672042":
                    case "581672043":
                        return "سامان";

                    case "581672051":
                    case "581672052":
                        return "پرداخت نوین";
                    case "581672061":
                    case "581672062":
                        return "تاپ";
                    case "581672081":
                    case "581672082":
                        return "  امید پی";
                    case "581672091":
                        return "فن آوا کارت";
                    case "581672111":
                    case "581672112":
                        return "ایران کیش";
                    case "581672131":
                    case "581672132":
                        return "سداد";
                    case "581672121":
                        return "سپهر";
                    case "581672141":
                        return "پاسارگاد";
                    case "581672142":
                        return "پاسارگاد";
                    case "1":
                        return "فن آوا کارت";
                    case "2":
                        return "ایران کیش";
                    case "3":
                        return "پارسیان";

                    default:
                        return "دیگر";
                }
            }
        }

        public string PspId { get; set; }
        public string CustomerId { get; set; }
    }

    public class GetInquiryDataParameters
    {
        public GetInquiryDataParameters()
        {
            StatusIdList = new List<byte>();
        }

        /// <summary>
        /// کد پیگیری
        /// </summary>
        public long? TerminalId { get; set; }

        /// <summary>
        /// وضعیت
        /// </summary>
        public List<byte> StatusIdList { get; set; }

        /// <summary>
        /// شرکت PSP
        /// </summary>
        public byte? PspId { get; set; }

        /// <summary>
        /// شماره پایانه
        /// </summary>
        public string TerminalNo { get; set; }

        /// <summary>
        /// شماره پذیرنده
        /// </summary>
        public string MerchantNo { get; set; }

        /// <summary>
        /// دارای درخواست تغییر حساب
        /// </summary>
        public bool ChangeAccountRequest { get; set; }

        /// <summary>
        /// دارای درخواست ابطال
        /// </summary>
        public bool RevokeRequest { get; set; }

        /// <summary>
        /// بازاریاب
        /// </summary>
        public long? MarketerId { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public long? DeviceTypeId { get; set; }

        /// <summary>
        /// شخصیت
        /// </summary>
        public bool? IsLegalPersonality { get; set; }

        /// <summary>
        /// عنوان
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// کد ملی
        /// </summary>
        public string NationalCode { get; set; }

        /// <summary>
        /// مرد
        /// </summary>
        public bool? IsMale { get; set; }

        /// <summary>
        /// صنف
        /// </summary>
        public long? ParentGuildId { get; set; }

        /// <summary>
        /// استان
        /// </summary>
        public byte? StateId { get; set; }

        /// <summary>
        /// شهر
        /// </summary>
        public long? CityId { get; set; }

        /// <summary>
        /// شعبه
        /// </summary>
        public long? BranchId { get; set; }

        /// <summary>
        /// شماره حساب
        /// </summary>
        public string AccountNo { get; set; }

        /// <summary>
        /// شماره مشتری
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// نام و نام خانوادگی
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// موبایل
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// از تاریخ ثبت
        /// </summary>
        public DateTime? FromSubmitTime { get; set; }

        /// <summary>
        /// تا تاریخ ثبت
        /// </summary>
        public DateTime? ToSubmitTime { get; set; }

        /// <summary>
        /// از تاریخ نصب
        /// </summary>
        public DateTime? FromInstallationDate { get; set; }

        /// <summary>
        /// تا تاریخ نصب
        /// </summary>
        public DateTime? ToInstallationDate { get; set; }

        /// <summary>
        /// از تاریخ تایید شاپرک
        /// </summary>
        public DateTime? FromBatchDate { get; set; }

        /// <summary>
        /// تا تاریخ تایید شاپرک
        /// </summary>
        public DateTime? ToBatchDate { get; set; }

        /// <summary>
        /// از تاریخ ابطال
        /// </summary>
        public DateTime? FromRevokeDate { get; set; }

        /// <summary>
        /// تا تاریخ ابطال
        /// </summary>
        public DateTime? ToRevokeDate { get; set; }

        /// <summary>
        /// از مبلغ تراکنش
        /// </summary>
        public decimal? FromTransactionPrice { get; set; }

        /// <summary>
        /// تا مبلغ تراکنش
        /// </summary>
        public decimal? ToTransactionPrice { get; set; }

        /// <summary>
        /// از تعداد تراکنش
        /// </summary>
        public int? FromTransactionCount { get; set; }

        /// <summary>
        /// تا تعداد تراکنش
        /// </summary>
        public int? ToTransactionCount { get; set; }

        /// <summary>
        /// بی سیم؟
        /// </summary>
        public bool? IsWireless { get; set; }

        /// <summary>
        /// از تاریخ تراکنش
        /// </summary>
        public DateTime? FromTransactionDate { get; set; }

        /// <summary>
        /// تا تاریخ تراکنش
        /// </summary>
        public DateTime? ToTransactionDate { get; set; }

        /// <summary>
        /// کاربر شعبه؟
        /// </summary>
        public bool IsBranchUser { get; set; }

        /// <summary>
        /// کاربر سرپرستی؟
        /// </summary>
        public bool IsSupervisionUser { get; set; }

        /// <summary>
        /// کد شعبه کاربر فعلی
        /// </summary>
        public long? CurrentUserBranchId { get; set; }

        /// <summary>
        /// اداره امور شعب تهران؟
        /// </summary>
        public bool IsTehranBranchManagment { get; set; }

        /// <summary>
        /// اداره امور شعب شهرستان؟
        /// </summary>
        public bool IsCountyBranchManagment { get; set; }

        /// <summary>
        /// وضعیت: فعال، غیر فعال، همه
        /// </summary>
        public bool? JustActive { get; set; }

        /// <summary>
        /// ویژه؟
        /// </summary>
        //public bool JustVip { get; set; }

        /// <summary>
        /// پر تراکنش، کم تراکنش، فاقد تراکنش، همه؟
        /// </summary>
        public List<Enums.TransactionStatus> TransactionStatusList { get; set; }

        /// <summary>
        /// سود ده زیان ده
        /// </summary>
        public List<Enums.TransactionStatus> TerminalTransactionStatusList { get; set; }


        /// <summary>
        /// کد رهگیری ثبت نام مالیاتی
        /// </summary>
        public string TaxPayerCode { get; set; }
    }
}