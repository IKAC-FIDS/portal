using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TES.Merchant.Web.UI.Functions
{
    public static class GetMonthValue
    {
        public static string GetMonth(int month)
        {
            switch (month)
            {
                case 0:
                    month = 12;
                    break;
                case -1:
                    month = 11;
                    break;
                case -2:
                    month = 10;
                    break;
            }

            switch(month)
            {
                case 1:
                    return "فروردین";
                case 2:
                    return "اردیبهشت";
                case 3:
                    return "خرداد";
                case 4:
                    return "تیر";
                case 5:
                    return "مرداد";
                case 6:
                    return "شهریور";
                case 7:
                    return "مهر";
                case 8:
                    return "آبان";
                case 9:
                    return "آذر";
                case 10:
                    return "دی";
                case 11:
                    return "بهمن";
                case 12:
                    return "اسفند";
                default:
                    return "";
            }

        }
    }
}