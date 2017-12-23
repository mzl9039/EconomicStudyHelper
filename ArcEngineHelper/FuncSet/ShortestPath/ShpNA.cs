using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.NetworkAnalyst;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;

namespace DataHelper.FuncSet.ShortestPath
{
    /************************************************************************/
    /* Description:	对两个shp进行网络分析、
    /* Authon:		mzl
    /* Date:		2017/11/26 16:37:19
    /************************************************************************/
    public class ShpNA
    {
        /// <summary>
        /// mdb文件的workspace
        /// </summary>
        public IWorkspace mdbWorkspace;
        /// <summary>
        /// mdb文件的dataset
        /// </summary>
        public IFeatureDataset featureDataset;
        /// <summary>
        /// originFeatClas 的名字
        /// </summary>
        private string origin = "ori";
        /// <summary>
        /// 起点shp文件
        /// </summary>
        public IFeatureClass originFeatCls;
        /// <summary>
        /// 要生成的起点和终点的字段结构
        /// </summary>
        public IFields fields;
        /// <summary>
        /// destinationFeatCls 的名字
        /// </summary>
        private string destination = "dest";
        /// <summary>
        /// 终点shp文件，即上面featureDataset的一个图层
        /// </summary>
        public IFeatureClass destinationFeatCls;
        /// <summary>
        /// 表示公路铁路线shp的名称
        /// </summary>
        private string railway = "";
        /// <summary>
        /// 公路/铁路线 feature class
        /// </summary>
        public IFeatureClass railwayFeatureClass;
        /// <summary>
        /// 用于创建network dataset 或更新network dataset的信息
        /// </summary>
        public INetworkBuild networkBuild;
        /// <summary>
        /// 网络分析上下文
        /// </summary>
        private INAContext m_NAContext;
        /// <summary>
        /// 网络数据集ND文件名,与mdb文件名、dataset名称同名
        /// </summary>
        private string ndName = null;
        /// <summary>
        /// 网络数据集
        /// </summary>
        private INetworkDataset ndDataset = null;
        /// <summary>
        /// OD 成本矩阵图层
        /// </summary>
        private INALayer3 odMatrixLayer = null;

        public bool init(IFeatureClass destFeatCls, string cutOff)
        {
            // 设置终点 feature Class
            if (destFeatCls == null)
            {
                Log.Log.Warn("destFeatCls can't not be null");
                return false;
            }
            // 设置目标图层，以便后续继续创建OD矩阵里设置dest图层
            destinationFeatCls = destFeatCls;

            fields = destFeatCls.Fields;
            // 获取ND文件的Dataset
            string railName = DataPreProcess.GetFileName("选择公路/铁路线的mdb文件", "gdb");
            if (railName == null || railName == "" || !File.Exists(railName))
            {
                Log.Log.Warn(string.Format("选择的mdb文件名为空，或文件不存在，文件名为：{0}.", railName));
                return false;
            }
            else
            {                
                // 作为 personal geodatabase 文件名和 feature dataset 的名字
                string name = System.IO.Path.GetFileNameWithoutExtension(railName);
                try
                {
                    // 如果mdb文件已存在，则直接打开，否则就抛异常
                    if (File.Exists(railName))
                    {
                        mdbWorkspace = Geodatabase.GeodatabaseOp.OpenFromFile_fGDB_Workspace(railName);
                        if (mdbWorkspace == null) throw new Exception(string.Format("文件{0}存在，但打开Personal Geodatabase失败", railName));
                    }
                    if (mdbWorkspace == null) throw new Exception(string.Format("文件{0}不存在", railName));
                    // 检查 dataset 是否存在，不存在抛异常
                    IFeatureWorkspace featureWorkspace = mdbWorkspace as IFeatureWorkspace;
                    IWorkspace2 workspace2 = mdbWorkspace as IWorkspace2;
                    if (workspace2.get_NameExists(esriDatasetType.esriDTFeatureDataset, name))
                    {
                        featureDataset = Geodatabase.GeodatabaseOp.OpenFeatureDataset(mdbWorkspace, name);
                    }
                    if (featureDataset == null) throw new Exception(string.Format("在文件{0}中创建要素集{1}失败", railName, name));
                    // 检查 destinationFeatCls 是否存在，若不存在则抛异常
                    IDictionary<string, string> featureClassDic = Geodatabase.GeodatabaseOp.GetFeatureClassNameDic(featureDataset);
                    if (featureClassDic.ContainsKey(name))
                    {
                        railwayFeatureClass = Geodatabase.GeodatabaseOp.OpenFeatClass(mdbWorkspace, name);
                    }
                    if (railwayFeatureClass == null) throw new Exception(string.Format("在featureDataset中不存在featureClass:{0}", railway));
                    ndDataset = Geodatabase.GeodatabaseOp.OpenNetworkDataset(mdbWorkspace, name + "_ND", name);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Log.Log.Error(string.Format("通过文件{0}创建Personal Geodatabase 或 Dataset 失败", railName), ex);
                    return false;
                }

                if (ndDataset != null)
                {
                    m_NAContext = CreateSolverContext(cutOff);
                    odMatrixLayer = CreateODCostMatrixLayer();
                    LoadNANetworkLocations("Destinations", destinationFeatCls, 5000);
                }
            }
        }

