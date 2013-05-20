using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sellars.Meal.UI.Model;
using System.Windows.Input;
using Sellars.Windows.Input;
using Sellars.Service;
using Sellars.Meal.Svc.Service;
using Sellars.Collections.Generic;
using Doc=System.Windows.Documents;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipeViewModel : NotifyPropertyChangedObject
   {
      public RecipeViewModel ()
      {
         SaveCommand = new RelayCommand (Save_Execute, Save_Enabled);
         EditMode = true;
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
            m_isRatingDirty = false;
            if (value == null)
            {
               if (m_recipeParts != null)
                  m_recipeParts.CollectionChanged -= m_recipeParts_CollectionChanged;
               m_recipeParts = null;
               if (m_tags != null)
                  m_tags.CollectionChanged -= m_tags_CollectionChanged;
               m_tags = null;
               m_userRating = null;
            }
            else
            {
               // Recipe parts
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
                                    PreparationMethod = new Tag {Name=""},
                                 },
                           };
                  };
               m_recipeParts.CollectionChanged += m_recipeParts_CollectionChanged;

               // Tags
               var existingTags = 
                  value
                     .Tags
                     .Select (p => new TagViewModel{Model=(Tag)p})
                     .ToList ();
               m_tags = new AutoAddCollection<ViewModel.TagViewModel> (existingTags);
               m_tags.CreateNewItem = 
                  delegate 
                  {
                     return 
                        new Sellars.Meal.UI.ViewModel.TagViewModel
                           {
                              Model = new Tag {Name = ""},
                           };
                  };
               m_tags.CollectionChanged += m_tags_CollectionChanged;

               // Ratings
               double rating;
               rating = 
                  (value.Ratings == null || value.Ratings.Count == 0) ?
                     0 :
                     (value.Ratings.Aggregate (
                        0.0,
                        (seed, r) => seed + r.Value) / value.Ratings.Count);

               m_userRating = new RatingViewModel (rating);
               // Cause a rating to be added if the value property changes.
               m_userRating.PropertyChanged += 
                  (s, dea) =>
                     {
                        if (dea.PropertyName == "Value")
                           m_isRatingDirty = true;
                     };
               for (int i = 0; i < m_ratingDescriptions.Length; i++)
               {
                  string description = m_ratingDescriptions[i];
                  m_userRating.AddOption (i + 1, description);
               }
            }
            OnPropertyChanged ("Recipe");
            OnPropertyChanged ("Document");
         }
      }

      public string SourceName
      {
         get
         {
            return Source.Source.Name;
         }
         set
         {
            var sourceVm = new SourceViewModel (new Source {Name = value}, true);
            Source = sourceVm;
            UpdateSources (sourceVm);
         }
      }

      public SourceViewModel Source
      {
         get
         {
            if (Recipe.Source == null)
               m_sourceVM = new SourceViewModel (Recipe.Source = new Source (), false);
            if (m_sourceVM == null)
            {
               m_sourceVM = 
                  AllSources.FirstOrDefault (s => StringComparer.CurrentCultureIgnoreCase.Equals (s.Name, Recipe.Source.Name));
               if (m_sourceVM == null)
                  m_sourceVM = new SourceViewModel (Recipe.Source, true);
            }
            return m_sourceVM;
         }
         set
         {
            if (SetValue(ref m_sourceVM, value, "Source"))
            {
               if (value == null || value.Source == null)
                  Recipe.Source = null;
               else
                  Recipe.Source = value.Source as Source ?? new Source {Name = value.Name};
            }
         }
      }

      public AutoAddCollection<ViewModel.TagViewModel> Tags
      {
         get
         {
            return m_tags;
         }
      }

      void m_recipeParts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if (m_recipe == null)
            return; // weird
         
         var modelParts = m_recipe.Parts;
         modelParts.Clear ();
         modelParts.AddRange (
            m_recipeParts
               .Take (m_recipeParts.Count - 1)
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
      
      void m_tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
      {
         if (m_recipe == null)
            return; // weird
         
         var modelTags = m_recipe.Tags;
         modelTags.Clear ();
         modelTags.AddRange (
            m_tags
               .Take (m_tags.Count - 1)
               .Select (vm => vm.Model));
         ResetRemainingTags ();
      }
      
      public IEnumerable<string>
      AllTags
      {
         get
         {
            if (m_allTags == null)
            {
               var allTags = GetTagsCore ();
               m_allTags = allTags as ObservableCollection<string> ?? new ObservableCollection<string> (allTags);
            }
            return m_allTags;
         }
      }
      
      public IEnumerable<string>
      AllRemainingTags
      {
         get
         {
            if (m_usedTagNames == null)
            {
               m_usedTagNames = new ObservableCollection<string> ();
               ResetRemainingTags ();
            }
            if (m_remainingTags == null)
               m_remainingTags = new ExceptList<string> (AllTags, m_usedTagNames);
            return m_remainingTags;
         }
      }
      
      private void ResetRemainingTags ()
      {
         m_usedTagNames.Clear ();
         foreach (string tagName in m_tags.Select(t => t.Name).Where (name => !string.IsNullOrWhiteSpace (name)))
         {
            m_usedTagNames.Add (tagName);
         }
      }

      public IEnumerable<SourceViewModel>
      AllSources
      {
         get
         {
            return m_allSources ?? (m_allSources = new ObservableCollection<SourceViewModel> (GetSourcesCore ()));
         }
      }
      
      private SourceViewModel m_sourceUpdated;
      private void UpdateSources (SourceViewModel sourceUpdated)
      {
         int index = m_allSources.IndexOf (m_sourceUpdated);
         if (index >= 0)
         {
            if (sourceUpdated == null)
               m_allSources.RemoveAt (index);
            else
               m_allSources[index] = sourceUpdated;
         }
         else
         {
            for (int i = 0; i < m_allSources.Count; i++)
            {
               string name = m_allSources[i].Name;
               if (name == sourceUpdated.Name)
                  sourceUpdated = null;
               else if (name.CompareTo (sourceUpdated.Name) > 0)
                  m_allSources.Insert (i, sourceUpdated);
               else
                  continue;
               break;
            }
         }
         m_sourceUpdated = sourceUpdated;
      }

      private IEnumerable<SourceViewModel>
      GetSourcesCore ()
      {
         var sources = 
            ServiceController.Get<Sellars.Meal.UI.Service.ISourceService> ().GetSources ()
               .OrderBy (s => s.Name);
         foreach (Model.Source source in sources)
         {
            yield return new SourceViewModel (source, true);
         }
      }

      private IEnumerable<string>
      GetTagsCore ()
      {
         var tags = 
            ServiceController.Get<Sellars.Meal.UI.Service.ITagService> ().GetTags ();
         return tags;
      }

      public RatingViewModel UserRating
      {
         get
         {
            return m_userRating;
         }
      }
      
      private bool Save_Enabled (object parameter)
      {
         return m_recipe != null && !string.IsNullOrWhiteSpace (m_recipe.Name);
      }
      
      private void Save_Execute (object parameter)
      {
         if (m_isRatingDirty)
         {
            Rating rating = new Rating ();
            rating.CreatedOn = DateTime.Now;
            rating.UserName = Sellars.Meal.UI.Service.UserService.CurrentUser.Name;
            rating.Value = m_userRating.Value;
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
         doc.ColumnWidth = 640;
         doc.Typography.Fraction = System.Windows.FontFraction.Slashed;
         doc.FontFamily = new System.Windows.Media.FontFamily ("Palatino Linotype");
         
         var cookTimeConverter = new Converters.CookTimespanConverter ();
         {
         Doc.Section recipeHeader = new Doc.Section ();
         doc.Blocks.Add (recipeHeader);

         int rating;

         rating = 
            (recipe.Ratings == null || recipe.Ratings.Count == 0)
               ? 0
               : (int)(recipe.Ratings.Aggregate (0.0, (sum, r) => sum + r.Value) / recipe.Ratings.Count);

         string ratedString = new string('«', rating);
         string notRatedString = new string('«', m_ratingDescriptions.Length - rating);

         Doc.Paragraph p;
         recipeHeader.Blocks.Add (p = new Doc.Paragraph (new Doc.Underline (new Doc.Run(recipe.Name))));
         if (recipe.Source != null)
         {
            p.Inlines.Add (new Doc.Run(Environment.NewLine));
            
            if (!string.IsNullOrEmpty (ratedString))
               p.Inlines.Add (new Doc.Run(ratedString){FontFamily = new System.Windows.Media.FontFamily ("Wingdings"), Foreground=System.Windows.Media.Brushes.Red});
            if (!string.IsNullOrEmpty (notRatedString))
               p.Inlines.Add (new Doc.Run(notRatedString){FontFamily = new System.Windows.Media.FontFamily ("Wingdings"), Foreground=System.Windows.Media.Brushes.Silver});
            p.Inlines.Add (new Doc.Run(Environment.NewLine));
            p.Inlines.Add (new Doc.Italic (new Doc.Run("From ")));
            p.Inlines.Add (new Doc.Run(recipe.Source.Name + "\r\n"));

         }

         if (recipe.Servings.ToDouble () > 0 || recipe.Yield.ToDouble () > 0)
         {
            string yieldAndOrServings = null;
            if (recipe.Servings.ToDouble () > 0)
            {
               yieldAndOrServings = "Serves " + recipe.Servings;
            }
            if (recipe.Yield.ToDouble () > 0)
            {
               string yieldUnit = recipe.YieldUnit == null ? null : recipe.YieldUnit.Name;
               string makes = "Makes " + recipe.Yield + " " + yieldUnit;
               if (yieldAndOrServings != null)
                  yieldAndOrServings = yieldAndOrServings + " \u2022 " + makes;
               else
                  yieldAndOrServings = makes;
            }
            recipeHeader.Blocks.Add (new Doc.Paragraph (new Doc.Run(yieldAndOrServings)));
         }

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

            if (!string.IsNullOrWhiteSpace (part.Name) && part.Name != recipe.Name)
               partHeader.Blocks.Add (new Doc.Paragraph (new Doc.Underline (new Doc.Run(part.Name))));
            string prepMethod = part.PreparationMethod != null ? part.PreparationMethod.Name : null;

            SmartListFormatter times = new SmartListFormatter (" \u2022 ", ": ");
            if (part.PreparationTime != TimeSpan.Zero)
               times.Append ("Prep Time", cookTimeConverter.Convert (part.PreparationTime, typeof (string), null, null) as string);
            if (prepMethod != null)
               times.Append (prepMethod);
            if (part.CookTime != TimeSpan.Zero)
               times.Append ("Cook", cookTimeConverter.Convert (part.CookTime, typeof (string), null, null) as string);
            if (part.Temperature != 0)
               times.Append (part.Temperature + " \u00BA F");
            if (part.ChillTime != TimeSpan.Zero)
               times.Append ("Chill", cookTimeConverter.Convert (part.ChillTime, typeof (string), null, null) as string);

            if (times.GetStringBuilder ().Length > 0)
            {
               Doc.Paragraph stats = new Doc.Paragraph ();
               partHeader.Blocks.Add (stats);
               stats.Inlines.Add (new Doc.Run(times.ToString ()));
            }

            string ingredients = 
               part.Ingredients == null ? "" :
               "Ingredients:\r\n" + 
                     part.Ingredients.Aggregate(
                        new StringBuilder (),
                        (sb,ing) => sb.AppendLine (IngredientToString (ing)),
                        sb => sb.ToString (0, sb.Length == 0 ? 0 : sb.Length - Environment.NewLine.Length));
            Doc.Paragraph partIngredients = new Doc.Paragraph (new Doc.Run(ingredients));
            partHeader.Blocks.Add (partIngredients);

            if (!string.IsNullOrWhiteSpace (part.Instructions))
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
            if (!string.IsNullOrWhiteSpace (comments))
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

         if (ing.Quantity != Fraction.Zero && ing.Amount != Fraction.Zero)
            s.Append (" - ");

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

      private Sellars.Meal.UI.Model.Recipe  m_recipe;
      private AutoAddCollection<RecipePartViewModel> m_recipeParts;
      private AutoAddCollection<TagViewModel> m_tags;
      private RatingViewModel m_userRating;
      private SourceViewModel m_sourceVM;
      private bool m_editMode;
      private bool m_isRatingDirty;
      private static ObservableCollection<SourceViewModel> m_allSources;
      private static ObservableCollection<string> m_allTags;
      private static ExceptList<string> m_remainingTags;
      private ObservableCollection<string> m_usedTagNames;
      private static readonly string [] m_ratingDescriptions = {"Terrible","Bad", "OK", "Good", "Great"};

      public class SmartListFormatter
      {
         public SmartListFormatter (string delimiter, string pairDelimiter)
         {
            if (delimiter == null)
               throw new ArgumentNullException ("delimiter");
            if (pairDelimiter == null)
               throw new ArgumentNullException ("pairDelimiter");

            m_delimiter = delimiter;
            m_pairDelimiter = pairDelimiter;
            m_buf = new StringBuilder ();
         }

         public override string ToString ()
         {
            return m_buf.ToString ();
         }

         public StringBuilder GetStringBuilder ()
         {
            return m_buf;
         }

         public void Append (string value)
         {
            AppendDelimiterIfNecessary ();
            m_buf.Append (value);
         }

         public void Append (string caption, string value)
         {
            AppendDelimiterIfNecessary ();
            m_buf.Append (caption);
            m_buf.Append (m_pairDelimiter);
            m_buf.Append (value);
         }

         private void AppendDelimiterIfNecessary ()
         {
            if (m_buf.Length != 0)
               m_buf.Append (m_delimiter);
         }

         private StringBuilder m_buf;
         private string m_delimiter;
         private string m_pairDelimiter;
      }
   }
}
