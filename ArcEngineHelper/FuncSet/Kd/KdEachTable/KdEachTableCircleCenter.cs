using DataHelper.BaseUtil;
using ESRI.ArcGIS.Geodatabase;
using LogHelper;
using MapEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using Common.Data;
using Common;
using ESRI.ArcGIS.Geoprocessor;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DataHelper.FuncSet.Kd.KdEachTable
{
    public class KdEachTableCircleCenter : KdEachTable
    {
        public KdEachTableCircleCenter(string filename, double diameter)
            : base(filename)
        {
            this.Diameter = diameter;
            this.CenterEnterprise = new CenterEnterprise(this.ExcelFile, diameter);
            strTrueFileName = GetTrueFileName();
            strSimulateFileName = GetSimulateFileName();
        }

        /************************************************************************/
        /* Description:	判断真实值和模拟值是否已经计算并导出
        /* Authon:		mzl
        /* Date:		2016/5/8
        /************************************************************************/
        public override bool HasCaculated()
        {
            return base.HasCaculated(strTrueFileName, strSimulateFileName);
        }

        public override void CaculateParams()
        {
            if (IsTrueValueCacualted())
                return;

            GetEnterprises();
            CaculateCenterEnterprise();
            GetMedium();
            GetKFunc();
        }

        public override void CaculateRandomParams()
        {
            GetRandomEnterprises();
            switch (Static.kdType)
            {
                case KdType.KdClassic:
                    GetRandomMedium();
                    break;
                case KdType.KdScale:
                    break;
                default:
                    break;
            }
            GetRandomKFunc();
        }

        #region 真实值计算相关

        protected void CaculateCenterEnterprise()
        {            
            CenterEnterprise model = this.CenterEnterprise;
            model.EnterpriseId = this.SingleDogEnterprise[0].ID;
            model.Enterprises = new List<Enterprise>();
            for (int i = 0; i < this.SingleDogEnterprise.Count; i++)
            {
                Enterprise ce = SingleDogEnterprise[i];
                List<Enterprise> tempList = (from e in this.SingleDogEnterprise.AsParallel()
                                             let distance = (Math.Sqrt((ce.Point.X - e.Point.X) * (ce.Point.X - e.Point.X) +
                                                      (ce.Point.Y - e.Point.Y) * (ce.Point.Y - e.Point.Y)) / 1000)
                                             where distance != 0 && distance <= (this.Diameter / 2)
                                             select e).ToList();

                if (tempList.Count > model.Enterprises.Count)
                {
                    model.EnterpriseId = ce.ID;
                    model.Enterprises = tempList;
                }
            }
            PrintEnterprises();
        }        

        public override void CaculateTrueValue()
        {
            if (IsTrueValueCacualted())
                return;

            GetTrueValue(this.CenterEnterprise.Enterprises);
        }

        // 圆内的企业和圆心企业 [3/21/2016 mzl]
        protected void PrintEnterprises()
        {
            FileIOInfo fileIo = new FileIOInfo(this.ExcelFile);
            if (!Directory.Exists(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt))
                Directory.CreateDirectory(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt);
            string filename = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, "所有在圆内的企业.txt");
            if (File.Exists(filename))
                return;

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(string.Format("圆心企业ID:{0}", this.CenterEnterprise.EnterpriseId));
                sw.WriteLine(string.Format("共有企业{0}个", this.CenterEnterprise.Enterprises.Count));
                foreach (var e in this.CenterEnterprise.Enterprises)
                {
                    sw.WriteLine(string.Format("企业ID:{0};\t X坐标:{1};\t Y坐标:{2}", e.ID, e.Point.X, e.Point.Y));
                }
                sw.Flush();
                sw.Close();
            }
        }

        protected override void GetMedium()
        {
            // 怎么求解 [3/21/2016 mzl]
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.CenterEnterprise.Enterprises, this.XValue);
            findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.CenterEnterprise.Enterprises.Count);
        }

        protected override void GetKFunc()
        {
            int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            this.KFunc = new KFunc(this.CenterEnterprise.Enterprises.Count, distance, this.MediumValue);
        }
        #endregion

        #region 模拟值计算相关
        protected override List<Enterprise> GetRandomEnterprises()
        {
            RandomEnterprises.Clear();

            string str_seed = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            Random random = new Random(Int32.Parse(str_seed));
            for (int i = 0; i < KdBase.Kd_Mdl.N; i++)
            {
                int k = random.Next(this.SingleDogEnterprise.Count);
                if (!RandomEnterprises.Contains(this.SingleDogEnterprise[k])) RandomEnterprises.Add(this.SingleDogEnterprise[k]);
                else i--;
            }
            return RandomEnterprises;
        }
        #endregion

        public override void CaculateSimulateValue()
        {
            if (IsSimulatedValueCaculated())
                return;

            GetSimulateValue();
        }

        protected override string GetTrueFileName()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string trueValueFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTableCircleTable真实值计算结果.txt";
            return trueValueFile;
        }

        public override void PrintTrueValue()
        {
            if (IsTrueValueCacualted())
                return;

            base.PrintTrueValue(strTrueFileName);
        }

        protected override string GetSimulateFileName()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string simualteFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTableCircleTable模拟值计算结果.txt";
            return simualteFile;
        }

        public override void PrintSimulateValue()
        {
            if (IsSimulatedValueCaculated())
                return;

            base.PrintSimulateValue(strSimulateFileName);
        }

        public double Diameter { get; private set; }
        public CenterEnterprise CenterEnterprise { get; set; }

        // 真实值结果文件名 [5/8/2016 mzl]
        private string strTrueFileName = string.Empty;
        // 模拟值结果文件名 [5/8/2016 mzl]
        private string strSimulateFileName = string.Empty;
    }
}
