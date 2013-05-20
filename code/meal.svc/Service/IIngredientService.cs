using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Service
{
   public interface IIngredientService
   {
      IReadonlyList<IIngredient> GetIngredients ();
   }
}
