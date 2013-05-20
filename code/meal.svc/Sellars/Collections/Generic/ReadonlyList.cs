using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sellars.Collections.Generic
{
   public class ReadonlyList<T> : IReadonlyList<T>
   {
      public 
      ReadonlyList ()
      {
         m_list = new List<T> ();
      }
      
      public 
      ReadonlyList (params T [] items)
      {
         m_list = new List<T> (items);
      }
      
      public ReadonlyList (IEnumerable<T> list)
      {
         if (list == null)
            throw new ArgumentNullException  ("list");
         m_list = list.ToList ();
      }
      
      public int Count
      {
         get
         {
            return m_list.Count;
         }
      }
      
      public T this [int index]
      {
         get
         {
            return m_list [index];
         }
      }
      
      
      public IEnumerator<T> GetEnumerator ()
      {
         return m_list.GetEnumerator ();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
      {
         return m_list.GetEnumerator ();
      }

      private List<T> m_list;
   }

   public static class ReadonlyListExtensions
   {
      public static IReadonlyList<T> ToReadonlyList<T> (this IEnumerable<T> list)
      {
         return new ReadonlyList<T> (list);
      }
   }
}
