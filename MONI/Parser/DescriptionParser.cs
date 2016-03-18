using System;
using MONI.Util;

namespace MONI.Parser {
    public class DescriptionParser {
        public static DescriptionParserResult ParseDescription(string s) {
            var ret = new DescriptionParserResult();
            if (!string.IsNullOrWhiteSpace(s)) {
                if (s.Contains("(+")) {
                    Tuple<string, string> splitOnFirst = s.SplitOnFirst("(+");
                    ret.BeforeDescription = splitOnFirst.Item1;
                    ret.Description = splitOnFirst.Item2.SplitOnLast(")").Item1;
                    ret.UsedAppendDelimiter = true;
                }
                else if (s.Contains("(")) {
                    Tuple<string, string> splitOnFirst = s.SplitOnFirst("(");
                    ret.BeforeDescription = splitOnFirst.Item1;
                    ret.Description = splitOnFirst.Item2.SplitOnLast(")").Item1;
                }
                else {
                    ret.BeforeDescription = s;
                }
            }
            return ret;
        }
    }

    public class DescriptionParserResult {
        public string BeforeDescription { get; set; }
        public string Description { get; set; }
        public bool UsedAppendDelimiter { get; set; }

        public DescriptionParserResult() {
            BeforeDescription = string.Empty;
            Description = string.Empty;
        }
    }
}