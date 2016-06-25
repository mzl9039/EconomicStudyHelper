using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessor;

namespace Geodatabase
{
    /// <summary>
    ///
    /// 功能描述:   静态类，处理和Geodatabase 相关的所有操作
    ///             包括：
    /// 开发者:     tangyb
    /// 建立时间:   2009-03-07 15:00:00
    /// 修订描述:
    /// 进度描述:
    /// 版本号      :    1.0
    /// 最后修改时间:    2009-03-07 15:00:00
    ///
    /// </summary>
    public partial class GeodatabaseOp
    {
        public static IDictionary<string,string> GetFeatureNameDic(IWorkspace workspace)
        {
            try
            {
                IDictionary<string, string> featureClassNameDic = new Dictionary<string, string>();
                IDictionary<string, string> layerNameDic = null;

                layerNameDic = GetFeatureNameDicInFeatDataset(workspace);
                foreach (KeyValuePair<string, string> keyValue in layerNameDic)
                {
                    featureClassNameDic.Add(keyValue);
                }

                layerNameDic = GetFeatureClassNameDic(workspace);
                foreach (KeyValuePair<string, string> keyValue in layerNameDic)
                {
                    featureClassNameDic.Add(keyValue);
                }

                return featureClassNameDic;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetFeatureNameDic:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static IDictionary<string,string> GetFeatureNameDicInFeatDataset(IWorkspace workspace)
        {
            try
            {
                IDictionary<string, string> featureClassNameDic = new Dictionary<string, string>();

                ESRI.ArcGIS.Geodatabase.IEnumDatasetName enumDatasetName = null;
                ESRI.ArcGIS.Geodatabase.IDatasetName datasetName = null;

                enumDatasetName = workspace.get_DatasetNames(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureDataset);
                datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    IFeatureDataset featureDataset = (datasetName as IName).Open() as IFeatureDataset;

                    IDictionary<string, string> layerNameDic = GetFeatureClassNameDic(featureDataset);
                    foreach (KeyValuePair<string, string> keyValue in layerNameDic)
                    {
                        featureClassNameDic.Add(keyValue);
                    }

                    datasetName = enumDatasetName.Next();
                }

                return featureClassNameDic;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetFeatureNameDicInFeatDataset:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static IDictionary<string, string> GetFeatureClassNameDic(IWorkspace workspace)
        {
            try
            {
                IDictionary<string, string> featureClassNameDic = new Dictionary<string, string>();

                ESRI.ArcGIS.Geodatabase.IEnumDatasetName enumDatasetName = null;
                ESRI.ArcGIS.Geodatabase.IDatasetName datasetName = null;

                enumDatasetName = workspace.get_DatasetNames(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTFeatureClass);
                datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    IFeatureClass featureClass = (datasetName as IName).Open() as IFeatureClass;

                    IDataset dataset = featureClass as IDataset;
                    featureClassNameDic.Add(dataset.BrowseName, featureClass.AliasName);

                    datasetName = enumDatasetName.Next();
                }

                return featureClassNameDic;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetFeatureClassNameDic:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
                //return null;
            }
        }

        public static IDictionary<string,string> GetFeatureClassNameDic(IFeatureDataset featureDataset)
        {
            try
            {
                IDictionary<string, string> featureClassNameDic = new Dictionary<string, string>();

                IFeatureClassContainer featureClassContainer = featureDataset as IFeatureClassContainer;
                for (int i = 0; i < featureClassContainer.ClassCount; i++ )
                {
                    IFeatureClass featureClass = featureClassContainer.get_Class(i);

                    IDataset dataset = featureClass as IDataset;
                    featureClassNameDic.Add(dataset.BrowseName, featureClass.AliasName);
                }

                return featureClassNameDic;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetFeatureClassNameDic:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static IWorkspace Open_ExcelFile_Workspace(string excelFileName)
        {
            try
            {                // Create a new OleDB workspacefactory and open the OleDB workspace
                IWorkspaceFactory workspaceFactory = new ESRI.ArcGIS.DataSourcesOleDB.ExcelWorkspaceFactory();
                ESRI.ArcGIS.Geodatabase.IWorkspace workspace = workspaceFactory.OpenFromFile(excelFileName, 0);

                return workspace;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.Open_ExcelFile_Workspace:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static IList<string> GetTableNameList(IWorkspace workspace)
        {
            try
            {
                IList<string> tableNameList = new List<string>();

                ESRI.ArcGIS.Geodatabase.IEnumDatasetName enumDatasetName = null;
                ESRI.ArcGIS.Geodatabase.IDatasetName datasetName = null;

                enumDatasetName = workspace.get_DatasetNames(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTTable);
                datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    ITable table = (datasetName as IName).Open() as ITable;

                    IDataset dataset = table as IDataset;

                    tableNameList.Add(dataset.BrowseName);

                    datasetName = enumDatasetName.Next();
                }

                return tableNameList;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetTableNameList:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                throw ex;
            }
        }

        public static IQueryFilter ConstructFilterWithOID(IFeatureClass featureClass,ISet featureSet)
        {
            try
            {
                if (!featureClass.HasOID)
                {
                    throw new Exception("当前对象没有OID字段，无法构造语句");
                }

                if (featureSet.Count < 1)
                {
                    return null;
                }

                IQueryFilter queryFilter = new QueryFilterClass();
                IFeature feature = null;
                long featCount = 0;

                string whereClause = String.Empty;
                string stemp = String.Empty;

                featureSet.Reset();
                feature = featureSet.Next() as IFeature;

                while (feature != null)
                {
                    if (featCount == 0)
                    {
                        stemp = String.Format("{0} = {1} ", featureClass.OIDFieldName, feature.OID);
                    }
                    else
                    {
                        stemp = String.Format(" OR {0} = {1} ", featureClass.OIDFieldName, feature.OID);
                    }
                    whereClause = whereClause + stemp;

                    feature = featureSet.Next() as IFeature;
                    featCount++;
                }

                queryFilter.WhereClause = whereClause;

                return queryFilter;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.ConstructFilterWithOID:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static IList<ESRI.ArcGIS.Carto.ILayer> GetRasterLayerList(string rasterFolder)
        {
            try
            {
                IList<ESRI.ArcGIS.Carto.ILayer> rasterLayerList = new List<ESRI.ArcGIS.Carto.ILayer>();

                ESRI.ArcGIS.Geodatabase.IWorkspace workspace = OpenRasterFileWorkspace(rasterFolder) as ESRI.ArcGIS.Geodatabase.IWorkspace;
                ESRI.ArcGIS.Geodatabase.IEnumDatasetName enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterDataset);
                enumDatasetName.Reset();

                IDatasetName datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    IRasterDataset rasterDataset = (datasetName as ESRI.ArcGIS.esriSystem.IName).Open() as IRasterDataset;
                    IDataset dataset = rasterDataset as IDataset;

                    ESRI.ArcGIS.Carto.IRasterLayer rasterLayer = new ESRI.ArcGIS.Carto.RasterLayerClass();
                    rasterLayer.CreateFromDataset(rasterDataset);
                    rasterLayer.Name = dataset.BrowseName;

                    //(rasterLayer as ESRI.ArcGIS.Carto.ILayerEffects).Transparency = 60;

                    rasterLayerList.Add(rasterLayer as ESRI.ArcGIS.Carto.ILayer);

                    //循环下一个
                    datasetName = enumDatasetName.Next();
                }

                return rasterLayerList;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterLayerList:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }
        }

        public static bool GetRasterLayerList(string rasterFolder,ref IList<ESRI.ArcGIS.Carto.ILayer> rasterLayerList )
        {
            try
            {
                ESRI.ArcGIS.Geodatabase.IWorkspace workspace = OpenRasterFileWorkspace(rasterFolder) as ESRI.ArcGIS.Geodatabase.IWorkspace;
                ESRI.ArcGIS.Geodatabase.IEnumDatasetName enumDatasetName = workspace.get_DatasetNames(esriDatasetType.esriDTRasterDataset);
                enumDatasetName.Reset();

                IDatasetName datasetName = enumDatasetName.Next();
                while (datasetName != null)
                {
                    IRasterDataset rasterDataset = (datasetName as ESRI.ArcGIS.esriSystem.IName).Open() as IRasterDataset;
                    IDataset dataset = rasterDataset as IDataset;

                    ESRI.ArcGIS.Carto.IRasterLayer rasterLayer = new ESRI.ArcGIS.Carto.RasterLayerClass();
                    rasterLayer.Name = dataset.BrowseName;

                    rasterLayerList.Add(rasterLayer as ESRI.ArcGIS.Carto.ILayer);

                    //循环下一个
                    datasetName = enumDatasetName.Next();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.GetRasterLayerList:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static void RepairGeometry(string filePath)
        {
            try
            {
                ESRI.ArcGIS.DataManagementTools.RepairGeometry repairTool = new ESRI.ArcGIS.DataManagementTools.RepairGeometry();
                ESRI.ArcGIS.Geoprocessor.Geoprocessor gp = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();

                gp.SetEnvironmentValue("workspace", filePath);
                repairTool.in_features = filePath;

                RunTool(gp, repairTool as IGPProcess, null);

            }
            catch (System.Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.RepairGeometry:" + ex.Message);
                //throw ex;
            }
        }


        /// <summary>
        /// 运行ArcTool
        /// </summary>
        /// <param name="geoprocessor"></param>
        /// <param name="process"></param>
        /// <param name="TC"></param>
        private static object RunTool(Geoprocessor geoprocessor, IGPProcess process, ITrackCancel TC)
        {
            object result;

            // Set the overwrite output option to true
            geoprocessor.OverwriteOutput = true;

            // Execute the tool
            try
            {
                result = geoprocessor.Execute(process, null);
                ReturnMessages(geoprocessor);

            }
            catch (Exception err)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.RunTool:" + err.Message);
                ReturnMessages(geoprocessor);
                throw err;
            }

            return result;
        }
        /// <summary>
        /// Function for returning the tool messages.
        /// </summary>
        /// <param name="gp"></param>
        private static void ReturnMessages(Geoprocessor gp)
        {
            if (gp.MessageCount > 0)
            {
                for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                {
                    //logger.WriteToLog(gp.GetMessage(Count), null);
                }
            }
        }
    }
}
