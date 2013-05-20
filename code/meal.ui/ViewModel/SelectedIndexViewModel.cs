using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.UI;

namespace Sellars.Meal.UI.ViewModel
{
   public class SelectedIndexViewModel : ViewModelBase
   {
      public string Filter
      {
         get
         {
            return m_filter;
         }
         set
         {
            if (OnPropertyChanged (Filter, ref m_filter, value))
            {
               FilteredIndex = App.CreateFilteredIndex (Index, m_filter);
            }
         }
      }

      public List<RecipeHeaderViewModel> FilteredIndex
      {
         get
         {
            return m_filteredIndex ?? Index;
         }
         set
         {
            OnPropertyChanged("FilteredIndex", ref m_filteredIndex, value);
         }
      }

      public List<RecipeHeaderViewModel> Index
      {
         get
         {
            return m_index;
         }
         set
         {
            if (OnPropertyChanged("Index", ref m_index, value))
            {
               if (string.IsNullOrWhiteSpace (m_filter))
                  FilteredIndex = null;
               else
                  FilteredIndex = App.CreateFilteredIndex (Index, m_filter);
            }
         }
      }

      public RecipeViewModel Recipe
      {
         get
         {
            return m_recipe;
         }
         set
         {
            OnPropertyChanged ("Recipe", ref m_recipe, value);
         }
      }

      public object SelectedItem
      {
         get
         {
            return m_recipe;
         }
         set
         {
            Recipe = value as RecipeViewModel;
            OnPropertyChanged ("SelectedItem", ref m_selectedItem, value);
         }
      }

      private string m_filter;
      private List<RecipeHeaderViewModel> m_index;
      private List<RecipeHeaderViewModel> m_filteredIndex;
      private RecipeViewModel m_recipe;
      private object m_selectedItem;
   }
}
