using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Comment : IComment
   {
      public static Comment FromComment (IComment comment)
      {
         if (comment == null)
            return null;
         Comment t = new Comment
         {
            UserName = comment.UserName,
            Text = comment.Text,
            CreatedOn = comment.CreatedOn,
         };
         return t;
      }
      
      public string UserName;  // {get;set;}
      public string Text;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}

      #region IComment Members

      string IComment.UserName
      {
         get { return UserName; }
      }

      string IComment.Text
      {
         get { return Text; }
      }

      DateTime IComment.CreatedOn
      {
         get { return CreatedOn; }
      }

      #endregion
   }
}
