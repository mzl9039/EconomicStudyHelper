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
using LogHelper;

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
            point.Project(srf.CreateProjectedCoordinateSystem(2431 + (int)((point.X - 100.5) / 3)));
            
            return point;
		}
		
		public static IPoint GetBeijing54GCSFromWGS84(double lng, double lat){
			IPoint point = new PointClass();
			point.PutCoords(lng, lat);
			ISpatialReferenceFactory srf = new SpatialReferenceEnvironmentClass();            
            point.SpatialReference = srf.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            //point.SpatialReference = GlobalShpInfo.SpatialReference;
            spatialReference = srf.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_Beijing1954);
            point.Project(spatialReference);
            
            return point;
		}



		public static bool IsPointValid(IPoint point, IGeometry geo) {
			bool result = false;
			try {
				IGeometry geoPoint = point as IGeometry;
				IRelationalOperator relationOperator = geo as IRelationalOperator;
				result = relationOperator.Contains(geoPoint);
			} catch (Exception ex) {
				Log.WriteError(ex.StackTrace);
				throw;
			}
			return result;
		}
	}
}
