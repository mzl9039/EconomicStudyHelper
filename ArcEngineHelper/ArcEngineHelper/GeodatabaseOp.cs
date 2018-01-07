using System;
using System.Collections.Generic;
using System.Text;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;

namespace Geodatabase
{
    /// <summary>
    ///
    /// 功能描述:   静态类，处理和Geodatabase 相关的所有操作
    ///             包括：连接sde，创建workspace，创建featureclass、table，返回字段，查询、更新等等
    /// 开发者:     XXX
    /// 建立时间:   2008-10-8 0:00:00
    /// 修订描述:
    /// 进度描述:
    /// 版本号      :    1.0
    /// 最后修改时间:    2008-10-7 13:36:48
    /// </summary>
    public partial class GeodatabaseOp
    {
        #region "Operation About Feature and Fields Related"

        /// <summary>
        ///
        /// 功能描述:   将某个要素存放到某个图层中
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="featLayer">目标图层</param>
        /// <param name="feat">源要素</param>
        /// <returns>无</returns>
        public static void CreateFeature(IFeatureLayer featLayer, IFeature featSource)
        {
            CreateFeature(featLayer.FeatureClass, featSource);
        }

        /// <summary>
        ///
        /// 功能描述:   将某个要素存放到某个图层中
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="featClass">目标图层</param>
        /// <param name="featSource">源要素</param>
        public static int CreateFeature(IFeatureClass featClass, IFeature featSource)
        {
            IFeature newFeature = featClass.CreateFeature();

            if (newFeature != null)
            {
                CopyAttribures(featSource, ref newFeature);

                newFeature.Shape = featSource.Shape;
                newFeature.Store();
                return newFeature.OID;
            }
            return -1;
        }

        /// <summary>
        ///
        /// 功能描述:   要素从原图层移动到到目标iDestClass
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatCursor"></param>
        /// <param name="iDestClass"></param>
        public static void FeatureMove(IFeatureCursor featCursorMove, IFeatureClass iDestClass)
        {
            IDataset dataset = (IDataset)iDestClass;
            IWorkspace workspace = dataset.Workspace;

            //Cast for an IWorkspaceEdit
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

            //Start an edit session and operation
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IGeometry iGeometry;
            IFeature iOriFeat;
            IFeatureCursor featCursorInsert = iDestClass.Insert(true);

            iOriFeat = featCursorMove.NextFeature();
            while (iOriFeat != null)
            {
                IFeatureBuffer featureBuffer = iDestClass.CreateFeatureBuffer();
                //+获得几何拷贝
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                //+属性拷贝
                CopyAttribures(iOriFeat, ref featureBuffer);

                //+设置几何
                featureBuffer.Shape = iGeometry;
                featCursorInsert.InsertFeature(featureBuffer);

                featCursorMove.DeleteFeature();
                iOriFeat = featCursorMove.NextFeature();
            }

            featCursorMove.Flush();
            featCursorInsert.Flush();

            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);
        }

