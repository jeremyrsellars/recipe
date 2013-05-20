using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Model
{
   public class NamedItem
   {
      private string m_name;
      public string Name
      {
         get
         {
            return m_name;
         }
         set
         {
            m_name = value;
         }
      }

      public override string ToString()
      {
         return Name;
      }
   }
}
