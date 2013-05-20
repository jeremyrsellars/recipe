using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class RecipePart : IRecipePart
   {
      public static RecipePart FromRecipePart (IRecipePart recipePart)
      {
         RecipePart r = new RecipePart
         {
            Name = recipePart.Name,
            PreparationMethod = Tag.FromTag (recipePart.PreparationMethod),
            PreparationSeconds = (int)recipePart.PreparationTime.TotalSeconds,
            CookSeconds = (int)recipePart.CookTime.TotalSeconds,
            ChillSeconds = (int)recipePart.ChillTime.TotalSeconds,
            Temperature = recipePart.Temperature,
            Ingredients = 
               recipePart.Ingredients
                  .Select<IIngredientDetail,IngredientDetail>(IngredientDetail.FromIngredientDetail)
                  .Where(i => !string.IsNullOrWhiteSpace (i.Amount) || !string.IsNullOrWhiteSpace (i.Ingredient) || !string.IsNullOrWhiteSpace (i.Preparation) || !string.IsNullOrWhiteSpace (i.Quantity))
                  .ToArray (),
            Instructions = new [] {recipePart.Instructions},
         };
         if (recipePart.Comments != null)
            r.Comments = recipePart.Comments.Select<IComment,Comment>(Comment.FromComment).ToArray ();
         return r;
      }
      
      public string Name;  // {get;set;}
      public Tag PreparationMethod;  // {get;set;}
      public int PreparationSeconds;  // {get;set;}
      public int CookSeconds;  // {get;set;}
      public int ChillSeconds;  // {get;set;}
      public int Temperature;  // {get;set;}
      public IngredientDetail[] Ingredients;  // {get;set;}
      public string [] Instructions;  // {get;set;}
      public Comment[] Comments;  // {get;set;}

      #region IRecipePart Members

      string IRecipePart.Name
      {
         get
         {
            return Name;
         }
      }

      ITag IRecipePart.PreparationMethod
      {
         get { return PreparationMethod; }
      }

      TimeSpan IRecipePart.PreparationTime
      {
         get { return TimeSpan.FromSeconds (PreparationSeconds); }
      }

      TimeSpan IRecipePart.CookTime
      {
         get { return TimeSpan.FromSeconds (CookSeconds); }
      }

      TimeSpan IRecipePart.ChillTime
      {
         get { return TimeSpan.FromSeconds (ChillSeconds); }
      }

      int IRecipePart.Temperature
      {
         get { return Temperature; }
      }

      IReadonlyList<IIngredientDetail> IRecipePart.Ingredients
      {
         get { return new ReadonlyList<IIngredientDetail> (Ingredients.Cast<IIngredientDetail>()); }
      }

      string IRecipePart.Instructions
      {
         get
         {
            if (Instructions == null || Instructions.Length == 0)
               return null;
            return string.Join (Environment.NewLine, Instructions);
         }
      }

      IReadonlyList<IComment> IRecipePart.Comments
      {
         get
         {
            if (Comments == null)
               return new ReadonlyList<IComment> ();
            return new ReadonlyList<IComment> (Comments.Cast<IComment>());
         }
      }

      #endregion
   }
}
