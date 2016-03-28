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
        }

        public virtual void CaculateParams()
        {
            GetEnterprises();
            GetMedium();
            GetPointsDistance();
            GetKFunc();
        }

        public virtual void CaculateTrueValue()
        {
            GetTrueValue();
        }

        public void CaculateSimulateValue()
        {
            GetSimulateValue();
        }

        protected override void GetEnterprises()
        {
            DataTable table = Static.Table;
            this.Enterprises = DataProcess.ReadExcel(this.ExcelFile, table, null, FunctionType.KdEachTable);
        }

        protected override void GetMedium()
        {
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.Enterprises, this.XValue);
            findMedium.CaculateMedium();
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;       
        }

        public virtual void PrintTrueValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string trueValueFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable真实值计算结果.txt";
            base.PrintTrueValue(trueValueFile);
        }

        public virtual void PrintSimulateValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string simualteFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTable模拟值计算结果.txt";
            base.PrintSimulateValue(simualteFile);
        }

        // 对当前的每一个Excel文件进行操作，ExcelFile指当前的Excel的全路径文件名
        public string ExcelFile { get; set; }
    }
}
