using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TES.Data.Domain
{
    [Table("psp.Terminal")]
    public class Terminal
    {
        [StringLength(500)] public string Description { get; set; }


        public long Id { get; set; }

        [StringLength(50)] public string MerchantNo { get; set; }

        public long DeviceTypeId { get; set; }

        [Column(TypeName = "date")] public DateTime? InstallationDate { get; set; }

        [Column(TypeName = "datetime2")] public DateTime LastUpdateTime { get; set; }

        [Column(TypeName = "date")] public DateTime? RevokeDate { get; set; }

        [StringLength(50)] public string TerminalNo { get; set; }

        [Required]
        //[StringLength(32)]
        public string Title { get; set; }

        public string StepCodeTitle { get; set; }
        public int? StepCode { get; set; }

        public int? InstallStatusId { get; set; }
        public string InstallStatus { get; set; }

        //[Required]
        [StringLength(500)] public string EnglishTitle { get; set; }

        public long BranchId { get; set; }

        [Required] [StringLength(50)] public string AccountNo { get; set; }

        [Required] [StringLength(50)] public string ShebaNo { get; set; }

        public byte StatusId { get; set; }

        public byte? PspId { get; set; }

        [Column(TypeName = "date")] public DateTime? BatchDate { get; set; }

        public long CityId { get; set; }

        public byte? RegionalMunicipalityId { get; set; }

        [Required] [StringLength(50)] public string TelCode { get; set; }

        [Required] [StringLength(50)] public string Tel { get; set; }

        [Required] [StringLength(2000)] public string Address { get; set; }

        //[Required]
        [StringLength(10)] public string PostCode { get; set; }

        public long MarketerId { get; set; }

        [StringLength(50)] public string ContractNo { get; set; }

        [Column(TypeName = "date")] public DateTime? ContractDate { get; set; }

        [Column(TypeName = "datetime2")] public DateTime SubmitTime { get; set; }

        public long UserId { get; set; }

        public long GuildId { get; set; }

        public long MerchantProfileId { get; set; }

        public byte ActivityTypeId { get; set; }

        [StringLength(4000)] public string ErrorComment { get; set; }

        [StringLength(2000)] public string ShaparakAddressFormat { get; set; }

        [StringLength(1000)] public string EnglishAddress { get; set; }

        public DateTime? BlockDocumentDate { get; set; }

        [StringLength(50)] public string BlockDocumentNumber { get; set; }

        [StringLength(50)] public string BlockAccountNumber { get; set; }

        public int? BlockPrice { get; set; }

        public byte? PreferredPspId { get; set; }
        public byte? BlockDocumentStatusId { get; set; }

        public DateTime? BlockDocumentStatusChangedToRegistredDate { get; set; }

        //public bool IsVip { get; set; }
        public string TaxPayerCode { get; set; }

        public bool? NewParsian { get; set; }
        public virtual City City { get; set; }
        public virtual OrganizationUnit Branch { get; set; }
        public virtual RegionalMunicipality RegionalMunicipality { get; set; }
        public virtual User User { get; set; }
        public virtual ActivityType ActivityType { get; set; }
        public virtual DeviceType DeviceType { get; set; }
        public virtual Guild Guild { get; set; }
        public virtual Marketer Marketer { get; set; }
        public virtual MerchantProfile MerchantProfile { get; set; }
        public virtual Psp Psp { get; set; }
        public virtual CustomerCategory CustomerCategory { get; set; }

        public virtual Psp PreferredPsp { get; set; }
        public virtual TerminalStatus Status { get; set; }
        public virtual BlockDocumentStatus BlockDocumentStatus { get; set; }

        public virtual ICollection<TerminalNote> TerminalNotes { get; set; } = new HashSet<TerminalNote>();
        public virtual ICollection<TerminalDocument> TerminalDocuments { get; set; } = new HashSet<TerminalDocument>();
        public int? TopiarId { get; set; }
        public int? CustomerCategoryId { get; set; }
        public bool? JobUpdated { get; set; }
        public bool? IsGood { get; set; }
        public double? IsGoodValue { get; set; }
        public int? IsGoodMonth { get; set; }
        public int? IsGoodYear { get; set; }
        public bool? LowTransaction { get; set; }
        public bool? IsActive { get; set; }
        public int? TransactionCount { get; set; }
        public double? TransactionValue { get; set; }
        public string Email { get; set; }
        public string WebUrl { get; set; }
        public bool? IsVirtualStore { get; set; }
        public int? PardakhtNovinSaveId { get; set; }
        public string FollowupCode { get; set; }
        public int? PardakhtEditNovinSaveId { get; set; }
        public int? RevokreRequestSavedId { get; set; } 

        static int CountDays(DayOfWeek day, DateTime start, DateTime end)
        {
            TimeSpan ts = end - start; // Total duration
            int count = (int) Math.Floor(ts.TotalDays / 7); // Number of whole weeks
            int remainder = (int) (ts.TotalDays % 7); // Number of remaining days
            int sinceLastDay = (int) (end.DayOfWeek - day); // Number of days since last [day]
            if (sinceLastDay < 0) sinceLastDay += 7; // Adjust for negative days since last [day]

            // If the days in excess of an even week are greater than or equal to the number days since the last [day], then count this one, too.
            if (remainder >= sinceLastDay) count++;

            return count;
        }

        public int GetTermialNumberDelay(int validDelayDay, List<DateTime> holidays)
        {
            var submit =  SubmitTime.Date.AddDays(1); 
            var Batch =BatchDate.Value.Date;


            if (submit > Batch)
                return 0;

            var span = Batch - submit;
            var businessDays = span.Days    ; 
            
            var fridaycount = CountDays(DayOfWeek.Friday, submit, Batch);
          //  var thusdaycount = CountDays(DayOfWeek.Thursday, firstDay, lastDay);

            businessDays = businessDays - fridaycount - 0; 
            foreach (var bh in holidays.Select(bankHoliday => bankHoliday.Date).Where(bh => submit <= bh && bh <= Batch))
            {
                --businessDays;
            } 
            return businessDays > validDelayDay ? businessDays - validDelayDay : 0;
        }
        
        public int GetInstallationDelay(int validDelayDay, List<DateTime> holidays)
        {
            var batch =  BatchDate.Value.Date.AddDays(1); 
            var install =InstallationDate.Value.Date;


            if (batch > install)
                return 0;

            var span = install - batch;
            var businessDays = span.Days    ; 
            
            var fridaycount = CountDays(DayOfWeek.Friday, batch, install);
            //  var thusdaycount = CountDays(DayOfWeek.Thursday, firstDay, lastDay);

            businessDays = businessDays - fridaycount - 0; 
            foreach (var bh in holidays.Select(bankHoliday => bankHoliday.Date).Where(bh => batch <= bh && bh <= install))
            {
                --businessDays;
            } 
            return businessDays > validDelayDay ? businessDays - validDelayDay : 0;
        }
    }
}