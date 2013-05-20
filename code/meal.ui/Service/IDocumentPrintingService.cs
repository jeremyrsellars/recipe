using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;

using Sellars.Service;

namespace Sellars.Meal.UI.Service
{
   public interface IDocumentPrintingService : IService
   {
      void PrintDocument (FlowDocument flowDocument, string name);
   }
}
