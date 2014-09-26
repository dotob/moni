using System.Collections.Generic;
using MONI.Data;
using NUnit.Framework;
using System.Linq;

namespace MONI.Tests {
  [TestFixture]
  public class HoursDuration_Tester {
    [Test]
    public void HoursDuration_OnADay_ShouldSumCorrect() {
      WorkDay wd = new WorkDay(2011,1,1, null);
      wd.AddWorkItem(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);
    }

    [Test]
    public void HoursDuration_OnAWeek_ShouldSumCorrect() {
      WorkMonth wm = new WorkMonth(2011, 1, null, new List<ShortCut>(), 1);
      WorkDay wd = wm.Days.First();
      wd.AddWorkItem(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);

      WorkWeek ww = wm.Weeks.First();
      Assert.AreEqual(3,ww.HoursDuration);
    }

    [Test]
    public void HoursDuration_OnAMonth_ShouldSumCorrect() {
      WorkMonth wm = new WorkMonth(2011, 1, null, new List<ShortCut>(), 1);
      WorkDay wd = wm.Days.First();
      wd.AddWorkItem(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.AddWorkItem(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);

      Assert.AreEqual(3,wm.HoursDuration);
    }

    [Test]
    public void ShortCutStatistic_OnAMonth_ShouldSumCorrect() {
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("ctb", "11111-111"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr, InsertDayBreak = false };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      WorkDayParser.Instance = wdp;
      WorkMonth wm = new WorkMonth(2011, 1, null, abbr, 1);
      WorkDay wd = wm.Days.First();
      wd.OriginalString = "8,8;ctb";

      wm.CalcShortCutStatistic();
      var scs = wm.ShortCutStatistic.FirstOrDefault(s => s.Key == "ctb");
      Assert.NotNull(scs);
      Assert.AreEqual(8, scs.UsedInMonth);
    }

  }
}