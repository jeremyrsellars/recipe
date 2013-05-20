using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;

namespace Sellars.Meal.Svc.Model
{
   public interface IRecipePart
   {
      string Name{get;}
      ITag PreparationMethod{get;}
      TimeSpan PreparationTime{get;}
      TimeSpan CookTime{get;}
      TimeSpan ChillTime{get;}
      int Temperature{get;}
      IReadonlyList<IIngredientDetail> Ingredients{get;}
      string Instructions{get;}
      IReadonlyList<IComment> Comments{get;}
   }
}
