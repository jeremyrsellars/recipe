using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Data.Model
{
   public interface IModelId<TModel>
      where TModel : IModelId<TModel>
   {
      ModelId<TModel> Id{get;}
   }
   public interface ICandidateKey
   {
      object Key{get;}
   }
   public interface ICandidateKey<TModel,TId> : ICandidateKey
      where TModel : IModelId<TModel>
   {
      new TId Key{get;}
   }
}
