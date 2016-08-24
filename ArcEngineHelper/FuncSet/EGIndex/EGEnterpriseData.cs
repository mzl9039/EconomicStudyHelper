/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 21:31
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
// using LogHelper;
using System.Collections.Concurrent;
using DataHelper.BaseUtil;

namespace DataHelper.FuncSet.EGIndex
{
	/// <summary>
	/// Description of EGIndexData.
	/// </summary>
	public class EGEnterpriseData
	{
		public EGEnterpriseData(string excel)
		{
			Excel = excel;
            List<Enterprise> enterprises = DataProcess.ReadExcel(Excel,Static.Table, null, FunctionType.EGIndex);
            enterprises.AsParallel().ForAll(e => this.Enterprises.Add(e));
			TotalStaff = Enterprises.Sum(e => e.man);
			CaculateXa();
		}
		
		private void CaculateXa() {
			try {				
				SumXa = Enterprises.Sum(e => Math.Pow(e.man / TotalStaff, 2));
			} catch (Exception ex) {
				Log.Log.Error(ex.ToString());
				//throw ex;
			}
		}

	    public double GetEGaResult()
	    {
	        return ((PartEGa - SumXa)/(1 - SumXa));
	    }

	    public void Close()
	    {
	        if (Enterprises != null)
	        {
	            Enterprises = null;
	        }
	    }

		public string Excel { get; set; }
		
		public ConcurrentBag<Enterprise> Enterprises { get; set; }
		
		public double TotalStaff { get; set; }
		
		public double SumXa { get; set; }
		
		public double PartEGa { get; set; }
	}
}
