/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 21:26
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Data;
using DataHelper.BaseUtil;

namespace DataHelper
{
	/// <summary>
	/// Description of GlobalDataInfo.
	/// </summary>
	public class GlobalDataInfo
	{		
		public static void InitalGlobalDataInfo()
		{
			Static.Table = DataPreProcess.GenerateDataTable();
		}
	}
}
