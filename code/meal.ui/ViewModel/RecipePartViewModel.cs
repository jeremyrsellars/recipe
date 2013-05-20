using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Sellars.Windows.Input;

using Sellars.Meal.UI.Model;
using System.ComponentModel;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipePartViewModel : NotifyPropertyChangedObject
   {
      public RecipePartViewModel ()
      {
         Visible = true;

         m_moveUpCommand = new RelayCommand (MoveUpExecute, MoveUpEnabled);
         m_moveDownCommand = new RelayCommand (MoveDownExecute, MoveDownEnabled);
         m_deleteCommand = new RelayCommand (DeleteExecute, DeleteEnabled);
      }
      
      public Model.RecipePart Model
      {
         get
         {
            return m_model;
         }
         set
         {
            if (m_model == value)
               return;
            m_model = value;
            if (value != null)
            {
               m_ingredients = 
                  new AutoAddCollection<IngredientDetailViewModel> ();
               if (value.Ingredients != null)
               {
                  foreach (var ingredient in value.Ingredients.OrderBy(model => model.Index))
                  {
                     m_ingredients.Add (new IngredientDetailViewModel (this, ingredient));
                  }
               }
               ResetIngredientIndexes ();
               m_ingredients.CollectionChanged += 
                  (s, dea) =>
                  {
                     if (m_model.Ingredients == null)
                        m_model.Ingredients = new System.Collections.ObjectModel.ObservableCollection<IngredientDetail> ();
                     m_model.Ingredients.Clear ();
                     foreach (var ingredient in m_ingredients.Select (vm => vm.IngredientDetail))
                        m_model.Ingredients.Add (ingredient);
                     base.OnPropertyChanged (new PropertyChangedEventArgs ("Ingredients"));
                  };
            }
         }
      }

      public AutoAddCollection<IngredientDetailViewModel> Ingredients
      {
         get
         {
            return m_ingredients;
         }
      }
      
      public bool Visible
      {
         get
         {
            return m_visible;
         }
         set
         {
            SetValue (ref m_visible, value, "Visible");
         }
      }
      
      public string Name
      {
         get
         {
            return m_model.Name;
         }
         set
         {
            if (m_model.Name == value)
               return;
            m_model.Name = value;
            OnPropertyChanged ("Name");
         }
      }

      public ICommand
      MoveUpCommand
      {
         get
         {
            return m_moveUpCommand;
         }
      }

      public ICommand
      MoveDownCommand
      {
         get
         {
            return m_moveDownCommand;
         }
      }

      public ICommand
      DeleteCommand
      {
         get
         {
            return m_deleteCommand;
         }
      }

      private void MoveUpExecute (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         if (ingredient == null)
            return;

         var top = ingredient;
         top.Index--;
         var bottom = m_ingredients[m_ingredients.IndexOf (ingredient) - 1];
         bottom.Index++;

         Flip (top, bottom);
         ResetIngredientIndexes ();
      }

      private bool MoveUpEnabled (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         return ingredient != null && m_ingredients.IndexOf (ingredient) != 0 && ingredient.Index != m_ingredients.Count - 1;
      }

      private void MoveDownExecute (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         if (ingredient == null)
            return;

         var bottom = ingredient;
         bottom.Index++;
         var top = m_ingredients[m_ingredients.IndexOf (ingredient) + 1];
         top.Index--;

         Flip (top, bottom);
         ResetIngredientIndexes ();
      }

      private bool MoveDownEnabled (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         return ingredient != null && m_ingredients.IndexOf (ingredient) < m_ingredients.Count - 2;
      }

      private void DeleteExecute (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         if (ingredient == null)
            return;

         m_ingredients.Remove (ingredient);
         ResetIngredientIndexes ();
      }

      private bool DeleteEnabled (object parameter)
      {
         var ingredient = parameter as IngredientDetailViewModel;
         return ingredient != null && ingredient.Index < m_ingredients.Count - 1;
      }

      private void Flip (IngredientDetailViewModel newTop, IngredientDetailViewModel newBottom)
      {
         m_ingredients[newTop.Index] = newTop;
         m_ingredients[newBottom.Index] = newBottom;
      }

      private void ResetIngredientIndexes ()
      {
         m_ingredients.CreateNewItem = null;
         try
         {
            foreach (var ingredient in m_ingredients)
            {
               ingredient.ResetIndex ();
            }
         }
         finally
         {
            m_ingredients.CreateNewItem = 
               delegate
               {
                  return new IngredientDetailViewModel (this, new Sellars.Meal.UI.Model.IngredientDetail ()){Index = m_ingredients.Count - 1};
               };
         }
      }

      private ICommand m_moveUpCommand;
      private ICommand m_moveDownCommand;
      private ICommand m_deleteCommand;

      private bool m_visible;
      private Model.RecipePart m_model;
      private AutoAddCollection<IngredientDetailViewModel>  m_ingredients;
   }
}
