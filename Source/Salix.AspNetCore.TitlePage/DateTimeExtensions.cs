using System.Text;

namespace Salix.AspNetCore.TitlePage;

/// <summary>
/// Extension methods for DateTime objects.
/// </summary>
#pragma warning disable CA1305 // Specify IFormatProvider
internal static class DateTimeExtensions
{
    /// <summary>
    /// Determines whether the evaluated DateTime value is between (inclusive) two specified dates.
    /// Time part is significant. Take care if specify <paramref name="endTime" /> without Time part - it may give logically false results near endDate (time should be 23:59:59).
    /// </summary>
    /// <param name="dateToCheck">The date to check.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>
    ///   <c>true</c> if the specified start time is between; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">DateTime.IsBetween() extension method got startTime bigger than endTime.</exception>
    internal static bool IsBetween(this DateTime dateToCheck, DateTime startTime, DateTime endTime)
    {
        if (startTime > endTime)
        {
            throw new ArgumentException("DateTime.IsBetween() extension method got startTime bigger than endTime");
        }

        return dateToCheck.Ticks >= startTime.Ticks && dateToCheck.Ticks <= endTime.Ticks;
    }

    /// <summary>
    /// Determines whether the evaluated DateTime value is between (inclusive) two specified dates.
    /// Null values are considered as valid (produces true outcome).
    /// Time part is significant. Take care if specify <paramref name="endTime" /> without Time part - it may give logically false results near endDate (time should be 23:59:59).
    /// </summary>
    /// <param name="dateToCheck">The date to check.</param>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>
    ///   <c>true</c> if the specified start time is between; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">DateTime.IsBetween() extension method got startTime bigger than endTime when both specified.</exception>
    internal static bool IsBetween(this DateTime dateToCheck, DateTime? startTime, DateTime? endTime)
    {
        if (startTime.HasValue && endTime.HasValue)
        {
            return dateToCheck.IsBetween(startTime.Value, endTime.Value);
        }

        if (endTime.HasValue && dateToCheck.Ticks >= endTime.Value.Ticks)
        {
            return false;
        }

        if (startTime.HasValue && dateToCheck.Ticks <= startTime.Value.Ticks)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns string of readable, nicely formatted, shortened and humanized date string.
    /// Using also "Today", "3 days ago" and 1.-2. February.
    /// Year added if it is not current year.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date (optional).</param>
    /// <param name="howManyDaysAsText">Days (from today) to use as textual (0 = today, 1 = tomorrow, yesterday, 2 = 2 days ago, 3 = 3 days ago etc.)</param>
    internal static string ToHumanDateString(this in DateTime startDate, DateTime? endDate = null, int howManyDaysAsText = 1)
    {
        var str = new StringBuilder();

        // Handle multiple dates separately
        if (endDate > startDate)
        {
            if (startDate.Month == endDate.Value.Month)
            {
                str.AppendFormat("{0}. – {1}. {2:MMMM}", startDate.Day, endDate.Value.Day, startDate);
            }
            else
            {
                str.AppendFormat("{0:M} – {1:M}", startDate, endDate.Value);
            }

            if (startDate.Year != DateTime.Now.Year)
            {
                str.AppendFormat(", {0}", startDate.Year);
            }

            return str.ToString();
        }

        // Here goes single date handling, starting with close to today
        if (startDate.Date.IsBetween(DateTime.Now.Date.AddDays(0 - howManyDaysAsText), DateTime.Now.Date.AddDays(howManyDaysAsText).AddHours(23).AddMinutes(59).AddSeconds(59)))
        {
            if (startDate.Date == DateTime.Now.Date)
            {
                return $"Today ({startDate:t})";
            }

            if (howManyDaysAsText > 0)
            {
                if (startDate.Date == DateTime.Now.Date.AddDays(1))
                {
                    return "Tomorrow";
                }

                if (startDate.Date == DateTime.Now.Date.AddDays(-1))
                {
                    return $"Yesterday ({startDate:t})";
                }
            }

            if (howManyDaysAsText > 1)
            {
                if (startDate.Date == DateTime.Now.Date.AddDays(2))
                {
                    return "The day after tomorrow";
                }

                if (startDate.Date == DateTime.Now.Date.AddDays(-2))
                {
                    return "The day before yesterday";
                }
            }

            if (howManyDaysAsText > 2)
            {
                if (startDate.Date == DateTime.Now.Date.AddDays(3))
                {
                    return "Three days from now";
                }

                if (startDate.Date == DateTime.Now.Date.AddDays(-3))
                {
                    return "Three days ago";
                }
            }

            for (int daysFromNow = 4; daysFromNow <= howManyDaysAsText; daysFromNow++)
            {
                if (startDate.Date == DateTime.Now.Date.AddDays(daysFromNow))
                {
                    return string.Format("{0} days from now", daysFromNow);
                }

                if (startDate.Date == DateTime.Now.Date.AddDays(0 - daysFromNow))
                {
                    return string.Format("{0} days ago", daysFromNow);
                }
            }
        }

        str.Append(startDate.ToString("M"));
        if (startDate.Year != DateTime.Now.Year)
        {
            str.AppendFormat(", {0}", startDate.Year);
        }

        return str.ToString();
    }
}
#pragma warning restore CA1305 // Specify IFormatProvider
