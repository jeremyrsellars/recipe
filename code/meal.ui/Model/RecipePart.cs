using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class RecipePart : IRecipePart
   {
      public static RecipePart FromRecipePart (IRecipePart recipePart)
      {
         RecipePart r = new RecipePart
         {
            Name = recipePart.Name,
            PreparationMethod = recipePart.PreparationMethod == null ? null : Tag.FromTag (recipePart.PreparationMethod),
            PreparationTime = recipePart.PreparationTime,
            CookTime = recipePart.CookTime,
            ChillTime = recipePart.ChillTime,
            Temperature = recipePart.Temperature,
            Ingredients = new ObservableCollection<IngredientDetail> ( recipePart.Ingredients.Select<IIngredientDetail,IngredientDetail>(IngredientDetail.FromIngredientDetail)),
            Instructions = recipePart.Instructions,
         };
         if (recipePart.Comments != null)
            r.Comments = new ObservableCollection<IComment> (recipePart.Comments.Select<IComment,Comment>(Comment.FromComment).Cast<IComment> ());
         return r;
      }
      
      public string Name{get;set;}
      public Tag PreparationMethod{get;set;}
      public TimeSpan PreparationTime{get;set;}
      public TimeSpan CookTime{get;set;}
      public TimeSpan ChillTime{get;set;}
      public int Temperature{get;set;}
      public ObservableCollection<IngredientDetail> Ingredients{get;set;}
      public string Instructions{get;set;}
      public ObservableCollection<IComment> Comments{get;set;}

      ITag IRecipePart.PreparationMethod
      {
         get
         {
            return PreparationMethod;
         }
      }

      IReadonlyList<IIngredientDetail> IRecipePart.Ingredients
      {
         get
         {
            if (Ingredients == null)
               return new ReadonlyList<IIngredientDetail> ();
            return new ReadonlyList<IIngredientDetail> (Ingredients.Cast<IIngredientDetail>());
         }
      }

      IReadonlyList<IComment> IRecipePart.Comments
      {
         get
         {
            if (Comments == null)
               return new ReadonlyList<IComment> ();
            return new ReadonlyList<IComment> (Comments);
         }
      }
   }
}
