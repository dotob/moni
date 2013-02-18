using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MONI.Util;

namespace MONI.Data
{
  public class WorkDayParser
  {
    private readonly string dayStartSeparator = ",";
    private readonly string hourProjectInfoSeparator = ";";
    private readonly char itemSeparator = ',';
    private readonly WorkDayParserSettings settings;

    public WorkDayParser() {
    }

    public WorkDayParser(WorkDayParserSettings settings) {
      this.settings = settings;
    }

    public static WorkDayParser Instance { get; set; }

    public WorkDayParserResult Parse(string userInput, ref WorkDay wdToFill) {
      // remove newlines
      userInput = userInput.Replace(Environment.NewLine, "");
      ShortCut wholeDayShortcut;
      userInput = PreProcessWholeDayExpansion(userInput, wdToFill.DateTime, out wholeDayShortcut);
      bool ignoreBreakSettings = userInput.StartsWith("//");
      if (ignoreBreakSettings) {
        userInput = userInput.Substring(2);
      }
      WorkDayParserResult ret = new WorkDayParserResult();
      if (!String.IsNullOrEmpty(userInput)) {
        TimeItem dayStartTime;
        string remainingString;
        string error;
        // should be like "<daystarttime>,..."
        // eg 7,... or 7:30,...
        if (this.GetDayStartTime(userInput, out dayStartTime, out remainingString, out error)) {
          // proceed with parsing items
          var wdItemsAsString = remainingString.Split(this.itemSeparator).Select(i => i.Trim()).ToList();
          if (wdItemsAsString.Any()) {
            List<WorkItemTemp> tmpList = new List<WorkItemTemp>();
            foreach (var wdItemString in wdItemsAsString) {
              WorkItemTemp workItem;
              if (this.GetWDItem(wdItemString, out workItem, out error, wdToFill.DateTime, wholeDayShortcut)) {
                tmpList.Add(workItem);
              } else {
                ret.Error = error;
                ret.Success = false;
                // todo: fail fast??
              }
            }
            IEnumerable<WorkItem> resultList;
            if (this.ProcessTempWorkItems(dayStartTime, tmpList, ignoreBreakSettings, out resultList, out error)) {
              wdToFill.Clear();
              foreach (var workItem in resultList) {
                wdToFill.AddWorkItem(workItem);
              }
              ret.Success = true;
            }
          } else {
            ret.Error = "no items found";
          }
        } else {
          ret.Error = error;
        }
      } else {
        ret.Error = "empty string given";
      }
      return ret;
    }

    private string PreProcessWholeDayExpansion(string userInput, DateTime dateTime, out ShortCut wholeDayShortcut) {
      if (this.settings != null) {
        var currentShortcuts = this.settings.GetValidShortCuts(dateTime);
        if (currentShortcuts.Any(sc => sc.WholeDayExpansion)) {
          var dic = currentShortcuts.Where(sc => sc.WholeDayExpansion).FirstOrDefault(sc => sc.Key == userInput);
          if (dic != null) {
            wholeDayShortcut = dic;
            return dic.Expansion;
          }
        }
      }
      wholeDayShortcut = null;
      return userInput;
    }

    private bool ProcessTempWorkItems(TimeItem dayStartTime, IEnumerable<WorkItemTemp> tmpList, bool ignoreBreakSettings, out IEnumerable<WorkItem> resultList, out string error) {
      bool success = false;
      error = string.Empty;
      List<WorkItem> resultListTmp = new List<WorkItem>();
      TimeItem lastTime = dayStartTime;
      foreach (var workItemTemp in tmpList) {
        // check for pause
        if (workItemTemp.IsPause) {
          lastTime += workItemTemp.HourCount;
        } else {
          bool endTimeMode = false; // if endTimeMode do not add, but substract break!
          TimeItem currentEndTime;
          if (workItemTemp.DesiredEndtime != null) {
            currentEndTime = workItemTemp.DesiredEndtime;
            endTimeMode = true;
          } else {
            currentEndTime = lastTime + workItemTemp.HourCount;
          }
          // check for split
          if (this.settings != null && this.settings.InsertDayBreak && !ignoreBreakSettings) {
            // the break is in an item
            if (this.settings.DayBreakTime.IsBetween(lastTime, currentEndTime)) {
              // insert new item
              resultListTmp.Add(new WorkItem(lastTime, this.settings.DayBreakTime, workItemTemp.ProjectString, workItemTemp.PosString, workItemTemp.Description, workItemTemp.ShortCut));
              lastTime = this.settings.DayBreakTime + this.settings.DayBreakDurationInMinutes / 60d;
              if (!endTimeMode) {
                // fixup currentEndTime, need to add the dayshiftbreak
                currentEndTime = currentEndTime + this.settings.DayBreakDurationInMinutes / 60d;
              }
            } else if (this.settings.DayBreakTime.Equals(lastTime)) {
              lastTime = lastTime + this.settings.DayBreakDurationInMinutes / 60d;
              if (!endTimeMode) {
                currentEndTime = currentEndTime + this.settings.DayBreakDurationInMinutes / 60d;
              }
            }
          }
          resultListTmp.Add(new WorkItem(lastTime, currentEndTime, workItemTemp.ProjectString, workItemTemp.PosString, workItemTemp.Description, workItemTemp.ShortCut));
          lastTime = currentEndTime;
          success = true;
        }
      }
      resultList = resultListTmp;
      return success;
    }

