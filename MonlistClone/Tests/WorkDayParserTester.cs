﻿using System.Collections.Generic;
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
  }
}