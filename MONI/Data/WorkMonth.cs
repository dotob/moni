﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using MONI.Data.SpecialDays;
using MONI.Util;
using NLog;

namespace MONI.Data
{
    public class WorkMonth : INotifyPropertyChanged
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly int month;
        private readonly float hoursPerDay;
        private readonly int year;

        public WorkMonth(int year, int month, GermanSpecialDays specialDays, WorkDayParserSettings parserSettings, float hoursPerDay)
        {
            this.year = year;
            this.month = month;
            this.hoursPerDay = hoursPerDay;
            this.Weeks = new ObservableCollection<WorkWeek>();
            this.Days = new ObservableCollection<WorkDay>();
            this.ShortCutStatistic = new QuickFillObservableCollection<ShortCutStatistic>();
            // TODO which date should i take?
            this.ReloadShortcutStatistic(parserSettings.ShortCuts);

            var cal = CultureInfo.CurrentCulture.Calendar;
            WorkWeek lastWeek = null;
            for (int day = 1; day <= cal.GetDaysInMonth(year, month); day++)
            {
                var dt = new DateTime(year, month, day);

                WorkDay wd = new WorkDay(year, month, day, specialDays);
                this.Days.Add(wd);
                var weekOfYear = cal.GetWeekOfYear(dt);
                if (lastWeek == null || lastWeek.WeekOfYear != weekOfYear)
                {
                    lastWeek = new WorkWeek(this, weekOfYear);
                    lastWeek.PropertyChanged += new PropertyChangedEventHandler(this.WeekPropertyChanged);
                    this.Weeks.Add(lastWeek);
                }
                lastWeek.AddDay(wd);
            }
        }

        public void ReloadShortcutStatistic(IEnumerable<ShortCut> shortCuts)
        {
            var orderedShortCutStatistics = shortCuts.OrderBy(s => s, new ShortCutStatisticComparer<ShortCut>())
                .Select(s => new ShortCutStatistic(s));
            this.ShortCutStatistic.AddItems(orderedShortCutStatistics, true);

            this.CalcShortCutStatistic();
        }

        private double previewHours;
        private QuickFillObservableCollection<ShortCutStatistic> shortCutStatistic;
        private double necessaryHours;

        public double PreviewHours
        {
            get { return this.previewHours; }
            set
            {
                if (this.previewHours == value)
                {
                    return;
                }
                this.previewHours = value;
                var tmp = this.PropertyChanged;
                if (tmp != null)
                {
                    tmp(this, new PropertyChangedEventArgs("PreviewHours"));
                }
            }
        }

        public double HoursDuration
        {
            get { return this.Weeks.Sum(i => i.HoursDuration); }
        }

        public int Month
        {
            get { return this.month; }
        }

        public string MonthName
        {
            get
            {
                switch (this.month)
                {
                    case 1:
                        return "Januar";
                    case 2:
                        return "Februar";
                    case 3:
                        return "März";
                    case 4:
                        return "April";
                    case 5:
                        return "Mai";
                    case 6:
                        return "Juni";
                    case 7:
                        return "Juli";
                    case 8:
                        return "August";
                    case 9:
                        return "September";
                    case 10:
                        return "Oktober";
                    case 11:
                        return "November";
                    case 12:
                        return "Dezember";
                }
                return string.Empty;
            }
        }

        public ObservableCollection<WorkWeek> Weeks { get; set; }
        public ObservableCollection<WorkDay> Days { get; set; }

        public QuickFillObservableCollection<ShortCutStatistic> ShortCutStatistic
        {
            get { return this.shortCutStatistic; }
            set
            {
                if (this.shortCutStatistic == value)
                {
                    return;
                }
                this.shortCutStatistic = value;
                var tmp = this.PropertyChanged;
                if (tmp != null)
                {
                    tmp(this, new PropertyChangedEventArgs("ShortCutStatistic"));
                }
            }
        }

        public double NecessaryHours
        {
            get { return necessaryHours; }
            set
            {
                if (this.necessaryHours == value)
                {
                    return;
                }
                this.necessaryHours = value;
                var tmp = this.PropertyChanged;
                if (tmp != null)
                {
                    tmp(this, new PropertyChangedEventArgs("NecessaryHours"));
                }
            }
        }

        public int Year
        {
            get { return this.year; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void WeekPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "HoursDuration")
            {
                var tmp = this.PropertyChanged;
                if (tmp != null)
                {
                    tmp(this, new PropertyChangedEventArgs("HoursDuration"));
                }
                this.CalcPreviewHours();
            }
        }

        public void CalcShortCutStatistic()
        {
            foreach (var scStat in this.ShortCutStatistic)
            {
                scStat.Calculate(this.Days);
            }
        }

        public void CalcPreviewHours()
        {
            this.NecessaryHours = this.Weeks.SelectMany(w => w.Days).Count(d => d.DayType == DayType.Working) * hoursPerDay;
            this.PreviewHours = this.HoursDuration + this.Weeks.SelectMany(w => w.Days).Count(d => d.DayType == DayType.Working && d.HoursDuration == null) * hoursPerDay;
        }

        public override string ToString()
        {
            return string.Format("year:{0},month:{1},weeks:{2},days:{3}", this.Year, this.month, this.Weeks.Count, this.Days.Count);
        }
    }
}