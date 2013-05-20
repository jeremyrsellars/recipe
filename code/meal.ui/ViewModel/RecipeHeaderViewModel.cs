using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipeHeaderViewModel
   {
      private string m_key;
      public List<RecipeViewModel> Recipes{get;set;}
      public string Key
      {
         get
         {
            return m_key;
         }
         set
         {
            m_key = value;
         }
      }
   }
}
