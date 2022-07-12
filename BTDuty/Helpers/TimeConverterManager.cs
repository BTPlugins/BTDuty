using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTDuty.Helpers
{
    public static class TimeConverterManager
    {
        public static string Format(TimeSpan ts, int maxItems = 2)
        {
            var timeItems = new List<string>();
            Func<int, string> pluralize = (int t) => t > 1 ? "s" : "";

            if (ts.Days > 0 && timeItems.Count < maxItems)
                timeItems.Add($"{ts.Days} Day{pluralize(ts.Days)}");

            if (ts.Hours > 0 && timeItems.Count < maxItems)
                timeItems.Add($"{ts.Hours} Hour{pluralize(ts.Hours)}");

            if (ts.Minutes > 0 && timeItems.Count < maxItems)
                timeItems.Add($"{ts.Minutes} Minute{pluralize(ts.Minutes)}");

            if (ts.Seconds > 0 && timeItems.Count < maxItems)
                timeItems.Add($"{ts.Seconds} Second{pluralize(ts.Seconds)}");
            /*
            if (ts.Milliseconds > 0 && timeItems.Count < maxItems)
                timeItems.Add($"{ts.Milliseconds} Millisecond{pluralize(ts.Milliseconds)}");
            */
            if (timeItems.Count == 0 && timeItems.Count < maxItems)
                timeItems.Add("0 Seconds");

            return string.Join(", ", timeItems);
        }
        public static TimeSpan getTimeSpan(DateTime startDate, DateTime endDate)
        {
            var fSeconds = endDate - startDate;
            TimeSpan totalSeconds = TimeSpan.FromSeconds(fSeconds.TotalSeconds);
            return totalSeconds;
        }
    }
}
