using System;
using MONI.Data;
using NUnit.Framework;

namespace MONI.Tests {
  public class TimeItemTester {
    [Test]
    public void TryParse_EmptyString_NoSuccess() {
      TimeItem ti;
      Assert.IsFalse(TimeItem.TryParse(string.Empty, out ti));
      Assert.IsNull(ti);
    }
 
    [Test]
    public void TryParse_OnlyHour_ParseIt() {
      TimeItem ti;
      Assert.IsTrue(TimeItem.TryParse("1", out ti));
      Assert.AreEqual(1, ti.Hour);
      Assert.AreEqual(0, ti.Minute);
    }

    [Test]
    public void TryParse_HourAndMinute_ParseIt() {
      TimeItem ti;
      Assert.IsTrue(TimeItem.TryParse("1:2", out ti));
      Assert.AreEqual(1, ti.Hour);
      Assert.AreEqual(2, ti.Minute);
    }

    [Test]
    public void TryParse_HourAndMinuteNoColon_ParseIt() {
      TimeItem ti;
      Assert.IsTrue(TimeItem.TryParse("1600", out ti));
      Assert.AreEqual(16, ti.Hour);
      Assert.AreEqual(0, ti.Minute);
      Assert.IsTrue(TimeItem.TryParse("630", out ti));
      Assert.AreEqual(6, ti.Hour);
      Assert.AreEqual(30, ti.Minute);
    }

    [Test]
    public void TryParse_OutOfRange_Throw() {
      TimeItem ti;
      Assert.Throws<ArgumentOutOfRangeException>(() => TimeItem.TryParse("25:0", out ti));
      Assert.Throws<ArgumentOutOfRangeException>(() => TimeItem.TryParse("-1:0", out ti));
      Assert.Throws<ArgumentOutOfRangeException>(() => TimeItem.TryParse("0:61", out ti));
      Assert.Throws<ArgumentOutOfRangeException>(() => TimeItem.TryParse("0:-1", out ti));
    }

    [Test]
    public void PlusOperator_AddHours_Work() {
      TimeItem ti = new TimeItem(1,0);
      var timeItem = ti + 1;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void PlusOperator_AddMinutes_Work() {
      TimeItem ti = new TimeItem(1,0);
      var timeItem = ti + 0.5;
      Assert.AreEqual(1, timeItem.Hour);
      Assert.AreEqual(30, timeItem.Minute);
    }

    [Test]
    public void PlusOperator_AddMinutes_HourOverflow_Work() {
      TimeItem ti = new TimeItem(1,30);
      var timeItem = ti + 0.75;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(15, timeItem.Minute);
    }

    [Test]
    public void PlusOperator_AddMinutesExact60_HourOverflow_Work() {
      TimeItem ti = new TimeItem(1,30);
      var timeItem = ti + 0.5;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void PlusOperator_AddHoursAndMinutes_Work() {
      TimeItem ti = new TimeItem(1,0);
      var timeItem = ti + 1.5;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(30, timeItem.Minute);
    }

    [Test]
    public void PlusOperator_AddHoursInMinutes_Work() {
      TimeItem ti = new TimeItem(1,30);
      var timeItem = ti + 1.5;
      Assert.AreEqual(3, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractHours_Work() {
      TimeItem ti = new TimeItem(2,0);
      var timeItem = ti - 1;
      Assert.AreEqual(1, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractMinutes_Work() {
      TimeItem ti = new TimeItem(1,0);
      var timeItem = ti - 0.5;
      Assert.AreEqual(0, timeItem.Hour);
      Assert.AreEqual(30, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractMinutes_HourOverflow_Work() {
      TimeItem ti = new TimeItem(1,30);
      var timeItem = ti - 0.75;
      Assert.AreEqual(0, timeItem.Hour);
      Assert.AreEqual(45, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractMinutesExact60_HourOverflow_Work() {
      TimeItem ti = new TimeItem(1,30);
      var timeItem = ti - 0.5;
      Assert.AreEqual(1, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractHoursAndMinutes_Work() {
      TimeItem ti = new TimeItem(4,0);
      var timeItem = ti - 1.5;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(30, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_SubtractHoursInMinutes_Work() {
      TimeItem ti = new TimeItem(3,30);
      var timeItem = ti - 1.5;
      Assert.AreEqual(2, timeItem.Hour);
      Assert.AreEqual(0, timeItem.Minute);
    }

    [Test]
    public void MinusOperator_OutOfRange_Fail() {
      TimeItem ti = new TimeItem(1,0);
      Assert.Throws<ArgumentOutOfRangeException>(() => { var i = ti - 5; });
    }

    [Test]
    public void IsBetween_InsideHourLevel_Work() {
      Assert.IsTrue(new TimeItem(12).IsBetween(new TimeItem(11), new TimeItem(13)));
    }

    [Test]
    public void IsBetween_InsideMinuteLevel_Work() {
      Assert.IsTrue(new TimeItem(12).IsBetween(new TimeItem(11,58), new TimeItem(12,2)));
    }

    [Test]
    public void IsBetween_OutsideHourLevel_Fail() {
      Assert.IsFalse(new TimeItem(13).IsBetween(new TimeItem(11), new TimeItem(12)));
    }
    [Test]
    public void IsBetween_OutsideMinuteLevel_Fail() {
      Assert.IsFalse(new TimeItem(13,13).IsBetween(new TimeItem(11,11), new TimeItem(12,12)));
    }

    [Test]
    public void CompareTo_IsLess_Works() {
      Assert.Less(new TimeItem(10).CompareTo(new TimeItem(11)),0);
      Assert.Less(new TimeItem(10,10).CompareTo(new TimeItem(10,11)),0);
      Assert.Less(new TimeItem(10,10).CompareTo(new TimeItem(11,9)),0);
    }

    [Test]
    public void CompareTo_IsMore_Works() {
      Assert.Greater(new TimeItem(11).CompareTo(new TimeItem(10)),0);
      Assert.Greater(new TimeItem(10, 11).CompareTo(new TimeItem(10, 10)),0);
      Assert.Greater(new TimeItem(11, 9).CompareTo(new TimeItem(10, 10)),0);
    }    
    
    [Test]
    public void CompareTo_IsEqual_Works() {
      Assert.AreEqual(0, new TimeItem(10).CompareTo(new TimeItem(10)));
      Assert.AreEqual(0, new TimeItem(10, 10).CompareTo(new TimeItem(10, 10)));
    }

    [Test]
    public void MinusOperator_Calc_DoitRight() {
      Assert.AreEqual(0, new TimeItem(1,1) - new TimeItem(1,1));
      Assert.AreEqual(0.5, new TimeItem(1,0) - new TimeItem(1,30));
      Assert.AreEqual(0.5, new TimeItem(1,30) - new TimeItem(1,0));
      Assert.AreEqual(1.5, new TimeItem(1,0) - new TimeItem(2,30));
      Assert.AreEqual(0.5, new TimeItem(1,40) - new TimeItem(2,10));
    }

    [Test]
    public void DateTime_StringFormat_DoItRight() {
      var d = new DateTime(2000, 1, 1);
      var s = string.Format("{0:s}", d);
      Assert.AreEqual("2000-01-01T00:00:00", s);
    }

  }
}