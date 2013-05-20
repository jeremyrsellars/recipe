using System;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using Sellars.Meal.Svc.Model;
using Sellars.Service;
using Sellars.Meal.Svc.Service;
using Sellars.Data.Model;
using Sellars.Meal.UI.Service;
using Sellars.Meal.UI.Service.Impl;
using Sellars.Meal.UI.ViewModel;

namespace Sellars.Meal.UI
{
   /// <summary>
   /// Interaction logic for App.xaml
   /// </summary>
   public partial class App : Application
   {
      private void Application_Startup(object sender, StartupEventArgs e)
      {
         var window = new Sellars.Meal.UI.View.MainWindow ();

         ServiceController.Put<IRecipeService> (new FileSystemRecipeService ());

         object dataContext;
         
         string [] args = Environment.GetCommandLineArgs ();
         
         //IRecipe irecipe;
         IRecipeService recipeService = ServiceController.Get<IRecipeService> ();
         SelectedIndexViewModel vm;// = new SelectedIndexViewModel ();
         //if (args.Length >= 2)
         //{
         //   irecipe = recipeService.GetRecipe (new ModelId<IRecipe> (args [1]));
         //   Sellars.Meal.UI.Model.Recipe recipe = 
         //      irecipe as Sellars.Meal.UI.Model.Recipe 
         //      ?? Sellars.Meal.UI.Model.Recipe.FromRecipe (irecipe);
         //   dataContext = 
         //      new Sellars.Meal.UI.ViewModel.RecipeViewModel
         //         {Recipe = recipe};
         //}
         //else
         {
            var recipe = new Sellars.Meal.UI.Model.Recipe ();
            recipe.Source = new Sellars.Meal.UI.Model.Source ();
            recipe.Parts.Add (new Sellars.Meal.UI.Model.RecipePart (){Name="Recipe"});
            vm = new SelectedIndexViewModel {Index = GetIndex ()};
            var newRecipe = new RecipeViewModel{Recipe=recipe, EditMode=true};
            var newHeader =
               new RecipeHeaderViewModel
               {
                  Key = "New", 
                  Recipes = new List<RecipeViewModel>{newRecipe}
               };
            vm.Index.Insert (0, newHeader);
            vm.Recipe = newRecipe;
            ServiceController.Put<ISourceService> (new IndexSourceService (vm.ObservableSources));
            ServiceController.Put<ITagService> (new IndexTagService (vm.ObservableTags));
            ServiceController.Put<IDocumentPrintingService> (new DocumentPrintingService ());
            dataContext = vm;
         }

         window.DataContext = dataContext;
         App.Current.MainWindow = window;
         window.ShowDialog ();
      }

      //public class SelectedIndexViewModel : ViewModelBase
      //{
      //   public string Filter
      //   {
      //      get
      //      {
      //         return m_filter;
      //      }
      //      set
      //      {
      //         if (OnPropertyChanged (Filter, ref m_filter, value))
      //         {
      //            FilteredIndex = CreateFilteredIndex (Index, m_filter);
      //         }
      //      }
      //   }

      //   public List<RecipeIndexViewModel> FilteredIndex
      //   {
      //      get
      //      {
      //         return m_filteredIndex ?? Index;
      //      }
      //      set
      //      {
      //         OnPropertyChanged("FilteredIndex", ref m_filteredIndex, value);
      //      }
      //   }

      //   public List<RecipeIndexViewModel> Index
      //   {
      //      get
      //      {
      //         return m_index;
      //      }
      //      set
      //      {
      //         if (OnPropertyChanged("Index", ref m_index, value))
      //         {
      //            if (string.IsNullOrWhiteSpace (m_filter))
      //               FilteredIndex = null;
      //            else
      //               FilteredIndex = CreateFilteredIndex (Index, m_filter);
      //         }
      //      }
      //   }
      //   public RecipeIndexViewModel Recipe
      //   {
      //      get
      //      {
      //         return m_recipe;
      //      }
      //      set
      //      {
      //         OnPropertyChanged ("Recipe", ref m_recipe, value);
      //      }
      //   }

      //   private string m_filter;
      //   private List<RecipeIndexViewModel> m_index;
      //   private List<RecipeIndexViewModel> m_filteredIndex;
      //   private RecipeIndexViewModel m_recipe;
      //}

