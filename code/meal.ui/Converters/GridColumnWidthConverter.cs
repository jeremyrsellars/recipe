using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Sellars.Meal.UI.Converters
{
   //[ValueConversion(typeof(System.Windows.Controls.GridView), typeof(double))]
   public class GridColumnWidthConverter : IValueConverter
   {
      #region IValueConverter Members

      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         string columnName = (string)parameter;

         var gridView = value as System.Windows.Controls.GridView;
         if (gridView != null)
         {
            var column = gridView.Columns.First (c => (string)c.Header==(string)parameter);
            return column.ActualWidth;
         }
         var gridViewP = value as System.Windows.Controls.GridViewRowPresenter;
         if (gridViewP != null)
         {
            var column = gridViewP.Columns.First (c => (string)c.Header==(string)parameter);
            return column.ActualWidth;
         }
         return value;
      }

      public object 
      ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException ();
      }

      #endregion
   }
}
