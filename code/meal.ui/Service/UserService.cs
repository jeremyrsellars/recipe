using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Service
{
   public static class UserService
   {
      public static IUser CurrentUser = new WindowsUser ();
      public interface IUser
      {
         string Name{get;}
      }

      private class WindowsUser : IUser
      {
         public string Name
         {
            get
            {
               return Environment.UserName;
            }
         }
      }
   }
}
