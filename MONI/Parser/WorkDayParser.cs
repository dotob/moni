using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MONI.Parser;
using MONI.Util;

namespace MONI.Data
{
    public class WorkDayParser
    {
        public static char dayStartSeparator = ',';
        public static char hourProjectInfoSeparator = ';';
        public static char itemSeparator = ',';
        public static char endTimeStartChar = '-';
        public static char pauseChar = '!';
        public static char projectPositionSeparator = '-';
        public static string automaticPauseDeactivation = "//";
        public WorkDayParserSettings settings;

        public WorkDayParser()
        {
        }

        public WorkDayParser(WorkDayParserSettings settings)
        {
            this.settings = settings;
        }

        public static WorkDayParser Instance { get; set; }

        public WorkDayParserResult Parse(string userInput, ref WorkDay wdToFill)
        {
            // remove newlines
            userInput = userInput.Replace(Environment.NewLine, "");
            ShortCut wholeDayShortcut;
            userInput = this.PreProcessWholeDayExpansion(userInput, wdToFill.DateTime, out wholeDayShortcut);
            bool ignoreBreakSettings = userInput.StartsWith(automaticPauseDeactivation);
            if (ignoreBreakSettings)
            {
                userInput = userInput.Substring(2);
            }
            WorkDayParserResult ret = new WorkDayParserResult();
            if (!String.IsNullOrEmpty(userInput))
            {
                TimeItem dayStartTime;
                string remainingString;
                string error;
                // should be like "<daystarttime>,..."
                // eg 7,... or 7:30,...

                if (this.ForceDurationToZero(userInput))
                {
                    ret.Success = true;
                }
                else if (this.GetDayStartTime(userInput, out dayStartTime, out remainingString, out error))
                {
                    // proceed with parsing items
                    var parts = remainingString.SplitWithIgnoreRegions(new[] { itemSeparator }, new IgnoreRegion('(', ')'));
                    var wdItemsAsString = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
                    if (wdItemsAsString.Any())
                    {
                        List<WorkItemTemp> tmpList = new List<WorkItemTemp>();
                        foreach (var wdItemString in wdItemsAsString)
                        {
                            WorkItemTemp workItem;
                            if (this.GetWDTempItem(wdItemString, out workItem, out error, wdToFill.DateTime, wholeDayShortcut))
                            {
                                tmpList.Add(workItem);
                            }
                            else
                            {
                                ret.Error = error;
                                ret.Success = false;
                                // todo: fail fast??
                            }
                        }
                        IEnumerable<WorkItem> resultList;
                        if (this.ProcessTempWorkItems(dayStartTime, tmpList, ignoreBreakSettings, out resultList, out error))
                        {
                            wdToFill.Clear();
                            foreach (var workItem in resultList)
                            {
                                wdToFill.AddWorkItem(workItem);
                            }
                            ret.Success = true;
                        }
                        else
                        {
                            ret.Error = error;
                        }
                    }
                    else
                    {
                        // this is no error for now
                        ret.Success = true;
                        ret.Error = "Noch keine Einträge gemacht";
                    }
                }
                else
                {
                    ret.Error = error;
                }
            }
            else
            {
                ret.Error = "Noch keine Eingabe";
            }
            return ret;
        }

        private string PreProcessWholeDayExpansion(string userInput, DateTime dateTime, out ShortCut wholeDayShortcut)
        {
            if (this.settings != null)
            {
                var currentShortcuts = this.settings.GetValidShortCuts(dateTime);
                if (currentShortcuts.Any(sc => sc.WholeDayExpansion))
                {
                    var dic = currentShortcuts.Where(sc => sc.WholeDayExpansion).FirstOrDefault(sc => sc.Key == userInput);
                    if (dic != null)
                    {
                        wholeDayShortcut = dic;
                        return dic.Expansion;
                    }
                }
            }
            wholeDayShortcut = null;
            return userInput;
        }

