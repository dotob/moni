using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MONI.Util;
using Newtonsoft.Json;
using NLog;

namespace MONI.Data
{
    public class ShortCutStatisticComparer<T> : IComparer, IComparer<T> where T : ShortCut
    {
        public int Compare(object x, object y)
        {
            return Compare(x as T, y as T);
        }

        public int Compare(T x, T y)
        {
            if (string.IsNullOrEmpty(x.Group) && !string.IsNullOrEmpty(y.Group))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(y.Group) && !string.IsNullOrEmpty(x.Group))
            {
                return -1;
            }
            var groupCompare = String.Compare(x.Group, y.Group, StringComparison.Ordinal);
            return groupCompare;
        }
    }

    public class ShortCutStatistic : ShortCut
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private ShortCut sourceShortCut;

        public ShortCutStatistic(ShortCut sc)
            : base(sc.Key, sc.Expansion)
        {
            this.sourceShortCut = sc;
            this.ID = sc.ID;
            this.WholeDayExpansion = sc.WholeDayExpansion;
            this.ValidFrom = sc.ValidFrom;
            this.Group = sc.Group;
        }

        public void SetNewGroup(string newGroup, int newIndex)
        {
            this.Group = newGroup;
            this.sourceShortCut.Group = newGroup;
        }

        private double usedInMonth;

        [JsonIgnore]
        public double UsedInMonth
        {
            get { return this.usedInMonth; }
            set { this.Set(ref this.usedInMonth, value); }
        }

        private ObservableCollection<UsageInfo> usageHistory;

        [JsonIgnore]
        public ObservableCollection<UsageInfo> UsageHistory
        {
            get { return this.usageHistory; }
            set { this.Set(ref this.usageHistory, value); }
        }

        public void Calculate(ObservableCollection<WorkDay> days)
        {
            // complete hours over all days
            this.UsedInMonth = days.SelectMany(d => d.Items).Where(i => i.ShortCut != null && Equals(this, i.ShortCut)).Sum(i => i.HoursDuration);

            // generate complete usage information over all days
            var usageInfos =
                (from workDay in days
                 let hours = workDay.Items.Where(i => i.ShortCut != null && Equals(this, i.ShortCut)).Sum(i => i.HoursDuration)
                 select new UsageInfo { Day = workDay.Day, Hours = hours, IsToday = workDay.IsToday }).ToList();

            if (this.UsageHistory == null)
            {
                logger.Debug("CalcShortCutStatistic => {0} Initial calculated shortcut statistics ({1}, {2})", usageInfos.Count(), this.Key, usageInfos.Sum(ui => ui.Hours));
                this.UsageHistory = new QuickFillObservableCollection<UsageInfo>(usageInfos);
            }
            else
            {
                foreach (var ui in this.UsageHistory)
                {
                    var calculatedUI = usageInfos.ElementAtOrDefault(ui.Day - 1);
                    if (calculatedUI != null)
                    {
                        ui.Hours = calculatedUI.Hours;
                        ui.IsToday = calculatedUI.IsToday;
                    }
                    else
                    {
                        logger.Error("CalcShortCutStatistic => No usage info found for day {0}, shortcut {1}!", ui.Day, this.Key);
                    }
                }
            }
        }
    }

    public class UsageInfo : ViewModelBase
    {
        public int Day { get; set; }

        private double hours;

        public double Hours
        {
            get { return this.hours; }
            set { this.Set(ref this.hours, value); }
        }

        private bool isToday;

        public bool IsToday
        {
            get { return this.isToday; }
            set { this.Set(ref this.isToday, value); }
        }
    }
}