﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.Svc.Model
{
   public interface ISource
   {
      string Name{get;}
      DateTime CreatedOn{get;}
   }
}
