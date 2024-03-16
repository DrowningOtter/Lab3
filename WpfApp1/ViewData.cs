﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Lab3;
using OxyPlot;

namespace WpfApp1
{
    public class BorderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string? left = values[0].ToString();
                string? right = values[1].ToString();
                return left + " ; " + right;
            }
            catch {
                return " ; ";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            try
            {
                string? str = value as string;
                string[] strs = str.Split(";", StringSplitOptions.RemoveEmptyEntries);
                return new object[] { double.Parse(strs[0]), double.Parse(strs[1]) };
            }
            catch {
                //ViewData.PrintError(nameof(ConvertBack));
                // TODO  в этом месте проверяем корректность
                //return new object[2];
                return new object[] { (double)0, (double)5 };
            }
        }
    }

    public class ViewData : INotifyPropertyChanged, IDataErrorInfo
    {
        public V2DataArray dataArray { get; set; }
        public SplineData splineData { get; set; }
        public MyPlot plotModel { get; set; }

        // Ввод параметров для DataArray
        public double _left_border;
        public double left_border {
            get { return _left_border; }
            set
            {
                if (_left_border != value)
                {
                    _left_border = value;
                    OnPropertyChanged(nameof(left_border));
                }
            }
        }
        public double _right_border;
        public double right_border {
            get { return _right_border; }
            set
            {
                if (_right_border != value)
                {
                    _right_border = value;
                    OnPropertyChanged(nameof(right_border));
                }
            }
        }
        public int _arrayNodeAmount;
        public int arrayNodesAmount {
            get { return _arrayNodeAmount; }
            set
            {
                if (_arrayNodeAmount != value)
                {
                    _arrayNodeAmount = value;
                    OnPropertyChanged(nameof(arrayNodesAmount));
                }
            }
        }
        public bool isUniform { get; set; }
        public bool isNonUniform { get; set; }

        // Ввод параметров для SplineData
        public int splineNodesAmount { get; set; }
        public int splineCalculationNodesAmount { get; set; }
        public double normResidual { get; set; }
        public int maxIterations { get; set; }
        public int _actualNumberOfIterations;
        public int actualNumberOfIterations
        {
            get { return _actualNumberOfIterations; }
            set
            {
                if (_actualNumberOfIterations != value)
                {
                    _actualNumberOfIterations = value;
                    OnPropertyChanged(nameof(actualNumberOfIterations));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<FValues> listFValues { get; set; }
        public FValues fValues { get; set; }
        public ViewData()
        {
            left_border = 0;
            right_border = 5;
            arrayNodesAmount = 10;
            isUniform = true;
            isNonUniform = false;
            splineNodesAmount = 5;
            splineCalculationNodesAmount = 10;
            normResidual = 1e-6;
            maxIterations = 1000;
            fValues = linear;
            listFValues = new List<FValues>();
            listFValues.Add(linear);
            listFValues.Add(cubic);
            listFValues.Add(random);
        }
        public bool Save(string filename)
        {
            if (dataArray == null)
            {
                MessageBox.Show("DataArray haven't been filled. First, fill data.");
                return false;
            }
            try
            {
                V2DataArray.Save(filename, dataArray);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }
        // TODO доделать изменение всех полей в интерфейсе при загрузке из файла
        public void Load(string filename)
        {
            try
            {
                V2DataArray tmp = new V2DataArray("tmpkey", DateTime.Now);
                V2DataArray.Load(filename, ref tmp);
                dataArray = tmp;
                left_border = dataArray.Net[0];
                right_border = dataArray.Net[dataArray.Net.Length - 1];
                arrayNodesAmount = dataArray.Net.Length;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                throw;
            }
        }
        public void FillDataArray()
        {
            if (isUniform)
            {
                dataArray = new V2DataArray("key", DateTime.Now, arrayNodesAmount, left_border, right_border, fValues);
            }
            else if (isNonUniform)
            {
                double[] x = new double[arrayNodesAmount];
                x[0] = left_border;
                x[arrayNodesAmount - 1] = right_border;
                double min = left_border;
                Random random = new Random();
                for (int i = 1; i < arrayNodesAmount - 1; i++)
                {
                    x[i] = random.NextDouble() * (right_border - min) + min;
                    min = x[i];
                }
                dataArray = new V2DataArray("key", DateTime.Today, x, fValues);
            }
            else
            {
                MessageBox.Show("None of Uniform or NonUniform grid was choosen.");
            }
        }
        public void CalculateSpline()
        {
            splineData = new SplineData(dataArray, splineNodesAmount, maxIterations, splineCalculationNodesAmount);
            SplineData.CalcSpline(splineData, (double x) => 2 * x + 3);
            actualNumberOfIterations = splineData.ActualNumberOfIterations;
        }

        public string Error { get { return "Incorrect data."; } }
        public string this[string propertyName]
        {
            get
            {
                string? error = null;
                switch(propertyName)
                {
                    case "arrayNodesAmount":
                        if (arrayNodesAmount < 3)
                            error = "Число узлов сетки, на которой заданы дискретные значения функции, должно быть больше или равно 3";
                        break;
                    case "splineCalculationNodesAmount":
                        if (splineCalculationNodesAmount < 3)
                            error = "Число узлов равномерной сетки, на которой вычисляются значения сглаживающего сплайна, должно быть больше или равно 3";
                        break;
                    case "left_border":
                        if (left_border >= right_border)
                            error = "Левый конец отрезка должен быть меньше правого конца отрезка";
                        break;
                    case "right_border":
                        if (left_border >= right_border)
                            error = "Левый конец отрезка должен быть меньше правого конца отрезка";
                        break;
                    case "splineNodesAmount":
                        if (splineNodesAmount < 2 || splineNodesAmount > arrayNodesAmount)
                            error = "Число узлов сглаживающего сплайна должно быть больше или равно 2 и меньше или равно числу заданных дискретных значений функции";
                        break;
                    default:
                        break;
                }
                return error;
            }
        }

        public void linear(double x, ref double y1, ref double y2)
        {
            y1 = 4 * x + 15;
            y2 = 2 * x + 6;
        }
        public void cubic(double x, ref double y1, ref double y2)
        {
            y1 = x * x * x;
            y2 = x * x * x;
        }
        public void random(double x, ref double y1, ref double y2)
        {
            var rand = new Random();
            y1 = rand.Next();
            y2 = rand.Next();
        }
        
    }
}
