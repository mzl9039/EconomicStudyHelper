using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class KdMaxDistance
    {
        public KdMaxDistance(string ec, double m)
        {
            this.ec = ec;
            this.max = m;
        }
        // 行业代码
        public string ec { get; set; }
        // 该行业的最大距离
        public double max { get; set; }
    }
}
