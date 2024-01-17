using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlTypes;
using System.Globalization;

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlString ToPersianDateTime(DateTime? input, string format)
    {
        if (!input.HasValue)
        {
            return SqlString.Null;
        }

        var persianCalendar = new PersianCalendar();
        var year = persianCalendar.GetYear(input.Value);
        var month = persianCalendar.GetMonth(input.Value);
        var day = persianCalendar.GetDayOfMonth(input.Value);

        string result;

        if (format == null)
        {
            result = String.Concat(year.ToString().PadLeft(4, '0'), "/", month.ToString().PadLeft(2, '0'), "/", day.ToString().PadLeft(2, '0'));
        }
        else
        {
            result = format
                .Replace("yyyy", year.ToString().PadLeft(4, '0'))
                .Replace("MM", month.ToString().PadLeft(2, '0'))
                .Replace("dd", day.ToString().PadLeft(2, '0'))
                .Replace("y", year.ToString())
                .Replace("M", month.ToString())
                .Replace("d", day.ToString())
                .Replace("HH", persianCalendar.GetHour(input.Value).ToString())
                .Replace("mm", persianCalendar.GetMinute(input.Value).ToString())
                .Replace("ss", persianCalendar.GetSecond(input.Value).ToString());
        }

        return new SqlString(result);
    }

    [Microsoft.SqlServer.Server.SqlFunction]
    public static SqlInt32 PersianDatePart(DateTime? input, string part)
    {
        if (input == null || part == null)
        {
            return SqlInt32.Null;
        }

        var persianCalendar = new PersianCalendar();

        switch (part.ToUpper())
        {
            case "YEAR":
                return new SqlInt32(persianCalendar.GetYear(input.Value));

            case "MONTH":
                return new SqlInt32(persianCalendar.GetMonth(input.Value));

            case "DAY":
                return new SqlInt32(persianCalendar.GetDayOfMonth(input.Value));

            case "HOUR":
                return new SqlInt32(persianCalendar.GetHour(input.Value));

            case "MINUTE":
                return new SqlInt32(persianCalendar.GetMinute(input.Value));

            case "SECOND":
                return new SqlInt32(persianCalendar.GetSecond(input.Value));

            default:
                return SqlInt32.Null;
        }
    }
}