using Lab3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public abstract class V2Data : IEnumerable<DataItem>
    {
        public string Key { get; set; }
        public DateTime Date { get; set; }
        public V2Data(string key, DateTime date)
        {
            this.Key = key;
            this.Date = date;
        }
        public abstract double MinField { get; }
        public abstract string ToLongString(string format);
        public override string ToString()
        {
            return $"{Key} {Date}";
        }

        public abstract IEnumerator<DataItem> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