        /// <summary>
        /// 更新 feature dataset 中的 network dataset，如果不存在，则创建，
        /// 如果存在，则删除后重新创建
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public IFeatureClass updateNetworkDataset(IFeature src)
        {
            updateOriginAndDestFeatClsByFeature(src);
            // 删除所有的 network dataset
            //deleteNetworkDataset();
            // 创建新的 network dataset
            return setFeatCls();
        }

        /// <summary>
        /// 根据 feature 更新 origin 和 destination feature class
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private void updateOriginAndDestFeatClsByFeature(IFeature src)
        {
            if (featureDataset == null || src == null)
            {
                Log.Log.Warn("featureDataset is null or feature is null");
                return;
            }
            try
            {
                originFeatCls = createFeatureClass(origin);
                Geodatabase.GeodatabaseOp.CreateFeature(originFeatCls, src);
            }
            catch (System.Exception ex)
            {
                Log.Log.Error(string.Format("failed to update Origin feature class by feature"), ex);
            }            
        }

        /// <summary>
        /// 由于代表起点的 feature class 要频繁删除并重建，因此专门写一个方法
        /// 若这个 feature class 已存在，则删除，否则直接新建
        /// </summary>
        /// <returns></returns>
        private IFeatureClass createFeatureClass(string name)
        {
            // 如果这个 feature class 已存在，则删除再新建
            IDictionary<string, string> featureClassDic = Geodatabase.GeodatabaseOp.GetFeatureClassNameDic(featureDataset);
            if (featureClassDic.ContainsKey(name))
            {
                Geodatabase.GeodatabaseOp.DeleteFeatureClass(mdbWorkspace, name);
            }
            string strShapeField = "Shape";
            IFeatureClassDescription fcd = new FeatureClassDescriptionClass();
            IObjectClassDescription ocd = (IObjectClassDescription)fcd;
            if (name == "ori")
            {
                originFeatCls = featureDataset.CreateFeatureClass(name, fields, ocd.InstanceCLSID, ocd.ClassExtensionCLSID, esriFeatureType.esriFTSimple, strShapeField, "");
                return originFeatCls;
            }
            else
            {
                destinationFeatCls = featureDataset.CreateFeatureClass(name, fields, ocd.InstanceCLSID, ocd.ClassExtensionCLSID, esriFeatureType.esriFTSimple, strShapeField, "");
                return destinationFeatCls;
            }
        }

        /// <summary>
        /// 设置 network dataset 的 Origins 和 Destinations
        /// </summary>
        /// <param name="srcFeatCls"></param>
        /// <param name="dstFeatCls"></param>
        /// <returns></returns>
        private IFeatureClass setFeatCls() {
            LoadNANetworkLocations("Origins", originFeatCls, 5000);            
            IGPMessages gpMessages = new GPMessagesClass();
            m_NAContext.Solver.UpdateContext(m_NAContext, GetDENetworkDataset(ndDataset), gpMessages);
            m_NAContext.Solver.Solve(m_NAContext, gpMessages, null);
            INAClass naClass = m_NAContext.NAClasses.ItemByName["ODLines"] as INAClass;
            return naClass as IFeatureClass;
        }

        /// <summary>
        /// 获取network dataset中的某个featureClass图层
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IFeatureClass getFeatureClassFromNAClasses(string name)
        {
            INAClass naClass = m_NAContext.NAClasses.ItemByName[name] as INAClass;
            if (naClass != null)
            {
                return naClass as IFeatureClass;
            }
            return null;
        }  

