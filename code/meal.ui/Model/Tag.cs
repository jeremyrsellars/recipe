using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Tag : NamedItem, ITag
   {
      public static Tag FromTag (ITag tag)
      {
         Tag t = new Tag
         {
            UserName = tag.UserName,
            Name = tag.Name,
            CreatedOn = tag.CreatedOn,
         };
         return t;
      }
      
      public string UserName{get;set;}
      public DateTime CreatedOn{get;set;}
   }
}
