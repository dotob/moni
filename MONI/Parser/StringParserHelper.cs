using System;
using System.Collections.Generic;
using System.Linq;

namespace MONI.Util {
  public static class StringParserHelper
  {

    /// <summary>
    ///   small convenience method to access token (parts) of a string
    ///   <example>
    ///     "la-li-lu".Token('-', 1) == "la"
    ///     "la-li-lu".Token('-', 4) == fallback
    ///     "la-li-lu".Token('-', 0) == "la-li-lu"
    ///     "la-li-lu".Token('-', -1) == "lu"
    ///     String.empty.Token('-', x) == fallback // for all x
    ///   </example>
    ///   <remarks>
    ///     the index is not nullbased!! so the first token is 1 and the last token is -1
    ///   </remarks>
    /// </summary>
    /// <param name = "s">the given string to split</param>
    /// <param name = "separator">the separator the split is done with</param>
    /// <param name = "token">index of token to use, this is 1-based</param>
    /// <param name="fallback">what will be returned if separator is not found</param>
    /// <returns></returns>
    public static string Token(this string s, string separator, int token, string fallback) {
      if (!String.IsNullOrEmpty(s) && s.Contains(separator)) {
        string[] tokens = s.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        if (token > 0) {
          // means: start at the beginning
          int idx = token - 1;
          if (idx < tokens.Length) {
            return tokens[idx];
          }
        } else if (token < 0) {
          //  mean: start from end
          int idx = tokens.Length + token;
          if (idx >= 0) {
            return tokens[idx];
          }
        } else {
          return s;
        }
      }
      return fallback;
    }

    /// <summary>
    ///   small convenience method to access token (parts) of a string
    ///   <example>
    ///     "la-li-lu".Token('-', 1) == "la"
    ///     "la-li-lu".Token('-', 4) == string.empty
    ///     "la-li-lu".Token('-', 0) == "la-li-lu"
    ///     "la-li-lu".Token('-', -1) == "lu"
    ///     String.empty.Token('-', x) == string.empty // for all x
    ///   </example>
    ///   <remarks>
    ///     the index is not nullbased!! so the first token is 1 and the last token is -1
    ///   </remarks>
    /// </summary>
    /// <param name = "s">the given string to split</param>
    /// <param name = "separator">the separator the split is done with</param>
    /// <param name = "token">index of token to use, this is 1-based</param>
    /// <returns></returns>
    public static string Token(this string s, string separator, int token) {
      return s.Token(separator, token, string.Empty);
    }

    public static Tuple<string, string> SplitOnFirst(this string s, string separator) {
      if (!string.IsNullOrWhiteSpace(s)) {
        if (!string.IsNullOrWhiteSpace(separator)) {
          var idx = s.IndexOf(separator, StringComparison.CurrentCulture);
          if (idx >= 0) {
            return new Tuple<string, string>(s.Substring(0, idx), s.Substring(idx + separator.Length));
          }
          return new Tuple<string, string>(s, s);
        }
        return new Tuple<string, string>(s, s);
      }
      return new Tuple<string, string>(string.Empty, string.Empty);
    }

    public static Tuple<string, string> SplitOnLast(this string s, string separator) {
      if (!string.IsNullOrWhiteSpace(s)) {
        if (!string.IsNullOrWhiteSpace(separator)) {
          var idx = s.LastIndexOf(separator, StringComparison.CurrentCulture);
          if (idx >= 0) {
            return new Tuple<string, string>(s.Substring(0, idx), s.Substring(idx + separator.Length));
          }
          return new Tuple<string, string>(s, s);
        }
        return new Tuple<string, string>(s, s);
      }
      return new Tuple<string, string>(string.Empty, string.Empty);
    }

    /// <summary>
    ///   small convenience method to access token (parts) of a string
    ///   <example>
    ///     "la-li-lu".Token('-', 1) == "la"
    ///     "la-li-lu".Token('-', 4) == input
    ///     "la-li-lu".Token('-', 0) == "la-li-lu"
    ///     "la-li-lu".Token('-', -1) == "lu"
    ///     String.empty.Token('-', x) == input // for all x
    ///   </example>
    ///   <remarks>
    ///     the index is not nullbased!! so the first token is 1 and the last token is -1
    ///   </remarks>
    /// </summary>
    /// <param name = "s">the given string to split</param>
    /// <param name = "separator">the separator the split is done with</param>
    /// <param name = "token">index of token to use, this is 1-based</param>
    /// <returns></returns>
    public static string TokenReturnInputIfFail(this string s, string separator, int token) {
      return s.Token(separator, token, s);
    }

    public static IEnumerable<string> SplitWithIgnoreRegions(this string s, char[] separators, params IgnoreRegion[] ignoreregions) {
      if (separators == null) {
        throw new ArgumentNullException("separators");
      }
      if (ignoreregions == null || !ignoreregions.Any()) {
        throw new ArgumentNullException("ignoreregions");
      }
      if (!string.IsNullOrWhiteSpace(s)) {
        Stack<char> irStack = new Stack<char>();
        var splitted = new List<string>();
        string tmp = string.Empty;
        foreach (char c in s) {
          var irMatch = ignoreregions.FirstOrDefault(ir => ir.Start == c);
          if (irStack.Any() && irStack.Peek() == c) {
            // found end of ignoreregion, remove last region info
            irStack.Pop();
            tmp += c;
          } else if (irMatch != null) {
            // found start of ignoreregion
            irStack.Push(irMatch.End);
            tmp += c;
          } else if (separators.Any(sep => sep == c) && !irStack.Any()) {
            // found valid separator, do split, but check if there are pending ignore regions in stack
            splitted.Add(tmp);
            tmp = string.Empty;
          } else {
            tmp += c;
          }
        }
        splitted.Add(tmp);
        return splitted;
      }
      return Enumerable.Empty<string>();
    }
  }

  public class IgnoreRegion
  {
    public char Start;
    public char End;

    public IgnoreRegion(char start, char end) {
      this.Start = start;
      this.End = end;
    }
  }
}