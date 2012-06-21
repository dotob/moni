using System;
using System.Collections.Generic;
using System.IO;
using MONI.Data;
using NUnit.Framework;
using Newtonsoft.Json;

namespace MONI.Tests
{
  [TestFixture]
  public class SettingsTester
  {
    [Test]
    public void WriteJson() {
      var parserSettings = new WorkDayParserSettings();
      parserSettings.ShortCuts.Add("ctbn", new ShortCut("25482-420(features)"));
      parserSettings.ShortCuts.Add("ctbp", new ShortCut("25482-811(performanceverbesserungen)"));
      parserSettings.ShortCuts.Add("ctbf", new ShortCut("25482-811(tracker)"));
      parserSettings.ShortCuts.Add("ctbm", new ShortCut("25482-140(meeting)"));
      parserSettings.ShortCuts.Add("ctbr", new ShortCut("25482-050(ac-hh-ac)"));
      parserSettings.ShortCuts.Add("ktln", new ShortCut("25710-420(feature)"));
      parserSettings.ShortCuts.Add("ktlf", new ShortCut("25710-811(tracker)"));
      parserSettings.ShortCuts.Add("ktlm", new ShortCut("25710-140(meeting)"));
      parserSettings.ShortCuts.Add("ktlr", new ShortCut("25710-050(reise)"));
      parserSettings.ShortCuts.Add("u"   , new ShortCut("20030-000(urlaub)"));
      parserSettings.ShortCuts.Add("krank",new ShortCut( "20020-000(krank/doc)"));
      parserSettings.ShortCuts.Add("tm"  , new ShortCut("20018-140(terminalmeeting)"));
      parserSettings.ShortCuts.Add("mm"  , new ShortCut("20018-140(tess/monatsmeeting)"));
      parserSettings.ShortCuts.Add("swe" , new ShortCut("20308-000(swe projekt)"));
      parserSettings.ShortCuts.Add("jmb" , new ShortCut("20308-000(jean-marie ausbildungsbetreuung)"));
      parserSettings.ShortCuts.Add("w"   , new ShortCut("20180-000(weiterbildung)"));
      parserSettings.InsertDayBreak = true;
      parserSettings.DayBreakTime = new TimeItem(12);
      parserSettings.DayBreakDurationInMinutes = 30;

      var mainSettings = new MainSettings();
      mainSettings.SpecialDates = new List<SpecialDate>();
      mainSettings.SpecialDates.Add(new SpecialDate {Name = "Heiligabend", Date = new DateTime(2012,12,24)});
      mainSettings.SpecialDates.Add(new SpecialDate {Name = "Sylvester", Date = new DateTime(2012,1,1)});


      MonlistSettings ms = new MonlistSettings();
      ms.ParserSettings = parserSettings;
      ms.MainSettings = mainSettings;

      var serializeObject = JsonConvert.SerializeObject(ms, Formatting.Indented);
      File.WriteAllText("settings.json.test", serializeObject);
    }
     
  }
}