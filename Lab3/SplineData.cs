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
        public double[] SplineTrue { get; set; }
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
        }
        public static void Func(double x, ref double y1, ref double y2)
        {
            y1 = 2 * x;
            y2 = 3 * x;
        }
        public static void CalcSpline()
        {
            Random random = new Random();
            int len = 5;
            double[] x = new double[len];
            for (int i = 0; i < x.Length; i++) { x[i] = (int)(random.NextDouble() * 10); }
            Array.Sort(x);
            Console.WriteLine("input values:");
            for (int i = 0; i < x.Length; ++i) Console.Write(x[i] + " ");
            Console.WriteLine();
            V2DataArray arr = new V2DataArray("key", DateTime.Today, x, Func);
            double[] splineValues = new double[len * 2];
            spline(len, arr.Net, arr[0].Length, arr[0], 3, splineValues);
            foreach (double item in  splineValues) { Console.Write(item + " "); }
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

        [DllImport("C:\\Users\\Artem\\source\\repos\\Lab3\\Build\\x64\\Debug\\dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void spline(int nX, double[] X, int nY, double[] Y, int m, double[] splineValues);

    }
}