using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sellars.Meal.UI.Converters
{
   public interface IDefaultValueProvider
   {
      bool IsDefaultValue (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
      object GetDefaultValue (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
   }
   
   public class StringNullOrEmptyValueProvider : IDefaultValueProvider
   {
      public string DefaultValue{get;set;}
      
      public bool IsDefaultValue(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value == null || value.ToString ().Length == 0;
      }

      public object GetDefaultValue(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return DefaultValue;
      }
   }
   
   public class SimpleDefaultValueProvider : IDefaultValueProvider
   {
      public static readonly IDefaultValueProvider Instance = new SimpleDefaultValueProvider ();
      
      public bool IsDefaultValue(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value == null;
      }

      public object GetDefaultValue(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return parameter;
      }
   }
   
   [ValueConversion(typeof(object), typeof(String))]
   public class DefaultConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         IDefaultValueProvider defValProvider = parameter as IDefaultValueProvider ?? SimpleDefaultValueProvider.Instance;
         
         if (defValProvider.IsDefaultValue (value, targetType, parameter, culture))
            return defValProvider.GetDefaultValue (value, targetType, parameter, culture);
         if (value == null)
            return parameter;
         return value.ToString ();
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         IDefaultValueProvider defValProvider = parameter as IDefaultValueProvider ?? SimpleDefaultValueProvider.Instance;
         
         object def = defValProvider.GetDefaultValue (value, targetType, parameter, culture);
         
         if (def == value)
            return null;
         if (def != null && def.Equals (value))
            return null;
         
         return value;
      }

      #endregion
   }
}
