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
   public class IndexRecipeService : Sellars.Meal.Svc.Service.IRecipeService
   {
      const string FILE = "file";
      const string ID = "id";
      const string NAME = "name";
      const string COMMENT = "comment";
      const string RATING = "rating";
      const string TAGS = "tags";
      const string SOURCE = "source";
      const string USER = "user";
      const string INGREDIENTS = "ingredients";
      const string INSTRUCTIONS = "instructions";
      private static readonly string [] FIELDS = 
      {
         FILE,
         ID,
         NAME,
         COMMENT,
         RATING,
         TAGS,
         SOURCE,
         USER,
         INGREDIENTS,
         INSTRUCTIONS,
      };

      public static IndexRecipeService CreateIndex ()
      {
         Indexing.IndexSession index = Indexing.IndexSession.CreateInMemorySession ();
         index.Fields = FIELDS;

         var recipeService = new FileSystemRecipeService ();
         foreach (var key in recipeService.SearchRecipeKeys ())
         {
            var recipe = recipeService.GetRecipe (key);
            var document = new Dictionary<string,string> ();

            document[FILE] = recipe.Key;
            document[ID] = recipe.Id.Id.ToString ();
            document[NAME] = recipe.Name;
            document[COMMENT] = recipe.Comments.Aggregate (
               new StringBuilder (),
               (sb, c) => sb.Append (c.UserName).Append (": ").Append (c.Text).Append (Environment.NewLine),
               sb => sb.ToString ());

            document[RATING] = recipe.Ratings.Aggregate (
               new StringBuilder (),
               (sb, r) => sb.Append (r.UserName).Append (": ").Append (r.Value).Append (Environment.NewLine),
               sb => sb.ToString ());

            document[TAGS] = recipe.Tags.Aggregate (
               new StringBuilder (),
               (sb, t) => sb.Append (t.Name).Append (", "),
               sb => sb.ToString ());

            document[SOURCE] = recipe.Source.Name;

            document[USER] = recipe.CreatedBy;

            document[INGREDIENTS] = 
               recipe.Parts
                  .SelectMany (part => part.Ingredients)
                  .Aggregate (
                     new StringBuilder (),
                     (sb, i) => sb.Append (IngredientToString (i) + Environment.NewLine),
                     sb => sb.ToString ());

            document[INSTRUCTIONS] = 
               recipe.Parts
                  .SelectMany (part => part.Instructions)
                  .Aggregate (
                     new StringBuilder (),
                     (sb, s) => sb.Append (s + Environment.NewLine),
                     sb => sb.ToString ());
            
            index.AddDocument (document);
         }

         return new IndexRecipeService (index, recipeService);
      }

      public IndexRecipeService (Sellars.Indexing.IndexSession index, FileSystemRecipeService recipeService)
      {
         m_index = index;
         m_recipeService = recipeService;
      }

      #region IRecipeService Members

      IRecipe IRecipeService.GetRecipe(ModelId<IRecipe> recipeId)
      {
         var fields = m_index.Search ("id:" + recipeId.Id).First ();
         string fileName = fields.Get ("file");
         return m_recipeService.LoadRecipe (fileName);
      }

      IReadonlyList<ModelId<IRecipe>> IRecipeService.SearchRecipes()
      {
         var documents = m_index.Search ("*:*");
         
         return documents
            .Select (document => document.Get (ID))
            .Where (id => !string.IsNullOrEmpty (id))
            .Select (id => new ModelId<IRecipe> (Guid.Parse (id)))
            .ToReadonlyList ();
      }

      IReadonlyList<ICandidateKey> IRecipeService.SearchRecipeKeys()
      {
         var documents = m_index.Search ("*:*");
         
         return documents
            .Select (document => document.Get (FILE))
            .Where (id => !string.IsNullOrEmpty (id))
            .Select (id => (ICandidateKey)new CandidateKey<IRecipe,string> (id))
            .ToReadonlyList ();
      }

      private static char [] DELIMITERS = "\r\n :".ToCharArray ();
      public IReadonlyList<IRecipeHeader> SearchHeaders(string query)
      {
         if (string.IsNullOrWhiteSpace (query))
            query = "*:*";
         var documents = m_index.Search (query);
         
         return documents
            .Select (CreateRecipeHeaderFromDocument)
            .ToReadonlyList ();
      }

      private static IRecipeHeader CreateRecipeHeaderFromDocument (Lucene.Net.Documents.Document document)
      {
         double rating = CalculateAverageRating (document.Get (RATING));

         var header = new RecipeHeader (
            document.Get (NAME), 
            document.Get (SOURCE),
            rating,
            Guid.Parse (document.Get (ID)),
            document.Get (FILE));
         return header;
      }

      private static double CalculateAverageRating (string indexValue)
      {
         double ratingSum = 0;

         if (string.IsNullOrWhiteSpace (indexValue))
            return ratingSum;

         double ct = 0;
         foreach (string part in indexValue.Split (DELIMITERS))
         {
            double value;
            if (double.TryParse (part, out value))
            {
               ct++;
               ratingSum += value;
            }
         }
         if (ct == 0)
            return 0;
         return ratingSum / ct;
      }

      IRecipe IRecipeService.GetRecipe(ICandidateKey recipeKey)
      {
         return m_recipeService.LoadRecipe ((string)recipeKey.Key);
      }

      IRecipe IRecipeService.SaveRecipe(string fileName, IRecipe recipe)
      {
         return m_recipeService.SaveRecipe (fileName, recipe);
      }

      #endregion

      void Sellars.Service.IService.Initialize(Sellars.Service.ServiceController controller)
      {
      }

      internal static string IngredientToString(IIngredientDetail ing)
      {
         StringBuilder s = new StringBuilder ();
         if (ing.Quantity != Fraction.Zero)
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Quantity);
         }

         if (ing.Quantity != Fraction.Zero && ing.Amount != Fraction.Zero)
            s.Append (" x ");

         if (ing.Amount != Fraction.Zero)
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Amount);
         }

         if (ing.AmountMax != Fraction.Zero)
         {
            s.Append (" - ");
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.AmountMax);
         }

         if (ing.Unit != null && ing.Unit.Name != "Unit" && ing.Unit.Name != "unit" )
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Unit);
         }

         if (ing.Ingredient != null)
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Ingredient.Name);
         }

         if (ing.Preparation != null && ing.Preparation.Count > 0)
         {
            ing.Preparation
               .Aggregate (
                  s,
                  (sb, prep) => s.Append (", ").Append (prep));
         }

         return s.ToString ();
      }
      
      private readonly Sellars.Indexing.IndexSession m_index;
      private readonly FileSystemRecipeService m_recipeService;

      private class RecipeHeader : IRecipeHeader
      {
         public RecipeHeader (string name, string source, double rating, Guid id, string key)
         {
            Name = name;
            Source = source;
            Rating = rating;
            m_id = id;
            m_key = key;
         }

         public string Name
         {
            get;private set;
         }

         public string Source
         {
            get;private set;
         }

         public double Rating
         {
            get;private set;
         }

         public ModelId<IRecipe> Id
         {
            get { return new ModelId<IRecipe> (m_id); }
         }

         public string Key
         {
            get { return m_key; }
         }

         object ICandidateKey.Key
         {
            get { return m_key; }
         }

         private readonly string m_key;
         private readonly Guid m_id;
      }
   }
}
