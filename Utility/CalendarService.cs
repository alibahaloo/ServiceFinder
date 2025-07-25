using System.Globalization;

namespace ServiceFinder.Utility
{
    public class CalendarService
    {
        public enum DaysOfWeek
        {
            Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
        }

        public static string FormatDateString(string inputString)
        {
            // Parse the input string
            DateTime parsedDate = DateTime.ParseExact(inputString, "yyyy-MMMM-d", null);

            // Format the date to the desired format
            string formattedDate = parsedDate.ToString("yyyy-MM-dd");

            return formattedDate;
        }

        public static List<int> GetDaysInMonth(string monthName, int year, List<DateOnly> dateList)
        {
            List<int> selectedDays = new List<int>();

            foreach (DateOnly date in dateList)
            {
                if (date.Year == year && date.Month == DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month)
                {
                    selectedDays.Add(date.Day);
                }
            }

            return selectedDays;
        }

        public static DaysOfWeek ConvertDayOfWeekToCustom(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => DaysOfWeek.Monday,
                DayOfWeek.Tuesday => DaysOfWeek.Tuesday,
                DayOfWeek.Wednesday => DaysOfWeek.Wednesday,
                DayOfWeek.Thursday => DaysOfWeek.Thursday,
                DayOfWeek.Friday => DaysOfWeek.Friday,
                DayOfWeek.Saturday => DaysOfWeek.Saturday,
                DayOfWeek.Sunday => DaysOfWeek.Sunday,
                _ => throw new ArgumentException("Invalid day of the week."),
            };
        }


        public static DayOfWeek GetFirstDayOfMonth(string monthName, int year)
        {
            DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
            string monthNumber = (dtfi.MonthNames.ToList().FindIndex(m => m == monthName) + 1).ToString();

            // Check if the monthName is valid (between 1 and 12) and year is valid (non-negative)
            if (int.TryParse(monthNumber, out int month) && month >= 1 && month <= 12 && year >= 1)
            {
                string date = $"{year}-{monthNumber}-1";
                DateTime firstDayOfMonth = DateTime.ParseExact(date, "yyyy-M-d", CultureInfo.InvariantCulture);
                return firstDayOfMonth.DayOfWeek;
            }
            else
            {
                // Return null to indicate an error
                throw new ArgumentException();
            }
        }

        public static int GetNumberOfDaysInMonth(string monthName, int year)
        {
            DateTimeFormatInfo dtfi = DateTimeFormatInfo.CurrentInfo;
            string monthNumber = (dtfi.MonthNames.ToList().FindIndex(m => m == monthName) + 1).ToString();

            // Check if the monthName is valid (between 1 and 12)
            if (int.TryParse(monthNumber, out int month) && month >= 1 && month <= 12)
            {
                string date = $"{year}-{monthNumber}-1";
                DateTime firstDayOfMonth = DateTime.ParseExact(date, "yyyy-M-d", CultureInfo.InvariantCulture);
                int daysInMonth = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);
                return daysInMonth;
            }
            else
            {
                // Return a negative value to indicate an error
                return -1;
            }
        }
    }
}
