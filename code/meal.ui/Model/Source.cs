using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Source : NamedItem, ISource
   {
      public static Source FromSource (ISource source)
      {
         if (source == null)
            return null;
         Source s = new Source
         {
            Name = source.Name,
            CreatedOn = source.CreatedOn,
         };
         return s;
      }
      
      public DateTime CreatedOn{get;set;}
      public override string ToString()
      {
         return Name;
      }
   }
}
