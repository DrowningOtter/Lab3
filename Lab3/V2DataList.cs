using Lab3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Lab3
{
    public class V2DataList : V2Data
    {
        public delegate DataItem FDI(double x);
        public List<DataItem> DataList { get; set; }
        public V2DataList(string key, DateTime date) : base(key, date)
        {
            DataList = new List<DataItem>();
        }
        public V2DataList(string key, DateTime date, double[] x, FDI F) : this(key, date)
        {
            for (int i = 0; i < x.Length; i++)
            {
                int found = DataList.FindIndex(item => item.x == x[i]);
                if (found == -1)
                {
                    DataList.Add(F(x[i]));
                }
                else
                {
                    DataList[found] = F(x[i]);
                }
            }
        }
        public override double MinField
        {
            get
            {
                double min_value = double.MaxValue;
                for (int i = 0; i < DataList.Count; i++)
                {
                    min_value = Math.Min(min_value, Math.Min(Math.Abs(DataList[i].fields[0]), Math.Abs(DataList[i].fields[1])));
                }
                return min_value;
            }
        }
        public V2DataArray GetArray
        {
            get
            {
                V2DataArray ret = new V2DataArray(Key, Date);
                ret.Net = new double[DataList.Count];
                ret.FieldsInNet = new double[2, DataList.Count];
                for (int i = 0; i < DataList.Count; ++i)
                {
                    var cur = DataList[i];
                    ret.Net[i] = cur.x;
                    ret.FieldsInNet[0, i] = cur.fields[0];
                    ret.FieldsInNet[1, i] = cur.fields[1];
                }
                return ret;
            }
        }
        public override string ToString()
        {
            return $"V2DataList {base.ToString()} {DataList.Count}";
        }
        public override string ToLongString(string format)
        {
            var output = new StringBuilder();
            output.Append(ToString() + "\n");
            foreach (DataItem item in DataList)
            {
                output.Append($"{item.ToLongString(format)}\n");
            }
            return output.ToString();
        }

        public override IEnumerator<DataItem> GetEnumerator() => DataList.GetEnumerator();
    }
}
