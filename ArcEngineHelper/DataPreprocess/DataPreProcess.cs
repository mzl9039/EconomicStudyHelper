using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.IO;
using ESRI.ArcGIS.DataSourcesFile;
using System.Data;
using LogHelper;
using System.Windows.Forms;

namespace DataHelper
{
    public class DataPreProcess
    {
        /// <summary>
        /// 根据空间坐标系和shp文件名创建shp文件
        /// </summary>
        /// <param name="spatialReference"></param>
        /// <param name="fullShpName"></param>
        /// <returns></returns>
        public static IFeatureClass CreateShpFile(IFields fields, string fullShpName)
        {
            try
            {
                IFeatureClass featureClass = null;
                string shpDir = System.IO.Path.GetDirectoryName(fullShpName);

                // 若路径不存在，则创建路径
                // 若路径存在，则文件已经创建，则删除文件，重新创建
                if (!Directory.Exists(shpDir))
                    Directory.CreateDirectory(shpDir);
                else
                {
                    if (File.Exists(fullShpName))
                    {
                        featureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(fullShpName);
                        return featureClass;
                    }
                }

                IWorkspaceFactory shpWorkspaceFactory = new ShapefileWorkspaceFactoryClass();
                const string SHAPE_FIELD_NAME = "SHAPE";
                IWorkspace shpWorkspace = shpWorkspaceFactory.OpenFromFile(shpDir, 0);
                IFeatureWorkspace shpFeatureWorkspace = shpWorkspace as IFeatureWorkspace;

                // Use IFieldChecker to create a validated fields collection.  
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IEnumFieldError enumFieldError = null;
                IFields validatedFields = null;
                fieldChecker.ValidateWorkspace = (IWorkspace)shpFeatureWorkspace;
                fieldChecker.Validate(fields, out enumFieldError, out validatedFields);

                featureClass = shpFeatureWorkspace.CreateFeatureClass(System.IO.Path.GetFileNameWithoutExtension(fullShpName),
                    validatedFields, null, null, esriFeatureType.esriFTSimple, SHAPE_FIELD_NAME, "");

                return featureClass;
            }
            catch (Exception ex)
            {
                Log.WriteError("DataPreProcess.CreateShpFile:" + ex.Message);
                //throw ex;
                return null;
            }
        }

        public static string GetShpName(string title)
        {
            string result = null;
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = title;
                ofd.Filter = "Shalefile文件 | *.shp";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    result = ofd.FileName;
                }
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.StackTrace);
                //throw ex;
            }
            return result;
        }

        public static DataTable GenerateDataTable()
        {
            DataTable table = new DataTable();

            DataColumn id = new DataColumn();
            id.ColumnName = "ID";
            id.DataType = System.Type.GetType("System.String");

            DataColumn company = new DataColumn();
            company.ColumnName = "Company";
            company.DataType = System.Type.GetType("System.String");

            DataColumn position = new DataColumn();
            position.ColumnName = "Position";
            position.DataType = System.Type.GetType("System.String");

            DataColumn longitude = new DataColumn();
            longitude.ColumnName = "Latitude";
            longitude.DataType = System.Type.GetType("System.Double");

            DataColumn latitude = new DataColumn();
            latitude.ColumnName = "Longitude";
            latitude.DataType = System.Type.GetType("System.Double");

            DataColumn man = new DataColumn();
            man.ColumnName = "Man";
            man.DataType = System.Type.GetType("System.String");

            DataColumn woman = new DataColumn();
            woman.ColumnName = "Woman";
            woman.DataType = System.Type.GetType("System.String");

            table.Columns.Add(id);
            table.Columns.Add(company);
            table.Columns.Add(position);
            table.Columns.Add(longitude);
            table.Columns.Add(latitude);
            table.Columns.Add(man);
            table.Columns.Add(woman);

            return table;
        }

        public static DataTable GenerateKdMaxDistanceTable()
        {
            DataTable table = new DataTable();

            DataColumn ec = new DataColumn();
            ec.ColumnName = "EC";           // EnterpriseCode
            ec.DataType = System.Type.GetType("System.String");

            DataColumn max = new DataColumn();
            max.ColumnName = "Max";         // MaxDistance
            max.DataType = System.Type.GetType("System.Double");

            table.Columns.Add(ec);
            table.Columns.Add(max);

            return table;
        }

        /// <summary>
        /// 保存各个excel要搜索的圆的直径
        /// </summary>
        /// <returns></returns>
        public static DataTable GenerateTableAllEnterpriseSearchDiameter()
        {
            DataTable table = new DataTable();

            DataColumn en = new DataColumn();
            en.ColumnName = "en";
            en.DataType = System.Type.GetType("System.String");

            DataColumn dm = new DataColumn();
            dm.ColumnName = "dm";
            dm.DataType = System.Type.GetType("System.Double");

            table.Columns.Add(en);
            table.Columns.Add(dm);

            return table;
        }
    }
}
