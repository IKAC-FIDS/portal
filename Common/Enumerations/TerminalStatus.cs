namespace TES.Common.Enumerations
{
    public enum TerminalStatus
    {
        /// <summary>
        /// ورود بازاریابی
        /// </summary>
        New = 1,

        /// <summary>
        /// برنگشته از سوییچ
        /// </summary>
        NotReturnedFromSwitch = 3,

        /// <summary>
        /// نیازمند اصلاح
        /// </summary>
        NeedToReform = 4,

        /// <summary>
        /// آماده تخصیص
        /// </summary>
        ReadyForAllocation = 5,

        /// <summary>
        /// تخصیص یافته
        /// </summary>
        Allocated = 6,

        /// <summary>
        /// تست شده
        /// </summary>
        Test = 7,

        /// <summary>
        /// نصب شده
        /// </summary>
        Installed = 8,

        /// <summary>
        /// ابطال شده
        /// </summary>
        Revoked = 9,

        /// <summary>
        /// ارسال شده به شاپرک
        /// </summary>
        SendToShaparak = 15,

        /// <summary>
        /// حذف شده
        /// </summary>
        Deleted = 16,

        /// <summary>
        /// دریافت شده از سوییچ ناموفق
        /// این مورد برای بحث ویرایش فن آوا اضافه شد. مواردی از فن آوا که این وضعیت را داشته باشند متد ویرایش روی آنها فراخوانی خواهد شد
        /// </summary>
        UnsuccessfulReturnedFromSwitch = 17,

        /// <summary>
        /// در انتظار جمع آوری
        /// </summary>
        WaitingForRevoke = 18,
        
        
        /// <summary>
        /// ارسال به نمایندگی
        /// </summary>
        SendToRepresentation = 19,
        
          
        /// <summary>
        ///    تحت تعمیر  
        /// </summary>
        Repairing = 20
    }


    public enum TypeList
    {
        hedye
    }
}