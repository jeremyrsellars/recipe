using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using Visibility=System.Windows.Visibility;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(string), typeof(TimeSpan))]
   public class TemperatureConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is int)
         {
            int temp = (int)value;

            if (temp == 0)
               return "";
            return temp.ToString ();
         }
         return null;
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is int)
            return value;
         if (value is string)
         {
            string svalue = (string)value;
            int temp;
            if (string.IsNullOrWhiteSpace(svalue))
               temp = 0;
            else if (!int.TryParse (svalue, out temp))
               temp = 0;
            return temp;
         }
         return null;
      }
   }
}
