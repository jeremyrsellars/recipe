using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Model
{
   public class NamedItem
   {
      public string Name{get;set;}

      public override string ToString()
      {
         return Name;
      }
   }
}
