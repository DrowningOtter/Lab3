using System.Runtime.CompilerServices;

namespace Lab3
{
    public struct DataItem
    {
        public double x { get; set; }
        public double[] fields { get; set; }

        public DataItem(double x, double y1, double y2)
        {
            this.x = x;
            this.fields = new double[2];
            this.fields[0] = y1;
            this.fields[1] = y2;
        }

        public string ToLongString(string format)
        {
            return $"{x.ToString(format)} {fields[0].ToString(format)} {fields[1].ToString(format)}";
        }
        public override string ToString()
        {
            return $"{x} {fields[0]} {fields[1]}";
        }
    }
}

