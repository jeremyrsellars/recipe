using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Windows;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipeViewModel : NotifyPropertyChangedObject
   {
      public RecipeViewModel ()
      {
         SaveCommand = new RelayCommand (Save_Execute, Save_Enabled);
         PrintCommand = new RelayCommand (Print_Execute);
         DeleteTagCommand = new RelayCommand (DeleteTag_Execute, DeleteTag_Enabled);
         //EditMode = true;
         ShouldAutoSaveTagsAndRatings = !EditMode;
      }

      public string FileName{get;set;}
      public ICommand SaveCommand{get;private set;}
      public ICommand PrintCommand{get;private set;}
      public ICommand DeleteTagCommand{get;private set;}
      public ICommand ShowRecipeCommand{get;set;}
      
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
            ShouldAutoSaveTagsAndRatings = !EditMode;
         }
      }

      public bool
      ShouldAutoSaveTagsAndRatings
      {
         get
         {
            return m_autoSave ?? false;
         }
         set
         {
            if (m_autoSave == value)
               return;
            m_autoSave = value;
            OnPropertyChanged ("ShouldAutoSaveTagsAndRatings");
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
            m_remainingTags = null;
            m_usedTagNames = null;

            // Clean up old event handlers.
            if (m_recipeParts != null)
               m_recipeParts.CollectionChanged -= RecipeParts_CollectionChanged;
            if (m_tagsRegistration != null)
            {
               m_tagsRegistration.Unregister ();
               m_tagsRegistration = null;
            }
            if (m_tags != null)
            {
               m_tags.CollectionChanged -= Tags_CollectionChanged;
            }
            if (m_userRating != null)
               m_userRating.PropertyChanged -= UserRating_PropertyChanged;

            if (value == null)
            {
               RecipeParts = null;
               Tags = null;
               UserRating = null;
            }
            else
            {
               // Recipe parts
               var existingParts = 
                  value
                     .Parts
                     .Select (p => new RecipePartViewModel{Model=(RecipePart)p})
                     .ToList ();
               var recipeParts = new AutoAddCollection<ViewModel.RecipePartViewModel> (existingParts);
               recipeParts.CreateNewItem = 
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
               RecipeParts = recipeParts;
               RecipeParts.CollectionChanged += RecipeParts_CollectionChanged;

               // Tags
               var existingTags = 
                  value
                     .Tags
                     .Select (p => new TagViewModel{Model=(Tag)p});
               Tags = new AutoAddCollection<ViewModel.TagViewModel> (existingTags);
               Tags.CreateNewItem = 
                  delegate 
                  {
                     return 
                        new Sellars.Meal.UI.ViewModel.TagViewModel
                           {
                              Model = new Tag {Name = ""},
                              IsEditable = true,
                           };
                  };
               Tags.CollectionChanged += Tags_CollectionChanged;
               m_tagsRegistration = new ItemChangedRegistration (Tags, Tags_Item_CollectionChanged);
               m_tagsRegistration.Register ();

               // Ratings
               double rating;
               rating = 
                  (value.Ratings == null || value.Ratings.Count == 0) ?
                     0 :
                     (value.Ratings.Aggregate (
                        0.0,
                        (seed, r) => seed + r.Value) / value.Ratings.Count);

               var userRating = new RatingViewModel (rating);
               for (int i = 0; i < m_ratingDescriptions.Length; i++)
               {
                  string description = m_ratingDescriptions[i];
                  userRating.AddOption (i + 1, description);
               }
               UserRating = userRating;
               // Cause a rating to be added if the value property changes.
               UserRating.PropertyChanged += UserRating_PropertyChanged;
            }

            m_autoSave = null;
            ShouldAutoSaveTagsAndRatings = !EditMode;
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
         private set
         {
            SetValue(ref m_tags, value, "Tags");
         }
      }

      public AutoAddCollection<RecipePartViewModel>
      RecipeParts
      {
         get
         {
            return m_recipeParts;
         }
         private set
         {
            SetValue (ref m_recipeParts, value, "RecipeParts");
         }
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
      
      public IEnumerable<SourceViewModel>
      AllSources
      {
         get
         {
            return m_allSources ?? (m_allSources = new ObservableCollection<SourceViewModel> (GetSourcesCore ()));
         }
      }
      
      public IEnumerable<IIngredient>
      AllIngredients
      {
         get
         {
            return m_allIngredients ?? (m_allIngredients = new ObservableCollection<IIngredient> (GetIngredientsCore ()));
         }
      }
      
      public RatingViewModel UserRating
      {
         get
         {
            return m_userRating;
         }
         private set
         {
            SetValue(ref m_userRating, value, "UserRating");
         }
      }
      
      public Doc.FlowDocument
      Document
      {
         get
         {
            return CreateDocument (m_recipe, ShowRecipeCommand);
         }
      }

      private void AutoSaveIfNecessary ()
      {
         if (!ShouldAutoSaveTagsAndRatings)
            return;

         Save_Execute (null);
      }

      private void RecipeParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
      
      private void UserRating_PropertyChanged (object sender, PropertyChangedEventArgs ea)
      {
         if (ea.PropertyName != "Value")
            return;

         m_isRatingDirty = true;
         OnPropertyChanged ("Rating");
         AutoSaveIfNecessary ();
      }

      private void Tags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
      {
         UpdateTagsAndAutoSaveIfNecessary ();
      }

      private void Tags_Item_CollectionChanged(object sender, PropertyChangedEventArgs e)
      {
         UpdateTagsAndAutoSaveIfNecessary ();
      }

      private class ItemChangedRegistration
      {
         private PropertyChangedEventHandler m_itemChangedEventHandler;
         private INotifyCollectionChanged m_collection;

         public ItemChangedRegistration (INotifyCollectionChanged collection, PropertyChangedEventHandler itemChangedEventHandler)
         {
            if (itemChangedEventHandler == null)
               throw new ArgumentNullException ("itemChangedEventHandler");
            m_collection = collection;
            m_itemChangedEventHandler = itemChangedEventHandler;
         }

         public void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs ea)
         {
            if (ea.OldItems != null)
               RegisterItemChanged(ea.OldItems, m_itemChangedEventHandler, false);
            if (ea.NewItems != null)
               RegisterItemChanged(ea.NewItems, m_itemChangedEventHandler, false);
         }

         public void Register ()
         {
            RegisterItemChanged ((System.Collections.IEnumerable)m_collection, m_itemChangedEventHandler, false);
         }

         public void Unregister ()
         {
            RegisterItemChanged ((System.Collections.IEnumerable)m_collection, m_itemChangedEventHandler, true);
         }

         private void RegisterItemChanged (System.Collections.IEnumerable items, PropertyChangedEventHandler itemChangedEventHandler, bool unregister)
         {
            if (items is INotifyCollectionChanged)
            {
               var cc = (INotifyCollectionChanged) items;
               cc.CollectionChanged -= CollectionChangedHandler;
               if (!unregister)
                  cc.CollectionChanged += CollectionChangedHandler;
            }
            foreach (object item in items)
            {
               var npc = item as INotifyPropertyChanged;
               if (npc == null)
                  continue;
               npc.PropertyChanged -= itemChangedEventHandler;
               if (!unregister)
                  npc.PropertyChanged += itemChangedEventHandler;
            }
         }
      }

      private void UpdateTagsAndAutoSaveIfNecessary ()
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
         AutoSaveIfNecessary ();
      }
      
      private void ResetRemainingTags ()
      {
         m_usedTagNames.Clear ();
         foreach (string tagName in m_tags.Select(t => t.Name).Where (name => !string.IsNullOrWhiteSpace (name)))
         {
            m_usedTagNames.Add (tagName);
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

      private IEnumerable<IIngredient>
      GetIngredientsCore ()
      {
         var ingredients = 
            ServiceController.Get<Sellars.Meal.UI.Service.IIngredientService> ().GetIngredients ()
               .OrderBy (s => s.Name);
         return ingredients;
      }

      private IEnumerable<string>
      GetTagsCore ()
      {
         var tags = 
            ServiceController.Get<Sellars.Meal.UI.Service.ITagService> ().GetTags ();
         return tags;
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
            rating.UserName = Sellars.Meal.UI.Service.Impl.UserService.CurrentUser.Name;
            rating.Value = m_userRating.Value;
#if !MultipleRatings
            Recipe.Ratings.Clear ();
#endif
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

      private void Print_Execute (object parameter)
      {
         
      }

      private bool DeleteTag_Enabled (object parameter)
      {
         var tag = parameter as TagViewModel;
         return 
            m_tags != null
            && tag != null
            && !string.IsNullOrEmpty (tag.Name);
      }

      private void DeleteTag_Execute (object parameter)
      {
         m_tags.Remove ((TagViewModel)parameter);
      }

      public static Doc.FlowDocument
      CreateDocument (IRecipe recipe, ICommand linkToRecipeCommand)
      {
         Doc.FlowDocument doc = new Doc.FlowDocument ();
         doc.PagePadding = new System.Windows.Thickness (40);
         doc.ColumnWidth = 640;
         doc.Typography.Fraction = System.Windows.FontFraction.Slashed;
         doc.FontFamily = new System.Windows.Media.FontFamily ("Palatino Linotype");

         var cookTimeConverter = new Converters.CookTimespanConverter ();
         {
         Doc.Section recipeHeader = new Doc.Section ();
         doc.Blocks.Add (recipeHeader);

         Doc.Paragraph p;
         // Recipe Name
         p = recipeHeader.AddParagraph ();
         p.Inlines.Add (new Doc.Underline (new Doc.Run(recipe.Name)));


         if (recipe.Ratings != null && recipe.Ratings.Count > 0)
         {
            int rating = 
               (recipe.Ratings == null || recipe.Ratings.Count == 0)
                  ? 0
                  : (int)(recipe.Ratings.Aggregate (0.0, (sum, r) => sum + r.Value) / recipe.Ratings.Count);

            string ratedString = new string('«', rating);
            string notRatedString = new string('«', m_ratingDescriptions.Length - rating);
            p = recipeHeader.AddParagraph ();
            if (!string.IsNullOrEmpty (ratedString))
               p.Inlines.Add (new Doc.Run(ratedString){FontFamily = new System.Windows.Media.FontFamily ("Wingdings"), Foreground=System.Windows.Media.Brushes.Red});
            if (!string.IsNullOrEmpty (notRatedString))
               p.Inlines.Add (new Doc.Run(notRatedString){FontFamily = new System.Windows.Media.FontFamily ("Wingdings"), Foreground=System.Windows.Media.Brushes.Silver});
         }

         if (recipe.Source != null)
         {
            p = recipeHeader.AddParagraph ();
            p.Inlines.Add (new Doc.Italic (new Doc.Run("From ")));
            p.Inlines.Add (new Doc.Run(recipe.Source.Name));
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
            recipeHeader.AddParagraph ().Inlines.Add (new Doc.Run(yieldAndOrServings));
         }

         //string tags =
         //   recipe.Tags
         //   .Aggregate(
         //      new StringBuilder ("Tags: "),
         //      (sb, t) =>
         //      {
         //         sb.Append (t.Name);
         //         sb.Append (", ");
         //         return sb;
         //      },
         //      sb => sb[sb.Length - 2] == ',' ? sb.ToString (0, sb.Length - 2) : sb.ToString ());
         //recipeHeader.AddParagraph ().Inlines.Add (new Doc.Run(tags)));
         }

         foreach (IRecipePart part in recipe.Parts)
         {
            Doc.Section partHeader = new Doc.Section ();
            doc.Blocks.Add (partHeader);

            //
            // Part Name
            //
            if (!string.IsNullOrWhiteSpace (part.Name) && part.Name != recipe.Name)
            {
               var pName = partHeader.AddParagraph ();
               pName.Margin=new Thickness (0);
               pName.Inlines.Add (new Doc.Underline (new Doc.Run(part.Name)));
            }

            //
            // Times/prep method
            //
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
               Doc.Paragraph stats = partHeader.AddParagraph ();
               stats.Margin=new Thickness (0);
               stats.Inlines.Add (new Doc.Run(times.ToString ()));
            }

            //
            // Ingredients
            //
            if (part.Ingredients != null)
            {
               Doc.Paragraph partIngredients = partHeader.AddParagraph ();
               partIngredients.TextAlignment = TextAlignment.Left;
               partIngredients.Margin = new Thickness (0);
               partIngredients.Inlines.Add (new Doc.LineBreak ());
               partIngredients.Inlines.Add (new Doc.Run ("Ingredients:"));
               foreach (var ingredient in part.Ingredients)
               {
                  partIngredients.Inlines.Add (new Doc.LineBreak ());
                  Sellars.Data.Model.ModelId<IRecipe> recipeId;
                  if (ingredient.Ingredient == null)
                     recipeId = null;
                  else
                     recipeId = ingredient.Ingredient.Id;

                  string text = IngredientToString (ingredient);
                  string ingredientName = ingredient.Ingredient.Name;
                  if (recipeId != null && recipeId.Id != Guid.Empty && linkToRecipeCommand != null && !string.IsNullOrEmpty (ingredientName))
                  {
                     int iName = text.IndexOf (ingredientName);
                     if (iName > 0)
                        partIngredients.Inlines.Add (new Doc.Run (text.Substring (0, iName)));
                     partIngredients.Inlines.Add (new Doc.Hyperlink (new Doc.Run (ingredientName)){Command=linkToRecipeCommand, CommandParameter=recipeId});
                     if (text.Length - iName - ingredientName.Length > 0)
                        partIngredients.Inlines.Add (new Doc.Run (text.Substring (iName + ingredientName.Length)));
                  }
                  else
                  {
                     partIngredients.Inlines.Add (new Doc.Run (text));
                  }
               };
            }

            if (!string.IsNullOrWhiteSpace (part.Instructions))
            {
               var p = partHeader.AddParagraph ();
               p.Inlines.Add (new Doc.LineBreak ());
               p.Inlines.Add (new Doc.Run("Instructions:"));
               
               foreach (string paragraph in part.Instructions.Split (new [] {Environment.NewLine}, StringSplitOptions.None))
               {
                  p.Inlines.Add (new Doc.LineBreak ());
                  p.Inlines.Add (new Doc.Run(paragraph));
               }
            }
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
               partHeader.AddParagraph ().Inlines.Add (new Doc.Run(comments));
         }

         return doc;
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

      private Sellars.Meal.UI.Model.Recipe  m_recipe;
      private AutoAddCollection<RecipePartViewModel> m_recipeParts;
      private AutoAddCollection<TagViewModel> m_tags;
      private ItemChangedRegistration m_tagsRegistration;
      private RatingViewModel m_userRating;
      private SourceViewModel m_sourceVM;
      private bool m_editMode;
      private bool? m_autoSave;
      private bool m_isRatingDirty;
      private static ObservableCollection<SourceViewModel> m_allSources;
      private static ObservableCollection<IIngredient> m_allIngredients;
      private static ObservableCollection<string> m_allTags;
      private static ExceptList<string> m_remainingTags;
      private ObservableCollection<string> m_usedTagNames;
      private static readonly string [] m_ratingDescriptions = {"Terrible", "Bad", "OK", "Good", "Great"};

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

   static class DocExtensions
   {
      public static Doc.Paragraph AddParagraph (this Doc.FlowDocument doc)
      {
         Doc.Paragraph paragraph = CreateParagraph ();
         doc.Blocks.Add (paragraph);
         return paragraph;
      }

      public static Doc.Paragraph AddParagraph (this Doc.Section doc)
      {
         Doc.Paragraph paragraph = CreateParagraph ();
         doc.Blocks.Add (paragraph);
         return paragraph;
      }

      private static Doc.Paragraph CreateParagraph ()
      {
         Doc.Paragraph paragraph = new Doc.Paragraph ();
         paragraph.TextAlignment = TextAlignment.Left;
         paragraph.Margin = new Thickness (0);
         return paragraph;
      }
   }
}
