using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Rating : IRating
   {
      public static Rating FromRating (IRating rating)
      {
         if (rating == null)
            return null;
         Rating r = new Rating
         {
            UserName = rating.UserName,
            Value = rating.Value,
            CreatedOn = rating.CreatedOn,
         };
         return r;
      }
      
      public string UserName;  // {get;set;}
      public double Value;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}

      #region IRating Members

      string IRating.UserName
      {
         get { return UserName; }
      }

      double IRating.Value
      {
         get { return Value; }
      }

      DateTime IRating.CreatedOn
      {
         get { return CreatedOn; }
      }

      #endregion
   }
}
