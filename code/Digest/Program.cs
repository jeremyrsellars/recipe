using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sellars.Meal.Svc.Persistance;
using Sellars.Meal.Svc.Model;
using Sellars.Meal.Svc.Service;
using Sellars;

namespace Digest
{
   class Program
   {
      static void Main(string[] args)
      {
         var input = new Input (@"c:\code\jeremy.sellars\food\fromOdp");//(Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments));
         //PrintDict (input.Ethnicity);
         //PrintDict (input.MealType);
         //PrintDict (input.Source);
         //PrintDict (input.Unit);

         var service = new FileSystemRecipeService ();
         string myRecipes = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "My Recipes");
         foreach (var recipe in CreateRecipes (input))
         {
            service.SaveRecipe (Path.Combine (myRecipes, recipe.FileName), recipe);
         }
      }

      static void PrintDict<TKey,TValue> (Dictionary<TKey,TValue> dict)
      {
         Console.WriteLine ("==========================================");
         foreach (KeyValuePair<TKey,TValue> kvp in dict)
            Console.WriteLine ("   " + kvp.Key + " ==> " + kvp.Value);
         Console.WriteLine ("==========================================");
      }

      class Input
      {
         public Input (string basePath)
         {
            Ethnicity = CsvDictionary.FromFile (Path.Combine ( basePath, "Ethnicity.csv"));
            MealType = CsvDictionary.FromFile (Path.Combine ( basePath, "MealType.csv"));
            Source = CsvDictionary.FromFile (Path.Combine ( basePath, "Source.csv"));
            Unit = CsvDictionary.FromFile (Path.Combine ( basePath, "Unit.csv"));

            Ingredient = CsvDictionary.ListFromFile (Path.Combine ( basePath, "Ingredient.csv"));
            MealComment = CsvDictionary.ListFromFile (Path.Combine ( basePath, "MealComment.csv"));
            Meal = CsvDictionary.ListFromFile (Path.Combine ( basePath, "Meal.csv"));
         }

         public Dictionary<string,string> Ethnicity;
         public Dictionary<string,string> MealType;
         public Dictionary<string,string> Source;
         public Dictionary<string,string> Unit;
         public List<Dictionary<string,string>> Ingredient;
         public List<Dictionary<string,string>> Meal;
         public List<Dictionary<string,string>> MealComment;
      }

