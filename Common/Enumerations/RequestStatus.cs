namespace TES.Common.Enumerations
{
    /// <summary>
    /// وضعیت درخواست های تغییر حساب و جمع آوری
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>
        /// ثبت شده
        /// </summary>
        Registered = 1,

        /// <summary>
        /// ارسال شده به PSP
        /// </summary>
        SentToPsp = 2,

        /// <summary>
        /// نیازمند اصلاح
        /// </summary>
        NeedToReform = 4,

        /// <summary>
        /// انجام شده
        /// </summary>
        Done = 5,

        /// <summary>
        /// رد شده
        /// </summary>
        Rejected = 6,

        /// <summary>
        /// خطای وب سرویس
        /// </summary>
        WebServiceError = 7,
       /// <summary>
       /// نیاز مند بررسی
       /// </summary>
        NeedToEdit = 8,
       
       ShaparkError = 9,
       
       SwitchError = 10
    }
}