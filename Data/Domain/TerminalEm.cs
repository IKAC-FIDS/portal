using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TES.Data.Domain
{
    [Table("psp.TerminalEm")]
    public class TerminalEm
    {
        public int Id { get; set; }
        public DateTime? EmTime { get; set; }

        [Required]
        [StringLength(50)]
        public string TerminalNo { get; set; }

        public DateTime RequestEmTime { get; set; }
        
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
        public int GetInstallationDelay(int validDelayDay, List<DateTime> holidays)
        {
            var requestdate =RequestEmTime.Date;

            var donDate =  EmTime.Value.Date; 
            

            if ( requestdate > donDate)
                return 0;

            var span = donDate  - requestdate;
            var businessDays = span.Days   ; 
            
         //   var fridaycount = CountDays(DayOfWeek.Friday, requestdate, donDate);
            //  var thusdaycount = CountDays(DayOfWeek.Thursday, firstDay, lastDay);

            businessDays = businessDays  ; 
            // foreach (var bh in holidays.Select(bankHoliday => bankHoliday.Date).Where(bh => requestdate  <= bh && bh <=donDate ))
            // {
            //     --businessDays;
            // } 
            var result = businessDays > validDelayDay ? businessDays - validDelayDay : 0;
            return result;
        }
    }
}