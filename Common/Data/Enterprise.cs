/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 15:04
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using ESRI.ArcGIS.Geometry;

namespace Common
{
	/// <summary>
	/// Description of Data.
	/// </summary>
	public class Enterprise
	{
		public Enterprise(string id, IPoint point, int manNum) {
			ID = id;
		    Point = point;
			man = manNum;
		}
		
		public string ID { get; set; }
	    public IPoint Point { get; set; }
		public int man { get; set; }
	}
}
