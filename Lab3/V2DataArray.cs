using Lab3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Lab3
{
    public delegate void FValues(double x, ref double y1, ref double y2);
    public class V2DataArray : V2Data
    {
        public double[] Net { get; set; }
        public double[,] FieldsInNet { get; set; }
        public V2DataArray(string key, DateTime date) : base(key, date)
        {
            Net = new double[0];
            FieldsInNet = new double[2, Net.Length];
        }
        public V2DataArray(string key, DateTime date, double[] x, FValues F) : base(key, date)
        {
            Net = (double[])x.Clone();
            FieldsInNet = new double[2, Net.Length];
            for (int i = 0; i < Net.Length; ++i)
            {
                F(Net[i], ref FieldsInNet[0, i], ref FieldsInNet[1, i]);
            }
        }
        public V2DataArray(string key, DateTime date, int nX, double xL, double xR, FValues F) : base(key, date)
        {
            Net = new double[nX];
            FieldsInNet = new double[2, Net.Length];

            double step = (double)(xR - xL) / (nX - 1);
            for (int i = 0; i < nX; ++i)
            {
                Net[i] = xL + step * i;
                F(Net[i], ref FieldsInNet[0, i], ref FieldsInNet[1, i]);
            }
        }
        public double[] this[int index]
        {
            get
            {
                if (index != 0 && index != 1) throw new ArgumentOutOfRangeException("Dimension number must be 0 or 1.");
                double[] ret = new double[FieldsInNet.GetLength(1)];
                for (int i = 0; i < ret.Length; ++i)
                {
                    ret[i] = (double)FieldsInNet[index, i];
                }
                return ret;
            }
        }
        public static explicit operator V2DataList(V2DataArray source)
        {
            V2DataList ret_list = new V2DataList(source.Key, source.Date);
            for (int i = 0; i < source.Net.Length; ++i)
            {
                DataItem cur = new DataItem(source.Net[i], source.FieldsInNet[0, i], source.FieldsInNet[1, i]);
                ret_list.DataList.Add(cur);
            }
            return ret_list;
        }
        public override double MinField
        {
            get
            {
                double min_value = double.MaxValue;
                for (int col_no = 0; col_no < FieldsInNet.GetLength(0); ++col_no)
                {
                    for (int str_no = 0; str_no < FieldsInNet.GetLength(1); ++str_no)
                    {
                        min_value = Math.Min(min_value, Math.Abs(FieldsInNet[col_no, str_no]));
                    }
                }
                return min_value;
            }
        }
        public override string ToString()
        {
            return $"V2DataArray {base.ToString()}";
        }
        public override string ToLongString(string format)
        {
            var output = new StringBuilder();
            output.Append(ToString() + "\n");
            for (int i = 0; i < Net.Length && i < FieldsInNet.GetLength(1); ++i)
            {
                output.Append($"{Net[i].ToString(format)} {FieldsInNet[0, i].ToString(format)} {FieldsInNet[1, i].ToString(format)}\n");
            }
            return output.ToString();
        }
        public override IEnumerator<DataItem> GetEnumerator()
        {
            for (int i = 0; i < Net.Length; ++i)
            {
                yield return new DataItem(Net[i], FieldsInNet[0, i], FieldsInNet[1, i]);
            }
        }
        public static bool Save(string filename, in V2DataArray data)
        {
            try
            {
                using (StreamWriter fs = new StreamWriter(filename))
                {
                    fs.WriteLine(JsonSerializer.Serialize(data.Key));
                    fs.WriteLine(JsonSerializer.Serialize(data.Date));
                    fs.WriteLine(JsonSerializer.Serialize(data.Net));
                    fs.WriteLine(JsonSerializer.Serialize(data[0]));
                    fs.WriteLine(JsonSerializer.Serialize(data[1]));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
        public static bool Load(string filename, ref V2DataArray data)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    data.Key = JsonSerializer.Deserialize<string>(sr.ReadLine());
                    data.Date = JsonSerializer.Deserialize<DateTime>(sr.ReadLine());
                    data.Net = JsonSerializer.Deserialize<double[]>(sr.ReadLine());
                    double[] fieldsInNet0 = (double[])JsonSerializer.Deserialize<double[]>(sr.ReadLine());
                    double[] fieldsInNet1 = (double[])JsonSerializer.Deserialize<double[]>(sr.ReadLine());
                    data.FieldsInNet = new double[2, fieldsInNet0.Length];
                    for (int i = 0; i < fieldsInNet0.Length; ++i)
                    {
                        data.FieldsInNet[0, i] = fieldsInNet0[i];
                        data.FieldsInNet[1, i] = fieldsInNet1[i];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return true;
        }
    }
}
