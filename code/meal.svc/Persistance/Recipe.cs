using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class Recipe : IRecipe
   {
      public static Recipe FromRecipe (IRecipe recipe)
      {
         Recipe r = new Recipe
         {
            Name=recipe.Name,
            Servings=recipe.Servings.ToString (),
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
      public RecipePart[] Parts;  // {get;set;}
      public Comment[] Comments;  // {get;set;}
      public Rating[] Ratings;  // {get;set;}
      public Tag[] Tags;  // {get;set;}
      public ISource Source;  // {get;set;}
      public DateTime CreatedOn;  // {get;set;}
      public string CreatedBy;  // {get;set;}

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
         get { return new Data.Model.ModelId<IRecipe> (FileName); }
      }

      #endregion

      [NonSerialized]
      public string FileName;  // {get;set;}

   }
}
