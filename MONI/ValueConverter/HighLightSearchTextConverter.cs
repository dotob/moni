using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;

namespace MONI.ValueConverter {
  public sealed class HighLightSearchTextConverter : IMultiValueConverter {
    private static HighLightSearchTextConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static HighLightSearchTextConverter() {}

    private HighLightSearchTextConverter() {}

    public static HighLightSearchTextConverter Instance {
      get { return instance ?? (instance = new HighLightSearchTextConverter()); }
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
      var result = values.ElementAtOrDefault(0) as string;
      var searchText = values.ElementAtOrDefault(1) as string;
      var wp = new Span();
      if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(searchText)) {
        bool found = false;
        string lastchars = string.Empty;
        Regex sr = new Regex(string.Format("(?<pre>.*?)(?<searchText>{0})", searchText), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        MatchCollection matchCollection = sr.Matches(result);
        foreach (Match match in matchCollection) {
          lastchars = result.Substring(match.Index + match.Length);
          wp.Inlines.Add(new Run(match.Groups["pre"].ToString()));
          var run = new Run(match.Groups["searchText"].ToString());
          run.Background = Brushes.Yellow;
          wp.Inlines.Add(run);
          found = true;
        }
        if (found) {
          if (!string.IsNullOrEmpty(lastchars)) {
            wp.Inlines.Add(new Run(lastchars));
          }
          return wp;
        } else {
          return new Run(result);
        }
      }
      if (!string.IsNullOrEmpty(result)) {
        return result;
      }
      return "-/-";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}