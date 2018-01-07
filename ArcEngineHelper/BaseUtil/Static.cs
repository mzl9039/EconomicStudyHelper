using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using Common;
using DataHelper.FuncSet;
using System.Windows.Forms;

namespace DataHelper.BaseUtil
{
    public class Static
    {
        // 选择的文件夹的路径 [3/11/2016 Administrator]
        public static string SelectedPath = string.Empty;

        // 根据选择的shp文件定义空间坐标系
        public static ISpatialReference SpatialReference = InitSpatialReference();

        public static IFeatureClass FeatureClass = null;

        // 计算的excel数据，保存在tabel里 [3/11/2016 Administrator]
        public static DataTable Table = null;

        public static DataTable TableDiametar = null;

        // 要生成的shp的属性表结构
        public static IFields Fields = null;

        public static List<Enterprise> Enterprises = null;

        public static FunctionType funcType = FunctionType.Default;

        public static KdType kdType = KdType.KdClassic;

        public static DensityType densityType = DensityType.Diameter;

        public static void ReleaseMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public static ISpatialReference InitSpatialReference()
        {
            if (Static.SpatialReference == null)
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
            return Static.SpatialReference;
        }
    }
}
