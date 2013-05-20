using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Collections
{
   public class BitArrayND
   {
      public
      BitArrayND (params int [] dimensions)
      {
         m_lengths = dimensions;
         int size1d = dimensions.Aggregate (1, (result, n) => result * n);
         m_array = new System.Collections.BitArray (size1d);
      }
      
      public bool
      this [params int [] indexes]
      {
         get
         {
            int index = GetIndex (indexes);
            return m_array [index];
         }
         set
         {
            int index = GetIndex (indexes);
            m_array [index] = value;
         }
      }
      
      public int GetLength (int dimension)
      {
         return m_lengths [dimension];
      }
      
      public void
      Clear ()
      {
         m_array.SetAll (false);
      }
      
      internal int 
      GetIndex(params int [] indexes)
      {
         if (indexes.Length != m_lengths.Length)
            throw new IndexOutOfRangeException ("Indexer dimensions do not match actual dimensions.");
         
         int index = 0;
         int placeValue = 1;
         for (int i = indexes.Length - 1; i >= 0; i--)
         {
            index += indexes [i] * placeValue;
            placeValue *= m_lengths [i];
         }
         
         return index;
      }
      
      private int [] m_lengths;
      System.Collections.BitArray m_array;
   }
}
