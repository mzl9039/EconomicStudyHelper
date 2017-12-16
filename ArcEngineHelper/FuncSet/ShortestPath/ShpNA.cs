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

        public bool init(IFeatureClass destFeatCls)
        {
            // 设置终点 feature Class
            if (destFeatCls == null)
            {
                Log.Log.Warn("destFeatCls can't not be null");
                return false;
            }
            destinationFeatCls = destFeatCls;
            // 获取ND文件的Dataset
            string railName = DataPreProcess.GetFileName("选择公路/铁路线的mdb文件", "mdb");
            if (railName == null || railName == "" || !File.Exists(railName))
            {
                Log.Log.Warn(string.Format("选择的mdb文件名为空，或文件不存在，文件名为：{0}.", railName));
                return false;
            }
            else
            {                
                // 作为 personal geodatabase 文件名和 feature dataset 的名字
                string name = System.IO.Path.GetFileNameWithoutExtension(railName);
                //string path = System.IO.Path.GetDirectoryName(railName);
                try
                {
                    //string mdbName = string.Format("{0}\\{1}.mdb", path, name);
                    //IFeatureClass selectShpFeatureClass = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(railName);
                    // 如果mdb文件已存在，则直接打开，否则就抛异常
                    if (File.Exists(railName))
                    {
                        mdbWorkspace = Geodatabase.GeodatabaseOp.Open_pGDB_Workspace(railName);
                        if (mdbWorkspace == null) throw new Exception(string.Format("文件{0}存在，但打开Personal Geodatabase失败", railName));
                    }
                    //else
                    //{
                        //mdbWorkspace = Geodatabase.GeodatabaseOp.Create_pGDB_Workspace(path, name + ".mdb");
                        if (mdbWorkspace == null) throw new Exception(string.Format("文件{0}不存在", railName));
                    //}
                    // 检查 dataset 是否存在，不存在抛异常
                    IFeatureWorkspace featureWorkspace = mdbWorkspace as IFeatureWorkspace;
                    IWorkspace2 workspace2 = mdbWorkspace as IWorkspace2;
                    if (workspace2.get_NameExists(esriDatasetType.esriDTFeatureDataset, name))
                    {
                        featureDataset = Geodatabase.GeodatabaseOp.OpenFeatureDataset(mdbWorkspace, name);
                    }
                    //else
                    //{
                        //featureDataset = Geodatabase.GeodatabaseOp.CreateFeatDataset(featureWorkspace, geoDataset.SpatialReference, name);
                        if (featureDataset == null) throw new Exception(string.Format("在文件{0}中创建要素集{1}失败", railName, name));
                    //}
                    // 检查 destinationFeatCls 是否存在，若不存在则抛异常
                    IDictionary<string, string> featureClassDic = Geodatabase.GeodatabaseOp.GetFeatureClassNameDic(featureDataset);
                    if (featureClassDic.ContainsKey(name))
                    {
                        railwayFeatureClass = Geodatabase.GeodatabaseOp.OpenFeatClass(mdbWorkspace, name);
                    }
                    //else
                    //{
                        //string strShapeField = "";
                        //for (int i = 0; i < selectShpFeatureClass.Fields.FieldCount; i++)
                        //{
                        //    if (selectShpFeatureClass.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                        //    {
                        //        strShapeField = selectShpFeatureClass.Fields.get_Field(i).Name;
                        //        break;
                        //    }
                        //}
                        //IFeatureClassDescription fcd = new FeatureClassDescriptionClass();
                        //IObjectClassDescription ocd = (IObjectClassDescription)fcd;
                        //railwayFeatureClass = featureDataset.CreateFeatureClass(railway, selectShpFeatureClass.Fields, ocd.InstanceCLSID, ocd.ClassExtensionCLSID, esriFeatureType.esriFTSimple, strShapeField, "");
                        if (railwayFeatureClass == null) throw new Exception(string.Format("在featureDataset中不存在featureClass:{0}", railway));
                        //IFeatureCursor featureCursor = selectShpFeatureClass.Search(null, false);
                        //Geodatabase.GeodatabaseOp.FeatureCopy(featureCursor, railwayFeatureClass);
                        //System.Runtime.InteropServices.Marshal.ReleaseComObject(featureCursor);
                    //}
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
                    m_NAContext = CreateSolverContext();
                    odMatrixLayer = CreateODCostMatrixLayer();
                }
            }
        }

        /// <summary>
        /// 更新 feature dataset 中的 network dataset，如果不存在，则创建，
        /// 如果存在，则删除后重新创建
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public IFeatureClass updateNetworkDataset(IFeature feature)
        {
            updateOriginFeatClsByFeature(feature);
            // 删除所有的 network dataset
            //deleteNetworkDataset();
            // 创建新的 network dataset            
            m_NAContext = CreateSolverContext();
            odMatrixLayer = CreateODCostMatrixLayer();
            return setFeatCls();
        }

        /// <summary>
        /// 根据 feature 更新 origin feature class
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        private void updateOriginFeatClsByFeature(IFeature feature)
        {
            if (featureDataset == null || feature == null)
            {
                Log.Log.Warn("featureDataset is null or feature is null");
                return;
            }
            try
            {
                originFeatCls = createOriginFeatureClass();
                Geodatabase.GeodatabaseOp.CreateFeature(originFeatCls, feature);
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
        private IFeatureClass createOriginFeatureClass()
        {
            // 如果这个 feature class 已存在，则删除再新建
            IDictionary<string, string> featureClassDic = Geodatabase.GeodatabaseOp.GetFeatureClassNameDic(featureDataset);
            if (featureClassDic.ContainsKey(origin))
            {
                Geodatabase.GeodatabaseOp.DeleteFeatureClass(mdbWorkspace, origin);
            }
            string strShapeField = "";
            for (int i = 0; i < destinationFeatCls.Fields.FieldCount; i++)
            {
                if (destinationFeatCls.Fields.get_Field(i).Type == esriFieldType.esriFieldTypeGeometry)
                {
                    strShapeField = destinationFeatCls.Fields.get_Field(i).Name;
                    break;
                }
            }
            IFeatureClassDescription fcd = new FeatureClassDescriptionClass();
            IObjectClassDescription ocd = (IObjectClassDescription)fcd;
            originFeatCls = featureDataset.CreateFeatureClass(origin, destinationFeatCls.Fields, ocd.InstanceCLSID, ocd.ClassExtensionCLSID, esriFeatureType.esriFTSimple, strShapeField, "");
            return originFeatCls;
        }

        /// <summary>
        /// 创建network dataset
        /// </summary>
        /// <param name="originFC"></param>
        /// <returns></returns>
        private INetworkDataset createNetworkDataset()
        {
            if (railwayFeatureClass == null)
            {
                Log.Log.Warn(string.Format("can't create network dataset with a featureClass, it is null."));
                return null;
            }
            try
            {
                #region DENetworkDataset 创建
                // 创建空的 data element，以便后面创建 network dataset.
                IDENetworkDataset2 deNetworkDataset = new DENetworkDatasetClass();
                deNetworkDataset.Buildable = true;
                deNetworkDataset.NetworkType = esriNetworkDatasetType.esriNDTGeodatabase;
                // 打开 feature class 并转为 IGeoDataset 接口
                IGeoDataset geoDataset = (IGeoDataset)featureDataset;
                // 将 feature class 的 extent 和 spatial reference 赋给 network dataset data element.
                IDEGeoDataset deGeoDataset = (IDEGeoDataset)deNetworkDataset;
                deGeoDataset.Extent = geoDataset.Extent;
                deGeoDataset.SpatialReference = geoDataset.SpatialReference;
                // Specify the name of the network dataset.
                // 指定  network dataset 的名称
                IDataElement dataElement = (IDataElement)deGeoDataset;
                dataElement.Name = string.Format("{0}_ND", railwayFeatureClass.AliasName);
                #endregion

                // Specify the network dataset's elevation model.
                deNetworkDataset.ElevationModel = esriNetworkElevationModel.esriNEMElevationFields;

                #region 边源创建
                // 创建 EdgeFeatureSource 对象并将其指向 feature class
                INetworkSource edgeNetworkSource = new EdgeFeatureSourceClass();
                edgeNetworkSource.Name = "edgeSources";
                edgeNetworkSource.ElementType = esriNetworkElementType.esriNETEdge;

                // 设置 source 的连通性
                IEdgeFeatureSource edgeFeatureSource = (IEdgeFeatureSource)edgeNetworkSource;
                edgeFeatureSource.UsesSubtypes = false;
                edgeFeatureSource.ClassConnectivityGroup = 1;
                edgeFeatureSource.ClassConnectivityPolicy = esriNetworkEdgeConnectivityPolicy.esriNECPEndVertex;
                #endregion

                #region 边源的方向

                IStreetNameFields streetNameFields = new StreetNameFieldsClass();
                streetNameFields.Priority = 1;
                streetNameFields.StreetNameFieldName = "FULL_NAME";

                INetworkSourceDirections nsDirections = new NetworkSourceDirectionsClass();
                IArray nsdArray = new ArrayClass();
                nsdArray.Add(streetNameFields);
                nsDirections.StreetNameFields = nsdArray;
                edgeNetworkSource.NetworkSourceDirections = nsDirections;

                deNetworkDataset.SupportsTurns = true;

                #endregion

                #region 转弯源设置
                // TODO 设置 source 的转弯，是否需要new一个对象出来
                deNetworkDataset.SupportsTurns = true;

                INetworkSource turnNetworkSource = new TurnFeatureSourceClass();
                //就是添加到网络数据集的要素类的名称
                turnNetworkSource.Name = railwayFeatureClass.AliasName;  
                turnNetworkSource.ElementType = esriNetworkElementType.esriNETTurn;
                #endregion

                #region 添加到IArray中
                // 将前面的设置添加到一个 IArray中，这个IArray可以添加到data element network dataset中
                IArray sourceArray = new ArrayClass();
                sourceArray.Add(edgeNetworkSource);
                sourceArray.Add(turnNetworkSource);
                deNetworkDataset.Sources = sourceArray;
                #endregion

                // 空的数组来创建属性，这些属性将被添加到数据中
                IArray attributeArray = new ArrayClass();
                // 初始化在创建属性时可以重用的变量
                IEvaluatedNetworkAttribute evalNetAttr;
                INetworkAttribute2 netAttr2;
                INetworkFieldEvaluator netFieldEval;
                INetworkConstantEvaluator netConstEval;
                // 创建属性Cost
                evalNetAttr = new EvaluatedNetworkAttributeClass();
                netAttr2 = (INetworkAttribute2)evalNetAttr;
                netAttr2.Name = "Cost";
                netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
                netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
                netAttr2.Units = esriNetworkAttributeUnits.esriNAUMinutes;
                netAttr2.UseByDefault = true;
                // 设置源属性赋值器
                // 设置源属性赋值器——from - to
                netFieldEval = new NetworkFieldEvaluatorClass();
                netFieldEval.SetExpression("[cost]", "");
                evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized,
                    (INetworkEvaluator)netFieldEval);
                // 设置源属性赋值器——to - from
                netFieldEval = new NetworkFieldEvaluatorClass();
                netFieldEval.SetExpression("[cost]", "");
                evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAgainstDigitized,
                    (INetworkEvaluator)netFieldEval);
                // 设置默认属性赋值器
                // 设置默认属性赋值器——默认边
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, (INetworkEvaluator)netConstEval);
                // 设置默认属性赋值器——默认交汇点
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction, (INetworkEvaluator)netConstEval);
                // 设置默认属性赋值器——默认交汇转弯
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn, (INetworkEvaluator)netConstEval);
                // 将属性Cost添加到数组中
                attributeArray.Add(evalNetAttr);
                // 创建属性长度
                evalNetAttr = new EvaluatedNetworkAttributeClass();
                netAttr2 = (INetworkAttribute2)evalNetAttr;
                netAttr2.Name = "Length";
                netAttr2.UsageType = esriNetworkAttributeUsageType.esriNAUTCost;
                netAttr2.DataType = esriNetworkAttributeDataType.esriNADTDouble;
                netAttr2.Units = esriNetworkAttributeUnits.esriNAUMeters;
                netAttr2.UseByDefault = false;
                // 设置源属性赋值器
                netFieldEval = new NetworkFieldEvaluatorClass();
                netFieldEval.SetExpression("[Shape]", "");
                evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAlongDigitized,
                    (INetworkEvaluator)netFieldEval);
                netFieldEval = new NetworkFieldEvaluatorClass();
                netFieldEval.SetExpression("[Shape]", "");
                evalNetAttr.set_Evaluator(edgeNetworkSource, esriNetworkEdgeDirection.esriNEDAgainstDigitized,
                    (INetworkEvaluator)netFieldEval);
                // 设置默认属性赋值器
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETEdge, (INetworkEvaluator)netConstEval);
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETJunction, (INetworkEvaluator)netConstEval);
                netConstEval = new NetworkConstantEvaluatorClass();
                netConstEval.ConstantValue = 0;
                evalNetAttr.set_DefaultEvaluator(esriNetworkElementType.esriNETTurn, (INetworkEvaluator)netConstEval);
                // 将属性Cost添加到数组中
                attributeArray.Add(evalNetAttr);

                //创建网络数据集
                IFeatureDatasetExtensionContainer featureDatasetExtensionContainer = (IFeatureDatasetExtensionContainer)featureDataset;
                IFeatureDatasetExtension featureDatasetExtension = featureDatasetExtensionContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
                IDatasetContainer2 datasetContainer2 = (IDatasetContainer2)featureDatasetExtension;
                IDEDataset deDataset = deNetworkDataset as IDEDataset;
                //IWorkspaceExtensionManager workspaceExtensionManager = mdbWorkspace as IWorkspaceExtensionManager; // Dynamic Cast
                //UID networkID = new UIDClass();
                //networkID.Value = "esriGeoDatabase.NetworkDatasetWorkspaceExtension";
                //IWorkspaceExtension workspaceExtension = workspaceExtensionManager.FindExtension(networkID);
                //IDatasetContainer2 datasetContainer2 = workspaceExtension as IDatasetContainer2;
                //IDEDataset deDataset = (IDEDataset)deNetworkDataset;
                ndDataset = (INetworkDataset)datasetContainer2.CreateDataset(deDataset);

                // Once the network dataset is created, build it.
                networkBuild = (INetworkBuild)ndDataset;
                networkBuild.BuildNetwork(geoDataset.Extent);
                return ndDataset;
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("create network dataset failed", ex);
                return null;
            }
            
        }

        /// <summary>
        /// 创建IDENetworkDataset（数据元素网络数据集）对象
        /// </summary>
        /// <param name="featureDataset">传入：要素数据集</param>
        /// <param name="NetworkName">传入：网络数据集名称</param>
        /// <returns>返回：数据元素网络数据集</returns>
        private IDENetworkDataset CreateDENetworkDataset(IFeatureDataset featureDataset, string NetworkName)
        {
            //判断传入参数是否为空
            if (string.IsNullOrEmpty(NetworkName) || null == featureDataset)
            {
                return null;
            }

            // 若传入参数不为空，实例化数据元素网络数据集对象
            IDENetworkDataset deNetworkDataset = new DENetworkDatasetClass();
            // 设置数据集类型、可以被构建
            deNetworkDataset.Buildable = true;
            deNetworkDataset.NetworkType = esriNetworkDatasetType.esriNDTGeodatabase;

            // 设置数据集的空间参考、空间范围
            IDEGeoDataset deGeoDataset = deNetworkDataset as IDEGeoDataset;
            IGeoDataset geoDataset = featureDataset as IGeoDataset;
            deGeoDataset.Extent = geoDataset.Extent;
            deGeoDataset.SpatialReference = geoDataset.SpatialReference;

            // 设置名称
            IDataElement dataElement = deNetworkDataset as IDataElement;
            dataElement.Name = NetworkName;

            return deNetworkDataset;
        }

        /// <summary>
        /// 删除当前featureDataset下面的所有networkDataset
        /// </summary>
        /// <returns></returns>
        private bool deleteNetworkDataset()
        {
            if (featureDataset != null)
            {
                IEnumDataset enumDataset = featureDataset.Subsets;
                IDataset dataset = null;
                while ((dataset = enumDataset.Next()) != null)
                {
                    if (dataset is INetworkDataset && dataset.CanDelete())
                    {
                        dataset.Delete();                        
                    }
                }
                return true;
            }
            else
            {
                Log.Log.Warn("featureDataset name is null");
                return false;
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
            LoadNANetworkLocations("Destinations", destinationFeatCls, 5000);
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
        private INAContext CreateSolverContext()
        {
            //Get the data element
            IDENetworkDataset deNDS = GetDENetworkDataset(ndDataset);
            ESRI.ArcGIS.NetworkAnalyst.INAODCostMatrixSolver naAODCostMatrixSolver = new ESRI.ArcGIS.NetworkAnalyst.NAODCostMatrixSolverClass();
            // 设置 output Lines 为 no lines
            naAODCostMatrixSolver.OutputLines = esriNAOutputLineType.esriNAOutputLineNone;
            INASolver naSolver = naAODCostMatrixSolver as ESRI.ArcGIS.NetworkAnalyst.INASolver;
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
    }
}
