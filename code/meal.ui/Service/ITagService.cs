using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.Service
{
   public interface ITagService : Sellars.Service.IService
   {
      IEnumerable<string> GetTags ();
      void AddTag (string name);
   }
}