      static IEnumerable<Recipe> CreateRecipes (Input input)
      {
         string user = "Amy Sellars";
         DateTime createdOn = DateTime.Today;
         foreach (Dictionary<string,string> meal in input.Meal)
         {
            string mealId = meal[MealColumn.MealId];
            string commentUser = null;

            List<Dictionary<string,string>> comments = 
               input.MealComment
                  .Where (mc => mc.ContainsKey (MealColumn.MealId) && mc[MealColumn.MealId] == mealId)
                  .Where (mc => mc.ContainsKey ("Comment") && !string.IsNullOrEmpty (mc["Comment"]))
                  .ToList ();
            var firstComment = comments.FirstOrDefault ();
            DateTime dt;
            if (firstComment != null && !string.IsNullOrEmpty (firstComment["CreatedOn"]))
            {
               DateTime.TryParse (firstComment["CreatedOn"], out dt);
               if (dt.Year < 2009)
                  dt = createdOn;
            }
            else
            {
               dt = createdOn;
            }

            Recipe recipe = new Recipe ();
            string mealType = meal["Type"];
            recipe.CreatedBy = user;
            recipe.CreatedOn = dt;
            recipe.FileName = meal[MealColumn.Name].Replace ("\"", "").Replace("\\", "").Replace("/", "") + ".recipe";
            recipe.Name = meal[MealColumn.Name];
            int rating;
            if (!string.IsNullOrEmpty (meal[MealColumn.Rating]) && int.TryParse (meal[MealColumn.Rating], out rating))
               recipe.Ratings = new [] {new Rating {CreatedOn = createdOn, UserName=user, Value=rating}};
            recipe.Servings = meal[MealColumn.Servings];
            recipe.Yield = meal[MealColumn.Yield];
            recipe.YieldUnit = meal[MealColumn.YieldUnitId];
            recipe.Source = new Source {Name = meal[MealColumn.Source], CreatedOn=createdOn};
            
            if (!string.IsNullOrWhiteSpace (mealType))
            {
               recipe.Tags = new []
                  {
                     new Tag {Name = mealType, CreatedOn=createdOn, UserName=user},
                  };
            }
            else
            {
               recipe.Tags = new Tag[0];
            };
            
            RecipePart part = new RecipePart ();
            recipe.Parts = new [] {part};

            // part.Comments =
            if (!string.IsNullOrEmpty (meal[MealColumn.CookTime]))
            {
               string value = meal[MealColumn.CookTime].Replace (" AM","").Replace (" PM","");
               part.CookSeconds = (int) TimeSpan.Parse (value).TotalSeconds;
            }
            part.CookSeconds = ParseTimeOfDayAsSeconds (meal[MealColumn.CookTime]);
            //part.ChillSeconds = ParseTimeOfDayAsSeconds (meal[MealColumn.ChillTime]);
            //part.Ingredients
            part.Instructions = new [] {FormatInstructions (meal[MealColumn.Instructions])};
            part.Name = meal[MealColumn.Name];
            //recipe. = meal[MealColumn.Description];
            part.PreparationSeconds = ParseTimeOfDayAsSeconds (meal[MealColumn.PreparationTime]);
            //part.Temperature = 



            part.Comments = 
               comments
                  .Select (
                     comment => 
                        new Comment
                        {
                           UserName = comment["Author"], 
                           Text = comment ["Comment"], 
                           CreatedOn = string.IsNullOrEmpty (comment ["CreatedOn"]) ? createdOn : DateTime.Parse (comment ["CreatedOn"])
                        })
                  .ToArray ();


            part.Ingredients =
               input.Ingredient
                  .Where (ingredient => ingredient.ContainsKey (IngredientColumn.MealId) && ingredient[IngredientColumn.MealId] == mealId)
                  .Select (
                     ingredient => 
                        new IngredientDetail
                        {
                           Amount = string.IsNullOrEmpty(ingredient[IngredientColumn.Amount]) ? "" : new Fraction((int) ((double.Parse (ingredient[IngredientColumn.Amount]) + .004) * 24), 24).Normalize ().ToString (),
                           Ingredient = ingredient[IngredientColumn.Name],
                           Preparation = ingredient[IngredientColumn.Preparation],
                           Unit = ingredient[IngredientColumn.UnitId],
                        })
                  .ToArray ();

            yield return recipe;
         }
      }

      private static System.Text.RegularExpressions.Regex m_regex = new System.Text.RegularExpressions.Regex (@"(\r\n)+");
      private static string FormatInstructions (string instructions)
      {
         return m_regex.Replace (instructions, "\r\n");
      }

      private static int ParseTimeOfDayAsSeconds (string value)
      {
         if (string.IsNullOrEmpty (value))
            return 0;
         
         //value = value.Replace (" AM","").Replace (" PM","");
         DateTime dt = DateTime.Parse (value);
         return (int) (dt - dt.Date).TotalSeconds;
      }

      private static class MealColumn
      {
         public const string MealId = "MealId";
         public const string Name = "Name";
         public const string Rating = "Rating";
         public const string Description = "Description";
         public const string PreparationTime = "PreparationTime";
         public const string CookTime = "CookTime";
         public const string Instructions = "Instructions";
         public const string Yield = "Yield";
         public const string YieldUnitId = "YieldUnitId";
         public const string Servings = "Servings";
         public const string Ethnicity = "Ethnicity";
         public const string Source = "Source";
         public const string Type = "Type";
         public const string ChillTime = "ChillTime";
      }

      private static class IngredientColumn
      {
         public const string IngredientId = "IngredientId";
         public const string MealId = "MealId";
         public const string Name = "Name";
         public const string Amount = "Amount";
         public const string UnitId = "UnitId";
         public const string Preparation = "Preparation";
      }
   }
}
