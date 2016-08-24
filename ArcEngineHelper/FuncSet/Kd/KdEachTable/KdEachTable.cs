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
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Geometry;

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
            densityType = Static.densityType;
            FileIOInfo fileIo = new FileIOInfo(ExcelFile);
            this.SingleDogEnterpriseFeatureClassFileName =
                System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + ".shp");                
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
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    GetMedium();
                    break;
                case KdType.KdScale:
                    GetStandardDeviation();
                    break;
                default:
                    break;
            }
            GetKFunc();
        }

        public virtual void CaculateRandomParams()
        {
            GetRandomEnterprises();
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    GetRandomMedium();
                    break;
                case KdType.KdScale:
                    GetRandomStandardDeviation();
                    break;
                default:
                    break;
            }
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

        public void GetPublicEnterprises()
        {
            GetEnterprises();
        }

        // 计算企业人口占行业总人口比率的平方和 [7/23/2016 15:09:54 mzl]
        public double CaculateManRatioInEnterprise()
        {
            double result = 0.0;
            double sumOfMan = this.SingleDogEnterprise.Sum(e => e.man);
            result = this.SingleDogEnterprise.Sum(e => Math.Pow(1.0 * e.man / sumOfMan, 2));
            return result;
        }

        public List<string> CaculateManRatioInEnterpriseByArea(string shpName)
        {
            List<string> result = new List<string>();
            IFeatureClass area = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(shpName);
            IFeatureCursor cursor = area.Search(null, false);
            IFeature feature;
            List<Enterprise> enterprises = new List<Enterprise>();
            while ((feature = cursor.NextFeature()) != null)
            {
                this.SingleDogEnterprise.ForEach(e =>
                {
                    IRelationalOperator relational = feature.Shape as IRelationalOperator;
                    if (relational.Contains(e.GeoPoint))
                        enterprises.Add(e);                                        
                });
                double sumOfMan = enterprises.Sum(e => e.man);
                double HIndex = enterprises.Sum(e => Math.Pow(1.0 * e.man / sumOfMan, 2));
                string areaName = feature.Value[feature.Fields.FindField("NAME")].ToString();
                result.Add(string.Format("{0}\t{1}", areaName, HIndex));
            }
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(cursor);
            return result;
        }

        /// <summary>
        /// 求真实值的中位数
        /// </summary>
        protected override void GetMedium()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.SingleDogEnterprise, this.XValue);
            findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.SingleDogEnterprise.Count);
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
            int distance = 0;
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
                    this.KFunc = new KFunc(this.SingleDogEnterprise.Count, distance, this.MediumValue);
                    break;
                case KdType.KdScale:
                    this.KFunc = new KFunc(this.SingleDogEnterprise.Count, this.TrueStandardDeviation);
                    break;
                default:
                    break;
            }
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

        #region 关于圆心的计算
        protected virtual CenterEnterprise BaseCaculateCenterEnterprise(List<Enterprise> enterprises, double diameter)
        {
            if (enterprises == null || enterprises.Count <= 0 || diameter <= 0.0)
            {
                Log.Log.Info("KdEachTable.BaseCaculateCenterEnterprise:企业集合为空或无数据，或传入的直径大小为0");
                return null;
            }

            CenterEnterprise enterpriseCircle = new CenterEnterprise();
            enterpriseCircle.EnterpriseId = enterprises[0].ID;
            enterpriseCircle.Enterprises = new List<Enterprise>();
            for (int i = 0; i < enterprises.Count; i++)
            {
                Enterprise en = enterprises[i];
                List<Enterprise> templist = (from e in enterprises.AsParallel()
                                             let distance = (Math.Sqrt((en.Point.X - e.Point.X) * (en.Point.X - e.Point.X) +
                                                      (en.Point.Y - e.Point.Y) * (en.Point.Y - e.Point.Y)) / 1000)
                                             where distance != 0 && distance <= (diameter / 2)
                                             select e).ToList();

                if (this.densityType == DensityType.Diameter && templist.Count > enterpriseCircle.Enterprises.Count)
                {
                    enterpriseCircle.EnterpriseId = en.ID;
                    enterpriseCircle.Enterprises = templist;
                    enterpriseCircle.Diameter = diameter;
                    enterpriseCircle.Excel = ExcelFile;
                }
                else if (this.densityType == DensityType.Scale && templist.Sum(x => x.man) > enterpriseCircle.Enterprises.Sum(x => x.man))
                {
                    enterpriseCircle.EnterpriseId = en.ID;
                    enterpriseCircle.Enterprises = templist;
                    enterpriseCircle.Diameter = diameter;
                    enterpriseCircle.Excel = ExcelFile;
                }
            }
            return enterpriseCircle;
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

        /// <summary>
        /// 求模拟值的中位数
        /// </summary>
        protected virtual void GetRandomMedium()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.RandomEnterprises, this.XValue);
            findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            KdBase.Kd_Mdl.SetN(this.RandomEnterprises.Count);
        }

        protected virtual void GetRandomStandardDeviation()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.RandomEnterprises);
            RandomStandardDeviation = findMedium.CaculateStandardDeviation();
            KdBase.Kd_Mdl.SetN(this.RandomEnterprises.Count);
        }
        
        protected virtual void GetRandomKFunc()
        {
            int distance = 0;
            double di = this.KFunc.Di;
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
                    this.KFunc = new KFunc(this.RandomEnterprises.Count, distance, di);
                    break;
                case KdType.KdScale:
                    this.KFunc = new KFunc(this.RandomEnterprises.Count, RandomStandardDeviation, di);
                    break;
                default:
                    break;
            }
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

        public virtual void PrintEnterpises()
        {
            if (File.Exists(SingleDogEnterpriseFeatureClassFileName))
            {
                if (this.SingleDogEnterpriseFeatureClass == null)
                {
                    this.SingleDogEnterpriseFeatureClass = Geodatabase.
                        GeodatabaseOp.OpenShapefileAsFeatClass(SingleDogEnterpriseFeatureClassFileName);
                }
            }   
            else
            {
                IFields fields = GlobalShpInfo.GeneratePointFields();
                int idxId = fields.FindField("ExcelId"),
                    idxMan = fields.FindField("Man");
                this.SingleDogEnterpriseFeatureClass = DataPreProcess.
                    CreateShpFile(fields, SingleDogEnterpriseFeatureClassFileName);
                using (ComReleaser comReleaser = new ComReleaser())
                {
                    IFeatureBuffer featureBuffer = null;

                    // Create an insert cursor.
                    IFeatureCursor insertCursor = SingleDogEnterpriseFeatureClass.Insert(true);
                    comReleaser.ManageLifetime(insertCursor);

                    for (int i = 0; i < this.SingleDogEnterprise.Count; i++)
                    {
                        featureBuffer = SingleDogEnterpriseFeatureClass.CreateFeatureBuffer();
                        comReleaser.ManageLifetime(featureBuffer);
                        featureBuffer.Value[idxId] = SingleDogEnterprise[i].ID;
                        featureBuffer.Value[idxMan] = SingleDogEnterprise[i].man;
                        featureBuffer.Shape = SingleDogEnterprise[i].GeoPoint;
                        insertCursor.InsertFeature(featureBuffer);
                    }
                    insertCursor.Flush();
                }                 
            }
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
        public IFeatureClass SingleDogEnterpriseFeatureClass { get; set; }
        private string SingleDogEnterpriseFeatureClassFileName { get; set; }
        // 浓度类型 [5/22/2016 16:58:05 mzl]
        protected DensityType densityType = DensityType.Diameter;
    }
}
