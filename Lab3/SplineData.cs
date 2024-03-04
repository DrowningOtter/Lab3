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
        public int m { get; set; }
        // Количество узлов равномерной сетки(узлы сплайна)
        public double[] calculatedSplineNonUniform { get; set; }
        // Значения сплайна в узлах сетки, на которой заданы значения поля
        public int MaxIterations { get; set; }
        public int StopReason { get; set; }
        public double MinResidual { get; set; }
        public List<SplineDataItem> calculatedSpline { get; set; }
        // Результаты сплайн-аппроксимации
        public int ActualNumberOfIterations { get; set; }

        public int SmallNetLength { get; set; }

        public List<DataItemS> SmallNetSplineValues { get; set; }

        public SplineData(V2DataArray Values, int m, int MaxIterations, int smallerNetSize)
        {
            this.Values = Values;
            this.m = m;
            this.SmallNetLength = smallerNetSize;
            this.MaxIterations = MaxIterations;
            this.calculatedSplineNonUniform = new double[this.Values.Net.Length];
            this.calculatedSpline = new List<SplineDataItem>();
            this.SmallNetSplineValues = new List<DataItemS>();
        }
        public static void Func(double x, ref double y1, ref double y2)
        {
            y1 = 5.5 * x * x;
            y2 = 3 * x;
        }
        public static void CalcSpline(SplineData data, Func<double, double> initial_approximation_func)
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
            double[] smaller_net_values = new double[data.SmallNetLength];
            double[] smaller_net_values_grid = linspace(data.Values.Net[0],
                data.Values.Net[data.Values.Net.Length - 1],
                data.SmallNetLength);

            SplineInterpolation(
                data.Values.Net.Length,
                data.Values.Net,
                data.Values[0].Length,
                data.Values[0],
                data.m,
                values_on_uniform_grid,
                data.calculatedSplineNonUniform,
                ref StopReasonLocal,
                data.MaxIterations,
                ref actual_iterations,
                smaller_net_values,
                smaller_net_values_grid,
                data.SmallNetLength);
            data.MinResidual = 0;
            for (int i = 0; i < data.calculatedSplineNonUniform.Length; ++i)
            {
                // Подсчет нормы невязки
                data.MinResidual += (data.calculatedSplineNonUniform[i] - data.Values[0][i]) * (data.calculatedSplineNonUniform[i] - data.Values[0][i]);
                // Заполнение листа результатов сплайн-аппроксимации
                data.calculatedSpline = data.calculatedSpline.Append(new SplineDataItem(data.Values.Net[i], data.Values[0][i], data.calculatedSplineNonUniform[i])).ToList();
            }
            for (int i = 0; i < smaller_net_values.Length; i++)
            {
                data.SmallNetSplineValues = data.SmallNetSplineValues.Append(new DataItemS(smaller_net_values_grid[i], smaller_net_values[i])).ToList();
            }
            data.MinResidual = Math.Sqrt(data.MinResidual);
            data.StopReason = StopReasonLocal;
            data.ActualNumberOfIterations = actual_iterations;
        }
        public string ToLongString(string format)
        {
            var output = new StringBuilder();
            output.Append(Values.ToLongString(format) + "\n");
            output.Append("Spline approximation results:\n");
            foreach (var item in this.calculatedSpline) output.AppendLine(item.ToString(format));
            output.AppendLine();
            output.Append("Minimal residual value: " + MinResidual.ToString() + "\n");
            output.Append("Stop reason: ");
            if (this.StopReason == 1) output.Append("specified number of iterations has been exceeded\n");
            else if (this.StopReason == 2) output.Append("specified trust region size has been reached\n");
            else if (this.StopReason == 3) output.Append("specified residual norm has been reached\n");
            else if (this.StopReason == 4) output.Append("the specified row norm of the Jacobian matrix has been reached\n");
            else if (this.StopReason == 5) output.Append("specified trial step size has been reached\n");
            else if (this.StopReason == 6) output.Append("the specified difference between the norm of the function and the error has been reached\n");
            output.Append("Actual number of iterations: " + ActualNumberOfIterations + "\n");
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
        static extern void SplineInterpolation(
            int nX,
            double[] X,
            int nY,
            double[] Y,
            int m,
            double[] values_on_uniform_grid,
            double[] splineValues,
            ref int stop_reason,
            int maxIterations,
            ref int ActualNumberOfIterations,
            double[] smaller_net_values,
            double[] smaller_net_values_grid,
            int smaller_net_values_length);
    }   
}