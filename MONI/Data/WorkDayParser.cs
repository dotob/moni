using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MONI.Util;

namespace MONI.Data
{
  public class WorkDayParser
  {

    private readonly char dayStartSeparator = ',';
    private readonly char hourProjectInfoSeparator = ';';
    private readonly char itemSeparator = ',';
    private readonly char endTimeStartChar = '-';
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
      userInput = this.PreProcessWholeDayExpansion(userInput, wdToFill.DateTime, out wholeDayShortcut);
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
          var parts = remainingString.SplitWithIgnoreRegions(new[]{this.itemSeparator}, new IgnoreRegion('(',')'));
          var wdItemsAsString = parts.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
          if (wdItemsAsString.Any()) {
            List<WorkItemTemp> tmpList = new List<WorkItemTemp>();
            foreach (var wdItemString in wdItemsAsString) {
              WorkItemTemp workItem;
              if (this.GetWDTempItem(wdItemString, out workItem, out error, wdToFill.DateTime, wholeDayShortcut)) {
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
            } else {
              ret.Error = error;
            }
          } else {
            // this is no error for now
            ret.Success = true;
            ret.Error = "Noch keine Einträge gemacht";
          }
        } else {
          ret.Error = error;
        }
      } else {
        ret.Error = "Noch keine Eingabe";
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
        try {
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
        catch (Exception exception) {
          error = string.Format("Beim Verarbeiten von \"{0}\" ist dieser Fehler aufgetreten: \"{1}\"", workItemTemp.OriginalString, exception.Message);
          success = false;
        }
      }
      resultList = resultListTmp;
      return success;
    }

