using Common;
using Common.Data;
using DataHelper.BaseUtil;
using DataHelper.FuncSet.KdBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
            this.TrueValue = new ConcurrentDictionary<int, double>();
            this.XValue = 0.0;
        }

        protected virtual void GetEnterprises() { }

        public virtual void GetAllEnterprises()
        {
            if (Static.Enterprises == null)
            {
                DataTable table = Static.Table;
                Static.Enterprises = DataProcess.ReadExcels(this.Excels, table, null, FunctionType.Kd);
            }
            this.Enterprises = Static.Enterprises;
        }

        protected virtual void GetMedium() { }

        protected virtual void GetStandardDeviation() { }

        protected virtual void GetKFunc() { }

        protected virtual void GetTrueValue(List<Enterprise> Enterprises)
        {
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    this.TrueValue = Kd.Func(this.KFunc, Enterprises);
                    break;
                case KdType.KdScale:
                    this.TrueValue = Kd.FuncScale(this.KFunc, Enterprises);
                    break;
                default:
                    break;
            }
        }

        protected virtual void GetSimulateValue() { }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual ConcurrentDictionary<int, double> GetRandomValueOnce()
        {
            return null;
        }

        protected virtual ConcurrentBag<TwoPointsDistance> CaculateRandomDistances()
        {
            return null;
        }

        // 随机选择n个企业
        protected virtual List<Common.Enterprise> GetRandomEnterprises()
        {
            return null;
        }

        protected virtual string GetTrueFileName()
        {
            return string.Empty;
        }

        protected virtual void PrintTrueValue(string filename)
        {
            if (IsValueCaculated(filename))
                return;
            
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

        /************************************************************************/
        /* Description:	判断真实值和模拟值是否已经计算并导出过
        /* Authon:		mzl
        /* Date:		2016/5/8
        /************************************************************************/
        protected virtual bool HasCaculated(string trueValueFileName, string simulateValueFileName)
        {
            return (IsValueCaculated(trueValueFileName) && IsValueCaculated(simulateValueFileName));                
        }             

        /************************************************************************/
        /* Description:	判断某个文件是否之前输出过，若这个文件存在，则认为这个
        /*              文件代表的结果在上一次运行时已经计算过了
        /* Authon:		mzl
        /* Date:		2016/5/8
        /************************************************************************/
        protected virtual bool IsValueCaculated(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    return true;
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename));

                return false;
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.ToString());
                return false;
            }
        }

        protected virtual string GetSimulateFileName()
        {
            return string.Empty;
        }

        protected virtual void PrintSimulateValue(string filename)
        {
            if (IsValueCaculated(filename))
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
        public KFunc KFunc { get; set; }
        // 真实值 [3/11/2016 mzl]
        public ConcurrentDictionary<int, double> TrueValue { get; set; }
        // 模拟值集合 [3/12/2016 mzl]
        public List<double> SimulateValue { get; set; }
        // 当前所有的excels [3/11/2016 mzl]
        public List<string> Excels { get; set; }
    }
}