        private bool ProcessTempWorkItems(TimeItem dayStartTime, IEnumerable<WorkItemTemp> tmpList, bool ignoreBreakSettings, out IEnumerable<WorkItem> resultList, out string error)
        {
            bool success = false;
            error = string.Empty;
            List<WorkItem> resultListTmp = new List<WorkItem>();
            TimeItem lastTime = dayStartTime;
            foreach (var workItemTemp in tmpList)
            {
                try
                {
                    // check for pause
                    if (workItemTemp.IsPause)
                    {
                        if (workItemTemp.DesiredEndtime != null)
                        {
                            lastTime = workItemTemp.DesiredEndtime;
                        }
                        else
                        {
                            lastTime += workItemTemp.HourCount;
                        }
                    }
                    else
                    {
                        bool endTimeMode = false; // if endTimeMode do not add, but substract break!
                        TimeItem currentEndTime;
                        if (workItemTemp.DesiredEndtime != null)
                        {
                            currentEndTime = workItemTemp.DesiredEndtime;
                            endTimeMode = true;
                        }
                        else
                        {
                            currentEndTime = lastTime + workItemTemp.HourCount;
                        }
                        // check for split
                        if (this.settings != null && this.settings.InsertDayBreak && !ignoreBreakSettings)
                        {
                            if (endTimeMode && currentEndTime.IsBetween(this.settings.DayBreakTime, this.settings.DayBreakTime + this.settings.DayBreakDurationInMinutes / 60d))
                            {
                                success = false;
                                error = string.Format("Der Eintrag {0} hat eine absolute Endzeit und endet in der automatischen Tagespause( {1} + {2}min ). Das ist nicht auflösbar.", workItemTemp.OriginalString, this.settings.DayBreakTime, this.settings.DayBreakDurationInMinutes);
                                break;
                            }
                            // the break is in an item
                            if (this.settings.DayBreakTime.IsBetween(lastTime, currentEndTime))
                            {
                                // insert new item
                                resultListTmp.Add(new WorkItem(lastTime, this.settings.DayBreakTime, workItemTemp.ProjectString, workItemTemp.PosString, workItemTemp.Description, workItemTemp.ShortCut, workItemTemp.OriginalString));
                                lastTime = this.settings.DayBreakTime + this.settings.DayBreakDurationInMinutes / 60d;
                                if (!endTimeMode)
                                {
                                    // fixup currentEndTime, need to add the dayshiftbreak
                                    currentEndTime = currentEndTime + this.settings.DayBreakDurationInMinutes / 60d;
                                }
                            }
                            else if (this.settings.DayBreakTime.Equals(lastTime))
                            {
                                lastTime = lastTime + this.settings.DayBreakDurationInMinutes / 60d;
                                if (!endTimeMode)
                                {
                                    currentEndTime = currentEndTime + this.settings.DayBreakDurationInMinutes / 60d;
                                }
                            }
                        }
                        resultListTmp.Add(new WorkItem(lastTime, currentEndTime, workItemTemp.ProjectString, workItemTemp.PosString, workItemTemp.Description, workItemTemp.ShortCut, workItemTemp.OriginalString));
                        lastTime = currentEndTime;
                        success = true;
                    }
                }
                catch (Exception exception)
                {
                    error = string.Format("Beim Verarbeiten von \"{0}\" ist dieser Fehler aufgetreten: \"{1}\"", workItemTemp.OriginalString, exception.Message);
                    success = false;
                }
            }
            resultList = resultListTmp;
            return success;
        }

