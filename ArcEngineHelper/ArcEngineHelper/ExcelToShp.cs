/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-17
 * 时间: 13:28
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using ESRI.ArcGIS.Geometry;
// using LogHelper;
using ESRI.ArcGIS.esriSystem;
using DataHelper.BaseUtil;

namespace DataHelper
{
	/// <summary>
	/// Description of ExcelToShp.
	/// </summary>
	public class ExcelToShp
	{
        public static ISpatialReference spatialReference = null;

		public static IPoint CastPointByFunctionType(double lng, double lat, FunctionType ft) {
			IPoint point = null;
			switch (ft) {
				case FunctionType.Default:
					point = GetBeijing54ProjFromWGS84(lng, lat);
					break;
				case FunctionType.Kd:
					point = GetBeijing54ProjFromWGS84(lng, lat);
					break;
				case FunctionType.PopulationStatics:
					point = GetBeijing54ProjFromWGS84(lng, lat);
					break;
				case FunctionType.EGIndex:
                    point = GetBeijing54ProjFromWGS84(lng, lat);
					break;
				case FunctionType.EGIndexRobust:
                    point = GetBeijing54ProjFromWGS84(lng, lat);
					break;
                case FunctionType.KdEachTable:
                    point = GetBeijing54ProjFromWGS84(lng, lat);
                    break;
                case FunctionType.KdEachTablbCara:
                    point = GetBeijing54ProjFromWGS84(lng, lat);
                    break;
                case FunctionType.KdEachTableCircle:
                    point = GetBeijing54ProjFromWGS84(lng, lat);
                    break;
				default:
					throw new Exception("Invalid value for FunctionType");
			}
		    return point;
		}

		public static IPoint GetBeijing54ProjFromWGS84(double lng, double lat){
			IPoint point = GetBeijing54GCSFromWGS84(lng, lat);
            ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();
            // IProjectedCoordinateSystem pcs = GlobalShpInfo.SpatialReference as IProjectedCoordinateSystem;
            // point.Project(pcs);
            point.Project(Static.SpatialReference);
            //point.Project(srf.CreateProjectedCoordinateSystem(2431 + (int)((point.X - 100.5) / 3)));
            //point.Project(srf.CreateProjectedCoordinateSystem(21483));

            return point;
		}
		
		public static IPoint GetBeijing54GCSFromWGS84(double lng, double lat){
			IPoint point = new PointClass();
			point.PutCoords(lng, lat);
			ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();            
            point.SpatialReference = srf.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_Beijing1954);
            //point.SpatialReference = GlobalShpInfo.SpatialReference;
            //spatialReference = srf.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_Beijing1954);
            //point.Project(spatialReference);
            
            return point;
		}        

        // <summary>
        /// 从地理坐标转换到投影坐标
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度，南半球为负数</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="zone">带区（1-60，从-180到+180，6度带）</param>
        public static void GeoToPrj(double longitude, double latitude, out double x, out double y, out int zone)
        {
            ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            IGeographicCoordinateSystem pGeoCoordSys = pSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            int startprjnum = 32601;
            if (latitude < 0) startprjnum = 32701;
            zone = (int)Math.Round(((longitude + 3) / 6)) + 30;
            int prjnum = startprjnum + zone - 1;
            //IProjectedCoordinateSystem pPrjCoordSys = pSpatialReferenceFactory.CreateProjectedCoordinateSystem(prjnum);
            IProjectedCoordinateSystem pPrjCoordSys = pSpatialReferenceFactory.CreateProjectedCoordinateSystem(32649);

            IPoint pt = new PointClass();
            pt.PutCoords(longitude, latitude);
            IGeometry geo = (IGeometry)pt;
            geo.SpatialReference = pGeoCoordSys;
            geo.Project(pPrjCoordSys);
            x = pt.X;
            y = pt.Y;
            if (latitude < 0)
                y = 0 - y;
        }
        /// <summary>
        /// 从投影坐标转换到地理坐标
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y，南半球为负数</param>
        /// <param name="zone">带区（1-60，从-180到+180，6度带）</param>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        public static void PrjToGeo(double x, double y, int zone, out double longitude, out double latitude)
        {
            ISpatialReferenceFactory pSpatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            IGeographicCoordinateSystem pGeoCoordSys = pSpatialReferenceFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            int startprjnum = 32601;
            bool South = false;
            if (y < 0)
            {
                South = true;
                y = 0 - y;
            }
            if (South) startprjnum = 32701;
            int prjnum = startprjnum + zone - 1;
            IProjectedCoordinateSystem pPrjCoordSys = pSpatialReferenceFactory.CreateProjectedCoordinateSystem(prjnum);

            IPoint pt = new PointClass();
            pt.PutCoords(x, y);
            IGeometry geo = pt as IGeometry;
            geo.SpatialReference = pPrjCoordSys;
            geo.Project(pGeoCoordSys);
            longitude = pt.X;
            latitude = pt.Y;
            if (South) latitude = 0 - latitude;
        }

        public static bool IsPointValid(IPoint point, IGeometry geo) {
			bool result = false;
			try {
				IGeometry geoPoint = point as IGeometry;
				IRelationalOperator relationOperator = geo as IRelationalOperator;
				result = relationOperator.Contains(geoPoint);
			} catch (Exception ex) {
				Log.Log.Error(ex.ToString());
				throw;
			}
			return result;
		}
	}
}
