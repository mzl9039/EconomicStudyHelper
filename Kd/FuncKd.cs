using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace Kd
{
    public class Kd
    {
        public static List<double> Func(int n, double r, List<double> table)
        {
            try
            {
                List<double> results = new List<double>();
                const double R = r;
                //const double A = R / 1.34;
                //double h = 0.9 * A * Math.Pow(n, -0.2);
                // 调整公式，换 h 的计算方式
                double h = 0.79 * R * Math.Pow(n, -0.2);
                double factor = 1 / ((n - 1) * n * h * Math.Sqrt(2 * Math.PI));

//                double result = 0.0;
//                for (int d = 0; d <= 647; d++)
//                {
//                    result = 0.0;
//                    for (int k = 0; k < table.Count; k++)
//                    {
//                        double temp = (d - table[k]) / h;
//                        result += Math.Pow(Math.E, -0.5 * temp * temp);
//                    }
//                    results.Add(factor * result);
//                }
                object lockflg = new object();
                Parallel.For<double>(0, 648, ()=> 0, 
	                 (i, loop, subTemp) => {										
						for (int j = 0; j < table.Count; j++) {
							double temp = (i - table[j]) / h;
							subTemp += Math.Pow(Math.E, -0.5 * temp * temp);
						}
	                 	return subTemp * factor;
	                 }, (x) => {
	                 	lock(lockflg){
	                 		results.Add(x);
	                 	}
	                 })
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kd.Func:" + ex.Message);
                return null ;
            }
        }
    }

    public class KFunc
    {
        int n { get; set; }
        double R { get; set; }
        double A { get; set; }
        public double h { get; set; }
        public double Factor { get; set; }

        public KFunc(int n, double r)
        {
            this.n = n;
            this.R = r;
            this.A = R / 1.34;
            this.h = 0.9 * A * Math.Pow(n, -0.2);
            this.Factor = 1 / ((n - 1) * n * h * Math.Sqrt(2 * Math.PI));
        }
    }
}
