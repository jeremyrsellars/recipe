using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Service;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Service
{
   public class IndexIngredientService : IIngredientService
   {
      public IndexIngredientService (ObservableCollection<IIngredient> list)
      {
         if (list == null)
            throw new ArgumentNullException ("list");
         m_list = list;
      }

      public IEnumerable<IIngredient> GetIngredients()
      {
         return m_list;
      }

      public void AddIngredient (IIngredient ingredient)
      {
         if (ingredient == null)
            throw new ArgumentNullException ("ingredient");

         m_list.Add (ingredient);
      }

      public void Initialize(Sellars.Service.ServiceController controller)
      {
      }

      private ObservableCollection<IIngredient> m_list;
   }
}
