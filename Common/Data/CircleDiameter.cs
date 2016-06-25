using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    public class CircleDiameter
    {
        public CircleDiameter() { }

        public CircleDiameter(string code, double diameter)
        {
            this.EnterpriseCode = code;
            this.Diameter = diameter;
        }

        public string EnterpriseCode { get; set; }
        public double Diameter { get; set; }
    }
}
