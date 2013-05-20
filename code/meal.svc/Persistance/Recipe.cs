using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Data.Model;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Recipe : IRecipe, IModelId<Recipe>
   {
      public static Recipe FromRecipe (IRecipe recipe)
      {
         Recipe r = new Recipe
         {
            Name=recipe.Name,
            Id = (recipe.Id == null || recipe.Id.Id == Guid.Empty) ? Guid.NewGuid () : recipe.Id.Id,
            Servings=recipe.Servings.ToString (),
            Yield=recipe.Yield.ToString (),
            YieldUnit=recipe.YieldUnit == null ? "" : recipe.YieldUnit.Name,
            Parts=recipe.Parts.Select<IRecipePart,RecipePart>(RecipePart.FromRecipePart).Where(x => x != null).ToArray (),
            Source=Sellars.Meal.Svc.Persistance.Source.FromSource(recipe.Source),
            CreatedOn=recipe.CreatedOn,
            CreatedBy=recipe.CreatedBy,
         };
         if (recipe.Comments != null)
            r.Comments=recipe.Comments.Select<IComment,Comment>(Comment.FromComment).Where(x => x != null).ToArray ();
         if (recipe.Ratings != null)
            r.Ratings=recipe.Ratings.Select<IRating,Rating>(Rating.FromRating).Where(x => x != null).ToArray ();
         if (recipe.Tags != null)
            r.Tags=recipe.Tags.Select<ITag,Tag>(Tag.FromTag).Where(x => x != null).ToArray ();
         return r;
      }
      
      #region IRecipe Members

      public string Name;  // {get;set;}
      public string Servings;  // {get;set;}
      public string Yield;  // {get;set;}
      public string YieldUnit;  // {get;set;}
      public RecipePart[] Parts;  // {get;set;}
      public Comment[] Comments;  // {get;set;}
      public Rating[] Ratings;  // {get;set;}
      public Tag[] Tags;  // {get;set;}
      public ISource Source;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}
      public string CreatedBy;  // {get;set;}
      public Guid Id;  // {get;set;}

      double IRecipeHeader.Rating
      {
         get
         {
            if (Ratings == null)
               return 0;
            double ct = Ratings.Count ();
            double value =
               Ratings.Aggregate(
                  (double)0, 
                  (sum, rating) => sum + rating.Value, 
                  sum => sum / ct);
            return value;
         }
      }

      string IRecipeHeader.Name
      {
         get { return Name; }
      }

      string IRecipe.Name
      {
         get { return Name; }
      }

      Fraction IRecipe.Servings
      {
         get
         {
            Fraction f;
            Fraction.TryParse (Servings, out f);
            return f;
         }
      }

      Fraction IRecipe.Yield
      {
         get
         {
            Fraction f;
            Fraction.TryParse (Yield, out f);
            return f;
         }
      }

      IUnit IRecipe.YieldUnit
      {
         get
         {
            return new Unit {Name = YieldUnit};
         }
      }

      IReadonlyList<IRecipePart> IRecipe.Parts
      {
         get { return new ReadonlyList<IRecipePart> (Parts.Cast<IRecipePart> ()); }
      }

      IReadonlyList<IComment> IRecipe.Comments
      {
         get
         {
            if (Comments == null)
               return new ReadonlyList<IComment> ();
            return new ReadonlyList<IComment> (Comments.Cast<IComment> ());
         }
      }

      IReadonlyList<IRating> IRecipe.Ratings
      {
         get
         {
            if (Ratings == null)
               return new ReadonlyList<IRating> ();
            return new ReadonlyList<IRating> (Ratings.Cast<IRating> ());
         }
      }

      IReadonlyList<ITag> IRecipe.Tags
      {
         get
         {
            if (Tags == null)
               return new ReadonlyList<ITag> ();
            return new ReadonlyList<ITag> (Tags.Cast<ITag>());
         }
      }

      ISource IRecipe.Source
      {
         get { return Source; }
      }

      DateTime IRecipe.CreatedOn
      {
         get { return CreatedOn; }
      }

      string IRecipe.CreatedBy
      {
         get { return CreatedBy; }
      }

      #endregion

      #region IModelId<IRecipe> Members

      Sellars.Data.Model.ModelId<IRecipe> Sellars.Data.Model.IModelId<IRecipe>.Id
      {
         get { return new Data.Model.ModelId<IRecipe> (Id); }
      }

      #endregion

      #region IModelId<Recipe> Members

      ModelId<Recipe> IModelId<Recipe>.Id
      {
         get
         {
            if (Id == Guid.Empty)
               Id = Guid.NewGuid ();
            return new ModelId<Recipe> (Id);
         }
      }

      #endregion

      string ICandidateKey<IRecipe,string>.Key
      {
         get
         {
            return FileName;
         }
      }

      object ICandidateKey.Key
      {
         get
         {
            return FileName;
         }
      }

      [NonSerialized]
      public string FileName;  // {get;set;}
   }
}
