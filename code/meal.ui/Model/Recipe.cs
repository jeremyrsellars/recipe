using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars;
using Sellars.Collections.Generic;
using Sellars.Data.Model;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class Recipe : NamedItem, IRecipe
   {
      public static Recipe FromRecipe (IRecipe recipe)
      {
         Recipe r = new Recipe
         {
            Name=recipe.Name,
            Servings=recipe.Servings,
            Parts=new List<RecipePart> (recipe.Parts.Select<IRecipePart,RecipePart>(RecipePart.FromRecipePart)),
            Source=Source.FromSource (recipe.Source),
            CreatedOn=recipe.CreatedOn,
            CreatedBy=recipe.CreatedBy,
         };
         if (recipe.Comments != null)
            r.Comments= new List<Comment> (recipe.Comments.Select<IComment,Comment>(Comment.FromComment));
         if (recipe.Ratings != null)
            r.Ratings= new List<Rating> (recipe.Ratings.Select<IRating,Rating>(Rating.FromRating));
         if (recipe.Tags != null)
            r.Tags= new List<Tag> (recipe.Tags.Select<ITag,Tag>(Tag.FromTag));
         return r;
      }
      
      public ModelId<IRecipe> Id{get;set;}
      public Fraction Servings{get;set;}
      public Source Source{get;set;}
      public DateTime CreatedOn{get;set;}
      public string CreatedBy{get;set;}

      public List<RecipePart> Parts
      {
         get
         {
            if (m_parts == null)
               m_parts = new List<RecipePart> ();
            return m_parts;
         }
         set
         {
            m_parts = value;
         }
      }
      public List<Comment> Comments
      {
         get
         {
            if (m_comments == null)
               m_comments = new List<Comment> ();
            return m_comments;
         }
         set
         {
            m_comments = value;
         }
      }

      public List<Rating> Ratings
      {
         get
         {
            if (m_ratings == null)
               m_ratings = new List<Rating> ();
            return m_ratings;
         }
         set
         {
            m_ratings = value;
         }
      }

      public List<Tag> Tags
      {
         get
         {
            if (m_tags == null)
               m_tags = new List<Tag> ();
            return m_tags;
         }
         set
         {
            m_tags = value;
         }
      }

      IReadonlyList<IRecipePart> IRecipe.Parts
      {
         get
         {
            return new ReadonlyList<IRecipePart> (Parts.Cast<IRecipePart> () ?? new IRecipePart[0]);
         }
      }
      IReadonlyList<IComment> IRecipe.Comments
      {
         get
         {
            return new ReadonlyList<IComment> (Comments.Cast<IComment> () ?? new IComment[0]);
         }
      }

      IReadonlyList<IRating> IRecipe.Ratings
      {
         get
         {
            return new ReadonlyList<IRating> (Ratings.Cast<IRating> () ?? new IRating[0]);
         }
      }

      IReadonlyList<ITag> IRecipe.Tags
      {
         get
         {
            return new ReadonlyList<ITag> (Tags.Cast<ITag> () ?? new ITag[0]);
         }
      }

      ISource IRecipe.Source
      {
         get
         {
            return Source;
         }
      }

      private List<RecipePart> m_parts;
      private List<Comment> m_comments;
      private List<Rating> m_ratings;
      private List<Tag> m_tags;
   }
}