    private bool GetWDItem(string wdItemString, out WorkItemTemp workItem, out string error, DateTime dateTime, ShortCut wholeDayShortcut) {
      bool success = false;
      workItem = null;
      error = string.Empty;
      // check for pause item
      if (wdItemString.EndsWith("!")) {
        double pauseDuration;
        if (double.TryParse(wdItemString.Substring(0, wdItemString.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out pauseDuration)) {
          workItem = new WorkItemTemp();
          workItem.HourCount = pauseDuration;
          workItem.IsPause = true;
          success = true;
        }
      } else {
        // workitem: <count of hours|-endtime>;<projectnumber>-<positionnumber>[(<description>)]
        var timeString = wdItemString.Token(this.hourProjectInfoSeparator, 1).Trim();
        if (!string.IsNullOrEmpty(timeString)) {
          if (timeString.StartsWith("-")) {
            TimeItem ti;
            if (TimeItem.TryParse(timeString.Substring(1), out ti)) {
              workItem = new WorkItemTemp();
              workItem.DesiredEndtime = ti;
            } else {
              error = string.Format("could not parse endtime string from {0}", timeString);
            }
          } else {
            double hours;
            if (double.TryParse(timeString, NumberStyles.Float, CultureInfo.InvariantCulture, out hours)) {
              workItem = new WorkItemTemp();
              workItem.HourCount = hours;
            } else {
              error = string.Format("could not parse hour string from {0}", timeString);
            }
          }
          if (workItem != null) {
            var projectPosDescString = wdItemString.Token(this.hourProjectInfoSeparator, 2).Trim();
            if (!string.IsNullOrEmpty(projectPosDescString)) {
              // expand abbreviations
              if (this.settings != null) {
                var abbrevString = projectPosDescString.TokenReturnInputIfFail("(", 1);
                ShortCut shortCut = this.settings.GetValidShortCuts(dateTime).FirstOrDefault(s => s.Key == abbrevString);
                if (shortCut != null) {
                  workItem.ShortCut = shortCut;
                  var expanded = shortCut.Expansion;
                  // if there is an desc given use its value instead of the one in the abbrev
                  if (!string.IsNullOrEmpty(projectPosDescString.Token("(+", 2).Token(")", 1))) {
                    // replace description in expanded
                    expanded = expanded.TokenReturnInputIfFail("(", 1) + "(" + expanded.Token("(", 2).Token(")", 1) + projectPosDescString.Token("(+", 2).Token(")", 1) + ")";
                  } else if (!string.IsNullOrEmpty(projectPosDescString.Token("(", 2).Token(")", 1))) {
                    // replace description in expanded
                    expanded = expanded.TokenReturnInputIfFail("(", 1) + "(" + projectPosDescString.Token("(", 2).Token(")", 1) + ")";
                  }
                  projectPosDescString = expanded;
                } else if(wholeDayShortcut!=null) {
                  workItem.ShortCut = wholeDayShortcut;
                }
              }

              var projectPosString = projectPosDescString.TokenReturnInputIfFail("(", 1);
              var parts = projectPosString.Split('-').Select(s => s.Trim()).ToList();
              if (parts.Any()) {
                workItem.ProjectString = parts.ElementAtOrDefault(0);
                workItem.PosString = parts.ElementAtOrDefault(1) ?? string.Empty;
                success = true;
              } else {
                error = string.Format("could not parse projectstring {0}", projectPosDescString);
              }
              var descString = projectPosDescString.Token("(", 2).Token(")", 1);
              if (!string.IsNullOrEmpty(descString)) {
                workItem.Description = descString;
              }
            } else {
              error = string.Format("projectstring was empty in {0}", wdItemString);
            }
          }
        } else {
          error = string.Format("could not parse hourstring from {0}", wdItemString);
        }
      }
      return success;
    }

    private bool GetDayStartTime(string input, out TimeItem dayStartTime, out string remainingString, out string error) {
      bool success = false;
      var dayStartToken = input.Token(this.dayStartSeparator, 1); // do not trim here, need original length later
      if (!string.IsNullOrEmpty(dayStartToken.Trim())) {
        if (TimeItem.TryParse(dayStartToken, out dayStartTime)) {
          remainingString = input.Substring(dayStartToken.Length + 1);
          error = string.Empty;
          success = true;
        } else {
          remainingString = input;
          error = string.Format("cannot parse daystarttime token: {0}", dayStartToken);
        }
      } else {
        error = "no daystart found";
        dayStartTime = null;
        remainingString = input;
      }
      return success;
    }
  }

  public class WorkDayParserResult
  {
    public WorkDayParserResult() {
      this.Error = string.Empty;
    }

    public bool Success { get; set; }
    public string Error { get; set; }
  }

  internal class WorkItemTemp
  {
    public bool IsPause { get; set; }
    public double HourCount { get; set; }
    public TimeItem DesiredEndtime { get; set; }
    public string ProjectString { get; set; }
    public string PosString { get; set; }
    public string Description { get; set; }
    public ShortCut ShortCut { get; set; }
  }
}