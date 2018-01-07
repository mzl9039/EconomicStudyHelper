using Common;
using DataHelper.BaseUtil;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataHelper.FuncSet.ShortestPath
{
    /// <summary>
    /// 计算行业内，一定时间范围内能达到的企业点（可以是直线距离到达，也可以是通过铁路到达）
    /// </summary>
    public class TransportationCircle
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
        /// 当前行业的excel名称
        /// </summary>
        private string excel = "";
        /// <summary>
        /// 时间范围，默认为0
        /// </summary>
        private double cutOff = 0.0d;
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
        /// 记录各个点的周边各类型企业的距离以及是直线距离还是铁路距离, 外层dict的key为企业ID，里面的dict的key为code, value 为pop和quan
        /// </summary>
        private Dictionary<string, Dictionary<double, Stat>> result = new Dictionary<string, Dictionary<double, Stat>>();
        /// <summary>
        /// 输出文件的名字
        /// </summary>
        private string output = "";

        public TransportationCircle(string excel, string cutOff)
        {
            this.cutOff = double.Parse(cutOff);
            this.excel = excel;
            try
            {                
                FileIOInfo fileIo = new FileIOInfo(excel);
                Directory.CreateDirectory(string.Format("{0}\\{1}", fileIo.FilePath, "shp"));
                tarShpName = string.Format("{0}\\{1}\\{2}.shp", fileIo.FilePath, "shp", fileIo.FileNameWithoutExt);
                IFields fields = GlobalShpInfo.GenerateTransportationCircleFields();
                tarFeatCls = DataPreProcess.CreateShpFile(fields, tarShpName);
                CopyAttrToShp();

                // 初始化时创建输出结果文件
                output = string.Format("{0}{1}{2}.csv", fileIo.FilePath, "\\", "output_" + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"));
                if (File.Exists(output))
                {
                    File.Delete(output);
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(output, FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(string.Format("OriginId, DestinationId, isRailway, cost"));
                    sw.Flush();
                }

                if (!shpNa.init(tarFeatCls, cutOff))
                {
                    Log.Log.Warn("初始化 ShpNA 失败");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Log.Error("处理excel文件失败，退出。", ex);
                return;
            }
        }

        /// <summary>
        /// 这个函数用来生成起点和终点的shp
        /// </summary>
        public void CopyAttrToShp()
        {
            List<Enterprise> enterprises = DataProcess.ReadExcel(excel, Static.Table, true, null, FunctionType.TransportationCirble);
            IFields fields = tarFeatCls.Fields;
            int idxId = fields.FindField("ExcelId");
            int idxMan = fields.FindField("Man");
            int idxLat2 = fields.FindField("lat2");
            int idxLng2 = fields.FindField("lng2");
            int idxID = fields.FindField("ID");
            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureBuffer featureBuffer = null;

                // Create an insert cursor.
                IFeatureCursor insertCursor = tarFeatCls.Insert(true);
                comReleaser.ManageLifetime(insertCursor);

                for (int i = 0; i < enterprises.Count; i++)
                {
                    featureBuffer = tarFeatCls.CreateFeatureBuffer();
                    comReleaser.ManageLifetime(featureBuffer);
                    featureBuffer.Value[idxId] = enterprises[i].ID;
                    featureBuffer.Value[idxMan] = enterprises[i].man;
                    featureBuffer.Value[idxLat2] = enterprises[i].Point.Y;
                    featureBuffer.Value[idxLng2] = enterprises[i].Point.X;
                    featureBuffer.Value[idxID] = i + 1;
                    featureBuffer.Shape = enterprises[i].GeoPoint;
                    insertCursor.InsertFeature(featureBuffer);
                }
                insertCursor.Flush();
            }
        }

        public void caculateTransportationCircle()
        {
            try
            {
                IFeature src, tar, line;
                int featureNum = tarFeatCls.FeatureCount(null);

                int idxId = tarFeatCls.FindField("ID");
                if (idxId < 0)
                {
                    Log.Log.Warn("找不到字段 ID");
                    return;
                }
                IFeatureClass lines, destFeatCls;
                for (int i = 0; i < featureNum; i++)
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
                    Dictionary<double, Stat> codeStat = new Dictionary<double, Stat>();
                    for (int j = 0; j < featureNum; j++)
                    {
                        if (i == j) continue;
                        tar = tarFeatCls.GetFeature(j);                        
                        // 计算直线的总 cost
                        //IProximityOperator proximityOp = (src.Shape as IPoint) as IProximityOperator;
                        double excelDistance = SPUtils.caculateStraightDistance((src.Shape as IPoint), (tar.Shape as IPoint));
                        double excelCost = 60 * excelDistance / speed;
                        if (excelCost > cutOff)
                        {
                            continue;
                        }
                        // 如果 cost 在 cutOff 范围内，则将目标点添加到 集合中
                        codeStat.Add(j + 1, new Stat("N", excelCost));
                    }

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
                            int tarFId = int.Parse(names[1]) - 1;                            
                            tar = tarFeatCls.GetFeature(tarFId);
                            if (tar == null)
                            {
                                Log.Log.Warn(string.Format("找不到shp图层中ID为：{0}的Feature.", name[1]));
                                continue;
                            }

                            int oriClosedFID = -1, destClosedFID = -1;
                            double oriDist = -1, destDist = -1;
                            // 尝试根据 feature 获得到最近的铁路线上的点的距离
                            indexQuery2.NearestFeature(src.Shape as IPoint, out oriClosedFID, out oriDist);
                            indexQuery2.NearestFeature(destFeatCls.GetFeature(tarFId + 1).Shape as IPoint, out destClosedFID, out destDist);
                            if (oriClosedFID == -1 || destClosedFID == -1)
                            {
                                Log.Log.Warn(string.Format("failed to find closed point, oriClosedFID: {0}, destClosedFID: {1}",
                                    oriClosedFID, destClosedFID));
                                continue;
                            }
                            // 计算走铁路时的总 cost，将cost 小于 cutOff 的终点添加到终点集合 ids 中
                            double railCost = 60 * (oriDist + destDist) / (speed * 1000) + totalCost;
                            if (railCost <= cutOff)
                            {
                                int tarID = tarFId + 1;
                                if (codeStat.ContainsKey(tarID))
                                {
                                    if (codeStat[tarID].cost > railCost) codeStat[tarID] = new Stat("Y", railCost);
                                }
                                else
                                {
                                    codeStat.Add(tarID, new Stat("Y", railCost));
                                }
                            }
                        }
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
            catch (Exception ex)
            {
                Log.Log.Error(string.Format("计算行业:{0}内企业点的交通圈失败，异常退出", excel), ex);
                return;
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
                foreach (KeyValuePair<string, Dictionary<double, Stat>> kv in result)
                {
                    foreach (KeyValuePair<double, Stat> ikv in kv.Value.OrderBy(k => k.Key))
                    {
                        sw.WriteLine(string.Format("{0}, {1}, {2}", kv.Key, ikv.Key, ikv.Value.ToString()));
                    }
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

    public class Stat
    {
        /// <summary>
        /// 是否是铁路线的花费
        /// </summary>
        public string isRailWay;
        /// <summary>
        /// 花费值是多少
        /// </summary>
        public double cost;

        public Stat(string isRailWay, double cost)
        {
            this.isRailWay = isRailWay;
            this.cost = cost;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", isRailWay, cost);
        }
    }
}
