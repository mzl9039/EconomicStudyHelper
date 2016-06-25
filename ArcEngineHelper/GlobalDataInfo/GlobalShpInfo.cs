/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-16
 * 时间: 20:41
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using LogHelper;
using DataHelper.BaseUtil;

namespace DataHelper
{
	/// <summary>
	/// Description of CreateShpfile.
	/// </summary>
	public class GlobalShpInfo
	{		
		// 定义生成shp的字段名
		//public static IFields Fields = null;

		// 工作空间工厂
		public static IWorkspaceFactory shpWSF = null;		
		
		public static void InitalShpInfo(){
			Static.SpatialReference = GetSpatialReferenceFromShp();
            // 这里的IFieds需要是生成的圆的shp，所以需要的Fields更改了，要是EGIndex需要Fields，需要使用下面的注释代码 [5/22/2016 16:34:00 mzl]
            //Static.Fields = GeneratePointFields();
            //Static.Fields = GenerateCircleFields();
			shpWSF = new ShapefileWorkspaceFactoryClass();				
		}

        public static ISpatialReference GetSpatialReferenceFromShp()
        {
            ISpatialReference spatialReference = null;
            string title = "导入包含正确坐标系信息的shp";
            string shpName = DataPreProcess.GetShpName(title);
            if (shpName == null)
            {
                Log.WriteLog("获取的shp文件名为空");
                return null;
            }

            try
            {
                IFeatureClass featureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(shpName);
                IFeatureCursor featureCursor = Geodatabase.GeodatabaseOp.QuerySearch(featureClass, null, null, false);
                IFeature feature;
                while ((feature = featureCursor.NextFeature()) != null)
                {
                    spatialReference = feature.Shape.SpatialReference;

                    if (spatialReference != null)
                        break;
                }
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featureCursor);
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                throw;
            }
            return spatialReference;
        }	
		
        /// <summary>
        /// 生成shp的各个字段
        /// </summary>
        /// <returns></returns>
        public static IFields GeneratePointFields()
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

        public static IFields GeneratePolygonFields()
        {
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;

            IGeometryDef geoDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geoDef as IGeometryDefEdit;
            geometryDefEdit.AvgNumPoints_2 = 1;
            geometryDefEdit.GridCount_2 = 0;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;

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

        public static IFields GenerateCircleFields()
        {
            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;

            // 定义shape字段
            IGeometryDef geoDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geoDef as IGeometryDefEdit;
            geometryDefEdit.AvgNumPoints_2 = 1;
            geometryDefEdit.GridCount_2 = 0;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;

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

            IField circleId = new FieldClass();
            IFieldEdit circleIdEdit = circleId as IFieldEdit;
            circleIdEdit.Name_2 = "CircleId";
            circleIdEdit.AliasName_2 = "圆心企业ID";
            circleIdEdit.Type_2 = esriFieldType.esriFieldTypeString;

            IField diameter = new FieldClass();
            IFieldEdit diameterEdit = diameter as IFieldEdit;
            diameterEdit.Name_2 = "Diameter";
            diameterEdit.AliasName_2 = "圆直径";
            diameterEdit.Type_2 = esriFieldType.esriFieldTypeDouble;

            IField numDensity = new FieldClass();
            IFieldEdit numDensityEdit = numDensity as IFieldEdit;
            numDensityEdit.Name_2 = "NumDensity";
            numDensityEdit.AliasName_2 = "企业数量浓度";
            numDensityEdit.Type_2 = esriFieldType.esriFieldTypeDouble;

            IField scaleDensity = new FieldClass();
            IFieldEdit scaleDensityEdit = scaleDensity as IFieldEdit;
            scaleDensityEdit.Name_2 = "ScaleDensity";
            scaleDensityEdit.AliasName_2 = "企业规模浓度";
            scaleDensityEdit.Type_2 = esriFieldType.esriFieldTypeDouble;

            fieldsEdit.AddField(shapeField);
            fieldsEdit.AddField(oidField);
            fieldsEdit.AddField(circleId);
            fieldsEdit.AddField(diameter);
            fieldsEdit.AddField(numDensity);
            fieldsEdit.AddField(scaleDensity);

            return fields;
        }
    }
}
