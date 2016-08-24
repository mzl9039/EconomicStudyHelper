/*
 * Created by SharpDevelop.
 * User: mzl
 * Date: 2015-10-16
 * Time: 18:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Linq;
// using LogHelper;
using DataHelper;
using DataHelper.FuncSet.EGIndex;
using DataHelper.FuncSet.EGIndex.RobustCheck;
using DataHelper.FuncSet.Kd.KdEachTable;
using DataHelper.FuncSet.KdBase;
using System.Collections.Concurrent;
using DataHelper.BaseUtil;
using ESRI.ArcGIS.Geometry;

namespace EconomicStudyHelper
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
            InitializeComponent();
			GlobalDataInfo.InitalGlobalDataInfo();			

            InitCbxFuncType();
		    cbxFuncType.SelectedItem = funcType[funcType.Length - 1];
            cbxKdFuncType.SelectedItem = kdFuncType[0];
            cb_densityType.SelectedItem = densityType[0];
		}
		
		void Btn_StartClick(object sender, EventArgs e)
		{
		    string[] excels = null;
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            try
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)                
                    excels = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*gps.xlsx");                
                else
                    return;

                Static.SelectedPath = folderBrowserDialog.SelectedPath;
            }
            catch (Exception ex)
            {
                Log.Log.Error(ex.ToString());
            }
            KdBase kdBase;

            switch (cbxFuncType.Text)
		    {
                case "K(d)":
                    Log.Log.Info("K(d)方法对所有表的所有企业计算真实值和模拟值，无意义，被废除该方法");
                    //kdBase = new KdBase(excels.ToList());
                    //kdBase.CaculateKdAllTable();
                    break;
                case "EGIndex":
                    // 这里的代码不能使用了，因为InitalShpInfo里生成的Fields不符合要求了 [5/22/2016 16:33:37 mzl]
                    GlobalShpInfo.InitalShpInfo();
                    EGIndex eg = new EGIndex(excels.ToList());
                    eg.OutPutEGIndex();
                    break;
                case "EGRobust":
                    // 这里的代码不能使用了，因为InitalShpInfo里生成的Fields不符合要求了 [5/22/2016 16:33:37 mzl]
                    GlobalShpInfo.InitalShpInfo();
                    EGRobust egRobust = new EGRobust(excels.ToList());
                    egRobust.OutPutEGIndex();
                    break;
                case "K(d)EachTable距离特征值":
                    kdBase = new KdBase(excels.ToList());
                    kdBase.GetMaxDistances();
                    kdBase.CaculateKdEachTable();
                    break;
                case "K(d)Cara":
                    kdBase = new KdBase(excels.ToList());
                    kdBase.CaculateKdEachTableByAllTableMedium();
                    break;
                case "K(d)Circle":
                    //GlobalShpInfo.InitalShpInfo();
                    Static.TableDiametar = DataPreProcess.GenerateTableAllEnterpriseSearchDiameter();
                    kdBase = new KdBase(excels.ToList());
                    kdBase.GetCircleeDiameters();
                    kdBase.CaculateKdEachTableByCircle();
                    break;
                case "K(d)单圆多圆":                    
                    Static.TableDiametar = DataPreProcess.GenerateIndustryDiameterTable();
                    kdBase = new KdBase(excels.ToList());
                    kdBase.GetMultiCircleDiameters();
                    kdBase.CaculateKdEachTableMultiCircleCenter();
                    break;
                case "H指数":
                    kdBase = new KdBase(excels.ToList());
                    kdBase.CaculateHIndex();
                    break;
                case "分区域H指数":
                    kdBase = new KdBase(excels.ToList());
                    kdBase.CaculateHIndexByArea();
                    break;
                default:
                    break;
		    }
            MessageBox.Show("执行结束！");
            this.Close();
		}

	    void InitCbxFuncType()
	    {	        
            this.cbxFuncType.Items.AddRange(funcType);
            // 允许使用企业规模的KFunc类型 [5/8/2016 21:34:59 mzl]
            this.cbxKdFuncType.Items.AddRange(kdFuncType);
            // 计算多圆时允许设置相应的浓度计算类型 [5/15/2016 22:13:29 mzl]
            this.cb_densityType.Items.AddRange(densityType);
	    }



        // 各种方法集合 [3/14/2016 mzl]
        // K(d)刘晔是指K(d)EachTable方法，其中每个excel里的中位数由全部excel的中位数确定 [3/14/2016 mzl]
        // K(d)圆心指K(d)EachTable方法，其中每个excel [3/14/2016 mzl]
        //  [5/15/2016 16:22:36 mzl]
        private string[] funcType = new[] { "K(d)", "EGIndex", "EGRobust", "K(d)EachTable距离特征值", "K(d)Cara", "K(d)Circle", "K(d)单圆多圆", "H指数", "分区域H指数" };
        // 对Kd计算做调整，看企业规模的变化会结果的影响 [5/8/2016 mzl]
        private string[] kdFuncType = new string[] { "原有Kd方法", "计算企业规模的Kd方法" };

        private string[] densityType = new string[] { "半径浓度", "人口浓度" };
        private void cbxKdFuncType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbxKdFuncType.SelectedText)
            {
                case "原有Kd方法":
                    Static.kdType = KdType.KdClassic;
                    break;
                case "计算企业规模的Kd方法":
                    Static.kdType = KdType.KdScale;
                    break; 
                default:
                    break;
            }
        }

        private void cb_densityType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cb_densityType.Text.ToString())
            {
                case "半径浓度":
                    Static.densityType = DensityType.Diameter;
                    break;
                case "人口浓度":
                    Static.densityType = DensityType.Scale;
                    break;
                default:
                    break;
            }
        }

        private void cbxFuncType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbxFuncType.SelectedText)
            {
                case "K(d)":
                    Static.funcType = FunctionType.Kd;
                    break;
                case "EGIndex":
                    Static.funcType = FunctionType.EGIndex;
                    break;
                case "EGRobust":
                    Static.funcType = FunctionType.EGIndexRobust;                    
                    break;
                case "K(d)EachTable距离特征值":
                    Static.funcType = FunctionType.KdEachTable;
                    break;
                case "K(d)Cara":
                    Static.funcType = FunctionType.KdEachTablbCara;
                    break;
                case "K(d)Circle":
                    Static.funcType = FunctionType.KdEachTableCircle;
                    break;
                case "K(d)单圆多圆":
                    Static.funcType = FunctionType.KdEachTableMultiCircle;
                    break;
                default:
                    break;
            }
        }
    }
}
