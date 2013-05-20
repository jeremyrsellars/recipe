using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars;
using Sellars.Data.Model;
using Sellars.Collections.Generic;

namespace Sellars.Meal.Svc.Model
{
   public interface IRecipe : IModelId<IRecipe>
   {
      string Name{get;}
      Fraction Servings{get;}
      IReadonlyList<IRecipePart> Parts{get;}
      IReadonlyList<IComment> Comments{get;}
      IReadonlyList<IRating> Ratings{get;}
      IReadonlyList<ITag> Tags{get;}
      ISource Source{get;}
      DateTime CreatedOn{get;}
      string CreatedBy{get;}
   }
}
