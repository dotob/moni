using System.Collections.Generic;
using MonlistClone.Data;
using NUnit.Framework;
using System.Linq;

namespace MonlistClone.Tests {
  [TestFixture]
  public class HoursDuration_Tester {
    [Test]
    public void HoursDuration_OnADay_ShouldSumCorrect() {
      WorkDay wd = new WorkDay(2011,1,1, Enumerable.Empty<SpecialDate>());
      wd.Items.Add(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);
    }

    [Test]
    public void HoursDuration_OnAWeek_ShouldSumCorrect() {
      WorkMonth wm = new WorkMonth(2011, 1, Enumerable.Empty<SpecialDate>(),new Dictionary<string, ShortCut>());
      WorkDay wd = wm.Days.First();
      wd.Items.Add(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);

      WorkWeek ww = wm.Weeks.First();
      Assert.AreEqual(3,ww.HoursDuration);
    }

    [Test]
    public void HoursDuration_OnAMonth_ShouldSumCorrect() {
      WorkMonth wm = new WorkMonth(2011, 1, Enumerable.Empty<SpecialDate>(), new Dictionary<string, ShortCut>());
      WorkDay wd = wm.Days.First();
      wd.Items.Add(new WorkItem(new TimeItem(10),new TimeItem(11)));
      Assert.AreEqual(1,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(11),new TimeItem(12)));
      Assert.AreEqual(2,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12),new TimeItem(12,15)));
      Assert.AreEqual(2.25,wd.HoursDuration);
      wd.Items.Add(new WorkItem(new TimeItem(12,15),new TimeItem(13)));
      Assert.AreEqual(3,wd.HoursDuration);

      Assert.AreEqual(3,wm.HoursDuration);
    }

    [Test]
    public void ShortCutStatistic_OnAMonth_ShouldSumCorrect() {
      var abbr = new Dictionary<string, ShortCut>();
      abbr.Add("ctb", new ShortCut("11111-111"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr, InsertDayBreak = false };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      WorkDayParser.Instance = wdp;
      WorkMonth wm = new WorkMonth(2011, 1, Enumerable.Empty<SpecialDate>(), abbr);
      WorkDay wd = wm.Days.First();
      wd.OriginalString = "8,8;ctb";

      var scs = wm.ShortCutStatistic.FirstOrDefault(kvp => kvp.Key == "ctb");
      Assert.NotNull(scs);
      Assert.AreEqual(8, scs.Value.UsedInMonth);
    }

  }
}