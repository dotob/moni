using System.Collections.Generic;
using System.Linq;
using MONI.Data;
using NUnit.Framework;

namespace MONI.Tests {
  public class WorkDayParserMainTester {
    [Test]
    public void WDParser_EmptyString_ReturnError() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse(string.Empty, ref wd);
      Assert.IsFalse(workItemParserResult.Success);
      Assert.IsNotEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithDayStartTime_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,2;11111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 0), new TimeItem(9, 0), "11111", string.Empty)}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SetEmptyStringAfterSuccessfulParsing_DeleteItems() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
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
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 0), new TimeItem(9, 0), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithOddDayStartTime_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 30), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_SingleItemWithOddDayStartTimeAndOddHourCount_ReturnWorkItemWithOneItem() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,1.5;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 0), "11111", "111")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_MoreItems_ReturnWorkItemWithMoreItems() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7:30,1.5;11111-111,3;22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(7, 30), new TimeItem(9, 0), "11111", "111"), new WorkItem(new TimeItem(9, 0), new TimeItem(12, 0), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_MoreItemsAndDayBreak_ReturnWorkItemWithSplittedItems() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser(new WorkDayParserSettings {InsertDayBreak = true, DayBreakTime = new TimeItem(12, 00), DayBreakDurationInMinutes = 30});
      var workItemParserResult = wdp.Parse("9:00,2;11111-111,3;22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "22222", "222"), new WorkItem(new TimeItem(12, 30), new TimeItem(14, 30), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_LokalBreakSettingsOptOut_IgnoreBreakSettings() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser(new WorkDayParserSettings {InsertDayBreak = true, DayBreakTime = new TimeItem(12, 00), DayBreakDurationInMinutes = 30});
      var workItemParserResult = wdp.Parse("//9:00,2;11111-111,3;22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(14, 00), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_WhiteSpace_StillWork() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser(new WorkDayParserSettings {InsertDayBreak = true, DayBreakTime = new TimeItem(12, 00), DayBreakDurationInMinutes = 30});
      var workItemParserResult = wdp.Parse("9 : 00 , 2; 11111   -111 , 3;   22222-222", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "22222", "222"), new WorkItem(new TimeItem(12, 30), new TimeItem(14, 30), "22222", "222")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }


    [Test]
    public void WDParser_UseAbbreviations_ExpandAbbreviations() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("ctb", "11111-111"));
      abbr.Add(new ShortCut("ktl", "22222-222"));
      abbr.Add(new ShortCut("u", "33333-333"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings {ShortCuts = abbr};
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,2;ctb,1;u", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111"), new WorkItem(new TimeItem(11, 0), new TimeItem(12, 00), "33333", "333")}, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_InsertTimeIntervalPauseItem_LeavePause() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
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
    public void WDParser_InsertEndTimePauseItem_LeavePause() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("7,1;11111-111,-10:30!,2;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(7, 0), new TimeItem(8, 0), "11111", "111"),
                                        new WorkItem(new TimeItem(10, 30), new TimeItem(12, 30), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_ParseHourFragment_MultiplyBy60() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
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
      WorkDay wd = new WorkDay(1, 1, 1, null);
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
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("9:15,7.25;11111-111(lalala)", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(9, 15), new TimeItem(16, 30), "11111", "111","lalala", null)
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_ParseDescriptionWithItemSeparator_GetDesc() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParser wdp = new WorkDayParser();
      var workItemParserResult = wdp.Parse("9:15,7.25;11111-111(lal,ala)", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(9, 15), new TimeItem(16, 30), "11111", "111","lal,ala", null)
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_UseAbbreviationsAndDesc_ExpandAbbreviationsAndOverwriteDescFromAbbr() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("ctb", "11111-111(donotuseme)"));
      abbr.Add( new ShortCut("ktl","22222-222(useme)"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,2;ctb(useme),2;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] { new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111","useme", null), new WorkItem(new TimeItem(11, 0), new TimeItem(13, 0), "22222", "222","useme", null) }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_UseAbbreviationsAndDesc_ExpandAbbreviationsAndAppendToDescFromAbbr() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("ctb", "11111-111(prefix)"));
      abbr.Add( new ShortCut("ktl","22222-222(useme)"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,2;ctb(+ suffix),2;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] { new WorkItem(new TimeItem(9, 0), new TimeItem(11, 0), "11111", "111","prefix suffix", null), new WorkItem(new TimeItem(11, 0), new TimeItem(13, 0), "22222", "222","useme", null) }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_InsteadOfHoursICanTellAnEndTime_UseEndTime() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add( new ShortCut("ctb","11111-111"));
      abbr.Add(new ShortCut("ktl", "22222-222"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("9:00,-12;ctb,-15;ktl", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] { new WorkItem(new TimeItem(9, 0), new TimeItem(12, 0), "11111", "111"), new WorkItem(new TimeItem(12, 0), new TimeItem(15, 0), "22222", "222") }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
    }

    [Test]
    public void WDParser_UsingEndTimeAndBreak_CalculateBreak() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("ctb","11111-111"));
      abbr.Add(new ShortCut("ktl", "22222-222"));
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr,DayBreakDurationInMinutes = 30, InsertDayBreak = true, DayBreakTime = new TimeItem(12)};
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
      WorkDay wd = new WorkDay(1, 1, 1, null);
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
      WorkDay wd = new WorkDay(1, 1, 1, null);
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

    [Test]
    public void WDParser_PartEndsAtBreakTime_AddBreakCorrectly() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { DayBreakDurationInMinutes = 30, InsertDayBreak = true, DayBreakTime = new TimeItem(12) };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("8,4;11111-111,4;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(12, 00), "11111", "111"),
                                        new WorkItem(new TimeItem(12, 30), new TimeItem(16, 30), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);
    }

    [Test]
    public void WDParser_PartEndsAtBreakTimeWithAbsolutEnd_AddBreakCorrectly() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { DayBreakDurationInMinutes = 30, InsertDayBreak = true, DayBreakTime = new TimeItem(12) };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("8,4;11111-111,-17:00;11111-111", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(12, 00), "11111", "111"),
                                        new WorkItem(new TimeItem(12, 30), new TimeItem(17), "11111", "111")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8.5, wd.HoursDuration);
    }

    [Test]
    public void WDParser_HoleDayExpansion_UseCompleteString() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("krank", "8,8;11111-111(krank)") { WholeDayExpansion = true});
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("krank", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(16), "11111", "111","krank")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);
    }

    [Test]
    public void WDParser_HoleDayExpansionAndNormalExpansionShareSameKey_ReturnBothExpansions() {
      var abbr = new List<ShortCut>();
      abbr.Add(new ShortCut("a", "11111-111(b)") { WholeDayExpansion = false});
      abbr.Add(new ShortCut("a", "8,8;11111-111(b)") { WholeDayExpansion = true});
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      // find wholeday expansion
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var workItemParserResult = wdp.Parse("a", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(16), "11111", "111","b")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);

      // find normal expansion
      wd = new WorkDay(1, 1, 2, null);
      workItemParserResult = wdp.Parse("8,8;a", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(16), "11111", "111","b")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);

    }


    [Test]
    public void WDParser_ShortcutLinkInWorkItem_NormalShortcut() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      var shortCut = new ShortCut("a", "11111-111(aa)");
      abbr.Add(shortCut);
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("8,8;a", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(16), "11111", "111","aa")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);
      Assert.AreEqual(shortCut, wd.Items.First().ShortCut);
    }

    [Test]
    public void WDParser_ShortcutLinkInWorkItem_WholeDayShortcut() {
      WorkDay wd = new WorkDay(1, 1, 1, null);
      var abbr = new List<ShortCut>();
      var shortCut = new ShortCut("a", "8,8;11111-111(aa)") { WholeDayExpansion = true };
      abbr.Add(shortCut);
      WorkDayParserSettings workDayParserSettings = new WorkDayParserSettings { ShortCuts = abbr };
      WorkDayParser wdp = new WorkDayParser(workDayParserSettings);
      var workItemParserResult = wdp.Parse("a", ref wd);
      Assert.IsTrue(workItemParserResult.Success, workItemParserResult.Error);
      CollectionAssert.IsNotEmpty(wd.Items);
      CollectionAssert.AreEqual(new[] {
                                        new WorkItem(new TimeItem(8), new TimeItem(16), "11111", "111","aa")
                                      }, wd.Items);
      Assert.IsEmpty(workItemParserResult.Error);
      Assert.AreEqual(8, wd.HoursDuration);
      Assert.AreEqual(shortCut, wd.Items.First().ShortCut);
    }
  }
}