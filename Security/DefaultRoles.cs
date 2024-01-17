namespace TES.Security
{
    public enum DefaultRoles
    {
        /// <summary>
        /// مدیر سایت
        /// </summary>
        Administrator = 1,

        /// <summary>
        /// کاربر شعبه
        /// </summary>
        BranchUser = 2,

        /// <summary>
        /// کاربر سرپرتی
        /// </summary>
        SupervisionUser = 3,

        /// <summary>
        /// کاربر فناوری اطلاعات
        /// </summary>
        ITUser = 4,

        /// <summary>
        /// کارشناس پذیرندگان
        /// </summary>
        AcceptorsExpertUser = 5,

        /// <summary>
        /// اداره امور شعب
        /// </summary>
        BranchManagment = 6,

        /// <summary>
        /// اداره امور شعب تهران
        /// </summary>
        TehranBranchManagement = 7,

        /// <summary>
        /// اداره امور شعب شهرستان
        /// </summary>
        CountyBranchManagement = 8,

        /// <summary>
        /// مدیر پیام همدیریت پشتیبانی
        /// </summary>
        TicketManager = 9,
        
        CardRequestManager = 10,
        ChangeAccountAdmin = 11,
        CardRequester = 12,
        CardProcessor = 13,
        JustCardRequester = 14,
        //حسابرس
        Auditor = 15,
        BlockDocumentChanger = 16
    }
}