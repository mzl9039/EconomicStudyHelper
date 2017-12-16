using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using System.IO;
using Common;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace DataHelper.FuncSet.ShortestPath
{
    public class SPStats
    {
        /// <summary>
        /// 代码网络分析中终点的 featureClass
        /// </summary>
        private IFeatureClass tarFeatCls = null;
        /// <summary>
        /// tarFeatCls 对应的shp文件的名称
        /// </summary>
        private string tarShpName = null;
        /// <summary>
        /// 所有的excels文件名
        /// </summary>
        private List<string> excels = null;
        /// <summary>
        /// 所有的企业信息
        /// </summary>
        private List<Enterprise> enterprises = null;
        /// <summary>
        /// 网络分析处理类
        /// </summary>
        private ShpNA shpNa = new ShpNA();
        /// <summary>
        /// 非铁路线上的速度
        /// </summary>
        private double speed = -1;
        /// <summary>
        /// 铁路线上的点集shp的文件名
        /// </summary>
        private string railFeatFileName = null;
        /// <summary>
        /// 铁路线上的点集
        /// </summary>
        private IFeatureClass railFeatCls = null;
        /// <summary>
        /// 用于查询铁路线上最近的点
        /// </summary>
        private IIndexQuery2 indexQuery2 = null;
        /// <summary>
        /// 记录各个点的cost的平均值和中位数
        /// </summary>
        private Dictionary<string, List<double>> result = new Dictionary<string, List<double>>();
        /// <summary>
        /// 输出文件的名字
        /// </summary>
        private string output = "";
        /// <summary>
        /// 构造函数
        /// </summary>
        public SPStats(List<string> excels)
        {
            try
            {
                // 获取所有企业点shp的featureClass
                tarShpName = DataPreProcess.GetShpName("选择起点/终点的shp文件");
                if (tarShpName == null || tarShpName == "" || !System.IO.File.Exists(tarShpName))
                {
                    Log.Log.Warn(string.Format("选择的shp文件不存在，文件名为 {0}", tarShpName));
                    return;
                }
                // 初始化时创建输出结果文件
                FileIOInfo fileIo = new FileIOInfo(tarShpName);
                output = string.Format("{0}{1}{2}.csv", fileIo.FilePath, "\\", "output");
                if (File.Exists(output))
                {
                    File.Delete(output);                   
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(output, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(string.Format("id, avg, mid"));
                    sw.Flush();
                }

                tarFeatCls = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(tarShpName);
                // 获取所有企业点的excels文件的名称以及所有企业点的list
                if (excels == null || excels.Count() <= 0)
                {
                    Log.Log.Warn(string.Format("未获取到excels文件信息，可能选择了一个错误的excel目录."));
                }
                else
                {
                    this.excels = excels;
                }
                if (!shpNa.init(tarFeatCls))
                {
                    Log.Log.Warn("初始化 ShpNA 失败");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("打开所有企业点的shp失败", ex);
                return;
            }
        }

        private bool checkParam()
        {
            if (tarFeatCls == null)
            {
                Log.Log.Warn("tarFeatCls 为空，计算错误，返回!");
                return false;
            }
            return true;
        }

        public void caculateShortestPath()
        {
            if (!checkParam()) return;
           
            try
            {
                IFeature src, tar;
                int featureNum = tarFeatCls.FeatureCount(null);
                
                int idxId = tarFeatCls.FindField("ID");
                if (idxId < 0)
                {
                    Log.Log.Warn(string.Format("在shp文件:{0}中无法找到字段:{1}, 异常退出", tarShpName, "ID"));
                    return;
                }
                for (int i = 0; i < featureNum; i++)
                {
                    src = tarFeatCls.GetFeature(i);
                    string srcId = src.Value[idxId].ToString();
                    List<double> tmpCost = new List<double>(tarFeatCls.FeatureCount(null));
                    Log.Log.Info(string.Format("src: {0} is being caculated.", srcId));
                    for (int j = 0; j < featureNum; j++)
                    {
                        if (i == j) continue;

                        tar = tarFeatCls.GetFeature(j);
                        string tarId = tar.Value[idxId].ToString();

                        /// <summary>
                        /// 代码网络分析中起点的 featureClass
                        /// 对于当前这种数据巨大的情况（一个shp可能有40w个点，计算两两之间的最短路径）
                        /// 需要对点进行循环，所以起点featureClass需要手动生成，用完后自动删除
                        /// </summary>                    
                        IFeatureClass lines = shpNa.updateNetworkDataset(src, tar);
                        if (lines != null)
                        {
                            int idxName = lines.Fields.FindField("Name");
                            int idxTotalCost = lines.Fields.FindField("Total_Cost");

                            // 因为起点和终点都只有一个，所以 lines 只会有一个feature，其 ObjectId 一定为 1
                            IFeature railFeat = lines.GetFeature(1);
                            double totalCost = -1;
                            try
                            {
                                totalCost = double.Parse(railFeat.Value[idxTotalCost].ToString());
                            }
                            catch (System.Exception ex)
                            {
                                Log.Log.Warn(string.Format("将 total_Cost 解析为 double 类型失败！Name 为：{0}", railFeat.Value[idxName].ToString()), ex);
                                continue;
                            }

                            int oriClosedFID = -1, destClosedFID = -1;
                            double oriDist = -1, destDist = -1;
                            // 尝试根据 feature 获得到最近的铁路线上的点的距离
                            indexQuery2.NearestFeature(src.Shape as IPoint, out oriClosedFID, out oriDist);
                            indexQuery2.NearestFeature(tar.Shape as IPoint, out destClosedFID, out destDist);
                            if (oriClosedFID == -1 || destClosedFID == -1)
                            {
                                Log.Log.Warn(string.Format("failed to find closed point, oriClosedFID: {0}, destClosedFID: {1}",
                                    oriClosedFID, destClosedFID));
                                continue;
                            }
                            // 计算走铁路时的总cost
                            double railCost = 60 * (oriDist + destDist) / (speed * 1000) + totalCost;
                            IProximityOperator proximityOp = (src.Shape as IPoint) as IProximityOperator;
                            double excelDistance = SPUtils.caculateStraightDistance((src.Shape as IPoint), (tar.Shape as IPoint));
                            double excelCost = 60 * excelDistance / speed;
                            tmpCost.Add(Math.Min(excelCost, railCost));
                        }
                        else
                        {
                            Log.Log.Error(string.Format("feature: {0} is failed to get NA result！", tarId));
                            continue;
                        }
                    }

                    double avg = tmpCost.Sum() / tmpCost.Count();
                    QuickSelect qSelect = new QuickSelect(tmpCost.Count());
                    double mid = qSelect.QSelect(tmpCost.ToArray(), 0, tmpCost.Count() - 1, tmpCost.Count() / 2);
                    List<double> val = new List<double>(2);
                    val.Add(avg);
                    val.Add(mid);
                    result.Add(srcId, val);
                    if (result.Count() >= 1000)
                    {
                        write();
                        result.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Log.Error("计算所有企业点的最短路径失败，异常退出.", ex);
            }
            finally
            {
                // 最后将剩余结果写到结果文件里
                write();
                result.Clear();
            }
        }

        public void write()
        {
            if (result != null && result.Count() <= 0)
            {
                return;
            }
            using (System.IO.FileStream fs = new System.IO.FileStream(output, FileMode.Append))
            {
                StreamWriter sw = new StreamWriter(fs);
                foreach (KeyValuePair<string, List<double>> kv in result)
                {
                    sw.WriteLine(string.Format("{0}, {1}, {2}", kv.Key, kv.Value[0], kv.Value[1]));
                }
                sw.Flush();
            }

        }

        public void setSpeed(double speed)
        {
            this.speed = speed;
        }

        public void setRailPoints(string railPoints)
        {
            railFeatFileName = railPoints;
            railFeatCls = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(railPoints);
            IGeoDataset tGeodataset = railFeatCls as IGeoDataset;
            // TODO index 使用接口 IFeatureIndex 可能有风险！建议使用 IFeatureIndex2，并设置 spatialreferance
            IFeatureIndex2 index = new FeatureIndexClass();
            ITrackCancel trackCancel = new TrackCancel();
            IIndexQuery indexQuery = index as IIndexQuery;
            indexQuery2 = indexQuery as IIndexQuery2;
            index.FeatureClass = railFeatCls;
            index.set_OutputSpatialReference(railFeatCls.OIDFieldName, tGeodataset.SpatialReference);
            index.Index(trackCancel, tGeodataset.Extent);
        }
    }
}
