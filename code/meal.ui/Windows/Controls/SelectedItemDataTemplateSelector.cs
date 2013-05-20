using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Sellars.Meal.UI.ViewModel;

namespace Sellars.Meal.UI.Windows.Controls
{
   public class SelectedItemDataTemplateSelector : DataTemplateSelector
   {
      public SelectedItemDataTemplateSelector ()
      {
         ToString ();
      }

      public DataTemplate HeaderTemplate { get; set; }
      public DataTemplate RecipeTemplate { get; set; }
      public DataTemplate DefaultTemplate { get; set; }
      
      public override DataTemplate SelectTemplate(object item, DependencyObject container)
      {
         if (item is RecipeViewModel)
            return RecipeTemplate ?? DefaultTemplate;
         if (item is RecipeHeaderViewModel)
            return HeaderTemplate ?? DefaultTemplate;
         return DefaultTemplate;
      }
   }
}
