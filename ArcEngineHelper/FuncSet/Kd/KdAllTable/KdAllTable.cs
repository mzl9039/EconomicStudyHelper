using Common;
using Common.Data;
using DataHelper.BaseUtil;
using DataHelper.FuncSet.Kd;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataHelper.FuncSet.Kd.KdAllTable
{
    public class KdAllTable : KdTableBase
    {
        public KdAllTable(IEnumerable<string> excels) 
            : base()
        {
            this.Excels = excels as List<string>;
            this.SimulateValue = new List<double>();
        }

        public void CaculateParams()
        {
            GetEnterprises();
            GetMedium();
            GetKFunc();
        }

        public void CaculateTrueValue()
        {
            GetTrueValue();
        }

        public void CaculateSimulateValue()
        {
            GetSimulateValue();
        }

        protected override void GetEnterprises()
        {
            base.GetAllEnterprises();
        }

        protected override void GetMedium()
        {
            FindMediumBase findMedium = new FindMediumBase(this.Enterprises, this.XValue);
            this.PointsDistances = findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.Enterprises.Count);
        }

        public void PrintTrueValue()
        {
            FileIOInfo fileIo = new FileIOInfo(string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "KdAllTable真实值计算结果"));
            if (!System.IO.Directory.Exists(fileIo.FilePath))
                System.IO.Directory.CreateDirectory(fileIo.FilePath);
            base.PrintTrueValue(fileIo.FullFileName);
        }

        public void PrintSimulateValue()
        {
            FileIOInfo fileIo = new FileIOInfo(string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "KdAllTable模拟值计算结果"));
            base.PrintSimulateValue(fileIo.FullFileName);
        }

        public void PrintMediumValue()
        {
            try
            {
                FileIOInfo fileIo = new FileIOInfo(string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "中位数"));
                if (!Directory.Exists(fileIo.FilePath))
                    Directory.CreateDirectory(fileIo.FilePath);

                if (File.Exists(fileIo.FullFileName))
                    File.Delete(fileIo.FullFileName);

                using (FileStream fs = new FileStream(fileIo.FullFileName, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    for (int i = 0; i < Medium.Count; i++)
                    {
                        sw.WriteLine("{0}：{1}", Medium.ElementAt(i).Symbol, Medium.ElementAt(i).DistanceFile.Distance);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log.WriteError(ex.ToString());
                throw;
            }
        }


    }
}
