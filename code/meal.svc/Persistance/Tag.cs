using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Tag : ITag
   {
      public static Tag FromTag (ITag tag)
      {
         if (tag == null)
            return null;
         Tag t = new Tag
         {
            UserName = tag.UserName,
            Name = tag.Name,
            CreatedOn = tag.CreatedOn,
         };
         return t;
      }
      
      public string UserName;  // {get;set;}
      public string Name;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}

      #region ITag Members

      string ITag.UserName
      {
         get { return UserName; }
      }

      string ITag.Name
      {
         get { return Name; }
      }

      DateTime ITag.CreatedOn
      {
         get { return CreatedOn; }
      }

      #endregion
   }
}
