using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3
{
    public class SplineDataItem
    {
        public double xCoord {  get; set; }
        public double yTrue { get; set; }
        public double yPred { get; set; }

        public SplineDataItem(double xCoord, double yTrue, double yPred)
        {
            this.xCoord = xCoord;
            this.yTrue = yTrue;
            this.yPred = yPred;
        }
        public string ToString(string format)
        {
            return xCoord.ToString(format) + ", " +
                yTrue.ToString(format) + ", " +
                yPred.ToString(format);
        }
        public override string ToString()
        {
            return xCoord.ToString() + ", " + yTrue.ToString() + ", " + yPred.ToString();
        }
    }
}
