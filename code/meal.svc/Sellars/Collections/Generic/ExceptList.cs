using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Sellars.Collections.Generic
{
   public class ExceptList<T> : IEnumerable<T>, INotifyCollectionChanged
   {
      public ExceptList (IEnumerable<T> source, IEnumerable<T> exclusion)
      {
         if (source == null)
            throw new ArgumentNullException ("source");
         if (exclusion == null)
            throw new ArgumentNullException ("exclusion");

         m_source = source;
         m_exclusion = exclusion;
         if (m_source is INotifyCollectionChanged)
            ((INotifyCollectionChanged)m_source).CollectionChanged += Source_CollectionChanged;
         if (m_exclusion is INotifyCollectionChanged)
            ((INotifyCollectionChanged)m_exclusion).CollectionChanged += Exclusion_CollectionChanged;
      }

      public event NotifyCollectionChangedEventHandler CollectionChanged;

      private void Source_CollectionChanged (object sender, NotifyCollectionChangedEventArgs ea)
      {
         if (ea.Action == NotifyCollectionChangedAction.Move)
            return;
         m_list = null;
         OnCollectionChanged (NotifyCollectionChangedAction.Reset);
      }

      private void Exclusion_CollectionChanged (object sender, NotifyCollectionChangedEventArgs ea)
      {
         if (ea.Action == NotifyCollectionChangedAction.Move)
            return;
         m_list = null;
         OnCollectionChanged (NotifyCollectionChangedAction.Reset);
      }

      public void OnCollectionChanged (NotifyCollectionChangedAction action)
      {
         if (CollectionChanged != null)
            CollectionChanged (this, new NotifyCollectionChangedEventArgs (action));
      }

      public IEnumerator<T> GetEnumerator()
      {
         return (m_list ?? (m_list = GetEnumerableCore ().ToList ())).GetEnumerator ();
      }

      private IEnumerable<T> GetEnumerableCore()
      {
         return m_source.Except(m_exclusion);
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return GetEnumerator ();
      }

      private readonly IEnumerable<T> m_source;
      private readonly IEnumerable<T> m_exclusion;
      private List<T> m_list;
   }
}
