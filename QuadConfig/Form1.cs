using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZedGraph;
using QuadroLib;
using System.Diagnostics;

namespace QuadConfig {
    public partial class Form1 : Form {

        // Starting time in milliseconds
        int tickStart = 0;

        PointPairList accXlist = new PointPairList();
        PointPairList accYlist = new PointPairList();
        PointPairList accZlist = new PointPairList();

        private int ox, oy, oz;

        RazorAhrs ahrs = new RazorAhrs("com12", true);

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            ahrs.Initialize(true);
            ahrs.ApplyOffsets = true;

            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Beschleunigungssensoren";
            myPane.XAxis.Title.Text = "Time";
            myPane.YAxis.Title.Text = "Force";

            // Save 1200 points.  At 50 ms sample rate, this is one minute
            // The RollingPointPairList is an efficient storage class that always
            // keeps a rolling set of point data without needing to shift any data values
            RollingPointPairList list = new RollingPointPairList(600);
            RollingPointPairList ylist = new RollingPointPairList(600);
            RollingPointPairList zlist = new RollingPointPairList(600);

            // Initially, a curve is added with no data points (list is empty)
            // Color is blue, and there will be no symbols
            LineItem curve = myPane.AddCurve("X Force", list, Color.Blue, SymbolType.None);
            curve.Line.IsSmooth = true;
            curve.Line.IsAntiAlias = true;

            LineItem ycurve = myPane.AddCurve("Y Force", ylist, Color.Red, SymbolType.None);
            ycurve.Line.IsSmooth = true;
            ycurve.Line.IsAntiAlias = true;

            LineItem zcurve = myPane.AddCurve("Z Force", zlist, Color.Green, SymbolType.None);
            zcurve.Line.IsSmooth = true;
            zcurve.Line.IsAntiAlias = true;

            // Sample at 50ms intervals
            timer1.Interval = 50;
            timer1.Enabled = true;
            timer1.Start();

            // Just manually control the X axis range so it scrolls continuously
            // instead of discrete step-sized jumps
            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;

            // Scale the axes
            zedGraphControl1.AxisChange();

            // Save the beginning time for reference
            tickStart = Environment.TickCount;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            // Make sure that the curvelist has at least one curve
            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            // Get the first CurveItem in the graph
            LineItem xcurve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;

            LineItem ycurve = zedGraphControl1.GraphPane.CurveList[1] as LineItem;

            LineItem zcurve = zedGraphControl1.GraphPane.CurveList[2] as LineItem;

            if (xcurve == null || ycurve == null || zcurve == null || !ahrs.HasValidData)
                return;

            // Get the PointPairList
            IPointListEdit xlist = xcurve.Points as IPointListEdit;
            IPointListEdit ylist = ycurve.Points as IPointListEdit;
            IPointListEdit zlist = zcurve.Points as IPointListEdit;
            // If this is null, it means the reference at curve.Points does not
            // support IPointListEdit, so we won't be able to modify it
            if (xlist == null || ylist == null || zlist == null)
                return;

            // Time is measured in seconds
            double time = (Environment.TickCount - tickStart) / 200.0;

            // 3 seconds per cycle
            ahrs.ParseData();
            xlist.Add(time, ahrs.AccX - ox);
            ylist.Add(time, ahrs.AccY - oy);
            zlist.Add(time, ahrs.AccZ - oz);

            // Keep the X scale at a rolling 30 second interval, with one
            // major step between the max X value and the end of the axis
            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            if (time > xScale.Max - xScale.MajorStep) {
                xScale.Max = time + xScale.MajorStep;
                xScale.Min = xScale.Max - 30.0;
            }

            // Make sure the Y axis is rescaled to accommodate actual data
            zedGraphControl1.AxisChange();
            // Force a redraw
            zedGraphControl1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e) {
            ox = ahrs.AccX;
            oy = ahrs.AccY;
            oz = ahrs.AccZ;
        }
    }
}
