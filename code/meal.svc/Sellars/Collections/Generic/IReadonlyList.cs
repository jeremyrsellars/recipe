using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Collections.Generic
{
   public interface IReadonlyList<T> : IEnumerable<T>
   {
      int Count {get;}
      T this[int index] {get;}
   }
}
