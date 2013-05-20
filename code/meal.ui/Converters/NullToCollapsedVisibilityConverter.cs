using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using Visibility=System.Windows.Visibility;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(Visibility), typeof(object))]
   public class NullToCollapsedVisibilityConverter : IValueConverter
   {
      #region IValueConverter Members
      
      public bool Inverted{get;set;}

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         Boolean b = value != null;
         b = b ^ Inverted;
            
         Visibility v = b ? Visibility.Visible : Visibility.Collapsed;
         return v;
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new InvalidOperationException ();
      }

      #endregion
   }
}
