using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Data.Model
{
   public class ModelId<T>
      where T : IModelId<T>
   {
      public ModelId (Guid id)
      {
         m_id = id;
      }
      
      public Guid Id
      {
         get
         {
            return m_id;
         }
      }
      
      private readonly Guid m_id;
   }

   public class CandidateKey<TModel,TId> : ICandidateKey
      where TModel : class, IModelId<TModel>, ICandidateKey<TModel,TId>
   {
      public CandidateKey (TId id)
      {
         m_id = id;
      }
      
      public TId Id
      {
         get
         {
            return m_id;
         }
      }
      
      object ICandidateKey.Key
      {
         get
         {
            return m_id;
         }
      }

      private readonly TId m_id;
   }
}
