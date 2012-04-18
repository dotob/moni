using System;
using System.Collections.Generic;
using System.IO;
using MonlistClone.Data;
using NUnit.Framework;
using Newtonsoft.Json;

namespace MonlistClone.Tests
{
  [TestFixture]
  public class SettingsTester
  {
    [Test]
    public void WriteJson() {
      var parserSettings = new WorkDayParserSettings();
      parserSettings.ProjectAbbreviations.Add("ctbn", "25482-420(features)");
      parserSettings.ProjectAbbreviations.Add("ctbp", "25482-811(performanceverbesserungen)");
      parserSettings.ProjectAbbreviations.Add("ctbf", "25482-811(tracker)");
      parserSettings.ProjectAbbreviations.Add("ctbm", "25482-140(meeting)");
      parserSettings.ProjectAbbreviations.Add("ctbr", "25482-050(ac-hh-ac)");
      parserSettings.ProjectAbbreviations.Add("ktln", "25710-420(feature)");
      parserSettings.ProjectAbbreviations.Add("ktlf", "25710-811(tracker)");
      parserSettings.ProjectAbbreviations.Add("ktlm", "25710-140(meeting)");
      parserSettings.ProjectAbbreviations.Add("ktlr", "25710-050(reise)");
      parserSettings.ProjectAbbreviations.Add("u", "20030-000(urlaub)");
      parserSettings.ProjectAbbreviations.Add("krank", "20020-000(krank/doc)");
      parserSettings.ProjectAbbreviations.Add("tm", "20018-140(terminalmeeting)");
      parserSettings.ProjectAbbreviations.Add("mm", "20018-140(tess/monatsmeeting)");
      parserSettings.ProjectAbbreviations.Add("swe", "20308-000(swe projekt)");
      parserSettings.ProjectAbbreviations.Add("jmb", "20308-000(jean-marie ausbildungsbetreuung)");
      parserSettings.ProjectAbbreviations.Add("w", "20180-000(weiterbildung)");
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