using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Meal.UI.Service;
using Sellars.Meal.UI;
using Sellars.Windows.Input;

namespace Sellars.Meal.UI.ViewModel
{
   public class SelectedIndexViewModel : ViewModelBase
   {
      public SelectedIndexViewModel ()
      {
         m_observableSources = new ObservableCollection<Meal.Svc.Model.ISource> ();
         m_observableTags = new ObservableCollection<string> ();
         NewRecipeCommand = new RelayCommand (NewRecipeExecute);
         PrintRecipeCommand = new RelayCommand (PrintRecipeExecute, PrintRecipeEnabled);
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
               var recipes = 
                  value.SelectMany (hdr => hdr.Recipes);

               Sources = 
                  recipes
                     .Select(rvm => rvm.Recipe.Source)
                     .Where (source => source != null && !string.IsNullOrWhiteSpace (source.Name))
                     .GroupBy (source => source.Name, (key, sources) => sources.FirstOrDefault ())
                     .ToList ();
               TagNames =
                  recipes
                     .SelectMany(rvm => rvm.Recipe.Tags)
                     .Select (t => t.Name)
                     .GroupBy (t => t)
                     .Select (g => g.Key)
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

      public List<string>
      TagNames
      {
         get
         {
            return m_tags ?? (m_tags = new List<string> ());
         }
         set
         {
            if (OnPropertyChanged("TagNames", ref m_tags, value))
            {
               m_observableTags.Clear ();
               if (value == null)
                  return;
               foreach (string tag in value.OrderBy(s => s, StringComparer.InvariantCultureIgnoreCase))
               {
                  m_observableTags.Add (tag);
               }
            }
         }
      }

      public ICommand NewRecipeCommand{get;set;}
      private ICommand m_printRecipeCommand;
      public ICommand PrintRecipeCommand
      {
         get
         {
            return m_printRecipeCommand;
         }
         private set
         {
            m_printRecipeCommand = value;
         }
      }

      internal ObservableCollection<Meal.Svc.Model.ISource> ObservableSources
      {
         get
         {
            return m_observableSources;
         }
      }

      internal ObservableCollection<string> ObservableTags
      {
         get
         {
            return m_observableTags;
         }
      }

      private void NewRecipeExecute (object parameter)
      {
         var recipe = new Sellars.Meal.UI.Model.Recipe ();
         recipe.Source = new Sellars.Meal.UI.Model.Source ();
         recipe.Parts.Add (new Sellars.Meal.UI.Model.RecipePart ());
         var rvm = new RecipeViewModel {Recipe=recipe, EditMode=true};
         m_index
            .Where(rhvm => rhvm.Key == "New")
            .First ()
            .Recipes.Add (rvm);
         Index = new List<RecipeHeaderViewModel> (m_index);
         SelectedItem = rvm;
      }

      private  bool PrintRecipeEnabled (object parameter)
      {
         return parameter is RecipeViewModel;
      }

      private void PrintRecipeExecute (object parameter)
      {
         RecipeViewModel recipeVM = (RecipeViewModel)parameter;
         var docPrintingService = Sellars.Service.ServiceController.Get<IDocumentPrintingService> ();
         docPrintingService.PrintDocument (recipeVM.Document, recipeVM.FileName ?? recipeVM.Recipe.Name);
      }

      private string m_filter;
      private List<RecipeHeaderViewModel> m_index;
      private List<RecipeHeaderViewModel> m_filteredIndex;
      private List<Model.Source> m_sources;
      private List<string> m_tags;
      private ObservableCollection<Meal.Svc.Model.ISource> m_observableSources;
      private ObservableCollection<string> m_observableTags;
      private RecipeViewModel m_recipe;
      private object m_selectedItem;
   }
}
