using Lab3;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        ViewData viewData = new ViewData();
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = viewData;
        }

        private void LoadDataFromFileClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    viewData.Load(filename);
                    MessageBox.Show("Data was loaded from \"" + filename + "\"");
                    //if (!ValidateFields()) return;
                    viewData.CalculateSpline();
                    SplineOnNet.ItemsSource = viewData.splineData.calculatedSpline;
                    SplineOnSmallerNet.ItemsSource = viewData.splineData.SmallNetSplineValues;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void CheckControls(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                bool canExecute = true;
                string? error = null;
                string[] fields = { "arrayNodesAmount", "splineCalculationNodesAmount", "left_border", "splineNodesAmount" };
                foreach (var field in fields)
                {
                    error = viewData[field];
                    canExecute = error == null && canExecute;
                    if (error != null)
                    {
                        //Console.WriteLine(error);
                    }
                }
                e.CanExecute = canExecute;
                //if(error != null)
                //{
                //    MessageBox.Show(error);
                //    error = null;
                //}
            }
            catch (Exception ex)
            {
                e.CanExecute = false;
            }
        }
        private void CalculateAndShowResult(object sender, ExecutedRoutedEventArgs e)
        {
            viewData.FillDataArray();
            viewData.CalculateSpline();
            SplineOnNet.ItemsSource = viewData.splineData.calculatedSpline;
            SplineOnSmallerNet.ItemsSource = viewData.splineData.SmallNetSplineValues;
            viewData.plotModel = new MyPlot(viewData.splineData);
            this.DataContext = null;
            this.DataContext = viewData;
        }
        private void CheckBeforeSave(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (viewData.dataArray == null) e.CanExecute = false;
                else { e.CanExecute = true;  }
            }
            catch (Exception ex)
            {
                e.CanExecute = false;
            }
        }
        private void SaveDataToFile(object sender, ExecutedRoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                try
                {
                    viewData.Save(filename);
                    MessageBox.Show("Your data saved as \"" + filename + "\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
