﻿/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 15:04
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using ESRI.ArcGIS.Geometry;
using System;

namespace Common
{
	/// <summary>
	/// Description of Data.
	/// </summary>
	public class Enterprise
	{
		public Enterprise(string id, IPoint getPoint, int manNum) {
			ID = id;
		    GeoPoint = getPoint;
            Point = new Point(GeoPoint.X, GeoPoint.Y);
			man = manNum;
		}
		
		public string ID { get; set; }
	    public Point Point { get; set; }
		public int man { get; set; }
        public IPoint GeoPoint { get; set; }
    }
}