    private bool GetWDTempItem(string wdItemString, out WorkItemTemp workItem, out string error, DateTime dateTime, ShortCut wholeDayShortcut) {
      bool success = false;
      workItem = null;
      error = string.Empty;
      // check for pause item
      if (wdItemString.EndsWith("!")) {
        double pauseDuration;
        if (double.TryParse(wdItemString.Substring(0, wdItemString.Length - 1), NumberStyles.Float, CultureInfo.InvariantCulture, out pauseDuration)) {
          workItem = new WorkItemTemp(wdItemString);
          workItem.HourCount = pauseDuration;
          workItem.IsPause = true;
          success = true;
        }
      } else {
        // workitem: <count of hours|-endtime>;<projectnumber>-<positionnumber>[(<description>)]
        var timeString = wdItemString.Token(this.hourProjectInfoSeparator.ToString(), 1, wdItemString).Trim();
        if (!string.IsNullOrEmpty(timeString)) {
          if (timeString.StartsWith(this.endTimeStartChar.ToString())) {
            TimeItem ti;
            if (TimeItem.TryParse(timeString.Substring(1), out ti)) {
              workItem = new WorkItemTemp(wdItemString);
              workItem.DesiredEndtime = ti;
            } else {
              error = string.Format("Die Endzeit kann nicht erkannt werden: {0}", timeString);
            }
          } else {
            double hours;
            if (double.TryParse(timeString, NumberStyles.Float, CultureInfo.InvariantCulture, out hours)) {
              workItem = new WorkItemTemp(wdItemString);
              workItem.HourCount = hours;
            } else {
              error = string.Format("Die Stundeninfo kann nicht erkannt werden: {0}", timeString);
            }
          }
          if (workItem != null) {
            var projectPosDescString = wdItemString.Token(this.hourProjectInfoSeparator.ToString(), 2).Trim();
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
                } else if (wholeDayShortcut != null) {
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
                error = string.Format("Projektnummer kann nicht erkannt werden: {0}", projectPosDescString);
              }
              var descString = projectPosDescString.Token("(", 2).Token(")", 1);
              if (!string.IsNullOrEmpty(descString)) {
                workItem.Description = descString;
              }
            } else {
              error = string.Format("Projektnummer ist leer: {0}", wdItemString);
            }
          }
        } else {
          error = string.Format("Stundenanzahl kann nicht erkannt werden: {0}", wdItemString);
        }
      }
      return success;
    }

    private bool GetDayStartTime(string input, out TimeItem dayStartTime, out string remainingString, out string error) {
      bool success = false;
      var dayStartToken = input.Token(this.dayStartSeparator.ToString(), 1, input); // do not trim here, need original length later
      if (!string.IsNullOrEmpty(dayStartToken.Trim())) {
        if (TimeItem.TryParse(dayStartToken, out dayStartTime)) {
          remainingString = dayStartToken.Length < input.Length ? input.Substring(dayStartToken.Length + 1) : string.Empty; // seems like no daystartseparator
          error = string.Empty;
          success = true;
        } else {
          remainingString = input;
          error = string.Format("Tagesbeginn wird nicht erkannt: {0}", dayStartToken);
        }
      } else {
        error = "no daystart found";
        dayStartTime = null;
        remainingString = input;
      }
      return success;
    }

    public string Increment(string text, int stepsToIncrementBy, ref int selectionStart) {
      return this.IncDec(text, stepsToIncrementBy, INCDEC_OPERATOR.INCREMENT, ref selectionStart);
    }

    public string Decrement(string text, int stepsToIncrementBy, ref int selectionStart) {
      return this.IncDec(text, stepsToIncrementBy, INCDEC_OPERATOR.DECREMENT, ref selectionStart);
    }

    public string IncDec(string text, int stepsToIncrementBy, INCDEC_OPERATOR incDec, ref int selectionStart) {
      if (!string.IsNullOrWhiteSpace(text)) {
        var hoursToIncrementBy = stepsToIncrementBy * 15 / 60f;
        var parts = this.SplitIntoParts(text).ToList();
        int idx;
        var part = this.FindPositionPart(parts, selectionStart, out idx);
        var newPart = part;
        if (idx == 0) {
          // is daystart, has no -
          TimeItem ti;
          if (TimeItem.TryParse(part, out ti)) {
            var tiIncremented = IncDecTimeItem(incDec, ti, hoursToIncrementBy);
            newPart = string.Format("{0}", tiIncremented.ToShortString());
          }
        } else if (part.StartsWith(this.endTimeStartChar.ToString())) {
          TimeItem ti;
          if (TimeItem.TryParse(part.TrimStart(this.endTimeStartChar), out ti)) {
            var tiIncremented = IncDecTimeItem(incDec, ti, hoursToIncrementBy);
            newPart = string.Format("-{0}", tiIncremented.ToShortString());
          }
        } else {
          double t;
          if (double.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out t)) {
            double hIncremented;
            if (incDec == INCDEC_OPERATOR.INCREMENT) {
              hIncremented = t + hoursToIncrementBy;
            } else {
              hIncremented = t - hoursToIncrementBy;
            }
            newPart = hIncremented.ToString(CultureInfo.InvariantCulture);
          }
        }
        if (idx >= 0) {
          parts[idx] = newPart;
        }
        return parts.Aggregate(string.Empty, (aggr, s) => aggr + s);
      }
      return string.Empty;
    }

    private static TimeItem IncDecTimeItem(INCDEC_OPERATOR incDec, TimeItem ti, float hoursToIncrementBy) {
      TimeItem tiIncremented;
      if (incDec == INCDEC_OPERATOR.INCREMENT) {
        tiIncremented = ti + hoursToIncrementBy;
      } else {
        tiIncremented = ti - hoursToIncrementBy;
      }
      return tiIncremented;
    }

    public IList<string> SplitIntoParts(string text) {
      var ret = new List<string>();
      var lines = text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
      foreach (var line in lines) {
        ret.AddRange(SplitIntoPartsIntern(line));
        ret.Add(Environment.NewLine);
      }
      // remove last newline
      return ret.Take(ret.Count - 1).ToList();
    }

    public IList<string> SplitIntoPartsIntern(string text) {
      var splitted = new List<string>();
      string tmp = string.Empty;
      foreach (char c in text) {
        if (c == this.itemSeparator || c == this.hourProjectInfoSeparator) {
          splitted.Add(tmp);
          splitted.Add(c.ToString());
          tmp = string.Empty;
        } else {
          tmp += c;
        }
      }
      if (!string.IsNullOrEmpty(tmp)) {
        splitted.Add(tmp);
      }
      return splitted;
    }

    public string FindPositionPart(IList<string> parts, int position, out int foundPartsIndex) {
      var partsComplete = parts.Aggregate(string.Empty, (aggr, s) => aggr + s);
      for (int i = 0; i < parts.Count(); i++) {
        var partsLower = parts.Take(i).Aggregate(string.Empty, (aggr, s) => aggr + s);
        var partsUpper = parts.Take(i + 1).Aggregate(string.Empty, (aggr, s) => aggr + s);

        var b = partsLower.Length;
        var t = partsUpper.Length;

        if ((position >= b && position < t) || partsUpper == partsComplete) {
          if (parts[i] == this.itemSeparator.ToString() || parts[i] == this.hourProjectInfoSeparator.ToString()) {
            foundPartsIndex = i - 1;
            return parts.ElementAt(i - 1);
          } else {
            foundPartsIndex = i;
            return parts.ElementAt(i);
          }
        }
      }
      foundPartsIndex = -1;
      return string.Empty;
    }

    public string AddCurrentTime(string originalString) {
      // test for daystart
      string newString = originalString;
      if (string.IsNullOrWhiteSpace(originalString)) {
        newString += TimeItem.Now.ToString();
      } else {
        if (!originalString.EndsWith(itemSeparator.ToString())) {
          newString += itemSeparator;
        }
        newString += endTimeStartChar + TimeItem.Now.ToString();
      }
      return newString;
    }
  }

  public enum INCDEC_OPERATOR
  {
    INCREMENT,
    DECREMENT
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
    public WorkItemTemp(string originalString) {
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