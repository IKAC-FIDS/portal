namespace TES.Common.Enumerations
{
    public enum BlockDocumentStatus
    {
        /// <summary>
        /// ثبت شده
        /// </summary>
        Registered = 1,

        /// <summary>
        /// ثبت نشده
        /// </summary>
        NotRegistered = 2,

        /// <summary>
        /// در انتظار پایش دوره ای
        /// </summary>
        WaitingForPeriodicMonitoring = 3,

        /// <summary>
        /// در انتظار بررسی
        /// </summary>
        WaitingForReview = 4
    }
}