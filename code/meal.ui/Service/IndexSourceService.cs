using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Service
{
   public class IndexSourceService : ISourceService
   {
      public IndexSourceService (ObservableCollection<Svc.Model.ISource> list)
      {
         if (list == null)
            throw new ArgumentNullException ("list");
         m_list = list;
      }

      public IEnumerable<Svc.Model.ISource> GetSources()
      {
         return m_list;
      }

      public void Initialize(Sellars.Service.ServiceController controller)
      {
      }

      private ObservableCollection<Svc.Model.ISource> m_list;
   }
}
