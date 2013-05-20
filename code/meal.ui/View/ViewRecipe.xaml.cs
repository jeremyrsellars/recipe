using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sellars.Meal.UI.View
{
   /// <summary>
   /// Interaction logic for ViewRecipe.xaml
   /// </summary>
   public partial class ViewRecipe : UserControl
   {
      public ViewRecipe()
      {
         InitializeComponent();
         //this.DataContextChanged += 
         //   (s, dea) =>
         //      {
         //         rtxt.Document = ((dynamic)dea.NewValue).Document;
         //         rtxt.TextChanged += (s2,dea2) => {txt.Text = FormatDocument (rtxt.Document);};
         //         txt.Text = FormatDocument (rtxt.Document);
         //      };
      }

      public string FormatDocument (FlowDocument doc)
      {
         StringBuilder buf = new StringBuilder ();
         FormatBlocks (doc, doc.Blocks, buf, 0);

         return buf.ToString ();
      }

      public void FormatBlocks (FlowDocument doc, BlockCollection blocks, StringBuilder buf, int indent)
      {
         foreach (Block block in blocks)
         {
            FormatBlock (doc, block, buf, indent);
         }
      }

      public void FormatBlock (FlowDocument doc, Block block, StringBuilder buf, int indent)
      {
         // indent
         buf.Append (' ', indent);
         buf.AppendFormat (
            "{0} {1} to {2}", 
            block.GetType ().Name + block.GetHashCode (), 
            block.ContentStart.CompareTo (doc.ContentStart), 
            block.ContentEnd.CompareTo (doc.ContentStart));
         buf.AppendLine ();
         if (block is Section)
         {
            FormatBlocks (doc, ((Section)block).Blocks, buf, indent + 3);
         }
         else if (block is Paragraph)
         {
            FormatInlines (doc, ((Paragraph)block).Inlines, buf, indent + 3);
         }
      }

      public void FormatInlines (FlowDocument doc, InlineCollection inlines, StringBuilder buf, int indent)
      {
         foreach (Inline block in inlines)
         {
            FormatInline (doc, block, buf, indent);
         }
      }

      public void FormatInline (FlowDocument doc, Inline inline, StringBuilder buf, int indent)
      {
         // indent
         buf.Append (' ', indent);
         buf.AppendFormat (
            "{0} {1} to {2}", 
            inline.GetType ().Name + inline.GetHashCode (), 
            inline.ContentStart.CompareTo (doc.ContentStart), 
            inline.ContentEnd.CompareTo (doc.ContentStart));
         if (inline is Run)
         {
            buf.Append ("  \"" + ((Run)inline).Text + "\"");
         }
         buf.AppendLine ();
         if (inline is Underline)
         {
            FormatInlines (doc, ((Underline)inline).Inlines,  buf, indent + 3);
         }
         if (inline is Italic)
         {
            FormatInlines (doc, ((Italic)inline).Inlines,  buf, indent + 3);
         }
      }

   }
}
