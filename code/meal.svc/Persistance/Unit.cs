using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Unit : IUnit
   {
      public static Unit FromUnit (IUnit unit)
      {
         Unit u = new Unit
         {
            Name = unit.Name,
         };
         return u;
      }
      
      public string Name;  // {get;set;}
      
      #region IUnit Members

      string IUnit.Name
      {
         get { return Name; }
      }

      #endregion
   }
}
