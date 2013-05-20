using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Collections.Generic;
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.Svc.Persistance
{
   [Serializable]
   public class IngredientDetail : IIngredientDetail
   {
      public static IngredientDetail FromIngredientDetail (IIngredientDetail ingredientDetail)
      {
         if (ingredientDetail == null)
            return null;
         Sellars.Data.Model.ModelId<IRecipe> recipeId = ingredientDetail.Ingredient == null ? null : ingredientDetail.Ingredient.Id;
         IngredientDetail i = new IngredientDetail
         {
            Ingredient = ingredientDetail.Ingredient == null ? "" : ingredientDetail.Ingredient.Name,
            IngredientRecipeId = recipeId == null ? Guid.Empty : recipeId.Id,
            Quantity = ingredientDetail.Quantity.ToString (),
            Amount = ingredientDetail.Amount.ToString (),
            AmountMax = ingredientDetail.AmountMax.ToString (),
            Unit = ingredientDetail.Unit == null ? "" : ingredientDetail.Unit.Name,
         };
         if (ingredientDetail.Preparation == null)
            i.Preparation = "";
         else
            i.Preparation = string.Join ("|", ingredientDetail.Preparation.ToArray ());
         return i;
      }
      
      public string Ingredient;  // {get;set;}
      public Guid IngredientRecipeId;  // {get;set;}
      public string Preparation;  // {get;set;}
      public string Quantity;  // {get;set;}
      public string Amount;  // {get;set;}
      public string AmountMax;  // {get;set;}
      public string Unit;  // {get;set;}

      #region IIngredientDetail Members

      IIngredient IIngredientDetail.Ingredient
      {
         get { return new Ingredient {Name=Ingredient, RecipeId = IngredientRecipeId}; }
      }

      IReadonlyList<string> IIngredientDetail.Preparation
      {
         get
         {
            if (string.IsNullOrEmpty (Preparation))
               return new ReadonlyList<string> ();
            return new ReadonlyList<string> (Preparation.Split ('|'));
         }
      }

      Fraction IIngredientDetail.Quantity
      {
         get
         {
            Fraction f;
            Fraction.TryParse (Quantity, out f);
            return f;
         }
      }

      Fraction IIngredientDetail.Amount
      {
         get
         {
            Fraction f;
            Fraction.TryParse (Amount, out f);
            return f;
         }
      }

      Fraction IIngredientDetail.AmountMax
      {
         get
         {
            Fraction f;
            Fraction.TryParse (AmountMax, out f);
            return f;
         }
      }

      IUnit IIngredientDetail.Unit
      {
         get { return new Unit {Name=Unit}; }
      }

      #endregion
   }
}
