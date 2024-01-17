namespace TES.Common.Enumerations
{
    public enum MessageStatus
    {
        /// <summary>
        /// باز
        /// </summary>
        Open = 1,

        /// <summary>
        /// در حال بررسی
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// بسته
        /// </summary>
        Close = 3,
        /// <summary>
        /// عدم تایید
        /// </summary>
        Reject = 4,
     
    }
    
    public enum DamageRequestStatus
    {
        /// <summary>
        /// باز
        /// </summary>
        Open = 1,

        /// <summary>
        /// در حال بررسی
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// بسته
        /// </summary>
        PayFromBranch = 3,
        /// <summary>
        ///    تعویق
        /// </summary>
        Delay = 4,
        /// <summary>
        /// پایان فرآیند
        /// </summary>
        
        EndProcess= 5,
        /// <summary>
        /// بسته
        /// </summary>
        PayFromCustomer = 6,
        
        NoNeedForPayment = 7
    }
    public enum CardRequestStatus
    {
        /// <summary>
        /// باز
        /// </summary>
        Open = 1,

        /// <summary>
        /// در حال بررسی
        /// </summary>
        UnderReview = 2,

        /// <summary>
        /// اماده تحویل
        /// </summary>
        ReadyForDeliver = 3,
        /// <summary>
        /// خاتمه یفاته
        /// </summary>
        Closed = 4 ,
        
        Cancel = 5
    }
   
}
