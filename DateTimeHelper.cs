using System;
using System.Collections.Generic;
using System.Text;

namespace Haukcode.UsefulDotNet
{
    public static class DateTimeHelper
    {
        public static DateTime SpecifyKind(this DateTime input, DateTimeKind kind)
        {
            return DateTime.SpecifyKind(input, kind);
        }

        public static DateTime? SpecifyKind(this DateTime? input, DateTimeKind kind)
        {
            if (!input.HasValue)
                return null;

            return DateTime.SpecifyKind(input.Value, kind);
        }

        public static IEnumerable<(DateTime Start, DateTime End)> SplitDateRange(DateTime start, DateTime end, int dayChunkSize, bool splitOnYear = false)
        {
            if (splitOnYear && dayChunkSize >= 365)
                throw new ArgumentException("Doesn't support splitOnYears and chunk size over a year");

            DateTime chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
            {
                if (chunkEnd.Year != start.Year)
                {
                    yield return (start, new DateTime(start.Year, 12, 31, start.Hour, start.Minute, start.Second, start.Millisecond, start.Kind));

                    start = new DateTime(start.Year + 1, 1, 1, start.Hour, start.Minute, start.Second, start.Millisecond, start.Kind);
                    continue;
                }
                else
                    yield return (start, chunkEnd);
                start = chunkEnd;
            }

            yield return (start, end);
        }

        public static IEnumerable<(DateTime Start, DateTime? End)> SplitDateRange(DateTime start, DateTime? end, int dayChunkSize)
        {
            DateTime? chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < (end ?? DateTime.Today.AddDays(2)))
            {
                if (chunkEnd >= DateTime.Today.AddDays(1))
                    chunkEnd = null;

                yield return (start, chunkEnd);

                if (!chunkEnd.HasValue)
                    break;

                start = chunkEnd.Value;
            }

            yield return (start, end);
        }
    }
}
