using System;

namespace MONI.Util {
  public static class StringParserHelper {

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
    public static string Token(this string s, char separator, int token, string fallback) {
      if (!String.IsNullOrEmpty(s) && s.Contains(separator.ToString())) {
        string[] tokens = s.Split(separator);
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
    public static string Token(this string s, char separator, int token) {
      return s.Token(separator, token, string.Empty);
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
    public static string TokenReturnInputIfFail(this string s, char separator, int token) {
      return s.Token(separator, token, s);
    } 
  }
}