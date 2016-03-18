using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MONI.Data;

namespace MONI.Parser {
    public class WorkDayParserExtensions {
        public static string Increment(string text, int stepsToIncrementBy, ref int selectionStart) {
            return IncDec(text, stepsToIncrementBy, INCDEC_OPERATOR.INCREMENT, ref selectionStart);
        }

        public static string Decrement(string text, int stepsToIncrementBy, ref int selectionStart) {
            return IncDec(text, stepsToIncrementBy, INCDEC_OPERATOR.DECREMENT, ref selectionStart);
        }

        public static string IncDec(string text, int stepsToIncrementBy, INCDEC_OPERATOR incDec, ref int selectionStart) {
            if (!String.IsNullOrWhiteSpace(text)) {
                var hoursToIncrementBy = stepsToIncrementBy * 15 / 60f;
                var parts = Enumerable.ToList(SplitIntoParts(text));
                int idx;
                bool moveCursorLeft;
                int cursorInPartPosition;
                var part = FindPositionPart(parts, selectionStart, out idx, out cursorInPartPosition);
                var newPart = part;
                if (idx == 0 || (parts[0] == WorkDayParser.automaticPauseDeactivation && idx == 1)) {
                    // is daystart, has no -
                    TimeItem ti;
                    if (TimeItem.TryParse(part, out ti)) {
                        var tiIncremented = IncDecTimeItem(incDec, ti, hoursToIncrementBy);
                        newPart = String.Format((string) "{0}", (object) tiIncremented.ToShortString());
                    }
                }
                else if (part.StartsWith(WorkDayParser.endTimeStartChar.ToString())) {
                    TimeItem ti;
                    if (TimeItem.TryParse(part.TrimStart(WorkDayParser.endTimeStartChar), out ti)) {
                        var tiIncremented = IncDecTimeItem(incDec, ti, hoursToIncrementBy);
                        newPart = String.Format("{0}{1}", WorkDayParser.endTimeStartChar, tiIncremented.ToShortString());
                    }
                }
                else {
                    double t;
                    if (Double.TryParse(part, NumberStyles.Any, CultureInfo.InvariantCulture, out t)) {
                        double hIncremented = t;
                        if (incDec == INCDEC_OPERATOR.INCREMENT) {
                            hIncremented += hoursToIncrementBy;
                        }
                        else if ((hIncremented - hoursToIncrementBy) >= 0) {
                            // do not go below zero
                            hIncremented -= hoursToIncrementBy;
                        }
                        newPart = hIncremented.ToString(CultureInfo.InvariantCulture);
                    }
                }
                // check if we need to move cursor to left
                if (cursorInPartPosition > newPart.Length) {
                    selectionStart = selectionStart - cursorInPartPosition + newPart.Length;
                }
                if (idx >= 0) {
                    parts[idx] = newPart;
                }
                return parts.Aggregate(String.Empty, (aggr, s) => aggr + s);
            }
            return String.Empty;
        }

        public static TimeItem IncDecTimeItem(INCDEC_OPERATOR incDec, TimeItem ti, float hoursToIncrementBy) {
            TimeItem tiIncremented;
            if (incDec == INCDEC_OPERATOR.INCREMENT) {
                tiIncremented = ti + hoursToIncrementBy;
            }
            else {
                tiIncremented = ti - hoursToIncrementBy;
            }
            return tiIncremented;
        }

        public static IList<string> SplitIntoParts(string text) {
            var ret = new List<string>();
            var lines = text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines) {
                ret.AddRange(SplitIntoPartsIntern(line));
                ret.Add(Environment.NewLine);
            }
            // remove last newline
            return ret.Take(ret.Count - 1).ToList();
        }

        public static IList<string> SplitIntoPartsIntern(string text) {
            var splitted = new List<string>();
            // check for pause deactivation
            if (text.StartsWith(WorkDayParser.automaticPauseDeactivation)) {
                splitted.Add(WorkDayParser.automaticPauseDeactivation);
                text = text.Substring(WorkDayParser.automaticPauseDeactivation.Length);
            }
            string tmp = String.Empty;
            foreach (char c in text) {
                if (c == WorkDayParser.itemSeparator || c == WorkDayParser.hourProjectInfoSeparator) {
                    splitted.Add(tmp);
                    splitted.Add(c.ToString());
                    tmp = String.Empty;
                }
                else {
                    tmp += c;
                }
            }
            if (!String.IsNullOrEmpty(tmp)) {
                splitted.Add(tmp);
            }
            return splitted;
        }

        public static string FindPositionPart(IList<string> parts, int cursorPosition, out int foundPartsIndex, out int cursorInPartPosition) {
            cursorInPartPosition = 0;
            var partsComplete = parts.Aggregate(String.Empty, (aggr, s) => aggr + s);
            for (int i = 0; i < parts.Count(); i++) {
                var partsLower = parts.Take(i).Aggregate(String.Empty, (aggr, s) => aggr + s);
                var partsUpper = parts.Take(i + 1).Aggregate(String.Empty, (aggr, s) => aggr + s);

                var b = partsLower.Length;
                var t = partsUpper.Length;

                if ((cursorPosition >= b && cursorPosition < t) || partsUpper == partsComplete) {
                    if (parts[i] == WorkDayParser.itemSeparator.ToString() || parts[i] == WorkDayParser.hourProjectInfoSeparator.ToString()) {
                        // cursor left of separator
                        foundPartsIndex = i - 1;
                        var prevPart = parts.ElementAt(foundPartsIndex);
                        // find out where in the found part the cursor is, need to use prevpart an its length
                        cursorInPartPosition = prevPart.Length;
                        return prevPart;
                    }
                    else {
                        // find out where in the found part the cursor is
                        cursorInPartPosition = cursorPosition - b;
                        foundPartsIndex = i;
                        return parts.ElementAt(i);
                    }
                }
            }
            // not found
            foundPartsIndex = -1;
            return String.Empty;
        }
    }

    public enum INCDEC_OPERATOR {
        INCREMENT,
        DECREMENT
    }
}