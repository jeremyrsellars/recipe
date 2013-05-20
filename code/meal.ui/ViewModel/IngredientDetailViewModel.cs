using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Sellars.Meal.UI.Model;
using Sellars.Windows.Input;

namespace Sellars.Meal.UI.ViewModel
{
   public class IngredientDetailViewModel : Sellars.Meal.UI.Model.NotifyPropertyChangedObject
   {
      public IngredientDetailViewModel (RecipePartViewModel recipePartViewModel, Model.IngredientDetail ingredientDetail)
      {
         if (recipePartViewModel == null)
            throw new ArgumentNullException ("recipePartViewModel");
         if (ingredientDetail == null)
            throw new ArgumentNullException ("ingredientDetail");

         m_recipePartViewModel = recipePartViewModel;
         m_ingredientDetail = ingredientDetail;
         m_index = m_recipePartViewModel.Ingredients.IndexOf (this);
         m_amountRange = new FractionRange (ingredientDetail.Amount, ingredientDetail.AmountMax);
         m_amountRange.PropertyChanged += 
            (s, dea) => 
               {
                  m_ingredientDetail.Amount = m_amountRange.Amount;
                  m_ingredientDetail.AmountMax = m_amountRange.AmountMax;
               };
      }

      public IngredientDetail IngredientDetail
      {
         get
         {
            return m_ingredientDetail;
         }
      }

      public void ResetIndex ()
      {
         Index = m_recipePartViewModel.Ingredients.IndexOf (this);
      }

      public int Index
      {
         get
         {
            return m_index;
         }
         set
         {
            if (m_index == value)
               return;
            m_ingredientDetail.Index = value;
            SetValue (ref m_index, value, "Index");
         }
      }

      public string Ingredient
      {
         get
         {
            return m_ingredientDetail.Ingredient;
         }
         set
         {
            if (m_ingredientDetail.Ingredient == value)
               return;
            m_ingredientDetail.Ingredient = value;
            OnPropertyChanged ("Ingredient");
         }
      }

      public string Preparation
      {
         get
         {
            return m_ingredientDetail.Preparation;
         }
         set
         {
            if (m_ingredientDetail.Preparation == value)
               return;
            m_ingredientDetail.Preparation = value;
            OnPropertyChanged ("Preparation");
         }
      }

      public Fraction Quantity
      {
         get
         {
            return m_ingredientDetail.Quantity;
         }
         set
         {
            if (m_ingredientDetail.Quantity == value)
               return;
            m_ingredientDetail.Quantity = value;
            OnPropertyChanged ("Quantity");
         }
      }

      public FractionRange Amount
      {
         get
         {
            return m_amountRange;
         }
      }

      public Unit Unit
      {
         get
         {
            return m_ingredientDetail.Unit;
         }
         set
         {
            if (m_ingredientDetail.Unit == value)
               return;
            m_ingredientDetail.Unit = value;
            OnPropertyChanged ("Unit");
         }
      }

      public ICommand
      MoveUpCommand
      {
         get
         {
            return m_recipePartViewModel.MoveUpCommand;
         }
      }

      public ICommand
      MoveDownCommand
      {
         get
         {
            return m_recipePartViewModel.MoveDownCommand;
         }
      }

      public ICommand
      DeleteCommand
      {
         get
         {
            return m_recipePartViewModel.DeleteCommand;
         }
      }

      private readonly RecipePartViewModel m_recipePartViewModel;
      private readonly Model.IngredientDetail m_ingredientDetail;
      private readonly FractionRange m_amountRange;
      private int m_index;
   }
}
