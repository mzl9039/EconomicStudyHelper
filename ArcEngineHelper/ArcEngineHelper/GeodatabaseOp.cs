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
    /// ��������:   ��̬�࣬�����Geodatabase ��ص����в���
    ///             ����������sde������workspace������featureclass��table�������ֶΣ���ѯ�����µȵ�
    /// ������:     XXX
    /// ����ʱ��:   2008-10-8 0:00:00
    /// �޶�����:
    /// ��������:
    /// �汾��      :    1.0
    /// ����޸�ʱ��:    2008-10-7 13:36:48
    /// </summary>
    public partial class GeodatabaseOp
    {
        #region "Operation About Feature and Fields Related"

        /// <summary>
        ///
        /// ��������:   ��ĳ��Ҫ�ش�ŵ�ĳ��ͼ����
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="featLayer">Ŀ��ͼ��</param>
        /// <param name="feat">ԴҪ��</param>
        /// <returns>��</returns>
        public static void CreateFeature(IFeatureLayer featLayer, IFeature featSource)
        {
            CreateFeature(featLayer.FeatureClass, featSource);
        }

        /// <summary>
        ///
        /// ��������:   ��ĳ��Ҫ�ش�ŵ�ĳ��ͼ����
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
        ///
        /// </summary>
        /// <param name="featClass">Ŀ��ͼ��</param>
        /// <param name="featSource">ԴҪ��</param>
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
        /// ��������:   Ҫ�ش�ԭͼ���ƶ�����Ŀ��iDestClass
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
                //+��ü��ο���
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                //+���Կ���
                CopyAttribures(iOriFeat, ref featureBuffer);

                //+���ü���
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
        /// ��������:   Ҫ�ظ��Ƶ�Ŀ��iDestClass
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
                //+��ü��ο���
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                //+���Կ���
                CopyAttribures(iOriFeat, ref featureBuffer);

                //+���ü���
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
        /// ��������:   Ҫ��ɾ��
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   Ҫ��ɾ��
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   ������ʼ��Ĭ��ֵ���¶���
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   ������ʼ��Ĭ��ֵ���¶���
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   �����ֶ���ȡ��ĳһFeature����һ�ֶ�ֵ
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   �����ֶ���ȡ��ĳһFeatureBuffer����һ�ֶ�ֵ
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   ��ĳһCursor������feature��ĳ�ֶθ�ֵ
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   ��ĳһfeatureĳ�ֶθ�ֵ
        /// ������:     ����
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   ��һ��FeatureBuffer��ĳ�ֶθ�ֵ
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   ���Ƶ�������
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   ��������
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   �������Ե�һ��featureBuffer
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   �õ����ID���Ա����ķ�����Ч���Ƚϵͣ�
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iTable">���ҵı�</param>
        /// <param name="sFieldName">ID�����ֶ��� </param>
        /// <param name="sWhereClause">�������������Բ��裩</param>
        /// <returns>���ҵ������ID</returns>
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

        ///<summary>�򿪲������������ݼ�.</summary>
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

        ///<summary>����һ���µ� OD �ɱ�����ͼ�� OD cost matrix layer.</summary>
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        ///// ��������:
        ///// ������:     XXX
        ///// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   Register this object as versioned with the option to move edits to base
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   Register this object as versioned with the option to move edits to base
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sPrefix"></param>
        /// <param name="nLibID"></param>
        public static void RegisterDatasetAsFullyVersioned(IFeatureWorkspace iFeatWS, string sPrefix, int nLibID)
        {
            if (iFeatWS == null)
            {
                throw new ApplicationException("ָ��ӿ�Ϊ��\nError in RegisterDatasetAsFullyVersioned");
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
                throw new ApplicationException(ex.Message + "\n\nͼ�����ݼ��޷���ȡ��ע��ʧ�ܡ�");
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
        /// ��������:   UnRegister this object as versioned with the option to compress the Default edits to base
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   UnRegister this object as versioned with the option to compress the Default edits to base
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
        ///
        /// </summary>
        /// <param name="iFeatWS"></param>
        /// <param name="sPrefix"></param>
        /// <param name="nLibID"></param>
        public static void UnRegisterDatasetAsFullyVersioned(IFeatureWorkspace iFeatWS, string sPrefix, int nLibID)
        {
            if (iFeatWS == null)
            {
                throw new ApplicationException("ָ��ӿ�Ϊ��\nError in UnRegisterDatasetAsFullyVersioned");
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
                throw new ApplicationException(ex.Message + "\n\nͼ�����ݼ��޷���ȡ����ע��ʧ�ܡ�");
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
        /// ��������:   Indicates if the object is registered as versioned
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
                throw new ApplicationException("ָ��ӿ�Ϊ��\nError in IsRegisteredAsVersioned");
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
                throw new ApplicationException(ex.Message + "\n\nͼ�����ݼ��޷���ȡ�����ע����Ϣʧ�ܡ�");
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
        /// ��������:   Indicates if the object is registered as versioned
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   Table��ѯ
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   Table��ѯ����
        /// ������:     XXX
        /// ����ʱ��:   2008-10-11 0:00:00
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
        /// ��������:   FeatureClass��ѯ
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   FeatureClass��ѯ����
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��������:   �ռ��ѯ
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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

                //ͼ��simplyfy
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
        /// �ų�esriGeometry0Dimension���������
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
            //���ʱ�����쳣�������ͷ�cursor�Ƿ��и��� by zy 2014.6.24
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featureCursor);
            return num;
        }

        /// <summary>
        ///
        /// ��������:   �ռ��ѯ����
        /// ������:     XXX
        /// ����ʱ��:   08-01-15 0:00:00
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
        /// ��ȡfeatureclass��name��������SDE.
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
        /// ���ܣ�����һ��feature�����Ե���һ��feature
        /// ʱ�䣺2011-12-08
        /// ���ߣ����Ž�
        /// </summary>
        /// <param name="featCursorCopy">Դfeature�ļ���</param>
        /// <param name="iDestClass">Ŀ��featureClass</param>
        /// <param name="IsSameStructure">Դ��Ŀ��feature���ֶν���Ƿ���ͬ</param>

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
                //+��ü��ο���
                iGeometry = iOriFeat.ShapeCopy;
                if (iGeometry == null)
                    continue;

                if (IsSameStructure)
                {
                    //+���Կ���
                    CopyAttribures(iOriFeat, ref featureBuffer);
                }
                else
                {
                    CopyAttriburesEx(iOriFeat, ref featureBuffer);
                }


                //+���ü���
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
        /// ���ܣ����������ֶνṹ��һ������feature����
        /// ʱ�䣺2011-12-08
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
