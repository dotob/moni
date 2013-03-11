using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace MONI.Util
{
  /// <summary>
  /// Generic implementation of a safe way to raise NotifyPropertyChanged-event for a property.
  /// Use this to avoid hard-coding property names.
  /// </summary>
  public static class NotifyPropertyChangedHelper
  {
    /// <summary>
    /// Called when [property changed].
    /// </summary>
    /// <typeparam name="TSenderType">The type of the sender type.</typeparam>
    /// <typeparam name="TPropType">the return type of the property, usually implicitely defined.</typeparam>
    /// <param name="sender">The sender.</param>
    /// <param name="handler">The PropChanged-handler.</param>
    /// <param name="projection">The Name of the changed Property.</param>
    public static void OnPropertyChanged<TSenderType, TPropType>(TSenderType sender, PropertyChangedEventHandler handler, Expression<Func<TPropType>> projection) where TSenderType : INotifyPropertyChanged {
      var tmp = handler;
      if (tmp != null) {
        if (projection != null) {
          CheckIfExpressionValid(projection);
          tmp(sender, new PropertyChangedEventArgs(PropertyName(projection)));
        } else {
          tmp(sender, new PropertyChangedEventArgs(String.Empty));
        }
      }
    }

    [Conditional("DEBUG")]
    private static void CheckIfExpressionValid<TPropType>(Expression<Func<TPropType>> projection) {
      var memberExpression = (MemberExpression)projection.Body;

      if ((memberExpression.Member.MemberType & MemberTypes.Property) != MemberTypes.Property) {
        throw new ArgumentException("Not a Property", "projection");
      }
    }

    /// <summary>
    /// Gets the Property name in a type-safe way, used for PropertyChanged-Events.
    /// </summary>
    /// <typeparam name="TPropType">the return type of the property.</typeparam>
    /// <param name="projection">The property-projection, s.th. like x=&gt;x.PropertyName.</param>
    /// <returns>the PropertyName.</returns>
    public static string PropertyName<TPropType>(Expression<Func<TPropType>> projection) {
      var memberExpression = (MemberExpression)projection.Body;
      return memberExpression.Member.Name;
    }
  }
}