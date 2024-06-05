using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MONI.Data;
using MONI.Util;
using NUnit.Framework;
using Newtonsoft.Json;

namespace MONI.Tests
{
    [TestFixture]
    public class SettingsTester
    {
        [Test]
        public void WriteJson()
        {
            var parserSettings = new WorkDayParserSettings();
            parserSettings.ShortCuts.Add(new ShortCut("proj1n", "12345-420(features)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj1p", "12345-811(performanceverbesserungen)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj1f", "12345-811(tracker)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj1m", "12345-140(meeting)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj1r", "12345-050(ac-hh-ac)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj2n", "22222-420(feature)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj2f", "22222-811(tracker)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj2m", "22222-140(meeting)"));
            parserSettings.ShortCuts.Add(new ShortCut("proj2r", "22222-050(reise)"));
            parserSettings.ShortCuts.Add(new ShortCut("u", "54321-000(urlaub)"));
            parserSettings.ShortCuts.Add(new ShortCut("krank", "65432-000(krank/doc)"));
            parserSettings.ShortCuts.Add(new ShortCut("tm", "66554-140(terminalmeeting)"));
            parserSettings.ShortCuts.Add(new ShortCut("mm", "66554-140(tess/monatsmeeting)"));
            parserSettings.ShortCuts.Add(new ShortCut("swe", "99998-000(swe projekt)"));
            parserSettings.ShortCuts.Add(new ShortCut("jmb", "99998-000(jean-marie ausbildungsbetreuung)"));
            parserSettings.ShortCuts.Add(new ShortCut("w", "97777-000(weiterbildung)"));
            parserSettings.InsertDayBreak = true;
            parserSettings.DayBreakTime = new TimeItem(12);
            parserSettings.DayBreakDurationInMinutes = 30;

            var mainSettings = new MainSettings();

            MoniSettings ms = new MoniSettings();
            ms.ParserSettings = parserSettings;
            ms.MainSettings = mainSettings;

            var serializeObject = JsonConvert.SerializeObject(ms, Formatting.Indented);
            File.WriteAllText("settings.json.test", serializeObject);
        }

        [Test]
        public void GetValidShortCuts_NoDoubles_ReturnListAsIs()
        {
            var shortCuts = new List<ShortCut>();
            shortCuts.Add(new ShortCut("proj1n", "12345-000"));
            shortCuts.Add(new ShortCut("proj1p", "12345-000"));
            shortCuts.Add(new ShortCut("proj1f", "12345-000"));

            var doesntMatter = DateTime.Now;
            var validShortCuts = WorkDayParserSettings.ValidShortCuts(shortCuts, doesntMatter);
            CollectionAssert.AreEqual(shortCuts, validShortCuts);
        }

        [Test]
        public void GetValidShortCuts_MultipleKeysInterval_ReturnJustRightShortcuts()
        {
            var shortCuts = new List<ShortCut>();
            shortCuts.Add(new ShortCut("proj1n", "12345-000"));
            var findMe = new ShortCut("proj1n", "54321-000", new DateTime(2000, 1, 1));
            shortCuts.Add(findMe);
            var andMe = new ShortCut("proj2n", "22222-420(feature)");
            shortCuts.Add(andMe);

            var matchOnShortcut = new DateTime(2005, 1, 1);
            var validShortCuts = WorkDayParserSettings.ValidShortCuts(shortCuts, matchOnShortcut);
            CollectionAssert.AreEqual(new[] { findMe, andMe }, validShortCuts);
        }

        [Test]
        public void GetValidShortCuts_MultipleKeysDateMatch_ReturnJustRightShortcuts()
        {
            var shortCuts = new List<ShortCut>();
            shortCuts.Add(new ShortCut("proj1n", "12345-000"));
            var matchDate = new DateTime(2000, 1, 1);
            var findMe = new ShortCut("proj1n", "54321-000", matchDate);
            shortCuts.Add(findMe);
            var andMe = new ShortCut("proj2n", "22222-420(feature)");
            shortCuts.Add(andMe);

            var validShortCuts = WorkDayParserSettings.ValidShortCuts(shortCuts, matchDate);
            CollectionAssert.AreEqual(new[] { findMe, andMe }, validShortCuts);
        }

        [Test]
        public void GetValidShortCuts_NoMatch_ReturnNoShortcuts()
        {
            var shortCuts = new List<ShortCut>();
            shortCuts.Add(new ShortCut("proj1n", "54321-000", new DateTime(2001, 1, 1)));
            shortCuts.Add(new ShortCut("proj1n", "54321-000", new DateTime(2002, 1, 1)));
            shortCuts.Add(new ShortCut("proj1n", "54321-000", new DateTime(2003, 1, 1)));

            var validShortCuts = WorkDayParserSettings.ValidShortCuts(shortCuts, new DateTime(2000, 1, 1));
            CollectionAssert.IsEmpty(validShortCuts);
        }

        [Test]
        public void Paths()
        {
            string patchFilePath = Utils.PatchFilePath("#{userhome}");
        }

        [Test]
        public void Regex()
        {
            Regex sr = new Regex(string.Format("(?<pre>.*?)(?<searchText>{0})", "l"), RegexOptions.Compiled);
            Match m = sr.Match("muller");
            while (m.Success)
            {
                m.ToString();
                foreach (Group g in m.Groups)
                {
                    g.ToString();
                }
                m = m.NextMatch();
            }
            MatchCollection matchCollection = sr.Matches("mulller");
            foreach (Match match in matchCollection)
            {
                foreach (var g in match.Groups)
                {
                    g.ToString();
                }
                match.ToString();
            }
        }
    }
}