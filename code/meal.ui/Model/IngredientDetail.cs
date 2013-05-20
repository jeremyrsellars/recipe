using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Text;

using Sellars;
using Sellars.Collections.Generic;

using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.Model
{
   public class IngredientDetail : NotifyPropertyChangedObject, IIngredientDetail, INotifyPropertyChanged
   {
      public static IngredientDetail FromIngredientDetail (IIngredientDetail ingredientDetail)
      {
         Sellars.Data.Model.ModelId<IRecipe> id = ingredientDetail.Ingredient.Id;
         IngredientDetail i = new IngredientDetail
         {
            Ingredient = ingredientDetail.Ingredient.Name,
            IngredientId = id == null ? Guid.Empty : id.Id,
            Quantity = ingredientDetail.Quantity,
            Amount = ingredientDetail.Amount,
            AmountMax = ingredientDetail.AmountMax,
            Unit = Unit.FromUnit (ingredientDetail.Unit),
            Preparation = GetPreparationString (ingredientDetail.Preparation)
         };
         return i;
      }
      public static string GetPreparationString (IReadonlyList<string> preparation)
      {
         if (preparation == null)
            return null;
         string prep = 
            preparation.Aggregate (
               new StringBuilder (),
               (buf, item) => 
               {
                  buf.Append (item);
                  buf.Append (", ");
                  return buf;
               },
               buf =>
               {
                  if (buf.Length > 0)
                     buf.Length -= 2;
                  return buf.ToString ();
               });
         return prep;
      }
      
      public int Index
      {
         get
         {
            return m_index;
         }
         set
         {
            SetValue (ref m_index, value, "Index");
         }
      }
      
      public string Ingredient
      {
         get
         {
            return m_ingredient;
         }
         set
         {
            SetValue (ref m_ingredient, value, "Ingredient");
         }
      }
      
      public Guid IngredientId
      {
         get
         {
            return m_ingredientId;
         }
         set
         {
            SetValue (ref m_ingredientId, value, "IngredientId");
         }
      }

      public string Preparation
      {
         get
         {
            return m_preparation;
         }
         set
         {
            SetValue (ref m_preparation, value, "Preparation");
         }
      }

      public Fraction Quantity
      {
         get
         {
            return m_quantity;
         }
         set
         {
            SetValue (ref m_quantity, value, "Quantity");
         }
      }

      public Fraction Amount
      {
         get
         {
            return m_amount;
         }
         set
         {
            SetValue (ref m_amount, value, "Amount");
         }
      }

      public Fraction AmountMax
      {
         get
         {
            return m_amountMax;
         }
         set
         {
            SetValue (ref m_amountMax, value, "AmountMax");
         }
      }

      public Unit Unit
      {
         get
         {
            return m_unit;
         }
         set
         {
            SetValue (ref m_unit, value, "Unit");
         }
      }

      IReadonlyList<string> IIngredientDetail.Preparation
      {
         get
         {
            string [] prep;
            if (string.IsNullOrEmpty (Preparation))
               prep = new string [0];
            else
               prep = Preparation.Split (new [] {", "}, StringSplitOptions.RemoveEmptyEntries);
            return new ReadonlyList<string> (prep);
         }
      }

      IIngredient IIngredientDetail.Ingredient
      {
         get
         {
            return 
               new Ingredient
                  {
                     Name = m_ingredient,
                     Id = new Data.Model.ModelId<IRecipe> (m_ingredientId)
                  };
         }
      }

      Fraction IIngredientDetail.Quantity
      {
         get { return Quantity; }
      }

      Fraction IIngredientDetail.Amount
      {
         get { return Amount; }
      }

      Fraction IIngredientDetail.AmountMax
      {
         get { return AmountMax; }
      }

      IUnit IIngredientDetail.Unit
      {
         get { return Unit; }
      }

      private int m_index;
      private string m_ingredient;
      private Guid m_ingredientId;
      private string m_preparation;
      private Fraction m_quantity;
      private Fraction m_amount;
      private Fraction m_amountMax;
      private Unit m_unit;
   }
}
