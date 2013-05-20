using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.Svc.Model
{
   public interface IComment
   {
      string UserName{get;}
      string Text{get;}
      DateTime CreatedOn{get;}
   }
}
