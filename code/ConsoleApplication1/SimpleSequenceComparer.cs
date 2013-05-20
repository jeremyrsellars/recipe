using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections;

namespace Sellars.Analysis
{
   public class SimpleSequenceComparer<T> : ISequenceComparer<T>
      where T : IEquatable<T>
   {

      public IEnumerable<ISequencePoint<T>> 
      CompareSequences(IEnumerable<T> left, IEnumerable<T> right)
      {
         return CompareSequences (left, right, EqualityComparer<T>.Default);
      }

      public IEnumerable<ISequencePoint<T>> 
      CompareSequences(IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> comparer)
      {
         List<T> leftList = new List<T> (left);
         List<T> rightList = new List<T> (right);
         
         BitArrayND eq;
         using (new Stopwatch ("BuildEqualityTable"))
            eq = BuildEqualityTable (leftList, rightList, comparer);
         
         List<Diagonal> list;
         
         using (new Stopwatch ("AnalyzeEqualityTable"))
            list = AnalyzeEqualityTable (eq);
         //list.Sort ((d1,d2) => d2.Length - d1.Length);  // longest to shortest
         
         using (new Stopwatch ("SortAndReduceSequences"))
            SortAndReduceSequences (list, 0, list.Count - 1);
         
         using (new Stopwatch ("RemoveBlankSequences"))
            RemoveBlankSequences (list);

         #error Need to reduce invalid sequences as part of SortAndReduceSequences.  Can use a MinimumX,MinimumY,MaxX,MaxY in signature to know which are valid.
         //list.Sort ((a,b) => b.Length - a.Length);

         using (new Stopwatch ("ReduceInvalidSequences"))
            ReduceInvalidSequences(list);
         
         using (new Stopwatch ("RemoveBlankSequences"))
            RemoveBlankSequences (list);
         
         int x = 0;
         int y = 0;
         int index = 0;
         while (x < leftList.Count || y < rightList.Count)
         {
            Diagonal diagonal;
            
            if (index == list.Count)
               diagonal = new Diagonal (){Length = 0,X = leftList.Count, Y=rightList.Count}; // Terminal
            else
               diagonal = list [index];

            for (; x < diagonal.X; x++)
            {
               yield return new SequencePoint (ComparisonResult.Left, leftList [x], default (T));
            }
            for (; y < diagonal.Y; y++)
            {
               yield return new SequencePoint (ComparisonResult.Right, default (T), rightList [y]);
            }
            if (index == list.Count)
               yield break;

#if DEBUG
   if (x != diagonal.X)
      throw new InvalidOperationException ("x != diagonal.X");
   if (y != diagonal.Y)
      throw new InvalidOperationException ("y != diagonal.Y");
#endif
            for (int i = 0; i < diagonal.Length; i++)
            {
#if DEBUG
   if (!comparer.Equals (leftList [x], rightList [y]))
      throw new InvalidOperationException ("left != right");
#endif
               yield return new SequencePoint (ComparisonResult.Both, leftList [x], rightList [y]);
               x++;
               y++;
            }
            // Advance to the next sequence point.
            index++;
         }
         
         //IEnumerable<ISequencePoint<T>> sorted = SortSequences (list, eq, 0, eq.Count);
         //SortSequences(list, 0, list.Count);
         
         //foreach (var diag in list)
         //{
         //   if (diag.Length == 0)
         //      continue;
         //   Console.Write (diag + "\t");
         //   for (int l = diag.X, i = 0; i < diag.Length; l++,i++)
         //   {
         //      Console.Write (leftList [l]);
         //   }
         //   Console.WriteLine ();
         //}
         
         //yield break;
      }

      private void 
      ReduceInvalidSequences(List<SimpleSequenceComparer<T>.Diagonal> list)
      {
         int minX = int.MinValue;
         int minY = int.MinValue;
         for (int i = 0; i < list.Count; i++)
         {
            Diagonal diagonal = list [i];
            if (diagonal.Length == 0)
               continue;
            //if (diagonal.X <= minX
            // || diagonal.Y <= minY)
            //{
            //   list [i] = new Diagonal ();
            //}
            if (minX <= diagonal.X)
               minX = diagonal.X + diagonal.Length;
            else
               list [i] = new Diagonal ();
            if (minY <= diagonal.Y)
               minY = diagonal.Y + diagonal.Length;
            else
               list [i] = new Diagonal ();
         }
      }

      private void RemoveBlankSequences(List<SimpleSequenceComparer<T>.Diagonal> list)
      {
         for (int i = list.Count - 1; i >= 0; i--)
         {
            int remove = 0;
            for (int j = i; j>=0 && list [j].Length == 0; remove++, j--)
               ;
            if (remove > 0)
            {
               i -= remove;
               list.RemoveRange (i + 1, remove);
            }
         }
      }

      private List<Diagonal> 
      AnalyzeEqualityTable(BitArrayND eq)
      {
         List<Diagonal> list = new List<SimpleSequenceComparer<T>.Diagonal> ();
         int xLen = eq.GetLength (0);
         int yLen = eq.GetLength (1);
         for (int x = 0; x < xLen; x++)
         {
            for (int y = 0; y < yLen; y++)
            {
               Diagonal diag = CountDiagonal (eq, x, y, xLen, yLen);
               if (diag.Length > 0)
                  list.Add (diag);
            }
         }
         
         return list;
      }

      private void
      SortAndReduceSequences(List<Diagonal> list, int start, int end)
      {
         //Console.WriteLine ("SRS({0}, {1})", start, end); 
         
         if (start >= end)
            return;
         
         // Pick pivot: the middle-most maxLength.
         int count = end - start + 1;
         int bestPivotIndex = (end + start) / 2;
         int bestPivotLength = list [bestPivotIndex].Length;
         
         // Make the (i - start) == (end - j)
         int i,j;
         if (count % 2 == 0)
         {
            i = bestPivotIndex;
            j = bestPivotIndex + 1;
         }
         else
         {
            i = bestPivotIndex - 1;
            j = bestPivotIndex + 1;
         }
         for (; j <= end; i--,j++)
         {
            int length;
            length = list [i].Length;
            if (length > bestPivotLength)
            {
               bestPivotIndex = i;
               bestPivotLength = length;
            }
            length = list [j].Length;
            if (length > bestPivotLength)
            {
               bestPivotIndex = j;
               bestPivotLength = length;
            }
         }
         
         //
         // Reduce
         //
         int bestPivotY = list[bestPivotIndex].Y;
         int bestPivotX = list[bestPivotIndex].X;
         for (i = start; i < bestPivotIndex; i++)
         {
            Diagonal current = list[i];
            if (current.Y >= bestPivotY && current.Y < bestPivotY + bestPivotLength)
            {
               if (list [i].Length > 100) Console.Error.WriteLine (list [i] + " deleted");
               list [i] = new Diagonal ();
               //Console.WriteLine ("RY({0})", i);
            }
            else if (current.X >= bestPivotX && current.X < bestPivotX + bestPivotLength)
            {
               if (list [i].Length > 100) Console.Error.WriteLine (list [i] + " deleted");
               list [i] = new Diagonal ();
               //Console.WriteLine ("RX({0})", i);
            }
         }
         for (i = bestPivotIndex + 1; i <= end; i++)
         {
            Diagonal current = list[i];
            if (current.Y >= bestPivotY && current.Y < bestPivotY + bestPivotLength)
            {
               if (list [i].Length > 100) Console.Error.WriteLine (list [i] + " deleted");
               list [i] = new Diagonal ();
               //Console.WriteLine ("RY({0})", i);
            }
            else if (current.X >= bestPivotX && current.X < bestPivotX + bestPivotLength)
            {
               if (list [i].Length > 100) Console.Error.WriteLine (list [i] + " deleted");
               list [i] = new Diagonal ();
               //Console.WriteLine ("RX({0})", i);
            }
         }
         
         SortAndReduceSequences (list, start, bestPivotIndex - 1);
         SortAndReduceSequences (list, bestPivotIndex + 1, end);
      }

      //private IEnumerable<ISequencePoint<T>>
      //SortSequences(BitArrayND eq, List<Diagonal> list, List<T> left, List<T> right, int start, int end)
      //{
         
      //   List<Diagonal> list = new List<SimpleSequenceComparer<T>.Diagonal> ();
      //   int xLen = eq.GetLength (0);
      //   int yLen = eq.GetLength (1);
      //   for (int x = 0; x < xLen; x++)
      //   {
      //      for (int y = 0; y < yLen; y++)
      //      {
      //         Diagonal diag = CountDiagonal (eq, x, y, xLen, yLen);
      //         if (diag.Length > 0)
      //            list.Add (diag);
      //      }
      //   }
         
      //   return list;
      //}

      private SimpleSequenceComparer<T>.Diagonal 
      CountDiagonal(BitArrayND eq, int x, int y, int xLen, int yLen)
      {
         int count = 0;
         
         for (
            int i = x, j = y; 
            i < xLen && j < yLen && eq [i, j];
            i++,j++)
         {
            count++;
         }
         return new Diagonal {X=x,Y=y,Length = count};
      }
      
      public static BitArrayND
      BuildEqualityTable (IList<T> left, IList<T> right, IEqualityComparer<T> comparer)
      {
         BitArrayND eq = new BitArrayND (left.Count, right.Count);
         
         for (int l = 0; l < left.Count; l++)
         {
            for (int r = 0; r < right.Count; r++)
            {
               eq [l,r] = comparer.Equals (left[l], right[r]);
            }
         }
         return eq;
      }
      
      struct Diagonal
      {
         public int X;
         public int Y;
         public int Length;

         public override string ToString()
         {
            return string.Format ("({0}, {1}, {2})", X, Y, Length);
         }
      }
      
      public class SequencePoint : ISequencePoint<T>
      {
         public
         SequencePoint (ComparisonResult result, T left, T right)
         {
            m_result = result;
            m_left = left;
            m_right = right;
         }
         
         #region ISequencePoint<T> Members

         public ComparisonResult Result
         {
            get { return m_result; }
         }

         public T Left
         {
            get { return m_left; }
         }

         public T Right
         {
            get { return m_right; }
         }

         #endregion

         private ComparisonResult m_result;
         private T m_left;
         private T m_right;
      }
   }
}
