using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Comment : IComment
   {
      public static Comment FromComment (IComment comment)
      {
         Comment t = new Comment
         {
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedOn = comment.CreatedOn,
         };
         return t;
      }
      
      public string UserName{get;set;}
      public string Text{get;set;}
      public DateTime CreatedOn{get;set;}
   }
}
