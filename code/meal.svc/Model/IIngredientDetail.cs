using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars;
using Sellars.Collections.Generic;

namespace Sellars.Meal.Svc.Model
{
   public interface IIngredientDetail
   {
      IIngredient Ingredient{get;}
      IReadonlyList<string> Preparation{get;}
      Fraction Quantity{get;}
      Fraction Amount {get;}
      Fraction AmountMax {get;}
      IUnit Unit{get;}
   }
}
