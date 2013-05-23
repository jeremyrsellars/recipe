using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Meal.Svc.Model;
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
         m_observableIngredients = new ObservableCollection<IIngredient> ();
         NewRecipeCommand = new RelayCommand (NewRecipeExecute);
         PrintRecipeCommand = new RelayCommand (PrintRecipeExecute, PrintRecipeEnabled);
         SelectRecipeCommand = new RelayCommand (SelectRecipeExecute, SelectRecipeEnabled);
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
               var ingredients =
                  recipes
                     .Where(rvm => !string.IsNullOrEmpty(rvm.FileName))
                     .SelectMany(rvm => rvm.Recipe.Parts)
                     .SelectMany(rpvm => rpvm.Ingredients)
                     .Select (id => ((IIngredientDetail)id).Ingredient);
               var recipeNameIngredients =
                  recipes
                     .Where(rvm => !string.IsNullOrEmpty(rvm.FileName))
                     .Select(rvm => new Model.Ingredient { Id = rvm.Recipe.Id, Name = rvm.Recipe.Name });
               Ingredients = 
                  recipeNameIngredients
                     .Union (ingredients)
                     .GroupBy (i => i.Name)
                     .Select (g => g.First ())
                     .ToList ();
               foreach (var recipe in recipes)
               {
                  recipe.ShowRecipeCommand = SelectRecipeCommand;
               }
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

      public List<Sellars.Meal.Svc.Model.IIngredient>
      Ingredients
      {
         get
         {
            return m_ingredients ?? (m_ingredients = new List<Sellars.Meal.Svc.Model.IIngredient> ());
         }
         set
         {
            if (OnPropertyChanged("Ingredients", ref m_ingredients, value))
            {
               m_observableIngredients.Clear ();
               foreach (Meal.Svc.Model.IIngredient ingredient in value)
               {
                  m_observableIngredients.Add (ingredient);
               }
            }
         }
      }

      public ICommand NewRecipeCommand{get;set;}
      public ICommand PrintRecipeCommand{get;set;}
      public ICommand SelectRecipeCommand{get;set;}

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

      internal ObservableCollection<Meal.Svc.Model.IIngredient> ObservableIngredients
      {
         get
         {
            return m_observableIngredients;
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

      private bool SelectRecipeEnabled (object parameter)
      {
         return parameter is Sellars.Data.Model.ModelId<IRecipe>;
      }

      private void SelectRecipeExecute (object parameter)
      {
         var recipeId = parameter as Sellars.Data.Model.ModelId<IRecipe>;
         if (recipeId == null)
            return;
         var selectedItem = 
            m_index
               .SelectMany (rhvm => rhvm.Recipes)
               .FirstOrDefault (rvm => rvm.Recipe.Id != null && rvm.Recipe.Id.Id == recipeId.Id);
         if (selectedItem != null)
            SelectedItem = selectedItem;
      }

      private string m_filter;
      private List<RecipeHeaderViewModel> m_index;
      private List<RecipeHeaderViewModel> m_filteredIndex;
      private List<Model.Source> m_sources;
      private List<Sellars.Meal.Svc.Model.IIngredient> m_ingredients;
      private List<string> m_tags;
      private ObservableCollection<Meal.Svc.Model.ISource> m_observableSources;
      private ObservableCollection<string> m_observableTags;
      private ObservableCollection<IIngredient> m_observableIngredients;
      private RecipeViewModel m_recipe;
      private object m_selectedItem;
   }
}
