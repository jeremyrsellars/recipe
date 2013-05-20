using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Xps;

using Sellars.Meal.UI.Service;
using Sellars.Service;

namespace Sellars.Meal.UI.Service.Impl
{
   public class DocumentPrintingService : IDocumentPrintingService, IService
   {
      public void PrintDocument(System.Windows.Documents.FlowDocument flowDocument, string name)
      {
         PrintDialog printDialog = new PrintDialog();
         if (printDialog.ShowDialog() == true)
         {
            printDialog.PrintDocument (((IDocumentPaginatorSource)flowDocument).DocumentPaginator, name);
         }
      }

      //private DocumentPaginator GetDocumentPaginator (FlowDocument flowDocument)
      //{
      //   return new RecipeDocumentPaginator (flowDocument.);
      //}

      //private void Print(FlowDocument document, PrintQueue pq)
      //{
      //   TextRange sourceDocument = new TextRange(document.ContentStart, document.ContentEnd);
      //   MemoryStream stream = new MemoryStream();
      //   sourceDocument.Save(stream, System.Windows.DataFormats.Xaml);
      //   // Clone the source document's content into a new FlowDocument.
      //   FlowDocument flowDocumentCopy = new FlowDocument();
      //   TextRange copyDocumentRange = new TextRange(flowDocumentCopy.ContentStart, flowDocumentCopy.ContentEnd);
      //   copyDocumentRange.Load(stream, System.Windows.DataFormats.Xaml);

      //   // Create a XpsDocumentWriter object, open a Windows common print dialog.
      //   // This methods returns a ref parameter that represents information about the dimensions of the printer media.
      //   XpsDocumentWriter docWriter = PrintQueue.CreateXpsDocumentWriter(pq);
      //   PageImageableArea ia = pq.GetPrintCapabilities().PageImageableArea;
      //   PrintTicket pt = pq.UserPrintTicket;
      //   if (docWriter != null && ia != null)
      //   {
      //      DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocumentCopy).DocumentPaginator;
      //      // Change the PageSize and PagePadding for the document to match the CanvasSize for the printer device.
      //      paginator.PageSize = new Size((double)pt.PageMediaSize.Width, (double)pt.PageMediaSize.Height);
      //      Thickness pagePadding = flowDocumentCopy.PagePadding;
      //      flowDocumentCopy.PagePadding = new Thickness(
      //      Math.Max(ia.OriginWidth, pagePadding.Left),
      //      Math.Max(ia.OriginHeight, pagePadding.Top),
      //      Math.Max((double)pt.PageMediaSize.Width - (double)(ia.OriginWidth + ia.ExtentWidth), pagePadding.Right),
      //      Math.Max((double)pt.PageMediaSize.Height - (double)(ia.OriginHeight + ia.ExtentHeight), pagePadding.Bottom));
      //      flowDocumentCopy.ColumnWidth = double.PositiveInfinity;
      //      // Send DocumentPaginator to the printer.
      //      docWriter.Write(paginator);
      //   }
      //}


      //private class RecipeDocumentPaginator : DocumentPaginator
      //{
      //    private Size              m_PageSize;
      //    private Size              m_Margin;
      //    private DocumentPaginator m_Paginator;
      //    private Typeface          m_Typeface;
    
      //    public RecipeDocumentPaginator(DocumentPaginator paginator, Size pageSize, Size margin)
      //    {
      //        m_PageSize  = pageSize;
      //        m_Margin    = margin;    
      //        m_Paginator = paginator;
 

      //        m_Paginator.PageSize = new Size(m_PageSize.Width  - margin.Width  * 2,
      //                                        m_PageSize.Height - margin.Height * 2);
      //    }
   
      //    Rect Move(Rect rect)
      //    {
      //        if (rect.IsEmpty)
      //        {
      //            return rect;
      //        }
      //        else
      //        {
      //            return new Rect(rect.Left + m_Margin.Width, rect.Top + m_Margin.Height, 

      //                            rect.Width, rect.Height);                
      //        }
      //    }
    
      //    public override DocumentPage GetPage(int pageNumber)
      //    {
      //        DocumentPage page = m_Paginator.GetPage(pageNumber);
 

      //        // Create a wrapper visual for transformation and add extras
      //        ContainerVisual newpage = new ContainerVisual();
 

      //        DrawingVisual title = new DrawingVisual();
 

      //        using (DrawingContext ctx = title.RenderOpen())
      //        {
      //            if (m_Typeface == null)
      //            {
      //                m_Typeface = new Typeface("Times New Roman");
      //            }
 

      //            FormattedText text = new FormattedText("Page " + (pageNumber + 1), 
      //                System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, 

      //                m_Typeface, 14, Brushes.Black);
 

      //            ctx.DrawText(text, new Point(0, -96 / 4)); // 1/4 inch above page content
      //        }
 

      //        DrawingVisual background = new DrawingVisual();
 

      //        using (DrawingContext ctx = background.RenderOpen())
      //        {
      //            ctx.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 240, 240)), null, page.ContentBox);
      //        }
 

      //        newpage.Children.Add(background); // Scale down page and center
        
      //            ContainerVisual smallerPage = new ContainerVisual();
      //            smallerPage.Children.Add(page.Visual);
      //            smallerPage.Transform = new MatrixTransform(0.95, 0, 0, 0.95, 

      //                0.025 * page.ContentBox.Width, 0.025 * page.ContentBox.Height);
 

      //        newpage.Children.Add(smallerPage);
      //        newpage.Children.Add(title);
 

      //        newpage.Transform = new TranslateTransform(m_Margin.Width, m_Margin.Height);
 

      //        return new DocumentPage(newpage, m_PageSize, Move(page.BleedBox), Move(page.ContentBox));
      //    }
 

      //    public override bool IsPageCountValid 
      //    { 
      //        get
      //        {
      //            return m_Paginator.IsPageCountValid;
      //        } 
      //    }
 

      //    public override int PageCount 
      //    { 
      //        get
      //        {
      //            return m_Paginator.PageCount;
      //        } 
      //    }
 

      //    public override Size PageSize
      //    {
      //        get
      //        {
      //            return m_Paginator.PageSize;
      //        }
        
      //        set
      //        {
      //            m_Paginator.PageSize = value;
      //        }
      //    }
 

      //    public override IDocumentPaginatorSource Source 
      //    { 
      //        get 
      //        {
      //            return m_Paginator.Source;
      //        }
      //    }
      //}

      public void Initialize(ServiceController controller)
      {
         // nothing to do
      }
   }
}
