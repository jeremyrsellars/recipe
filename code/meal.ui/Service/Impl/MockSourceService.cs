using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.UI.Model;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Service.Impl
{
   public class MockSourceDataProvider //: Sellars.Meal.Svc.DataProvider.ISourceDataProvider
   {
//      public static Guid guid = Guid.NewGuid ();
      
//      public ISource GetSource(IModelId<ISource> sourceId)
//      {
//         return new Source
//         {
////            Id=new ModelId<ISource>(guid), 
//            Name="Jeremy's Kitchen", 
//         };
//      }

      public IReadonlyList<ISource> SearchSource()
      {
         return new ReadonlyList<ISource> (new [] {
            new Source {Name="Jeremy's Kitchen"},
            new Source {Name="Amy's Kitchen"},
            new Source {Name="Lil's Kitchen"},
            new Source {Name="Joni's Kitchen"},
         });
      }
   }
}
