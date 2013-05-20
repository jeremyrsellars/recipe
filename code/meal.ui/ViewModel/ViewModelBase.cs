using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.ViewModel
{
   public class ViewModelBase : INotifyPropertyChanged
   {
      protected bool OnPropertyChanged<T> (string propertyName, ref T field, T value)
      {
         if (field == null && value == null)
            return false;
         if (field != null && field.Equals (value))
            return false;
         field = value;
         if (PropertyChanged != null)
            PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
         return true;
      }
      protected void OnPropertyChanged (string propertyName)
      {
         if (PropertyChanged != null)
            PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
      }
      public event PropertyChangedEventHandler PropertyChanged;
   }
}
