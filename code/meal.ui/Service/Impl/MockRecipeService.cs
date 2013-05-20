using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.UI.Model;
using Sellars.Meal.Svc.Model;
using Sellars.Runtime.Serialization;
using System.Windows.Markup;

namespace Sellars.Meal.UI.Service.Impl
{
   public class MockRecipeDataProvider : Sellars.Meal.Svc.Service.IRecipeService
   {
      public static string id = "mock";
      
      public IRecipe LoadRecipe (string filename)
      {
         var formatter = new XmlFormatter ();
         using (var stream = System.IO.File.OpenRead (filename))
         {
            object o = formatter.Deserialize (stream);
            return o as IRecipe;
         }

         //using (var stream = System.IO.File.OpenRead (filename))
         //{
         //   object o = XamlReader.Load (stream);
            
         //   var r = o as Sellars.Meal.Svc.Persistance.Recipe;
            
         //   r.FileName = filename;
            
         //   return o as IRecipe;
         //}
      }
      
      public IRecipe SaveRecipe (string filename, IRecipe recipe)
      {
         var formatter = new XmlFormatter ();
         
         Meal.Svc.Persistance.Recipe r = recipe as Meal.Svc.Persistance.Recipe ?? Meal.Svc.Persistance.Recipe.FromRecipe (recipe);
         
         using (var stream = System.IO.File.Create (filename))
         {
            //XamlWriter.Save (r, stream);
            
            formatter.Serialize (stream, r);
            stream.Close ();
         }
         return recipe;
      }
      
      public IRecipe GetRecipe(ModelId<IRecipe> recipeId)
      {
         return new Recipe
         {
            Id=new ModelId<IRecipe>(recipeId.Id), 
            Name="Jeremy's Brownie Sensations", 
            Servings=new Fraction (12),
            Parts = new List<RecipePart> (new RecipePart []
               {
                  new RecipePart
                  {
                     Name = "Brownie",
                     CookTime = TimeSpan.FromMinutes (30),
                     PreparationTime = TimeSpan.FromMinutes (15),
                     PreparationMethod = new Tag () {Name="Oven"},
                     Temperature = 350,
                     Ingredients = new ObservableCollection<IngredientDetail> (new IngredientDetail []
                     {
                        new IngredientDetail
                        {
                           Index=0,
                           Quantity=4, 
                           Ingredient = "Egg",
                           Preparation = "Unbroken,Uncracked,Uncooked",
                        },
                        new IngredientDetail
                        {
                           Index=1,
                           Amount=new Fraction (2), 
                           Ingredient = "Sugar, Granulated",
                           Unit = new Unit {Name = "c"},
                        },
                        new IngredientDetail
                        {
                           Index=2,
                           Amount=new Fraction (2), 
                           Ingredient = "Vanilla Extract",
                           Unit = new Unit {Name = "tsp"},
                        },

                        new IngredientDetail
                        {
                           Index=3,
                           Amount=new Fraction (1,2), 
                           Ingredient = "Oil, Vegetable",
                           Unit = new Unit {Name = "c"},
                        },
                        new IngredientDetail
                        {
                           Index=4,
                           Amount=new Fraction (1,4), 
                           Ingredient = "Cocoa Powder",
                           Unit = new Unit {Name = "c"},
                        },

                        new IngredientDetail
                        {
                           Index=5,
                           Amount=new Fraction (1, 1,2), 
                           Ingredient = "Flour, All-purpose",
                           Unit = new Unit {Name = "c"},
                        },
                        new IngredientDetail
                        {
                           Index=6,
                           Amount=new Fraction (1,2), 
                           Ingredient = "Salt",
                           Unit = new Unit {Name = "tsp"},
                        },
                        new IngredientDetail
                        {
                           Index=7,
                           Amount=new Fraction (1,2), 
                           Ingredient = "Baking Powder",
                           Unit = new Unit {Name = "tsp"},
                        },
                     }),
                     Instructions = 
@"1. In small bowl, mix Eggs, Sugar, Vanilla.
2. In small bowl, mix oil and cocoa.
3. Fold oil mixture into egg mixture.
4. In medium bowl, mix remaining dry ingredients. (Be sure to break up soda pockets.)
5. Fold wet mixture into dry mixture.
(optionally mix in 1/2 c chocolate chips and/or 1/2 c coconut)
6. Bake at 350 F for 30 minutes, or until toothpick comes out clean.
Let cool slowly or top will fall."
                  },
                  new RecipePart
                  {
                     Name = "Frosting",
                     CookTime = TimeSpan.FromMinutes (30),
                     PreparationTime = TimeSpan.FromMinutes (15),
                     Temperature = 32,
                     PreparationMethod = new Tag () {Name="Refridgerator"},
                     Ingredients = new ObservableCollection<IngredientDetail> (new IngredientDetail []
                     {
                        new IngredientDetail
                        {
                           Index=0,
                           Amount=new Fraction(4, 1, 2),
                           Unit = new Unit {Name = "lbs"},
                           Ingredient = "Lion Meat",
                        },
                        new IngredientDetail
                        {
                           Index=1,
                           Amount=new Fraction(4, 1, 2),
                           Ingredient = "Sugar, Powdered",
                           Unit = new Unit {Name = "c"},
                        },
                        new IngredientDetail
                        {
                           Index=2,
                           Amount=new Fraction (2), 
                           Ingredient = "Vanilla Extract",
                           Unit = new Unit {Name = "tsp"},
                        },
                     }),
                     Instructions = 
@"1. In large bowl blend meat.
2. Add vanilla
3. Blend in sugar."
                  },
               })
         };
      }

      public IReadonlyList<ModelId<IRecipe>> SearchRecipes()
      {
         return new ReadonlyList<ModelId<IRecipe>> (new [] {new ModelId<IRecipe> (id)});
      }

      #region IService Members

      public void Initialize(Sellars.Service.ServiceController controller)
      {
      }

      #endregion
   }
}
