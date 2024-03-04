using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class SplineDataItem
    {
        public double netValue { get; set; }
        public double exactValue { get; set; }
        public double calculatedValue { get; set; }

        public SplineDataItem(double netValue, double exactValue, double calculatedValue)
        {
            this.netValue = netValue;
            this.exactValue = exactValue;
            this.calculatedValue = calculatedValue;
        }
        public string ToString(string format)
        {
            return $"{netValue.ToString(format)}, {exactValue.ToString(format)}, {calculatedValue.ToString(format)}";
        }
        public override string ToString()
        {
            return $"{netValue.ToString()}, {exactValue.ToString()}, {calculatedValue.ToString()}";
        }
    }
}
