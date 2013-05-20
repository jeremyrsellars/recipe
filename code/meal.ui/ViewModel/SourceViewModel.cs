using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.UI.Model;

namespace Sellars.Meal.UI.ViewModel
{
   public class SourceViewModel : NotifyPropertyChangedObject
   {
      private Source m_source;
      private bool m_isReadOnly;

      public SourceViewModel (Source source, bool isReadOnly)
      {
         if (source == null)
            throw new ArgumentNullException ("source");
         m_source = source;
         m_isReadOnly = isReadOnly;
      }

      public string Name
      {
         get
         {
            return m_source.Name;
         }
         set
         {
            m_source.Name = value;
            OnPropertyChanged ("Name");
         }
      }

      public Source Source
      {
         get
         {
            return m_source;
         }
         set
         {
            SetValue (ref m_source, value, "Source");
         }
      }

      public bool IsReadOnly
      {
         get
         {
            return m_isReadOnly;
         }
      }
   }
}
