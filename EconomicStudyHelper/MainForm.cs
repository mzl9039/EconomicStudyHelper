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
using LogHelper;
using DataHelper;
using DataHelper.FuncSet.EGIndex;
using DataHelper.FuncSet.EGIndex.RobustCheck;
using DataHelper.FuncSet.Kd.KdEachTable;
using DataHelper.FuncSet.KdBase;
using System.Collections.Concurrent;
using DataHelper.BaseUtil;

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
		    cbxFuncType.SelectedItem = funcType[4];
            cbxKdFuncType.SelectedItem = kdFuncType[0];
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
                Log.WriteError(ex.ToString());
                //throw ex;
            }
            KdBase kdBase;

            switch (cbxFuncType.Text)
		    {
                case "K(d)":
                    Log.WriteLog("K(d)方法对所有表的所有企业计算真实值和模拟值，无意义，被废除该方法");
                    //kdBase = new KdBase(excels.ToList());
                    //kdBase.CaculateKdAllTable();
                    break;
                case "EGIndex":
                    GlobalShpInfo.InitalShpInfo();
                    EGIndex eg = new EGIndex(excels.ToList());
                    eg.OutPutEGIndex();
                    break;
                case "EGRobust":
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
	    }

        // 各种方法集合 [3/14/2016 mzl]
        // K(d)刘晔是指K(d)EachTable方法，其中每个excel里的中位数由全部excel的中位数确定 [3/14/2016 mzl]
        // K(d)圆心指K(d)EachTable方法，其中每个excel [3/14/2016 mzl]
        private string[] funcType = new[] { "K(d)", "EGIndex", "EGRobust", "K(d)EachTable距离特征值", "K(d)Cara", "K(d)Circle" };
        // 对Kd计算做调整，看企业规模的变化会结果的影响 [5/8/2016 mzl]
        private string[] kdFuncType = new string[] { "原有Kd方法", "计算企业规模的Kd方法" };

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
    }
}
