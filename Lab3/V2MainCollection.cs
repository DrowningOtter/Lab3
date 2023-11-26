using Lab3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class V2MainCollection : System.Collections.ObjectModel.ObservableCollection<V2Data>
    {
        public bool Contains(string key)
        {
            return Items.Any(elem => elem.Key == key);
        }
        public new bool Add(V2Data v2Data)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Key == v2Data.Key) return false;
            }
            base.Add(v2Data);
            return true;
        }
        public V2MainCollection(int nV2DataArray, int nv2DataList)
        {
            Random rnd = new Random();
            string key = "key";
            DateTime date = DateTime.Today;
            for (int i = 0; i < nV2DataArray; i++)
            {
                int leng = rnd.Next(1, 10);
                double[] x = new double[leng];
                for (int j = 0; j < leng; ++j)
                {
                    x[j] = rnd.NextDouble();
                }
                Items.Add(new V2DataArray(key, date, x, Programm.func1));
            }
            for (int i = 0; i < nv2DataList; i++)
            {
                int leng = rnd.Next(1, 10);
                double[] x = new double[leng];
                for (int j = 0; j < leng; ++j)
                {
                    x[j] = rnd.NextDouble();
                }
                Items.Add(new V2DataList(key, date, x, Programm.func2));
            }
        }
        public string ToLongString(string format)
        {
            var output = new StringBuilder();
            for (int i = 0; i < Items.Count; ++i)
            {
                output.Append($"{Items[i].ToLongString(format)}\n");
            }
            return $"{output}";
        }
        public override string ToString()
        {
            var output = new StringBuilder();
            for (int i = 0; i < Items.Count; ++i)
            {
                output.Append($"{Items[i].ToString()}\n");
            }
            return output.ToString();
        }
        public int MaxZeroNumber
        {
            get
            {
                if (Items.Count == 0) return -1;
                var ans = (from collection in Items
                           select collection.Count(item => Math.Pow(Math.Pow(item.fields[0], 2) + Math.Pow(item.fields[1], 2), 0.5) == 0)).Max();
                return ans;
            }
        }
        public DataItem? MaxModule
        {
            get
            {
                if (Items.Count == 0) return null;
                return (from collection in Items
                        from item in collection
                        select item).OrderBy(p => Math.Pow(Math.Pow(p.fields[0], 2) + Math.Pow(p.fields[1], 2), 0.5)).Last();
            }
        }
        public IEnumerable<double>? GetAllxAscending
        {
            get
            {
                if (Items.Count == 0) return null;
                return (from collection in Items
                        from item in collection
                        orderby item.x
                        select item.x).Distinct();
            }
        }
    }
}
