/*
 * 由SharpDevelop创建。
 * 用户： mzl
 * 日期: 2015-10-18
 * 时间: 17:02
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using Common;
using LogHelper;
using MapEdit;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;

namespace DataHelper.FuncSet.EGIndex
{
	/// <summary>
	/// Description of BaseUtil.
	/// </summary>
	public class EGIndexBaseUtil
	{
		public static bool ExcelData2Shp(IWorkspace ws, IFeatureClass fc, List<Enterprise> enterprises) {
			if (ws == null || fc == null || enterprises == null)
				return false;
			
			MapEditEnvOp.InitalMapEditEvn(ws, fc);
            try
            {
                MapEditEnvOp.StartEditing();
                MapEditEnvOp.GetWorkspaceEdit().StartEditOperation();

                for (int i = 0; i < enterprises.Count; i++)
                {                    
                    IFeature feature = fc.CreateFeature();
                    //feature.Shape.SpatialReference = GlobalShpInfo.SpatialReference;
                    feature.Shape = enterprises[i].Point;
                    feature.set_Value(fc.FindField("ExcelId"), enterprises[i].ID);
                    feature.set_Value(fc.FindField("Man"), enterprises[i].man);
                    feature.Store();
                }

                MapEditEnvOp.GetWorkspaceEdit().StopEditOperation();
                MapEditEnvOp.StopEditing(true);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.StackTrace);
                //throw ex;
                return false;
            }
		}
		
        /// <summary>
        /// 按 查询条件 查询符合条件的点集，根据获得的点获取相应的企业信息
        /// 由于不仅要获得该地区某行业的信息，还要获取所有行业的信息，所以
        /// 需要进行两次查询
        /// </summary>
        /// <param name="fcCounty">县级行政区划的Cursor</param>
        /// <param name="egEnterprise"></param>
        /// <param name="whereClause"></param>
        /// <returns></returns>
        public static List<string> CaculateSaiAndSi(ConcurrentDictionary<string, EGEnterpriseData> egEnterpisesDataDir, IFeatureClass fclsCounty, EGType egType)
        {
            if (egEnterpisesDataDir == null || fclsCounty == null)
            {
                return null;
            }
            List<string> result = new List<string>();

            ConcurrentBag<Enterprise> enterprises = new ConcurrentBag<Enterprise>();
            double totalStaff = egEnterpisesDataDir.Sum(kv => kv.Value.TotalStaff);
            
            foreach (var kv in egEnterpisesDataDir)
            {
                enterprises.Union(kv.Value.Enterprises);
            }

            try
            {
                #region 注释foreach
                foreach (var kv in egEnterpisesDataDir)
                {
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();

                    double Sai = 0.0, Si = 0.0;
                    double SumSaiSi = 0.0, SumSi = 0.0;
                    try
                    {
                        // 遍历中国所有的县
                        IFeatureCursor fcCounty = Geodatabase.GeodatabaseOp.QuerySearch(fclsCounty, null, null, false);
                        IFeature feature;
                        while ((feature = fcCounty.NextFeature()) != null)
                        {
                            IGeometry geo = feature.Shape;

                            // 选择所有行业里在这个地区内的企业
                            ConcurrentBag<Enterprise> AllEnterprises = new ConcurrentBag<Enterprise>();
                            enterprises.AsParallel().ForAll(e =>
                            {
                                IRelationalOperator relationalOperator = geo as IRelationalOperator;
                                if (relationalOperator.Contains(e.Point))
                                    AllEnterprises.Add(e);
                            });
                            // 若全部企业在该地区没有数据，则A企业在该地区一定没有数据
                            if (AllEnterprises != null && AllEnterprises.Count <= 0)
                                continue;

                            Si = AllEnterprises.Sum(e => e.man) / totalStaff;

                            // 选择A行业中所有在该地区的企业
                            ConcurrentBag<Enterprise> AEnterprises = new ConcurrentBag<Enterprise>();
                            kv.Value.Enterprises.AsParallel().ForAll(e => 
                            {
                                IRelationalOperator relationalOperator = geo as IRelationalOperator;
                                if (relationalOperator.Contains(e.Point))
                                    AEnterprises.Add(e);
                            });
                            Sai = AEnterprises.Sum(e => e.man) / kv.Value.TotalStaff;

                            SumSaiSi += Math.Pow(Sai - Si, 2);
                            SumSi += Math.Pow(Si, 2);
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fcCounty);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError(ex.StackTrace);
                        //throw ex;
                        return null;
                    }
                    stopwatch.Stop();
                    kv.Value.PartEGa = SumSaiSi / (1 - SumSi);
                    string str = (egType == EGType.EGIndex) ? "文件的EG指标结果为：" : "文件的E指标Robust检查结果为：";

                    Log.WriteLog("文件" + kv.Value.Excel + "计算用时：" + stopwatch.ElapsedMilliseconds / 1000 +
                        ";\r\nSumSai计算结果为：" + SumSaiSi + ";\r\nSumSi计算结果为：" + SumSi + ";\r\n" +
                        kv.Key + str + kv.Value.GetEGaResult().ToString());

                    result.Add(kv.Key + str + kv.Value.GetEGaResult().ToString());
                }
                #endregion

                #region 注释
                //object lockflg = new object();
                //Parallel.ForEach(egEnterpisesDataDir, () => string.Empty, (kv, loop, temp) =>
                //{
                //    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //    stopwatch.Start();

                //    double Sai = 0.0, Si = 0.0;
                //    double SumSaiSi = 0.0, SumSi = 0.0;
                //    try
                //    {
                //        // 遍历中国所有的县
                //        IFeatureCursor fcCounty = Geodatabase.GeodatabaseOp.QuerySearch(fclsCounty, null, null, false);
                //        IFeature feature;
                //        while ((feature = fcCounty.NextFeature()) != null)
                //        {
                //            IGeometry geo = feature.Shape;

                //            // 选择所有行业里在这个地区内的企业
                //            List<Enterprise> AllEnterprises = enterprises.FindAll(e =>
                //            {                                
                //                IRelationalOperator relationalOperator = geo as IRelationalOperator;
                //                return relationalOperator.Contains(e.Point);
                //            });
                //            // 若全部企业在该地区没有数据，则A企业在该地区一定没有数据
                //            if (AllEnterprises != null && AllEnterprises.Count <= 0)
                //                continue;

                //            Si = AllEnterprises.Sum(e => e.man) / totalStaff;

                //            // 选择A行业中所有在该地区的企业
                //            List<Enterprise> AEnterprises = kv.Value.Enterprises.FindAll(e =>
                //            {                                
                //                IRelationalOperator relationalOperator = geo as IRelationalOperator;
                //                return relationalOperator.Contains(e.Point);
                //            });
                //            Sai = AEnterprises.Sum(e => e.man) / kv.Value.TotalStaff;

                //            SumSaiSi += Math.Pow(Sai - Si, 2);
                //            SumSi += Math.Pow(Si, 2);
                //        }
                //        System.Runtime.InteropServices.Marshal.ReleaseComObject(fcCounty);
                //    }
                //    catch (Exception ex)
                //    {
                //        Log.WriteError(ex.StackTrace);
                //        //throw ex;
                //        return null;
                //    }
                //    stopwatch.Stop();
                //    Log.WriteLog("文件" + kv.Value.Excel + "计算用时：" + stopwatch.ElapsedMilliseconds / 1000 +
                //        ";\r\nSumSai计算结果为：" + SumSaiSi + ";\r\nSumSi计算结果为：" + SumSi);

                //    kv.Value.PartEGa = SumSaiSi / (1 - SumSi);
                //    string str = (egType == EGType.EGIndex) ? "文件的EG指标结果为：" : "文件的E指标Robust检查结果为：";
                //    return kv.Key + str + kv.Value.GetEGaResult().ToString();
                //}, (x) =>
                //{
                //    lock (lockflg)
                //    {
                //        result.Add(x);
                //    }
                //});
                #endregion
            }
            catch (Exception ex)
            {
                Log.WriteError(ex.StackTrace);
                return null;
            }
            
            return result;
        }    
	}
}
