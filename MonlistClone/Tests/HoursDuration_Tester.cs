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
      WorkMonth wm = new WorkMonth(2011, 1, Enumerable.Empty<SpecialDate>());
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
      WorkMonth wm = new WorkMonth(2011, 1, Enumerable.Empty<SpecialDate>());
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

  }
}