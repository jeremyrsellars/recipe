using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Ingredient : IIngredient
   {
      public static Ingredient FromIngredient (IIngredient ingredient)
      {
         Ingredient i = new Ingredient
         {
            Name = ingredient.Name,
         };
         return i;
      }
      
      public string Name;  // {get;set;}
      
      #region IIngredient Members

      string IIngredient.Name
      {
         get { return Name; }
      }

      #endregion
   }
}