        /// <summary>
        ///
        /// 功能描述:   要素复制到目标iDestClass
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatCursor"></param>
        /// <param name="iDestClass"></param>
        public static void FeatureCopy(IFeatureCursor featCursorCopy, IFeatureClass iDestClass)
        {
            IDataset dataset = (IDataset)iDestClass;
            IWorkspace workspace = dataset.Workspace;

            //Cast for an IWorkspaceEdit
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

            //Start an edit session and operation
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IGeometry iGeometry;
            IFeature iOriFeat;
            IFeatureCursor featCursorInsert = iDestClass.Insert(true);

            int lFeatureCount = 0;
            iOriFeat = featCursorCopy.NextFeature();
            while (iOriFeat != null)
            {
                IFeatureBuffer featureBuffer = iDestClass.CreateFeatureBuffer();
                //+获得几何拷贝
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                //+属性拷贝
                CopyAttribures(iOriFeat, ref featureBuffer);

                //+设置几何
                featureBuffer.Shape = iGeometry;
                featCursorInsert.InsertFeature(featureBuffer);

                lFeatureCount++;
                iOriFeat = featCursorCopy.NextFeature();
                if (lFeatureCount > 0 && (lFeatureCount % 1000) == 0)
                {
                    featCursorInsert.Flush();
                }
            }

            featCursorInsert.Flush();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featCursorInsert);

            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);
        }

        /// <summary>
        ///
        /// 功能描述:   要素删除
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatCursor"></param>
        public static void FeatureDelete(IFeatureCursor iFeatCursor)
        {
            IFeature iOriFeat;

            iOriFeat = iFeatCursor.NextFeature();
            if (iOriFeat == null)
            {
                return;
            }
            else
            {
                IFeatureClass iDestClass = iOriFeat.Class as IFeatureClass;
                IDataset dataset = (IDataset)iDestClass;
                IWorkspace workspace = dataset.Workspace;

                //Cast for an IWorkspaceEdit
                IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

                //Start an edit session and operation
                workspaceEdit.StartEditing(true);
                workspaceEdit.StartEditOperation();

                while (iOriFeat != null)
                {
                    iFeatCursor.DeleteFeature();
                    iOriFeat = iFeatCursor.NextFeature();
                }

                iFeatCursor.Flush();

                workspaceEdit.StopEditOperation();
                workspaceEdit.StopEditing(true);
            }
        }

        /// <summary>
        ///
        /// 功能描述:   要素删除
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatCursor"></param>
        public static void ClearFeatureClass(IFeatureCursor iFeatCursor)
        {
            IFeature iOriFeat;

            iOriFeat = iFeatCursor.NextFeature();
            if (iOriFeat == null)
            {
                return;
            }
            else
            {
                IFeatureClass iDestClass = iOriFeat.Class as IFeatureClass;
                IDataset dataset = (IDataset)iDestClass;
                IWorkspace workspace = dataset.Workspace;

                //Cast for an IWorkspaceEdit
                IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

                //Start an edit session and operation
                workspaceEdit.StartEditing(true);
                workspaceEdit.StartEditOperation();

                iOriFeat = iFeatCursor.NextFeature();
                while (iOriFeat != null)
                {
                    iFeatCursor.DeleteFeature();
                    iOriFeat = iFeatCursor.NextFeature();
                }
                iFeatCursor.Flush();                

                workspaceEdit.StopEditOperation();
                workspaceEdit.StopEditing(true);
            }
        }

        /// <summary>
        ///
        /// 功能描述:   创建初始化默认值的新对象
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="tFeatureLayer"></param>
        /// <returns></returns>
        public static IFeature NewFeatureDefault(IFeatureLayer tFeatureLayer)
        {
            IFeatureClass fclass = tFeatureLayer.FeatureClass;
            IFeature newfeat = fclass.CreateFeature();
            if (fclass.FeatureDataset != null)
            {
                IRowSubtypes rowSubTypes = newfeat as IRowSubtypes;
                rowSubTypes.InitDefaultValues();
            }

            return newfeat;
        }

        /// <summary>
        ///
        /// 功能描述:   创建初始化默认值的新对象
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="tFeatureClass"></param>
        /// <returns></returns>
        public static IFeature NewFeatureDefault(IFeatureClass tFeatureClass)
        {
            IFeature newfeat = tFeatureClass.CreateFeature();
            if (tFeatureClass.FeatureDataset != null)
            {
                IRowSubtypes rowSubTypes = newfeat as IRowSubtypes;
                rowSubTypes.InitDefaultValues();
            }

            return newfeat;
        }

        /// <summary>
        ///
        /// 功能描述:   根据字段名取得某一Feature的这一字段值
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="theFeat"></param>
        /// <param name="fldName"></param>
        /// <returns></returns>
        public static object GetAttributeByFieldName(IFeature theFeat, string fldName)
        {
            int idx = theFeat.Fields.FindField(fldName);
            if (idx >= 0)
                return theFeat.get_Value(idx);
            else
                return null;
        }

        /// <summary>
        ///
        /// 功能描述:   根据字段名取得某一FeatureBuffer的这一字段值
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="theFeatBuffer"></param>
        /// <param name="fldName"></param>
        /// <returns></returns>
        public static object GetAttributeByFieldName(IFeatureBuffer theFeatBuffer, string fldName)
        {
            int idx = theFeatBuffer.Fields.FindField(fldName);
            if (idx >= 0)
                return theFeatBuffer.get_Value(idx);
            else
                return null;
        }

        /// <summary>
        ///
        /// 功能描述:   给某一Cursor中所有feature的某字段赋值
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="theFeat"></param>
        /// <param name="fldName"></param>
        /// <param name="value"></param>
        public static void SetAttributeByFieldName(IFeatureCursor theFeatCursor, string fldName, object value)
        {
            int idx = theFeatCursor.Fields.FindField(fldName);
            if (idx >= 0)
            {
                try
                {
                    IFeature theFeat = theFeatCursor.NextFeature();

                    IFeatureClass iDestClass = theFeat.Class as IFeatureClass;
                    IDataset dataset = (IDataset)iDestClass;
                    IWorkspace workspace = dataset.Workspace;

                    //Cast for an IWorkspaceEdit
                    IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;
                    //Start an edit session and operation
                    workspaceEdit.StartEditing(true);
                    workspaceEdit.StartEditOperation();

                    while (theFeat != null)
                    {
                        theFeat.set_Value(idx, value);
                        theFeat.Store();

                        theFeat = theFeatCursor.NextFeature();
                    }

                    workspaceEdit.StopEditOperation();
                    workspaceEdit.StopEditing(true);
                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.SetAttributeByFieldName:" + ex.Message);
                    //throw ex;
                }
            }
        }

        /// <summary>
        ///
        /// 功能描述:   给某一feature某字段赋值
        /// 开发者:     方雷
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="theFeat"></param>
        /// <param name="fldName"></param>
        /// <param name="value"></param>
        public static void SetAttributeByFieldName(IFeature theFeat, string fldName, object value)
        {
            int idx = theFeat.Fields.FindField(fldName);
            if (idx >= 0)
            {
                try
                {
                    theFeat.set_Value(idx, value);
                    theFeat.Store();
                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.SetAttributeByFieldName:" + ex.Message);
                    //throw ex;
                }
            }
        }

        /// <summary>
        ///
        /// 功能描述:   给一个FeatureBuffer的某字段赋值
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        /// </summary>
        /// <param name="theFeatBuffer"></param>
        /// <param name="fldName"></param>
        /// <param name="value"></param>
        public static void SetAttributeByFieldName(IFeatureBuffer theFeatBuffer, string fldName, object value)
        {
            int idx = theFeatBuffer.Fields.FindField(fldName);
            if (idx >= 0)
            {
                try
                {
                    theFeatBuffer.set_Value(idx, value);
                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.SetAttributeByFieldName:" + ex.Message);
                    //throw ex;
                }
            }
        }

        /// <summary>
        ///
        /// 功能描述:   复制单个属性
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="oFeature"></param>
        /// <param name="oFld"></param>
        /// <param name="tFeature"></param>
        /// <param name="tFld"></param>
        public static void CopySingleAttribute(IFeature oFeature, string oFld, ref IFeature tFeature, string tFld)
        {
            int oIndex = oFeature.Fields.FindField(oFld);
            int tIndex = tFeature.Fields.FindField(tFld);
            if (oIndex >= 0 && tIndex >= 0)
            {
                tFeature.set_Value(tIndex, oFeature.get_Value(oIndex));
            }
        }

        /// <summary>
        ///
        /// 功能描述:   复制属性
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="oFeature"></param>
        /// <param name="tFeature"></param>
        public static void CopyAttribures(IFeature oFeature, ref IFeatureBuffer theFeatBuffer)
        {
            IFields tFields = oFeature.Fields;
            for (int i = 0; i < tFields.FieldCount; i++)
            {
                IField fld = tFields.get_Field(i);
                if (fld.Type != esriFieldType.esriFieldTypeGeometry &&
                    fld.Type != esriFieldType.esriFieldTypeOID &&
                    fld.Editable == true)
                {
                    theFeatBuffer.set_Value(i, oFeature.get_Value(i));
                }
            }
        }

        /// <summary>
        ///
        /// 功能描述:   复制属性到一个featureBuffer
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="oFeature"></param>
        /// <param name="tFeature"></param>
        public static void CopyAttribures(IFeature oFeature, ref IFeature tFeature)
        {
            IFields tFields = oFeature.Fields;
            for (int i = 0; i < tFields.FieldCount; i++)
            {
                IField fld = tFields.get_Field(i);
                if (fld.Type != esriFieldType.esriFieldTypeGeometry &&
                    fld.Type != esriFieldType.esriFieldTypeOID &&
                    fld.Editable == true)
                {
                    tFeature.set_Value(i, oFeature.get_Value(i));
                }
            }
        }

        /// <summary>
        ///
        /// 功能描述:   得到最大ID（以遍历的方法，效果比较低）
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iTable">查找的表</param>
        /// <param name="sFieldName">ID所在字段名 </param>
        /// <param name="sWhereClause">限制条件（可以不设）</param>
        /// <returns>查找到的最大ID</returns>
        public static int GetTabMaxIDByFldName(ITable iTable, string sFieldName, string sWhereClause)
        {
            IQueryFilter iQueryFilter = new QueryFilter();
            ICursor iCursor = null;
            int lIndex = 1;
            IRow iRow = null;

            iQueryFilter.SubFields = sFieldName;
            if (!sWhereClause.Equals(""))
            {
                iQueryFilter.WhereClause = sWhereClause;
            }
            lIndex = iTable.FindField(sFieldName);
            iCursor = iTable.Search(iQueryFilter, false);
            iRow = iCursor.NextRow();

            int maxID = 0, curID = 0;
            while (iRow != null)
            {
                int.TryParse(iRow.get_Value(lIndex).ToString(), out curID);
                if (curID > maxID)
                {
                    maxID = curID;
                }
                iRow = iCursor.NextRow();
            }

            return maxID;
        }

        #endregion

        #region About network analysis

        ///<summary>打开并返回网络数据集.</summary>
        ///
        ///<param name="networkDatasetWorkspace">An IWorkspace interface that contains the network dataset</param>
        ///<param name="networkDatasetName">A System.String that is the name of the network dataset. Example: "roads"</param>
        ///<param name="featureDatasetName">A System.String that is the name of the feature dataset that contains the network dataset. This name is only required for geodatabase workspaces. An empty string may be passed in for shapefile/SDC workspaces. Example: "Highways" or "".</param>
        ///
        ///<returns>The INetworkDataset interface of the opened network dataset</returns>
        /// 
        ///<remarks></remarks>
        public static ESRI.ArcGIS.Geodatabase.INetworkDataset OpenNetworkDataset(ESRI.ArcGIS.Geodatabase.IWorkspace networkDatasetWorkspace, System.String networkDatasetName, System.String featureDatasetName)
        {
            if (networkDatasetWorkspace == null || networkDatasetName == "" || featureDatasetName == null)
            {
                return null;
            }

            try
            {
                ESRI.ArcGIS.Geodatabase.IDatasetContainer3 datasetContainer3 = null;
                switch (networkDatasetWorkspace.Type)
                {
                    case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriFileSystemWorkspace:

                        // Shapefile or SDC network dataset workspace
                        ESRI.ArcGIS.Geodatabase.IWorkspaceExtensionManager workspaceExtensionManager = networkDatasetWorkspace as ESRI.ArcGIS.Geodatabase.IWorkspaceExtensionManager; // Dynamic Cast
                        ESRI.ArcGIS.esriSystem.UID networkID = new ESRI.ArcGIS.esriSystem.UIDClass();

                        networkID.Value = "esriGeoDatabase.NetworkDatasetWorkspaceExtension";
                        ESRI.ArcGIS.Geodatabase.IWorkspaceExtension workspaceExtension = workspaceExtensionManager.FindExtension(networkID);
                        datasetContainer3 = workspaceExtension as ESRI.ArcGIS.Geodatabase.IDatasetContainer3; // Dynamic Cast
                        break;

                    case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriLocalDatabaseWorkspace:

                    // Personal Geodatabase or File Geodatabase network dataset workspace

                    case ESRI.ArcGIS.Geodatabase.esriWorkspaceType.esriRemoteDatabaseWorkspace:

                        // SDE Geodatabase network dataset workspace
                        ESRI.ArcGIS.Geodatabase.IFeatureWorkspace featureWorkspace = networkDatasetWorkspace as ESRI.ArcGIS.Geodatabase.IFeatureWorkspace; // Dynamic Cast
                        ESRI.ArcGIS.Geodatabase.IFeatureDataset featureDataset = featureWorkspace.OpenFeatureDataset(featureDatasetName);
                        ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtensionContainer featureDatasetExtensionContainer = featureDataset as ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtensionContainer; // Dynamic Cast
                        ESRI.ArcGIS.Geodatabase.IFeatureDatasetExtension featureDatasetExtension = featureDatasetExtensionContainer.FindExtension(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset);
                        datasetContainer3 = featureDatasetExtension as ESRI.ArcGIS.Geodatabase.IDatasetContainer3; // Dynamic Cast
                        break;
                }

                if (datasetContainer3 == null)
                    return null;

                ESRI.ArcGIS.Geodatabase.IDataset dataset = datasetContainer3.get_DatasetByName(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTNetworkDataset, networkDatasetName);

                return dataset as ESRI.ArcGIS.Geodatabase.INetworkDataset; // Dynamic Cast
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("OpenNetworkDataset failed", ex);
            }
            return null;            
        }

        ///<summary>Set OD cost matrix solver parameters, including settings.</summary>
        /// 
        ///<param name="naSolver">An INASolver interface.</param>
        ///<param name="defaultCutoff">A System.Object that is the default cutoff value to stop traversing. Ex: Nothing (VBNet) or null (C#)</param>
        ///<param name="defaultTargetDestinationCount">A System.Object that is the default number of destinations to find. Ex: Nothing (VBNet) or null (C#)</param>
        ///
        ///<returns>An INAODCostMatrixSolver2 with default parameters set.</returns>
        ///
        ///<remarks>Solving for 10 destinations will return the cost-distance to the 10 closest destinations from each origin. If this value is Nothing or null, all destinations will be found.</remarks>
        public static ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver2 SetODCostMatrixProperties(ESRI.ArcGIS.NetworkAnalyst.INASolver naSolver, object defaultCutoff, object defaultTargetDestinationCount)
        {

            // Set OD cost matrix solver parameters, including settings for...
            ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver2 naODCostMatrixSolver = (ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver2)naSolver;

            // ...default cutoff
            naODCostMatrixSolver.DefaultCutoff = defaultCutoff;

            // ...number of destinations to find
            naODCostMatrixSolver.DefaultTargetDestinationCount = defaultTargetDestinationCount;

            // ...output
            naODCostMatrixSolver.OutputLines = ESRI.ArcGIS.NetworkAnalyst.esriNAOutputLineType.esriNAOutputLineStraight;
            naODCostMatrixSolver.MatrixResultType = ESRI.ArcGIS.NetworkAnalyst.esriNAODCostMatrixType.esriNAODCostMatrixFull;
            naODCostMatrixSolver.PopulateODLines = false;

            return naODCostMatrixSolver;

        }

        ///<summary>创建一个新的 OD 成本矩阵图层 OD cost matrix layer.</summary>
        ///  
        ///<param name="networkDataset">An INetworkDataset interface that is the network dataset on which to perform the OD cost matrix analysis.</param>
        ///  
        ///<returns>An INALayer3 interface that is the newly created network analysis layer.</returns>
        public static ESRI.ArcGIS.NetworkAnalyst.INALayer3 CreateODCostMatrixLayer(ESRI.ArcGIS.Geodatabase.INetworkDataset networkDataset)
        {
            ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver naAODCostMatrixSolver = new ESRI.ArcGIS.NetworkAnalyst.NAODCostMatrixSolverClass();
            ESRI.ArcGIS.NetworkAnalyst.INASolver naSolver = naAODCostMatrixSolver as ESRI.ArcGIS.NetworkAnalyst.INASolver;

            ESRI.ArcGIS.Geodatabase.IDatasetComponent datasetComponent = networkDataset as ESRI.ArcGIS.Geodatabase.IDatasetComponent; // Dynamic Cast
            ESRI.ArcGIS.Geodatabase.IDENetworkDataset deNetworkDataset = datasetComponent.DataElement as ESRI.ArcGIS.Geodatabase.IDENetworkDataset; // Dynamic Cast
            ESRI.ArcGIS.NetworkAnalyst.INAContext naContext = naSolver.CreateContext(deNetworkDataset, naSolver.Name);
            ESRI.ArcGIS.NetworkAnalyst.INAContextEdit naContextEdit = naContext as ESRI.ArcGIS.NetworkAnalyst.INAContextEdit; // Dynamic Cast

            ESRI.ArcGIS.Geodatabase.IGPMessages gpMessages = new ESRI.ArcGIS.Geodatabase.GPMessagesClass();
            naContextEdit.Bind(networkDataset, gpMessages);

            ESRI.ArcGIS.NetworkAnalyst.INALayer naLayer = naSolver.CreateLayer(naContext);
            ESRI.ArcGIS.NetworkAnalyst.INALayer3 naLayer3 = naLayer as ESRI.ArcGIS.NetworkAnalyst.INALayer3;
            
            return naLayer3;
        }

        #endregion

        #region "Open :Feature FeatureClass table Raster"

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWorkSpace"></param>
        /// <param name="featureClassName"></param>
        /// <returns></returns>
        public static IFeatureLayer OpenFeatureLayer(IFeatureWorkspace iFeatWorkSpace, string featureClassName)
        {
            IFeatureClass featureClass = null;

            try
            {
                featureClass = iFeatWorkSpace.OpenFeatureClass(featureClassName);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.OpenFeatureLayer:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }

            IFeatureLayer iLayer = new FeatureLayerClass();

            iLayer.FeatureClass = featureClass;

            return iLayer;
        }

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWorkSpace"></param>
        /// <param name="featureClassName"></param>
        /// <returns></returns>
        public static IFeatureLayer OpenFeatureLayer(IWorkspace ws, string featureClassName)
        {
            return OpenFeatureLayer(ws as IFeatureWorkspace, featureClassName);
        }

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sFeatClassName"></param>
        /// <returns></returns>
        public static IFeatureClass OpenFeatClass(IFeatureWorkspace iFeatWS, string sFeatClassName)
        {
            IFeatureClass iFeatClass;

            try
            {
                iFeatClass = iFeatWS.OpenFeatureClass(sFeatClassName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
                return null;
            }

            return iFeatClass;
        }

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featWS"></param>
        /// <param name="lyrName"></param>
        /// <returns></returns>
        public static IFeatureClass OpenFeatClass(IWorkspace featWS, String lyrName)
        {
            return OpenFeatClass(featWS as IFeatureWorkspace, lyrName);
        }

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featWS"></param>
        /// <param name="lyrName"></param>
        /// <returns></returns>
        public static IFeatureLayer OpenFeatLayer(IWorkspace featWS, String lyrName)
        {
            IFeatureLayer featLayer = new FeatureLayerClass();
            featLayer.FeatureClass = OpenFeatClass(featWS, lyrName);
            featLayer.Name = featLayer.FeatureClass.AliasName + "_" + lyrName;

            return featLayer;
        }


        public static IFeatureLayer OpenFeatLayer(IFeatureClass ipFeaCls)
        {
            IFeatureLayer feaLayer = new FeatureLayerClass();
            feaLayer.FeatureClass = ipFeaCls;
            feaLayer.Name = ipFeaCls.AliasName + "_" + GetDataSetName(ipFeaCls);

            return feaLayer;
        }
        ///// <summary>
        /////
        ///// 功能描述:
        ///// 开发者:     XXX
        ///// 建立时间:   08-01-15 0:00:00
        /////
        ///// </summary>
        ///// <param name="featClass"></param>
        ///// <returns></returns>
        //public static IFeatureLayer OpenFeatLayer(IFeatureClass featClass)
        //{
        //    IFeatureLayer featLayer = new FeatureLayerClass();
        //    featLayer.FeatureClass = featClass;
        //    return featLayer;
        //}

        /// <summary>
        ///
        /// 功能描述:
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="featLayers"></param>
        /// <returns></returns>
        public static ILayer GetLayerFromLayerList(string layerName, IList<IFeatureLayer> featLayers)
        {
            ILayer layer = null;

            foreach (IFeatureLayer featLayer in featLayers)
            {
                if (featLayer.FeatureClass.AliasName == layerName)
                {
                    layer = featLayer as ILayer;
                }
            }
            return layer;
        }

        #endregion

        #region "Version"

        /// <summary>
        ///
        /// 功能描述:   Register this object as versioned with the option to move edits to base
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dataset"></param>
        public static void RegisterDatasetAsFullyVersioned(IDataset dataset)
        {
            IVersionedObject3 versionedObject3 = (IVersionedObject3)dataset;
            bool IsRegistered;
            bool IsMovingEditsToBase;

            versionedObject3.GetVersionRegistrationInfo(out IsRegistered, out IsMovingEditsToBase);
            if (IsRegistered)
            {
                if (IsMovingEditsToBase)
                {
                    try
                    {
                        //first unregister without compressing edits
                        versionedObject3.UnRegisterAsVersioned3(false);
                        //then register as fully versioned
                        versionedObject3.RegisterAsVersioned3(false);
                    }
                    catch (Exception ex)
                    {
                        // // LogHelper.LogHelper.("GeodatabaseOp.RegisterDatasetAsFullyVersioned:" + ex.Message);
                        //throw ex;
                    }
                }
                else
                {
                    IVersionedObject versionedObject = (IVersionedObject)dataset;
                    try
                    {
                        if (versionedObject.IsRegisteredAsVersioned)
                            versionedObject.RegisterAsVersioned(false);
                        versionedObject3.RegisterAsVersioned3(false);
                    }
                    catch (Exception ex)
                    {
                        // // LogHelper.LogHelper.("GeodatabaseOp.RegisterDatasetAsFullyVersioned:" + ex.Message);
                        //throw ex;
                    }
                }
            }
            else
            {
                try
                {
                    //registering as fully versioned
                    versionedObject3.RegisterAsVersioned3(false);
                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.RegisterDatasetAsFullyVersioned:" + ex.Message);
                    //throw ex;
                }
            }
        }

        public static void RegisterDatasetAsFullyVersioned(IDataset dataset, bool movingEditsToBase)
        {
            IVersionedObject3 versionedObject3 = (IVersionedObject3)dataset;
            bool IsRegistered;
            bool IsMovingEditsToBase;

            versionedObject3.GetVersionRegistrationInfo(out IsRegistered, out IsMovingEditsToBase);
            if (IsRegistered)
            {
                return;
            }
            else
            {
                versionedObject3.RegisterAsVersioned3(movingEditsToBase);
            }
        }

        /// <summary>
        ///
        /// 功能描述:   Register this object as versioned with the option to move edits to base
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sPrefix"></param>
        /// <param name="nLibID"></param>
        public static void RegisterDatasetAsFullyVersioned(IFeatureWorkspace iFeatWS, string sPrefix, int nLibID)
        {
            if (iFeatWS == null)
            {
                throw new ApplicationException("指针接口为空\nError in RegisterDatasetAsFullyVersioned");
            }

            IFeatureDataset iCurFeatDataset;
            string datasetName;

            datasetName = String.Format("{0}GIS{1}", sPrefix, nLibID);

            try
            {
                iCurFeatDataset = iFeatWS.OpenFeatureDataset(datasetName);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.RegisterDatasetAsFullyVersioned:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n图库数据集无法获取，注册失败。");
            }

            try
            {
                RegisterDatasetAsFullyVersioned(iCurFeatDataset as IDataset);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.RegisterDatasetAsFullyVersioned:" + ex.Message);
                //throw ex;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   UnRegister this object as versioned with the option to compress the Default edits to base
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dataset"></param>
        public static void UnRegisterDatasetAsFullyVersioned(IDataset dataset)
        {
            IVersionedObject3 versionedObject3 = (IVersionedObject3)dataset;
            bool IsRegistered;
            bool IsMovingEditsToBase;

            versionedObject3.GetVersionRegistrationInfo(out IsRegistered, out IsMovingEditsToBase);
            if (IsRegistered)
            {
                if (IsMovingEditsToBase)
                {
                    try
                    {
                        //first unregister without compressing edits
                        versionedObject3.UnRegisterAsVersioned3(false);
                    }
                    catch (Exception ex)
                    {
                        // // LogHelper.LogHelper.("GeodatabaseOp.UnRegisterDatasetAsFullyVersioned:" + ex.Message);
                        //throw ex;
                    }
                }
                else
                {
                    IVersionedObject versionedObject = (IVersionedObject)dataset;
                    try
                    {
                        if (versionedObject.IsRegisteredAsVersioned)
                            versionedObject.RegisterAsVersioned(false);
                    }
                    catch (Exception ex)
                    {
                        // // LogHelper.LogHelper.("GeodatabaseOp.UnRegisterDatasetAsFullyVersioned:" + ex.Message);
                        //throw ex;
                    }
                }
            }
            else
            {
                try
                {
                    //first registering as fully versioned
                    versionedObject3.RegisterAsVersioned3(false);
                    // unregister without compressing edits
                    versionedObject3.UnRegisterAsVersioned3(false);

                }
                catch (Exception ex)
                {
                    // // LogHelper.LogHelper.("GeodatabaseOp.UnRegisterDatasetAsFullyVersioned:" + ex.Message);
                    //throw ex;
                }
            }
        }

        public static void UnRegisterDatasetAsFullyVersioned(IDataset dataset, bool compressToDefault)
        {
            IVersionedObject3 versionedObject3 = (IVersionedObject3)dataset;
            bool IsRegistered;
            bool IsMovingEditsToBase;

            versionedObject3.GetVersionRegistrationInfo(out IsRegistered, out IsMovingEditsToBase);
            if (IsRegistered)
            {
                versionedObject3.UnRegisterAsVersioned3(compressToDefault);
            }
            else
            {
                return;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   UnRegister this object as versioned with the option to compress the Default edits to base
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sPrefix"></param>
        /// <param name="nLibID"></param>
        public static void UnRegisterDatasetAsFullyVersioned(IFeatureWorkspace iFeatWS, string sPrefix, int nLibID)
        {
            if (iFeatWS == null)
            {
                throw new ApplicationException("指针接口为空\nError in UnRegisterDatasetAsFullyVersioned");
            }

            IFeatureDataset iCurFeatDataset;
            string datasetName;

            datasetName = String.Format("{0}GIS{1}", sPrefix, nLibID);

            try
            {
                iCurFeatDataset = iFeatWS.OpenFeatureDataset(datasetName);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.UnRegisterDatasetAsFullyVersioned:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n图库数据集无法获取，反注册失败。");
            }

            try
            {
                UnRegisterDatasetAsFullyVersioned(iCurFeatDataset as IDataset);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.UnRegisterDatasetAsFullyVersioned:" + ex.Message);
                //throw ex;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   Indicates if the object is registered as versioned
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sPrefix"></param>
        /// <param name="nLibID"></param>
        /// <returns></returns>
        public static bool IsRegisteredAsVersioned(IFeatureWorkspace iFeatWS, string sPrefix, int nLibID)
        {
            if (iFeatWS == null)
            {
                throw new ApplicationException("指针接口为空\nError in IsRegisteredAsVersioned");
            }

            IFeatureDataset iCurFeatDataset;
            string datasetName;

            datasetName = String.Format("{0}GIS{1}", sPrefix, nLibID);

            try
            {
                iCurFeatDataset = iFeatWS.OpenFeatureDataset(datasetName);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.IsRegisteredAsVersioned:" + ex.Message);
                throw new ApplicationException(ex.Message + "\n\n图库数据集无法获取，获得注册信息失败。");
            }

            try
            {
                return IsRegisteredAsVersioned(iCurFeatDataset as IDataset);
            }
            catch (Exception ex)
            {
                // // LogHelper.LogHelper.("GeodatabaseOp.IsRegisteredAsVersioned:" + ex.Message);
                throw ex;
            }
        }

        /// <summary>
        ///
        /// 功能描述:   Indicates if the object is registered as versioned
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static bool IsRegisteredAsVersioned(IDataset dataset)
        {
            IVersionedObject3 versionedObject3 = (IVersionedObject3)dataset;
            bool IsRegistered;
            bool IsMovingEditsToBase;

            versionedObject3.GetVersionRegistrationInfo(out IsRegistered, out IsMovingEditsToBase);

            return IsRegistered;
        }

        #endregion

        #region "query and spatial query"

        /// <summary>
        ///
        /// 功能描述:   Table查询
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="table"></param>
        /// <param name="whereClause"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static ICursor QuerySearch(ITable table, String whereClause, String subFields, bool recycling)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = whereClause;
            if (subFields != "")
            {
                queryFilter.SubFields = subFields;
            }

            return table.Search(queryFilter, recycling);
        }

        /// <summary>
        ///
        /// 功能描述:   Table查询更新
        /// 开发者:     XXX
        /// 建立时间:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="table"></param>
        /// <param name="whereClause"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static ICursor QuerySearchforUpdate(ITable table, String whereClause, String subFields, bool recycling)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = whereClause;
            if (subFields != "")
            {
                queryFilter.SubFields = subFields;
            }

            return table.Update(queryFilter, recycling);
        }

        /// <summary>
        ///
        /// 功能描述:   FeatureClass查询
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featClass"></param>
        /// <param name="whereClause"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static IFeatureCursor QuerySearch(IFeatureClass featClass, String whereClause, String subFields, bool recycling)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = whereClause;
            if (subFields != "")
            {
                queryFilter.SubFields = subFields;
            }

            return featClass.Search(queryFilter, recycling);
        }

        /// <summary>
        ///
        /// 功能描述:   FeatureClass查询更新
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featClass"></param>
        /// <param name="whereClause"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static IFeatureCursor QuerySearchforUpdate(IFeatureClass featClass, String whereClause, String subFields, bool recycling)
        {
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = whereClause;
            if (subFields != "")
            {
                queryFilter.SubFields = subFields;
            }

            return featClass.Update(queryFilter, recycling);
        }

        /// <summary>
        ///
        /// 功能描述:   空间查询
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featClass"></param>
        /// <param name="geo"></param>
        /// <param name="spatialRel"></param>
        /// <param name="whereClause"></param>
        /// <param name="searchOrder"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static IFeatureCursor SpatialRelQurey(IFeatureClass featClass, IGeometry geo, esriSpatialRelEnum spatialRel,
                                                String whereClause, esriSearchOrder searchOrder, String subFields, bool recycling)
        {
            try
            {
                ISpatialFilter spatialFilter = new SpatialFilterClass();

                //图形simplyfy
                ITopologicalOperator topo = geo as ITopologicalOperator;
                topo.Simplify();

                //spatialFilter.Geometry = geo;
                spatialFilter.Geometry = topo as IGeometry;
                spatialFilter.GeometryField = featClass.ShapeFieldName;
                spatialFilter.SpatialRel = spatialRel;
                if (whereClause != "")
                {
                    spatialFilter.WhereClause = whereClause;
                    spatialFilter.SearchOrder = searchOrder;
                }
                if (subFields != "")
                {
                    spatialFilter.SubFields = subFields;
                }

                return featClass.Search(spatialFilter, recycling);
            }
            catch
            {
                return null;
            }
        }

        public static int FeatCountBySpatialRel(IFeatureClass featClass, IGeometry geo, esriSpatialRelEnum spatialRel,
                                               String whereClause, esriSearchOrder searchOrder, String subFields, bool recycling)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = geo;
            spatialFilter.GeometryField = featClass.ShapeFieldName;
            spatialFilter.SpatialRel = spatialRel;
            if (whereClause != "")
            {
                spatialFilter.WhereClause = whereClause;
                spatialFilter.SearchOrder = searchOrder;
            }
            if (subFields != "")
            {
                spatialFilter.SubFields = subFields;
            }

            return featClass.FeatureCount(spatialFilter as IQueryFilter);
        }

        /// <summary>
        /// 排除esriGeometry0Dimension的那种情况
        /// </summary>
        /// <param name="featClass"></param>
        /// <param name="geo"></param>
        /// <param name="spatialRel"></param>
        /// <param name="whereClause"></param>
        /// <param name="searchOrder"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>

        public static int FeatCountBySpatialRelTouches(IFeatureClass featClass, IGeometry geo, String whereClause,
                                                        esriSearchOrder searchOrder, String subFields, bool recycling)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = geo;
            spatialFilter.GeometryField = featClass.ShapeFieldName;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches;
            if (whereClause != "")
            {
                spatialFilter.WhereClause = whereClause;
                spatialFilter.SearchOrder = searchOrder;
            }
            if (subFields != "")
            {
                spatialFilter.SubFields = subFields;
            }

            IFeatureCursor featureCursor = featClass.Search(spatialFilter, recycling);

            IGeometry geoTouch = null;
            IGeometry geoTemp = null;
            int num = 0;
            ITopologicalOperator topologicalOp = geo as ITopologicalOperator;

            IFeature feat = featureCursor.NextFeature();
            while (feat != null)
            {
                geoTouch = feat.ShapeCopy;

                geoTemp = topologicalOp.Intersect(geoTouch, esriGeometryDimension.esriGeometry1Dimension);
                if (geoTemp != null && !geoTemp.IsEmpty)
                {
                    num++;
                }
                feat = featureCursor.NextFeature();
            }
            //入库时出现异常，尝试释放cursor是否有改善 by zy 2014.6.24
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featureCursor);
            return num;
        }

        /// <summary>
        ///
        /// 功能描述:   空间查询更新
        /// 开发者:     XXX
        /// 建立时间:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="featClass"></param>
        /// <param name="geo"></param>
        /// <param name="spatialRel"></param>
        /// <param name="whereClause"></param>
        /// <param name="searchOrder"></param>
        /// <param name="subFields"></param>
        /// <param name="recycling"></param>
        /// <returns></returns>
        public static IFeatureCursor SpatialRelQureyforUpdate(IFeatureClass featClass, IGeometry geo, esriSpatialRelEnum spatialRel,
                                        String whereClause, esriSearchOrder searchOrder, String subFields, bool recycling)
        {
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = geo;
            spatialFilter.GeometryField = featClass.ShapeFieldName;
            spatialFilter.SpatialRel = spatialRel;
            if (whereClause != "")
            {
                spatialFilter.WhereClause = whereClause;
                spatialFilter.SearchOrder = searchOrder;
            }
            if (subFields != "")
            {
                spatialFilter.SubFields = subFields;
            }

            return featClass.Update(spatialFilter, recycling);
        }

        #endregion
    
        #region added by yinth

        public static string GetDataSetBrowseName(IFeatureClass feaCls)
        {
            if (feaCls == null)
            {
                return "";
            }
            IDataset ipDt = feaCls as IDataset;
            return ipDt.BrowseName;
        }
        /// <summary>
        /// 获取featureclass的name，不包含SDE.
        /// </summary>
        /// <param name="feaCls"></param>
        /// <returns></returns>
        public static string GetDataSetName(IFeatureClass feaCls)
        {
            if (feaCls == null)
            {
                return "";
            }
            IDataset ipDt = feaCls as IDataset;
            string strDTName = ipDt.Name;
            if (strDTName.Length < 5)
            {
                return strDTName;
            }
            else if (strDTName.Substring(0, 4).ToUpper().Equals("SDE."))
            {
                strDTName = strDTName.Substring(4, strDTName.Length - 4);
            }
            return strDTName;
        }
        #endregion

        #region Added by hero
        /// <summary>
        /// 功能：复制一个feature的属性到另一个feature
        /// 时间：2011-12-08
        /// 作者：何榕健
        /// </summary>
        /// <param name="featCursorCopy">源feature的集合</param>
        /// <param name="iDestClass">目标featureClass</param>
        /// <param name="IsSameStructure">源与目标feature的字段结果是否相同</param>

        public static void FeatureCopy(IFeatureCursor featCursorCopy, IFeatureClass iDestClass, bool IsSameStructure)
        {
            IDataset dataset = (IDataset)iDestClass;
            IWorkspace workspace = dataset.Workspace;

            //Cast for an IWorkspaceEdit
            IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)workspace;

            //Start an edit session and operation
            workspaceEdit.StartEditing(true);
            workspaceEdit.StartEditOperation();

            IGeometry iGeometry;
            IFeature iOriFeat;
            IFeatureCursor featCursorInsert = iDestClass.Insert(true);

            iOriFeat = featCursorCopy.NextFeature();
            while (iOriFeat != null)
            {
                IFeatureBuffer featureBuffer = iDestClass.CreateFeatureBuffer();
                //+获得几何拷贝
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                if (IsSameStructure)
                {
                    //+属性拷贝
                    CopyAttribures(iOriFeat, ref featureBuffer);
                }
                else
                {
                    CopyAttriburesEx(iOriFeat, ref featureBuffer);
                }


                //+设置几何
                featureBuffer.Shape = iGeometry;
                featCursorInsert.InsertFeature(featureBuffer);

                iOriFeat = featCursorCopy.NextFeature();
            }

            featCursorInsert.Flush();

            workspaceEdit.StopEditOperation();
            workspaceEdit.StopEditing(true);

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workspaceEdit);//test
        }

        /// <summary>
        /// 功能：复制数据字段结构不一的两个feature属性
        /// 时间：2011-12-08
        /// </summary>
        /// <param name="oFeature"></param>
        /// <param name="theFeatBuffer"></param>
        public static void CopyAttriburesEx(IFeature oFeature, ref IFeatureBuffer theFeatBuffer)
        {
            IFields oFields = oFeature.Fields;
            int scrIndex;
            string tFieldName;
            for (int i = 0; i < oFields.FieldCount; i++)
            {
                IField fld = oFields.get_Field(i);
                if (fld.Name == "SHAPE.AREA" || fld.Name == "SHAPE.LEN")
                {
                    tFieldName = fld.Name.Replace(".", "_");
                }
                else
                {
                    tFieldName = fld.Name;
                }
                scrIndex = theFeatBuffer.Fields.FindField(tFieldName);

                if (scrIndex != -1)
                {
                    theFeatBuffer.set_Value(scrIndex, oFeature.get_Value(i));
                }
            }
        }
        #endregion
    }
}
