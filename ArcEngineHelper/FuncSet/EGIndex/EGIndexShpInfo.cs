/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 15:49
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Common;
using DataHelper.BaseUtil;

namespace DataHelper.FuncSet.EGIndex
{
	/// <summary>
	/// Description of EGIndexShpInfo.
	/// </summary>
	public class EGIndexShpInfo : GlobalShpInfo
	{
		public EGIndexShpInfo(List<Enterprise> enterprises, string openShpDialogTitle, string shpname)
		{
            InitEGIndexShp(enterprises, openShpDialogTitle, shpname);
		}
		
		
		
		/// <summary>
		/// 初始化 EGIndex要使用的全国范围的 shp，
		/// 同时把企业信息写入要创建的shp中去
		/// </summary>
		/// <param name="enterprises">要写入 shp 中的数据列表</param>
		/// <param name="dataName">要写入的 shp 的文件名，不是全路径文件名</param>
        private void InitEGIndexShp(List<Enterprise> enterprises, string openShpDialogTitle, string dataName)
        {
			string shpName = DataPreProcess.GetShpName(openShpDialogTitle);
			if (shpName == null)
				return;
			
			ShpName = shpName;
			FeatureClassCounty = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(ShpName);	
			
            // 生成 shp 速度太慢，不再生成 shp
            FieldsPt = GeneratePointFields();
            //string dir = System.Windows.Forms.Application.StartupPath;

            //if (!Directory.Exists(dir + "\\EGIndex"))
            //    Directory.CreateDirectory(dir + "\\EGIndex");

            //ShpName = dir + "\\EGIndex\\" + dataName;
            //// 如果 shp 文件不存在，则创建 shp， 否则直接打开 shp
            //if (FeatureClassPt == null || WorkspacePt == null)
            //{
            //    FeatureClassPt = DataPreProcess.CreateShpFile(FieldsPt, ShpName);
            //    WorkspacePt = Geodatabase.GeodatabaseOp.Open_shapefile_Workspace(ShpName);
            //}

            //EGIndexBaseUtil.ExcelData2Shp(WorkspacePt, FeatureClassPt, enterprises);	
		}

        /// <summary>
        /// 定义要创建的shp文件的字段集,使用全局的shp空间坐标系 
        /// </summary>
        /// <returns></returns>
        public IFields GeneratePointFields()
        {
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;

            IGeometryDef geoDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geoDef as IGeometryDefEdit;
            geometryDefEdit.AvgNumPoints_2 = 1;
            geometryDefEdit.GridCount_2 = 0;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;

            IField shapeField = new FieldClass();
            IFieldEdit shapeFieldEdit = shapeField as IFieldEdit;
            shapeFieldEdit.Name_2 = "SHAPE";
            shapeFieldEdit.IsNullable_2 = true;
            shapeFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shapeFieldEdit.GeometryDef_2 = geoDef;
            shapeFieldEdit.Required_2 = true;
            geometryDefEdit.SpatialReference_2 = Static.SpatialReference;

            IField oidField = new FieldClass();
            IFieldEdit oidFieldEdit = oidField as IFieldEdit;
            oidFieldEdit.Name_2 = "ObjectID";
            oidFieldEdit.AliasName_2 = "FID";
            oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;

            IField excelId = new FieldClass();
            IFieldEdit excelIdEdit = excelId as IFieldEdit;
            excelIdEdit.Name_2 = "ExcelId";
            excelIdEdit.AliasName_2 = "Excel源";
            excelIdEdit.Type_2 = esriFieldType.esriFieldTypeString;

            IField man = new FieldClass();
            IFieldEdit manEdit = man as IFieldEdit;
            manEdit.Name_2 = "Man";
            manEdit.AliasName_2 = "员工数";
            manEdit.Type_2 = esriFieldType.esriFieldTypeInteger;

            fieldsEdit.AddField(shapeField);
            fieldsEdit.AddField(oidField);
            fieldsEdit.AddField(excelId);
            fieldsEdit.AddField(man);

            return fields;
        }

		public string ShpName { get; set; }
		
		// 定义生成shp的字段集
		public static IFields FieldsPt = null;
		// 全国县级行政区划 shp 的 featureclass
		public IFeatureClass FeatureClassCounty { get; set; }
		// 全部 excel 数据点 shp 的 workspace
		public IWorkspace WorkspacePt { get; set; }
		// 全部 excel 数据点 shp 的 featureclass
		public IFeatureClass FeatureClassPt { get; set; }		
	}
}
