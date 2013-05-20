using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Service
{
   public interface IIngredientService : Sellars.Service.IService
   {
      IEnumerable<IIngredient> GetIngredients ();
      void AddIngredient (IIngredient name);
   }
}
