using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.IO;
using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.Svc.Model;
using Sellars.Runtime.Serialization;

namespace Sellars.Meal.Svc.Service
{
   public class FileSystemRecipeService : Sellars.Meal.Svc.Service.IRecipeService
   {
      public static string id = "mock";
      
      public IRecipe LoadRecipe (string filename)
      {
         var formatter = new XmlFormatter ();
         using (var stream = System.IO.File.OpenRead (filename))
         {
            object o = formatter.Deserialize (stream);
            if (o is Sellars.Meal.Svc.Persistance.Recipe)
            {
               ((Sellars.Meal.Svc.Persistance.Recipe)o).FileName = filename;
            }
            return (IRecipe)o;
         }
      }
      
      public IRecipe SaveRecipe (string filename, IRecipe recipe)
      {
         if (recipe == null)
            throw new ArgumentNullException ("recipe");

         return SaveRecipeCore (filename, recipe);
      }
      public IRecipe GetRecipe(ModelId<IRecipe> recipeId)
      {
         throw new NotImplementedException ();
         //return LoadRecipe (recipeId.Id);
      }
      public IRecipe GetRecipe(ICandidateKey recipeKey)
      {
         string fileName = recipeKey.Key as string;
         if (string.IsNullOrEmpty(fileName))
            return null;
         return LoadRecipe (fileName);
      }

      public IReadonlyList<ModelId<IRecipe>> SearchRecipes()
      {
         throw new NotImplementedException ();
         //return 
         //   SearchFiles (RootPath)
         //   .Select (fileName => new ModelId<IRecipe> (fileName))
         //   .ToReadonlyList ();
      }

      public IReadonlyList<ICandidateKey> SearchRecipeKeys()
      {
         return 
            SearchFiles (RootPath)
            .Select (fileName => (ICandidateKey)new CandidateKey<IRecipe,string> (fileName))
            .ToReadonlyList ();
      }

      public IReadonlyList<IRecipeHeader> SearchHeaders(string query)
      {
         return 
            SearchFiles (RootPath)
            .Select (fileName => (IRecipeHeader) LoadRecipe (fileName))
            .ToReadonlyList ();
      }

      public IEnumerable<string> SearchFiles(string path)
      {
         if (!Directory.Exists (path))
            return new string[0];
         
         return Directory.GetFiles (path, "*.recipe", SearchOption.AllDirectories);
      }

      public FileSystemRecipeService ()
      {
         string path = 
            Environment.GetEnvironmentVariable("RECIPE_PATH") ??
            Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "My Recipes");
         RootPath = path;
      }

      private IRecipe SaveRecipeCore (string filename, IRecipe recipe)
      {
         if (filename == null)
            throw new ArgumentNullException ("fileName");
         if (string.IsNullOrEmpty (filename))
            throw new ArgumentException ("FileName cannot be null");
         if (recipe == null)
            throw new ArgumentNullException ("recipe");
         var formatter = new XmlFormatter ();
         
         Meal.Svc.Persistance.Recipe r = recipe as Meal.Svc.Persistance.Recipe ?? Meal.Svc.Persistance.Recipe.FromRecipe (recipe);
         
         using (var stream = System.IO.File.Create (filename))
         {
            formatter.Serialize (stream, r);
            stream.Close ();
         }
         return recipe;
      }
      
      public string RootPath{get;set;}

      #region IService Members

      public void Initialize(Sellars.Service.ServiceController controller)
      {
      }

      #endregion
   }
}
