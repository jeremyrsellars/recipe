using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Source : ISource
   {
      public static Source FromSource (ISource Source)
      {
         Source t = Source == null ? null : new Source
         {
            Name = Source.Name,
            CreatedOn = Source.CreatedOn,
         };
         return t;
      }
      
      public string Name;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}

      #region ISource Members

      string ISource.Name
      {
         get { return Name; }
      }

      DateTime ISource.CreatedOn
      {
         get { return CreatedOn; }
      }

      #endregion
   }
}
