/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 22:01
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace Common
{
	/// <summary>
	/// Description of ExcelOp.
	/// </summary>
	public class ExcelOp
	{
		public static IExcelOp GetExcelReader(string filename) {
			IExcelOp excelReader;
			if (filename.EndsWith("xlsx"))
				excelReader = new Excel07() as IExcelOp;
			else
				excelReader = new Excel03() as IExcelOp;
			
			return excelReader;
		}
	}
	
	public enum DefaltExcel {
		DefaltSheet = 1
	}
}
