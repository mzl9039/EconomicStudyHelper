using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using LogHelper;
using DataHelper.BaseUtil;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.ADF;
using System.IO;
using System.Data;
using OfficeOpenXml;

namespace DataHelper.FuncSet.Kd.KdEachTable
{
    public class KdEachTableMultiCircleCenter : KdEachTable
    {
        public KdEachTableMultiCircleCenter(string filename, MultiCircleDiameters multiCircleDiameter)
            : base(filename)
        {
            FileIOInfo fileIo = new FileIOInfo(filename);
            if (File.Exists(System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + ".mdb")))
                File.Delete(System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + ".mdb"));

            //Workspace = Geodatabase.GeodatabaseOp.Create_pGDB_Workspace(System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt), fileIo.FileNameWithoutExt);
            MultiCircleDiameters = multiCircleDiameter;
            Diameters = MultiCircleDiameters.Diameters;
            Diameters.RemoveAll(x => x == 0);
            IsMultiCircle = (Diameters.Count(x => x > 0) > 1);
            ExcelEnterprises = this.SingleDogEnterprise;
            densityType = Static.densityType;
        }

        public override void CaculateParams()
        {
            GetEnterprises();
            // 由于需要找到圆后把圆内的企业排除掉再找下一个，所以这里把excel内的企业复制到一个变量中
            ExcelEnterprises.AddRange(this.SingleDogEnterprise);
        }

        public void CaculateCircleEnterprise()
        {
            if (IsMultiCircle)
                CaculateMultiCircleEnterprise();
            else
                CaculateSingleCircleEnterprise();
        }

        #region 输出圆的信息
        /// <summary>
        /// 输出每个圆的信息，包括圆心，圆内所有企业的id；
        /// 输出多个圆圆心的两两距离，多个圆的shp
        /// </summary>
        public void PrintCircleEnterprise()
        {
            FileIOInfo fileIo = new FileIOInfo(ExcelFile);
            string filename = string.Empty;
            for (int i=0;i<CenterEnterprises.Count;i++)
            {
                filename = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, "第" + i + "个圆(直径" + CenterEnterprises[i].Diameter + ").txt");
                FileIOInfo file = new FileIOInfo(filename);
                CenterEnterprises[i].PrintEnterprise(filename);
            }

            PrintCircleShp(CenterEnterprises, "AllCircles.shp");
            PrintCirclesDistance(CenterEnterprises);
        }

        /// <summary>
        /// 找到多个圆后，生成这些圆的shp
        /// </summary>
        public void PrintCircleShp(List<CenterEnterprise> CenterEnterprises, string output)
        {        
            try
            {
                FileIOInfo fileIo = new FileIOInfo(ExcelFile);
                CircleShpFileName = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + output);
                Static.Fields = GlobalShpInfo.GenerateCircleFields();
                IFeatureClass shpFeatureClass = DataPreProcess.CreateShpFile(Static.Fields, CircleShpFileName);
                int idxCircleId = Static.Fields.FindField("CircleId"),
                    idxDiameter = Static.Fields.FindField("Diameter"),
                    idxNumDensity = Static.Fields.FindField("NumDensity"),
                    idxScaleDensity = Static.Fields.FindField("ScaleDensity");
                using (ComReleaser comReleaser = new ComReleaser())
                {
                    IFeatureBuffer featureBuffer = null;

                    // Create an insert cursor.
                    IFeatureCursor insertCursor = shpFeatureClass.Insert(true);
                    comReleaser.ManageLifetime(insertCursor);

                    for (int i = 0; i < CenterEnterprises.Count; i++)
                    {
                        featureBuffer = shpFeatureClass.CreateFeatureBuffer();
                        comReleaser.ManageLifetime(featureBuffer);
                        CenterEnterprise centerEnterprise = CenterEnterprises[i];
                        double circleArea = (Math.PI * Math.Pow(centerEnterprise.Diameter / 2, 2));
                        IPoint point = new PointClass();
                        Enterprise enterprise = this.SingleDogEnterprise.Find(x => x.ID.Equals(centerEnterprise.EnterpriseId));
                        point.PutCoords(enterprise.Point.X, enterprise.Point.Y);

                        ICircularArc circularArc = new CircularArcClass();
                        IConstructCircularArc construtionCircularArc = circularArc as IConstructCircularArc;
                        // TODO 生成圆Polygon [5/22/2016 17:45:38 mzl]
                        construtionCircularArc.ConstructCircle(point, centerEnterprise.Diameter * 1000 / 2, true);
                        ISegmentCollection segmentCollection = new RingClass();
                        object missing = Type.Missing;
                        segmentCollection.AddSegment((ISegment)circularArc, missing, missing);
                        IRing ring = (IRing)segmentCollection;
                        ring.Close();
                        IGeometryCollection geoCol = new PolygonClass();
                        geoCol.AddGeometry(ring, missing, missing);
                        IPolygon polygon = (IPolygon)geoCol;
                        featureBuffer.Shape = polygon as IGeometry;
                        featureBuffer.Value[idxCircleId] = centerEnterprise.EnterpriseId;
                        featureBuffer.Value[idxDiameter] = centerEnterprise.Diameter;
                        switch (Static.densityType)
                        {
                            case DensityType.Diameter:
                                featureBuffer.Value[idxNumDensity] = centerEnterprise.Enterprises.Count / circleArea;
                                featureBuffer.Value[idxScaleDensity] = 0;
                                break;
                            case DensityType.Scale:
                                featureBuffer.Value[idxScaleDensity] = centerEnterprise.Enterprises.Sum(e => e.man) / circleArea;
                                featureBuffer.Value[idxNumDensity] = 0;
                                break;
                            default:
                                break;
                        }
                        insertCursor.InsertFeature(featureBuffer);
                    }
                    insertCursor.Flush();
                }
            }
            catch (System.Exception ex)
            {
                Log.WriteLog(ex.Message);
            }                            
        }

        /// <summary>
        /// 找到多个圆后，计算两两之间圆心的距离
        /// </summary>
        /// <param name="CenterEnterprises"></param>
        public void PrintCirclesDistance(List<CenterEnterprise> CenterEnterprises)
        {
            FileIOInfo fileIo = new FileIOInfo(ExcelFile);
            CircleDistancesFileName = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + "企业圆心两两距离.txt");
            using (FileStream fs = new FileStream(CircleDistancesFileName, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                for (int i=0;i<CenterEnterprises.Count;i++)
                {
                    for (int j=i+1;j<CenterEnterprises.Count;j++)
                    {
                        CenterEnterprise ceIn = CenterEnterprises[j], ceOut = CenterEnterprises[i];
                        Enterprise enIn = this.SingleDogEnterprise.Find(x => x.ID.Equals(ceIn.EnterpriseId)),
                            enOut = this.SingleDogEnterprise.Find(x => x.ID.Equals(ceOut.EnterpriseId));
                        string content = string.Format("第{0}个圆（ID:{1}）与第{2}个圆（ID：{3}）的圆距离为：{4}", i, ceOut.EnterpriseId,
                            j, ceIn.EnterpriseId, PointsDistance(enIn.Point, enOut.Point));
                        sw.WriteLine(content);
                    }
                }
                sw.Flush(); sw.Close();
            }
        }
        #endregion

        /// <summary>
        /// 查找给定查找范围和给定直径的包含企业数最多的圆
        /// </summary>
        private CenterEnterprise BaseCaculateCenterEnterprise(List<Enterprise> enterprises, double diameter)
        {
            if (enterprises == null || enterprises.Count <= 0 || diameter <= 0.0)
            {
                Log.WriteLog("KdEachTableMultiCircleCenter.BaseCaculateCenterEnterprise:企业集合为空或无数据，或传入的直径大小为0");
                return null;
            }

            CenterEnterprise enterpriseCircle = new CenterEnterprise();
            enterpriseCircle.EnterpriseId = enterprises[0].ID;
            enterpriseCircle.Enterprises = new List<Enterprise>();
            for (int i=0;i< enterprises.Count;i++)
            {
                Enterprise en = enterprises[i];
                List<Enterprise> templist = (from e in enterprises.AsParallel()
                                             let distance = (Math.Sqrt((en.Point.X - e.Point.X) * (en.Point.X - e.Point.X) +
                                                      (en.Point.Y - e.Point.Y) * (en.Point.Y - e.Point.Y)) / 1000)
                                             where distance != 0 && distance <= (diameter / 2)
                                             select e).ToList();

                if (this.densityType == DensityType.Diameter && templist.Count > enterpriseCircle.Enterprises.Count)
                {
                    enterpriseCircle.EnterpriseId = en.ID;
                    enterpriseCircle.Enterprises = templist;
                    enterpriseCircle.Diameter = diameter;
                    enterpriseCircle.Excel = ExcelFile;
                }
                else if (this.densityType == DensityType.Scale && templist.Sum(x => x.man) > enterpriseCircle.Enterprises.Sum(x => x.man))
                {
                    enterpriseCircle.EnterpriseId = en.ID;
                    enterpriseCircle.Enterprises = templist;
                    enterpriseCircle.Diameter = diameter;
                    enterpriseCircle.Excel = ExcelFile;
                }
            }
            return enterpriseCircle;
        }

        #region 计算多圆，事实是2个圆
        /// <summary>
        /// 便于在多圆情况下调用
        /// </summary>
        /// <param name="enterprises"></param>
        /// <param name="diameter"></param>
        private void CaculateCenterEnterprise(List<Enterprise> enterprises, double diameter)
        {            
            CenterEnterprise enterpriseCircle = BaseCaculateCenterEnterprise(enterprises, diameter);
            if (enterpriseCircle != null)
            {
                // 找到第一个圆的信息,并把这个圆内的企业从这个行业内去掉 [5/15/2016 19:56:05 mzl]
                CenterEnterprises.Add(enterpriseCircle);
                ExcelEnterprises.RemoveAll(x => enterpriseCircle.Enterprises.Exists(e => e.ID == x.ID));
            }
        }

        /// <summary>
        /// 计算多圆情况，由于现在多圆只是两个圆
        /// </summary>
        private void CaculateMultiCircleEnterprise()
        {
            // 先计算第一个圆 [5/15/2016 20:17:28 mzl]
            CaculateCenterEnterprise(ExcelEnterprises, Diameters[0]);
            // 再计算第二个圆,因为计算第一个圆后，ExcelEnterprises已经去掉了第一个圆内企业的信息 [5/15/2016 20:20:17 mzl]
            CaculateCenterEnterprise(ExcelEnterprises, Diameters[1]);
        }
        #endregion

        #region 计算检查单圆的情况，这种情况要查找是否还有其它的圆，采用递归实现
        /// <summary>
        /// 计算单圆情况，这种情况下，先计算第一个圆，然后递归查找其它可能的圆
        /// </summary>
        private void CaculateSingleCircleEnterprise()
        {
            // 先计算第一个圆,计算完成后计算其浓度，并与初始excel里的浓度比较，保留较高的那个浓度 [5/15/2016 20:25:15 mzl]
            CaculateCenterEnterprise(ExcelEnterprises, Diameters[0]);
            double density = CaculateDensity(CenterEnterprises[0]);
            if (density > MultiCircleDiameters.Density)
                MultiCircleDiameters.Density = density;
            // 递归调用查找其它的圆 [5/15/2016 20:25:37 mzl]
            CenterEnterprise enterpriseCircle = null;
            do 
            {
                enterpriseCircle = SearchOtherCircleEnterprises(ExcelEnterprises, MultiCircleDiameters.Diameters[0], null);
                if (enterpriseCircle != null)
                {
                    CenterEnterprises.Add(enterpriseCircle);
                    ExcelEnterprises.RemoveAll(x => enterpriseCircle.Enterprises.Exists(e => e.ID == x.ID));
                }
            } while (enterpriseCircle != null);            
        }

        private CenterEnterprise SearchOtherCircleEnterprises(List<Enterprise> curEnterprises, double curDiameter, CenterEnterprise lastEnterpriseCircle)
        {
            if (curDiameter < 20)
                return null;

            CenterEnterprise curEnterpriseCircle = BaseCaculateCenterEnterprise(curEnterprises, curDiameter);
            
            double curDensity = CaculateDensity(curEnterpriseCircle);
            double lastDensity = lastEnterpriseCircle == null
                                ? 0.0
                                : CaculateDensity(lastEnterpriseCircle);

            if (curDensity > MultiCircleDiameters.Density)
                return SearchOtherCircleEnterprises(curEnterprises, (curDiameter + MultiCircleDiameters.Diameters[0]) / 2, curEnterpriseCircle);
            else if (curDensity < MultiCircleDiameters.Density && lastDensity < MultiCircleDiameters.Density)
                return SearchOtherCircleEnterprises(curEnterprises, curDiameter / 2, curEnterpriseCircle);
            else                          
                return lastEnterpriseCircle;                            
        }
        #endregion

        public void FindEnterpriseDistribution()
        {
            // 按半径从大到小排列
            CenterEnterprises.Sort((x, y) => y.Diameter.CompareTo(x.Diameter));
            CenterEnterprise centerEnterprise = CenterEnterprises[0];
            FindEnterpriseDistributionInBiggestCircle(centerEnterprise);
        }

        /// <summary>
        /// 在最大的圆内，找到小圆的分布情况
        /// </summary>
        /// <param name="centerEnterprise"></param>
        private void FindEnterpriseDistributionInBiggestCircle(CenterEnterprise centerEnterprise)
        {
            List<CenterEnterprise> result = new List<CenterEnterprise>();
            switch (densityType)
            {
                case DensityType.Diameter:
                    result = NumFindEnterpriseDistributionInBiggestCircle(centerEnterprise);
                    break;
                case DensityType.Scale:
                    result = ScaleFindEnterpriseDistributionInBiggestCircle(centerEnterprise);
                    break;
                default:
                    break;
            }
            PrintCircleShp(result, "AllLittleCircles.shp");
        }

        /// <summary>
        /// 在最大的圆内，找小圆的信息，这是数量浓度的方法
        /// </summary>
        /// <returns></returns>
        private List<CenterEnterprise> NumFindEnterpriseDistributionInBiggestCircle(CenterEnterprise centerEns)
        {
            List<CenterEnterprise> result = new List<CenterEnterprise>();
            List<Enterprise> Enterprises = centerEns.Enterprises;
            double totalEnterprises = Enterprises.Count;
            // 当剩余的企业数量不足总数的20%的时候，就停止查找 [6/4/2016 15:30:15 mzl]
            while(Enterprises.Count / totalEnterprises > 0.2)
            {
                // 直径为20，即要查找的圆，半径为10 [6/4/2016 15:34:43 mzl]
                CenterEnterprise centerEnterprise = BaseCaculateCenterEnterprise(Enterprises, 20);
                if (centerEnterprise != null && centerEnterprise.Enterprises.Count > 0)
                {
                    result.Add(centerEnterprise);
                    Enterprises.RemoveAll(x => centerEnterprise.Enterprises.Exists(e => e.ID.Equals(x.ID)));
                }
                // 排除那种找不到小圆，但剩下的总数仍然在总企业数的20%以上
                if (centerEnterprise.Enterprises.Count <= 0)
                    break;
            }
            return result;
        }

        /// <summary>
        /// 在最大的圆内，找小圆的信息，这是规模浓度的方法
        /// </summary>
        /// <param name="centerEns"></param>
        /// <returns></returns>
        private List<CenterEnterprise> ScaleFindEnterpriseDistributionInBiggestCircle(CenterEnterprise centerEns)
        {
            List<CenterEnterprise> result = new List<CenterEnterprise>();
            List<Enterprise> Enterprises = centerEns.Enterprises;
            double totalScale = Enterprises.Sum(x => x.man);
            while(Enterprises.Sum(x => x.man) / totalScale > 0.2)
            {
                CenterEnterprise centerEnterprise = BaseScaleFindEnterpriseDistributionInBiggestCircle(Enterprises, centerEns.Diameter);
                if (centerEnterprise != null && centerEnterprise.Enterprises.Count > 0)
                {
                    result.Add(centerEnterprise);
                    Enterprises.RemoveAll(x => centerEnterprise.Enterprises.Exists(e => e.ID.Equals(x.ID)));
                }
                if (centerEnterprise.Enterprises.Count <= 0)
                    break;
            }
            return result;
        }

        private CenterEnterprise BaseScaleFindEnterpriseDistributionInBiggestCircle(List<Enterprise> Enterprises, double diameter)
        {
            CenterEnterprise result = null;
            double curDiameter = 20;
            while(curDiameter < diameter)
            {
                CenterEnterprise curCenterEnterprise = BaseCaculateCenterEnterprise(Enterprises, curDiameter);
                if (result == null)                
                    result = curCenterEnterprise;                
                else
                {
                    if (CaculateDensity(curCenterEnterprise) > CaculateDensity(result))
                        result = curCenterEnterprise;
                }                    
                
                curDiameter += 5;
            }
            return result;
        }

        public void SetEnterpriseNumsInCountry(ref DataTable table, IFeatureClass shpFeatureClass)
        {
            try
            {
                FileIOInfo fileIo = new FileIOInfo(this.ExcelFile);
                string enterprise = fileIo.FileNameWithoutExt.Substring(0, 4);
                using (ComReleaser comReleaser = new ComReleaser())
                {
                    IFeatureCursor cursor;
                    IFeature feature;
                    IQueryFilter filter = new QueryFilterClass();
                    int idxName = shpFeatureClass.Fields.FindField("NAME");                   
                    
                    for (int i = 0; i < this.CenterEnterprises.Count; i++)
                    {
                        for (int j = 0; j < this.CenterEnterprises[i].Enterprises.Count; i++)
                        {
                            for (int k=0;k<table.Rows.Count;k++)
                            {
                                cursor = Geodatabase.GeodatabaseOp.SpatialRelQurey(shpFeatureClass,
                                this.CenterEnterprises[i].Enterprises[j].GeoPoint,
                                esriSpatialRelEnum.esriSpatialRelWithin, string.Format("NAME LIKE '{0}'", table.Rows[k][0].ToString()),
                                esriSearchOrder.esriSearchOrderAttribute, null, false);
                                comReleaser.ManageLifetime(cursor);
                                while ((feature = cursor.NextFeature()) != null)
                                {
                                    string name = feature.Fields.Field[idxName].ToString();
                                    if (table.Rows[k][name] == null || table.Rows[k][name].ToString() == "")
                                    {
                                        table.Rows[k][name] = 1;
                                    }
                                    else
                                    {
                                        table.Rows[k][name] = int.Parse(table.Rows[k][name].ToString()) + 1;
                                    }
                                }
                            }                                                       
                        }
                    }
                }                                
            }
            catch (System.Exception ex)
            {
                Log.WriteError("出错文件：" + shpFeatureClass.AliasName + " " + ex.Message);
                throw;
            }            
        }

        #region 基本计算方法
        #region 计算浓度
        /// <summary>
        /// 计算浓度，调用全局的浓度类型，调用相应类型的浓度计算函数
        /// </summary>
        /// <param name="centerEnterprise"></param>
        /// <returns></returns>
        private double CaculateDensity(CenterEnterprise centerEnterprise)
        {
            double result = 0.0;
            switch (densityType)
            {
                case DensityType.Diameter:
                    result = DiameterDensity(centerEnterprise);
                    break;
                case DensityType.Scale:
                    result = ScaleDensity(centerEnterprise);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// 半径计算浓度
        /// </summary>
        /// <param name="centerEnterprise"></param>
        /// <returns></returns>
        private double DiameterDensity(CenterEnterprise centerEnterprise)
        {
            if (centerEnterprise.Diameter <= 0)
                return 0;
            return centerEnterprise.Enterprises.Count
                             / (Math.PI * Math.Pow(centerEnterprise.Diameter / 2, 2));
        }

        /// <summary>
        /// 人口计算浓度
        /// </summary>
        /// <param name="centerEnterprise"></param>
        /// <returns></returns>
        private double ScaleDensity(CenterEnterprise centerEnterprise)
        {
            if (centerEnterprise.Diameter <= 0)
                return 0;
            return centerEnterprise.Enterprises.Sum(x => x.man) / (Math.PI * Math.Pow(centerEnterprise.Diameter / 2, 2));
        }
        #endregion

        private double PointsDistance(Common.Point pt1, Common.Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
        }

        public static void TestPointPosition(List<Enterprise> Enterprises)
        {
            IFields Fields = GlobalShpInfo.GeneratePointFields();
            string shpname = "E:\\test.shp";
            IFeatureClass FeatureClass = DataPreProcess.CreateShpFile(Fields, shpname);
            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureBuffer featureBuffer = null;

                // Create an insert cursor.
                IFeatureCursor insertCursor = FeatureClass.Insert(true);
                comReleaser.ManageLifetime(insertCursor);

                for (int i=0;i<Enterprises.Count;i++)
                {
                    featureBuffer = FeatureClass.CreateFeatureBuffer();
                    comReleaser.ManageLifetime(featureBuffer);

                    IPoint point = new PointClass();
                    point.PutCoords(Enterprises[i].Point.X, Enterprises[i].Point.Y);
                    featureBuffer.Shape = point as IGeometry;

                    insertCursor.InsertFeature(featureBuffer);
                }

                insertCursor.Flush();
            }
        }
        #endregion

        // excel内的企业信息 [5/15/2016 17:16:41 mzl]
        private List<Enterprise> ExcelEnterprises = new List<Enterprise>();
        // 当前的excel是多圆还是单圆 [5/15/2016 16:20:12 mzl]
        private bool IsMultiCircle = false;
        // 每个excel都可能有多个半径 [5/16/2016 7:28:31 mzl]
        private List<double> Diameters = new List<double>();
        // 行业内的圆及圆内企业 [5/16/2016 7:32:29 mzl]
        private List<CenterEnterprise> CenterEnterprises = new List<CenterEnterprise>();
        // 当前excel对应行业的直径以及浓度信息 [5/15/2016 16:54:56 mzl]
        MultiCircleDiameters MultiCircleDiameters = null;
        // 浓度类型 [5/22/2016 16:58:05 mzl]
        private DensityType densityType = DensityType.Diameter;
        // 行业内圆shp的文件名 [5/22/2016 16:58:59 mzl]
        private string CircleShpFileName = string.Empty;
        // 圆心之间两两距离的txt [6/4/2016 14:40:35 mzl]
        private string CircleDistancesFileName = string.Empty;
    }
}
