using Lab3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace Lab3
{
    public static class Programm
    {
        public static void func1(double x, ref double y1, ref double y2)
        {
            y1 = x * x;
            y2 = x * 2;
            if (x > 10)
            {
                y1 = 0;
                y2 = 0;
            }
        }
        public static DataItem func2(double x) => new DataItem(x, x * x, x * 2);
        public static DataItem func3(double x)
        {
            if (x > 10) return new DataItem(x, 0, 0);
            return new DataItem(x, x * 13, x * x * x);
        }
        private static void Main()
        {
            //TestSavingToFile();
            //TestMainCollection();
            //V2DataArray tmp_arr = new V2DataArray("key", DateTime.Today);
            //SplineData tmp = new SplineData(tmp_arr, 10, 100);
            //tmp.CalcSpline();
            SplineData.CalcSpline();
        }
        private static void TestSavingToFile()
        {
            var x = new double[] { 4, 5, 9, 7, 23 };
            var newArr = new V2DataArray("key", DateTime.Today, x, func1);
            Console.WriteLine(newArr.ToLongString("f3"));
            V2DataArray.Save("array.json", newArr);
            V2DataArray data = new V2DataArray("key1", new DateTime());
            V2DataArray.Load("array.json", ref data);
            Console.WriteLine(data.ToLongString("f3"));
            Console.WriteLine("\n\n\n");
        }
        private static void TestMainCollection()
        {
            var newCol = new V2MainCollection(0, 0);
            //newCol.Add(new V2DataList("key1", DateTime.Today, new double[] { 8, 5, 5, 12, 7, 4 }, func3));
            newCol.Add(new V2DataArray("key2", DateTime.Today, new double[] { 13, 5, 3, 2, 14, 4 }, func1));
            //newCol.Add(new V2DataList("key3", DateTime.Today));
            newCol.Add(new V2DataArray("key4", DateTime.Today));

            Console.WriteLine(newCol.ToLongString("f3"));
            Console.WriteLine("Checking MaxZeroNumber property...\n" + newCol.MaxZeroNumber + "\n");
            Console.WriteLine("Checking MaxModule property...\n" + newCol.MaxModule + "\n");
            Console.WriteLine("Checking GetAllxAscending property...");
            foreach (var item in newCol.GetAllxAscending) Console.WriteLine(item);
        }
        private static void Question1()
        {
            var iso8601String = "20190501T08:30:52Z";
            DateTime date = DateTime.ParseExact(iso8601String, "yyyyMMddTHH:mm:ssZ",
                                            System.Globalization.CultureInfo.InvariantCulture);
            V2DataArray obj = new V2DataArray("question_1", date, 5, 6, 12, func1);
            string format = "F3";
            Console.WriteLine(obj.ToLongString(format));
            V2DataList new_list = (V2DataList)obj;
            Console.WriteLine(new_list.ToLongString(format));
        }
        private static void Question2()
        {
            var iso8601String = "20190501T08:30:52Z";
            DateTime date = DateTime.ParseExact(iso8601String, "yyyyMMddTHH:mm:ssZ",
                                            System.Globalization.CultureInfo.InvariantCulture);
            double[] x = new double[] { 1, 4, 6, 9, 10, 12, 15, 16, 18, 20 };
            V2DataList list = new V2DataList("question_2", date, x, func2);
            string format = "F3";
            Console.WriteLine(list.GetArray.ToLongString(format));
        }
        private static void Question3_4()
        {
            V2MainCollection col = new V2MainCollection(3, 3);
            string format = "F3";
            Console.WriteLine(col.ToLongString(format));
            foreach (var elem in col)
            {
                Console.WriteLine(elem.MinField);
            }
        }
    }
}
