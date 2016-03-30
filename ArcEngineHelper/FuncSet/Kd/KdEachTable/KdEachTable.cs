using Common;
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
            this.RandomPointDistances = new ConcurrentBag<TwoPointsDistance>();
        }

        public virtual void CaculateParams()
        {
            GetEnterprises();
            GetMedium();
            GetKFunc();
        }

        public virtual void CaculateRandomParams()
        {
            GetRandomEnterprises();
            GetRandomMedium();
            GetRandomKFunc();
        }

        public virtual void CaculateTrueValue()
        {
            GetTrueValue();
        }

        public virtual void CaculateSimulateValue()
        {
            GetSimulateValue();
        }

        #region 真实值计算相关
        protected override void GetEnterprises()
        {
            DataTable table = Static.Table;
            this.SingleDogEnterprise = DataProcess.ReadExcel(this.ExcelFile, table, null, FunctionType.KdEachTable);
        }        

        protected override void GetMedium()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.SingleDogEnterprise, this.XValue);
            this.PointsDistances = findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.SingleDogEnterprise.Count);
        }

        protected override void GetKFunc()
        {
            int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            this.KFunc = new KFunc(this.SingleDogEnterprise.Count, distance, this.MediumValue);
        }

        public virtual void PrintTrueValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string trueValueFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable真实值计算结果.txt";
            base.PrintTrueValue(trueValueFile);
        }
        #endregion

        #region 模拟值计算相关
        protected override void GetSimulateValue()
        {
            for (int i = 0; i < Kd_Mdl.SimulateTimes; i++)
            {
                this.SimulateValue.AddRange(GetRandomValueOnce().Select(x => x.Value).ToList());
            }
        }

        protected override ConcurrentDictionary<int, double> GetRandomValueOnce()
        {
            ConcurrentDictionary<int, double> randomValue = Kd.Func(this.KFunc, RandomPointDistances.Select(r => r.Distance).ToList());
            return randomValue;
        }

        protected override List<Common.Enterprise> GetRandomEnterprises()
        {
            RandomEnterprises.Clear();

            string str_seed = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            Random random = new Random(Int32.Parse(str_seed));
            for (int i = 0; i < KdBase.Kd_Mdl.N; i++)
            {
                int k = random.Next(this.SingleDogEnterprise.Count);
                if (!RandomEnterprises.Contains(this.Enterprises[k])) RandomEnterprises.Add(this.Enterprises[k]);
                else i--;
            }
            return RandomEnterprises;
        }

        protected virtual void GetRandomMedium()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.RandomEnterprises, this.XValue);
            this.RandomPointDistances = findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            //this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.RandomEnterprises.Count);
        }

        protected virtual void GetRandomKFunc()
        {
            int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            double oldMediumValue = this.KFunc.Di;
            this.KFunc = new KFunc(this.RandomEnterprises.Count, distance, oldMediumValue);
        }

        #endregion

        public virtual void PrintSimulateValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string simualteFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable模拟值计算结果.txt";
            base.PrintSimulateValue(simualteFile);
        }

        // 对当前的每一个Excel文件进行操作，ExcelFile指当前的Excel的全路径文件名
        public string ExcelFile { get; set; }
        public List<Enterprise> SingleDogEnterprise { get; set; }

        public List<Enterprise> RandomEnterprises { get; set; }

        public ConcurrentBag<TwoPointsDistance> RandomPointDistances { get; set; }
    }
}
