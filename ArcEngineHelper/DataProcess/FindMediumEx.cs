using Common;
using Common.Data;
using LogHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHelper
{
    public partial class FindMedium
    {
        /// <summary>
        /// 计算两点间距离，但距离不能大于某一个值，否则不包括在返回结果内
        /// </summary>
        /// <param name="enterprises"></param>
        /// <param name="MaxDistance"></param>
        /// <returns></returns>
        public static ConcurrentBag<TwoPointsDistance> CacuPointDistance(List<Enterprise> enterprises, double MaxDistance)
        {
            ConcurrentBag<TwoPointsDistance> pointDistances = new ConcurrentBag<TwoPointsDistance>();
            if (enterprises == null || enterprises.Count <= 0)
                return pointDistances;

            try
            {
                int EnterprisesCount = enterprises.Count;

                Parallel.For(0, EnterprisesCount, (i, loopStateOut) =>
                {
                    Enterprise eOut = enterprises[i];
                    for (int j = i + 1; j < EnterprisesCount; j++)
                    {
                        Enterprise eIn = enterprises[j];
                        double distance = Math.Sqrt((eOut.Point.Y - eIn.Point.Y) * (eOut.Point.Y - eIn.Point.Y) +
                                                    (eOut.Point.X - eIn.Point.X) * (eOut.Point.X - eIn.Point.X)) / 1000;

                        if (0 == distance || (MaxDistance > 0.0 && distance > MaxDistance))
                            continue;                        
                        else                        
                            pointDistances.Add(new TwoPointsDistance(eOut.ID, eIn.ID, distance));                        
                    }
                });
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                //throw ex;
            }

            return pointDistances;
        }
    }
}
