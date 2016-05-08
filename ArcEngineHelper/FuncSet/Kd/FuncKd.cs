using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Common;
using LogHelper;

namespace DataHelper.FuncSet.Kd
{
    public class Kd
    {
        public static ConcurrentDictionary<int, double> Func(KFunc kfunc, List<Enterprise> Enterprises)
        {
            ConcurrentDictionary<int, double> results = new ConcurrentDictionary<int, double>();
            try
            {
                double factor = 1 / ((kfunc.n - 1) * kfunc.n * kfunc.h * Math.Sqrt(2 * Math.PI));
                int EnterprisesCount = Enterprises.Count;

                Parallel.For(0, (int)(kfunc.Di + 1), (d, state) => 
                {
                    double result = 0.0;
                    for (int i = 0; i < EnterprisesCount; i++)
                    {
                        Enterprise eOut = Enterprises[i];
                        for (int j = i + 1; j < EnterprisesCount; j++)
                        {
                            Enterprise eIn = Enterprises[j];
                            double distance = Math.Sqrt((eOut.Point.Y - eIn.Point.Y) * (eOut.Point.Y - eIn.Point.Y) +
                                                        (eOut.Point.X - eIn.Point.X) * (eOut.Point.X - eIn.Point.X)) / 1000;

                            if (0 == distance)
                                continue;
                            else
                            {
                                double temp = (d - distance) / kfunc.h;
                                result += Math.Pow(Math.E, -0.5 * temp * temp);
                            }
                        }
                    }                    
                    results.TryAdd(d, factor * result);
                });
                return results;
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.Flatten().InnerExceptions)
                {
                    Log.WriteError(ex.ToString());
                }
                return null;
            }
        }

        /// <summary>
        ///   K（d）函数的计算考虑企业规模（即企业的总人数）,只计算真实值
        /// author:     by mzl 
        /// date:       2016.5.8
        /// </summary>
        public static ConcurrentDictionary<int, double> FuncScale(KFunc kfunc, List<Enterprise> Enterprises)
        {
            ConcurrentDictionary<int, double> results = new ConcurrentDictionary<int, double>();
            try
            {
                // 计算行业内，两两企业人数乘积的总和 [5/8/2016 mzl]
                double scale = 0.0;
                for (int i=0; i<Enterprises.Count;i++)
                {
                    for (int j=i+1;j<Enterprises.Count;j++)
                    {
                        scale += Enterprises[i].man * Enterprises[j].man;
                    }
                }
                double factor = 1 / (kfunc.h * Math.Sqrt(2 * Math.PI) * scale);

                int EnterprisesCount = Enterprises.Count;
                Parallel.For(0, (int)(kfunc.Di + 1), (d, state) =>
                {
                    double result = 0.0;
                    for (int i = 0; i < EnterprisesCount; i++)
                    {
                        Enterprise eOut = Enterprises[i];
                        for (int j = i + 1; j < EnterprisesCount; j++)
                        {
                            Enterprise eIn = Enterprises[j];
                            double distance = Math.Sqrt((eOut.Point.Y - eIn.Point.Y) * (eOut.Point.Y - eIn.Point.Y) +
                                                        (eOut.Point.X - eIn.Point.X) * (eOut.Point.X - eIn.Point.X)) / 1000;

                            if (0 == distance)
                                continue;
                            else
                            {
                                double temp = (d - distance) / kfunc.h;
                                // 距离计算后乘以两个企业的人数 [5/8/2016 mzl]
                                result += Math.Pow(Math.E, -0.5 * temp * temp) * Enterprises[i].man * Enterprises[j].man;
                            }
                        }
                    }
                    results.TryAdd(d, factor * result);
                });
                return results;
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.Flatten().InnerExceptions)
                {
                    Log.WriteError(ex.ToString());
                }
                return null;
            }
        }
    }

    public class KFunc
    {
        public int n { get; set; }
        public double R { get; set; }
        public double h { get; set; }
        // 1/2距离值
        public double Di { get; set; }

        public KFunc(int n, double r, double d = 0)
        {
            this.Di = d;
            this.n = n;
            this.R = r;
            // 原来的 h 少乘一个R [5/8/2016 mzl]
            this.h = 0.79 * R * Math.Pow(n, -0.2);
        }
    }
}
