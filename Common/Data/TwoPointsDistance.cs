using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    /// <summary>
    /// 两点间距离的model
    /// </summary>
    public class TwoPointsDistance
    {
        // 第一个点的id号 [3/10/2016 Administrator]
        public string IdSrc { get; set; }
        // 第二个点的id号 [3/10/2016 Administrator]
        public string IdTar { get; set; }
        // 两点的距离 [3/10/2016 Administrator]
        public double Distance { get; set; }
        public TwoPointsDistance(string IdS, string IdT, double distance)
        {
            IdSrc = IdS;
            IdTar = IdT;
            Distance = distance;
        }
    }
}