        /// <summary>
        /// Set solver settings
        /// </summary>
        /// <param name="strNAClassName">NAClass name</param>
        /// <param name="inputFC">Input feature class</param>
        /// <param name="maxSnapTolerance">Max snap tolerance</param>
        private void LoadNANetworkLocations(string strNAClassName, IFeatureClass inputFC, double maxSnapTolerance)
        {
            INamedSet classes = m_NAContext.NAClasses;
            INAClass naClass = classes.get_ItemByName(strNAClassName) as INAClass;

            // Delete existing locations from the specified NAClass
            naClass.DeleteAllRows();

            // Create a NAClassLoader and set the snap tolerance (meters unit)
            INAClassLoader classLoader = new NAClassLoader();
            classLoader.Locator = m_NAContext.Locator;
            classLoader.Locator.SnapToleranceUnits = ESRI.ArcGIS.esriSystem.esriUnits.esriKilometers;
            if (maxSnapTolerance > 0) ((INALocator3)classLoader.Locator).MaxSnapTolerance = maxSnapTolerance;
            classLoader.NAClass = naClass;           
            

            // Create field map to automatically map fields from input class to NAClass
            INAClassFieldMap fieldMap = new NAClassFieldMapClass();
            fieldMap.CreateMapping(naClass.ClassDefinition, inputFC.Fields);
            fieldMap.set_MappedField("Name", "ID");
            classLoader.FieldMap = fieldMap;


            // Avoid loading network locations onto non-traversable portions of elements
            INALocator3 locator = m_NAContext.Locator as INALocator3;
            locator.ExcludeRestrictedElements = true;
            locator.CacheRestrictedElements(m_NAContext);
            

            // Load network locations
            int rowsIn = 0;
            int rowsLocated = 0;
            classLoader.Load((ICursor)inputFC.Search(null, true), null, ref rowsIn, ref rowsLocated);

            // Message all of the network analysis agents that the analysis context has changed.
            ((INAContextEdit)m_NAContext).ContextChanged();
        }

        /// <summary>
        /// Geodatabase function: get network dataset
        /// </summary>
        /// <param name="networkDataset">Input network dataset</param>
        /// <returns>DE network dataset</returns>
        private IDENetworkDataset GetDENetworkDataset(INetworkDataset networkDataset)
        {
            // Cast from the network dataset to the DatasetComponent
            IDatasetComponent dsComponent = networkDataset as IDatasetComponent;

            // Get the data element
            return dsComponent.DataElement as IDENetworkDataset;
        }

        /// <summary>
        /// Create NASolver and NAContext
        /// </summary>
        /// <returns>NAContext</returns>
        private INAContext CreateSolverContext(string cutoff)
        {
            //Get the data element
            IDENetworkDataset deNDS = GetDENetworkDataset(ndDataset);
            ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver naAODCostMatrixSolver = new ESRI.ArcGIS.NetworkAnalyst.NAODCostMatrixSolverClass();
            // 设置 output Lines 为 no lines
            naAODCostMatrixSolver.OutputLines = esriNAOutputLineType.esriNAOutputLineNone;
            INASolver naSolver = naAODCostMatrixSolver as ESRI.ArcGIS.NetworkAnalyst.INASolver;
            INAClosestFacilitySolver cfSolver = naSolver as INAClosestFacilitySolver;
            cfSolver.DefaultCutoff = cutoff;
            INAContextEdit contextEdit = naSolver.CreateContext(deNDS, naSolver.Name) as INAContextEdit;
            //Bind a context using the network dataset 
            contextEdit.Bind(ndDataset, new GPMessagesClass());

            return contextEdit as INAContext;
        }

        ///<summary>创建一个新的 OD 成本矩阵图层 OD cost matrix layer.</summary>
        ///  
        ///<param name="networkDataset">An INetworkDataset interface that is the network dataset on which to perform the OD cost matrix analysis.</param>
        ///  
        ///<returns>An INALayer3 interface that is the newly created network analysis layer.</returns>
        private INALayer3 CreateODCostMatrixLayer()
        {
            ESRI.ArcGIS.NetworkAnalyst.INAContextEdit naContextEdit = m_NAContext as ESRI.ArcGIS.NetworkAnalyst.INAContextEdit; // Dynamic Cast

            IGPMessages gpMessages = new ESRI.ArcGIS.Geodatabase.GPMessagesClass();
            naContextEdit.Bind(ndDataset, gpMessages);

            ESRI.ArcGIS.NetworkAnalyst.INALayer naLayer = m_NAContext.Solver.CreateLayer(m_NAContext);
            return naLayer as ESRI.ArcGIS.NetworkAnalyst.INALayer3;
        }

        /// <summary>
        /// Encapsulates returning an empty string if the object is NULL.
        /// </summary>
        private string GetStringFromObject(object value)
        {
            if (value == null)
                return "";
            else
                return value.ToString();
        }
    }
}
