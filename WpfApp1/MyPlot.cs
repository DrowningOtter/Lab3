using Lab3;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace WpfApp1
{
    public class MyPlot
    {
        SplineData data {  get; set; }
        public PlotModel plotModel { get; private set; }
        public MyPlot(SplineData data)
        {
            this.data = data;
            this.plotModel = new PlotModel { Title = "Approximation results" };
            AddSeries();
        }
        private void AddSeries()
        {
            plotModel.Series.Clear();
            OxyColor color = OxyColors.Red;
            LineSeries lineSeries = new LineSeries();
            var points = data.Values.Net;
            var values = data.Values[0];
            for (int i = 0; i < points.Length;  i++)
            {
                lineSeries.Points.Add(new DataPoint(points[i], values[i]));
            }
            lineSeries.MarkerType = MarkerType.Diamond;
            lineSeries.Color = OxyColors.Transparent;
            lineSeries.MarkerSize = 5;
            lineSeries.MarkerFill = color;
            lineSeries.MarkerStroke = color;
            lineSeries.MarkerStrokeThickness = 2.3;
            lineSeries.Title = "Дискретные значения функции";
            Legend legend = new Legend { LegendPosition = LegendPosition.LeftTop, LegendPlacement = LegendPlacement.Inside};
            plotModel.Legends.Add(legend);
            plotModel.Series.Add(lineSeries);

            lineSeries = new LineSeries();
            var splinePoints = data.SmallNetSplineValues;
            foreach (var splinePoint in splinePoints)
                lineSeries.Points.Add(new DataPoint(splinePoint.NodeCoord, splinePoint.SplineValue));
            OxyColor splineColor = OxyColors.Blue;
            lineSeries.Color = splineColor;
            lineSeries.MarkerType = MarkerType.Circle;
            lineSeries.MarkerSize = 3;
            lineSeries.MarkerFill = splineColor;
            lineSeries.MarkerStroke = splineColor;
            lineSeries.Title = "Аппроксимирующий сплайн";

            legend = new Legend { LegendPosition = LegendPosition.LeftTop, LegendPlacement = LegendPlacement.Inside };
            plotModel.Legends.Add(legend);
            plotModel.Series.Add(lineSeries);
        }
    }
}
