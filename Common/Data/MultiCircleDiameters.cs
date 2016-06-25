using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Data
{
    // 保存各个行业的直径大小，以及浓度值 [5/15/2016 16:36:53 mzl]
    public class MultiCircleDiameters
    {
        public MultiCircleDiameters()
        {

        }

        public MultiCircleDiameters(string code, List<double> diameters, double density)
        {
            EnterpriseCode = code;
            Diameters = diameters;
            Density = density;
        }

        // 行业代码+企业Id [5/15/2016 16:33:10 mzl]
        public string EnterpriseCode { get; set; }
        // 企业的直径，可能有多个 [5/15/2016 16:34:13 mzl]
        public List<double> Diameters { get; set; }
        // 浓度 [5/15/2016 16:35:05 mzl]
        public double Density { get; set; }
    }
}
