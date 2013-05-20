﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Data.Model;

namespace Sellars.Meal.Svc.Model
{
   public interface IIngredient : IModelId<IRecipe>
   {
      string Name{get;}
   }
}
