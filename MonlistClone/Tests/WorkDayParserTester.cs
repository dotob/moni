using System.Collections.Generic;
using MonlistClone.Data;
using NUnit.Framework;

namespace MonlistClone.Tests {
  public class WorkDayParserTester {
    [Test]
    public void WDParser_EmptyString_ReturnError() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse(string.Empty, ref wd);
      Assert.IsFalse(workItemParserResult.Success);
      Assert.IsNotEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithDayStartTime_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,2;11111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 0), new TimeItem(9, 0), "11111", string.Empty)}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SetEmptyStringAfterSuccessfulParsing_DeleteItems() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,2;11111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 0), new TimeItem(9, 0), "11111", string.Empty)}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      wd.OriginalString = string.Empty;
      CollectionAssert.IsEmpty(wd.Items);
      Assert.AreEqual(0, wd.HoursDuration);
    }

    [Test]
    public void WDParser_SingleItemWithDayStartTimeAndPos_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 0), new TimeItem(9, 0), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithOddDayStartTime_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 30), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithOddDayStartTimeAndOddHourCount_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,1.5;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 0), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_MoreItems_ReturnWorkItemWithMoreItems() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,1.5;11111-111,3;22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 0), "11111", "111"), new WorkItem(new TimeItem(9, 0), new TimeItem(12, 0), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_MoreItemsAndDayBreak_ReturnWorkItemWithSplittedItems() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser(new WorkDayParserSettings {InsertDayBreak = true, DayBreakTime = new TimeItem(12, 00), DayBreakDurationInMinutes = 30});
      var workItemParserResult = wdp.Parse("9:00,2;11111-111,3;22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "22222", "222"), new WorkItem(new TimeItem(12, 30), new TimeItem(14, 30), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_WhiteSpace_StillWork() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser(new WorkDayParserSettings {InsertDayBreak = true, DayBreakTime = new TimeItem(12, 00), DayBreakDurationInMinutes = 30});
      var workItemParserResult = wdp.Parse("9 : 00 , 2; 11111   -111 , 3;   22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "22222", "222"), new WorkItem(new TimeItem(12, 30), new TimeItem(14, 30), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }


    [Test]
    public void WDParser_UseAbbreviations_ExpandAbbreviations() {
      WorkDay wd = new WorkDay(1, 1, 1);
      Dictionary<string, string> abbr = new Dictionary<string, string>();
      abbr.Add("ctb", "11111-111");
      abbr.Add("ktl", "22222-222");
      abbr.Add("u", "33333-333");
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings {ProjectAbbreviations = abbr};
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,2;ctb,1;u", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "33333", "333")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_InsertPauseItem_LeavePause() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,1;11111-111,2!,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(7, 0), new TimeItem(8, 0), "11111", "111"),
                                        new WorkItem(new TimeItem(10, 0), new TimeItem(12, 0), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_ParseHourFragment_MultiplyBy60() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,1.75;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(7, 0), new TimeItem(8, 45), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_ParseHourFragment2_MultiplyBy60() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("9:15,7.25;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(9, 15), new TimeItem(16, 30), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_ParseDescription_GetDesc() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("9:15,7.25;11111-111(lalala)", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(9, 15), new TimeItem(16, 30), "11111", "111","lalala")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_UseAbbreviationsAndDesc_ExpandAbbreviationsAndOverwriteDescFromAbbr() {
      WorkDay wd = new WorkDay(1, 1, 1);
      Dictionary<string, string> abbr = new Dictionary<string, string>();
      abbr.Add("ctb", "11111-111(donotuseme)");
      abbr.Add("ktl", "22222-222(useme)");
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ProjectAbbreviations = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,2;ctb(useme),2;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] { new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111","useme"), new WorkItem(new TimeItem(11, 0), new TimeItem(13, 0), "22222", "222","useme") }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_InsteadOfHoursICanTellAnEndTime_UseEndTime() {
      WorkDay wd = new WorkDay(1, 1, 1);
      Dictionary<string, string> abbr = new Dictionary<string, string>();
      abbr.Add("ctb", "11111-111");
      abbr.Add("ktl", "22222-222");
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ProjectAbbreviations = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,-12;ctb,-15;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] { new WorkItem(new TimeItem(9, 0), new TimeItem(12, 0), "11111", "111"), new WorkItem(new TimeItem(12, 0), new TimeItem(15, 0), "22222", "222") }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_UsingEndTimeAndBreak_CalculateBreak() {
      WorkDay wd = new WorkDay(1, 1, 1);
      Dictionary<string, string> abbr = new Dictionary<string, string>();
      abbr.Add("ctb", "11111-111");
      abbr.Add("ktl", "22222-222");
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ProjectAbbreviations = abbr,DayBreakDurationInMinutes = 30, InsertDayBreak = true, DayBreakTime = new TimeItem(12)};
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,-14;ctb,-16;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(9, 0), new TimeItem(12, 0), "11111", "111"),
                                        new WorkItem(new TimeItem(12, 30), new TimeItem(14, 0), "11111", "111"), 
                                        new WorkItem(new TimeItem(14, 0), new TimeItem(16, 0), "22222", "222")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_BrokenHours_CalculateCorrectly() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("8:15,-15:30;11111-111,1;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8, 15), new TimeItem(15, 30), "11111", "111"),
                                        new WorkItem(new TimeItem(15, 30), new TimeItem(16, 30), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8.25, wd.HoursDuration);
    }

    [Test]
    public void WDParser_BrokenHoursWithBreak_CalculateCorrectly() {
      WorkDay wd = new WorkDay(1, 1, 1);
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { DayBreakDurationInMinutes = 30, InsertDayBreak = true, DayBreakTime = new TimeItem(12) };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("8:15,-15:30;11111-111,1;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8, 15), new TimeItem(12, 00), "11111", "111"),
                                        new WorkItem(new TimeItem(12, 30), new TimeItem(15, 30), "11111", "111"),
                                        new WorkItem(new TimeItem(15, 30), new TimeItem(16, 30), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(7.75, wd.HoursDuration);
    }
  }
}