using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(Model.NamedItem), typeof(String))]
   public class NamedItemConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is Model.NamedItem)
            return ((Model.NamedItem)value).ToString ();
         if (value == null)
            return null;
         return value.ToString ();
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (Type == null)
            return null;
         try
         {
            Sellars.Meal.UI.Model.NamedItem item = (Sellars.Meal.UI.Model.NamedItem) Type.GetConstructor (Type.EmptyTypes).Invoke (null);
            item.Name = (value ?? "").ToString ();
            return item;
         }
         catch
         {
         }
         //Fraction result;
         //if (value == null)
         //   return null;
         //if (value is string)
         //{
         //   if (Fraction.TryParse (value.ToString (), out result))
         //      return result;
         //}
         
         return null;
      }

      public Type Type
      {
         get;set;
      }

      #endregion
   }
}
