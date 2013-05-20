using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sellars.Meal.UI.Model;
using System.Windows.Input;
using Sellars.Windows.Input;
using Sellars.Service;
using Sellars.Meal.Svc.Service;
using Doc=System.Windows.Documents;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipeViewModel : NotifyPropertyChangedObject
   {
      public RecipeViewModel ()
      {
         SaveCommand = new RelayCommand (Save_Execute);
      }

      public string FileName{get;set;}
      public ICommand SaveCommand{get;set;}
      
      public bool
      EditMode
      {
         get
         {
            return m_editMode;
         }
         set
         {
            if (m_editMode == value)
               return;
            m_editMode = value;
            OnPropertyChanged ("EditMode");
         }
      }

      public Sellars.Meal.UI.Model.Recipe
      Recipe
      {
         get
         {
            return m_recipe;
         }
         set
         {
            if (m_recipe == value)
               return;
            m_recipe = value;
            if (value == null)
            {
               m_recipeParts.CollectionChanged -= m_recipeParts_CollectionChanged;
               m_recipeParts = null;
               m_userRating = null;
            }
            else
            {
               var existingParts = 
                  value
                     .Parts
                     .Select (p => new RecipePartViewModel{Model=(RecipePart)p})
                     .ToList ();
               m_recipeParts = new AutoAddCollection<ViewModel.RecipePartViewModel> (existingParts);
               m_recipeParts.CreateNewItem = 
                  delegate 
                  {
                     return 
                        new Sellars.Meal.UI.ViewModel.RecipePartViewModel
                           {
                              Visible = false,
                              Model = new RecipePart
                                 {
                                    PreparationMethod = new Tag {Name="Add Part"},
                                 },
                           };
                  };
               m_recipeParts.CollectionChanged += m_recipeParts_CollectionChanged;
               double rating;
               rating = 
                  (value.Ratings == null || value.Ratings.Count == 0) ?
                     2.5 :
                     (value.Ratings.Aggregate (
                        0.0,
                        (seed, r) => seed + r.Value) / value.Ratings.Count);

               m_userRating = new RatingViewModel (rating);
            }
            OnPropertyChanged ("Recipe");
            OnPropertyChanged ("Document");
         }
      }

      void m_recipeParts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if (m_recipe == null)
            return; // weird
         
         var modelParts = m_recipe.Parts;
         modelParts.Clear ();
         modelParts.AddRange (
            m_recipeParts.Take (m_recipeParts.Count - 1)
               .Select (vm => vm.Model));
      }
      
      public AutoAddCollection<RecipePartViewModel>
      RecipeParts
      {
         get
         {
            return m_recipeParts;
         }
      }
      
      public IEnumerable<Model.Source>
      Sources
      {
         get
         {
            foreach (Model.Source source in new Service.MockSourceDataProvider().SearchSource ())
            {
               yield return source;
            }
         }
      }

      public RatingViewModel UserRating
      {
         get
         {
            return m_userRating;
         }
      }
      
      private void Save_Execute (object parameter)
      {
         if (m_userRating.IsDirty)
         {
            Rating rating = new Rating ();
            rating.CreatedOn = DateTime.Now;
            rating.UserName = Sellars.Meal.UI.Service.UserService.CurrentUser.Name;
            rating.Value = m_userRating.Rating;
            Recipe.Ratings.Add (rating);
         }
         if (string.IsNullOrWhiteSpace(FileName))
         {
            var x = new Microsoft.Win32.SaveFileDialog();
            x.AddExtension = true;
            x.DefaultExt = ".recipe";
            x.FileName = Recipe.Name;
            x.Filter = "Recipe file|*.recipe";
            x.InitialDirectory = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments), "My Recipes");
            x.OverwritePrompt = true;
            x.ValidateNames = true;
            if (!x.ShowDialog ().Value)
               return;
            FileName = x.FileName;
         }
         var savedRecipe = Sellars.Meal.Svc.Persistance.Recipe.FromRecipe (Recipe);
         ServiceController.Get<IRecipeService> ().SaveRecipe (FileName, savedRecipe);
         Recipe = Sellars.Meal.UI.Model.Recipe.FromRecipe (savedRecipe);
         EditMode = false;
      }

      public Doc.FlowDocument
      Document
      {
         get
         {
            return CreateDocument (m_recipe);
         }
      }

      public static Doc.FlowDocument
      CreateDocument (IRecipe recipe)
      {
         Doc.FlowDocument doc = new Doc.FlowDocument ();
         doc.Typography.Fraction = System.Windows.FontFraction.Slashed;
         doc.FontFamily = new System.Windows.Media.FontFamily ("Palatino Linotype");
         
         var cookTimeConverter = new Converters.CookTimespanConverter ();
         {
         Doc.Section recipeHeader = new Doc.Section ();
         doc.Blocks.Add (recipeHeader);

         Doc.Paragraph p;
         recipeHeader.Blocks.Add (new Doc.Paragraph (new Doc.Underline (new Doc.Run(recipe.Name))));
         if (recipe.Source != null)
         {
            recipeHeader.Blocks.Add (p = new Doc.Paragraph (new Doc.Italic (new Doc.Run("From "))));
            p.Inlines.Add (new Doc.Run(recipe.Source.Name));
         }
         recipeHeader.Blocks.Add (new Doc.Paragraph (new Doc.Run("Serves " + recipe.Servings)));
         string tags =
            recipe.Tags
            .Aggregate(
               new StringBuilder ("Tags: "),
               (sb, t) =>
               {
                  sb.Append (t.Name);
                  sb.Append (", ");
                  return sb;
               },
               sb => sb[sb.Length - 2] == ',' ? sb.ToString (0, sb.Length - 2) : sb.ToString ());
         recipeHeader.Blocks.Add (new Doc.Paragraph (new Doc.Run(tags)));
         }

         foreach (IRecipePart part in recipe.Parts)
         {
            Doc.Section partHeader = new Doc.Section ();
            doc.Blocks.Add (partHeader);

            partHeader.Blocks.Add (new Doc.Paragraph (new Doc.Underline (new Doc.Run(part.Name))));
            string prepTime = cookTimeConverter.Convert (part.PreparationTime, typeof (string), null, null) as string;
            string cookTime = cookTimeConverter.Convert (part.CookTime, typeof (string), null, null) as string;
            string prepMethod = part.PreparationMethod != null ? part.PreparationMethod.Name : null;

            prepTime = "Preparation: " + prepTime;
            cookTime = "Cook: " + cookTime;
            Doc.Paragraph stats = new Doc.Paragraph ();
            partHeader.Blocks.Add (stats);
            stats.Inlines.Add (new Doc.Run(prepTime + " \u2022 "));
            stats.Inlines.Add (new Doc.Run(prepMethod + " \u2022 "));
            stats.Inlines.Add (new Doc.Run(cookTime + " \u2022 "));
            stats.Inlines.Add (new Doc.Run(part.Temperature + " \u00BA F"));
            
            string ingredients = 
               part.Ingredients == null ? "" :
               "Ingredients:\r\n" + 
                     part.Ingredients.Aggregate(
                        new StringBuilder (),
                        (sb,ing) => sb.AppendLine (IngredientToString (ing)),
                        sb => sb.ToString ());
            Doc.Paragraph partIngredients = new Doc.Paragraph (new Doc.Run(ingredients));
            partHeader.Blocks.Add (partIngredients);

            partHeader.Blocks.Add (new Doc.Paragraph (new Doc.Run("Instructions:\r\n" + part.Instructions)));

            string comments = (part.Comments == null || part.Comments.Count == 0) ? "" :
               part.Comments
               .Aggregate(
                  new StringBuilder (),
                  (sb, t) =>
                  {
                     sb.Append ("On " + t.CreatedOn + ", " + t.UserName + " commented: " + t.Text + Environment.NewLine);
                     return sb;
                  },
                  sb => sb.ToString (0, sb.Length - Environment.NewLine.Length));
            partHeader.Blocks.Add (new Doc.Paragraph (new Doc.Run(comments)));
         }

         return doc;
      }

      private static string IngredientToString(IIngredientDetail ing)
      {
         StringBuilder s = new StringBuilder ();
         if (ing.Quantity != Fraction.Zero)
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Quantity);
         }

         if (ing.Amount != Fraction.Zero)
         {
            if (s.Length > 0)
               s.Append (' ');
            s.Append (ing.Amount);
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

      private AutoAddCollection<RecipePartViewModel> m_recipeParts;
      private Sellars.Meal.UI.Model.Recipe  m_recipe;
      private RatingViewModel m_userRating;
      private bool m_editMode;

      public class RatingViewModel : NotifyPropertyChangedObject
      {
         public RatingViewModel (double rating)
         {
            m_rating = rating;
         }

         public bool IsDirty
         {
            get
            {
               return m_isDirty;
            }
            set
            {
               SetValue(ref m_isDirty, value, "IsDirty");
            }
         }

         public double Rating
         {
            get
            {
               return m_rating;
            }
            set
            {
               IsDirty = true;
               SetValue(ref m_rating, value, "Rating");
            }
         }

         private double m_rating;
         private bool m_isDirty;
      }
   }
}
