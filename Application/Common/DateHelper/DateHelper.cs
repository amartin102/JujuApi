using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.DateHelper
{
    public static class DateHelper
    {
        private static readonly TimeZoneInfo ColombiaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "SA Pacific Standard Time"   // Windows
                    : "America/Bogota"            // Linux/Mac
            );

        public static DateTime ToLocalTime(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {               
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), ColombiaTimeZone);
        }
    }
}
