﻿using Common;
using Common.Data;
using DataHelper.BaseUtil;
using DataHelper.FuncSet.Kd.KdAllTable;
using DataHelper.FuncSet.Kd.KdEachTable;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace DataHelper.FuncSet.KdBase
{
    public class KdBase
    {
        public KdBase(IEnumerable<string> excels)
        {
            Excels = excels as List<string>;
            // 根据方法类型，确定是否设置模拟值
            if (Static.funcType == FunctionType.Kd || Static.funcType == FunctionType.KdEachTable
                || Static.funcType == FunctionType.KdEachTablbCara || Static.funcType == FunctionType.KdEachTableCircle
                || Static.funcType == FunctionType.KdEachTableMultiCircle)
                Kd_Mdl.SetSimulateTimes();
            //GetMaxDistances();       
        }

        public void GetMaxDistances()
        {
            DataTable table = DataPreProcess.GenerateKdMaxDistanceTable();
            string filename = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "打开距离特征值";
            ofd.Filter = "距离特征值文件 | *Distance.xlsx";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
            }
            MaxDistances = DataProcess.ReadExcelMaxDistance(filename, table) as List<KdMaxDistance>;
        }

        public void GetCircleeDiameters()
        {
            DataTable table = DataPreProcess.GenerateTableAllEnterpriseSearchDiameter();
            string filename = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "打开企业代码与直径文件";
            ofd.Filter = "直径 | *.xlsx";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
            }
            CircleDiameters = DataProcess.ReadExcelCircleDiameter(filename, table) as List<CircleDiameter>;
        }

        public void GetMultiCircleDiameters()
        {
            DataTable table = Static.TableDiametar;
            string filename = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "打开企业代码与直径文件";
            ofd.Filter = "直径 | *.xlsx";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
                filename = ofd.FileName;

            MultiDiameters = DataProcess.ReadMultiCircleDiameter(filename, table) as List<MultiCircleDiameters>;
        }

        public DataTable GetEnterpriseInCountry(DataTable table)
        {
            string filename = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "打开中国县界表";
            ofd.Filter = "直径 | *.xlsx";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
                string destFileName = Static.SelectedPath + "\\" + new FileIOInfo(filename).FileNameWithoutExt + ".xlsx";
                if (File.Exists(destFileName))
                    File.Delete(destFileName);
                File.Copy(filename, destFileName);
                countryExcel = new ExcelAccess(destFileName);
                table = DataProcess.ReadEnterpriseDistributedInCountryExcel(destFileName, table);
            }
            
            return table;
        }

        public void InitSpatialReference()
        {
            ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
            string filename = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "打开坐标系文件";
            ofd.Filter = "直径 | *.prj";
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filename = ofd.FileName;
            }
            Static.SpatialReference = srf.CreateESRISpatialReferenceFromPRJFile(filename);
        }

        public void CaculateKdEachTable()
        {
            Excels.ForEach(e =>
            {
                if (MaxDistances.Exists(m => e.Contains(m.ec)))
                {
                    KdEachTable kdET = new FuncSet.Kd.KdEachTable.KdEachTable(e);
                    kdET.XValue = MaxDistances.Find(m => e.Contains(m.ec + "gps.xlsx")).max;
                    kdET.CaculateParams();
                    kdET.CaculateTrueValue();
                    kdET.PrintTrueValue();
                    kdET.CaculateSimulateValue();
                    kdET.PrintSimulateValue();
                }
            });
        }

        /************************************************************************/
        /* Description:	这个函数是无数学意义的，应该被删除
        /* Authon:		mzl
        /* Date:		2016/5/8
        /************************************************************************/
        public void CaculateKdAllTable()
        {
            KdAllTable kdAT = new KdAllTable(this.Excels);
            kdAT.XValue = 0.0;
            kdAT.CaculateParams();
            kdAT.PrintMediumValue();
            kdAT.CaculateTrueValue();
            kdAT.PrintTrueValue();
            kdAT.CaculateSimulateValue();
            kdAT.PrintSimulateValue();
        }

        #region 2016.3.14 由全部表获取中位数，然后对每张表，由这个中位数构建KFunc
        public int GetKdAllTableMediumValue()
        {
            string filename = string.Format("{0}\\所有表的数据\\{1}.txt", Static.SelectedPath, "中位数");
            int medium = 0;
            if (System.IO.File.Exists(filename))
            {
                using (FileStream fs = new System.IO.FileStream(filename, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    string line = sr.ReadLine();
                    // 第二行保存的是中位数
                    line = sr.ReadLine();
                    string[] strMediumInfos = line.Split(new char[] { '：' });
                    medium = int.Parse(strMediumInfos[1].Trim());
                    sr.Close();
                }
            }
            else
            {
                KdAllTable kdAT = new KdAllTable(this.Excels);
                kdAT.XValue = 0.0;
                kdAT.CaculateParams();
                kdAT.PrintMediumValue();
                medium = kdAT.MediumValue;
            }
            return medium;
        }

        public void CaculateKdEachTableByAllTableMedium()
        {
            int mediumValue = GetKdAllTableMediumValue();
            Excels.ForEach(e =>
            {
                KdEachTable kdET = new FuncSet.Kd.KdEachTable.KdEachTable(e);
                // 若已经计算过这个行业的真实值和模拟值，则跳过这个行业 [5/8/2016 mzl]
                #region 被注释的原来的代码
                //kdET.Excels = Excels;
                //kdET.XValue = 0;
                //kdET.CaculateParams();
                //// 重设kdET的KFunc的Di值 [3/14/2016 mzl]
                //if (kdET.KFunc != null)
                //    kdET.KFunc.Di = mediumValue;
                //kdET.CaculateTrueValue();
                //kdET.PrintTrueValue();
                //kdET.GetAllEnterprises();
                ////kdET.KFunc.Di = mediumValue;
                //kdET.CaculateSimulateValue();
                //kdET.PrintSimulateValue();
                #endregion
                if (!kdET.HasCaculated())
                {
                    kdET.Excels = Excels;
                    kdET.XValue = 0;
                    kdET.CaculateParams();
                    // 重设kdET的KFunc的Di值 [3/14/2016 mzl]
                    if (kdET.KFunc != null)
                        kdET.KFunc.Di = mediumValue;
                    kdET.CaculateTrueValue();
                    kdET.PrintTrueValue();
                    kdET.GetAllEnterprises();
                    //kdET.KFunc.Di = mediumValue;
                    kdET.CaculateSimulateValue();
                    kdET.PrintSimulateValue();
                }
            });
        }
        #endregion

        #region 对每一个excel,生成一个shp，然后遍历所有表内的企业，使用
        public void CaculateKdEachTableByCircle()
        {
            Excels.ForEach(e =>
            {
                FileIOInfo fileIo = new FileIOInfo(e);
                CircleDiameter cd = this.CircleDiameters.Find(x => fileIo.FileName.Contains(x.EnterpriseCode));
                if (cd != null)
                {
                    KdEachTableCircleCenter kdEtCC = new KdEachTableCircleCenter(e, cd.Diameter);
                    // 若已经计算过这个行业的真实值和模拟值，则跳过这个行业 [5/8/2016 mzl]
                    #region 被注释的代码
                    //kdEtCC.CaculateParams();
                    //kdEtCC.CaculateTrueValue();
                    //kdEtCC.PrintTrueValue();
                    //kdEtCC.CaculateSimulateValue();
                    //kdEtCC.PrintSimulateValue();
                    #endregion
                    if (!kdEtCC.HasCaculated())
                    {
                        kdEtCC.CaculateParams();
                        kdEtCC.CaculateTrueValue();
                        kdEtCC.PrintTrueValue();
                        kdEtCC.CaculateSimulateValue();
                        kdEtCC.PrintSimulateValue();
                    }
                }
            });
        }
        #endregion

        #region 对多圆单圆情况进行计算，多圆直接计算，单圆要查找可能的其它的圆
        public void CaculateKdEachTableMultiCircleCenter()
        {
            //GlobalShpInfo.InitalShpInfo();
            InitSpatialReference();
            string shpFileName = DataPreProcess.GetShpName("选择中国县界shp");
            IFeatureClass shpFeatureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(shpFileName);            
            DataTable table = DataPreProcess.GenerateEnterpriseInCountryTable(shpFeatureClass);
            DataTable excelTable = GetEnterpriseInCountry(table);

            Excels.ForEach(e =>
            {
                FileIOInfo fileIo = new FileIOInfo(e);
                MultiCircleDiameters mcd = this.MultiDiameters.Find(x => fileIo.FileName.Contains(x.EnterpriseCode));
                if (mcd != null)
                {
                    KdEachTableMultiCircleCenter kdetmcc = new KdEachTableMultiCircleCenter(e, mcd);
                    kdetmcc.CaculateParams();
                    kdetmcc.CaculateCircleEnterprise();
                    kdetmcc.PrintCircleEnterprise();
                    //kdetmcc.FindEnterpriseDistribution();
                    kdetmcc.FindEnterpriseDistributionInChina();
                    kdetmcc.PrintEnterpises();
                    kdetmcc.SetEnterpriseNumsInCountry(ref table, shpFeatureClass);
                }
            });
            countryExcel.FastExportToExcel(table, 1, 0);
            countryExcel.Save();
            countryExcel.Close();     
        }
        #endregion

        #region 计算就业人口
        public void CaculateHIndex()
        {
            string filename = string.Format("{0}\\{1}.txt", Static.SelectedPath, "H指数");
            if (System.IO.File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (FileStream fs = new System.IO.FileStream(filename, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs);
                Excels.ForEach(e =>
                {
                    KdEachTable kdET = new FuncSet.Kd.KdEachTable.KdEachTable(e);
                    kdET.GetPublicEnterprises();
                    double result = kdET.CaculateManRatioInEnterprise();
                    System.IO.FileInfo fi = new FileInfo(kdET.ExcelFile);
                    sw.WriteLine(string.Format("{0}:\t{1}", fi.Name, result));
                });
                sw.Close();
            }
        }
        #endregion

        #region 分区域计算就业人口
        // 分区域计算H指数 [8/24/2016 8:37:43 mzl]
        public void CaculateHIndexByArea()
        {
            string shpName = DataPreProcess.GetShpName("选择shp文件");
            Static.SpatialReference = GlobalShpInfo.GetSpatialReferenceFromShp(shpName);
            string filename = string.Format("{0}\\{1}.txt", Static.SelectedPath, "H指数By行政区");
            if (System.IO.File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (FileStream fs = new System.IO.FileStream(filename, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs);
                Excels.ForEach(e =>
                {
                    KdEachTable kdET = new FuncSet.Kd.KdEachTable.KdEachTable(e);
                    kdET.GetPublicEnterprises();
                    List<string> result = kdET.CaculateManRatioInEnterpriseByArea(shpName);
                    System.IO.FileInfo fi = new FileInfo(kdET.ExcelFile);
                    result.ForEach(r => sw.WriteLine(string.Format("{0}:\t{1}", fi.Name, r)));
                });
                sw.Close();
            }
        }
        #endregion

        public List<string> Excels { get; set; }
        public List<KdMaxDistance> MaxDistances { get; set; }
        // 单圆直径 [5/15/2016 16:45:58 mzl]
        public List<CircleDiameter> CircleDiameters { get; set; }
        // 可能有多个圆的直径 [5/15/2016 16:49:12 mzl]
        public List<MultiCircleDiameters> MultiDiameters { get; set; }
        ExcelAccess countryExcel { get; set; }
    }
}
