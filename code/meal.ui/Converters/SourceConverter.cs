using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sellars.Meal.UI.Converters
{
   [ValueConversion(typeof(Model.Source), typeof(String))]
   public class SourceConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is Model.Source)
            return ((Model.Source)value).Name;
         if (value == null)
            return null;
         return value.ToString ();
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value is Model.Source)
            return value;

         if (value is string)
         {
            var provider = new Service.MockSourceDataProvider ();
            
            string name = (string) value;
            
            foreach (var source in provider.SearchSource ())
            {
               if (name == source.Name)
                  return source;
            }
         }
         
         return null;
      }

      #endregion
   }
}
