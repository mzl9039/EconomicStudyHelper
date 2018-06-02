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
        ///// <summary>
        ///// 所有的excels文件名
        ///// </summary>
        //private List<string> excels = null;
        ///// <summary>
        ///// 所有的企业信息
        ///// </summary>
        //private List<Enterprise> enterprises = null;
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
        /// 记录各个点的周边各类型企业的数量及工人数量, 外层dict的key为企业ID，里面的dict的key为code, value 为pop和quan
        /// </summary>
        private Dictionary<string, Dictionary<double,List<double>>> result = new Dictionary<string, Dictionary<double, List<double>>>();
        /// <summary>
        /// 输出文件的名字
        /// </summary>
        private string output = "";
        /// <summary>
        /// 最大时间限制，超过这个时间的cost都会被放弃
        /// </summary>
        private double cutOff = 0.0d;
        /// <summary>
        /// 终点shp中起始的FID值，startFID到stopFID中间的点将作为起点
        /// </summary>
        private int startFID = 0;
        /// <summary>
        /// 终点shp中终止的FID值
        /// </summary>
        private int stopFID = 0;
        /// <summary>
        /// 构造函数
        /// </summary>
        public SPStats(List<string> excels, string cutOff)
        {
            try
            {
                this.cutOff = double.Parse(cutOff);
                // 获取所有企业点shp的featureClass
                tarShpName = DataPreProcess.GetShpName("选择起点/终点的shp文件");
                if (tarShpName == null || tarShpName == "" || !System.IO.File.Exists(tarShpName))
                {
                    Log.Log.Warn(string.Format("选择的shp文件不存在，文件名为 {0}", tarShpName));
                    return;
                }
                // 初始化时创建输出结果文件
                FileIOInfo fileIo = new FileIOInfo(tarShpName);
                output = string.Format("{0}{1}{2}.csv", fileIo.FilePath, "\\", "output_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"));
                if (File.Exists(output))
                {
                    File.Delete(output);                   
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(output, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(string.Format("id, code, pop, quan, popWeight"));
                    sw.Flush();
                }

                tarFeatCls = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(tarShpName);
                if (!shpNa.init(tarFeatCls, cutOff))
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
                IFeature src, tar, line;
                int featureNum = tarFeatCls.FeatureCount(null);
                
                int idxId = tarFeatCls.FindField("ID");
                int idxPop = tarFeatCls.FindField("pop");
                int idxCode = tarFeatCls.FindField("code");
                int idxQuan = tarFeatCls.FindField("quan");
                if (idxId < 0 || idxPop < 0 || idxCode < 0 || idxQuan < 0)
                {
                    Log.Log.Warn(string.Format("在shp文件:{0}中无法找到所有字段，{1}:{5}, {2}:{6}, {3}:{7}, {4}:{8}, 异常退出", 
                        tarShpName, "ID", "pop", "code", "quan", idxId, idxPop, idxCode, idxQuan));
                    return;
                }
                int idxLat2 = tarFeatCls.FindField("lat2");
                int idxLng2 = tarFeatCls.FindField("lng2");
                if (idxLat2 < 0 || idxLng2 < 0)
                {
                    Log.Log.Warn("找不到字段 lat2 lng2");
                    return;
                }
                IFeatureClass lines, destFeatCls;
                //IFeatureCursor linesCursor;
                for (int i = startFID; i < Math.Min(stopFID, featureNum); i++)
                {
                    // 拿到起点在 企业点集合 中的feature
                    src = tarFeatCls.GetFeature(i);
                    if (src == null)
                    {
                        Log.Log.Error(string.Format("无法获取FID为{0}的点", i));
                        return;
                    }
                    string srcId = src.Value[idxId].ToString();
                    Log.Log.Info(string.Format("src: {0} is being caculated.", srcId));
                    double lat, lng;
                    lat = double.Parse(src.Value[idxLat2].ToString());
                    lng = double.Parse(src.Value[idxLng2].ToString());
                    if (lat <= 0 || lng <= 0)
                    {
                        continue;
                    }
                    // alter by mzl 2018-6-2 需要添加除周边企业人口总数和企业数之外的三重统计函数
                    // List<int> ids = new List<int>();
                    // 适用 dict 并不是一个好的数据结构，但现在的方法，不重构很难存储 cost 信息
                    IDictionary<int, double> idsAndCosts = new Dictionary<int, double>();
                    for (int j = 0; j < featureNum; j++)
                    {
                        if (i == j) continue;
                        tar = tarFeatCls.GetFeature(j);
                        lat = double.Parse(tar.Value[idxLat2].ToString());
                        lng = double.Parse(tar.Value[idxLng2].ToString());
                        if (lat <= 0 || lng <= 0)
                        {
                            continue;
                        }
                        // 计算直线的总 cost
                        //IProximityOperator proximityOp = (src.Shape as IPoint) as IProximityOperator;
                        double excelDistance = SPUtils.caculateStraightDistance((src.Shape as IPoint), (tar.Shape as IPoint));
                        double excelCost = 60 * excelDistance / speed;
                        if (excelCost > cutOff)
                        {
                            continue;
                        }
                        // 如果 cost 在 cutOff 范围内，则将目标点添加到 集合中
                        //ids.Add(j);
                        idsAndCosts.Add(j, excelCost);
                    }
                    /// <summary>
                    /// 代码网络分析中起点的 featureClass
                    /// 对于当前这种数据巨大的情况（一个shp可能有40w个点，计算两两之间的最短路径）
                    /// 需要对点进行循环，所以起点featureClass需要手动生成，用完后自动删除
                    /// </summary>                    
                    lines = shpNa.updateNetworkDataset(src);
                    if (lines != null)
                    {
                        destFeatCls = shpNa.getFeatureClassFromNAClasses("Destinations");
                        int idxName = lines.FindField("Name");
                        int idxTotalCost = lines.Fields.FindField("Total_Cost");
                        if (idxName < 0 || idxTotalCost < 0)
                        {
                            Log.Log.Warn(string.Format("找不到 ND Lines 图层中的 Name({0}) 或 Total_Cost({1})", idxName, idxTotalCost));
                            return;
                        }
                                                                              
                        int lineCount = lines.FeatureCount(null);
                        for (int j = 1; j <= lineCount; j++)
                        {
                            line = lines.GetFeature(j);
                            string name = line.Value[idxName].ToString();
                            // name 字段过滤掉空的字符
                            string[] names = name.Split('-').Where(n => !string.IsNullOrWhiteSpace(n.Trim())).Select(n => n.Trim()).ToArray();
                            if (names == null || names.Count() < 2)
                            {
                                Log.Log.Warn(string.Format("Lines图层的 Name 字段格式错误，Name：{0}", name));
                                continue;
                            }
                            else if (names[0] == names[1]) continue;

                            // 获取两企业点在铁路线上的时间 totalCost
                            double totalCost = -1;
                            try
                            {
                                totalCost = double.Parse(line.Value[idxTotalCost].ToString());
                            }
                            catch (System.Exception ex)
                            {
                                Log.Log.Warn(string.Format("将 total_Cost 解析为 double 类型失败！Name 为：{0}", line.Value[idxName].ToString()), ex);
                                continue;
                            }
                            // 拿到终点在 企业点集合 中的 feature
                            int tarId = int.Parse(names[1]) - 1;
                            tar = tarFeatCls.GetFeature(tarId);
                            if (tar == null)
                            {
                                Log.Log.Warn(string.Format("找不到shp图层中ID为：{0}的Feature.", name[1]));
                                continue;
                            }

                            int oriClosedFID = -1, destClosedFID = -1;
                            double oriDist = -1, destDist = -1;
                            // 尝试根据 feature 获得到最近的铁路线上的点的距离
                            indexQuery2.NearestFeature(src.Shape as IPoint, out oriClosedFID, out oriDist);
                            indexQuery2.NearestFeature(destFeatCls.GetFeature(tarId + 1).Shape as IPoint, out destClosedFID, out destDist);
                            if (oriClosedFID == -1 || destClosedFID == -1)
                            {
                                Log.Log.Warn(string.Format("failed to find closed point, oriClosedFID: {0}, destClosedFID: {1}",
                                    oriClosedFID, destClosedFID));
                                continue;
                            }
                            // 计算走铁路时的总 cost，将cost 小于 cutOff 的终点添加到终点集合 ids 中
                            double railCost = 60 * (oriDist + destDist) / (speed * 1000) + totalCost;
                            // if (railCost <= cutOff) ids.Add(tarId);
                            if (railCost <= cutOff)
                            {
                                if (idsAndCosts.ContainsKey(tarId))
                                {
                                    if (railCost < idsAndCosts[tarId])
                                    {
                                        idsAndCosts[tarId] = railCost;
                                    }                                    
                                }
                                else
                                {
                                    idsAndCosts.Add(tarId, railCost);
                                }
                            }
                        }                                                                                           
                    }

                    // ids = ids.Distinct().ToList();
                    // 对企业点 src，统计其周边各种code类型的企业的信息
                    Dictionary<double, List<double>> codeStat = new Dictionary<double, List<double>>();
                    //foreach (int id in ids)
                    foreach (int id in idsAndCosts.Keys)
                    {
                        tar = tarFeatCls.GetFeature(id);
                        int code = int.Parse(tar.Value[idxCode].ToString());
                        if (!codeStat.ContainsKey(code))
                        {
                            codeStat.Add(code, new List<double>(){0, 0, 0});
                        }
                        codeStat[code][0] += double.Parse(tar.Value[idxPop].ToString());
                        codeStat[code][1] += double.Parse(tar.Value[idxQuan].ToString());
                        double weight = Math.Pow(1 - Math.Pow(idsAndCosts[id] / cutOff, 3), 3);
                        codeStat[code][2] += double.Parse(tar.Value[idxPop].ToString()) * weight;
                    }
                    result.Add(srcId, codeStat);

                    int sum = result.Sum(r => r.Value.Count());
                    if (sum >= 1000)
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
                foreach (KeyValuePair<string, Dictionary<double, List<double>>> kv in result)
                {                                                        
                    foreach (KeyValuePair<double, List<double>> ikv in kv.Value.OrderBy(k => k.Key))
                    {
                        sw.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", kv.Key, ikv.Key, ikv.Value[0], ikv.Value[1], ikv.Value[2]));
                    }
                }
                sw.Flush();
            }

        }

        public void setSpeed(double speed)
        {
            this.speed = speed;
        }

        public void setStartFID(int startFID)
        {
            this.startFID = startFID;
        }

        public void setStopFID(int stopFID)
        {
            this.stopFID = stopFID;
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
