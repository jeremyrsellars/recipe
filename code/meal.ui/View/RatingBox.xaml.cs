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
   /// Interaction logic for Rating.xaml
   /// </summary>
   public partial class RatingBox : UserControl
   {
      public static readonly DependencyProperty RatingProperty = DependencyProperty.Register ("Rating", typeof(double), typeof (RatingBox), new PropertyMetadata ((double)2.5, RatingChanged), ValidateRatingValue);
      public static readonly DependencyProperty MaxRatingProperty = DependencyProperty.Register ("MaxRating", typeof(int), typeof (RatingBox), new PropertyMetadata (5, MaxRatingChanged), ValidateMaxRatingValue);
      public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register ("Glyph", typeof(char), typeof (RatingBox), new PropertyMetadata ((char)0x98, GlyphChanged), ValidateGlyphValue);

      public RatingBox()
      {
         m_glyph = (char)0x98;
         m_maxRating = 5;
         m_rating = 2.5;
         InitializeComponent();

         Reset (true);
      }

      public double Rating
      {
         get
         {
            return m_rating;
         }
         set
         {
            if (value < 0)
               throw new ArgumentOutOfRangeException("Rating must be at least 0.");
            if (value == m_maxRating)
               return;
            m_rating = value;
            Reset (false);
            SetValue (RatingProperty, m_rating);
         }
      }

      private static bool ValidateRatingValue (object value)
      {
         double rating = (double) value;
         return rating >= 0;
      }

      private static void RatingChanged (DependencyObject sender, DependencyPropertyChangedEventArgs ea)
      {
         RatingBox rb = sender as RatingBox;
         if (rb == null)
            return;

         rb.Rating = (double)ea.NewValue;
      }

      public int MaxRating
      {
         get
         {
            return m_maxRating;
         }
         set
         {
            if (value < 1)
               throw new ArgumentOutOfRangeException("MaxRating be at least 1.");
            if (value == m_maxRating)
               return;
            m_maxRating = value;
            Reset (true);
         }
      }

      private static bool ValidateMaxRatingValue (object value)
      {
         int rating = (int) value;
         return rating > 0;
      }

      private static void MaxRatingChanged (DependencyObject sender, DependencyPropertyChangedEventArgs ea)
      {
         RatingBox rb = sender as RatingBox;
         if (rb == null)
            return;

         rb.MaxRating = (int)ea.NewValue;
      }

      public char Glyph
      {
         get
         {
            return m_glyph;
         }
         set
         {
            if (m_glyph == value)
               return;
            m_glyph = value;
            Reset (true);
         }
      }

      private static bool ValidateGlyphValue (object value)
      {
         char c = (char) value;
         return c > 0;
      }

      private static void GlyphChanged (DependencyObject sender, DependencyPropertyChangedEventArgs ea)
      {
         RatingBox rb = sender as RatingBox;
         if (rb == null)
            return;

         rb.Glyph = (char)ea.NewValue;
      }

      private void Reset (bool max)
      {
         double rating = m_hoverRating ?? Rating;
         if (max)
         {
            string s = new string (m_glyph, MaxRating);
            txtMaxRating.Text = s;
         }
            txtRating.Text = new string (m_glyph, (int)Math.Ceiling(rating) + 1);
         var size = txtMaxRating.RenderSize;
         size.Width = size.Width * rating / MaxRating;
         txtRating.RenderSize = size;//Width = txtMaxRating.Width * rating / MaxRating;
      }

      private void OnMouseDown(object sender, MouseButtonEventArgs e)
      {
         Point position = e.GetPosition (txtMaxRating);
         if (position.Y < 0 || position.Y > txtMaxRating.ActualHeight)
         {
            m_hoverRating = 0;
            Reset (false);
            return;
         }
         double x = position.X;
         double maxX = txtMaxRating.ActualWidth;
         
         Rating = GetRatingAt (x, maxX);
         m_hoverRating = null;
         Reset (false);
      }

      private void OnMouseMove(object sender, MouseEventArgs e)
      {
         Point position = e.GetPosition (txtMaxRating);
         if (position.Y < 0 || position.Y > txtMaxRating.ActualHeight)
         {
            m_hoverRating = 0;
            Reset (false);
            return;
         }
         double x = position.X;
         double maxX = txtMaxRating.ActualWidth;
         
         m_hoverRating = GetRatingAt (x, maxX);
         Reset (false);
      }

      private void OnMouseUp(object sender, MouseButtonEventArgs e)
      {
         Point position = e.GetPosition (txtMaxRating);
         if (position.Y < 0 || position.Y > txtMaxRating.ActualHeight)
         {
            m_hoverRating = 0;
            Reset (false);
            return;
         }
         double x = position.X;
         double maxX = txtMaxRating.ActualWidth;
         
         Rating = GetRatingAt (x, maxX);
         m_hoverRating = null;
         Reset (false);
      }

      private void OnMouseLeave(object sender, MouseEventArgs e)
      {
         m_hoverRating = null;
         Reset (false);
      }

      private double GetRatingAt (double x, double maxX)
      {
         int thirds = (int) (3 * MaxRating * x / maxX);
         double rating = 0;
         while (thirds >= 3)
         {
            thirds -= 3;
            rating++;
         }
         switch (thirds)
         {
         case 0:
            return rating;
         case 1:
            return rating + 1/2;
         case 2:
            return rating + 1;
         default: // Shouldn't happen
            return rating;
         }
      }

      private int m_maxRating;
      private double m_rating;
      private double? m_hoverRating;
      private char m_glyph;

   }
}
