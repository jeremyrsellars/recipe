using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Data.Model;

namespace Sellars.Meal.Svc.Model
{
   public interface IRecipeHeader : IModelId<IRecipe>, ICandidateKey<IRecipe,string>
   {
      string Name{get;}
      double Rating{get;}
   }
}
