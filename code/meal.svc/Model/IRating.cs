using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.Svc.Model
{
   public enum RatingValue : byte
   {
      NotRated = 0,
      Gross,
      Poor,
      Edible,
      Good,
      Awesome,
   }
   
   public interface IRating
   {
      string UserName{get;}
      double Value{get;}
      DateTime CreatedOn{get;}
   }
}