        private bool GetWDTempItem(string wdItemString, out WorkItemTemp workItem, out string error, DateTime dateTime, ShortCut wholeDayShortcut)
        {
            bool success = false;
            workItem = null;
            error = string.Empty;
            // check for pause item
            if (wdItemString.EndsWith(pauseChar.ToString()))
            {
                if (wdItemString.StartsWith(endTimeStartChar.ToString()))
                {
                    TimeItem ti;
                    var parsePart = wdItemString.Substring(1, wdItemString.Length - 2);
                    var parsePartNoComment = parsePart.TokenReturnInputIfFail("(", 1);
                    if (TimeItem.TryParse(parsePartNoComment, out ti))
                    {
                        workItem = new WorkItemTemp(wdItemString);
                        workItem.DesiredEndtime = ti;
                        workItem.IsPause = true;
                        success = true;
                    }
                }
                else
                {
                    double pauseDuration;
                    var parsePart = wdItemString.Substring(0, wdItemString.Length - 1);
                    var parsePartNoComment = parsePart.TokenReturnInputIfFail("(", 1);
                    if (double.TryParse(parsePartNoComment, NumberStyles.Float, CultureInfo.InvariantCulture, out pauseDuration))
                    {
                        workItem = new WorkItemTemp(wdItemString);
                        workItem.HourCount = pauseDuration;
                        workItem.IsPause = true;
                        success = true;
                    }
                }
            }
            else
            {
                // workitem: <count of hours|-endtime>;<projectnumber>-<positionnumber>[(<description>)]
                var timeString = wdItemString.Token(hourProjectInfoSeparator.ToString(), 1, wdItemString).Trim();
                if (!string.IsNullOrEmpty(timeString))
                {
                    if (timeString.StartsWith(endTimeStartChar.ToString()))
                    {
                        TimeItem ti;
                        if (TimeItem.TryParse(timeString.Substring(1), out ti))
                        {
                            workItem = new WorkItemTemp(wdItemString);
                            workItem.DesiredEndtime = ti;
                        }
                        else
                        {
                            error = string.Format("Die Endzeit kann nicht erkannt werden: {0}", timeString);
                        }
                    }
                    else
                    {
                        double hours;
                        if (double.TryParse(timeString, NumberStyles.Float, CultureInfo.InvariantCulture, out hours))
                        {
                            workItem = new WorkItemTemp(wdItemString);
                            workItem.HourCount = hours;
                        }
                        else
                        {
                            error = string.Format("Die Stundeninfo kann nicht erkannt werden: {0}", timeString);
                        }
                    }
                    if (workItem != null)
                    {
                        var projectPosDescString = wdItemString.Substring(wdItemString.IndexOf(hourProjectInfoSeparator) + 1).Trim();
                        if (!string.IsNullOrEmpty(projectPosDescString))
                        {
                            // expand abbreviations
                            if (this.settings != null)
                            {
                                var abbrevStringNoComment = projectPosDescString.TokenReturnInputIfFail("(", 1).Trim();
                                var abbrevString = abbrevStringNoComment.TokenReturnInputIfFail("-", 1).Trim();
                                var posReplaceString = abbrevStringNoComment.Token("-", 2).Trim();
                                ShortCut shortCut = this.settings.GetValidShortCuts(dateTime).Where(s => !s.WholeDayExpansion).FirstOrDefault(s => s.Key == abbrevString);
                                if (shortCut != null)
                                {
                                    workItem.ShortCut = shortCut;
                                    var expanded = shortCut.Expansion;
                                    // if there is an desc given use its value instead of the one in the abbrev
                                    var desc = DescriptionParser.ParseDescription(projectPosDescString);
                                    var descExpanded = DescriptionParser.ParseDescription(expanded);
                                    if (!string.IsNullOrWhiteSpace(desc.Description) && desc.UsedAppendDelimiter)
                                    {
                                        // append description in expanded
                                        expanded = string.Format("{0}({1}{2})", this.replacePosIfNecessary(descExpanded.BeforeDescription, posReplaceString), descExpanded.Description, desc.Description);
                                    }
                                    else if (!string.IsNullOrWhiteSpace(desc.Description))
                                    {
                                        // replace to description in expanded
                                        expanded = string.Format("{0}({1})", this.replacePosIfNecessary(descExpanded.BeforeDescription, posReplaceString), desc.Description);
                                    }
                                    else
                                    {
                                        expanded = this.replacePosIfNecessary(expanded, posReplaceString);
                                    }
                                    projectPosDescString = expanded;
                                }
                                else if (wholeDayShortcut != null)
                                {
                                    workItem.ShortCut = wholeDayShortcut;
                                }
                            }

                            var projectPosString = projectPosDescString.TokenReturnInputIfFail("(", 1);
                            var parts = projectPosString.Split(projectPositionSeparator).Select(s => s.Trim()).ToList();
                            if (parts.Any())
                            {
                                workItem.ProjectString = parts.ElementAtOrDefault(0);
                                workItem.PosString = parts.ElementAtOrDefault(1) ?? string.Empty;
                                success = true;
                            }
                            else
                            {
                                error = string.Format("Projektnummer kann nicht erkannt werden: {0}", projectPosDescString);
                            }
                            var descNoExpand = DescriptionParser.ParseDescription(projectPosDescString);
                            if (!string.IsNullOrWhiteSpace(descNoExpand.Description))
                            {
                                workItem.Description = descNoExpand.Description;
                            }
                        }
                        else
                        {
                            error = string.Format("Projektnummer ist leer: {0}", wdItemString);
                        }
                    }
                }
                else
                {
                    error = string.Format("Stundenanzahl kann nicht erkannt werden: {0}", wdItemString);
                }
            }
            return success;
        }

