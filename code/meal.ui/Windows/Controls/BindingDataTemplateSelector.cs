using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Sellars.Meal.UI.ViewModel;

namespace Sellars.Meal.UI.Windows.Controls
{
   public class BindingDataTemplate
   {
      public BindingDataTemplate()
      {
      }
      public System.Windows.Data.Binding Binding{get;set;}
      public DataTemplate Template { get; set; }
      public object Value { get; set; }
   }

   public class BindingDataTemplateSelector : DataTemplateSelector
   {
      private List<BindingDataTemplate> m_templates;
      public BindingDataTemplateSelector ()
      {
         m_templates = new List<BindingDataTemplate> ();
      }

      public DataTemplate DefaultTemplate { get; set; }
      public List<BindingDataTemplate> Templates
      {
         get
         {
            return m_templates;
         }
      }

      public override DataTemplate SelectTemplate(object item, DependencyObject container)
      {
         foreach (var template in m_templates)
         {
            var value = DataBinder.Eval (item, template.Binding.Path.Path);
            if (value == template.Value
                || template.Value != null && template.Value.Equals (value))
            {
               return template.Template;
            }
         }
         return DefaultTemplate;
      }
   }
}
