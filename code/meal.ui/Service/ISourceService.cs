using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Service
{
   public interface ISourceService : Sellars.Service.IService
   {
      IEnumerable<Meal.Svc.Model.ISource> GetSources ();
   }
}
