﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using MonlistClone.Util;

namespace MonlistClone.Data {
  public class WorkDayParser {
    private char dayStartSeparator = ',';
    private char hourProjectInfoSeparator = ';';
    private char itemSeparator = ',';
    private WorkDayParserSettings settings;
    public WorkDayParser() {}

    public WorkDayParser(WorkDayParserSettings settings) {
      this.settings = settings;
    }

    public static WorkDayParser Instance { get; set; }

    public WorkDayParserResult Parse(string s, ref WorkDay wdToFill) {
      WorkDayParserResult ret = new WorkDayParserResult();
      if (!String.IsNullOrEmpty(s)) {
        TimeItem dayStartTime;
        string remainingString;
        string error;
        // should be like "<daystarttime>,..."
        // eg 7,... or 7:30,...
        if (this.GetDayStartTime(s, out dayStartTime, out remainingString, out error)) {
          // proceed with parsing items
          var wdItemsAsString = remainingString.Split(this.itemSeparator).Select(i => i.Trim()).ToList();
          if (wdItemsAsString.Any()) {
            List<WorkItemTemp> tmpList = new List<WorkItemTemp>();
            foreach (var wdItemString in wdItemsAsString) {
              WorkItemTemp workItem;
              if (this.GetWDItem(wdItemString, out workItem, out error)) {
                tmpList.Add(workItem);
              } else {
                ret.Error = error;
                ret.Success = false;
                // todo: fail fast??
              }
            }
            IEnumerable<WorkItem> resultList;
            if (this.ProcessTempWorkItems(dayStartTime, tmpList, out resultList, out error)) {
              wdToFill.Items.Clear();
              foreach (var workItem in resultList) {
                wdToFill.Items.Add(workItem);
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

    private bool ProcessTempWorkItems(TimeItem dayStartTime, List<WorkItemTemp> tmpList, out IEnumerable<WorkItem> resultList, out string error) {
      bool success = false;
      error = string.Empty;
      List<WorkItem> resultListTmp = new List<WorkItem>();
      TimeItem lastTime = dayStartTime;
      foreach (var workItemTemp in tmpList) {
        // check for pause
        if (workItemTemp.IsPause) {
          lastTime += workItemTemp.HourCount;
        } else {
          // TODO: fail when timeitem stuff fails...
          var currentEndTime = lastTime + workItemTemp.HourCount;
          // check for split
          if (this.settings != null && this.settings.InsertDayBreak) {
            if (this.settings.DayBreakTime.IsBetween(lastTime, currentEndTime)) {
              // insert new item
              resultListTmp.Add(new WorkItem(lastTime, this.settings.DayBreakTime, workItemTemp.ProjectString, workItemTemp.PosString));
              lastTime = this.settings.DayBreakTime + this.settings.DayBreakDurationInMinutes/60d;
              // fixup currentEndTime, need to add the dayshiftbreak
              currentEndTime = currentEndTime + this.settings.DayBreakDurationInMinutes/60d;
            }
          }
          resultListTmp.Add(new WorkItem(lastTime, currentEndTime, workItemTemp.ProjectString, workItemTemp.PosString));
          lastTime = currentEndTime;
          success = true;
        }
      }
      resultList = resultListTmp;
      return success;
    }

    private bool GetWDItem(string wdItemString, out WorkItemTemp workItem, out string error) {
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
        // workitem: <count of hours>;<projectnumber>-<positionnumber>
        var timeString = wdItemString.Token(this.hourProjectInfoSeparator, 1).Trim();
        if (!string.IsNullOrEmpty(timeString)) {
          double hours;
          if (double.TryParse(timeString, NumberStyles.Float, CultureInfo.InvariantCulture, out hours)) {
            workItem = new WorkItemTemp();
            workItem.HourCount = hours;

            var projectPosString = wdItemString.Token(this.hourProjectInfoSeparator, 2).Trim();
            if (!string.IsNullOrEmpty(projectPosString)) {
              // expand abbreviations
              if (this.settings != null && this.settings.ProjectAbbreviations != null && this.settings.ProjectAbbreviations.Any()) {
                string expanded;
                if (this.settings.ProjectAbbreviations.TryGetValue(projectPosString, out expanded)) {
                  projectPosString = expanded;
                }
              }

              var parts = projectPosString.Split('-').Select(s => s.Trim()).ToList();
              if (parts.Any()) {
                workItem.ProjectString = parts.ElementAtOrDefault(0);
                workItem.PosString = parts.ElementAtOrDefault(1) ?? string.Empty;
                success = true;
              } else {
                error = string.Format("could not parse projectstring {0}", projectPosString);
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

  public class WorkDayParserResult {
    public WorkDayParserResult() {
      this.Error = string.Empty;
    }

    public bool Success { get; set; }
    public string Error { get; set; }
  }

  public class WorkDayParserSettings {
    public WorkDayParserSettings() {
      this.ProjectAbbreviations = new Dictionary<string, string>();
    }

    public bool InsertDayBreak { get; set; }
    public TimeItem DayBreakTime { get; set; }
    public int DayBreakDurationInMinutes { get; set; }
    public Dictionary<string, string> ProjectAbbreviations { get; set; }
  }

  internal class WorkItemTemp {
    public bool IsPause { get; set; }
    public double HourCount { get; set; }
    public string ProjectString { get; set; }
    public string PosString { get; set; }
  }
}