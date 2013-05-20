using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Data.Model
{
   public class ModelId<T>
      where T : IModelId<T>
   {
      public ModelId (string id)
      {
         m_id = id;
      }
      
      public string Id
      {
         get
         {
            return m_id;
         }
      }
      
      private readonly string m_id;
   }
}
