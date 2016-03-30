using DataHelper.BaseUtil;
using ESRI.ArcGIS.Geodatabase;
using LogHelper;
using MapEdit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;
using Common.Data;
using Common;
using ESRI.ArcGIS.Geoprocessor;

namespace DataHelper.FuncSet.Kd.KdEachTable
{
    public class KdEachTableCircleCenter : KdEachTable
    {
        public KdEachTableCircleCenter(string filename, double diameter)
            : base(filename)
        {
            this.Diameter = diameter;
            this.CenterEnterprise = new CenterEnterprise(this.ExcelFile, diameter);
        }

        public override void CaculateParams()
        {
            GetEnterprises();
            GenerateShp();
            CaculateCenterEnterprise();
            GetMedium();
            GetKFunc();
        }

        public override void CaculateRandomParams()
        {
            GetRandomEnterprises();
            GetRandomMedium();
            GetRandomKFunc();
        }

        #region 真实值计算相关
        protected void GenerateShp()
        {
            FileIOInfo fileIo = new FileIOInfo(this.ExcelFile);
            if (!Directory.Exists(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt))
                Directory.CreateDirectory(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt);
            shpName = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + ".shp");
            bufferShpName = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, fileIo.FileNameWithoutExt + "_buffer.shp");

            if (EnterpriseFeatureCls == null || EnterpriseWorkspace == null)
            {
                if (!File.Exists(shpName))
                {
                    EnterpriseFeatureCls = DataPreProcess.CreateShpFile(Static.Fields, shpName);
                    EnterpriseFeatureClsBuffer = DataPreProcess.CreateShpFile(GlobalShpInfo.GeneratePolygonFields(), bufferShpName);
                    EnterpriseWorkspace = Geodatabase.GeodatabaseOp.Open_shapefile_Workspace(shpName);
                    ExcelData2Shp();
                    GenerateBufferShp();
                }
                else
                {
                    EnterpriseFeatureCls = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(shpName);
                    EnterpriseFeatureClsBuffer = Geodatabase.GeodatabaseOp.OpenShapefileAsFeatClass(bufferShpName);
                    EnterpriseWorkspace = Geodatabase.GeodatabaseOp.Open_shapefile_Workspace(shpName);
                }
            }
        }

        protected void CaculateCenterEnterprise()
        {
            IFeature featPt;
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            IFeatureCursor featCursor = this.EnterpriseFeatureClsBuffer.Search(null, false);
            while ((featPt = featCursor.NextFeature()) != null)
            {                
                spatialFilter.Geometry = featPt.Shape;
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                int count = EnterpriseFeatureCls.FeatureCount(spatialFilter);
                if (count > this.CenterEnterprise.EnterprisesInCircle)
                {
                    this.CenterEnterprise.EnterprisesInCircle = count;
                    this.CenterEnterprise.EnterpriseId = featPt.Value[featPt.Fields.FindField("ExcelId")].ToString();
                }
            }

            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = string.Format("ExcelId = '{0}'", this.CenterEnterprise.EnterpriseId);
            featCursor = this.EnterpriseFeatureClsBuffer.Search(queryFilter, false);
            featPt = featCursor.NextFeature();
            if (featPt != null)
            {     
                spatialFilter.Geometry = featPt.Shape;
                spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                IFeatureCursor cursor = EnterpriseFeatureCls.Search(spatialFilter, false);
                IFeature feature;
                while ((feature = cursor.NextFeature()) != null)
                {
                    Enterprise en = this.Enterprises.Find(x => x.ID.Equals(feature.Value[feature.Fields.FindField("ExcelId")].ToString()));
                    if (en != null)
                        this.CenterEnterprise.Enterprises.Add(en);
                }
            }
            PrintEnterprises();
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(featCursor);
        }

        protected bool GenerateBufferShp()
        {
            MapEditEnvOp.InitalMapEditEvn(EnterpriseWorkspace, EnterpriseFeatureClsBuffer);
            try
            {
                MapEditEnvOp.StartEditing();
                MapEditEnvOp.GetWorkspaceEdit().StartEditOperation();

                for (int i = 0; i < Enterprises.Count; i++)
                {
                    IFeature feature = EnterpriseFeatureClsBuffer.CreateFeature();
                    //feature.Shape.SpatialReference = GlobalShpInfo.SpatialReference;
                    //feature.Shape = ExcelToShp.CastPointByFunctionType(Enterprises[i].Point.X, Enterprises[i].Point.Y, FunctionType.KdEachTableCircle);
                    ITopologicalOperator topo = Enterprises[i].Point as ITopologicalOperator;
                    IGeometry geo = topo.Buffer(this.Diameter * 1000 / 2);
                    feature.Shape = geo;
                    feature.set_Value(EnterpriseFeatureClsBuffer.FindField("ExcelId"), Enterprises[i].ID);
                    feature.Store();
                }

                MapEditEnvOp.GetWorkspaceEdit().StopEditOperation();
                MapEditEnvOp.StopEditing(true);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return false;
            }
        }

        // 圆内的企业和圆心企业 [3/21/2016 mzl]
        protected void PrintEnterprises()
        {
            FileIOInfo fileIo = new FileIOInfo(this.ExcelFile);
            if (!Directory.Exists(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt))
                Directory.CreateDirectory(fileIo.FilePath + "\\" + fileIo.FileNameWithoutExt);
            string filename = System.IO.Path.Combine(fileIo.FilePath, fileIo.FileNameWithoutExt, "所有在圆内的企业.txt");
            if (File.Exists(filename))
                return;

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(string.Format("圆心企业ID:{0}", this.CenterEnterprise.EnterpriseId));
                sw.WriteLine(string.Format("共有企业{0}个", this.CenterEnterprise.Enterprises.Count));
                foreach (var e in this.CenterEnterprise.Enterprises)
                {
                    sw.WriteLine(string.Format("企业ID:{0};\t X坐标:{1};\t Y坐标:{2}", e.ID, e.Point.X, e.Point.Y));
                }
                sw.Flush();
                sw.Close();
            }
        }

        protected override void GetMedium()
        {
            // 怎么求解 [3/21/2016 mzl]
            FindMedium findMedium = new FindMedium(this.ExcelFile, this.CenterEnterprise.Enterprises, this.XValue);
            this.PointsDistances = findMedium.CaculateMediumAndGetPointDistance(0.0);
            this.Medium = findMedium.Mediums;
            this.MediumValue = Medium.ElementAt((0 + Medium.Count) / 2).DistanceFile.Distance;
            KdBase.Kd_Mdl.SetN(this.CenterEnterprise.Enterprises.Count);
        }

        protected override void GetKFunc()
        {
            int distance = this.Medium.ElementAt(this.Medium.Count - 1).DistanceFile.Distance - this.Medium.ElementAt(0).DistanceFile.Distance;
            this.KFunc = new KFunc(this.CenterEnterprise.EnterprisesInCircle, distance, this.MediumValue);
        }
        #endregion

        #region 模拟值计算相关
        protected override List<Enterprise> GetRandomEnterprises()
        {
            RandomEnterprises.Clear();

            string str_seed = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString();
            Random random = new Random(Int32.Parse(str_seed));
            for (int i = 0; i < KdBase.Kd_Mdl.N; i++)
            {
                int k = random.Next(this.CenterEnterprise.Enterprises.Count);
                if (!RandomEnterprises.Contains(this.SingleDogEnterprise[k])) RandomEnterprises.Add(this.SingleDogEnterprise[k]);
                else i--;
            }
            return RandomEnterprises;
        }
        #endregion

        public override void CaculateSimulateValue()
        {
            GetSimulateValue();
        }

        private bool ExcelData2Shp()
        {            
            MapEditEnvOp.InitalMapEditEvn(EnterpriseWorkspace, EnterpriseFeatureCls);
            try
            {
                MapEditEnvOp.StartEditing();
                MapEditEnvOp.GetWorkspaceEdit().StartEditOperation();

                for (int i = 0; i < Enterprises.Count; i++)
                {
                    IFeature feature = EnterpriseFeatureCls.CreateFeature();
                    //feature.Shape.SpatialReference = GlobalShpInfo.SpatialReference;
                    //feature.Shape = ExcelToShp.CastPointByFunctionType(Enterprises[i].Point.X, Enterprises[i].Point.Y, FunctionType.KdEachTableCircle);
                    feature.Shape = Enterprises[i].GeoPoint;
                    feature.set_Value(EnterpriseFeatureCls.FindField("ExcelId"), Enterprises[i].ID);
                    //feature.set_Value(EnterpriseFeatureCls.FindField("Man"), Enterprises[i].man);
                    feature.Store();
                }

                MapEditEnvOp.GetWorkspaceEdit().StopEditOperation();
                MapEditEnvOp.StopEditing(true);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.ToString());
                return false;
            }
        }

        public override void PrintTrueValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string trueValueFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTableCircleTable真实值计算结果.txt";
            base.PrintTrueValue(trueValueFile);
        }

        public override void PrintSimulateValue()
        {
            FileIOInfo fileIO = new FileIOInfo(this.ExcelFile);
            string simualteFile = fileIO.FilePath + "\\" + fileIO.FileNameWidthoutPath + "\\KdEachTableCircleTable模拟值计算结果.txt";
            base.PrintSimulateValue(simualteFile);
        }

        // 要创建的shp的全路径文件名 [3/21/2016 mzl]
        private string shpName = string.Empty;
        private string bufferShpName = string.Empty;
        // 创建的shp的featureclass [3/21/2016 mzl]
        private IFeatureClass EnterpriseFeatureCls = null;
        private IFeatureClass EnterpriseFeatureClsBuffer = null;
        // 创建的shp的workspace [3/21/2016 mzl]
        private IWorkspace EnterpriseWorkspace = null;
        public double Diameter { get; private set; }
        public CenterEnterprise CenterEnterprise { get; set; }
    }
}
