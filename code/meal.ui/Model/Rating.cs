using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Rating : IRating
   {
      public static Rating FromRating (IRating rating)
      {
         Rating r = new Rating
         {
            UserName = rating.UserName,
            Value = rating.Value,
            CreatedOn = rating.CreatedOn,
         };
         return r;
      }
      
      public string UserName{get;set;}
      public double Value{get;set;}
      public DateTime CreatedOn{get;set;}
   }
}
