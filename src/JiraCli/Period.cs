﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace JiraCli
{
    public class Period
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public Period(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public IEnumerable<DateTime> EnumerateDays()
        {
            DateTime currentDate = StartDate;
            while (currentDate.Date <= EndDate.Date)
            {
                yield return currentDate;
                currentDate = currentDate.AddDays(1);
            }
        }

        public override string ToString()
        {
            return $"{(EndDate - StartDate).TotalDays.ToString("##")} days " +
                   $"from {StartDate.ToShortDateString()} to {EndDate.ToShortDateString()}";
        }

        public static Period FromDays(int days)
        {
            var today = DateTime.Today.AddDays(1).AddSeconds(-1);
            return new Period(today.AddDays(-days + 1).Date, today);
        }

        public static Period FromString(string period)
        {
            string dateFormat = "yyyy/MM/dd";
            string[] items = period.Split('-');
            if (items.Length == 2)
            {
                DateTime startDate;
                if (!DateTime.TryParseExact(items[0], dateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out startDate))
                {
                    DateTime endDate;
                    if (!DateTime.TryParseExact(items[1], dateFormat, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out endDate))
                    {
                        return new Period(startDate, endDate);
                    }
                }
            }

            throw new Exception("Got error while parsing.");
        }

        public static Period ForPrevMonth()
        {
            var today = DateTime.Today.AddDays(1).AddSeconds(-1);
            var month = new DateTime(today.Year, today.Month, 1);
            return new Period(month.AddMonths(-1), month.AddDays(-1));
        }
    }
}