      internal static List<RecipeHeaderViewModel> CreateFilteredIndex(List<RecipeHeaderViewModel> index, string filter)
      {
         return CreateFilteredIndexByName (index, filter);
      }
      internal static List<RecipeHeaderViewModel> CreateFilteredIndexByName(List<RecipeHeaderViewModel> index, string filter)
      {
         return
            index
               .SelectMany (rivm => rivm.Recipes)
               .Where(rvm => rvm.Recipe != null && rvm.Recipe.Name != null && rvm.Recipe.Name.IndexOf (filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
               .OrderBy (rvm => rvm.Recipe.Name)
               .Aggregate (
                  new List<RecipeHeaderViewModel> (),
                  (list, rvm) => 
                  {
                     string key = rvm.Recipe.Name.FirstOrDefault(Char.IsLetterOrDigit).ToString ().ToUpper ();
                     var ivm = list.FirstOrDefault(v => v.Key == key);
                     if (ivm == null)
                     {
                        ivm = new RecipeHeaderViewModel (){Key = key, Recipes = new List<RecipeViewModel> ()};
                        list.Add (ivm);
                     }
                     ivm.Recipes.Add (rvm);
                     return list;
                  })
               .OrderBy (ivm => ivm.Key)
               .ToList ();
      }
      private struct KeyRecipePair
      {
         public string Key;
         public RecipeViewModel Recipe;
      }
      internal static List<RecipeHeaderViewModel> CreateFilteredIndexByTag(List<RecipeHeaderViewModel> index, string filter)
      {
         return
            index
               .SelectMany (rivm => rivm.Recipes)
               .OrderBy (rvm => rvm.Recipe.Name)
               .SelectMany (
                  r =>
                     (r.Recipe.Tags == null || r.Recipe.Tags.Count == 0)
                        ? new KeyRecipePair[] {new KeyRecipePair{Key="Untagged", Recipe=r}}
                        : r.Recipe.Tags.Select (t => new KeyRecipePair {Key = t.Name, Recipe = r}))
               .GroupBy (
                  pair => pair.Key, 
                  StringComparer.InvariantCultureIgnoreCase)
               .OrderBy (group => group.Key, StringComparer.InvariantCultureIgnoreCase)
               .Select (
                  group => 
                  {
                     string key = group.Key;
                     RecipeHeaderViewModel ivm;
                     ivm = new RecipeHeaderViewModel (){Key = key, Recipes = new List<RecipeViewModel> ()};
                     ivm.Recipes = 
                        group
                           .Select (pair => pair.Recipe)
                           .ToList ();
                     return ivm;
                  })
               .ToList ();
      }

      private static List<RecipeHeaderViewModel> GetIndex ()
      {
         IRecipeService recipeService = ServiceController.Get<IRecipeService> ();
         return 
            recipeService
               .SearchRecipes ()
               .Select (recipeId => recipeService.GetRecipeNoThrow (recipeId))
               .Where (recipe => recipe != null)
               .OrderBy (recipe => recipe.Name)
               .Aggregate (
                  new List<RecipeHeaderViewModel> (),
                  (list, recipe) => 
                  {
                     string key = recipe.Name.FirstOrDefault(Char.IsLetterOrDigit).ToString ().ToUpper ();
                     var vm = list.FirstOrDefault(v => v.Key == key);
                     if (vm == null)
                     {
                        vm = new RecipeHeaderViewModel (){Key = key, Recipes = new List<RecipeViewModel> ()};
                        list.Add (vm);
                     }
                     vm.Recipes.Add (new RecipeViewModel {FileName=recipe.Id.Id, Recipe=Sellars.Meal.UI.Model.Recipe.FromRecipe (recipe)});
                     return list;
                  })
               .OrderBy (vm => vm.Key)
               .ToList ();
      }
   }

   static class Extensions
   {
      public static IRecipe GetRecipeNoThrow (this IRecipeService service, ModelId<IRecipe> id)
      {
         try
         {
            return service.GetRecipe (id);
         }
         catch
         {
            return null;
         }
      }

      public static string JoinIfValueIsNotEmpty (this IEnumerable<KeyValuePair<string,string>> values, string kvpDelimiter, string delimiter)
      {
         return 
            Join(
               values.Where (kvp => !string.IsNullOrWhiteSpace (kvp.Value)),
               kvpDelimiter, delimiter);
      }

      public static string Join (this IEnumerable<KeyValuePair<string,string>> values, string kvpDelimiter, string delimiter)
      {
         return values.Aggregate (
            new StringBuilder (),
            (sb, kvp) => (sb.Length == 0 ? sb : sb.Append (delimiter)).Append (kvp.Key).Append (kvpDelimiter).Append (kvp.Value))
            .ToString ();
      }
   }
}
