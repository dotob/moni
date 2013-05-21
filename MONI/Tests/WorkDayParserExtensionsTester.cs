using System;
using System.Collections.Generic;
using MONI.Data;
using NUnit.Framework;

namespace MONI.Tests
{
  [TestFixture]
  public class WorkDayParserExtensionsTester
  {
    WorkDayParser wdp = new WorkDayParser();

    [Test]
    public void WDPCurserUp_StringEmpty_DoNothing() {
      int pos = 0;
      var newText = wdp.Increment(string.Empty, 1, ref pos);
      Assert.AreEqual(string.Empty, newText);
    }

    [Test]
    public void WDPCurserUp_StringNull_DoNothing() {
      int pos = 0;
      var newText = wdp.Increment(null, 1, ref pos);
      Assert.AreEqual(string.Empty, newText);
    }

    [Test]
    public void WDPCurserUp_HourStringEasy_Increment() {
      int pos = 2;
      var newText = wdp.Increment("9,4;12345-000,-16:00;12345-000", 1, ref pos);
      Assert.AreEqual("9,4.25;12345-000,-16:00;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_HourStringHourOverflow_Increment() {
      int pos = 2;
      var newText = wdp.Increment("9,4;12345-000,-16:00;12345-000", 5, ref pos);
      Assert.AreEqual("9,5.25;12345-000,-16:00;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_EndTimeStringEasy_Increment() {
      int pos = 15;
      var newText = wdp.Increment("9,4;12345-000,-16:00;12345-000", 1, ref pos);
      Assert.AreEqual("9,4;12345-000,-16:15;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_EndTimeStringHourOverflow_Increment() {
      int pos = 15;
      var newText = wdp.Increment("9,4;12345-000,-16:00;12345-000", 5, ref pos);
      Assert.AreEqual("9,4;12345-000,-17:15;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_HourStringEasy_Decrement() {
      int pos = 2;
      var newText = wdp.Decrement("9,4;12345-000,-16:00;12345-000", 1, ref pos);
      Assert.AreEqual("9,3.75;12345-000,-16:00;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_HourStringHourOverflow_Decrement() {
      int pos = 2;
      var newText = wdp.Decrement("9,4;12345-000,-16:00;12345-000", 5, ref pos);
      Assert.AreEqual("9,2.75;12345-000,-16:00;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_EndTimeStringEasy_Decrement() {
      int pos = 15;
      var newText = wdp.Decrement("9,4;12345-000,-16:00;12345-000", 1, ref pos);
      Assert.AreEqual("9,4;12345-000,-15:45;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_EndTimeStringHourOverflow_Decrement() {
      int pos = 15;
      var newText = wdp.Decrement("9,4;12345-000,-16:00;12345-000", 5, ref pos);
      Assert.AreEqual("9,4;12345-000,-14:45;12345-000", newText);
    }

    [Test]
    public void WDPCurserUp_DayStart_Increment() {
      int pos = 1;
      var newText = wdp.Increment("9,4;12345-000,-16:00;12345-000", 1, ref pos);
      Assert.AreEqual("9:15,4;12345-000,-16:00;12345-000", newText);
    }

    [Test]
    public void WDPFindPositionPart_Pos0_Return0() {
      var parts = wdp.SplitIntoParts("9,4;12345-000,-16:00;12345-000");
      int idx;
      var newText = wdp.FindPositionPart(parts, 2, out idx);
      Assert.AreEqual("4", newText, "for pos 2");
      newText = wdp.FindPositionPart(parts, 3, out idx);
      Assert.AreEqual("4", newText, "for pos 3");
    }

    [Test]
    public void WDPFindPositionPart_Pos2_Return0() {
      var parts = wdp.SplitIntoParts("8");
      int idx;
      var newText = wdp.FindPositionPart(parts, 1, out idx);
      Assert.AreEqual("8", newText);
    }

    [Test]
    public void WDPFindPositionPart_Pos1_Return0() {
      var parts = wdp.SplitIntoParts("9,4;12345-000,-16:00;12345-001");
      for (int i = 14; i <= 20; i++) {
        int idx;
        var newText = wdp.FindPositionPart(parts, i, out idx);
        Assert.AreEqual("-16:00", newText, string.Format("wrong text pos:{0}", i));
        Assert.AreEqual(6, idx);
      }
    }

    [Test]
    public void WDPSplitIntoParts_ConsiderNewline() {
      var parts = wdp.SplitIntoParts(@"9,4;12345-000,
-16:00;12345-001,
-17:00;12345-001");
      CollectionAssert.AreEqual(new[] { "9", ",", "4", ";", "12345-000", ",", Environment.NewLine, "-16:00", ";", "12345-001", ",", Environment.NewLine, "-17:00", ";", "12345-001" }, parts);
    }
  }
}