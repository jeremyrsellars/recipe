using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(Fraction), typeof(String))]
   public class FractionConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is Fraction)
         {
            Fraction f = (Fraction)value;
            if (f == Fraction.Zero)
               return null;
            return f.Normalize ().ToString ();
         }
         if (value == null)
            return null;
         return value.ToString ();
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         Fraction result;
         if (value == null)
            return null;
         if (value is string)
         {
            string s = (string) value;
            if (s == "")
               return new Fraction ();
            if (Fraction.TryParse (s, out result))
               return result;
         }
         
         return null;
      }

      #endregion
   }
}
