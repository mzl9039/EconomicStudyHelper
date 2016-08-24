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
            // 创建文件夹，由于三个文件共用一个文件夹，只需要创建一次就够了 [5/8/2016 mzl]
            if (!System.IO.Directory.Exists(strMediumFileName))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(strMediumFileName));
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

        public void CaculateParams()
        {
            GetEnterprises();
            GetMedium();
            GetKFunc();
        }

        public void CaculateTrueValue()
        {
            GetTrueValue(Enterprises);
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
            findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.Enterprises.Count);
        }

        protected override string GetTrueFileName()
        {
            FileIOInfo fileIo = new FileIOInfo(strTrueFileName);
            if (!System.IO.Directory.Exists(fileIo.FilePath))
                System.IO.Directory.CreateDirectory(fileIo.FilePath);
            return fileIo.FullFileName; 
        }

        public void PrintTrueValue()
        {
            string trueFileName = GetTrueFileName();
            if (!IsValueCaculated(trueFileName))
            {
                base.PrintTrueValue(trueFileName);
            }
        }

        protected override string GetSimulateFileName()
        {
            FileIOInfo fileIo = new FileIOInfo(strSimulateFileName);
            return fileIo.FullFileName;
        }

        public void PrintSimulateValue()
        {
            string simulateFileName = GetSimulateFileName();
            if (!IsValueCaculated(simulateFileName))
            {
                base.PrintSimulateValue(simulateFileName);
            }
        }

        public void PrintMediumValue()
        {
            try
            {
                FileIOInfo fileIo = new FileIOInfo(strMediumFileName);
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
                Log.Log.Error(ex.ToString());
                throw;
            }
        }

        private string strMediumFileName = string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "中位数");
        private string strTrueFileName = string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "KdAllTable真实值计算结果");
        private string strSimulateFileName = string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "KdAllTable模拟值计算结果");
    }
}
