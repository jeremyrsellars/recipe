using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Unit : NamedItem, IUnit
   {
      public static Unit FromUnit (IUnit unit)
      {
         Unit u = new Unit
         {
            Name = unit.Name,
         };
         return u;
      }
   }
}
