using Lab3;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewData viewData { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            viewData = new ViewData();
            this.DataContext = viewData;
        }

        private void SaveDataToFileClick(object sender, RoutedEventArgs e)
        {
            if (viewData.dataArray == null)
            {
                MessageBox.Show("DataArray haven't been filled. First, fill data.");
                return;
            }
            if (!ValidateFields()) return;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                //MessageBox.Show("\"" + filename + "\" filename selected.");
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
                    if (!ValidateFields()) return;
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
        private void LoadDataFromControlsClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields()) return;
            viewData.FillDataArray();
            viewData.CalculateSpline();
            //MessageBox.Show("Вычисления завершены");
            SplineOnNet.ItemsSource = viewData.splineData.calculatedSpline;
            SplineOnSmallerNet.ItemsSource = viewData.splineData.SmallNetSplineValues;
        }

        private bool ValidateFields()
        {
            if (!int.TryParse(nodeAmount.Text, out int nodeAmountInt) || nodeAmountInt <= 0)
            {
                ViewData.PrintError("nodeAmountInvalidValue");
                nodeAmount.Text = "10";
                return false;
            }
            else if (!int.TryParse(splineNodesAmount.Text, out int splineNodesAmountInt) || 
                splineNodesAmountInt <= 0 || splineNodesAmountInt >= nodeAmountInt)
            {
                ViewData.PrintError("splineNodesAmountInvalidValue");
                splineNodesAmount.Text = "5";
                return false;
            }
            else if (!int.TryParse(uniformGridNodesAmount.Text, out int uniformGridNodesAmountInt) || 
                uniformGridNodesAmountInt <= 0)
            {
                ViewData.PrintError("uniformGridNodesAmountInvalidValue");
                uniformGridNodesAmount.Text = "10";
                return false;
            }
            else if (!double.TryParse(residualNormToStop.Text, out double residualNormToStopDouble) || residualNormToStopDouble <= 0)
            {
                ViewData.PrintError("residualNormToStopInvalidValue");
                residualNormToStop.Text = "1E-06";
                return false;
            }
            else if (!int.TryParse(maxIterationsNum.Text, out int maxIterationsNumInt) || maxIterationsNumInt <= 0)
            {
                ViewData.PrintError("maxIterationsNumInvalidValue");
                maxIterationsNum.Text = "1000";
                return false;
            }
            return true;
        }
    }
}
