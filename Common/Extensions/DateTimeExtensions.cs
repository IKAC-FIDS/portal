using Persia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Calendar = Persia.Calendar;

namespace TES.Common.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the 12:00:00 instance of a DateTime
        /// </summary>
        public static DateTime AbsoluteStart(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Gets the 11:59:59 instance of a DateTime
        /// </summary>
        public static DateTime AbsoluteEnd(this DateTime dateTime)
        {
            return AbsoluteStart(dateTime).AddDays(1).AddTicks(-1);
        }

        /// <summary>
        /// A simple date range
        /// Example Get next 80 days:
        /// IEnumerable<DateTime/> dateRange = DateTime.Now.GetDateRangeTo(DateTime.Now.AddDays(80));
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> GetDateRangeTo(this DateTime self, DateTime toDate)
        {
            var range = Enumerable.Range(0, new TimeSpan(toDate.Ticks - self.Ticks).Days);

            return from p in range
                   select self.Date.AddDays(p);
        }

        /// <summary>
        /// Wraps DateTime.TryParse() and all the other kinds of code you need to determine 
        /// if a given string holds a value that can be converted into a DateTime object.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsDate(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return DateTime.TryParse(input, out _);
            }
            return false;
        }

        public static int DaysInMonth(int year, int month)
        {
            var date = Calendar.ConvertToPersian(year, month, 1, DateType.Persian);

            var daysInMonth = 0;

            if (month >= 1 && month <= 6)
                daysInMonth = 31;

            if (month >= 7 && month <= 11)
                daysInMonth = 30;

            if (month == 12)
            {
                daysInMonth = date.IsLeapYear ? 30 : 29;
            }

            return daysInMonth;
        }

        public static Tuple<DateTime, DateTime> GetOneMonthFromStartOfPreviousMonth()
        {
            var today = Calendar.ConvertToPersian(DateTime.Today);
            var currentYear = today.ArrayType[0];
            var currentMonth = today.ArrayType[1];
            var month = currentMonth - 1 == 0 ? 12 : currentMonth - 1;
            var year = currentMonth - 1 == 0 ? currentYear - 1 : currentYear;
            var fromDate = Calendar.ConvertToGregorian(year, month, 1, DateType.Gerigorian);
            var daysInMonth = DaysInMonth(year, month);

            var toDate = fromDate.AddDays(daysInMonth - 1);
            return new Tuple<DateTime, DateTime>(fromDate, toDate);
        }

        #region Shamsi Date
        /// <summary>
        /// Swap to int number.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private static void Swap(ref int x, ref int y)
        {
            var temp = x;
            x = y;
            y = temp;
        }

        /// <summary>
        /// Split year, month and day from persian date string.
        /// </summary>
        /// <param name="stringPersianDate"></param>
        /// <param name="y"></param>
        /// <param name="m"></param>
        /// <param name="d"></param>
        public static void SplitSolarDate(string stringPersianDate, out int y, out int m, out int d)
        {
            stringPersianDate = stringPersianDate.Replace("۰", "0").Replace("۱", "1").Replace("۲", "2").Replace("۳", "3").Replace("۴", "4").Replace(
                    "۵", "5").Replace("۶", "6").Replace("۷", "7").Replace("۸", "8").Replace("۹", "9");
            if (stringPersianDate.IndexOf('/') < 2)
            {
                y = 1300;
                m = 1;
                d = 1;
            }
            else
            {
                var strItems = new string[3];
                var itemCounter = 0;
                foreach (var ch in stringPersianDate)
                {
                    if (ch == '/')
                    {
                        itemCounter++;
                    }
                    else
                    {
                        strItems[itemCounter] += ch;
                    }
                }
                try
                {
                    y = int.Parse(strItems[2]);
                    m = int.Parse(strItems[1]);
                    d = int.Parse(strItems[0]);
                    if (d > y)
                        Swap(ref d, ref y);
                }
                catch
                {
                    y = 1300;
                    m = 1;
                    d = 1;
                }
            }
        }

        /// <summary>
        /// Convert Shamsi Date To Miladi
        /// </summary>
        /// <param name="persianDate">Shamsi Date</param>
        /// <returns></returns>
        public static DateTime ToMiladiDate(this string persianDate)
        {
            SplitSolarDate(persianDate, out var y, out var m, out var d);

            return Calendar.ConvertToGregorian(y, m, d, DateType.Persian);
        }

        public static DateTime? ToNullableMiladiDate(this string persianDate)
        {
            if (string.IsNullOrEmpty(persianDate))
            {
                return null;
            }

            SplitSolarDate(persianDate, out var y, out var m, out var d);

            return Calendar.ConvertToGregorian(y, m, d, DateType.Persian);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="f">Extension.PersiaDateFormatString.Tx</param>
        /// <returns></returns>
        public static string ToPersianDate(this object dt, string f)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);
            return solarDate.ToString(f);
        }

        public static int GetPersianMonth(this object dt)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);
            return solarDate.ArrayType[1];
        }

        public static int GetPersianYear(this object dt)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);
            return solarDate.ArrayType[0];
        }

        /// <summary>
        /// Get Shamsi Year
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>YEAR 1390</returns>
        public static int ToPersianYear(this object dt)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ArrayType[0];
        }

        public static string ToPersianYearMonth(this object dt)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ArrayType[0] + solarDate.ArrayType[1].ToString("00");
        }

        /// <summary>
        /// Get Shamsi Year
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>YEAR 1390</returns>
        public static int ToPersianMonth(this object dt)
        {
            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ArrayType[1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToPersianDate(this object dt)
        {
            if (dt == null)
                return string.Empty;

            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ToString();
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToPersianDayOfWeek(this object dt)
        {
            if (dt == null)
                return string.Empty;

            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ArrayType[2]  > 9 ? solarDate.ArrayType[2].ToString() : "0" +solarDate.ArrayType[2].ToString();
            
        }
        public static int GetPersianMonthInt(this object dt)
        {
            if (dt == null)
                return 0;

            var temp = dt.ToDateTime();
            var pc = new PersianCalendar();
        var mo =     pc.GetMonth(temp);
        return mo;
        }

        public static string ToPersianDateTime(this object dt)
        {
            if (dt == null)
                return string.Empty;


            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ToString() + " ساعت " + temp.ToString("HH:mm");
        }

        public static string ToLongPersianDateTime(this object dt)
        {
            if (dt == null)
                return string.Empty;


            var temp = dt.ToDateTime();
            var solarDate = Calendar.ConvertToPersian(temp);

            return solarDate.ToString() + " ساعت " + temp.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Convert.ToDatetime if can not cast object to date time data type return datetime.now.
        /// [Mabna Method]
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this object o)
        {
            DateTime dt;
            try
            {
                dt = Convert.ToDateTime(o);
            }
            catch (Exception)
            {
                dt = DateTime.Now;
            }

            return dt;
        }

        /// <summary>
        /// Calculate Age and Return 24 Years, 5 Months, 21 Days
        /// </summary>
        /// <param name="birthDate">Birth Date</param>
        /// <returns>24 Years, 5 Months, 21 Days</returns>
        public static string GetAge(this DateTime birthDate)
        {
            var x = DateTime.Now - birthDate;
            var age = DateTime.MinValue + x;
            int year = age.Year - 1;
            int month = age.Month - 1;
            int day = age.Day - 1;
            int week = day / 7;
            day %= 7;
            var r = string.Empty;
            if (year != 0) r += string.Format("{0} سال ", year);
            if (month != 0) r += string.Format("{0} ماه ", month);
            if (week != 0) r += string.Format("{0} هفته ", week);
            if (day != 0) r += string.Format("{0} روز ", day);

            return r;
        }

        public static int CalculateAge(this DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return 0;

            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;

            if (birthDate > today.AddYears(-age))
                age--;

            return age;
        }

        public static DateTime? ConvertToDate(this int intDate)
        {
            try
            {
                var str = intDate.ToString();
                var year = str.Substring(0, 4);
                var month = str.Substring(4, 2);
                var day = str.Substring(6, 2);

                return string.Format("{0}/{1}/{2}", year, month, day).ToMiladiDate();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region RelativeDateTime

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static string ToRelativeDate(this DateTime dateTime)
        {
            var ts = new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks);
            var delta = Math.Abs(ts.TotalSeconds);
            if (delta < 1 * Minute)
            {
                return ts.Seconds <= 1 ? "لحظه ای قبل" : ts.Seconds + " ثانیه قبل";
            }
            if (delta < 2 * Minute)
            {
                return "یک دقیقه قبل";
            }
            if (delta < 45 * Minute)
            {
                return ts.Minutes + " دقیقه قبل";
            }
            if (delta < 90 * Minute)
            {
                return "یک ساعت قبل";
            }
            if (delta < 24 * Hour)
            {
                return ts.Hours + " ساعت قبل";
            }
            if (delta < 48 * Hour)
            {
                return "دیروز";
            }
            if (delta < 30 * Day)
            {
                return ts.Days + " روز قبل";
            }
            if (delta < 12 * Month)
            {
                var months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));

                return months <= 1 ? "یک ماه قبل" : months + " ماه قبل";
            }

            var years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));

            return years <= 1 ? "یک سال قبل" : years + " سال قبل";
        }
        #endregion

        public static IEnumerable<object> GenerateDates(DateTime d0, DateTime d1)
        {
            return Enumerable.Range(0, (d1.Year - d0.Year) * 12 + (d1.Month - d0.Month) + 1)
                .Select(m => new DateTime(d0.Year, d0.Month, 1).AddMonths(m)).ToList()
                .Select(x => new { Text = Calendar.ConvertToPersian(x).ToString("E"), Value = Calendar.ConvertToPersian(x).ArrayType[0] + "/" + Calendar.ConvertToPersian(x).ArrayType[1] }).ToList();
        }

        public static object ConvertToDbReadyDateTime(this DateTime? value)
        {
            return value.HasValue ? string.Format("'{0}'", value.Value) : "NULL";
        }

        public static string GetMonthName(this string month)
        {
            var monthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };

            return monthNames[Convert.ToInt32(month) - 1];
        }

        public static IEnumerable<Tuple<int, int>> GetMonthBetween(int fromYear, int fromMonth, int toYear, int toMonth)
        {
            var result = new List<Tuple<int, int>>();
            var fromYearMonth = fromYear + fromMonth.ToString("00");
            var toYearMonth = toYear + toMonth.ToString("00");

            while (fromYearMonth != toYearMonth)
            {
                if (toMonth >= 1)
                {
                    result.Add(new Tuple<int, int>(toYear, toMonth));
                }
                else
                {
                    toMonth = 12;
                    toYear--;
                    result.Add(new Tuple<int, int>(toYear, toMonth));
                }

                toYearMonth = toYear + toMonth.ToString("00");
                toMonth--;
            }

            return result;
        }

        public static string GetReadableMonthYear(this string persianYearMonth)
        {
            var year = persianYearMonth.Substring(0, 4);
            var month = persianYearMonth.Substring(4, 2);

            return string.Format("{0} {1}", GetMonthName(month), year.GetPersianNumbers());
        }
    }
}