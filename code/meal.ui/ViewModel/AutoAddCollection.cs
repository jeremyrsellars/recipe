using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sellars.Meal.UI.ViewModel
{
   public class AutoAddCollection<T> : INotifyCollectionChanged, IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
      where T : class, INotifyPropertyChanged
   {
      public AutoAddCollection ()
         : this (null)
      {
      }
      
      public AutoAddCollection (IEnumerable<T> items)
      {
         if (items == null)
            m_items = new ObservableCollection<T> ();
         else
            m_items = new ObservableCollection<T> (items);
         m_items.CollectionChanged += new NotifyCollectionChangedEventHandler(m_items_CollectionChanged);
      }

      void m_items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         if (!DisregardCollectionChanges)
            OnCollectionChanged (e);
      }
      
      public Func<T> CreateNewItem
      {
         get
         {
            return m_createNewItem;
         }
         set
         {
            m_createNewItem = value;
            m_useNewItem = value != null;
            ResetNewItem ();
         }
      }
      

      #region INotifyCollectionChanged Members

      public event NotifyCollectionChangedEventHandler CollectionChanged;

      #endregion

      #region IList<T> Members

      public int IndexOf(T item)
      {
         int index = m_items.IndexOf (item);
         if (index >= 0)
            return index;
         if (m_useNewItem && Equals (item, m_newItem))
            return m_items.Count;
         return -1;
      }

      public void Insert(int index, T item)
      {
         if (m_useNewItem && index > m_items.Count)
            index--; // Trying to insert after the new item.
         m_items.Insert (index, item);
      }

      public void RemoveAt(int index)
      {
         if (m_useNewItem && m_items.Count == index)
            ResetNewItem ();
         m_items.RemoveAt (index);
      }

      public T this[int index]
      {
         get
         {
            if (m_useNewItem && m_items.Count == index)
               return NewItem;
            return m_items [index];
         }
         set
         {
            if (m_useNewItem && m_items.Count == index)
               NewItem = value;
            else
               m_items [index] = value;
         }
      }

      #endregion

      #region ICollection<T> Members

      public void Add(T item)
      {
         m_items.Add (item);
      }

      public void Clear()
      {
         m_items.Clear ();
         ResetNewItem ();
      }

      public bool Contains(T item)
      {
         return IndexOf (item) >= 0;
         // return Equals (m_newItem, item) || m_items.Contains (item);
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         m_items.CopyTo (array, arrayIndex);
         if (m_useNewItem && (m_newItem != null || m_createNewItem != null))
            array[arrayIndex + m_items.Count] = NewItem;
      }

      public int Count
      {
         get
         {
            if (m_useNewItem && (m_newItem != null || m_createNewItem != null))
               return m_items.Count + 1;
            return m_items.Count;
         }
      }

      bool ICollection<T>.IsReadOnly
      {
         get { return false; }
      }

      public bool Remove(T item)
      {
         int index = IndexOf (item);
         if (index < 1)
            return false;
         RemoveAt (index);
         return true;
      }

      #endregion

      #region IEnumerable<T> Members

      public IEnumerator<T> GetEnumerator()
      {
         return Enumerate ().GetEnumerator ();
      }

      #endregion

      #region IEnumerable Members

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator ();
      }

      #endregion

      #region IList Members

      int IList.Add(object value)
      {
         return ((IList)m_items).Add (value);
      }

      bool IList.Contains(object value)
      {
         if (m_useNewItem && value is T && Equals (m_newItem, value))
            return true;
         
         return ((IList)m_items).Contains (value);
      }

      int IList.IndexOf(object value)
      {
         if (value is T)
            return this.IndexOf ((T) value);
         return -1;
      }

      void IList.Insert(int index, object value)
      {
         Insert (index, (T)value);
      }

      bool IList.IsFixedSize
      {
         get { return false; }
      }

      bool IList.IsReadOnly
      {
         get { return false; }
      }

      void IList.Remove(object value)
      {
         if (value is T)
            Remove ((T) value);
      }

      object IList.this[int index]
      {
         get
         {
            return this[index];
         }
         set
         {
            this[index] = (T) value;
         }
      }

      #endregion

      #region ICollection Members

      void ICollection.CopyTo(Array array, int index)
      {
         foreach (T item in this)
         {
            array.SetValue (item, index++);
         }
      }

      bool ICollection.IsSynchronized
      {
         get { return false; }
      }

      object ICollection.SyncRoot
      {
         get { return this; }
      }

      #endregion
      
      protected void OnCollectionChanged (NotifyCollectionChangedEventArgs ea)
      {
         if (CollectionChanged != null)
            CollectionChanged (this, ea);
      }
      
      protected readonly ObservableCollection<T> m_items;
      protected Func<T> m_createNewItem;

      protected T m_newItem;
      protected bool m_useNewItem;

      protected virtual bool Equals (T objA, T objB)
      {
         return object.ReferenceEquals (objA, objB)
            || (objA == null && objB == null)
            || (objA != null && objB != null && objA.Equals (objB));
      }
      
      private IEnumerable<T> Enumerate ()
      {
         foreach (T item in m_items)
         {
            yield return item;
         }
         if (!m_useNewItem)
            yield break;
         T newItem = NewItem;
         if (newItem != null)
            yield return NewItem;
      }
      
      void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         T addedItem = m_newItem;
         Unsubscribe (addedItem);
         m_newItem = null;
         try
         {
            BeginDisregardingCollectionChanges ();
            m_items.Add (addedItem);
         }
         finally
         {
            EndDisregardingCollectionChanges ();
         }
         ResetNewItem ();
      }
      
      private T NewItem
      {
         get
         {
            if (m_useNewItem && m_newItem == null && m_createNewItem != null)
               NewItem = m_createNewItem ();
            return m_newItem;
         }
         set
         {
            T oldValue = m_newItem;
            if (oldValue == value)
               return;
            
            if (oldValue != null)
               Unsubscribe (oldValue);
            m_newItem = value;
            if (m_newItem != null)
               Subscribe (m_newItem);
            if (oldValue == null)
               OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, value, m_items.Count));
            else
               OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, value, oldValue));
         }
      }
      
      private void ResetNewItem ()
      {
         if (m_useNewItem && m_createNewItem != null)
            NewItem = m_createNewItem ();
         else
            NewItem = null;
      }
      
      private void Subscribe (T item)
      {
         item.PropertyChanged += item_PropertyChanged;
      }
      
      private void Unsubscribe (T item)
      {
         item.PropertyChanged -= item_PropertyChanged;
      }

      private bool DisregardCollectionChanges
      {
         get
         {
            return m_disregardCollectionChanges > 0;
         }
      }

      private void BeginDisregardingCollectionChanges ()
      {
         m_disregardCollectionChanges++;
      }

      private void EndDisregardingCollectionChanges ()
      {
         if (m_disregardCollectionChanges == 0)
            throw new InvalidOperationException ("Must call BeginDisregardingCollectionChanges before EndDisregardingCollectionChanges.");
         m_disregardCollectionChanges--;
      }

      /// <summary>
      /// The depth of the DisregardCollectionChanges.
      /// </summary>
      private int m_disregardCollectionChanges;
   }
}
