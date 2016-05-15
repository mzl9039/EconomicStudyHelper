using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Common;
using DataHelper.FuncSet;

namespace DataHelper.BaseUtil
{
    public class Static
    {
        // 选择的文件夹的路径 [3/11/2016 Administrator]
        public static string SelectedPath = string.Empty;

        // 根据选择的shp文件定义空间坐标系
        public static ISpatialReference SpatialReference = null;

        public static IFeatureClass FeatureClass = null;

        // 计算的excel数据，保存在tabel里 [3/11/2016 Administrator]
        public static DataTable Table = null;

        public static DataTable TableDiametar = null;

        // 要生成的shp的属性表结构
        public static IFields Fields = null;

        public static List<Enterprise> Enterprises = null;

        public static KdType kdType = KdType.KdClassic;

        public static void ReleaseMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
