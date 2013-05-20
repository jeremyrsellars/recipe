using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Sellars.Meal.UI.Model;
using System.ComponentModel;

namespace Sellars.Meal.UI.ViewModel
{
   public class TagViewModel : NotifyPropertyChangedObject
   {
      public TagViewModel ()
      {
      }
      
      public Model.Tag Model
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
         }
      }

      public string Name
      {
         get
         {
            return m_model == null ? "" : m_model.Name;
         }
         set
         {
            if (m_model == null)
               m_model = new Tag ();
            else if (m_model.Name == value)
               return;
            m_model.Name = value;
            m_model.UserName=Sellars.Meal.UI.Service.Impl.UserService.CurrentUser.Name;
            m_model.CreatedOn=DateTime.Now;
            if (!string.IsNullOrWhiteSpace (value))
               Sellars.Service.ServiceController.Get<Sellars.Meal.UI.Service.ITagService> ().AddTag (value);
            OnPropertyChanged ("Name");
         }
      }

      public bool IsEditable
      {
         get
         {
            return m_isEditable;
         }
         set
         {
            SetValue (ref m_isEditable, value, "IsEditable");
         }
      }

      private Model.Tag m_model;
      private bool m_isEditable;
   }
}
