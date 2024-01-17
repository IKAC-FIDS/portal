using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.ParsianRequestForInfo")]
    public class ParsianRequestForInfo
    {
        public bool? Failed { get; set; }
        public int Id { get; set; }
        public string Module { get; set; }
        public string Method { get; set; }
        public string Input { get; set; }
        public string Result { get; set; }
        public int? TerminalId { get; set; }
        public  DateTime? Create { get; set; } =DateTime.Now;
        public int? TopiarId { get; set; }
        public  int MerchantProfileId { get; set; }
        public string ShebaNo { get; set; }
        // 0 => pending 
        // 1 => archive
        // 2 => faild
        // 3 => ارسافل شده به پی اس پی
        //4 done
        public int? StatusId { get; set; }
        public string NationalCode { get; set; }
        public string Error { get; set; }
    }
}