        private string replacePosIfNecessary(string beforeDescription, string posReplacement)
        {
            if (!string.IsNullOrWhiteSpace(posReplacement))
            {
                return string.Format("{0}-{1}", beforeDescription.TokenReturnInputIfFail("-", 1), posReplacement);
            }
            return beforeDescription;
        }

        private bool ForceDurationToZero(string input)
        {
            var dayStartToken = input.Token(dayStartSeparator.ToString(), 1, input); // do not trim here, need original length later
            bool forceDurationToZero = dayStartToken.Trim().Equals(pauseChar.ToString());
            bool noFurtherItemsToParse = dayStartToken.Length + 1 >= input.TrimEnd().Length;

            return forceDurationToZero && noFurtherItemsToParse;
        }

        private bool GetDayStartTime(string input, out TimeItem dayStartTime, out string remainingString, out string error)
        {
            bool success = false;
            var dayStartToken = input.Token(dayStartSeparator.ToString(), 1, input); // do not trim here, need original length later
            if (!string.IsNullOrEmpty(dayStartToken.Trim()))
            {
                if (TimeItem.TryParse(dayStartToken, out dayStartTime))
                {
                    remainingString = dayStartToken.Length < input.Length ? input.Substring(dayStartToken.Length + 1) : string.Empty; // seems like no daystartseparator
                    error = string.Empty;
                    success = true;
                }
                else
                {
                    remainingString = input;
                    error = string.Format("Tagesbeginn wird nicht erkannt: {0}", dayStartToken);
                }
            }
            else
            {
                error = "no daystart found";
                dayStartTime = null;
                remainingString = input;
            }
            return success;
        }

        public string AddCurrentTime(string originalString)
        {
            // test for daystart
            string newString = originalString;
            if (string.IsNullOrWhiteSpace(originalString))
            {
                newString += TimeItem.Now.ToString();
            }
            else
            {
                if (!originalString.EndsWith(itemSeparator.ToString()))
                {
                    newString += itemSeparator;
                }
                newString += endTimeStartChar + TimeItem.Now.ToString();
            }
            return newString;
        }
    }

    public class WorkDayParserResult
    {
        public WorkDayParserResult()
        {
            this.Error = string.Empty;
        }

        public bool Success { get; set; }
        public string Error { get; set; }
    }

    internal class WorkItemTemp
    {
        public WorkItemTemp(string originalString)
        {
            this.OriginalString = originalString;
        }

        public bool IsPause { get; set; }
        public double HourCount { get; set; }
        public TimeItem DesiredEndtime { get; set; }
        public string ProjectString { get; set; }
        public string PosString { get; set; }
        public string Description { get; set; }
        public string OriginalString { get; set; }
        public ShortCut ShortCut { get; set; }
    }
}