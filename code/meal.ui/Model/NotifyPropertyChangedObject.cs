using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Model
{
   public class NotifyPropertyChangedObject : INotifyPropertyChanged
   {
      public static bool SetValue<T> (object sender, ref T field, T value, string propertyName, PropertyChangedEventHandler propChangedEvent)
      {
         if (field == null && value == null)
            return false;
         if (field != null && field.Equals (value))
            return false;
         field = value;
         if (propChangedEvent != null)
            propChangedEvent (sender, new PropertyChangedEventArgs (propertyName));
         return true;
      }
      
      #region INotifyPropertyChanged Members

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      protected void OnPropertyChanged (PropertyChangedEventArgs ea)
      {
         if (PropertyChanged != null)
            PropertyChanged (this, ea);
      }

      protected void OnPropertyChanged (string propertyName)
      {
         OnPropertyChanged (new PropertyChangedEventArgs (propertyName));
      }

      protected bool SetValue<T>(ref T field, T value, string propertyName)
      {
         return SetValue<T> (this, ref field, value, propertyName, PropertyChanged);
         //if (m_field == null && value == null)
         //   return;
         //if (m_field != null && m_field.Equals (value))
         //   return;
         //m_field = value;
         //OnPropertyChanged (propertyName);
      }
   }
}
