using Common;
using Common.Data;
using DataHelper.BaseUtil;
using LogHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DataHelper
{
    /// <summary>
    /// 计算中位数的类，用于计算所有excel内所有企业的两两距离
    /// </summary>
    public class FindMediumBase
    {
        public FindMediumBase(List<Enterprise> enterprises, double MaxDistance = 0.0)
        {
            this.Enterprises = enterprises;
            Mediums = new ConcurrentQueue<MediumInfo>();
            this.MaxDistance = MaxDistance;
        }

        public void CaculateMediumAndGetPointDistance(double MaxDistance)    
        {
            CountDistancesPerKilometer(MaxDistance);
            SetMediumsAndFindDistanceRange();
        }

        /// <summary>
        /// 计算落在每公里范围内的距离值的个数，计算中位数需要两次遍历（每次遍历均
        /// 为双层for循环，运算次数n(n-1)/2）运算，这是第一次
        /// </summary>
        protected void CountDistancesPerKilometer(double MaxDistance)
        {
            try
            {
                if (Enterprises == null || Enterprises.Count <= 0)
                    return;

                InitDistanceFiles();

                int EnterprisesCount = Enterprises.Count;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Parallel.For(0, EnterprisesCount, (i, loopStateOut) =>
                {
                    Enterprise eOut = Enterprises[i];
                    try
                    {
                        for (int j = i + 1; j < EnterprisesCount; j++)
                        {
                            Enterprise eIn = Enterprises[j];
                            double distance = Math.Sqrt((eOut.Point.Y - eIn.Point.Y) * (eOut.Point.Y - eIn.Point.Y) +
                                                        (eOut.Point.X - eIn.Point.X) * (eOut.Point.X - eIn.Point.X)) / 1000;

                            if (0 == distance || (MaxDistance > 0 && distance > MaxDistance))
                                continue;
                            else
                            {
                                if (!DistanceFiles.ContainsKey((int)distance))
                                    continue;

                                DistanceFiles[(int)distance].FileRowCount++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError("距离并行计算异常中止：" + ex.ToString());
                        loopStateOut.Stop();
                    }
                });
                watch.Stop();
                Log.WriteLog("CountDistancesPerKilometer:运行时间：" + (watch.ElapsedMilliseconds / 1000));
            }
            catch (AggregateException aex)
            {
                for (int i = 0; i < aex.Flatten().InnerExceptions.Count; i++)
                {
                    Log.WriteError(aex.Flatten().InnerExceptions[i].InnerException.ToString());
                }
            }       
        }

        /// <summary>
        /// 这里确定要找哪些中位数，如1/4中位数，1/2中位数，3/4中位数等等
        /// 这个方法可以确定要找的这些中位数的整数部分（由于距离是按公里来
        /// 划分范围的），这个方法可以确定在哪个公里范围内,
        /// 如果结果要求不精确的话，可以不进行第二次计算
        /// </summary>
        protected void SetMediumsAndFindDistanceRange()
        {
            try
            {
                // 计算一共产生的数据条数
                double total_num = 0;
                foreach (var d in this.DistanceFiles)
                {
                    total_num += d.Value.FileRowCount;
                }
                // 若要修改要找的中位数的个数或值，只需要修改这个数组
                double[] mediumSymbols = new double[] { 0.25, 0.5, 0.75 };
                foreach (var symbol in mediumSymbols)
                {                
                    Mediums.Enqueue(new MediumInfo(symbol, (total_num * symbol)));
                }

                for (int i = 0; i < DistanceFiles.Count; i++)
                {
                    foreach (var medium in Mediums)
                    {
                        medium.SetMedium(DistanceFiles[i]);
                    }
                    if (Mediums.All(m => m.Stop == true))
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 计算精确中位数时，第二次计算，调用第二次计算的具体过程，将中位数所在距离范围内的所有值保留到Mediums
        /// 留待下一步调用快选算法取得中位数
        /// </summary>
        protected void GetFindMediumFinalResults()
        {
            try
            {
                ConcurrentBag<ConcurrentBag<double>> mediumDistances =
                    CacuDistanceAndStore(Enterprises, Mediums.Select(m => m.DistanceFile.Distance).ToList());
                for (int i = 0; i < mediumDistances.Count; i++)
                {
                    (Mediums.ElementAt(i).DistanceFile.Distances as List<double>).AddRange(mediumDistances.ElementAt(i));
                }
                // 释放内存，存储的distance值释放掉
                for (int i = 0; i < DistanceFiles.Count; i++)
                {
                    DistanceFiles[i].Close();
                }
                DistanceFiles = null;

            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                //throw ex;
            }
        }

        /// <summary>
        /// 计算中位数的第二次运算的具体过程，保存（在内存中）每公里范围内的distance的double值
        /// 完成之后，可以调用快速选择算法，使用Mediums中的Counter，获取当前范围内所求中位数的准确值
        /// </summary>
        /// <param name="enterprises"></param>
        /// <param name="smallDistance"></param>
        /// <param name="bigDistance"></param>
        protected ConcurrentBag<ConcurrentBag<double>> CacuDistanceAndStore(List<Enterprise> enterprises, List<int> distances)
        {
            try
            {
                ConcurrentBag<ConcurrentBag<double>> distanceMemory = new ConcurrentBag<ConcurrentBag<double>>();
                for (int i = 0; i < distances.Count; i++)
                {
                    distanceMemory.Add(new ConcurrentBag<double>());
                }
                Parallel.For(0, enterprises.Count, (i, loopStateOut) =>
                {
                    Enterprise eOut = enterprises.ElementAt(i);
                    for (int j = i + 1; j < enterprises.Count; j++)
                    {
                        Enterprise eIn = enterprises.ElementAt(j);
                        double distance = Math.Sqrt((eOut.Point.X - eIn.Point.X) * (eOut.Point.X - eIn.Point.X) +
                                                    (eOut.Point.Y - eIn.Point.Y) * (eOut.Point.Y - eIn.Point.Y)) / 1000;

                        int idistance = (int)(distance);
                        if (distances.Contains(idistance))
                            distanceMemory.ElementAt(distances.FindIndex(ind => ind == idistance)).Add(distance);
                    }
                });
                return distanceMemory;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                //throw ex;
                return null;
            }
        }

        protected List<Enterprise> Enterprises { get; set; }
        // 由于每公里的距离相关信息值作为一个distanceFile
        protected ConcurrentDictionary<int, DistanceFile> DistanceFiles { get; set; }

        public ConcurrentQueue<MediumInfo> Mediums { get; set; }

        public virtual void InitDistanceFiles()
        {
            Log.WriteLog("初始化DistanceFiles");
            DistanceFiles = new ConcurrentDictionary<int, DistanceFile>();
            string filename = System.IO.Path.Combine(Static.SelectedPath, Const.AllCaculatedPath + ".txt");
            FileIOInfo fio = new FileIOInfo(filename);
            string dfDir = fio.FilePath + @"\" + fio.FileNameWithoutExt;
            for (int i = 0; i < 5000; i++)
            {
                DistanceFile df = new DistanceFile(string.Format(dfDir + @"\{0}.txt", i.ToString()), i);
                DistanceFiles.TryAdd(i, df);
            }
        }

        protected double MaxDistance { get; set; }
    }
}
