using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Sellars.Windows.Input;

namespace Sellars.Meal.UI.ViewModel
{
   public class RatingViewModel : Sellars.Meal.UI.Model.NotifyPropertyChangedObject
   {
      public RatingViewModel ()
         : this (0)
      {
      }

      public RatingViewModel (double value)
      {
         m_value = value;
         m_options = new List<RatingOptionViewModel> ();
         m_selectCommand = new RelayCommand (SelectExecute);
      }

      public class RatingOptionViewModel : Sellars.Meal.UI.Model.NotifyPropertyChangedObject
      {
         public RatingOptionViewModel (RatingViewModel rating, double value, string description)
         {
            if (rating == null)
               throw new ArgumentNullException ("rating");
            Model = rating;
            Value = value;
            Description = description;
            Model.PropertyChanged += Model_PropertyChanged;
         }
         public double Value{get;private set;}
         public string Description{get;private set;}
         public RatingViewModel Model{get; private set;}
         public bool IsSelected
         {
            get
            {
               return m_isSelected;
            }
            set
            {
               SetValue(ref m_isSelected, value, "IsSelected");
            }
         }

         void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
         {
            if (e.PropertyName != "Value")
               return;
            IsSelected = Model.Value >= Value;
         }

         private bool m_isSelected;
      }

      public RatingOptionViewModel AddOption (double value, string description)
      {
         RatingOptionViewModel vm = new RatingOptionViewModel (this, value, description);
         m_options.Add (vm);
         OnPropertyChanged ("Value");
         return vm;
      }

      public IList<RatingOptionViewModel> Options
      {
         get
         {
            return m_options;
         }
      }

      public double Value
      {
         get
         {
            return m_value;
         }
         set
         {
            SetValue(ref m_value, value, "Value");
         }
      }

      public ICommand
      SelectCommand
      {
         get
         {
            return m_selectCommand;
         }
      }

      private static void SelectExecute (object parameter)
      {
         RatingOptionViewModel option = parameter as RatingOptionViewModel;
         if (option == null)
            return;

         option.Model.Value = option.Value;
      }

      private readonly IList<RatingOptionViewModel> m_options;
      private double m_value;
      private ICommand m_selectCommand;
   }
}
