using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class DataItemS
    {
        public double NodeCoord { get; set; }
        public double SplineValue { get; set; }
        public DataItemS(double NodeCoord, double SplineValue)
        {
            this.NodeCoord = NodeCoord;
            this.SplineValue = SplineValue;
        }
        public override string ToString()
        {
            return $"Node:{NodeCoord}, spline value:{SplineValue}";
        }
    }
}
