﻿using Common;
using Common.Data;
using DataHelper.BaseUtil;
using DataHelper.FuncSet.Kd;
using DataHelper.FuncSet.KdBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataHelper.FuncSet.Kd.KdEachTable
{
    public class KdEachTable : KdTableBase
    {
        public KdEachTable(string filename)
            : base()
        {
            this.ExcelFile = filename;
            this.SimulateValue = new List<double>();
            this.SingleDogEnterprise = new List<Enterprise>();
            this.RandomEnterprises = new List<Enterprise>();
            strTrueFileName = GetTrueFileName();
            strSimulateFileName = GetSimulateFileName();
        }

        /************************************************************************/
        /* Description:	判断真实值和模拟值是否已经计算并导出
        /* Authon:		mzl
        /* Date:		2016/5/8
        /************************************************************************/
        public virtual bool HasCaculated()
        {
            return base.HasCaculated(strTrueFileName, strSimulateFileName);
        }

        public virtual void CaculateParams()
        {
            if (IsTrueValueCacualted())
                return;

            GetEnterprises();
            // 中位数换为标准差，所以获取中位数的函数弃用 [5/8/2016 21:16:09 mzl]
            //GetMedium();
            GetStandardDeviation();
            GetKFunc();
        }

        public virtual void CaculateRandomParams()
        {
            GetRandomEnterprises();
            // 标准差替换中位数，所以求模拟值中位数的函数弃用 [5/8/2016 21:18:55 mzl]
            //GetRandomMedium();
            GetRandomStandardDeviation();
            GetRandomKFunc();
        }

        public virtual void CaculateTrueValue()
        {
            if (IsTrueValueCacualted())
                return;

            GetTrueValue(this.SingleDogEnterprise);
        }

        public virtual void CaculateSimulateValue()
        {
            if (IsSimulatedValueCaculated())
                return;

            GetSimulateValue();
        }

        #region 真实值计算相关
        protected override void GetEnterprises()
        {
            DataTable table = Static.Table;
            this.SingleDogEnterprise = DataProcess.ReadExcel(this.ExcelFile, table, null, FunctionType.KdEachTable);
        }

        // 中位数换为标准差，所以这个方法弃用 [5/8/2016 21:12:02 mzl]
        /// <summary>
        /// 求真实值的中位数
        /// </summary>
        protected override void GetMedium()
        {
            //FindMedium findMedium = new FindMedium(this.ExcelFile, this.SingleDogEnterprise, this.XValue);
            //findMedium.CaculateMediumAndGetPointDistance(0.0);
            //this.Medium = findMedium.Mediums;
            //this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            //KdBase.Kd_Mdl.SetN(this.SingleDogEnterprise.Count);
        }

        /// <summary>
        /// 求真实值的标准差
        /// </summary>
        protected override void GetStandardDeviation()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.SingleDogEnterprise);
            this.TrueStandardDeviation = findMedium.CaculateStandardDeviation();
            KdBase.Kd_Mdl.SetN(this.SingleDogEnterprise.Count);
        }

        protected override void GetKFunc()
        {
            #region 注释
            // 中位数换为标准差，所以这distance不再有意义，弃用,KFunc的参数也做替换 [5/8/2016 21:13:09 mzl]
            //int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            //this.KFunc = new KFunc(this.SingleDogEnterprise.Count, distance, this.MediumValue);
            #endregion
            this.KFunc = new KFunc(this.SingleDogEnterprise.Count, this.TrueStandardDeviation);
        }

        protected override string GetTrueFileName()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            return fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable真实值计算结果.txt";
        }

        public virtual bool IsTrueValueCacualted()
        {
            return IsValueCaculated(strTrueFileName);
        }

        public virtual void PrintTrueValue()
        {
            if (IsTrueValueCacualted())
                return;

            base.PrintTrueValue(strTrueFileName);
        }
        #endregion

        #region 模拟值计算相关
        protected override void GetSimulateValue()
        {
            for (int i = 0; i < Kd_Mdl.SimulateTimes; i++)
            {
                CaculateRandomParams();
                this.SimulateValue.AddRange(GetRandomValueOnce().Select(x => x.Value).ToList());
            }
        }

        protected override ConcurrentDictionary<int, double> GetRandomValueOnce()
        {
            ConcurrentDictionary<int, double> randomValue = new ConcurrentDictionary<int, double>();
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    randomValue = Kd.Func(this.KFunc, RandomEnterprises);
                    break;
                case KdType.KdScale:
                    randomValue = Kd.FuncScale(this.KFunc, RandomEnterprises);
                    break;
                default:
                    break;
            }
            return randomValue;
        }

        protected override List<Common.Enterprise> GetRandomEnterprises()
        {
            RandomEnterprises.Clear();

            string str_seed = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            Random random = new Random(Int32.Parse(str_seed));
            for (int i = 0; i < KdBase.Kd_Mdl.N; i++)
            {
                int k = random.Next(this.Enterprises.Count);
                if (!RandomEnterprises.Contains(this.Enterprises[k])) RandomEnterprises.Add(this.Enterprises[k]);
                else i--;
            }
            return RandomEnterprises;
        }

        
        //标准差替换中位数，所以这个函数充用[5 / 8 / 2016 21:18:13 mzl]
        /// <summary>
        /// 求模拟值的中位数，由于要用标准差替换中位数，所以这个函数弃用
        /// </summary>
        protected virtual void GetRandomMedium()
        {
            //FindMedium findMedium = new FindMedium(this.ExcelFile, this.RandomEnterprises, this.XValue);
            //findMedium.CaculateMediumAndGetPointDistance(0.0);
            //this.Medium = findMedium.Mediums;
            //KdBase.Kd_Mdl.SetN(this.RandomEnterprises.Count);
        }

        protected virtual void GetRandomStandardDeviation()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.RandomEnterprises);
            RandomStandardDeviation = findMedium.CaculateStandardDeviation();
            KdBase.Kd_Mdl.SetN(this.RandomEnterprises.Count);
        }
        
        protected virtual void GetRandomKFunc()
        {
            #region 注释
            // 用标准差替换中位数，不再使用中位数 [5/8/2016 21:22:55 mzl]
            //int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            #endregion
            // 每次new KFunc 时，参数Di会为0，为保留原来的Di，使用原有的di保存这个结果 [5/8/2016 21:24:16 mzl]
            double di = this.KFunc.Di;
            this.KFunc = new KFunc(this.RandomEnterprises.Count, RandomStandardDeviation, di);
        }

        protected override string GetSimulateFileName()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string simualteFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable模拟值计算结果.txt";
            return simualteFile;
        }

        public virtual bool IsSimulatedValueCaculated()
        {
            return IsValueCaculated(strSimulateFileName);
        }

        #endregion

        public virtual void PrintSimulateValue()
        {
            if (IsSimulatedValueCaculated())
                return;

            base.PrintSimulateValue(strSimulateFileName);
        }

        // 对当前的每一个Excel文件进行操作，ExcelFile指当前的Excel的全路径文件名
        public string ExcelFile { get; set; }
        public List<Enterprise> SingleDogEnterprise { get; set; }
        public List<Enterprise> RandomEnterprises { get; set; }
        // 真实值文件名 [5/8/2016 mzl]
        private string strTrueFileName = string.Empty;
        // 模拟值文件名 [5/8/2016 mzl]
        private string strSimulateFileName = string.Empty;
        // 行业内两两企业距离的方差 [5/8/2016 21:04:24 mzl]
        protected double TrueStandardDeviation = 0.0;
        // 模拟值两两企业距离的方差 [5/8/2016 21:20:55 mzl]
        protected double RandomStandardDeviation = 0.0;
    }
}
