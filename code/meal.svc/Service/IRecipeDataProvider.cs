using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.Svc.Model;
using Sellars.Service;

namespace Sellars.Meal.Svc.Service
{
   public interface IRecipeService : IService
   {
      IRecipe GetRecipe (ModelId<IRecipe> recipeId);
      IReadonlyList<ModelId<IRecipe>> SearchRecipes ();
      IRecipe SaveRecipe (string fileName, IRecipe recipe);
   }
}
