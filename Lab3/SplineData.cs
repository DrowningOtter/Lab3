using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class SplineData
    {
        public V2DataArray Values { get; set; }
        public int m {  get; set; }
        public double[] SplinePredNonUniform { get; set; }
        // Значения сплайна в узлах сетки, на которой заданы значения поля
        public int MaxIterations { get; set; }
        public int StopReason { get; set; }
        public double MinResidual { get; set; }
        public List<SplineDataItem> SplinePred { get; set; }
        // Результаты сплайн-аппроксимации
        public int ActualNumberOfIterations { get; set; }

        public SplineData(V2DataArray Values, int m, int MaxIterations)
        {
            this.Values = Values;
            this.m = m;
            this.MaxIterations = MaxIterations;
            this.SplinePredNonUniform = new double[this.Values.Net.Length];
            SplinePred = new List<SplineDataItem>();
        }
        public static void Func(double x, ref double y1, ref double y2)
        {
            y1 = 5.5 * x * x;
            y2 = 3 * x;
        }
        public static double initial_approximation_func(double x)
        {
            return x * x;
        }
        public static void CalcSpline(SplineData data)
        {
            int StopReasonLocal = 0;
            double[] values_on_uniform_grid = linspace(data.Values[0][0], 
                data.Values[0][data.Values[0].Length - 1], 
                data.m);
            for (int i = 0; i < values_on_uniform_grid.Length; ++i)
            {
                values_on_uniform_grid[i] = initial_approximation_func(values_on_uniform_grid[i]);
            }
            int actual_iterations = 0;
            spline(data.Values.Net.Length,
                data.Values.Net, 
                data.Values[0].Length, 
                data.Values[0], 
                data.m,
                values_on_uniform_grid,
                data.SplinePredNonUniform,
                ref StopReasonLocal,
                data.MaxIterations,
                ref actual_iterations);
            data.MinResidual = 0;
            for (int i = 0; i < data.SplinePredNonUniform.Length; ++i)
            {
                // Подсчет нормы невязки
                data.MinResidual += (data.SplinePredNonUniform[i] - data.Values[0][i]) * (data.SplinePredNonUniform[i] - data.Values[0][i]);
                // Заполнение листа результатов сплайн-аппроксимации
                data.SplinePred.Append(new SplineDataItem(data.Values.Net[i], data.Values[0][i], data.SplinePredNonUniform[i]));
            }
            data.MinResidual = Math.Sqrt(data.MinResidual);
            data.StopReason = StopReasonLocal;
            data.ActualNumberOfIterations = actual_iterations;
        }
        public string ToLongString(string format)
        {
            var output = new StringBuilder();
            output.Append("Initial grid: ");
            output.Append(Values.ToLongString(format) + "\n");
            foreach (var item in SplinePred)
            {
                output.Append(item.ToString() + "\n");
            }
            output.AppendLine();
            output.Append("Minimal residual value: " + MinResidual.ToString() + "\n");
            output.Append("Stop reason: " + StopReason.ToString() + "\n");
            output.Append("Actual number of iterations: " + ActualNumberOfIterations);
            return output.ToString();
        }
        public bool Save(string filename, string format)
        {
            try
            {
                using (StreamWriter fs = new StreamWriter(filename))
                {
                    fs.WriteLine(ToLongString(format));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static double[] linspace(double left_border, double right_border, int num_of_dots)
        {
            double step = (right_border - left_border) / (num_of_dots - 1);
            double[] ret = new double[num_of_dots];
            for (int i = 0; i < num_of_dots; i++)
            {
                ret[i] = left_border + step * i;
            }
            return ret;
        }

        [DllImport("C:\\Users\\Artem\\source\\repos\\Lab3\\Build\\x64\\Debug\\dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void spline(int nX, 
            double[] X,
            int nY,
            double[] Y,
            int m,
            double[] values_on_uniform_grid,
            double[] splineValues,
            ref int stop_reason,
            int maxIterations,
            ref int ActualNumberOfIterations);

    }
}