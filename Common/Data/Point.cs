using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Point
    {
        public Point(double x, double y, int zone = 0)
        {
            this.X = x;
            this.Y = y;
            this.Zone = zone;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public int Zone { get; set; }
    }
}
