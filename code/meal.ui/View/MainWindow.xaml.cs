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
using Sellars.Meal.Svc.Model;

namespace Sellars.Meal.UI.View
{
   /// <summary>
   /// Interaction logic for Window1.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         //Closed += new EventHandler(Window1_Closed);
//         Loaded += new RoutedEventHandler(Window1_Loaded);
      }

      //void Window1_Closed(object sender, EventArgs e)
      //{
      //   var provider = new Sellars.Meal.DataProvider.MockRecipeDataProvider ();
      //   var viewModel = ((Sellars.Meal.UI.ViewModel.RecipeViewModel) DataContext);
      //   var recipe = viewModel.Recipe;
      //   provider.SaveRecipe (
      //      viewModel.FileName ?? "junk.recipe",
      //      recipe);
      //}

      //void Window1_Loaded(object sender, RoutedEventArgs e)
      //{
      //   for (int i = 0; i < 20; i++)
      //   {
      //      double y = Height * i / 20;
      //      Line line = new Line (){X1 = 0, X2 = Width, Y1 = y, Y2 = y, SnapsToDevicePixels = true, Stroke = Brushes.SteelBlue, StrokeThickness=1,};
      //      grdMain.Children.Add (line);
      //   }
      //}
   }
}
