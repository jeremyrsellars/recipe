using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Service
{
   public class IndexTagService : ITagService
   {
      public IndexTagService (ObservableCollection<string> list)
      {
         if (list == null)
            throw new ArgumentNullException ("list");
         m_list = list;
      }

      public IEnumerable<string> GetTags()
      {
         return m_list;
      }

      public void AddTag (string name)
      {
         if (!m_list.Contains (name, StringComparer.InvariantCultureIgnoreCase))
            m_list.Add (name);
      }

      public void Initialize(Sellars.Service.ServiceController controller)
      {
      }

      private ObservableCollection<string> m_list;
   }
}
