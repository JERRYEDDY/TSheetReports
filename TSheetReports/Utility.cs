using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSheetReports
{
    public class Utility
    {

        //public string DurationToHours(double duration)
        //{
        //    var t = TimeSpan.FromSeconds(duration);
        //    //var durationString = string.Format("{0:N2}", t.TotalHours);
        //    var durationString = $"{t.TotalHours,0:N2}";
        //    return durationString;
        //}

        public double DurationToHours(double duration)
        {
            var t = TimeSpan.FromSeconds(duration);
            //var durationString = string.Format("{0:N2}", t.TotalHours);
            //var durationString = $"{t.TotalHours,0:N2}";
            return t.TotalHours;
        }

        public string FormatIso8601(DateTime dt)
        {
            var dto = new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt));
            var formatIso8601 = dto.ToString("yyyy-MM-ddTHH:mm:ssK");
            return formatIso8601;
        }
        public string FormatIso8601(DateTimeOffset dto)
        {
            //var dto = new DateTimeOffset(dt, TimeZoneInfo.Local.GetUtcOffset(dt));
            var formatIso8601 = dto.ToString("yyyy-MM-ddTHH:mm:ssK");
            return formatIso8601;
        }
        public DateTimeOffset DTOFromString(string offsetString)
        {
            if (!DateTimeOffset.TryParse(offsetString, out DateTimeOffset offset))
            {
                offset = DateTimeOffset.Now;
            }
            return offset;
        }
        public double FormatIso8601Duration(DateTimeOffset sDate, DateTimeOffset eDate)
        {
            TimeSpan duration = eDate - sDate;

            return duration.TotalSeconds;
        }
    }
}