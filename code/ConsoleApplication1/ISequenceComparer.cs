using System;
using System.Collections.Generic;

namespace Sellars.Analysis
{
   [Flags]
   public enum ComparisonResult
   {
      None = 0,
      Left = 1,
      Right = 2,
      Both = 3,
   }
   
   public interface ISequencePoint<T>
   {
      ComparisonResult Result{get;}
      T Left{get;}
      T Right{get;}
   }
   
   public interface ISequenceComparer<T>
   {
      IEnumerable<ISequencePoint<T>> 
      CompareSequences (IEnumerable<T> left, IEnumerable<T> right);

      IEnumerable<ISequencePoint<T>> 
      CompareSequences (IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> comparer);
   }
}