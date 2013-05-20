using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using Visibility=System.Windows.Visibility;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(Visibility), typeof(bool))]
   public class BooleanToCollapsedVisibilityConverter : IValueConverter
   {
      #region IValueConverter Members
      
      public bool Inverted{get;set;}

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is bool)
         {
            Boolean b = (Boolean)value;
            
            b = b ^ Inverted;
            
            Visibility v = b ? Visibility.Visible : Visibility.Collapsed;
            return v;
         }
         return null;
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is Visibility)
         {
            bool b;
            var v = (Visibility) value;
            if (v == Visibility.Visible)
               b = true;
            else
               b = false;
            
            return b ^ Inverted;
         }
         
         return null;
      }

      #endregion
   }
}
