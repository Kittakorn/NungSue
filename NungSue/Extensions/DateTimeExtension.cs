using System.Globalization;

namespace NungSue.Extensions
{
    public static class DateTimeExtension
    {
        public static string ToThaiString(this DateTime dateTime, string format)
        {
            return dateTime.ToString(format, new CultureInfo("th-TH"));
        }
    }
}