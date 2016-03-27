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
		    cbxFuncType.SelectedItem = ft[5];
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
                Log.WriteError(ex.StackTrace);
                //throw ex;
            }
            KdBase kdBase;

            switch (cbxFuncType.Text)
		    {
                case "K(d)":
                    kdBase = new KdBase(excels.ToList());
                    kdBase.CaculateKdAllTable();
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
                    GlobalShpInfo.InitalShpInfo();
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
            this.cbxFuncType.Items.AddRange(ft);
	    }

        // 各种方法集合 [3/14/2016 mzl]
        // K(d)刘晔是指K(d)EachTable方法，其中每个excel里的中位数由全部excel的中位数确定 [3/14/2016 mzl]
        // K(d)圆心指K(d)EachTable方法，其中每个excel [3/14/2016 mzl]
        private string[] ft = new[] { "K(d)", "EGIndex", "EGRobust", "K(d)EachTable距离特征值", "K(d)Cara", "K(d)Circle" };
	}
}
