using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;


namespace MapEdit
{
    /// <summary>
    ///
    /// 功能描述:   Map的地图数据编辑操作类，包括图形绘制;要素创建、更新、删除以及回退版本管理
    ///             说明:这里IWorkspaceEdit管理数据，对于GeoDatabase的数据必须注册为版本才支持Edit Session；否则delete 数据直接更改数据并且不可回退
    /// 开发者:     XXX
    /// 建立时间:   2008-11-02 19:00:00
    /// 修订描述:
    /// 进度描述:
    /// 版本号      :    1.0
    /// 最后修改时间:    2008-10-7 13:36:48
    ///
    /// </summary>
    public class MapEditEnvOp
    {
        #region "公共字段和初始化环境"

        public static IWorkspace m_workSpace = null;
        public static IFeatureClass m_featClassEdit = null;

        //来自Editor的属性
        private static bool m_IsEdited = false;                 //是否在编辑状态？

        //来自Editor的属性设置
        #region 属性

        /// <summary>
        /// 判断是否处以编辑状态
        /// </summary>
        public static bool IsEditing
        {
            get { return m_IsEdited; }
            set { m_IsEdited = value; }
        }
        #endregion

        public static void InitalMapEditEvn(IWorkspace workspace, IFeatureClass featureClass)
        {
            if (workspace == null || featureClass == null)
                return;

            m_workSpace = workspace;
            m_featClassEdit = featureClass;
        }

        /// <summary>
        /// 设置编辑图层
        /// </summary>
        /// <param name="featLayer">图层</param>
        /// <returns></returns>
        public static bool SetEditFeatureClass(IFeatureClass featClass)
        {
            if (featClass == null)
            {
                return false;
            }

            m_featClassEdit = featClass;

            return true;
        }

        public static bool IsMapEditEnvValidAfterInitial()
        {
            if (m_workSpace == null || m_featClassEdit == null)
            {
                return false;
            }

            return true;
        }

        public static IWorkspace GetWorkspace()
        {
            return m_workSpace;
        }

        public static IFeatureClass GetFeatureClass()
        {
            if (m_featClassEdit == null)
            {
                return null;
            }
            else
            {
                return m_featClassEdit;
            }
        }

        #endregion

        #region "编辑工作空间管理"

        //说明:这里IWorkspaceEdit管理数据，对于GeoDatabase的数据必须注册为版本才支持Edit Session；否则delete 数据直接更改数据并且不可回退

        public static bool StartEditing()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                // 开始编辑,并设置Undo/Redo 为可用
                if (!workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.StartEditing(true);
                    workspaceEdit.EnableUndoRedo();
                    m_IsEdited = true;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.StartEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool StartEditing(IFeatureLayer featLayer)
        {
            try
            {
                IDataset pDataset = featLayer.FeatureClass as IDataset;
                if (pDataset == null)
                    return false;

                // 开始编辑,并设置Undo/Redo 为可用
                m_workSpace = pDataset.Workspace;
                IWorkspaceEdit workspaceEdit = pDataset.Workspace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                // 开始编辑,并设置Undo/Redo 为可用
                if (!workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.StartEditing(true);
                    workspaceEdit.EnableUndoRedo();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.StartEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool SaveEditing()
        {
            StopEditing(true);
            StartEditing();

            return true;
        }

        public static bool StopEditing(bool isSave)
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.HasEdits(ref hasEdits);
                    if (hasEdits)
                    {
                        workspaceEdit.StopEditing(isSave);
                    }
                    else
                    {
                        workspaceEdit.StopEditing(false);
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.StopEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool UndoEditing()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {

                    workspaceEdit.HasEdits(ref hasEdits);
                    if (hasEdits)
                    {
                        workspaceEdit.UndoEditOperation();
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.UndoEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool RedoEditing()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.HasEdits(ref hasEdits);
                    if (hasEdits)
                    {
                        workspaceEdit.RedoEditOperation();
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.RedoEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static ESRI.ArcGIS.Geodatabase.IWorkspaceEdit GetWorkspaceEdit()
        {
            return m_workSpace as IWorkspaceEdit;
        }

        public static bool UndoEditing(int step)
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    while (step > 0)
                    {
                        workspaceEdit.HasEdits(ref hasEdits);
                        if (hasEdits)
                        {
                            workspaceEdit.UndoEditOperation();
                        }
                        step--;
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.UndoEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool RedoEditing(int step)
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    int xx = 0;
                    while (xx < step)
                    {
                        workspaceEdit.HasRedos(ref hasEdits);
                        if (hasEdits)
                        {
                            workspaceEdit.RedoEditOperation();
                        }
                        xx++;
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.RedoEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool HasEdits()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.HasEdits(ref hasEdits);

                    return hasEdits;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.HasEdits:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        public static bool HasRedos()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.HasRedos(ref hasEdits);

                    return hasEdits;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.HasRedos:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }

        }

        public static bool HasUndos()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                bool hasEdits = false;
                if (workspaceEdit.IsBeingEdited())
                {
                    workspaceEdit.HasUndos(ref hasEdits);

                    return hasEdits;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.HasUndos:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }

        }

        public static bool IsBeginEditing()
        {
            try
            {
                IWorkspaceEdit workspaceEdit = m_workSpace as IWorkspaceEdit;
                if (workspaceEdit == null)
                {
                    return false;
                }

                return workspaceEdit.IsBeingEdited();
            }
            catch (System.Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.IsBeginEditing:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pFeature"></param>
        /// <param name="pGeometry"></param>
        public static void UpdateFeature(IFeature pFeature, IGeometry pGeometry)
        {
            try
            {
                if (pFeature == null)
                    return;

                if (pGeometry == null)
                    return;

                IWorkspaceEdit pWorkspaceEdit = GetWorkspaceEdit();
                pWorkspaceEdit.StartEditOperation();

                pFeature.Shape = pGeometry;

                //ReCalculateArea(ref pFeature);    //2014/3/24 莫致良

                pFeature.Store();

                pWorkspaceEdit.StopEditOperation();
            }
            catch (Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.UpdateFeature:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        public static void UpdateFeature(IFeature pFeature)
        {
            try
            {
                if (pFeature == null)
                    return;

                IWorkspaceEdit pWorkspaceEdit = GetWorkspaceEdit();
                pWorkspaceEdit.StartEditOperation();

                pFeature.Store();

                pWorkspaceEdit.StopEditOperation();
            }
            catch (Exception ex)
            {
                // LogHelper.LogHelper.("MapEditEnvOp.UpdateFeature:" + ex.Message);
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        #endregion

    }
}
