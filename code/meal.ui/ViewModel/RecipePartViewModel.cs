using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.UI.Model;
using System.ComponentModel;

namespace Sellars.Meal.UI.ViewModel
{
   public class RecipePartViewModel : NotifyPropertyChangedObject
   {
      public RecipePartViewModel ()
      {
         Visible = true;
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
               m_ingredients = new AutoAddCollection<Model.IngredientDetail> (value.Ingredients);
               m_ingredients.CreateNewItem = 
                  delegate
                  {
                     return new Sellars.Meal.UI.Model.IngredientDetail ();
                  };
               m_ingredients.CollectionChanged += 
                  (s, dea) =>
                  {
                     if (m_model.Ingredients == null)
                        m_model.Ingredients = new System.Collections.ObjectModel.ObservableCollection<IngredientDetail> ();
                     m_model.Ingredients.Clear ();
                     foreach (var ingredient in m_ingredients)
                        m_model.Ingredients.Add (ingredient);
                     base.OnPropertyChanged (new PropertyChangedEventArgs ("Ingredients"));
                  };
            }
         }
      }

      public AutoAddCollection<Model.IngredientDetail> Ingredients
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

      private bool m_visible;
      private Model.RecipePart m_model;
      private AutoAddCollection<Model.IngredientDetail>  m_ingredients;
   }
}
