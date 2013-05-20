using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Ingredient : NamedItem, IIngredient
   {
      public static Ingredient FromIngredient (IIngredient ingredient)
      {
         Ingredient i = new Ingredient
         {
            Name = ingredient.Name,
         };
         return i;
      }

      public Data.Model.ModelId<IRecipe> Id{get;set;}
   }
}
