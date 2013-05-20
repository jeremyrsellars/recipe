using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Meal.UI;
using Sellars.Windows.Input;

namespace Sellars.Meal.UI.ViewModel
{
   public class SelectedIndexViewModel : ViewModelBase
   {
      public SelectedIndexViewModel ()
      {
         m_observableSources = new ObservableCollection<Meal.Svc.Model.ISource> ();
         NewRecipeCommand = new RelayCommand (NewRecipeExecute);
      }

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
               {
                  if (m_filteredIndex == null)
                     OnPropertyChanged ("FilteredIndex");
                  else
                     FilteredIndex = null;
               }
               else
                  FilteredIndex = App.CreateFilteredIndex (Index, m_filter);
               Sources = 
                  value.SelectMany (hdr => hdr.Recipes)
                  .Select(rvm => rvm.Recipe.Source)
                  .Where (source => source != null && !string.IsNullOrWhiteSpace (source.Name))
                  .GroupBy (source => source.Name, (key, sources) => sources.FirstOrDefault ())
                  .ToList ();
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

      public List<Model.Source>
      Sources
      {
         get
         {
            return m_sources ?? (m_sources = new List<Model.Source> ());
         }
         set
         {
            if (OnPropertyChanged("Sources", ref m_sources, value))
            {
               m_observableSources.Clear ();
               foreach (Meal.Svc.Model.ISource source in value)
               {
                  m_observableSources.Add (source);
               }
            }
         }
      }

      public ICommand NewRecipeCommand{get;set;}

      internal ObservableCollection<Meal.Svc.Model.ISource> ObservableSources
      {
         get
         {
            return m_observableSources;
         }
      }

      private void NewRecipeExecute (object parameter)
      {
         var recipe = new Sellars.Meal.UI.Model.Recipe ();
         recipe.Source = new Sellars.Meal.UI.Model.Source ();
         recipe.Parts.Add (new Sellars.Meal.UI.Model.RecipePart ());
         var rvm = new RecipeViewModel {Recipe=recipe};
         m_index
            .Where(rhvm => rhvm.Key == "New")
            .First ()
            .Recipes.Add (rvm);
         Index = new List<RecipeHeaderViewModel> (m_index);
         SelectedItem = rvm;
      }

      private string m_filter;
      private List<RecipeHeaderViewModel> m_index;
      private List<RecipeHeaderViewModel> m_filteredIndex;
      private List<Model.Source> m_sources;
      private ObservableCollection<Meal.Svc.Model.ISource> m_observableSources;
      private RecipeViewModel m_recipe;
      private object m_selectedItem;
   }
}
