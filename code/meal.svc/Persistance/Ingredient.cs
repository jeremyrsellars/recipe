using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Ingredient : IIngredient, IModelId<IRecipe>
   {
      public static Ingredient FromIngredient (IIngredient ingredient)
      {
         if (ingredient == null)
            throw new ArgumentNullException ("ingredient");
         Guid recipeId;
         if (ingredient.Id == null)
            recipeId = Guid.Empty;
         else
            recipeId = ingredient.Id.Id;
         Ingredient i = new Ingredient
         {
            Name = ingredient.Name,
            RecipeId = recipeId,
         };
         return i;
      }
      
      public string Name;  // {get;set;}
      public Guid RecipeId;  // {get;set;}
      
      #region IIngredient Members

      string IIngredient.Name
      {
         get { return Name; }
      }

      #endregion

      #region IModelId<IRecipe> Members

      public ModelId<IRecipe> Id
      {
         get
         {
            return new ModelId<IRecipe> (RecipeId);
         }
      }

      #endregion
   }
}
