using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DataHelper.FuncSet.Kd
{
    public class Kd
    {
        public static ConcurrentDictionary<int, double> Func(KFunc kfunc, List<double> table)
        {
            ConcurrentDictionary<int, double> results = new ConcurrentDictionary<int, double>();
            try
            {
                double factor = kfunc.Factor;

                Parallel.For(0, (int)(kfunc.Di + 1), (d, state) => 
                {
                    double result = 0.0;
                    for (int k = 0; k < table.Count; k++)
                    {
                        double temp = (d - table[k]) / kfunc.h;
                        result += Math.Pow(Math.E, -0.5 * temp * temp);
                    }
                    results.TryAdd(d, factor * result);
                });
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kd.Func:" + ex.Message);
                return null;
            }
        }
    }

    public class KFunc
    {
        public int n { get; set; }
        public double R { get; set; }
        public double h { get; set; }
        public double Factor { get; set; }
        // 1/2距离值
        public double Di { get; set; }

        public KFunc(int n, double r, double d)
        {
            this.Di = d;
            this.n = n;
            this.R = r;
            this.h = 0.79 * Math.Pow(n, -0.2);
            this.Factor = 1 / ((n - 1) * n * h * Math.Sqrt(2 * Math.PI));
        }
    }
}
