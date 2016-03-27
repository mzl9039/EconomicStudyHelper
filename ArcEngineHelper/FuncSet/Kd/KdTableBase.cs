using Common;
using Common.Data;
using DataHelper.FuncSet.KdBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataHelper.FuncSet.Kd
{
    /// <summary>
    /// KdAllTable 和 KdEachTable 的父类
    /// </summary>
    public class KdTableBase
    {
        public KdTableBase()
        {
            this.Enterprises = new List<Enterprise>();
            this.Medium = new ConcurrentQueue<MediumInfo>();
            this.PointsDistances = new ConcurrentBag<TwoPointsDistance>();
            this.TrueValue = new ConcurrentDictionary<int, double>();
            this.XValue = 0.0;
        }

        protected virtual void GetEnterprises()
        {

        }

        protected virtual void GetMedium()
        {

        }

        protected virtual void GetKFunc()
        {
            int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            this.KFunc = new KFunc(this.Enterprises.Count, distance, this.MediumValue);
        }

        protected virtual void GetPointsDistance()
        {
            this.PointsDistances = FindMedium.CacuPointDistance(this.Enterprises, this.XValue);
            KdBase.Kd_Mdl.SetN(this.Enterprises.Count);
        }

        protected virtual void GetTrueValue()
        {
            this.TrueValue = Kd.Func(this.KFunc, this.PointsDistances.Select(d => d.Distance).ToList());
        }

        protected virtual void GetSimulateValue()
        {
            for (int i = 0; i < Kd_Mdl.SimulateTimes; i++)
            {
                this.SimulateValue.AddRange(GetRandomValueOnce().Select(x => x.Value).ToList());
            }
        }

        protected virtual ConcurrentDictionary<int, double> GetRandomValueOnce()
        {
            ConcurrentBag<TwoPointsDistance> randomDistances = CaculateRandomDistances();
            ConcurrentDictionary<int, double> randomValue = Kd.Func(this.KFunc, randomDistances.Select(r => r.Distance).ToList());
            return randomValue;
        }

        protected virtual ConcurrentBag<TwoPointsDistance> CaculateRandomDistances()
        {
            ConcurrentBag<TwoPointsDistance> randomDistances = new ConcurrentBag<TwoPointsDistance>();

            List<Enterprise> enterprise = GetRandomEnterprise();
            randomDistances = FindMedium.CacuPointDistance(enterprise, 0.0);

            return randomDistances;
        }

        // 随机选择n个企业
        protected virtual List<Common.Enterprise> GetRandomEnterprise()
        {
            List<Enterprise> enterprise = new List<Common.Enterprise>();

            string str_seed = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            Random random = new Random(Int32.Parse(str_seed));
            for (int i = 0; i < KdBase.Kd_Mdl.N; i++)
            {
                int k = random.Next(this.Enterprises.Count);
                if (!enterprise.Contains(this.Enterprises[k])) enterprise.Add(this.Enterprises[k]);
                else i--;
            }
            return enterprise;
        }

        protected virtual void PrintTrueValue(string filename)
        {
            if (File.Exists(filename))
                return;
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));
            
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);

                for (int i = 0; i < this.TrueValue.Count; i++)
                {
                    sw.WriteLine("距离为" + i + "处的真实值为：" + TrueValue.ElementAt(i).Value + "\n\t");
                }

                sw.Flush();
                sw.Close();
            }
        }

        protected virtual void PrintSimulateValue(string filename)
        {
            if (File.Exists(filename))
                return;

            List<double> simulate = new List<double>();
            List<string> results = new List<string>();
            for (int j = 0; j < this.TrueValue.Count; j++)
            {
                simulate.Clear();
                for (int i = 0; i < Kd_Mdl.SimulateTimes; i++)
                {
                    simulate.Add(this.SimulateValue[j + i * this.TrueValue.Count]);
                }
                if (simulate.Count > 0)
                {
                    simulate.Sort();
                    results.Add(j.ToString() + "\t1%:" + simulate[(int)(Kd_Mdl.SimulateTimes * 0.01)] + "\t5%:"
                        + simulate[(int)(Kd_Mdl.SimulateTimes * 0.05)] + "\t95%:" + simulate[(int)(Kd_Mdl.SimulateTimes * 0.95)] + "\t99%:" + simulate[(int)(Kd_Mdl.SimulateTimes * 0.99)]);
                }
            }

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);

                results.ForEach(r => sw.WriteLine(r));

                sw.Flush();
                sw.Close();
            }
        }

        // 特定的距离值，表内两点之间的距离大于这个值要被过滤
        public double XValue { get; set; }
        // 求出的1/4，1/2，3/4这三个中位数中的1/2处的值
        public int MediumValue { get; set; }
        protected ConcurrentQueue<MediumInfo> Medium { get; set; }
        // 每个Excel文件中所有的数据
        protected List<Enterprise> Enterprises { get; set; }
        // 两点间距离的集合 [3/11/2016 mzl]
        protected ConcurrentBag<TwoPointsDistance> PointsDistances { get; set; }
        public KFunc KFunc { get; set; }
        // 真实值 [3/11/2016 mzl]
        public ConcurrentDictionary<int, double> TrueValue { get; set; }
        // 模拟值集合 [3/12/2016 mzl]
        public List<double> SimulateValue { get; set; }
    }
}
